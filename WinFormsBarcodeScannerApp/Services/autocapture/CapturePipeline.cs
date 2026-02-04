using Emgu.CV;
using Emgu.CV.Structure;
// 必须添加下面这一行，否则无法识别扩展方法
using Emgu.Util.TypeEnum;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using WinFormsBarcodeScannerApp.Services;

namespace WinFormsBarcodeScannerApp.Services.autocapture
{
    /// <summary>
    /// CapturePipeline - Thread-safe single-frame processor
    /// -----------------------------------------------
    /// NON-NEGOTIABLE RULES:
    /// 1. MUST clone callerImage immediately - callerImage is NOT owned by us
    /// 2. MUST clone AGAIN for each distinct operation (barcode, save)
    /// 3. MUST own and Dispose all Bitmaps we create
    /// 4. MUST be synchronous (no async/await)
    /// </summary>
    public sealed class CapturePipeline
    {
        private readonly BarcodeService _barcodeService;
        private readonly BarcodeDeduplicator _deduplicator;
        private readonly string _outputDir;

        public CapturePipeline(
            BarcodeService barcodeService,
            string outputDir)
        {
            _barcodeService = barcodeService ?? throw new ArgumentNullException(nameof(barcodeService));
            _deduplicator = new BarcodeDeduplicator();
            _outputDir = outputDir ?? throw new ArgumentNullException(nameof(_outputDir));

            if (!Directory.Exists(_outputDir))
                Directory.CreateDirectory(_outputDir);
        }

        public CaptureResult ProcessFrame(Bitmap callerImage)
        {
            if (callerImage == null)
                return CaptureResult.CreateFailure("输入图像为空");

            long t0 = DateTime.Now.Ticks;
            long t1, t2, t3, t4, t5;

            // OWNERSHIP: We own all these Bitmaps - dispose them in finally
            Bitmap ownedBarcodeImage = null;
            Bitmap ownedCorrectedImage = null;

            try
            {
                LogService.Info("[Pipeline] 开始处理帧");

                // STEP 1: Clone for barcode recognition (WE OWN THIS)
                t1 = DateTime.Now.Ticks;
                ownedBarcodeImage = (Bitmap)callerImage.Clone();
                LogService.Info($"[Pipeline] Clone: {(t1 - t0) / 10000}ms");

                // STEP 2: A4 detection + correction (High camera, EmguCV) - may create new Bitmap (ownedCorrectedImage)
                t2 = DateTime.Now.Ticks;
                try
                {
                    Mat warpedMat = A4PaperDetectorHighCamera.ProcessDocument(ownedBarcodeImage);
                    if (warpedMat != null && !warpedMat.IsEmpty)
                    {
                        Image<Rgba, byte> warpedImage = warpedMat.ToImage<Rgba, byte>();
                        ownedBarcodeImage = warpedImage.ToBitmap();
                        warpedImage.Dispose();
                        LogService.Info("[Pipeline] A4检测与校正(高拍仪)成功");
                    }
                    else
                    {
                        LogService.Info("[Pipeline] A4检测与校正(高拍仪)未找到有效A4，继续使用原图");
                        return CaptureResult.CreateFailure("A4检测与校正(高拍仪)未找到有效A4");
                    }

                }
                catch (Exception ex)
                {
                    LogService.Error("[Pipeline] A4检测与校正(高拍仪)异常，继续使用原图", ex);
                }
                LogService.Info($"[Pipeline] A4检测与校正(高拍仪)耗时: {(DateTime.Now.Ticks - t2) / 10000}ms");

                // STEP 3: Barcode recognition - uses ownedBarcodeImage
                t3 = DateTime.Now.Ticks;
                string barcode = _barcodeService.Recognize(ownedBarcodeImage);
                LogService.Info($"[Pipeline] 条码识别: barcode='{barcode}', 耗时: {(DateTime.Now.Ticks - t3) / 10000}ms");

                if (string.IsNullOrWhiteSpace(barcode))
                    return CaptureResult.CreateFailure("条码识别失败");

                // STEP 4: Deduplication
                t4 = DateTime.Now.Ticks;
                bool isDuplicate = _deduplicator.IsDuplicate(barcode);
                LogService.Info($"[Pipeline] 去重检查: barcode={barcode}, isDuplicate={isDuplicate}, 耗时: {(DateTime.Now.Ticks - t4) / 10000}ms");
                if (isDuplicate)
                    return CaptureResult.CreateDuplicate(barcode);

                // STEP 5: Save image - clone from corrected or original (同步执行，阻塞主流程直到完成)
                t5 = DateTime.Now.Ticks;
                string safeBarcode = PathHelper.MakeSafeFileName(barcode);
                string fileName = safeBarcode + ".jpg";
                string filePath = Path.Combine(_outputDir, fileName);

                // Clone for saving - we OWN the source (either corrected or barcode image)
                using (Bitmap imageToSave = (Bitmap)(ownedCorrectedImage ?? ownedBarcodeImage).Clone())
                {
                    LogService.Info($"[Pipeline] Clone: {(DateTime.Now.Ticks - t5) / 10000}ms");
                    try
                    {
                        long saveStart = DateTime.Now.Ticks;

                        // 压缩图片：限制最大尺寸以减小文件体积，使用 JPEG 格式
                        using (Bitmap compressedImage = CompressImage(imageToSave, maxWidth: 1200))
                        {
                            var encoder = GetJpegEncoder(80);
                            compressedImage.Save(filePath, encoder, _jpegEncoderParams);
                        }
                        LogService.Info($"[Pipeline] 同步保存完成: {filePath}, 耗时: {(DateTime.Now.Ticks - saveStart) / 10000}ms");
                    }
                    catch (Exception ex)
                    {
                        LogService.Error("保存失败: " + filePath, ex);
                    }
                }

                return CaptureResult.CreateSuccess(barcode, filePath);
            }
            catch (Exception ex)
            {
                LogService.Error("处理帧失败", ex);
                return CaptureResult.CreateFailure(ex.Message);
            }
            finally
            {
                // CRITICAL: Dispose owned Bitmaps in reverse order
                // A4 detection creates a new bitmap (ownedCorrectedImage) that we own
                // We always own ownedBarcodeImage
                if (ownedCorrectedImage != null)
                {
                    ownedCorrectedImage.Dispose();
                    ownedCorrectedImage = null;
                }
                if (ownedBarcodeImage != null)
                {
                    ownedBarcodeImage.Dispose();
                    ownedBarcodeImage = null;
                }
            }
        }

