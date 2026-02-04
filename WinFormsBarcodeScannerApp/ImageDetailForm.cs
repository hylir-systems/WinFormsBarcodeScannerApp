using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsBarcodeScannerApp
{
    /// <summary>
    /// 图片详情窗口（独立窗口/对话框）
    /// 仅使用：Form / PictureBox / Button / TextBox
    /// </summary>
    public partial class ImageDetailForm : Form
    {
        private Bitmap _image;
        private float _zoom = 1.0f;
        private bool _dragging;
        private Point _dragStartMouse;
        private Point _dragStartLocation;

        // 设计器需要无参构造
        public ImageDetailForm() : this(null, DateTime.Now, null, new Bitmap(10, 10))
        {
        }

        public ImageDetailForm(string orderNo, DateTime time, string url, Bitmap image)
        {
            InitializeComponent();

            if (IsInDesignMode())
            {
                _image = new Bitmap(10, 10);
                _picture.Image = _image;
                return;
            }

            if (image == null) throw new ArgumentNullException("image");

            try { Icon = SystemIcons.Application; } catch { }

            // Important: do NOT dispose record-owned bitmap; clone for this dialog.
            _image = (Bitmap)image.Clone();
            _picture.Image = _image;

            _txtOrderNo.Text = "单号： " + (orderNo ?? string.Empty);
            _txtTime.Text = "时间： " + time.ToString("yyyy-MM-dd HH:mm:ss");
            _txtUrl.Text = "URL： " + (url ?? string.Empty);

            _picture.MouseDown += PictureOnMouseDown;
            _picture.MouseMove += PictureOnMouseMove;
            _picture.MouseUp += PictureOnMouseUp;
            _picture.MouseWheel += PictureOnMouseWheel;
            _picture.DoubleClick += PictureOnDoubleClick;
            _btnClose.Click += (s, e) => Close();

            Load += (s, e) =>
            {
                FitToViewport();
            };
            FormClosing += (s, e) =>
            {
                try { _picture.Image = null; } catch { }
                try { if (_image != null) _image.Dispose(); } catch { }
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

        private Rectangle GetViewport()
        {
            // 使用 Designer 设置的控件位置推导出“可视区域”：
            // 介于 URL 文本框和 关闭按钮 之间的中间区域。
            int pad = 12;
            int top = _txtUrl.Bottom + 10;
            int bottom = _btnClose.Top - 10;
            int h = Math.Max(0, bottom - top);
            int w = Math.Max(0, ClientSize.Width - pad * 2);
            return new Rectangle(pad, top, w, h);
        }

        private void FitToViewport()
        {
            var viewport = GetViewport();
            if (viewport.Width <= 0 || viewport.Height <= 0) return;

            float zx = (float)viewport.Width / _image.Width;
            float zy = (float)viewport.Height / _image.Height;
            _zoom = Math.Min(zx, zy);
            if (_zoom <= 0.01f) _zoom = 0.01f;

            ApplyZoomAndCenter();
        }

        private void ApplyZoomAndCenter()
        {
            var viewport = GetViewport();
            int newW = Math.Max(1, (int)(_image.Width * _zoom));
            int newH = Math.Max(1, (int)(_image.Height * _zoom));

            _picture.Width = newW;
            _picture.Height = newH;

            // Center within viewport
            int left = viewport.Left + (viewport.Width - newW) / 2;
            int top = viewport.Top + (viewport.Height - newH) / 2;
            _picture.Left = left;
            _picture.Top = top;
        }

        private void PictureOnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            _dragStartMouse = Cursor.Position;
            _dragStartLocation = _picture.Location;
            _picture.Focus();
        }

        private void PictureOnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            var delta = new Point(Cursor.Position.X - _dragStartMouse.X, Cursor.Position.Y - _dragStartMouse.Y);
            _picture.Left = _dragStartLocation.X + delta.X;
            _picture.Top = _dragStartLocation.Y + delta.Y;
        }

        private void PictureOnMouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void PictureOnMouseWheel(object sender, MouseEventArgs e)
        {
            // Wheel zoom: stable and simple (no extra controls)
            float step = 0.1f;
            if (e.Delta > 0) _zoom += step;
            else _zoom -= step;

            if (_zoom < 0.05f) _zoom = 0.05f;
            if (_zoom > 8.0f) _zoom = 8.0f;

            // Keep zoom centered around viewport center
            var viewport = GetViewport();
            var center = new Point(viewport.Left + viewport.Width / 2, viewport.Top + viewport.Height / 2);
            var before = new Point(center.X - _picture.Left, center.Y - _picture.Top);

            int newW = Math.Max(1, (int)(_image.Width * _zoom));
            int newH = Math.Max(1, (int)(_image.Height * _zoom));
            _picture.Width = newW;
            _picture.Height = newH;

            // reposition so that the same relative point stays near center
            float sx = before.X / (float)Math.Max(1, _picture.Width);
            float sy = before.Y / (float)Math.Max(1, _picture.Height);
            _picture.Left = center.X - (int)(_picture.Width * sx);
            _picture.Top = center.Y - (int)(_picture.Height * sy);
        }

        private void PictureOnDoubleClick(object sender, EventArgs e)
        {
            // Double click: fit-to-window toggle
            FitToViewport();
        }
    }
}


