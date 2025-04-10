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
        private Process _kannadaKeyboardProcess;

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
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFileName);
            bool isAlreadyRunning = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeFileName)).Length > 0;

            if (!isAlreadyRunning && File.Exists(exePath))
            {
                try
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
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to start kannadaKeyboard.exe:\n" + ex.Message);
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_kannadaKeyboardProcess != null && !_kannadaKeyboardProcess.HasExited)
            {
                try { _kannadaKeyboardProcess.Kill(); }
                catch (Exception ex) { Debug.WriteLine("Error stopping keyboard: " + ex.Message); }
            }
        }
    }
}
