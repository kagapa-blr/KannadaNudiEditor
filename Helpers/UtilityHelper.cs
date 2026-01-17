using System.Windows;
using System.Windows.Media.Animation;
using Syncfusion.Licensing;

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
            string message = ex == null ? title : $"{title}\n{ex.Message}\n\n{ex.StackTrace}";
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



}
