using System;
using System.Diagnostics;
using System.IO;
using Syncfusion.DocIO.DLS;

public static class SimpleLogger
{
    private static readonly object _lock = new();
    private static readonly string LogFilePath;

    static SimpleLogger()
    {
        try
        {
            string baseLogFolder = GetRootLogFolder();
            string dateFolder = Path.Combine(baseLogFolder, DateTime.Now.ToString("dd-MM-yyyy"));
            Directory.CreateDirectory(dateFolder);

            string timestamp = DateTime.Now.ToString("HHmmss");
            string logFileName = $"NudiBaraha_{timestamp}.log";

            LogFilePath = Path.Combine(dateFolder, logFileName);
        }
        catch (Exception ex)
        {
            // Absolute fallback
            string fallback = Path.Combine(Environment.CurrentDirectory, "NudiBaraha_fallback.log");
            LogFilePath = fallback;

            Debug.WriteLine("Logger init failed: " + ex);
        }
    }

    public static void Log(string message)
    {
        try
        {
            string logMessage = $"{DateTime.Now:HH:mm:ss} | {message}";
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Logging failed: " + ex.Message);
        }
    }

    public static void LogException(Exception ex, string context = "")
    {
        if (ex == null)
        {
            Log($"[ERROR] {context} (Exception is null)");
            return;
        }

        Log($"[ERROR] {context}\n{ex.Message}\n{ex.StackTrace}");
    }

    [Obsolete("Use Log(string) instead.")]
    internal static void Log(IWParagraph para)
    {
        if (para != null)
            Log($"IWParagraph: {para.Text}");
    }

    private static string GetRootLogFolder()
    {
        try
        {
            string appName = "KannadaNudiBaraha";

            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "Logs"
            );

            Directory.CreateDirectory(basePath);
            return basePath;
        }
        catch
        {
            string fallback = Path.Combine(Environment.CurrentDirectory, "Logs");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }
}
