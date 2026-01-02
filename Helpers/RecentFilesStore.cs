using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace KannadaNudiEditor.Helpers
{
    public sealed class RecentFileItem
    {
        public string FullPath { get; set; } = "";
        public string FileType { get; set; } = "";   // "txt", "rtf", "docx", ...
        public string FileName { get; set; } = "";

        private bool _isAscii;
        public bool IsAscii
        {
            get => _isAscii;
            set
            {
                _isAscii = value;
                if (value) _isUnicode = false;
            }
        }

        private bool _isUnicode = true;
        public bool IsUnicode
        {
            get => _isUnicode;
            set
            {
                _isUnicode = value;
                if (value) _isAscii = false;
            }
        }

        public bool IsUpdated { get; set; }
        public DateTimeOffset LastOpen { get; set; }
    }

    public static class RecentFilesStore
    {
        private const int MaxRecentFiles = 15;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true // pretty JSON [web:80]
        };

        private static string RecentFilesJsonPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KannadaNudiBaraha",
                "recent-files.json");

        private static TimeZoneInfo GetIstTimeZoneOrFallback()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"); // [web:174]
            }
            catch
            {
                return TimeZoneInfo.Local;
            }
        }

        private static DateTimeOffset NowIst()
        {
            var ist = GetIstTimeZoneOrFallback();
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ist);
        }

        public static List<RecentFileItem> ReadAll()
        {
            try
            {
                if (!File.Exists(RecentFilesJsonPath))
                    return new List<RecentFileItem>();

                var json = File.ReadAllText(RecentFilesJsonPath);
                return JsonSerializer.Deserialize<List<RecentFileItem>>(json, JsonOptions) ?? new List<RecentFileItem>();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] ReadAll failed");
                return new List<RecentFileItem>();
            }
        }

        public static List<RecentFileItem> AddOrUpdate(
            string fullPath,
            bool isAscii,
            bool isUnicode,
            string? fileType = null,
            string? fileName = null)
        {
            try
            {
                var normalizedPath = Path.GetFullPath(fullPath);

                if (isAscii && isUnicode) isUnicode = false;
                if (!isAscii && !isUnicode) isUnicode = true;

                var recentFiles = ReadAll();

                int existingIndex = recentFiles.FindIndex(x =>
                    string.Equals(x.FullPath, normalizedPath, StringComparison.OrdinalIgnoreCase));

                bool wasUpdated;

                if (existingIndex >= 0)
                {
                    var existingItem = recentFiles[existingIndex];
                    existingItem.LastOpen = NowIst();
                    existingItem.IsUpdated = true;

                    existingItem.FileName = !string.IsNullOrWhiteSpace(fileName)
                        ? fileName
                        : (!string.IsNullOrWhiteSpace(existingItem.FileName) ? existingItem.FileName : Path.GetFileName(normalizedPath));

                    existingItem.FileType = !string.IsNullOrWhiteSpace(fileType)
                        ? NormalizeFileType(fileType)
                        : (!string.IsNullOrWhiteSpace(existingItem.FileType) ? existingItem.FileType : GetFileTypeFromPath(normalizedPath));

                    existingItem.IsAscii = isAscii;
                    existingItem.IsUnicode = isUnicode;

                    recentFiles.RemoveAt(existingIndex);
                    recentFiles.Insert(0, existingItem);

                    wasUpdated = true;
                }
                else
                {
                    var newItem = new RecentFileItem
                    {
                        FullPath = normalizedPath,
                        FileName = !string.IsNullOrWhiteSpace(fileName) ? fileName : Path.GetFileName(normalizedPath),
                        FileType = !string.IsNullOrWhiteSpace(fileType) ? NormalizeFileType(fileType) : GetFileTypeFromPath(normalizedPath),
                        IsAscii = isAscii,
                        IsUnicode = isUnicode,
                        IsUpdated = false,
                        LastOpen = NowIst()
                    };

                    recentFiles.Insert(0, newItem);
                    wasUpdated = false;
                }

                // keep only existing files (remove if you want to show missing too)
                recentFiles = recentFiles
                    .Where(x => !string.IsNullOrWhiteSpace(x.FullPath) && File.Exists(x.FullPath))
                    .ToList();

                if (recentFiles.Count > MaxRecentFiles)
                    recentFiles = recentFiles.Take(MaxRecentFiles).ToList();

                Directory.CreateDirectory(Path.GetDirectoryName(RecentFilesJsonPath)!);
                File.WriteAllText(RecentFilesJsonPath, JsonSerializer.Serialize(recentFiles, JsonOptions));

                SimpleLogger.Log($"[RECENTFILE] {(wasUpdated ? "Updated" : "Added")} : {normalizedPath}");

                return recentFiles;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] AddOrUpdate failed");
                return ReadAll();
            }
        }

        private static string GetFileTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(ext) ? "unknown" : ext.TrimStart('.').ToLowerInvariant();
        }


        public static void Clear()
        {
            try
            {
                if (File.Exists(RecentFilesJsonPath))
                    File.Delete(RecentFilesJsonPath);

                SimpleLogger.Log("[RECENTFILE] Cleared store");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] Clear failed");
            }
        }

        private static string NormalizeFileType(string fileType)
        {
            var t = fileType.Trim();
            if (t.StartsWith(".", StringComparison.Ordinal)) t = t.TrimStart('.');
            return string.IsNullOrWhiteSpace(t) ? "unknown" : t.ToLowerInvariant();
        }
    }
}
