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
            SimpleLogger.Log("App startup complete");

            // Global UI thread exception handler
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Global non-UI thread exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }



        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Register Syncfusion license
                SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH5fdHZWRGNfVkVwW0RWYEg=");
                SimpleLogger.Log("Application starting...");

                base.OnStartup(e);

                // Launch Kannada Keyboard
                LaunchKannadaKeyboard();

                // Show banner
                var banner = new BannerWindow();
                banner.Show();

                // Yield to render banner immediately
                await Dispatcher.Yield(DispatcherPriority.Render);

                // Initialize editor on background thread
                await Task.Run(() => InitializeEditor());

                // Once initialization is done, show main window
                var main = new MainWindow();
                main.Show();
                SimpleLogger.Log("Nudi Editor main window loaded successfully.");

                // Fade out banner now that main window is loaded
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(500)));
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
                string exeFileName = "kannadaKeyboard.exe";
                bool isAlreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeFileName)).Length > 0;

                if (!isAlreadyRunning)
                {
                    string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFileName);

                    if (File.Exists(exePath))
                    {
                        _kannadaKeyboardProcess = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = exePath,
                                CreateNoWindow = true,
                                UseShellExecute = false
                            }
                        };
                        _kannadaKeyboardProcess.Start();
                    }
                    else
                    {
                        MessageBox.Show("kannadaKeyboard.exe not found at the specified path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The Nudi Engine is already running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to start kannadaKeyboard.exe", ex);
            }
        }

        private static void InitializeEditor()
        {
            // Simulate heavy editor initialization
            // Replace this with actual editor loading logic (fonts, dictionaries, plugins, etc.)
            Thread.Sleep(3000);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                if (_kannadaKeyboardProcess != null && !_kannadaKeyboardProcess.HasExited)
                {
                    _kannadaKeyboardProcess.Kill();
                    _kannadaKeyboardProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                ShowError("Error stopping the Nudi Engine on exit", ex);
            }
        }

        // Global UI thread exception
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowError("A UI error occurred", e.Exception);
            e.Handled = true;
        }

        // Global non-UI thread exception
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                ShowError("An unexpected error occurred", ex);
            else
                MessageBox.Show("An unknown error occurred.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private static void ShowError(string title, Exception ex)
        {
            string message = $"{title}\n\n{ex.Message}\n\nDetails:\n{ex.StackTrace}";
            MessageBox.Show(message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
