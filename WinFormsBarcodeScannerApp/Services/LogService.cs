using System;
using System.IO;
using System.Text;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 工业现场：最小可用的文件日志（不引入第三方，不依赖复杂线程）
    /// </summary>
    public static class LogService
    {
        private static readonly object LockObj = new object();

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }
        public static void Warn(string message)
        {
            Write("WARN", message, null);
        }
        public static void Error(string message, Exception ex)
        {
            Write("ERROR", message, ex);
        }

        private static void Write(string level, string message, Exception ex)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                string file = Path.Combine(dir, "app.log");
                Directory.CreateDirectory(dir);

                var sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                sb.Append(" [").Append(level).Append("] ");
                sb.Append(message ?? string.Empty);

                if (ex != null)
                {
                    sb.AppendLine();
                    sb.Append(ex.ToString());
                }

                string line = sb.ToString();
                lock (LockObj)
                {
                    File.AppendAllText(file, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // 现场环境：日志失败不影响主流程
            }
        }
    }
}


