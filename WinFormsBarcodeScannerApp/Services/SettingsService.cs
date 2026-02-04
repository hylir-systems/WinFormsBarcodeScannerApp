using System;
using System.IO;

namespace WinFormsBarcodeScannerApp.Services
{
    /// <summary>
    /// 极简本地配置：仅负责后端地址与 A4 保存目录。
    /// 不引入额外库，使用 key=value 文本文件。
    /// </summary>
    public sealed class AppSettings
    {
        public string BackendUrl;
        public string A4SaveDirectory;

        public static AppSettings CreateDefault()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            return new AppSettings
            {
                BackendUrl = "http://117.143.214.90:3680/api",
                A4SaveDirectory = Path.Combine(baseDir, "A4")
            };
        }
    }

    public static class SettingsService
    {
        private const string FileName = "settings.config";

        public static AppSettings Load()
        {
            var settings = AppSettings.CreateDefault();

            try
            {
                string path = GetPath();
                if (!File.Exists(path))
                    return settings;

                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;

                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1).Trim();

                    if (string.Equals(key, "BackendUrl", StringComparison.OrdinalIgnoreCase))
                        settings.BackendUrl = value;
                    else if (string.Equals(key, "A4SaveDirectory", StringComparison.OrdinalIgnoreCase))
                        settings.A4SaveDirectory = value;
                }
            }
            catch
            {
                // 读取失败时使用默认值
            }

            return settings;
        }

        public static void Save(AppSettings settings)
        {
            if (settings == null) return;

            try
            {
                string path = GetPath();
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                using (var writer = new StreamWriter(path, false))
                {
                    writer.WriteLine("BackendUrl=" + (settings.BackendUrl ?? string.Empty));
                    writer.WriteLine("A4SaveDirectory=" + (settings.A4SaveDirectory ?? string.Empty));
                }
            }
            catch
            {
                // 写入失败不抛异常，交给上层日志
            }
        }

        private static string GetPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
        }
    }
}


