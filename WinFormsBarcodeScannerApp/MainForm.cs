using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinFormsBarcodeScannerApp.Controllers;
using WinFormsBarcodeScannerApp.Services;
using WinFormsBarcodeScannerApp.Services.autocapture;
using WinFormsBarcodeScannerApp.Utils;

namespace WinFormsBarcodeScannerApp
{
    public partial class MainForm : Form
    {
        // ===== 控制器（关注点分离） =====
        private readonly CameraController _cameraController;
        private readonly PreviewManager _previewManager;
        private readonly HistoryManager _historyManager;
        private readonly LayoutManager _layoutManager;

        // ===== 采集相关 =====
        private readonly BarcodeService _barcodeService;
        private readonly FrameChangeDetector _frameChangeDetector;
        private AutoCaptureService _autoCaptureService;
        private CapturePipeline _capturePipeline;

        // ===== 设置 =====
        private AppSettings _settings;

        // 避免 InitializeComponent 期间触发 Resize/Load 导致空引用
        private bool _runtimeInitialized;

        public MainForm() : this(null)
        {
        }

        public MainForm(string asposeLicensePath)
        {
            InitializeComponent();

            // 启动时最大化窗口
            WindowState = FormWindowState.Maximized;

            if (IsInDesignMode())
                return;

            // 初始化预览管理器（必须在 InitializeComponent 之后，因为需要 _pictureBox）
            _previewManager = new PreviewManager(_pictureBox);

            // 初始化其他控制器和服务
            _layoutManager = new LayoutManager(this);
            _historyManager = new HistoryManager();
            _cameraController = new CameraController();
            _barcodeService = new BarcodeService(asposeLicensePath);
            _frameChangeDetector = new FrameChangeDetector();

            // 初始化控制器绑定和事件
            InitializeControllers();

            _runtimeInitialized = true;

            // 初始化 UI 状态
            _btnStop.Enabled = false;
            _btnRecognize.Text = "手动识别";
            SetSystemStatusReady();
            SetProgress(0, "就绪");
        }

        private void InitializeControllers()
        {
            // 1. 预览管理器（已在构造函数中初始化）
            _previewManager.FrameReceived += frame => { /* 可扩展：帧处理回调 */ };

            // 2. 历史管理器（已在构造函数中初始化）
            _historyManager.Bind(_historyListView, _historyImageList);
            _historyManager.InitializeListView();
            _historyManager.OnRecordSelected += recordId => { /* 可扩展 */ };
            // 删除历史记录时，同时清除去重记录
            _historyManager.OnRecordDeleted += barcode => _capturePipeline?.RemoveDuplicateBarcode(barcode);

            // 3. 布局管理器（已在构造函数中初始化）
            _layoutManager.Bind(
                _txtTitleBarBg, _btnSettings, _txtSystemStatus, _picCameraIcon, _txtDeviceName,
                _lblLiveHeader, _comboCameras, _btnStart, _btnStop, _btnRecognize,
                _pictureBox, _lblProgressHeader, _progressBar, _txtProgressStage, _txtResult,
                _lblHistoryHeader, _historyListView, _lblLogHeader, _txtUiLog);

            // 4. 摄像头控制器（已在构造函数中初始化）
            _cameraController.Bind(
                _comboCameras, _txtDeviceName, _txtSystemStatus, _picCameraIcon, _txtResult,
                OnCameraStarted, OnCameraStopped, OnCameraError);

            // 5. 绑定预览帧事件
            _cameraController.CameraService.FrameReceived += (s, frame) => _previewManager.OnFrameReceived(frame);

            // 6. 初始化采集服务（服务已在构造函数中初始化）
            InitializeAutoCapture();
        }

        private void InitializeAutoCapture()
        {
            string outputDir = _settings?.A4SaveDirectory;
            if (string.IsNullOrWhiteSpace(outputDir))
                outputDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "A4");

            _capturePipeline = new CapturePipeline(_barcodeService, outputDir);
            _autoCaptureService = new AutoCaptureService(_frameChangeDetector, _capturePipeline);

            // AutoCaptureService 直接订阅 CameraService 的帧事件
            _cameraController.CameraService.FrameReceived += (s, frame) => _autoCaptureService?.OnFrame(frame);

