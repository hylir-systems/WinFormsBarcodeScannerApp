using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 极简工业扫描器 - 适配固定相机场景
    /// 假设：固定相机、固定距离、固定分辨率(3264x2448)、桌面白纸
    /// </summary>
    public static class A4PaperDetectorHighCamera
    {
        #region 配置

        /// <summary>
        /// 源图像分辨率（高拍仪固定分辨率）
        /// </summary>
        private static readonly Size SourceResolution = new Size(3264, 2448);

        /// <summary>
        /// 调试输出配置（调试时改为 true）
        /// </summary>
        private static readonly bool EnableDebug = false;
        private static readonly string DebugDir = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "DebugOutput");

        #endregion

        #region 工业视觉流程：阈值 → 最大白块 → 框 → 裁 → 比例判断

        /// <summary>
        /// 处理文档（工业视觉五步法）
        /// 1. 阈值 - 2. 最大白块 - 3. 框 - 4. 裁 - 5. 比例判断
        /// </summary>
        public static Mat ProcessDocument(Bitmap srcBitmap, int dpi = 300)
        {
            if (srcBitmap == null)
                return null;

            // 确保调试目录存在
            if (EnableDebug) Directory.CreateDirectory(DebugDir);

            // ===== 1. 预处理 = 原图裁剪 =====
            using Mat src = Preprocess(srcBitmap);
            if (src == null || src.IsEmpty)
            {
                LogService.Error("[Scanner] Preprocess 返回空",null);
                return null;
            }
            SaveDebug(src, "01_preprocess", EnableDebug);

            // ===== 2. 阈值 = Otsu 自动二值化 = 找白纸 =====
            using Mat mask = GetDocumentMask(src);
            if (mask == null || mask.IsEmpty)
            {
                LogService.Error("[Scanner] GetDocumentMask 返回空", null);
                return null;
            }
            SaveDebug(mask, "02_threshold_mask", EnableDebug);

            // ===== 3. 最大白块 = 最大轮廓 =====
            PointF[] corners = ExtractCorners(mask);
            SaveDebug(DrawCorners(src, corners), "03_contours", EnableDebug);

            // 降级：4. 框（自动失败时用预设框）
            if (corners == null || corners.Length != 4)
            {
                corners = GetDefaultCorners();
                LogService.Warn("[Scanner] 自动检测失败，使用预设框");
                SaveDebug(DrawCorners(src, corners), "04_default_corners", EnableDebug);
            }

            // ===== 5. 裁 = 透视变换（直接取4个点包含的内容）=====
            using Mat warped = WarpPerspective(src, corners);
            SaveDebug(warped, "05_warped", EnableDebug);

            // ===== 6. 比例判断 = A4 / A5 =====
            ValidatePaperRatio(warped);

            return warped.Clone();
        }

        #region 预处理

        /// <summary>
        /// 预处理：裁剪有效区域 + 灰度化
        /// </summary>
        private static Mat Preprocess(Bitmap bmp)
        {
            try
            {
                // Bitmap → Mat
                byte[] bytes = (byte[])new ImageConverter().ConvertTo(bmp, typeof(byte[]));
                using var ms = new MemoryStream(bytes);
                using Mat mat = new Mat();
                CvInvoke.Imdecode(ms, ImreadModes.AnyColor, mat);

                if (mat.IsEmpty)
                {
                    LogService.Error("[Scanner] Imdecode 失败，返回空 Mat",null);
                    return null;
                }

                LogService.Info($"[Scanner] 源图: {mat.Width}x{mat.Height}");

                // 裁剪中心有效区域（去除边缘暗角）
                int margin = Math.Min(mat.Width, mat.Height) / 30;
                Rectangle roi = new Rectangle(margin, margin,
                    mat.Width - margin * 2, mat.Height - margin * 2);

                using Mat cropped = new Mat(mat, roi);
                return cropped.Clone();
            }
            catch (Exception ex)
            {
                LogService.Error("[Scanner] Preprocess 异常", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取文档二值化掩码
        /// </summary>
        private static Mat GetDocumentMask(Mat src)
        {
            using Mat gray = new Mat();
            CvInvoke.CvtColor(src, gray, ColorConversion.Bgr2Gray);

            // Otsu 自动阈值（需要临时 Mat 接收结果）
            using Mat tempDst = new Mat();
            double threshold = CvInvoke.Threshold(gray, tempDst, 0, 255, ThresholdType.Otsu);
            
            using Mat binary = new Mat();
            CvInvoke.Threshold(gray, binary, threshold, 255, ThresholdType.Binary);

            // 反转：白纸区域为白色
            CvInvoke.BitwiseNot(binary, binary);

            // 开运算去噪
            using Mat kernel = CvInvoke.GetStructuringElement(MorphShapes.Rectangle, new Size(5, 5), new Point(-1, -1));
            CvInvoke.MorphologyEx(binary, binary, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Default, default);

            return binary.Clone();
        }

        #endregion

        #region 角点提取

        /// <summary>
        /// 提取四个角点（简化版）
        /// </summary>
        private static PointF[] ExtractCorners(Mat mask)
        {
            using Emgu.CV.Util.VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(mask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // 找最大轮廓
            int maxIdx = -1;
            double maxArea = 0;

            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if (area > maxArea)
                {
                    maxArea = area;
                    maxIdx = i;
                }
            }

            if (maxIdx < 0)
                return null;

            // 近似多边形
            using var approx = new VectorOfPoint();
            double peri = CvInvoke.ArcLength(contours[maxIdx], true);
            CvInvoke.ApproxPolyDP(contours[maxIdx], approx, 0.02 * peri, true);

            if (approx.Size == 4)
            {
                PointF[] pts = Array.ConvertAll(approx.ToArray(), p => new PointF(p.X, p.Y));
                return SortCorners(pts);
            }
            else if (approx.Size > 4)
            {
                // 使用凸包提取四边形
                return GetQuadFromHull(contours[maxIdx]);
            }

            return null;
        }

        /// <summary>
        /// 从凸包提取四边形
        /// </summary>
        private static PointF[] GetQuadFromHull(VectorOfPoint contour)
        {
            using var hull = new VectorOfPoint();
            CvInvoke.ConvexHull(contour, hull, true);

            if (hull.Size < 4)
                return null;

            var pts = Array.ConvertAll(hull.ToArray(), p => new PointF(p.X, p.Y));
            return SortCorners(pts);
        }

        /// <summary>
        /// 排序角点：TL, TR, BR, BL
        /// 使用更可靠的算法：先按X坐标分组，再按Y坐标排序
        /// </summary>
        private static PointF[] SortCorners(PointF[] pts)
        {
            if (pts == null || pts.Length != 4)
                return pts;

            // 按X坐标排序
            var sortedByX = pts.OrderBy(p => p.X).ToArray();
            
            // 左边的两个点（X较小的两个）
            PointF[] leftPoints = new[] { sortedByX[0], sortedByX[1] };
            // 右边的两个点（X较大的两个）
            PointF[] rightPoints = new[] { sortedByX[2], sortedByX[3] };

            // 在左边两点中，按Y坐标排序：上面的就是 TL，下面的就是 BL
            PointF tl = leftPoints.OrderBy(p => p.Y).First();
            PointF bl = leftPoints.OrderBy(p => p.Y).Last();

            // 在右边两点中，按Y坐标排序：上面的就是 TR，下面的就是 BR
            PointF tr = rightPoints.OrderBy(p => p.Y).First();
            PointF br = rightPoints.OrderBy(p => p.Y).Last();

            return new[] { tl, tr, br, bl };
        }

        /// <summary>
        /// 预定义角点（固定相机预标定）
        /// </summary>
        private static PointF[] GetDefaultCorners()
        {
            float w = 3264 * 0.85f, h = 2448 * 0.88f;
            float x = (3264 - w) / 2, y = (2448 - h) / 2;
            return new[] { new PointF(x, y), new PointF(x + w, y), new PointF(x + w, y + h), new PointF(x, y + h) };
        }

        #endregion

        #region 比例判断

        /// <summary>
        /// 5. 比例判断 - A4 ≈ 1.414 | A5 ≈ 0.707
        /// </summary>
        private static void ValidatePaperRatio(Mat src)
        {
            double ratio = (double)src.Width / src.Height;
            string type = ratio > 1.2 ? "A4" : (ratio < 0.85 ? "A5" : "Unknown");
            LogService.Info($"[Scanner] 输出: {src.Width}x{src.Height}px, 比例={ratio:F3} → {type}");
        }

        #endregion

        #region 透视校正

        /// <summary>
        /// 透视校正：直接取4个点包含的内容，使用实际检测到的尺寸
        /// </summary>
        private static Mat WarpPerspective(Mat src, PointF[] corners)
        {
            // ========== 1. 计算倾斜角度 ==========
            // corners: [0]=TL, [1]=TR, [2]=BR, [3]=BL
            double leftAngle = Math.Atan2(corners[3].Y - corners[0].Y, corners[3].X - corners[0].X);
            double rightAngle = Math.Atan2(corners[2].Y - corners[1].Y, corners[2].X - corners[1].X);
            double avgAngle = (leftAngle + rightAngle) / 2; // 弧度

            // ========== 2. 透视变换 ==========
            // 计算源图实际边长
            double topLen = Distance(corners[0], corners[1]);
            double bottomLen = Distance(corners[2], corners[3]);
            double leftLen = Distance(corners[3], corners[0]);
            double rightLen = Distance(corners[1], corners[2]);

            double avgWidth = (topLen + bottomLen) / 2;
            double avgHeight = (leftLen + rightLen) / 2;

            // 目标尺寸 = 检测到的实际像素尺寸
            int dstWidth = (int)avgWidth;
            int dstHeight = (int)avgHeight;

            // 目标角点（竖向矩形）
            PointF[] dst = new[]
            {
                new PointF(0, 0),
                new PointF(dstWidth - 1, 0),
                new PointF(dstWidth - 1, dstHeight - 1),
                new PointF(0, dstHeight - 1)
            };

            // 透视变换
            using Mat M = CvInvoke.GetPerspectiveTransform(corners, dst);

            Mat result = new Mat();
            CvInvoke.WarpPerspective(src, result, M, new Size(dstWidth, dstHeight),
                Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255, 255, 255));

            return result;
        }
        #endregion

        #region 工具

        private static double Distance(PointF a, PointF b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 绘制角点（调试用）
        /// </summary>
        private static Mat DrawCorners(Mat src, PointF[] corners)
        {
            if (corners == null || corners.Length != 4)
                return src.Clone();

            Mat result = src.Clone();
            var colors = new[] { new MCvScalar(0, 255, 0), new MCvScalar(0, 0, 255),
                               new MCvScalar(255, 0, 0), new MCvScalar(0, 255, 255) };
            var labels = new[] { "TL", "TR", "BR", "BL" };

            for (int i = 0; i < 4; i++)
            {
                var pt = new Point((int)corners[i].X, (int)corners[i].Y);
                CvInvoke.Circle(result, pt, 15, colors[i], 3);
                CvInvoke.PutText(result, labels[i], new Point(pt.X + 18, pt.Y - 18),
                    FontFace.HersheySimplex, 1.0, colors[i], 2);
            }

            Point[] poly = Array.ConvertAll(corners, p => new Point((int)p.X, (int)p.Y));
            CvInvoke.Polylines(result, new VectorOfPoint(poly), true, new MCvScalar(0, 255, 0), 3);
            return result;
        }

        private static void SaveDebug(Mat img, string name, bool enabled)
        {
            if (!enabled) return;
            try
            {
                Directory.CreateDirectory(DebugDir);
                CvInvoke.Imwrite(Path.Combine(DebugDir, $"{name}_{DateTime.Now:HHmmss_fff}.png"), img);
            }
            catch { /* 忽略 */ }
        }

        #endregion
    }
}

#endregion