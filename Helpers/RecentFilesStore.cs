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
        public const int MaxRecentFiles = 15;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static string RecentFilesJsonPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KannadaNudiBaraha",
                "recent-files.json");

        public static IReadOnlyList<RecentFileItem> RefreshFromDisk()
        {
            // “Refresh” semantics: read + normalize + save back (keeps file clean).
            var items = ReadAllInternal();
            items = NormalizeAndTrim(items);
            SaveAllInternal(items);
            return items;
        }

        public static IReadOnlyList<RecentFileItem> AddOrUpdate(
            string fullPath,
            bool isAscii,
            bool isUnicode,
            string? fileType = null,
            string? fileName = null)
        {
            try
            {
                var normalizedPath = Path.GetFullPath(fullPath);

                // enforce mutually exclusive flags (same as your logic)
                if (isAscii && isUnicode) isUnicode = false;
                if (!isAscii && !isUnicode) isUnicode = true;

                var items = ReadAllInternal();

                var existingIndex = items.FindIndex(x =>
                    string.Equals(x.FullPath, normalizedPath, StringComparison.OrdinalIgnoreCase));

                if (existingIndex >= 0)
                {
                    var existing = items[existingIndex];
                    existing.LastOpen = NowIst();
                    existing.IsUpdated = true;

                    existing.FileName = !string.IsNullOrWhiteSpace(fileName)
                        ? fileName
                        : (!string.IsNullOrWhiteSpace(existing.FileName) ? existing.FileName : Path.GetFileName(normalizedPath));

                    existing.FileType = !string.IsNullOrWhiteSpace(fileType)
                        ? NormalizeFileType(fileType)
                        : (!string.IsNullOrWhiteSpace(existing.FileType) ? existing.FileType : GetFileTypeFromPath(normalizedPath));

                    existing.IsAscii = isAscii;
                    existing.IsUnicode = isUnicode;

                    items.RemoveAt(existingIndex);
                    items.Insert(0, existing);
                }
                else
                {
                    items.Insert(0, new RecentFileItem
                    {
                        FullPath = normalizedPath,
                        FileName = !string.IsNullOrWhiteSpace(fileName) ? fileName : Path.GetFileName(normalizedPath),
                        FileType = !string.IsNullOrWhiteSpace(fileType) ? NormalizeFileType(fileType) : GetFileTypeFromPath(normalizedPath),
                        IsAscii = isAscii,
                        IsUnicode = isUnicode,
                        IsUpdated = false,
                        LastOpen = NowIst()
                    });
                }

                items = NormalizeAndTrim(items);
                SaveAllInternal(items);

                SimpleLogger.Log($"[RECENTFILE] Upsert OK: {normalizedPath}");
                return items;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] AddOrUpdate failed");
                return RefreshFromDisk();
            }
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

        // ----------------- Internals -----------------

        private static List<RecentFileItem> ReadAllInternal()
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
                SimpleLogger.LogException(ex, "[RECENTFILE] Read failed");
                return new List<RecentFileItem>();
            }
        }

        private static void SaveAllInternal(List<RecentFileItem> items)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(RecentFilesJsonPath)!);
                File.WriteAllText(RecentFilesJsonPath, JsonSerializer.Serialize(items, JsonOptions));
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] Save failed");
            }
        }

        private static List<RecentFileItem> NormalizeAndTrim(List<RecentFileItem> items)
        {
            // Remove invalid/missing, fix fields, sort by LastOpen desc, take max.
            var normalized = items
                .Where(x => x != null)
                .Where(x => !string.IsNullOrWhiteSpace(x.FullPath))
                .Select(x =>
                {
                    x.FullPath = Path.GetFullPath(x.FullPath);

                    if (string.IsNullOrWhiteSpace(x.FileName))
                        x.FileName = Path.GetFileName(x.FullPath);

                    if (string.IsNullOrWhiteSpace(x.FileType))
                        x.FileType = GetFileTypeFromPath(x.FullPath);
                    else
                        x.FileType = NormalizeFileType(x.FileType);

                    // keep your mutual exclusion behavior safe
                    if (x.IsAscii && x.IsUnicode) x.IsUnicode = false;
                    if (!x.IsAscii && !x.IsUnicode) x.IsUnicode = true;

                    return x;
                })
                // your existing design: keep only existing files
                .Where(x => File.Exists(x.FullPath))
                .OrderByDescending(x => x.LastOpen)
                // remove duplicates by path (keep first = newest)
                .GroupBy(x => x.FullPath, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .Take(MaxRecentFiles)
                .ToList();

            return normalized;
        }

        private static string GetFileTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path);
            return string.IsNullOrWhiteSpace(ext) ? "unknown" : ext.TrimStart('.').ToLowerInvariant();
        }

        private static string NormalizeFileType(string fileType)
        {
            var t = fileType.Trim();
            if (t.StartsWith(".", StringComparison.Ordinal)) t = t.TrimStart('.');
            return string.IsNullOrWhiteSpace(t) ? "unknown" : t.ToLowerInvariant();
        }

        private static DateTimeOffset NowIst()
        {
            var ist = GetIstTimeZoneOrFallback();
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ist);
        }

        private static TimeZoneInfo GetIstTimeZoneOrFallback()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                return TimeZoneInfo.Local;
            }
        }
    }
}
