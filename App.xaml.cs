using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Syncfusion.Licensing;

namespace Document_Editor_.NET_8
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Process _kannadaKeyboardProcess;


        protected override void OnStartup(StartupEventArgs e)
        {
            // Register Syncfusion license
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NNaF5cXmZCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtedXRVQmNYUUN/XUdWYUA=");

            base.OnStartup(e);

            // Path to kannadaKeyboard.exe in the output directory (relative)
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "kannadaKeyboard.exe");

            if (File.Exists(exePath))
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
                    MessageBox.Show("Failed to start kannadaKeyboard.exe:\n" + ex.Message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("kannadaKeyboard.exe not found in Assets folder.\nExpected at: " + exePath, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_kannadaKeyboardProcess != null && !_kannadaKeyboardProcess.HasExited)
            {
                try
                {
                    _kannadaKeyboardProcess.Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error stopping kannadaKeyboard.exe: " + ex.Message);
                }
            }
        }
    }
}
