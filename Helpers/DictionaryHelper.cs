using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public static class DictionaryHelper
{
    public static string AppDataBasePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KannadaNudiBaraha");

    private static readonly HttpClient http = new HttpClient();

    // -----------------------------
    // Existing method (DO NOT change)
    // -----------------------------
    public static string GetWritableDictionaryPath(string fileName)
    {
        try
        {
            if (!Directory.Exists(AppDataBasePath))
            {
                Directory.CreateDirectory(AppDataBasePath);
                SimpleLogger.Log($"Created AppData directory: {AppDataBasePath}");
            }

            string appDataFile = Path.Combine(AppDataBasePath, fileName);
            string installedFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);

            SimpleLogger.Log($"Requested dictionary: {fileName}");
            SimpleLogger.Log($"Installed source: {installedFile}");
            SimpleLogger.Log($"AppData target: {appDataFile}");

            if (!File.Exists(appDataFile))
            {
                if (File.Exists(installedFile))
                {
                    File.Copy(installedFile, appDataFile);
                    SimpleLogger.Log($"Copied dictionary to AppData: {fileName}");
                }
                else
                {
                    SimpleLogger.Log($"ERROR: Installed dictionary missing: {installedFile}");
                }
            }

            return appDataFile;
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"EXCEPTION in GetWritableDictionaryPath({fileName}): {ex}");
            throw;
        }
    }

    // ======================================================
    // ⭐ NEW METHOD 1: Read all words from custom dictionary
    // ======================================================
    public static List<string> LoadCustomDictionaryWords()
    {
        try
        {
            string path = GetWritableDictionaryPath("Custom_MyDictionary_kn_IN.dic");

            if (!File.Exists(path))
            {
                SimpleLogger.Log($"ERROR: Custom dictionary file missing at: {path}");
                return new List<string>();
            }

            var words = new List<string>();

            foreach (var line in File.ReadAllLines(path))
            {
                string word = line.Trim();
                if (!string.IsNullOrWhiteSpace(word))
                    words.Add(word);
            }

            SimpleLogger.Log($"Loaded {words.Count} custom words.");
            return words;
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"EXCEPTION in LoadCustomDictionaryWords: {ex}");
            return new List<string>();
        }
    }

    // ======================================================
    // ⭐ NEW METHOD 2: Upload dictionary words to your API
    // ======================================================
    public static async Task<bool> UploadDictionaryWordsAsync(string apiUrl)
    {
        try
        {
            List<string> words = LoadCustomDictionaryWords();

            if (words.Count == 0)
            {
                SimpleLogger.Log("Upload skipped — No words found.");
                return false;
            }

            SimpleLogger.Log($"Uploading {words.Count} words to API: {apiUrl}");

            var payload = new
            {
                dictionary_name = "Custom_MyDictionary_kn_IN",
                words = words
            };

            HttpResponseMessage response = await http.PostAsJsonAsync(apiUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                SimpleLogger.Log("Dictionary upload successful.");
                return true;
            }
            else
            {
                SimpleLogger.Log($"API returned error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return false;
            }
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"EXCEPTION in UploadDictionaryWordsAsync: {ex}");
            return false;
        }
    }
}
