using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using Kannada.AsciiUnicode.Converters;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class ConversionFileManager
    {
        public sealed record ConvertResult(DocumentAdv Document, int ConvertedParagraphs);

        // Singleton instance of the SDK
        private static KannadaConverter? _converter;

        // ============================================================
        // GET CUSTOM CONVERTER WITH MAPPINGS
        // ============================================================
        private static KannadaConverter GetConverter()
        {
            if (_converter != null)
                return _converter;

            try
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;

                var asciiToUnicodePath = Path.Combine(basePath, "Resources", "AsciiToUnicodeMapping.json");
                var unicodeToAsciiPath = Path.Combine(basePath, "Resources", "UnicodeToAsciiMapping.json");

                var asciiToUnicode = LoadMapping(asciiToUnicodePath);
                var unicodeToAscii = LoadMapping(unicodeToAsciiPath);

                _converter = KannadaConverter.CreateWithCustomMapping(
                    userAsciiToUnicodeMapping: asciiToUnicode,
                    userUnicodeToAsciiMapping: unicodeToAscii
                );

                SimpleLogger.Log($"Custom mappings loaded | A→U={asciiToUnicode.Count}, U→A={unicodeToAscii.Count}");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Error initializing KannadaConverter: " + ex.Message);
                _converter = KannadaConverter.Instance; // fallback to default
            }

            return _converter;
        }

        // ============================================================
        // LOAD JSON MAPPINGS
        // ============================================================
        private static Dictionary<string, string> LoadMapping(string filePath)
        {
            try
            {
                SimpleLogger.Log($"Loading mapping file: {filePath}");

                if (!File.Exists(filePath))
                {
                    SimpleLogger.LogWarning($"Mapping file not found: {filePath}");
                    return new Dictionary<string, string>();
                }

                var json = File.ReadAllText(filePath, Encoding.UTF8);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                return dict ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"Failed to load mapping file '{filePath}': {ex.Message}");
                return new Dictionary<string, string>();
            }
        }

        // ============================================================
        // MAIN CONVERSION ENTRY
        // ============================================================
        public static Task<ConvertResult> ConvertFileToDocumentAsync(
            string filePath,
            bool asciiToUnicode = true,
            string fontFamilyName = "NudiParijatha",
            bool applyA4NormalMargins = true,
            string[]? nudiFontKeywords = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                SimpleLogger.LogError("File path is empty.");
                throw new ArgumentException("File path is empty.", nameof(filePath));
            }

            string fileName = Path.GetFileName(filePath);
            SimpleLogger.Log($"Conversion started: {fileName}");

            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
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
                        asciiToUnicode,
                        fontFamilyName,
                        nudiFontKeywords
                    );

                    if (applyA4NormalMargins)
                        ApplyA4NormalMargins(tempEditor.Document);

                    SimpleLogger.Log($"Conversion completed: {fileName} | paragraphs converted={paraCount}");
                    return new ConvertResult(tempEditor.Document, paraCount);
                }
                catch (Exception ex)
                {
                    SimpleLogger.LogError($"Failed to convert file '{fileName}': {ex.Message}");
                    return new ConvertResult(new DocumentAdv(), 0); // return empty document on failure
                }

            }).Task;
        }

        // ============================================================
        // DOCUMENT CONVERSION
        // ============================================================
        private static int ConvertDocumentInPlace(
            DocumentAdv doc,
            bool asciiToUnicode,
            string fontFamilyName,
            string[]? nudiFontKeywords)
        {
            var stats = new ConversionStats();
            var targetFont = new FontFamily(fontFamilyName);
            var keywords = nudiFontKeywords ?? new[] { "nudi", "parijatha", "lipi" };

            try
            {
                foreach (SectionAdv section in doc.Sections)
                {
                    for (int i = 0; i < section.Blocks.Count; i++)
                    {
                        ProcessBlockWithSDK(
                            section.Blocks[i],
                            asciiToUnicode,
                            targetFont,
                            keywords,
                            ref stats
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Error during document conversion: " + ex.Message);
            }

            LogConversionStats(stats);
            return stats.ConvertedParagraphs;
        }

        // ============================================================
        // PARAGRAPH PROCESSOR
        // ============================================================
        private static void ProcessBlockWithSDK(
            BlockAdv block,
            bool asciiToUnicode,
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
                    if (nudiBuffer.Length > 0 && nudiFormat != null)
                    {
                        try
                        {
                            var converter = GetConverter();
                            var convertedText = asciiToUnicode
                                ? converter.ConvertAsciiToUnicode(nudiBuffer.ToString())
                                : converter.ConvertUnicodeToAscii(nudiBuffer.ToString());

                            var convertedSpan = new SpanAdv { Text = convertedText };
                            ApplyCharacterFormat(nudiFormat, convertedSpan.CharacterFormat, targetFont);

                            newInlines.Add(convertedSpan);

                            paragraphConverted = true;
                            stats = stats with { ConvertedSpans = stats.ConvertedSpans + 1 };
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.LogError($"Error converting span: {ex.Message}");
                        }
                        finally
                        {
                            nudiBuffer.Clear();
                            nudiFormat = null;
                        }
                    }

                    newInlines.Add(span);
                }
            }

            // Flush trailing Nudi text
            if (nudiBuffer.Length > 0 && nudiFormat != null)
            {
                try
                {
                    var converter = GetConverter();
                    var convertedText = asciiToUnicode
                        ? converter.ConvertAsciiToUnicode(nudiBuffer.ToString())
                        : converter.ConvertUnicodeToAscii(nudiBuffer.ToString());

                    var convertedSpan = new SpanAdv { Text = convertedText };
                    ApplyCharacterFormat(nudiFormat, convertedSpan.CharacterFormat, targetFont);

                    newInlines.Add(convertedSpan);
                    paragraphConverted = true;
                    stats = stats with { ConvertedSpans = stats.ConvertedSpans + 1 };
                }
                catch (Exception ex)
                {
                    SimpleLogger.LogError($"Error converting trailing span: {ex.Message}");
                }
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
        // FONT APPLICATION
        // ============================================================
        private static void ApplyCharacterFormat(CharacterFormat source, CharacterFormat target, FontFamily font)
        {
            try
            {
                target.FontFamily = font;
                target.FontSize = source.FontSize;
                target.Bold = source.Bold;
                target.Italic = source.Italic;
                target.Underline = source.Underline;
                target.FontColor = source.FontColor;
                target.HighlightColor = source.HighlightColor;
                target.BaselineAlignment = source.BaselineAlignment;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError("Error applying character format: " + ex.Message);
            }
        }

        // ============================================================
        // FONT DETECTION
        // ============================================================
        private static bool IsNudiFontFast(SpanAdv span, string[] keywords)
        {
            try
            {
                var source = span.CharacterFormat.FontFamily?.Source;
                if (string.IsNullOrEmpty(source))
                    return false;

                string fontLower = source.ToLowerInvariant();
                return keywords.Any(fontLower.Contains);
            }
            catch
            {
                return false;
            }
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
            try
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
            catch (Exception ex)
            {
                SimpleLogger.LogError("Error applying page setup: " + ex.Message);
            }
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
