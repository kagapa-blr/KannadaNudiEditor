using System;
using System.IO;

public static class SimpleLogger
{
    private static readonly string BaseLogFolder = GetRootLogFolder();
    private static readonly string DateFolder = Path.Combine(BaseLogFolder, DateTime.Now.ToString("dd-MM-yyyy"));
    private static string LogFilePath;

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

    private static string GetRootLogFolder()
    {
        try
        {
            var rootDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)
                            ?.Parent?.Parent?.Parent?.FullName;

            if (string.IsNullOrWhiteSpace(rootDir))
                throw new Exception("Could not resolve root folder");

            return Path.Combine(rootDir, "Logs");
        }
        catch
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        }
    }
}
