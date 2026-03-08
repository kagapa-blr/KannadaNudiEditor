using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class CustomMappingsHelper
    {
        private static readonly string CustomMappingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"KannadaNudiBaraha");

        private static readonly string CustomMappingsFilePath = Path.Combine(
            CustomMappingsDirectory,
            "custom_mappings.json");

        /// <summary>
        /// Loads custom mappings from the JSON file.
        /// </summary>
        public static Dictionary<string, string> LoadMappings()
        {
            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(CustomMappingsDirectory))
                {
                    Directory.CreateDirectory(CustomMappingsDirectory);
                    SimpleLogger.Log($"Created custom mappings directory: {CustomMappingsDirectory}");
                    return new Dictionary<string, string>();
                }

                // Return empty if file doesn't exist
                if (!File.Exists(CustomMappingsFilePath))
                {
                    SimpleLogger.Log($"Custom mappings file not found: {CustomMappingsFilePath}");
                    return new Dictionary<string, string>();
                }

                string json = File.ReadAllText(CustomMappingsFilePath, Encoding.UTF8);
                var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                SimpleLogger.Log($"Loaded {mappings?.Count ?? 0} custom mappings from {CustomMappingsFilePath}");
                return mappings ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"Failed to load custom mappings from {CustomMappingsFilePath}");
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Saves custom mappings to the JSON file.
        /// </summary>
        public static void SaveMappings(Dictionary<string, string> mappings)
        {
            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(CustomMappingsDirectory))
                {
                    Directory.CreateDirectory(CustomMappingsDirectory);
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string json = JsonSerializer.Serialize(mappings, options);
                File.WriteAllText(CustomMappingsFilePath, json, Encoding.UTF8);

                SimpleLogger.Log($"Saved {mappings.Count} custom mappings to {CustomMappingsFilePath}");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"Failed to save custom mappings to {CustomMappingsFilePath}");
                throw;
            }
        }

        /// <summary>
        /// Gets the custom mappings file path.
        /// </summary>
        public static string GetMappingsFilePath() => CustomMappingsFilePath;

        /// <summary>
        /// Gets the custom mappings directory path.
        /// </summary>
        public static string GetMappingsDirectory() => CustomMappingsDirectory;
    }
}
