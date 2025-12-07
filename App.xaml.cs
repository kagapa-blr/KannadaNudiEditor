using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using KannadaNudiEditor.Views;
using Syncfusion.Licensing;

namespace KannadaNudiEditor
{
    public partial class App : Application
    {
        private Process? _kannadaKeyboardProcess;

        public App()
        {
            SimpleLogger.Log("App startup initialized.");

            // Global UI exception handler
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Non-UI exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Register Syncfusion license first
                SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1ed3VXRmNfWUJ3WUpWYEg=");

                SimpleLogger.Log("Syncfusion license applied.");

                // Start Kannada keyboard executable
                LaunchKannadaKeyboard();

                // Show splash / banner
                var banner = new BannerWindow();
                banner.Show();

                // Allow UI to render immediately
                await Dispatcher.Yield(DispatcherPriority.Render);

                // Background initialization
                await Task.Run(() => InitializeEditor());

                // Launch main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
                SimpleLogger.Log("Main window launched.");

                // Fade out splash
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(
                    1, 0, new Duration(TimeSpan.FromMilliseconds(600))
                );
                fadeOut.Completed += (s, a) => banner.Close();
                banner.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error during application startup.", ex);
                Shutdown();
            }
        }

        private void LaunchKannadaKeyboard()
        {
            try
            {
                const string exeFile = "kannadaKeyboard";

                // Prevent duplicate instances
                if (Process.GetProcessesByName(exeFile).Length > 0)
                {
                    SimpleLogger.Log("kannadaKeyboard.exe already running. Skip launch.");
                    return;
                }

                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFile + ".exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show("kannadaKeyboard.exe not found in Assets folder.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _kannadaKeyboardProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };

                _kannadaKeyboardProcess.Start();
                SimpleLogger.Log("kannadaKeyboard.exe started.");
            }
            catch (Exception ex)
            {
                ShowError("Failed to start kannadaKeyboard.exe", ex);
            }
        }

        private static void InitializeEditor()
        {
            // Heavy initialization logic placeholder
            Thread.Sleep(2000);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                // 1. Close the tracked process if running
                if (_kannadaKeyboardProcess != null)
                {
                    if (!_kannadaKeyboardProcess.HasExited)
                    {
                        _kannadaKeyboardProcess.Kill();
                        _kannadaKeyboardProcess.WaitForExit(1000);
                    }
                }

                // 2. As a fallback, kill all running nudi engine processes
                foreach (var p in Process.GetProcessesByName("kannadaKeyboard"))
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(500);
                    }
                    catch { }
                }

                SimpleLogger.Log("kannadaKeyboard.exe terminated on app exit.");
            }
            catch (Exception ex)
            {
                ShowError("Error stopping kannadaKeyboard.exe on exit", ex);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError("A UI thread error occurred", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ShowError("A background thread error occurred", ex);
            else
                MessageBox.Show("An unknown fatal error occurred.", "Fatal Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void ShowError(string title, Exception ex)
        {
            string message = $"{title}\n\n{ex.Message}\n\nDetails:\n{ex.StackTrace}";
            MessageBox.Show(message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
