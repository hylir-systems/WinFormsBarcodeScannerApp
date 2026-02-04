using System;
using System.IO;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 通用路径 / 文件名工具，避免各处重复实现。
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// 将任意字符串转换为安全的文件名。
        /// </summary>
        public static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "sheet";

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new System.Text.StringBuilder(name.Length);
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (Array.IndexOf(invalid, c) >= 0)
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取默认 A4 图片保存目录。
        /// </summary>
        public static string GetDefaultA4Directory()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDir, "A4");
        }
    }
}


