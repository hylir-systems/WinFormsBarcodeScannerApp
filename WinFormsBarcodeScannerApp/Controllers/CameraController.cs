using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AForge.Video.DirectShow;
using WinFormsBarcodeScannerApp.Services;
using WinFormsBarcodeScannerApp.Services.autocapture;

namespace WinFormsBarcodeScannerApp.Controllers
{
    /// <summary>
    /// 摄像头控制器：负责摄像头的枚举、启动、停止
    /// 关注点：摄像头硬件交互
    /// </summary>
    public sealed class CameraController : IDisposable
    {
        private readonly CameraService _cameraService;
        private FilterInfoCollection _cameras;
        private ComboBox _comboCameras;
        private TextBox _txtDeviceName;
        private TextBox _txtSystemStatus;
        private PictureBox _picCameraIcon;
        private TextBox _txtResult;
        private Action<string> _onCameraStarted;
        private Action<string> _onCameraStopped;
        private Action<string> _onCameraError;

        public CameraController()
        {
            _cameraService = new CameraService();
        }

        /// <summary>
        /// 绑定 UI 控件
        /// </summary>
        public void Bind(
            ComboBox comboCameras,
            TextBox txtDeviceName,
            TextBox txtSystemStatus,
            PictureBox picCameraIcon,
            TextBox txtResult,
            Action<string> onCameraStarted,
            Action<string> onCameraStopped,
            Action<string> onCameraError)
        {
            _comboCameras = comboCameras;
            _txtDeviceName = txtDeviceName;
            _txtSystemStatus = txtSystemStatus;
            _picCameraIcon = picCameraIcon;
            _txtResult = txtResult;
            _onCameraStarted = onCameraStarted;
            _onCameraStopped = onCameraStopped;
            _onCameraError = onCameraError;

            // 订阅摄像头帧事件
            _cameraService.FrameReceived += (s, frame) => { /* 帧事件由 PreviewManager 处理 */ };
        }

        /// <summary>
        /// 枚举可用摄像头
        /// </summary>
        public void RefreshCameras()
        {
            _comboCameras.Items.Clear();
            _cameras = null;

            try
            {
                _cameras = _cameraService.GetCameras();
                if (_cameras.Count == 0)
                {
                    _txtResult.Text = "No camera found.";
                    LogService.Info("No camera found.");
                    _txtDeviceName.Text = "无摄像头";
                    SetSystemStatusError();
                    return;
                }

                for (int i = 0; i < _cameras.Count; i++)
                    _comboCameras.Items.Add(_cameras[i].Name);

                _comboCameras.SelectedIndex = 0;
                _txtResult.Text = "Select camera and click Start.";
                LogService.Info("Cameras enumerated: " + _cameras.Count);
                UpdateDeviceNameFromSelection();
                SetSystemStatusReady();
            }
            catch (Exception ex)
            {
                _txtResult.Text = "Enumerate cameras failed: " + ex.Message;
                LogService.Error("Enumerate cameras failed.", ex);
                _txtDeviceName.Text = "枚举失败";
                SetSystemStatusError();
            }
        }

        /// <summary>
        /// 启动摄像头预览
        /// </summary>
        public bool StartPreview()
        {
            if (_cameras == null || _cameras.Count == 0)
            {
                RefreshCameras();
                return false;
            }

            if (_comboCameras.SelectedIndex < 0 || _comboCameras.SelectedIndex >= _cameras.Count)
            {
                _txtResult.Text = "请选择摄像头";
                return false;
            }

            try
            {
                var moniker = _cameras[_comboCameras.SelectedIndex].MonikerString;
                _cameraService.Start(moniker);
                _onCameraStarted?.Invoke(_cameras[_comboCameras.SelectedIndex].Name);
                UpdateDeviceNameFromSelection();
                SetSystemStatusWorking();
                return true;
            }
            catch (Exception ex)
            {
                _txtResult.Text = "启动预览失败: " + ex.Message;
                LogService.Error("Start camera failed.", ex);
                _onCameraError?.Invoke(ex.Message);
                SetSystemStatusError();
                return false;
            }
        }

        /// <summary>
        /// 停止摄像头预览
        /// </summary>
        public void StopPreview()
        {
            try
            {
                _cameraService.Stop();
                _onCameraStopped?.Invoke("预览已停止");
                SetSystemStatusReady();
            }
            catch (Exception ex)
            {
                _txtResult.Text = "停止预览失败: " + ex.Message;
                LogService.Error("Stop camera failed.", ex);
                _onCameraError?.Invoke(ex.Message);
                SetSystemStatusError();
            }
        }

        /// <summary>
        /// 获取当前选中的摄像头名称
        /// </summary>
        public string CurrentDeviceName => _cameras?[_comboCameras.SelectedIndex]?.Name ?? "未知设备";

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _cameraService.IsRunning;

        /// <summary>
        /// 摄像头服务实例（供外部订阅帧事件）
        /// </summary>
        public CameraService CameraService => _cameraService;

        private void UpdateDeviceNameFromSelection()
        {
            try
            {
                if (_cameras == null || _cameras.Count == 0 || _comboCameras.SelectedIndex < 0)
                {
                    _txtDeviceName.Text = "未选择设备";
                    return;
                }
                _txtDeviceName.Text = _cameras[_comboCameras.SelectedIndex].Name;
            }
            catch
            {
                _txtDeviceName.Text = "未知设备";
            }
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

        public void Dispose()
        {
            _cameraService?.Stop();
            _cameraService?.Dispose();
        }
    }
}

