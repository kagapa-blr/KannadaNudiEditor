using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            // Capture file path from "Open With" command line args
            string? startupFilePath = null;

            SimpleLogger.Log($"[APP] Total args received: {e.Args.Length}");

            // Try to find a valid file path among the arguments
            // Handle both quoted and unquoted paths, and paths split across multiple args
            for (int i = 0; i < e.Args.Length; i++)
            {
                string arg = e.Args[i];
                SimpleLogger.Log($"[APP] Arg[{i}]: '{arg}'");

                // Remove quotes if present
                string cleanedArg = arg.Trim('"', '\'');
                SimpleLogger.Log($"[APP] Cleaned arg[{i}]: '{cleanedArg}'");

                // Check if this single arg is a valid file
                if (!string.IsNullOrWhiteSpace(cleanedArg) && File.Exists(cleanedArg))
                {
                    startupFilePath = cleanedArg;
                    SimpleLogger.Log($"[APP] ✓ Startup file detected (single arg): {startupFilePath}");
                    break;
                }

                // If not found as a single arg, try combining this arg with subsequent args
                // (some shells / run helpers may split a quoted path into multiple arguments).
                if (!string.IsNullOrWhiteSpace(cleanedArg))
                {
                    // Heuristic: treat this arg as a path start if it contains a backslash (drive or folder)
                    if (cleanedArg.Contains("\\"))
                    {
                        var parts = new List<string> { cleanedArg };
                        for (int j = i + 1; j < e.Args.Length; j++)
                        {
                            string next = e.Args[j].Trim('"', '\'');
                            parts.Add(next);
                            string candidate = string.Join(' ', parts);
                            SimpleLogger.Log($"[APP] Trying combined candidate: '{candidate}'");
                            if (File.Exists(candidate))
                            {
                                startupFilePath = candidate;
                                SimpleLogger.Log($"[APP] ✓ Startup file detected (combined args): {startupFilePath}");
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(startupFilePath))
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(cleanedArg))
                {
                    SimpleLogger.Log($"[APP] ✗ File not found: {cleanedArg}");
                }
            }

            if (string.IsNullOrEmpty(startupFilePath))
            {
                SimpleLogger.Log("[APP] No startup file detected (normal launch)");
            }
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
