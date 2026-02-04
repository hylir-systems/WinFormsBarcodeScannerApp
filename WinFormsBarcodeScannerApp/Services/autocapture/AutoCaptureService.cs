using System;
using System.Drawing;
using System.Threading;

namespace WinFormsBarcodeScannerApp.Services.autocapture
{
    /// <summary>
    /// 工业级自动采集状态机（高拍仪 / 扫描仪专用）
    /// </summary>
    public sealed class AutoCaptureService
    {
        // ===== 可调参数 =====
        private const int StableFrameThreshold = 2;     // Ready 状态下只需 2 帧稳定即可触发识别
        private const int EnterReadyThreshold = 1;      // 换页后只需 1 帧稳定即可进入 Ready
        // 用户换纸需要时间（拿走旧纸、放上新纸）
        // 避免同一张纸被连续识别多次
        // 防止"狂点"识别
        private const long CooldownMs = 1200;            // 两次采集冷却时间

        // 换纸场景：画面一直在变化时（比如用户正在放纸），最多等待这个时间后直接更新参考帧
        private const int ChangingTimeoutMs = 3000;      // 3秒画面持续变化则视为换纸，直接更新参考帧

        // 首次初始化后直接进入 Ready（跳过等待）
        private bool _firstFrameInitialized = false;

        private enum State
        {
            Disabled,
            Unstable,
            Ready,
            Processing,
            Processed
        }

        private readonly FrameChangeDetector _detector;
        private readonly CapturePipeline _pipeline;

        private volatile State _state = State.Disabled;
        private int _stableCount = 0;
        private long _lastCaptureTime = 0;

        private volatile bool _enabled = false;
        private volatile bool _shutdown = false;

        // 换纸场景：记录开始检测到变化的时刻
        private long _changingStartTime = 0;

        private Bitmap _currentFrame;
        private readonly object _frameLock = new object();

        public Action<CapturePipeline.CaptureResult> Callback { get; set; }

        private readonly Thread _workerThread;
        private readonly AutoResetEvent _frameEvent = new AutoResetEvent(false);

        public AutoCaptureService(FrameChangeDetector detector, CapturePipeline pipeline)
        {
            _detector = detector ?? throw new ArgumentNullException(nameof(detector));
            _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));

