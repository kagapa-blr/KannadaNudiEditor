using System;
using System.IO;
using Syncfusion.DocIO.DLS;

public static class SimpleLogger
{
    private static readonly string BaseLogFolder = GetRootLogFolder();
    private static readonly string DateFolder = Path.Combine(BaseLogFolder, DateTime.Now.ToString("dd-MM-yyyy"));
    private static readonly string LogFilePath;

    static SimpleLogger()
    {
        try
        {
            // Ensure date folder exists
            Directory.CreateDirectory(DateFolder);

            // Build log file name with timestamp
            string timestamp = DateTime.Now.ToString("HHmmss");
            string logFileName = $"Nudilog_{timestamp}_.log";
            LogFilePath = Path.Combine(DateFolder, logFileName);

            Log("=== Application Started ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Logger init failed: " + ex.Message);
        }
    }

    public static void Log(string message)
    {
        try
        {
            string logMessage = $"{DateTime.Now:HH:mm:ss} | {message}";
            File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
        }
        catch
        {
            // Silently ignore
        }
    }

    public static void LogException(Exception ex, string context = "")
    {
        Log($"[ERROR] {context} {ex.Message}\n{ex.StackTrace}");
    }

    internal static void Log(IWParagraph para)
    {
        throw new NotImplementedException();
    }


    private static string GetRootLogFolder()
    {
        try
        {
            // Always create Logs folder inside the current running folder
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string logPath = Path.Combine(basePath, "Logs");
            return logPath;
        }
        catch
        {
            return Path.Combine(Environment.CurrentDirectory, "Logs");
        }
    }

}
