using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinFormsBarcodeScannerApp.Services;

namespace WinFormsBarcodeScannerApp
{
    /// <summary>
    /// 系统设置窗口（模态），仅使用允许的控件：
    /// Form / TextBox / Button。
    /// </summary>
    public partial class SettingsForm : Form
    {
        private AppSettings _working;

        public AppSettings ResultSettings { get; private set; }

        // 设计器需要无参构造
        public SettingsForm() : this(null)
        {
        }

        public SettingsForm(AppSettings current)
        {
            InitializeComponent();

            // 设计器模式下不执行运行时逻辑
            if (IsInDesignMode())
                return;

            _working = Clone(current ?? AppSettings.CreateDefault());
            _txtBackendUrl.Text = _working.BackendUrl ?? string.Empty;
            _txtA4Directory.Text = _working.A4SaveDirectory ?? string.Empty;

            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
        }

        private static AppSettings Clone(AppSettings src)
        {
            if (src == null) return AppSettings.CreateDefault();
            return new AppSettings
            {
                BackendUrl = src.BackendUrl,
                A4SaveDirectory = src.A4SaveDirectory
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

        private void BtnBrowseOnClick(object sender, EventArgs e)
        {
            // 使用标准目录选择对话框，仅作为对话框，不作为控件添加到窗体
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择 A4 保存目录";
                if (Directory.Exists(_txtA4Directory.Text))
                    dlg.SelectedPath = _txtA4Directory.Text;

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _txtA4Directory.Text = dlg.SelectedPath;
                }
            }
        }

        private void BtnResetOnClick(object sender, EventArgs e)
        {
            _working = AppSettings.CreateDefault();
            _txtBackendUrl.Text = _working.BackendUrl ?? string.Empty;
            _txtA4Directory.Text = _working.A4SaveDirectory ?? string.Empty;
        }

        private void BtnSaveOnClick(object sender, EventArgs e)
        {
            _working.BackendUrl = (_txtBackendUrl.Text ?? string.Empty).Trim();
            _working.A4SaveDirectory = (_txtA4Directory.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(_working.BackendUrl))
            {
                MessageBox.Show(this, "后端服务地址不能为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_working.A4SaveDirectory))
            {
                MessageBox.Show(this, "A4 保存目录不能为空。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SettingsService.Save(_working);
            }
            catch
            {
                // 失败由主窗体日志捕获
            }

            ResultSettings = Clone(_working);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}


