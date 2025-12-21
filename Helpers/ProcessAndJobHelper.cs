using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace KannadaNudiEditor.Helpers
{
    public class ProcessHelper
    {
        private Process? _kannadaKeyboardProcess;
        private Job? _keyboardJob;

        public void LaunchKannadaKeyboard()
        {
            try
            {
                const string exeFile = "kannadaKeyboard";
                string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFile + ".exe");

                if (!File.Exists(exePath))
                {
                    SimpleLogger.Log("kannadaKeyboard.exe not found.");
                    return;
                }

                if (Process.GetProcessesByName(exeFile).Length > 0) return;

                _kannadaKeyboardProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                _keyboardJob = new Job();
                _keyboardJob.AddProcess(_kannadaKeyboardProcess);

                SimpleLogger.Log("kannadaKeyboard.exe started.");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Failed to start kannadaKeyboard.exe: " + ex);
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
                }

                foreach (var p in Process.GetProcessesByName("kannadaKeyboard"))
                {
                    try { p.Kill(); p.WaitForExit(500); } catch { }
                }

                _keyboardJob?.Dispose();
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Error killing keyboard process: " + ex);
            }
        }
    }

    public class Job : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, int infoType, ref JOBOBJECT_EXTENDED_LIMIT_INFORMATION lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

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
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount, WriteOperationCount, OtherOperationCount,
                          ReadTransferCount, WriteTransferCount, OtherTransferCount;
        }

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
    }
}
