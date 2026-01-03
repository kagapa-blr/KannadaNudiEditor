using System;

namespace KannadaNudiEditor.Helpers
{
    public static class SimpleLogger
    {
        public static void Log(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public static void LogException(Exception ex, string message)
        {
            Console.WriteLine($"[ERROR] {message}: {ex.Message}");
        }
    }
}