            _autoCaptureService.Callback = result =>
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => HandleCaptureResult(result)));
                else
                    HandleCaptureResult(result);
            };
        }

        private static bool IsInDesignMode()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                    return true;
            }
            catch { }
            return false;
        }

        // ===== 摄像头事件回调 =====
        private void OnCameraStarted(string deviceName)
        {
            _autoCaptureService?.Enable();
            _btnStart.Text = "停止预览";
            _btnStart.Click -= BtnStartOnClick;
            _btnStart.Click += BtnStopPreviewOnClick;
            _comboCameras.Enabled = false;
            _btnStop.Enabled = true;
            _txtResult.Text = "预览已开始，自动识别已启用";
            LogService.Info("Camera started: " + deviceName);
            AppendUiLog("预览开始：" + deviceName);
            AppendUiLog("自动识别已启用，画面稳定后将自动识别条码");
            SetProgress(0, "预览中");
        }

        private void ComboCamerasOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (_comboCameras.SelectedItem != null)
            {
                _txtDeviceName.Text = _comboCameras.SelectedItem.ToString();
            }
        }

        private void OnCameraStopped(string message)
        {
            _autoCaptureService?.Disable();
            _btnStart.Text = "预览";
            _btnStart.Click -= BtnStopPreviewOnClick;
            _btnStart.Click += BtnStartOnClick;
            _comboCameras.Enabled = true;
            _btnStop.Enabled = false;
            _txtResult.Text = message;
            LogService.Info("Camera stopped.");
            AppendUiLog(message);
            SetSystemStatusReady();
            SetProgress(0, "就绪");
            ClearPreviewAndBuffers();
        }

        private void OnCameraError(string error)
        {
            AppendUiLog("摄像头错误：" + error);
            SetSystemStatusError();
        }

        // ===== 采集结果处理 =====
        private void HandleCaptureResult(CapturePipeline.CaptureResult result)
        {
            if (result.IsSuccess)
            {
                _txtResult.Text = result.Barcode;
                AppendUiLog("自动识别成功：" + result.Barcode);
                 AppendUiLog("文件路径：" + result.FilePath);
                SetProgress(50, "保存中");

                // 直接传入文件路径
                if (System.IO.File.Exists(result.FilePath))
                {
                    var rec = _historyManager.AddRecord(result.Barcode, result.FilePath);
                    if (rec != null)
                        TryUploadReceiptAsync(rec);
                    SetProgress(100, "完成");
                }
                else
                {
                    AppendUiLog("警告：图片文件不存在");
                    SetProgress(100, "完成");
                }
            }
            else if (result.IsDuplicate)
            {
                AppendUiLog("自动识别：条码 " + result.Barcode + " 已在5分钟内识别过，跳过");
                SetProgress(0, "就绪");
            }
            else
            {
                AppendUiLog("自动识别失败：" + (result.ErrorMessage ?? "未知错误"));
                SetProgress(0, "就绪");
            }
        }

        // ===== 上传服务 =====
        private void TryUploadReceiptAsync(ScanRecord record)
        {
            if (record == null || string.IsNullOrWhiteSpace(record.imagePath))
                return;

            var sheetNo = record.OrderNo ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sheetNo))
            {
                AppendUiLog("上传跳过：单号为空。");
                return;
            }

            if (_settings == null || string.IsNullOrWhiteSpace(_settings.BackendUrl))
            {
                AppendUiLog("上传跳过：后台地址未配置。");
                return;
            }

            AppendUiLog("开始上传：单号 " + sheetNo);

            // 上传成功的回调
            void OnUploadSuccess(string fileUrl)
            {
                // 1. 更新历史记录中的 URL
                // _historyManager.UpdateRecordUrl(record.Id, fileUrl);
                // 2. 记录日志
                AppendUiLog("上传成功：单号 " + sheetNo);
                AppendUiLog("上传成功：url " + fileUrl);
            }

            // 上传失败的回调
            void OnUploadError(string error)
            {
                // 记录错误日志
                AppendUiLog("上传失败：单号 " + sheetNo + "，原因：" + (error ?? "未知错误。移除记录"));
                // 从历史记录中移除该记录
                _historyManager.RemoveRecord(record.Id);
            }

            // 确保在 UI 线程上执行回调
            Action<string> safeSuccessCallback = fileUrl =>
            {
                if (InvokeRequired)
                    BeginInvoke((Action)(() => OnUploadSuccess(fileUrl)));
                else
                    OnUploadSuccess(fileUrl);
            };

            Action<string> safeErrorCallback = error =>
            {
                if (InvokeRequired)
                    BeginInvoke((Action)(() => OnUploadError(error)));
                else
                    OnUploadError(error);
            };

            UploadService.StartUploadAsync(
                _settings.BackendUrl,
                sheetNo,
                record.imagePath,
                safeSuccessCallback,
                safeErrorCallback,
                PlaySuccessSound);
        }

        // ===== UI 事件处理 =====
        private void OnLoad(object sender, EventArgs e)
        {
            if (IsInDesignMode())
                return;
            if (!_runtimeInitialized)
                return;

            // 启动时清空日志
            try
            {
                string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");
                if (File.Exists(logFile))
                    File.WriteAllText(logFile, string.Empty);
            }
            catch { }

            _settings = SettingsService.Load();

            _cameraController.RefreshCameras();
            _layoutManager.ApplyLayout();

        }

        private void OnResize(object sender, EventArgs e)
        {
            if (IsInDesignMode())
                return;
            if (!_runtimeInitialized || _layoutManager == null)
                return;
            _layoutManager.ApplyLayout();
        }

        private void BtnStartOnClick(object sender, EventArgs e)
        {
            _cameraController.RefreshCameras();
            _cameraController.StartPreview();
        }

        private void BtnStopPreviewOnClick(object sender, EventArgs e)
        {
            _cameraController.StopPreview();
        }

        private void BtnStopOnClick(object sender, EventArgs e)
        {
            try
            {
                _historyManager.Clear();
                _txtResult.Text = string.Empty;
                _txtUiLog.Text = string.Empty;

                // 重置自动采集服务
                _autoCaptureService?.Disable();
                InitializeAutoCapture();

                AppendUiLog("重置完成：已清空历史记录");
                LogService.Info("Reset completed.");
                SetSystemStatusReady();
                SetProgress(0, "就绪");
            }
            catch (Exception ex)
            {
                _txtResult.Text = "重置失败: " + ex.Message;
                LogService.Error("Reset failed.", ex);
                AppendUiLog("重置失败：" + ex.Message);
                SetSystemStatusError();
            }
        }

        private void BtnRecognizeOnClick(object sender, EventArgs e)
        {
            if (_autoCaptureService == null || !_autoCaptureService.IsEnabled)
            {
                _txtResult.Text = "请先启动预览";
                AppendUiLog("手动识别：请先启动预览");
                return;
            }

            Bitmap snapshot = _previewManager.GetCurrentFrame();
            if (snapshot == null)
            {
                _txtResult.Text = "当前无画面帧";
                AppendUiLog("手动识别失败：当前无画面帧");
                return;
            }

            try
            {
                SetProgress(10, "识别中...");

                Bitmap imageForWork = snapshot;

                // 使用高拍仪专用 A4 检测与透视矫正（EmguCV）；失败则回退原图
                try
                {
                    Mat warpedMat = A4PaperDetectorHighCamera.ProcessDocument(snapshot);
                    if (warpedMat != null && !warpedMat.IsEmpty)
                    {
                        Image<Rgba, byte> warpedImage = warpedMat.ToImage<Rgba, byte>();
                        imageForWork = warpedImage.ToBitmap();
                        warpedImage.Dispose();
                    }

                    
                }
                catch (Exception ex)
                {
                    LogService.Error("手动识别 A4 检测与矫正异常，使用原图继续识别", ex);
                }

                string barcodeText = _barcodeService.Recognize(imageForWork);

                if (string.IsNullOrWhiteSpace(barcodeText))
                {
                    _txtResult.Text = "未检测到条码";
                    AppendUiLog("手动识别：未检测到条码");
                    SetProgress(0, "就绪");
                    return;
                }

                _txtResult.Text = barcodeText;
                AppendUiLog("手动识别成功：" + barcodeText);
                SetProgress(50, "保存中...");

                // 保存图片到磁盘
                string filePath = null;
                try
                {
                    string outputDir = _settings?.A4SaveDirectory;
                    if (string.IsNullOrWhiteSpace(outputDir))
                        outputDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "A4");
                    if (!System.IO.Directory.Exists(outputDir))
                        System.IO.Directory.CreateDirectory(outputDir);

                    string safeBarcode = PathHelper.MakeSafeFileName(barcodeText);
                    string fileName = safeBarcode + ".png";
                    filePath = System.IO.Path.Combine(outputDir, fileName);

                    imageForWork.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    LogService.Info($"手动识别保存图片: {filePath}");
                }
                catch (Exception ex)
                {
                    LogService.Error("保存图片失败", ex);
                }

                // 添加到历史记录
                if (filePath != null && System.IO.File.Exists(filePath))
                {
                    var rec = _historyManager.AddRecord(barcodeText, filePath);
                    if (rec != null)
                        TryUploadReceiptAsync(rec);
                }

                SetProgress(100, "完成");
            }
            catch (Exception ex)
            {
                _txtResult.Text = "手动识别失败: " + ex.Message;
                AppendUiLog("手动识别异常：" + ex.Message);
                SetSystemStatusError();
                SetProgress(0, "错误");
                LogService.Error("Manual recognition failed.", ex);
            }
            finally
            {
                snapshot?.Dispose();
            }
        }

        private void ClearPreviewAndBuffers()
        {
            _previewManager.Clear();
        }

        private void SetProgress(int percent, string stage)
        {
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;
            _progressBar.Value = percent;
            _txtProgressStage.Text = percent + "%  " + (stage ?? string.Empty);
        }

        private void SetSystemStatusReady()
        {
            _txtSystemStatus.Text = "就绪";
            _txtSystemStatus.ForeColor = Color.White;
        }

        private void SetSystemStatusWorking()
        {
            _txtSystemStatus.Text = "工作中";
            _txtSystemStatus.ForeColor = Color.White;
        }

        private void SetSystemStatusError()
        {
            _txtSystemStatus.Text = "错误";
            _txtSystemStatus.ForeColor = Color.OrangeRed;
        }

        private void AppendUiLog(string message)
        {
            string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
            if (_txtUiLog.TextLength == 0)
                _txtUiLog.Text = line;
            else
                _txtUiLog.AppendText(Environment.NewLine + line);
        }

        private void BtnSettingsOnClick(object sender, EventArgs e)
        {
            using (var dlg = new SettingsForm(_settings))
            {
                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK && dlg.ResultSettings != null)
                {
                    _settings = dlg.ResultSettings;
                    AppendUiLog("系统设置已保存。");
                    LogService.Info("Settings saved. BackendUrl=" + _settings.BackendUrl + ", A4SaveDirectory=" + _settings.A4SaveDirectory);
                }
                else
                {
                    AppendUiLog("系统设置已取消。");
                }
            }
        }

        private void _txtProgressStage_TextChanged(object sender, EventArgs e)
        {
            // 空事件处理器：设计器要求
        }

        private void HistoryListViewOnItemActivate(object sender, EventArgs e)
        {
            if (_historyListView.SelectedItems.Count <= 0)
                return;

            var item = _historyListView.SelectedItems[0];
            var record = item.Tag as ScanRecord;
            if (record != null)
            {
                AppendUiLog("打开详情：#" + record.Id);
                _historyManager.OpenDetail(record);
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _autoCaptureService?.Disable();
                _autoCaptureService?.Shutdown();
            }
            catch { }

            try
            {
                _cameraController.CameraService.FrameReceived -= (s, frame) => _autoCaptureService?.OnFrame(frame);
                _cameraController.StopPreview();
            }
            catch { }

            // 清理资源
            _previewManager?.Dispose();
            _historyManager?.Dispose();
            _cameraController?.Dispose();
        }

        /// <summary>
        /// 播放成功提示音（使用 Windows Media Player）
        /// </summary>
        private void PlaySuccessSound()
        {
            try
            {
                string soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "asserts", "success.mp3");
                if (!File.Exists(soundPath))
                {
                    LogService.Warn("[MainForm] 提示音文件不存在: " + soundPath);
                    return;
                }

                // NAudio 在后台线程播放，不受对象释放影响
                var player = new AudioPlayer();
                player.Play(soundPath);
            }
            catch (Exception ex)
        {
                LogService.Error("[MainForm] 播放提示音失败", ex);
            }
        }
    }
}
