using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KannadaNudiEditor.Views;
using Syncfusion.Licensing;

namespace KannadaNudiEditor
{
    public partial class App : Application
    {
        private Process? _kannadaKeyboardProcess;
        private Job? _keyboardJob;

        public App()
        {
            SimpleLogger.Log("=== Application Started ===");

            // Global UI exception handler
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // Non-UI exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // First chance exception logging
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                SimpleLogger.Log("FirstChanceException: " + e.Exception.Message);
            };
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 1. Register Syncfusion license
                SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH1ccHRSR2hfWUJ2WURWYEs=");
                SimpleLogger.Log("Syncfusion license applied.");

                // 2. Launch Kannada keyboard
                LaunchKannadaKeyboard();

                // 3. Show splash / banner
                var banner = new BannerWindow();
                banner.Show();

                // Render UI immediately
                await Dispatcher.Yield(DispatcherPriority.Render);

                // 4. Background initialization
                await Task.Run(() => InitializeEditor());

                // 5. Launch main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
                SimpleLogger.Log("Main window launched.");

                // 6. Fade out splash
                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(
                    1, 0, new Duration(TimeSpan.FromMilliseconds(600))
                );
                fadeOut.Completed += (s, a) => banner.Close();
                banner.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Startup exception: " + ex.Message + "\n" + ex.StackTrace);
                KillKeyboardProcess();
                ShowError("Unexpected error during application startup.", ex);
                Shutdown();
            }
        }

        private void LaunchKannadaKeyboard()
        {
            try
            {
                const string exeFile = "kannadaKeyboard";
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFile + ".exe");

                if (!File.Exists(exePath))
                {
                    SimpleLogger.Log("kannadaKeyboard.exe not found in Assets folder.");
                    MessageBox.Show("kannadaKeyboard.exe not found in Assets folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Prevent duplicate instances
                if (Process.GetProcessesByName(exeFile).Length > 0)
                {
                    SimpleLogger.Log("kannadaKeyboard.exe already running. Skipping launch.");
                    return;
                }

                _kannadaKeyboardProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                _kannadaKeyboardProcess.Start();

                // Use Job object to terminate the keyboard if the app crashes
                _keyboardJob = new Job();
                _keyboardJob.AddProcess(_kannadaKeyboardProcess);

                SimpleLogger.Log("kannadaKeyboard.exe started with Job Object.");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Failed to start kannadaKeyboard.exe: " + ex.Message);
                ShowError("Failed to start kannadaKeyboard.exe", ex);
            }
        }

        private static void InitializeEditor()
        {
            // Heavy initialization placeholder
            Thread.Sleep(2000);
            SimpleLogger.Log("Editor initialized in background thread.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                KillKeyboardProcess();
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Error during OnExit: " + ex.Message);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SimpleLogger.Log("DispatcherUnhandledException: " + e.Exception.Message + "\n" + e.Exception.StackTrace);
            KillKeyboardProcess();
            ShowError("A UI thread error occurred", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                SimpleLogger.Log("UnhandledException: " + ex.Message + "\n" + ex.StackTrace);
                KillKeyboardProcess();
                ShowError("A background thread error occurred", ex);
            }
            else
            {
                SimpleLogger.Log("UnhandledException: Unknown exception object");
                KillKeyboardProcess();
                MessageBox.Show("An unknown fatal error occurred.", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void KillKeyboardProcess()
        {
            try
            {
                if (_kannadaKeyboardProcess != null && !_kannadaKeyboardProcess.HasExited)
                {
                    _kannadaKeyboardProcess.Kill();
                    _kannadaKeyboardProcess.WaitForExit(500);
                    SimpleLogger.Log("kannadaKeyboard.exe terminated via tracked process.");
                }

                // fallback for safety
                foreach (var p in Process.GetProcessesByName("kannadaKeyboard"))
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(500);
                        SimpleLogger.Log("kannadaKeyboard.exe terminated via fallback.");
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.Log("Failed to kill kannadaKeyboard.exe process: " + ex.Message);
                    }
                }

                _keyboardJob?.Dispose();
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Error in KillKeyboardProcess: " + ex.Message);
            }
        }

        private static void ShowError(string title, Exception ex)
        {
            string message = $"{title}\n\n{ex.Message}\n\nDetails:\n{ex.StackTrace}";
            SimpleLogger.Log($"Showing error message: {message}");
            MessageBox.Show(message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Job Object for killing child processes if parent dies
        public class Job : IDisposable
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

            [DllImport("kernel32.dll")]
            private static extern bool SetInformationJobObject(IntPtr hJob, int infoType, ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, uint cbJobObjectInfoLength);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

            private IntPtr _handle;
            private bool _disposed = false;

            [StructLayout(LayoutKind.Sequential)]
            private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                public long PerProcessUserTimeLimit;
                public long PerJobUserTimeLimit;
                public int LimitFlags;
                public UIntPtr MinimumWorkingSetSize;
                public UIntPtr MaximumWorkingSetSize;
                public int ActiveProcessLimit;
                public long Affinity;
                public int PriorityClass;
                public int SchedulingClass;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct IO_COUNTERS { public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount, ReadTransferCount, WriteTransferCount, OtherTransferCount; }

            [StructLayout(LayoutKind.Sequential)]
            private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
                public IO_COUNTERS IoInfo;
                public UIntPtr ProcessMemoryLimit;
                public UIntPtr JobMemoryLimit;
                public UIntPtr PeakProcessMemoryUsed;
                public UIntPtr PeakJobMemoryUsed;
            }

            private const int JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;
            private const int JobObjectExtendedLimitInformation = 9;

            public Job()
            {
                _handle = CreateJobObject(IntPtr.Zero, null);

                JOBOBJECT_EXTENDED_LIMIT_INFORMATION info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

                SetInformationJobObject(_handle, JobObjectExtendedLimitInformation, ref info, (uint)Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION)));
            }

            public void AddProcess(Process process)
            {
                AssignProcessToJobObject(_handle, process.Handle);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    if (_handle != IntPtr.Zero)
                    {
                        CloseHandle(_handle);
                        _handle = IntPtr.Zero;
                    }
                }
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(IntPtr hObject);
        }
        #endregion
    }
}