        public sealed class CaptureResult
        {
            public bool IsSuccess { get; }
            public bool IsDuplicate { get; }
            public string Barcode { get; }
            public string FilePath { get; }
            public string ErrorMessage { get; }

            private CaptureResult(bool isSuccess, bool isDuplicate, string barcode, string filePath, string errorMessage)
            {
                IsSuccess = isSuccess;
                IsDuplicate = isDuplicate;
                Barcode = barcode;
                FilePath = filePath;
                ErrorMessage = errorMessage;
            }

            public static CaptureResult CreateSuccess(string barcode, string filePath)
                => new CaptureResult(true, false, barcode, filePath, null);

            public static CaptureResult CreateDuplicate(string barcode)
                => new CaptureResult(false, true, barcode, null, null);

            public static CaptureResult CreateFailure(string message)
                => new CaptureResult(false, false, null, null, message);
        }

        /// <summary>
        /// 压缩图片，限制最大尺寸
        /// </summary>
        private static Bitmap CompressImage(Bitmap source, int maxWidth)
        {
            int width = source.Width;
            int height = source.Height;

            // 计算缩放比例
            float scale = 1.0f;
            if (width > maxWidth)
            {
                scale = (float)maxWidth / width;
            }

            if (scale < 1.0f)
            {
                int newWidth = (int)(width * scale);
                int newHeight = (int)(height * scale);

                var result = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(source, 0, 0, newWidth, newHeight);
                }
                return result;
            }

            // 不需要缩放，返回原图副本
            return (Bitmap)source.Clone();
        }

        /// <summary>
        /// 获取 JPEG 编码器
        /// </summary>
        private static ImageCodecInfo GetJpegEncoder(long quality)
        {
            var jpegCodec = ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            if (jpegCodec == null)
                return null;

            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            // 保存编码器参数供 Save 使用（通过委托或闭包，这里简化为直接保存）
            _jpegEncoderParams = encoderParams;
            return jpegCodec;
        }
        private static EncoderParameters _jpegEncoderParams;

        /// <summary>
        /// 移除去重记录（删除历史记录时调用）
        /// </summary>
        public void RemoveDuplicateBarcode(string barcode)
        {
            _deduplicator.Remove(barcode);
        }
    }
}
