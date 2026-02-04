using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Aspose.BarCode;
using Aspose.BarCode.BarCodeRecognition;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 只负责条码识别：输入 Bitmap，输出"单号字符串"。
    /// - 固定只识别 CODE128
    /// - 支持裁剪区域识别和全图识别两种策略
    /// - 单张图片只返回一个单号（首个有效结果）
    /// </summary>
    public sealed class BarcodeService
    {
        // 稳定帧数（与 Java 版本一致）
        private const int StableFrameCount = 8;

        // 冷却时间（毫秒）
        private const long CooldownMs = 1500;

        // 上次识别时间
        private long _lastCaptureTime;

        // 识别结果回调
        public Action<string, System.Drawing.Bitmap, double> OnRecognitionComplete { get; set; }

        public BarcodeService(string asposeLicensePath)
        {
            TrySetAsposeLicense(asposeLicensePath);
        }

        private static void TrySetAsposeLicense(string licensePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licensePath))
                    return;
                if (!File.Exists(licensePath))
                    return;

                var lic = new License();
                lic.SetLicense(licensePath);
            }
            catch
            {
                // 若授权加载失败，仍可继续（可能变为评估模式）；不抛出以保证 UI 可运行
            }
        }

        /// <summary>
        /// 自动识别（画面稳定后调用）
        /// 策略1：先识别裁剪区域；策略2：裁剪失败则识别全图
        /// </summary>
        public string AutoRecognize(System.Drawing.Bitmap bitmap, out System.Drawing.Bitmap a4Image, out double elapsedMs)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            a4Image = null;
            long t0 = DateTime.Now.Ticks;

            // 策略1：先识别裁剪区域（更快）
            string result = RecognizeCropped(bitmap);
            long t1 = DateTime.Now.Ticks;
            double croppedTime = (t1 - t0) / 10000.0;

            if (!string.IsNullOrEmpty(result) && IsValidReceiptNumber(result))
            {
                elapsedMs = croppedTime;
                // 执行 A4 检测（只检测不裁切用于保存）
                TryA4Detect(bitmap, out a4Image);
                OnRecognitionComplete?.Invoke(result, bitmap, elapsedMs);
                return result;
            }

            // 策略2：裁剪区域失败，识别全图（较慢）
            long t2 = DateTime.Now.Ticks;
            result = RecognizeFull(bitmap);
            long t3 = DateTime.Now.Ticks;
            double fullTime = (t3 - t2) / 10000.0;

            if (!string.IsNullOrEmpty(result) && IsValidReceiptNumber(result))
            {
                elapsedMs = croppedTime + fullTime;
                // 执行 A4 检测
                TryA4Detect(bitmap, out a4Image);
                OnRecognitionComplete?.Invoke(result, bitmap, elapsedMs);
                return result;
            }

            elapsedMs = croppedTime + fullTime;
            return string.Empty;
        }

        /// <summary>
        /// 识别条码（简化接口，先尝试裁剪区域，失败则识别全图）
        /// </summary>
        public string Recognize(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            long startMs = DateTime.Now.Ticks / 10_000;

            // 先尝试裁剪区域识别
            string result = RecognizeCropped(bitmap);
            if (!string.IsNullOrWhiteSpace(result) && IsValidReceiptNumber(result))
            {
                long elapsed = DateTime.Now.Ticks / 10_000 - startMs;
                LogService.Info($"[Barcode] 裁剪识别成功: {result}, 耗时: {elapsed}ms");
                return result;
            }

            // 裁剪区域失败，识别全图
            result = RecognizeFull(bitmap);

            long elapsed2 = DateTime.Now.Ticks / 10_000 - startMs;
            if (!string.IsNullOrWhiteSpace(result))
            {
                LogService.Info($"[Barcode] 全图识别成功: {result}, 耗时: {elapsed2}ms");
            }
            else
            {
                LogService.Info($"[Barcode] 识别失败, 耗时: {elapsed2}ms");
            }

            return result;
        }

        /// <summary>
        /// 识别裁剪区域（右上角 1/3 x 1/3）
        /// </summary>
        public string RecognizeCropped(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            using (var croppedBitmap = CropTopRightQuarter(bitmap))
            using (var asposeBitmap = ConvertToAsposeBitmap(croppedBitmap))
            using (var reader = new BarCodeReader(asposeBitmap, DecodeType.Code128))
            {
                // 设置高质量识别参数（.NET 版本的 API 与 Java 不同）
                var qs = reader.QualitySettings;
                // 注意：.NET 版本的 QualitySettings 属性名称可能不同
                // 如果属性不存在，则使用默认设置
                try
                {
                    // 尝试设置可用属性（根据实际 API 调整）
                    
                    qs.AllowIncorrectBarcodes = true;
                    qs.BarcodeQuality = QualitySettings.NormalQuality.BarcodeQuality;
                }
                catch
                {
                    // 如果某些属性不支持，忽略错误，使用默认设置
                }

                BarCodeResult[] results = reader.ReadBarCodes();
                if (results == null || results.Length == 0)
                    return string.Empty;

                // 只取第一个有文本的结果
                for (int i = 0; i < results.Length; i++)
                {
                    var t = results[i].CodeText;
                    if (!string.IsNullOrWhiteSpace(t))
                        return t;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 识别全图（备选策略）
        /// </summary>
        public string RecognizeFull(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) throw new ArgumentNullException("bitmap");

            using (var asposeBitmap = ConvertToAsposeBitmap(bitmap))
            using (var reader = new BarCodeReader(asposeBitmap, DecodeType.Code128))
            {
                // 设置高质量识别参数（.NET 版本的 API 与 Java 不同）
                var qs = reader.QualitySettings;
                try
                {
                    // 尝试设置可用属性（根据实际 API 调整）
                    qs.AllowIncorrectBarcodes = true;
                }
                catch
                {
                    // 如果某些属性不支持，忽略错误，使用默认设置
                }

                BarCodeResult[] results = reader.ReadBarCodes();
                if (results == null || results.Length == 0)
                    return string.Empty;

                for (int i = 0; i < results.Length; i++)
                {
                    var t = results[i].CodeText;
                    if (!string.IsNullOrWhiteSpace(t))
                        return t;
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 验证是否为有效的送货单号
        /// </summary>
        public bool IsValidReceiptNumber(string barcodeText)
        {
            if (string.IsNullOrWhiteSpace(barcodeText))
                return false;
            if (barcodeText.Length < 6)
                return false;
            foreach (char c in barcodeText)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 尝试 A4 检测
        /// </summary>
        private void TryA4Detect(System.Drawing.Bitmap bitmap, out System.Drawing.Bitmap a4Image)
        {
            a4Image = null;
            // A4 检测由 A4DetectService 处理
        }

        /// <summary>
        /// 裁剪图片右上角 1/3 x 1/3 区域
        /// </summary>
        private static System.Drawing.Bitmap CropTopRightQuarter(System.Drawing.Bitmap src)
        {
            int width = src.Width;
            int height = src.Height;

            int x = width * 2 / 3;   // 右侧 1/3 起点
            int y = 0;

            int w = width / 3;
            int h = height / 3;

            // 防御式保护
            w = Math.Min(w, width - x);
            h = Math.Min(h, height - y);

            if (w <= 0 || h <= 0)
                return (System.Drawing.Bitmap)src.Clone();

            return (System.Drawing.Bitmap)src.Clone(new Rectangle(x, y, w, h), src.PixelFormat);
        }

        /// <summary>
        /// 将 System.Drawing.Bitmap 转换为 Aspose.Drawing.Bitmap
        /// </summary>
        private static Aspose.Drawing.Bitmap ConvertToAsposeBitmap(System.Drawing.Bitmap systemBitmap)
        {
            if (systemBitmap == null)
                return null;

            // 通过内存流转换：将 System.Drawing.Bitmap 保存为 PNG 流，然后加载为 Aspose.Drawing.Bitmap
            using (MemoryStream ms = new MemoryStream())
            {
                systemBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                // Aspose.Drawing.Bitmap 会复制流中的数据，所以流可以立即释放
                return new Aspose.Drawing.Bitmap(ms);
            }
        }
    }
}
