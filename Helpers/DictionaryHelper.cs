using System.IO;

public static class DictionaryHelper
{
    public static string AppDataBasePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KannadaNudiBaraha");


    public static string GetWritableDictionaryPath(string fileName)
    {
        if (!Directory.Exists(AppDataBasePath))
        {
            _ = Directory.CreateDirectory(AppDataBasePath);
        }

        string appDataFile = Path.Combine(AppDataBasePath, fileName);
        string installedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);

        // Copy only once
        if (!File.Exists(appDataFile) && File.Exists(installedFile))
        {
            File.Copy(installedFile, appDataFile);
        }

        return appDataFile;
    }
}
