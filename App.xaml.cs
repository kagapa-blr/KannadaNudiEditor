using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using KannadaNudiEditor.Views;
using Syncfusion.Licensing;

namespace KannadaNudiEditor
{
    public partial class App : Application
    {
        private Process? _kannadaKeyboardProcess;

        protected override void OnStartup(StartupEventArgs e)
        {
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtedXRVQmNYUUN/XUdWYUA=");

            base.OnStartup(e);

            LaunchKannadaKeyboard();

            var banner = new BannerWindow();
            banner.Show();
        }

        private void LaunchKannadaKeyboard()
        {
            string exeFileName = "kannadaKeyboard.exe";
            // Check if the process is already running
            bool isAlreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeFileName)).Length > 0;

            if (!isAlreadyRunning)
            {
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFileName);

                if (File.Exists(exePath))
                {
                    try
                    {
                        // Start the process if it's not already running
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
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to start kannadaKeyboard.exe:\n" + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("kannadaKeyboard.exe not found at the specified path.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Optionally show a message if it's already running
                MessageBox.Show("The Nudi Engine is already running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Ensure the Nudi engine is killed upon app exit
            base.OnExit(e);

            if (_kannadaKeyboardProcess != null && !_kannadaKeyboardProcess.HasExited)
            {
                try
                {
                    // Kill the process if it is running
                    _kannadaKeyboardProcess.Kill();
                    _kannadaKeyboardProcess.WaitForExit();
                    //MessageBox.Show("Nudi Engine has been stopped.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Log any error that occurs when trying to kill the process
                    Debug.WriteLine("Error stopping keyboard: " + ex.Message);
                    MessageBox.Show("Error stopping the Nudi Engine: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
