using System.Windows;

namespace KannadaNudiEditor.Helpers
{
    /// <summary>
    /// Represents a single page size entry expressed in inches.
    /// </summary>
    public sealed class PageSizeInfo
    {
        public string Key { get; init; } = string.Empty;
        public double WidthInInches { get; init; }
        public double HeightInInches { get; init; }

        private const double DefaultDpi = 96d;

        /// <summary>
        /// Returns the <see cref="Size"/> in pixels for the given DPI (Syncfusion uses pixels internally).
        /// </summary>
        public Size ToPixelSize(double dpi = DefaultDpi) => new(WidthInInches * dpi, HeightInInches * dpi);

        public override string ToString() => $"{Key} ({WidthInInches} in × {HeightInInches} in)";
    }

    /// <summary>
    /// Central repository for all predefined page‑sizes. Keeps UI and logic in sync and avoids hard‑coded duplicates.
    /// </summary>
    public static class PageSizeHelper
    {
        private static readonly IReadOnlyDictionary<string, PageSizeInfo> _pageSizes;

        static PageSizeHelper()
        {
            // ⚠ Keep this list in a single location to avoid divergence between the ComboBox items and the code‑behind.
            var sizes = new[]
            {
                // North American Sizes
                new PageSizeInfo { Key = "Letter",    WidthInInches = 8.5,  HeightInInches = 11   },
                new PageSizeInfo { Key = "Legal",     WidthInInches = 8.5,  HeightInInches = 14   },
                new PageSizeInfo { Key = "Tabloid",   WidthInInches = 11,   HeightInInches = 17   },
                new PageSizeInfo { Key = "Executive", WidthInInches = 7.25, HeightInInches = 10.5 },
                new PageSizeInfo { Key = "Statement", WidthInInches = 5.5,  HeightInInches = 8.5  },

                // ISO A Series
                new PageSizeInfo { Key = "A0",  WidthInInches = 33.1, HeightInInches = 46.8 },
                new PageSizeInfo { Key = "A1",  WidthInInches = 23.4, HeightInInches = 33.1 },
                new PageSizeInfo { Key = "A2",  WidthInInches = 16.5, HeightInInches = 23.4 },
                new PageSizeInfo { Key = "A3",  WidthInInches = 11.7, HeightInInches = 16.5 },
                new PageSizeInfo { Key = "A4",  WidthInInches = 8.3,  HeightInInches = 11.7 },
                new PageSizeInfo { Key = "A5",  WidthInInches = 5.8,  HeightInInches = 8.3  },
                new PageSizeInfo { Key = "A6",  WidthInInches = 4.1,  HeightInInches = 5.8  },
                new PageSizeInfo { Key = "A7",  WidthInInches = 2.9,  HeightInInches = 4.1  },
                new PageSizeInfo { Key = "A8",  WidthInInches = 2.0,  HeightInInches = 2.9  },
                new PageSizeInfo { Key = "A9",  WidthInInches = 1.5,  HeightInInches = 2.0  },
                new PageSizeInfo { Key = "A10", WidthInInches = 1.0,  HeightInInches = 1.5  },

                // ISO B Series (JIS)
                new PageSizeInfo { Key = "B4 (JIS)", WidthInInches = 10.1, HeightInInches = 14.3 },
                new PageSizeInfo { Key = "B5 (JIS)", WidthInInches = 7.2,  HeightInInches = 10.1 },

                // ANSI Architectural Sizes
                new PageSizeInfo { Key = "ANSI A", WidthInInches = 8.5,  HeightInInches = 11  },
                new PageSizeInfo { Key = "ANSI B", WidthInInches = 11,   HeightInInches = 17  },
                new PageSizeInfo { Key = "ANSI C", WidthInInches = 17,   HeightInInches = 22  },
                new PageSizeInfo { Key = "ANSI D", WidthInInches = 22,   HeightInInches = 34  },
                new PageSizeInfo { Key = "ANSI E", WidthInInches = 34,   HeightInInches = 44  }
            };

            _pageSizes = sizes.ToDictionary(p => p.Key, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// All known predefined page sizes (excluding the custom option).
        /// </summary>
        public static IReadOnlyCollection<PageSizeInfo> All => (IReadOnlyCollection<PageSizeInfo>)_pageSizes.Values;

        public static bool TryGet(string key, out PageSizeInfo pageSize) => _pageSizes.TryGetValue(key, out pageSize);

        /// <summary>
        /// Convenience helper for callers that only need the converted <see cref="Size"/>.
        /// </summary>
        public static Size ToPixelSize(string key, double dpi = 96)
        {
            if (!TryGet(key, out var info))
                throw new ArgumentException($"Unknown page size: {key}", nameof(key));

            return info.ToPixelSize(dpi);
        }
    }
}