            _workerThread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "AutoCaptureWorker"
            };
            _workerThread.Start();
        }

        // ================= 生命周期 =================

        public void Enable()
        {
            _enabled = true;
            ResetInternal();
            _state = State.Unstable;
            _detector.Reset();
            _firstFrameInitialized = false;

            LogService.Info("[AutoCapture] Enabled");
        }

        public void Disable()
        {
            _enabled = false;
            lock (_frameLock)
            {
                // ⭐ 不直接 Dispose，而是用 null 覆盖
                // 避免工作线程正在 Clone 时被 Dispose 导致异常
                _currentFrame = null;
            }
            ResetInternal();
            LogService.Info("[AutoCapture] Disabled");
        }

        public bool IsEnabled => _enabled;

        public void Shutdown()
        {
            _shutdown = true;
            _frameEvent.Set();
            _workerThread.Join(1000);
        }

        // ================= 帧输入 =================

        public void OnFrame(Bitmap frame)
        {
            if (!_enabled || frame == null)
                return;

            lock (_frameLock)
            {
                // ⭐ 直接用新帧覆盖，不要 Dispose 旧帧
                // 避免工作线程正在 Clone 时被 Dispose
                // GC 会自动回收变成垃圾的 Bitmap
                _currentFrame = (Bitmap)frame.Clone();
            }

            _frameEvent.Set();
        }

        // ================= 工作线程 =================

        private void WorkerLoop()
        {
            while (!_shutdown)
            {
                if (_frameEvent.WaitOne(16)) // 16ms = ~60fps max latency
                {
                    ProcessFrame();
                }
            }
        }

        private void ProcessFrame()
        {
            if (!_enabled)
                return;

            Bitmap frame = null;
            lock (_frameLock)
            {
                // ⭐ 双重保险：即使 _currentFrame 不为 null，也可能已损坏
                try
                {
                    if (_currentFrame != null)
                        frame = (Bitmap)_currentFrame.Clone();
                }
                catch (Exception ex)
                {
                    LogService.Error("[AutoCapture] Failed to clone frame", ex);
                    return;
                }
            }

            if (frame == null)
                return;

            // ⭐ 额外的安全检查：确保帧有效
            if (frame.Width <= 0 || frame.Height <= 0)
            {
                frame.Dispose();
                return;
            }

            try
            {
                bool changing = _detector.IsFrameChanging(frame);

                // ⭐ 移除每帧日志，避免文件 I/O 阻塞（会造成明显延迟）
                // LogService.Info($"[AutoCapture] state={_state}, changing={changing}, stable={_stableCount}");

                switch (_state)
                {
                    case State.Unstable:
                        HandleUnstable(changing, frame);
                        break;

                    case State.Ready:
                        HandleReady(changing, frame);
                        break;

                    case State.Processing:
                        // 🚫 Processing 期间绝不被打断
                        break;

                    case State.Processed:
                        // 只有明显变化才允许重新开始
                        if (changing)
                        {
                            ResetToUnstable();
                        }
                        break;
                }
            }
            finally
            {
                frame.Dispose();
            }
        }

        // ================= 状态处理 =================

        private void HandleUnstable(bool changing, Bitmap frame)
        {
            long now = DateTime.Now.Ticks / 10_000;

            if (!_firstFrameInitialized)
            {
                // ⭐ 首次初始化：直接进入 Ready，建立当前画面为参考基准
                _state = State.Ready;
                _detector.ConfirmStable(frame);
                _firstFrameInitialized = true;
                LogService.Info("[AutoCapture] -> Ready (首次初始化)");
                return;
            }

            if (!changing)
            {
                // 画面静止，重置变化计时器
                _changingStartTime = 0;

                _stableCount++;
                if (_stableCount >= EnterReadyThreshold)
                {
                    _state = State.Ready;
                    _stableCount = 0;
                    // ⭐ 确认当前帧为稳定参考帧，后续变化都以此为基准
                    _detector.ConfirmStable(frame);
                    LogService.Info("[AutoCapture] -> Ready (稳定帧已确认)");
                }
            }
            else
            {
                // 画面在变化（可能是用户正在换纸）
                // 记录首次检测到变化的时刻
                if (_changingStartTime == 0)
                {
                    _changingStartTime = now;
                }

                // 超时说明用户已经换好新纸，直接更新参考帧
                if (now - _changingStartTime > ChangingTimeoutMs)
                {
                    _detector.ConfirmStable(frame);
                    _changingStartTime = 0;
                    _stableCount = 0;
                    // 直接进入 Ready，等待短暂稳定后触发识别
                    _state = State.Ready;
                    LogService.Info("[AutoCapture] -> Ready (换纸超时，更新参考帧)");
                }
                else
                {
                    _stableCount = 0;
                }
            }
        }

        private void HandleReady(bool changing, Bitmap frame)
        {
            if (changing)
            {
                ResetToUnstable();
                return;
            }

            _stableCount++;
            if (_stableCount >= StableFrameThreshold)
            {
                TryCapture(frame);
            }
        }

        // ================= Capture =================

        private void TryCapture(Bitmap frame)
        {
            long now = DateTime.Now.Ticks / 10_000;
            if (now - _lastCaptureTime < CooldownMs)
            {
                ResetToUnstable();
                return;
            }

            _lastCaptureTime = now;
            _state = State.Processing;
            _stableCount = 0;

            // ⭐ ConfirmStable 已经在 HandleUnstable 中调用，无需再次提交

            Bitmap snapshot;
            lock (_frameLock)
            {
                snapshot = _currentFrame != null ? (Bitmap)_currentFrame.Clone() : null;
            }

            if (snapshot == null)
            {
                ResetToUnstable();
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var result = _pipeline.ProcessFrame(snapshot);

                    Callback?.Invoke(result);

                    _state = (result.IsSuccess || result.IsDuplicate)
                        ? State.Processed
                        : State.Unstable;
                }
                catch (Exception ex)
                {
                    LogService.Error("Capture error", ex);
                    _state = State.Unstable;
                }
                finally
                {
                    snapshot.Dispose();
                }
            });
        }

        // ================= 工具 =================

        private void ResetInternal()
        {
            _state = State.Disabled;
            _stableCount = 0;
        }

        private void ResetToUnstable()
        {
            _state = State.Unstable;
            _stableCount = 0;
            LogService.Info("[AutoCapture] -> Unstable");
        }
    }
}
