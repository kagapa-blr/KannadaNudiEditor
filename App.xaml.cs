using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KannadaNudiEditor.Helpers;
using KannadaNudiEditor.Views;

namespace KannadaNudiEditor
{
    public partial class App : Application
    {
        private readonly ProcessHelper _processHelper;

        public App()
        {
            SimpleLogger.Log("=== Application Started ===");

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
                SimpleLogger.Log("FirstChanceException: " + e.Exception.Message);

            _processHelper = new ProcessHelper();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string? startupFilePath = null;
            SimpleLogger.Log($"[APP] Args received: {e.Args.Length}");

            for (int i = 0; i < e.Args.Length; i++)
            {
                string arg = e.Args[i].Trim('"', '\'');
                SimpleLogger.Log($"[APP] Arg[{i}]: '{arg}'");

                // Check if this arg is a valid file
                if (File.Exists(arg))
                {
                    startupFilePath = arg;
                    SimpleLogger.Log($"[APP] File detected: {startupFilePath}");
                    break;
                }

                // Try combining adjacent args (for shell-split quoted paths)
                if (arg.Contains("\\"))
                {
                    var parts = new List<string> { arg };
                    for (int j = i + 1; j < e.Args.Length; j++)
                    {
                        string next = e.Args[j].Trim('"', '\'');
                        parts.Add(next);
                        string candidate = string.Join(' ', parts);
                        if (File.Exists(candidate))
                        {
                            startupFilePath = candidate;
                            SimpleLogger.Log($"[APP] File detected (combined): {startupFilePath}");
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(startupFilePath))
                        break;
                }
            }

            if (string.IsNullOrEmpty(startupFilePath))
                SimpleLogger.Log("[APP] No startup file detected");
            try
            {
                LicenseHelper.RegisterSyncfusionLicense();
                _processHelper.LaunchKannadaKeyboard();

                var banner = new BannerWindow();
                banner.Show();
                await Dispatcher.Yield(DispatcherPriority.Render);

                await Task.Run(EditorInitializer.Initialize);

                // Pass startup file directly to MainWindow after full initialization
                var mainWindow = new MainWindow(startupFilePath);
                mainWindow.Show();

                AnimationHelper.FadeOutAndClose(banner, 600);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Startup exception: " + ex);
                KillKeyboardProcess();
                ErrorHelper.ShowError("Unexpected error during startup", ex);
                Shutdown();
            }
        }

        // Make this method public so windows can call it
        public void KillKeyboardProcess()
        {
            _processHelper.KillKeyboardProcess();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            KillKeyboardProcess(); // Ensure keyboard is killed on exit
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SimpleLogger.Log("DispatcherUnhandledException: " + e.Exception);

            // Ignore clipboard corruption completely
            if (e.Exception is COMException comEx &&
                (uint)comEx.HResult == 0x800401D3)
            {
                SimpleLogger.Log("Handled clipboard bad data at App level.");
                SafeClipboard.ClearSafely();
                e.Handled = true;
                return;
            }

            // Real UI crash
            KillKeyboardProcess();
            ErrorHelper.ShowError("A UI thread error occurred", e.Exception);
            e.Handled = true;
        }



        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                SimpleLogger.Log("UnhandledException: " + ex);
                KillKeyboardProcess();
                ErrorHelper.ShowError("A background thread error occurred", ex);
            }
            else
            {
                SimpleLogger.Log("UnhandledException: Unknown exception");
                KillKeyboardProcess();
                ErrorHelper.ShowError("A fatal unknown error occurred", null);
            }
        }
    }
}
