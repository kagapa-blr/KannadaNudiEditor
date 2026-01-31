using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Animation;
using Syncfusion.Licensing;
using System;
using System.Threading;

namespace KannadaNudiEditor.Helpers
{
    public static class LicenseHelper
    {
        public static void RegisterSyncfusionLicense()
        {
            SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JGaF5cXGpCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWX5fdnZWQmdeWUx+X0NWYEs=");

            SimpleLogger.Log("Syncfusion license applied.");
        }
    }

    public static class EditorInitializer
    {
        public static void Initialize()
        {
            Thread.Sleep(2000); // Simulate heavy initialization
            SimpleLogger.Log("Editor initialized in background thread.");
        }
    }

    public static class AnimationHelper
    {
        public static void FadeOutAndClose(Window window, int durationMs)
        {
            var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(durationMs)));
            fadeOut.Completed += (s, e) => window.Close();
            window.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    public static class ErrorHelper
    {
        public static void ShowError(string title, Exception? ex)
        {
            // Suppress clipboard corruption errors
            if (ex is COMException comEx &&
                (uint)comEx.HResult == 0x800401D3)
            {
                SimpleLogger.Log("Suppressed clipboard bad data error dialog.");
                SafeClipboard.ClearSafely();
                return;
            }

            string message = ex == null
                ? title
                : $"{title}\n{ex.Message}\n\n{ex.StackTrace}";

            SimpleLogger.Log("Showing error: " + message);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }


    public static class TimeHelper
    {
        // Formats elapsed time like "30 mins 10 seconds"
        public static string FormatElapsed(TimeSpan t)
        {
            if (t.TotalHours >= 1)
                return $"{(int)t.TotalHours} hours {t.Minutes} mins {t.Seconds} seconds";

            return $"{t.Minutes} mins {t.Seconds} seconds";
        }
    }



    public static class SafeClipboard
    {
        private const uint CLIPBRD_E_BAD_DATA = 0x800401D3;

        public static bool TryGetFileDropList(out StringCollection files)
        {
            files = new StringCollection();

            try
            {
                if (!Clipboard.ContainsFileDropList())
                    return false;

                files = Clipboard.GetFileDropList();
                return files != null && files.Count > 0;
            }
            catch (COMException ex) when ((uint)ex.HResult == CLIPBRD_E_BAD_DATA)
            {
                SimpleLogger.Log("Clipboard contains bad data. Ignoring safely.");
                return false;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Clipboard access failed: " + ex);
                return false;
            }
        }

        public static void ClearSafely()
        {
            try
            {
                Clipboard.Clear();
            }
            catch
            {
                // Clipboard may be locked by another process – ignore
                SimpleLogger.Log("Failed to clear clipboard, possibly locked by another process.");
            }
        }

        public static void ExecuteWithRetry(Action action, int retries = 3)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    action();
                    return;
                }
                catch (COMException ex) when ((uint)ex.HResult == CLIPBRD_E_BAD_DATA)
                {
                    SimpleLogger.Log($"Clipboard retry {i + 1} failed.");
                    Thread.Sleep(50);
                }
            }

            SimpleLogger.Log("Clipboard retries exhausted.");
        }




    }


}
