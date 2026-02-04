using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 只负责摄像头：启动 / 停止 / 高优先级帧处理
    /// </summary>
    public sealed class CameraService : IDisposable
    {
        private VideoCaptureDevice _device;
        private Thread _processingThread;
        private readonly AutoResetEvent _frameEvent = new AutoResetEvent(false);
        private Bitmap _pendingFrame;
        private readonly object _frameLock = new object();
        private bool _firstFrameLogged;

        public event EventHandler<Bitmap> FrameReceived;

        public bool IsRunning
        {
            get { return _device != null && _device.IsRunning; }
        }

        public FilterInfoCollection GetCameras()
        {
            return new FilterInfoCollection(FilterCategory.VideoInputDevice);
        }

        public void Start(string monikerString)
        {
            if (string.IsNullOrWhiteSpace(monikerString))
                throw new ArgumentException("Camera monikerString is empty.", "monikerString");

            Stop();
            
            // 重置首帧日志标志
            _firstFrameLogged = false;

            // 启动高优先级处理线程
            _processingThread = new Thread(ProcessingLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest,
                Name = "CameraProcessing"
            };
            _processingThread.Start();

            _device = new VideoCaptureDevice(monikerString);
            // 1. 选择最佳的视频能力
            var capabilities = _device.VideoCapabilities;
            // 优先选择帧率高的配置，而不是分辨率最高的
            VideoCapabilities bestCap = null;
           

            _device.NewFrame += OnNewFrame;
            _device.Start();
        }

        public void Stop()
        {
            if (_device == null)
                return;

            try
            {
                _device.NewFrame -= OnNewFrame;

                if (_device.IsRunning)
                {
                    _device.SignalToStop();
                    _device.WaitForStop();
                }
            }
            finally
            {
                _device = null;
            }

            // 停止处理线程
            _frameEvent.Set();
            _processingThread?.Join(500);
            _processingThread = null;
        }

        private void OnNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // AForge 回调线程：只负责把帧放入队列，立即返回
            long tick = DateTime.Now.Ticks;

            // 打印帧信息（仅第一帧）
            if (!_firstFrameLogged && eventArgs.Frame != null)
            {
                _firstFrameLogged = true;
                LogService.Info($"[Camera] 首帧信息: {eventArgs.Frame.Width}x{eventArgs.Frame.Height}, " +
                               $"PixelFormat: {eventArgs.Frame.PixelFormat}");
            }

            Bitmap frameToQueue = null;
            try
            {
                frameToQueue = (Bitmap)eventArgs.Frame.Clone();
            }
            catch { }

            if (frameToQueue != null)
            {
                lock (_frameLock)
                {
                    _pendingFrame?.Dispose();
                    _pendingFrame = frameToQueue;
                }
                _frameEvent.Set(); // 唤醒处理线程
                long ms = (DateTime.Now.Ticks - tick) / 10_000;
                if (ms > 20)
                    LogService.Info($"[Camera] Clone 耗时: {ms}ms");
            }
        }

        private void ProcessingLoop()
        {
            while (_device != null || _pendingFrame != null)
            {
                if (_frameEvent.WaitOne(100))
                {
                    long tick = DateTime.Now.Ticks;
                    Bitmap frame = null;
                    lock (_frameLock)
                    {
                        frame = _pendingFrame;
                        _pendingFrame = null;
                    }

                    if (frame != null)
                    {
                        long ms = tick / 10_000;
                        try
                        {
                            FrameReceived?.Invoke(this, frame);
                            // LogService.Info($"[Camera] 回调耗时: {(DateTime.Now.Ticks - tick)/10000}ms");
                        }
                        catch
                        {
                            frame.Dispose();
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Stop();
            _frameEvent.Dispose();
        }

        
    }


}
