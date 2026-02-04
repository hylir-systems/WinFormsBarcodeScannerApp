using System;
using System.Windows.Forms;
using WinFormsBarcodeScannerApp.Services;

namespace WinFormsBarcodeScannerApp
{
    internal static class Program
    {
        // Aspose License（使用项目内的许可文件，会自动复制到输出目录）
        private static readonly string AsposeLicensePath =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "asserts", "Aspose.Total.NET.lic");

        [STAThread]
        private static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += ApplicationOnThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(AsposeLicensePath));
        }

        private static void ApplicationOnThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogService.Error("UI thread exception.", e.Exception);
            MessageBox.Show(
                "发生未处理异常（UI 线程）。详细信息已写入 logs\\app.log。\r\n\r\n" + e.Exception.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            LogService.Error("Non-UI unhandled exception.", ex);
            // 非 UI 线程异常：尽量记录即可，避免二次异常
        }
    }
}


