using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsBarcodeScannerApp.Controllers
{
    /// <summary>
    /// 布局管理器：负责窗体的响应式布局
    /// 关注点：自适应缩放、屏幕无关性
    /// </summary>
    public sealed class LayoutManager
    {
        private readonly Form _form;
        private Size _baseClientSize;
        private Dictionary<Control, Rectangle> _baseBounds;
        private bool _layoutScalingInitialized;

        // 布局常量
        private const int TitleBarHeight = 56;
        private const int Gap = 20;
        private const int RightPad = 12;
        private const int Padding = 12;
        private const int ControlRowHeight = 34;
        private const int HeaderHeight = 22;
        private const int RightPanelWidth = 380;
        private const int ControlGap = 8;
        private const int ProgressAreaHeight = 46;
        private const int ResultAreaHeight = 90;

        // 控件引用
        private TextBox _txtTitleBarBg;
        private Button _btnSettings;
        private TextBox _txtSystemStatus;
        private PictureBox _picCameraIcon;
        private TextBox _txtDeviceName;
        private Label _lblLiveHeader;
        private ComboBox _comboCameras;
        private Button _btnStart;
        private Button _btnStop;
        private Button _btnRecognize;
        private PictureBox _pictureBox;
        private Label _lblProgressHeader;
        private ProgressBar _progressBar;
        private TextBox _txtProgressStage;
        private TextBox _txtResult;
        private Label _lblHistoryHeader;
        private ListView _historyListView;
        private Label _lblLogHeader;
        private TextBox _txtUiLog;

        public LayoutManager(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        /// <summary>
        /// 绑定所有需要布局的控件
        /// </summary>
        public void Bind(
            TextBox txtTitleBarBg,
            Button btnSettings,
            TextBox txtSystemStatus,
            PictureBox picCameraIcon,
            TextBox txtDeviceName,
            Label lblLiveHeader,
            ComboBox comboCameras,
            Button btnStart,
            Button btnStop,
            Button btnRecognize,
            PictureBox pictureBox,
            Label lblProgressHeader,
            ProgressBar progressBar,
            TextBox txtProgressStage,
            TextBox txtResult,
            Label lblHistoryHeader,
            ListView historyListView,
            Label lblLogHeader,
            TextBox txtUiLog)
        {
            _txtTitleBarBg = txtTitleBarBg;
            _btnSettings = btnSettings;
            _txtSystemStatus = txtSystemStatus;
            _picCameraIcon = picCameraIcon;
            _txtDeviceName = txtDeviceName;
            _lblLiveHeader = lblLiveHeader;
            _comboCameras = comboCameras;
            _btnStart = btnStart;
            _btnStop = btnStop;
            _btnRecognize = btnRecognize;
            _pictureBox = pictureBox;
            _lblProgressHeader = lblProgressHeader;
            _progressBar = progressBar;
            _txtProgressStage = txtProgressStage;
            _txtResult = txtResult;
            _lblHistoryHeader = lblHistoryHeader;
            _historyListView = historyListView;
            _lblLogHeader = lblLogHeader;
            _txtUiLog = txtUiLog;
        }

        /// <summary>
        /// 初始化基准尺寸（只调用一次）
        /// </summary>
        public void Initialize()
        {
            if (_layoutScalingInitialized)
                return;

            _baseClientSize = _form.ClientSize;
            _baseBounds = new Dictionary<Control, Rectangle>();

            // 排除设置按钮（使用 Anchor 自动定位）
            BindControl(_txtTitleBarBg);
            BindControl(_txtSystemStatus);
            BindControl(_picCameraIcon);
            BindControl(_txtDeviceName);
            BindControl(_lblLiveHeader);
            BindControl(_comboCameras);
            BindControl(_btnStart);
            BindControl(_btnStop);
            BindControl(_btnRecognize);
            BindControl(_pictureBox);
            BindControl(_lblProgressHeader);
            BindControl(_progressBar);
            BindControl(_txtProgressStage);
            BindControl(_txtResult);
            BindControl(_lblHistoryHeader);
            BindControl(_historyListView);
            BindControl(_lblLogHeader);
            BindControl(_txtUiLog);

            _layoutScalingLayout();
        }

        /// <summary>
        /// 执行布局计算（窗口大小变化时调用）
        /// </summary>
        public void ApplyLayout()
        {
            if (!_layoutScalingInitialized)
                Initialize();

            ApplyScalingLayout();
        }

        private void BindControl(Control ctl)
        {
            if (ctl != null)
                _baseBounds[ctl] = ctl.Bounds;
        }

        private void ApplyScalingLayout()
        {
            if (_form.IsDisposed)
                return;

            // ========== 标题栏区域（手动布局） ==========
            _txtTitleBarBg.Left = 0;
            _txtTitleBarBg.Top = 0;
            _txtTitleBarBg.Width = _form.ClientSize.Width;
            _txtTitleBarBg.Height = TitleBarHeight;

            // 右侧控件：从右到左排列
            int x = _form.ClientSize.Width - RightPad;

            // 设置按钮
            x -= _btnSettings.Width;
            _btnSettings.Left = Math.Max(0, x);
            _btnSettings.Top = 8;
            x -= Gap;

            // 系统状态
            x -= _txtSystemStatus.Width;
            _txtSystemStatus.Left = Math.Max(0, x);
            _txtSystemStatus.Top = 19;
            x -= Gap;

            // 相机图标
            x -= _picCameraIcon.Width;
            _picCameraIcon.Left = Math.Max(0, x);
            _picCameraIcon.Top = 19;
            x -= Gap;

            // 设备名称
            x -= _txtDeviceName.Width;
            _txtDeviceName.Left = Math.Max(0, x);
            _txtDeviceName.Top = 19;

            // ========== 主内容区域（自适应布局） ==========
            int contentTop = TitleBarHeight + Padding;
            int contentLeft = Padding;
            int contentRight = _form.ClientSize.Width - Padding - RightPanelWidth - Padding;
            int contentWidth = contentRight - contentLeft;
            int contentBottom = _form.ClientSize.Height - Padding;

            int leftY = contentTop;

            // 1. 实时影像标题
            _lblLiveHeader.Left = contentLeft;
            _lblLiveHeader.Top = leftY;
            _lblLiveHeader.Width = 90;
            _lblLiveHeader.Height = ControlRowHeight;

            // 2. 控制栏
            int controlY = leftY;
            int controlX = contentLeft + _lblLiveHeader.Width + ControlGap;

            _comboCameras.Left = controlX;
            _comboCameras.Top = controlY + 4;
            _comboCameras.Width = 250;
            _comboCameras.Height = 26;

            controlX += _comboCameras.Width + ControlGap;
            _btnStart.Left = controlX;
            _btnStart.Top = controlY;
            _btnStart.Width = 90;
            _btnStart.Height = ControlRowHeight;

            controlX += _btnStart.Width + ControlGap;
            _btnStop.Left = controlX;
            _btnStop.Top = controlY;
            _btnStop.Width = 90;
            _btnStop.Height = ControlRowHeight;

            controlX += _btnStop.Width + ControlGap;
            _btnRecognize.Left = controlX;
            _btnRecognize.Top = controlY;
            _btnRecognize.Width = 90;
            _btnRecognize.Height = ControlRowHeight;

            leftY += ControlRowHeight + ControlGap;

            // 3. 图片预览区域
            int pictureTop = leftY;
            int pictureBottom = contentBottom - ResultAreaHeight - ControlGap - ProgressAreaHeight - ControlGap;
            _pictureBox.Left = contentLeft;
            _pictureBox.Top = pictureTop;
            _pictureBox.Width = contentWidth;
            _pictureBox.Height = Math.Max(100, pictureBottom - pictureTop);

            leftY = pictureBottom + ControlGap;

            // 4. 进度区域
            _lblProgressHeader.Left = contentLeft;
            _lblProgressHeader.Top = leftY;
            _lblProgressHeader.Width = 60;
            _lblProgressHeader.Height = ProgressAreaHeight;

            int progressX = contentLeft + _lblProgressHeader.Width + ControlGap;
            int progressWidth = contentWidth - _lblProgressHeader.Width - ControlGap;

            _progressBar.Left = progressX;
            _progressBar.Top = leftY + 4;
            _progressBar.Width = progressWidth;
            _progressBar.Height = 16;

            _txtProgressStage.Left = progressX;
            _txtProgressStage.Top = leftY + 24;
            _txtProgressStage.Width = progressWidth;
            _txtProgressStage.Height = 20;

            leftY += ProgressAreaHeight + ControlGap;

            // 5. 结果区域
            _txtResult.Left = contentLeft;
            _txtResult.Top = leftY;
            _txtResult.Width = contentWidth;
            _txtResult.Height = ResultAreaHeight;

            // ========== 右侧区域布局 ==========
            int rightX = _form.ClientSize.Width - Padding - RightPanelWidth;
            int rightY = contentTop;

            // 6. 扫描历史标题
            _lblHistoryHeader.Left = rightX;
            _lblHistoryHeader.Top = rightY;
            _lblHistoryHeader.Width = RightPanelWidth;
            _lblHistoryHeader.Height = HeaderHeight;

            rightY += HeaderHeight + ControlGap;

            // 7. 扫描历史列表
            int historyHeight = (contentBottom - rightY - HeaderHeight - ControlGap - ControlGap) / 2;

            if (_historyListView != null)
            {
                _historyListView.Left = rightX;
                _historyListView.Top = rightY;
                _historyListView.Width = RightPanelWidth;
                _historyListView.Height = Math.Max(100, historyHeight);
            }

            rightY += historyHeight + ControlGap;

            // 8. 操作日志标题
            _lblLogHeader.Left = rightX;
            _lblLogHeader.Top = rightY;
            _lblLogHeader.Width = RightPanelWidth;
            _lblLogHeader.Height = HeaderHeight;

            rightY += HeaderHeight + ControlGap;

            // 9. 操作日志内容
            _txtUiLog.Left = rightX;
            _txtUiLog.Top = rightY;
            _txtUiLog.Width = RightPanelWidth;
            _txtUiLog.Height = Math.Max(100, contentBottom - rightY);
        }

        // 保持兼容性，修复方法名拼写错误
        private void _layoutScalingLayout()
        {
            _layoutScalingInitialized = true;
        }
    }
}

