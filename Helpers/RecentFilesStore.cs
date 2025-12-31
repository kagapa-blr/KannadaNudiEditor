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

        // Store as IST with +05:30 offset
        public DateTimeOffset LastOpen { get; set; }

        public string DisplayName => Path.GetFileName(FullPath);
    }

    public static class RecentFilesStore
    {
        private const int MaxItems = 15;

        private static string StorePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KannadaNudiBaraha",
                "recent-files.json");

        private static TimeZoneInfo IstTimeZone
        {
            get
            {
                // Windows ID for IST
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
        }

        private static DateTimeOffset NowIst()
        {
            var nowUtc = DateTimeOffset.UtcNow;
            var istNow = TimeZoneInfo.ConvertTime(nowUtc, IstTimeZone);
            return istNow;
        }

        public static List<RecentFileItem> Load()
        {
            SimpleLogger.Log("[RECENTFILE] Load from: " + StorePath);
            try
            {
                if (!File.Exists(StorePath))
                {
                    SimpleLogger.Log("[RECENTFILE] File not found.");
                    return new();
                }

                var json = File.ReadAllText(StorePath);
                var list = JsonSerializer.Deserialize<List<RecentFileItem>>(json) ?? new();

                // Optional: keep only existing
                list = list.Where(x => File.Exists(x.FullPath)).ToList();

                SimpleLogger.Log("[RECENTFILE] Loaded count: " + list.Count);
                return list;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] Load failed");
                return new();
            }
        }

        public static void Save(List<RecentFileItem> items)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);

                // enforce max here also
                if (items.Count > MaxItems)
                    items = items.Take(MaxItems).ToList();

                var json = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(StorePath, json);

                SimpleLogger.Log("[RECENTFILE] Saved count: " + items.Count + " to " + StorePath);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[RECENTFILE] Save failed");
            }
        }

        public static List<RecentFileItem> AddOrBump(List<RecentFileItem> list, string filePath)
        {
            filePath = Path.GetFullPath(filePath);

            list.RemoveAll(x => string.Equals(x.FullPath, filePath, StringComparison.OrdinalIgnoreCase));
            list.Insert(0, new RecentFileItem { FullPath = filePath, LastOpen = NowIst() });

            // keep only existing
            list = list.Where(x => File.Exists(x.FullPath)).ToList();

            if (list.Count > MaxItems)
                list = list.Take(MaxItems).ToList();

            return list;
        }
    }
}
