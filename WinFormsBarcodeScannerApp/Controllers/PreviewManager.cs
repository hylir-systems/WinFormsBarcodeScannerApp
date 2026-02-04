using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsBarcodeScannerApp.Controllers
{
    /// <summary>
    /// 预览画面管理器：负责实时帧的接收与显示
    /// 关注点：UI 更新、低延迟画面展示
    /// </summary>
    public sealed class PreviewManager : IDisposable
    {
        private readonly PictureBox _pictureBox;
        private Bitmap _pendingFrame;
        private readonly object _pendingLock = new object();
        private bool _disposed;

        public event Action<Image> FrameReceived;

        public PreviewManager(PictureBox pictureBox)
        {
            _pictureBox = pictureBox ?? throw new ArgumentNullException(nameof(pictureBox));
        }

        /// <summary>
        /// 处理摄像头帧（CameraService 线程中调用）
        /// </summary>
        public void OnFrameReceived(Bitmap frame)
        {
            if (_pictureBox.IsDisposed || frame == null)
            {
                frame?.Dispose();
                return;
            }

            // 单槽缓冲：替换旧帧
            Bitmap toDispose = null;
            lock (_pendingLock)
            {
                toDispose = _pendingFrame;
                _pendingFrame = frame;
            }
            toDispose?.Dispose();

            // 回到 UI 线程更新显示
            if (_pictureBox.InvokeRequired)
                _pictureBox.BeginInvoke(new Action(UpdatePreview));
            else
                UpdatePreview();
        }

        /// <summary>
        /// 清空预览画面
        /// </summary>
        public void Clear()
        {
            lock (_pendingLock)
            {
                _pendingFrame?.Dispose();
                _pendingFrame = null;
            }

            if (_pictureBox.InvokeRequired)
            {
                _pictureBox.BeginInvoke(new Action(() =>
                {
                    _pictureBox.Image?.Dispose();
                    _pictureBox.Image = null;
                }));
            }
            else
            {
                _pictureBox.Image?.Dispose();
                _pictureBox.Image = null;
            }
        }

        /// <summary>
        /// 获取当前预览帧（用于手动识别）
        /// </summary>
        public Bitmap GetCurrentFrame()
        {
            lock (_pendingLock)
            {
                return _pendingFrame?.Clone() as Bitmap;
            }
        }

        private void UpdatePreview()
        {
            if (_pictureBox.IsDisposed)
                return;

            Bitmap frame = null;
            lock (_pendingLock)
            {
                frame = _pendingFrame;
                _pendingFrame = null;
            }

            if (frame == null)
                return;

            try
            {
                var old = _pictureBox.Image;
                _pictureBox.Image = frame;
                old?.Dispose();
            }
            catch
            {
                frame.Dispose();
            }

            FrameReceived?.Invoke(_pictureBox.Image);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            Clear();
        }
    }
}

