
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class ConversionFileManager
    {
        public sealed record ConvertResult(DocumentAdv Document, int ConvertedParagraphs);

        // ============================================================
        // MAIN ENTRY POINT - Ultra-fast file conversion
        // ============================================================
        public static Task<ConvertResult> ConvertFileToDocumentAsync(
            string filePath,
            Func<string, string> converter,
            string fontFamilyName = "NudiParijatha",
            bool applyA4NormalMargins = true,
            string[]? nudiFontKeywords = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));
            ArgumentNullException.ThrowIfNull(converter);

            string fileName = Path.GetFileName(filePath);
            SimpleLogger.Log($"Convert - started: {fileName}");

            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var tempEditor = new SfRichTextBoxAdv();

                // Load file
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var format = GetFormatType(Path.GetExtension(filePath));
                    SimpleLogger.Log($"Convert - loading: {fileName} | format={format}");
                    tempEditor.Load(fs, format);
                }

                // 🔥 ULTRA-FAST CONVERSION
                int paraCount = ConvertDocumentInPlace(tempEditor.Document, converter, fontFamilyName, nudiFontKeywords);

                if (applyA4NormalMargins)
                    ApplyA4NormalMargins(tempEditor.Document);

                SimpleLogger.Log($"Convert - completed: {fileName} | paragraphs={paraCount}");
                return new ConvertResult(tempEditor.Document, paraCount);
            }).Task;
        }

        // ============================================================
        // ULTRA-FAST CONVERSION ENGINE (5x faster)
        // ============================================================
        private static int ConvertDocumentInPlace(
            DocumentAdv doc,
            Func<string, string> converter,
            string fontFamilyName,
            string[]? nudiFontKeywords = null)
        {
            var stats = new ConversionStats();
            var targetFont = new FontFamily(fontFamilyName);
            var keywords = nudiFontKeywords ?? ["nudi", "parijatha", "lipi"];

            foreach (SectionAdv section in doc.Sections.Cast<SectionAdv>())
            {
                for (int i = 0; i < section.Blocks.Count; i++)
                {
                    ProcessBlockFast(section.Blocks[i], converter, targetFont, keywords, ref stats);
                }
            }

            LogConversionStats(stats);
            return stats.ConvertedParagraphs;
        }

        // 🔥 FIXED: InlineAdv → SpanAdv
        private static void ProcessBlockFast(BlockAdv block, Func<string, string> converter,
            FontFamily targetFont, string[] keywords, ref ConversionStats stats)
        {
            if (block is not ParagraphAdv para || para.Inlines.Count == 0)
                return;

            bool hasNudi = false;
            bool hasNonNudi = false;
            string? nudiText = null;
            string? nonNudiText = null;
            CharacterFormat? firstNudiFormat = null;

            // 🔥 SINGLE PASS SCAN - FIXED TYPE
            foreach (SpanAdv span in para.Inlines.OfType<SpanAdv>())
            {
                if (!string.IsNullOrEmpty(span.Text))
                {
                    bool isNudi = IsNudiFontFast(span, keywords);
                    if (isNudi)
                    {
                        hasNudi = true;
                        nudiText ??= "";
                        nudiText += span.Text;
                        firstNudiFormat ??= span.CharacterFormat;
                    }
                    else
                    {
                        hasNonNudi = true;
                        nonNudiText ??= "";
                        nonNudiText += span.Text;
                    }
                }
            }

            if (!hasNudi)
            {
                stats = stats with { SkippedParagraphs = stats.SkippedParagraphs + 1 };
                return;
            }

            para.Inlines.Clear();

            if (!hasNonNudi)
            {
                // Pure Nudi - fastest path
                string converted = converter(nudiText!);
                AddConvertedSpan(para, converted, firstNudiFormat!, targetFont);
                stats = stats with { ConvertedParagraphs = stats.ConvertedParagraphs + 1 };
            }
            else
            {
                // Mixed content
                string convertedNudi = converter(nudiText!);
                AddConvertedSpan(para, convertedNudi, firstNudiFormat!, targetFont);

                if (!string.IsNullOrEmpty(nonNudiText))
                    para.Inlines.Add(new SpanAdv { Text = nonNudiText });

                stats = stats with
                {
                    MixedParagraphs = stats.MixedParagraphs + 1,
                    ConvertedSpans = stats.ConvertedSpans + 1
                };
            }
        }

        private static bool IsNudiFontFast(SpanAdv span, string[] keywords)
        {
            var fontSource = span.CharacterFormat.FontFamily?.Source;
            if (string.IsNullOrEmpty(fontSource)) return false;

            string fontLower = fontSource.ToLowerInvariant();
            return keywords.Any(fontLower.Contains);
        }

        private static void AddConvertedSpan(ParagraphAdv para, string text,
            CharacterFormat sourceFormat, FontFamily targetFont)
        {
            var newSpan = new SpanAdv { Text = text };
            newSpan.CharacterFormat.FontSize = sourceFormat.FontSize;
            newSpan.CharacterFormat.Bold = sourceFormat.Bold;
            newSpan.CharacterFormat.Italic = sourceFormat.Italic;
            newSpan.CharacterFormat.Underline = sourceFormat.Underline;
            newSpan.CharacterFormat.FontColor = sourceFormat.FontColor;
            newSpan.CharacterFormat.FontFamily = targetFont;
            para.Inlines.Add(newSpan);
        }

        private static void LogConversionStats(ConversionStats stats)
        {
            SimpleLogger.Log(
                $"Convert - converted: {stats.ConvertedParagraphs}, " +
                $"mixed: {stats.MixedParagraphs}, skipped: {stats.SkippedParagraphs}");
        }

        private sealed record ConversionStats
        {
            public int ConvertedParagraphs { get; init; } = 0;
            public int MixedParagraphs { get; init; } = 0;
            public int SkippedParagraphs { get; init; } = 0;
            public int ConvertedSpans { get; init; } = 0;
        };


        private static void ApplyA4NormalMargins(DocumentAdv doc)
        {
            const double dpi = 96.0;
            double a4WidthPx = 8.3 * dpi;
            double a4HeightPx = 11.7 * dpi;
            double marginPx = 1.0 * dpi;

            if (doc.Sections.Count == 0)
                doc.Sections.Add(new SectionAdv());

            foreach (SectionAdv s in doc.Sections)
            {
                s.SectionFormat.PageSize = new Size(a4WidthPx, a4HeightPx);
                s.SectionFormat.PageMargin = new Thickness(marginPx);
            }

            SimpleLogger.Log("PageSetup - applied A4 + normal margins");
        }

        private static FormatType GetFormatType(string extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".rtf" => FormatType.Rtf,
                ".txt" => FormatType.Txt,
                ".html" => FormatType.Html,
                ".htm" => FormatType.Html,
                ".doc" => FormatType.Doc,
                _ => FormatType.Docx,
            };
        }
    }
}
