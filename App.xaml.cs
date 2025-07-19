using System;
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
            // Global UI thread exception handler
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Global non-UI thread exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXhfeXRTRGVfWEZzXktWYEk=");

                base.OnStartup(e);

                LaunchKannadaKeyboard();

                var banner = new BannerWindow();
                banner.Show();
            }
            catch (Exception ex)
            {
                ShowError("Unexpected error during application startup.", ex);
                Shutdown(); // Ensure app doesn't remain running in corrupted state
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

        private void ShowError(string title, Exception ex)
        {
            string message = $"{title}\n\n{ex.Message}\n\nDetails:\n{ex.StackTrace}";
            MessageBox.Show(message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
