using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class ConversionFileManager
    {
        public sealed record ConvertResult(DocumentAdv Document, int ConvertedParagraphs);

        // ============================================================
        // MAIN ENTRY POINT
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
            SimpleLogger.Log($"Convert started: {fileName}");

            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var tempEditor = new SfRichTextBoxAdv();

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var format = GetFormatType(Path.GetExtension(filePath));
                    SimpleLogger.Log($"Loading file: {fileName} | format={format}");
                    tempEditor.Load(fs, format);
                }

                int paraCount = ConvertDocumentInPlace(
                    tempEditor.Document,
                    converter,
                    fontFamilyName,
                    nudiFontKeywords
                );

                if (applyA4NormalMargins)
                    ApplyA4NormalMargins(tempEditor.Document);

                SimpleLogger.Log($"Convert completed: {fileName} | paragraphs={paraCount}");
                return new ConvertResult(tempEditor.Document, paraCount);

            }).Task;
        }

        // ============================================================
        // DOCUMENT CONVERSION
        // ============================================================
        private static int ConvertDocumentInPlace(
            DocumentAdv doc,
            Func<string, string> converter,
            string fontFamilyName,
            string[]? nudiFontKeywords)
        {
            var stats = new ConversionStats();
            var targetFont = new FontFamily(fontFamilyName);
            var keywords = nudiFontKeywords ?? new[] { "nudi", "parijatha", "lipi" };

            foreach (SectionAdv section in doc.Sections)
            {
                for (int i = 0; i < section.Blocks.Count; i++)
                {
                    ProcessBlockFast(
                        section.Blocks[i],
                        converter,
                        targetFont,
                        keywords,
                        ref stats
                    );
                }
            }

            LogConversionStats(stats);
            return stats.ConvertedParagraphs;
        }

        // ============================================================
        // ORDER-PRESERVING PARAGRAPH PROCESSOR
        // ============================================================
        private static void ProcessBlockFast(
            BlockAdv block,
            Func<string, string> converter,
            FontFamily targetFont,
            string[] keywords,
            ref ConversionStats stats)
        {
            if (block is not ParagraphAdv para || para.Inlines.Count == 0)
                return;

            var newInlines = new List<SpanAdv>();
            var nudiBuffer = new StringBuilder();
            CharacterFormat? nudiFormat = null;
            bool paragraphConverted = false;

            foreach (SpanAdv span in para.Inlines.OfType<SpanAdv>())
            {
                if (string.IsNullOrEmpty(span.Text))
                    continue;

                bool isNudi = IsNudiFontFast(span, keywords);

                if (isNudi)
                {
                    nudiFormat ??= span.CharacterFormat;
                    nudiBuffer.Append(span.Text);
                }
                else
                {
                    // Flush pending Nudi text before non-Nudi span
                    if (nudiBuffer.Length > 0 && nudiFormat != null)
                    {
                        var convertedSpan = new SpanAdv
                        {
                            Text = converter(nudiBuffer.ToString())
                        };

                        convertedSpan.CharacterFormat.FontFamily = targetFont;
                        convertedSpan.CharacterFormat.FontSize = nudiFormat.FontSize;
                        convertedSpan.CharacterFormat.Bold = nudiFormat.Bold;
                        convertedSpan.CharacterFormat.Italic = nudiFormat.Italic;
                        convertedSpan.CharacterFormat.Underline = nudiFormat.Underline;
                        convertedSpan.CharacterFormat.FontColor = nudiFormat.FontColor;

                        newInlines.Add(convertedSpan);

                        nudiBuffer.Clear();
                        nudiFormat = null;

                        paragraphConverted = true;
                        stats = stats with { ConvertedSpans = stats.ConvertedSpans + 1 };
                    }

                    // Keep original non-Nudi span exactly as-is
                    newInlines.Add(span);
                }
            }

            // Flush trailing Nudi text
            if (nudiBuffer.Length > 0 && nudiFormat != null)
            {
                var convertedSpan = new SpanAdv
                {
                    Text = converter(nudiBuffer.ToString())
                };

                convertedSpan.CharacterFormat.FontFamily = targetFont;
                convertedSpan.CharacterFormat.FontSize = nudiFormat.FontSize;
                convertedSpan.CharacterFormat.Bold = nudiFormat.Bold;
                convertedSpan.CharacterFormat.Italic = nudiFormat.Italic;
                convertedSpan.CharacterFormat.Underline = nudiFormat.Underline;
                convertedSpan.CharacterFormat.FontColor = nudiFormat.FontColor;

                newInlines.Add(convertedSpan);

                paragraphConverted = true;
                stats = stats with { ConvertedSpans = stats.ConvertedSpans + 1 };
            }

            if (!paragraphConverted)
            {
                stats = stats with { SkippedParagraphs = stats.SkippedParagraphs + 1 };
                return;
            }

            para.Inlines.Clear();
            foreach (var s in newInlines)
                para.Inlines.Add(s);

            stats = stats with { ConvertedParagraphs = stats.ConvertedParagraphs + 1 };
        }

        // ============================================================
        // FONT DETECTION
        // ============================================================
        private static bool IsNudiFontFast(SpanAdv span, string[] keywords)
        {
            var source = span.CharacterFormat.FontFamily?.Source;
            if (string.IsNullOrEmpty(source))
                return false;

            string fontLower = source.ToLowerInvariant();
            return keywords.Any(fontLower.Contains);
        }

        // ============================================================
        // FORMAT COPY (SYNCFUSION-SAFE)
        // ============================================================
        private static void CopyCharacterFormat(CharacterFormat source, CharacterFormat target)
        {
            target.FontSize = source.FontSize;
            target.Bold = source.Bold;
            target.Italic = source.Italic;
            target.Underline = source.Underline;
            target.FontColor = source.FontColor;
            target.HighlightColor = source.HighlightColor;
            target.BaselineAlignment = source.BaselineAlignment;
        }

        // ============================================================
        // LOGGING
        // ============================================================
        private static void LogConversionStats(ConversionStats stats)
        {
            SimpleLogger.Log(
                $"Converted paragraphs={stats.ConvertedParagraphs}, " +
                $"converted spans={stats.ConvertedSpans}, " +
                $"skipped paragraphs={stats.SkippedParagraphs}"
            );
        }

        private sealed record ConversionStats
        {
            public int ConvertedParagraphs { get; init; }
            public int ConvertedSpans { get; init; }
            public int SkippedParagraphs { get; init; }
        }

        // ============================================================
        // PAGE SETUP
        // ============================================================
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

            SimpleLogger.Log("Page setup applied: A4 with normal margins");
        }

        // ============================================================
        // FORMAT DETECTION
        // ============================================================
        private static FormatType GetFormatType(string extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".rtf" => FormatType.Rtf,
                ".txt" => FormatType.Txt,
                ".html" => FormatType.Html,
                ".htm" => FormatType.Html,
                ".doc" => FormatType.Doc,
                _ => FormatType.Docx
            };
        }
    }
}
