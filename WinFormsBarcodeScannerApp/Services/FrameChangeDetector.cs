using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// FrameChangeDetector - PURE, FAST PER-FRAME COMPARATOR
    /// -----------------------------------------------
    /// Contract:
    /// - Answers ONLY: "Is current frame significantly changing from last stable frame?"
    /// - NO temporal decisions (no counting stable frames)
    /// - NO automatic reference frame updates
    /// - NO business logic or pipeline control
    ///
    /// Key optimizations:
    /// - Luminance-only diff (single channel, faster than RGB)
    /// - Early-exit when change ratio clearly exceeds threshold
    /// - Minimal memory allocations
    /// - Single LockBits per frame (shared buffer extraction)
    /// </summary>

    public sealed class FrameChangeDetector
    {
        // === TUNABLE PARAMETERS ===
        private const int SampleStep = 20;         // Sampling stride (20px for accuracy)
        private const int LuminanceDiffThreshold = 20; // Noise dead-zone (ignore small changes)
        private const double ChangeRatioThreshold = 0.15; // 15%+ luminance change = "changing"
        private const double EarlyExitRatio = 0.25; // Exit early if clearly > 25% changed

        // === STATE ===
        private byte[] _lastSamples; // Luminance values of last confirmed stable frame
        private int _sampleWidth;
        private int _sampleHeight;
        private bool _initialized;

        public void Reset()
        {
            _lastSamples = null;
            _initialized = false;
        }

        /// <summary>
        /// Extract luminance samples from frame into pre-allocated byte array.
        /// Optimized: sample directly from LockBits pointer; do NOT copy full frame to managed buffer.
        /// </summary>
        private static void ExtractLuminanceSamples(Bitmap frame, byte[] samples, int sampleW, int sampleH)
        {
            int width = frame.Width;
            int height = frame.Height;

            Rectangle rect = new Rectangle(0, 0, width, height);
            var pixelFormat = frame.PixelFormat;
            int bpp = Image.GetPixelFormatSize(pixelFormat) / 8;
            if (bpp != 3 && bpp != 4)
                throw new NotSupportedException("Only 24/32 bpp frames are supported: " + pixelFormat);

            BitmapData data = frame.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
            try
            {
                int stride = data.Stride;
                int idx = 0;
                unsafe
                {
                    byte* basePtr = (byte*)data.Scan0;
                    for (int y = 0; y < sampleH; y++)
                    {
                        int py = y * SampleStep;
                        byte* rowPtr = basePtr + (py * stride);
                        for (int x = 0; x < sampleW; x++)
                        {
                            int px = x * SampleStep;
                            byte* p = rowPtr + (px * bpp);

                            // BGR(A) in little-endian DIBs used by GDI+
                            byte b = p[0];
                            byte g = p[1];
                            byte r = p[2];

                            // Luminance: 0.299*R + 0.587*G + 0.114*B (scaled by 1000)
                            samples[idx++] = (byte)((r * 299 + g * 587 + b * 114) / 1000);
                        }
                    }
                }
            }
            finally
            {
                frame.UnlockBits(data);
            }
        }

        /// <summary>
        /// Called by pipeline when frame is confirmed stable.
        /// Updates reference frame from current frame.
        /// </summary>
        public void ConfirmStable(Bitmap frame)
        {
            int width = frame.Width;
            int height = frame.Height;
            int sampleW = (width + SampleStep - 1) / SampleStep;
            int sampleH = (height + SampleStep - 1) / SampleStep;
            int totalSamples = sampleW * sampleH;

            byte[] samples = new byte[totalSamples];
            ExtractLuminanceSamples(frame, samples, sampleW, sampleH);

            _lastSamples = samples;
            _sampleWidth = sampleW;
            _sampleHeight = sampleH;
            _initialized = true;
        }

        /// <summary>
        /// Called when barcode capture is committed.
        /// Finalizes the current stable frame reference (no-op if already finalized).
        /// </summary>
        public void CommitStableFrame()
        {
            // Reference frame is already committed via ConfirmStable()
            // This method exists for symmetry with pipeline lifecycle
        }

        /// <summary>
        /// PURE FUNCTION: Compare current frame against last confirmed stable frame.
        /// Returns true if frame has significantly changed (change ratio > threshold).
        /// </summary>
        public bool IsFrameChanging(Bitmap frame)
        {
            if (frame == null)
                return false;

            int width = frame.Width;
            int height = frame.Height;
            int sampleW = (width + SampleStep - 1) / SampleStep;
            int sampleH = (height + SampleStep - 1) / SampleStep;
            int totalSamples = sampleW * sampleH;

            // First frame or resolution change: establish baseline
            if (!_initialized || _lastSamples == null || sampleW != _sampleWidth || sampleH != _sampleHeight)
            {
                ConfirmStable(frame);
                return false; // First frame, no change
            }

            // Compare with early exit
            int changedPixels = 0;
            int earlyExitThreshold = (int)(totalSamples * EarlyExitRatio);

            Rectangle rect = new Rectangle(0, 0, width, height);
            var pixelFormat = frame.PixelFormat;
            int bpp = Image.GetPixelFormatSize(pixelFormat) / 8;
            if (bpp != 3 && bpp != 4)
                return true; // unexpected pixel format -> treat as changing to be safe

            BitmapData data = frame.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
            try
            {
                int stride = data.Stride;
                int idx = 0;
                unsafe
                {
                    byte* basePtr = (byte*)data.Scan0;
                    for (int y = 0; y < sampleH; y++)
                    {
                        int py = y * SampleStep;
                        byte* rowPtr = basePtr + (py * stride);
                        for (int x = 0; x < sampleW; x++)
                        {
                            int px = x * SampleStep;
                            byte* p = rowPtr + (px * bpp);
                            byte b = p[0];
                            byte g = p[1];
                            byte r = p[2];
                            byte lum = (byte)((r * 299 + g * 587 + b * 114) / 1000);

                            if (Math.Abs(lum - _lastSamples[idx]) > LuminanceDiffThreshold)
                            {
                                changedPixels++;
                                if (changedPixels > earlyExitThreshold)
                                    return true;
                            }
                            idx++;
                        }
                    }
                }
            }
            finally
            {
                frame.UnlockBits(data);
            }

            // Final decision based on change ratio
            double changeRatio = (double)changedPixels / totalSamples;
            return changeRatio > ChangeRatioThreshold;
        }
    }
}
