using System.IO;
using System.Net.Http;
using System.Net.Http.Json;

public static class DictionaryHelper
{
    // ======================================================
    // Configuration
    // ======================================================

    public static readonly string AppDataBasePath =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "KannadaNudiBaraha"
        );

    private static readonly HttpClient http = new HttpClient();

    // ======================================================
    // Core Utility (DO NOT CHANGE)
    // ======================================================

    public static string GetWritableDictionaryPath(string fileName)
    {
        try
        {
            if (!Directory.Exists(AppDataBasePath))
            {
                Directory.CreateDirectory(AppDataBasePath);
                SimpleLogger.Log($"[Dictionary] AppData directory created: {AppDataBasePath}");
            }

            string appDataFile = Path.Combine(AppDataBasePath, fileName);
            string installedFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                fileName
            );

            SimpleLogger.Log($"[Dictionary] Requested: {fileName}");
            SimpleLogger.Log($"[Dictionary] Asset source: {installedFile}");
            SimpleLogger.Log($"[Dictionary] AppData target: {appDataFile}");

            if (!File.Exists(appDataFile))
            {
                if (File.Exists(installedFile))
                {
                    File.Copy(installedFile, appDataFile);
                    SimpleLogger.Log($"[Dictionary] Copied to AppData: {fileName}");
                }
                else
                {
                    SimpleLogger.Log($"[Dictionary][ERROR] Asset dictionary missing: {installedFile}");
                }
            }

            return appDataFile;
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"[Dictionary][EXCEPTION] GetWritableDictionaryPath({fileName}): {ex}");
            throw;
        }
    }

    // ======================================================
    // Custom Dictionary Reader
    // ======================================================

    public static List<string> LoadCustomDictionaryWords()
    {
        try
        {
            string customPath = GetWritableDictionaryPath("KannadaNudiBaraha_Kn_IN.dic");

            if (!File.Exists(customPath))
            {
                SimpleLogger.Log($"[Dictionary][ERROR] Custom dictionary missing: {customPath}");
                return new List<string>();
            }

            var words = File.ReadAllLines(customPath)
                            .Select(w => w.Trim())
                            .Where(w => !string.IsNullOrWhiteSpace(w))
                            .ToList();

            SimpleLogger.Log($"[Dictionary] Loaded {words.Count} words from KannadaNudiBaraha dictionary");
            return words;
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"[Dictionary][EXCEPTION] LoadCustomDictionaryWords: {ex}");
            return new List<string>();
        }
    }

    // ======================================================
    // API Upload (Custom Dictionary Only)
    // ======================================================

    public static async Task<bool> UploadDictionaryWordsAsync(string apiUrl)
    {
        try
        {
            List<string> words = LoadCustomDictionaryWords();

            if (words.Count == 0)
            {
                SimpleLogger.Log("[Dictionary] Upload skipped. No custom words found.");
                return false;
            }

            SimpleLogger.Log($"[Dictionary] Uploading {words.Count} words to API");
            SimpleLogger.Log($"[Dictionary] API Endpoint: {apiUrl}");

            var payload = new
            {
                dictionary_name = "KannadaNudiBaraha_Kn_IN",
                words = words
            };

            HttpResponseMessage response =
                await http.PostAsJsonAsync(apiUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                SimpleLogger.Log("[Dictionary] Dictionary upload completed successfully");
                return true;
            }

            SimpleLogger.Log(
                $"[Dictionary][ERROR] API response: {response.StatusCode} | {await response.Content.ReadAsStringAsync()}"
            );

            return false;
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"[Dictionary][EXCEPTION] UploadDictionaryWordsAsync: {ex}");
            return false;
        }
    }

    // ======================================================
    // Sync Custom → Standard Dictionary
    // Rules:
    // - kn_IN.dic contains ONLY unique words
    // - Case-insensitive uniqueness
    // - https://kagapa.com/tools/api/v1/dictionary/user/add   -> upload the words from custom dictionary 
    // -if successfull uploaded then only clean the custom dictionary 
    // - Custom dictionary is cleared after sync

    // ======================================================





    public static bool SyncCustomToStandardDictionary()
    {
        try
        {
            string standardPath = GetWritableDictionaryPath("kn_IN.dic");
            string customPath = GetWritableDictionaryPath("KannadaNudiBaraha_Kn_IN.dic");

            if (!File.Exists(standardPath))
            {
                SimpleLogger.Log($"[Dictionary][ERROR] Standard dictionary missing: {standardPath}");
                return false;
            }

            if (!File.Exists(customPath))
            {
                SimpleLogger.Log($"[Dictionary][ERROR] Custom dictionary missing: {customPath}");
                return false;
            }

            // --------------------------------------------------
            // STEP 1: Read custom words
            // --------------------------------------------------

            var customWords = File.ReadAllLines(customPath)
                                  .Select(w => w.Trim())
                                  .Where(w => !string.IsNullOrWhiteSpace(w))
                                  .Distinct(StringComparer.OrdinalIgnoreCase)
                                  .ToList();

            if (customWords.Count == 0)
            {
                SimpleLogger.Log("[Dictionary] No custom words to sync");
                return true;
            }

            SimpleLogger.Log($"[Dictionary] Custom words found: {customWords.Count}");

            // --------------------------------------------------
            // STEP 2: Sync into standard dictionary (UNIQUE)
            // --------------------------------------------------

            var standardWords = new HashSet<string>(
                File.ReadAllLines(standardPath)
                    .Select(w => w.Trim())
                    .Where(w => !string.IsNullOrWhiteSpace(w)),
                StringComparer.OrdinalIgnoreCase
            );

            int mergedCount = 0;

            foreach (var word in customWords)
            {
                if (standardWords.Add(word))
                {
                    mergedCount++;
                }
            }

            var finalStandardList = standardWords
                .OrderBy(w => w, StringComparer.OrdinalIgnoreCase)
                .ToList();

            File.WriteAllLines(standardPath, finalStandardList);

            SimpleLogger.Log($"[Dictionary] Synced to standard dictionary");
            SimpleLogger.Log($"[Dictionary] New words merged: {mergedCount}");
            SimpleLogger.Log($"[Dictionary] Total unique words in kn_IN.dic: {finalStandardList.Count}");

            // --------------------------------------------------
            // STEP 3: Upload custom words to API
            // --------------------------------------------------

            const string apiUrl = "https://kagapa.com/tools/api/v1/dictionary/user/add";

            SimpleLogger.Log($"[Dictionary] Uploading {customWords.Count} words to API");
            SimpleLogger.Log($"[Dictionary] API Endpoint: {apiUrl}");

            var payload = new
            {
                words = customWords
            };

            HttpResponseMessage response =
                http.PostAsJsonAsync(apiUrl, payload).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                SimpleLogger.Log(
                    $"[Dictionary][ERROR] Upload failed: {response.StatusCode} | {response.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"
                );

                // IMPORTANT: Do NOT clean up custom dictionary
                return false;
            }

            SimpleLogger.Log("[Dictionary] Upload successful");

            // --------------------------------------------------
            // STEP 4: Cleanup custom dictionary (ONLY after upload)
            // --------------------------------------------------

            File.WriteAllText(customPath, string.Empty);

            string lastWord = finalStandardList.Count > 0
                ? finalStandardList[^1]
                : "<EMPTY>";

            SimpleLogger.Log("[Dictionary] Custom dictionary cleaned");
            SimpleLogger.Log($"[Dictionary] Last word in kn_IN.dic: {lastWord}");
            SimpleLogger.Log("[Dictionary] Sync process completed successfully");

            return true;
        }
        catch (Exception ex)
        {
            SimpleLogger.Log($"[Dictionary][EXCEPTION] SyncCustomToStandardDictionary: {ex}");
            return false;
        }
    }







}
