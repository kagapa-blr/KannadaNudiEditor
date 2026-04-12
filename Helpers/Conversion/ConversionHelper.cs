using System.IO;
using System.Text;
using Kannada.AsciiUnicode.Converters;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Media;
using System.Windows;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class ConversionHelper
    {
        private static readonly string TempFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"KannadaNudiBaraha\TempConverted");

        private static KannadaConverter? _converter;
        private static object _converterLock = new();

        /// <summary>
        /// Gets or creates the Kannada converter with custom mappings merged and prioritized.
        /// Custom mappings override SDK defaults for edge cases while maintaining full compatibility.
        /// Cached for performance but can be reset when custom mappings are updated.
        /// </summary>
        public static KannadaConverter Converter
        {
            get
            {
                if (_converter == null)
                {
                    lock (_converterLock)
                    {
                        if (_converter == null)
                        {
                            _converter = LoadConverterWithCustomMappings();
                        }
                    }
                }
                return _converter;
            }
        }

        /// <summary>
        /// Loads SDK default mappings and merges with custom mappings.
        /// Custom mappings take priority - if a mapping exists in both, custom wins.
        /// Existing mappings are preserved (no data loss).
        /// </summary>
        private static KannadaConverter LoadConverterWithCustomMappings()
        {
            Directory.CreateDirectory(TempFolder);

            Dictionary<string, string>? customMappings = null;
            int customMappingCount = 0;

            try
            {
                customMappings = CustomMappingsHelper.LoadMappings();
                customMappingCount = customMappings.Count;

                if (customMappingCount > 0)
                {
                    SimpleLogger.Log($"✓ Loaded {customMappingCount} custom ASCII→Unicode mappings");
                    SimpleLogger.Log($"  Custom mappings will override SDK defaults for these {customMappingCount} edge cases");
                    foreach (var kvp in customMappings.Take(5))
                    {
                        SimpleLogger.Log($"    Custom: '{kvp.Key}' → '{kvp.Value}'");
                    }
                    if (customMappingCount > 5)
                    {
                        SimpleLogger.Log($"    ... and {customMappingCount - 5} more custom mappings");
                    }
                }
                else
                {
                    SimpleLogger.Log("ℹ No custom mappings found. Using SDK default mappings only.");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Failed to load custom mappings");
                SimpleLogger.Log("⚠ Continuing with SDK default mappings only due to loading error");
                customMappings = null;
            }

            // Create converter with merged custom + SDK default mappings
            // SDK properly merges them with custom taking priority (existing mappings preserved)
            var converter = KannadaConverter.CreateWithCustomMapping(customMappings);

            SimpleLogger.Log($"✓ KannadaConverter initialized - {(customMappingCount > 0 ? $"using custom + SDK default mappings ({customMappingCount} custom overrides)" : "using SDK default mappings only")}");
            return converter;
        }

        /// <summary>
        /// Resets the converter cache to force reload of mappings on next access.
        /// Call this after custom mappings are saved to apply them immediately.
        /// </summary>
        public static void ResetConverter()
        {
            lock (_converterLock)
            {
                _converter = null;
            }
            SimpleLogger.Log("✓ Converter cache reset. Custom mappings will be reloaded on next conversion.");
        }

        /// <summary>
        /// Validates that custom mappings are saved.
        /// For testing/debugging purposes.
        /// </summary>
        public static bool HasCustomMappingsLoaded()
        {
            var customMappings = CustomMappingsHelper.LoadMappings();
            return customMappings.Count > 0;
        }

        /// <summary>
        /// Gets the count of custom mappings currently saved.
        /// </summary>
        public static int GetCustomMappingsCount()
        {
            return CustomMappingsHelper.LoadMappings().Count;
        }

        public static (string tempFile, int totalParagraphs) ConvertFileToTempWithParagraphCount(
            string inputPath, Func<string, string> converter)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException($"Input file not found: {inputPath}", inputPath);

            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath).ToLowerInvariant();
            string tempFile = Path.Combine(TempFolder, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmssfff}{ext}");

            SimpleLogger.Log($"Starting conversion: {inputPath} → {tempFile}");

            int paragraphCount = ext switch
            {
                ".txt" => ConvertTextFile(inputPath, tempFile, converter),
                ".docx" => ConvertDocxFileWithParagraphCount(inputPath, tempFile, converter),
                _ => throw new NotSupportedException($"Unsupported file type: {ext}")
            };

            SimpleLogger.Log($"Conversion completed. Output: {tempFile}, Paragraphs processed: {paragraphCount}");
            return (tempFile, paragraphCount);
        }

        private static int ConvertTextFile(string inputPath, string outputPath, Func<string, string> converter)
        {
            SimpleLogger.Log($"Processing text file: {Path.GetFileName(inputPath)}");
            string content = File.ReadAllText(inputPath, Encoding.UTF8);
            string converted = converter(content);
            File.WriteAllText(outputPath, converted, Encoding.UTF8);
            SimpleLogger.Log($"Text file conversion completed: {Path.GetFileName(outputPath)}");
            return 1;
        }

        private static int ConvertDocxFileWithParagraphCount(string inputPath, string outputPath, Func<string, string> converter)
        {
            SimpleLogger.Log($"Processing DOCX file: {Path.GetFileName(inputPath)}");
            return DocxHelper.ConvertDocxWithParagraphCount(inputPath, outputPath, converter);
        }

        public static async Task LoadFileIntoEditorAsync(string filePath, SfRichTextBoxAdv richTextBox, CancellationToken? cancellationToken = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Converted file not found: {filePath}", filePath);

            SimpleLogger.Log($"Loading file into editor: {Path.GetFileName(filePath)}");

            var format = GetFormatType(Path.GetExtension(filePath));
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            // Create a completely fresh control (like the sample app)
            var tempControl = new SfRichTextBoxAdv();
            tempControl.Load(fs, format);

            // Copy the loaded document to the main control
            richTextBox.Document = tempControl.Document;

            await Task.Delay(200);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                richTextBox.Focus();
                richTextBox.Document.CharacterFormat.FontFamily = new FontFamily("NudiParijatha");
                richTextBox.Selection.CharacterFormat.FontFamily = new FontFamily("NudiParijatha");
                SimpleLogger.Log("Document loaded into editor with NudiParijatha font applied");
            });
        }

        private static FormatType GetFormatType(string ext) => ext.ToLowerInvariant() switch
        {
            ".txt" => FormatType.Txt,
            ".rtf" => FormatType.Rtf,
            ".xaml" => FormatType.Xaml,
            ".docx" => FormatType.Docx,
            ".doc" => FormatType.Doc,
            ".htm" or ".html" => FormatType.Html,
            _ => throw new NotSupportedException($"Unsupported file type: {ext}")
        };

        private static class DocxHelper
        {
            public static int ConvertDocxWithParagraphCount(string inputPath, string outputPath, Func<string, string> converter)
            {
                File.Copy(inputPath, outputPath, true);
                using var doc = WordprocessingDocument.Open(outputPath, true);
                var main = doc.MainDocumentPart;
                if (main?.Document == null)
                {
                    SimpleLogger.LogWarning("Main document part not found in DOCX");
                    return 0;
                }

                var loggedFonts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int totalParagraphs = 0;
                totalParagraphs += ConvertBody(main.Document.Body, converter, null, loggedFonts);

                if (main.HeaderParts != null)
                {
                    foreach (var header in main.HeaderParts)
                        totalParagraphs += ConvertBody(header.Header, converter, null, loggedFonts);
                }

                if (main.FooterParts != null)
                {
                    foreach (var footer in main.FooterParts)
                        totalParagraphs += ConvertBody(footer.Footer, converter, null, loggedFonts);
                }

                if (main.FootnotesPart != null)
                    totalParagraphs += ConvertBody(main.FootnotesPart.Footnotes, converter, null, loggedFonts);

                if (main.EndnotesPart != null)
                    totalParagraphs += ConvertBody(main.EndnotesPart.Endnotes, converter, null, loggedFonts);

                main.Document.Save();
                SimpleLogger.Log($"DOCX conversion completed. Total paragraphs processed: {totalParagraphs}");
                return totalParagraphs;
            }

            private static int ConvertBody(OpenXmlElement? element, Func<string, string> converter, string[]? nudiFontKeywords = null, HashSet<string>? loggedFonts = null)
            {
                if (element == null) return 0;
                nudiFontKeywords ??= new[] { "nudi", "parijatha", "lipi" };
                loggedFonts ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                int paragraphCount = 0;
                int skippedRuns = 0;
                int convertedRuns = 0;

                foreach (var para in element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    foreach (var run in para.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                    {
                        var runProps = run.RunProperties;
                        string? fontName = runProps?.RunFonts?.Ascii?.Value;

                        // Ensure run properties exist and set NudiParijatha font
                        if (runProps == null)
                            runProps = run.RunProperties = new DocumentFormat.OpenXml.Wordprocessing.RunProperties();

                        runProps.RunFonts = new DocumentFormat.OpenXml.Wordprocessing.RunFonts()
                        {
                            Ascii = "NudiParijatha",
                            HighAnsi = "NudiParijatha",
                            ComplexScript = "NudiParijatha",
                            EastAsia = "NudiParijatha"
                        };

                        // Convert if: font is empty/none OR font starts with any keyword in nudiFontKeywords (case-insensitive)
                        // Only skip if font is explicitly set and doesn't match any keyword
                        if (!string.IsNullOrWhiteSpace(fontName) && !nudiFontKeywords.Any(k => fontName.ToLowerInvariant().StartsWith(k)))
                        {
                            skippedRuns++;
                            if (!loggedFonts.Contains(fontName))
                            {
                                SimpleLogger.Log($"Skipped run - Font: '{fontName}' (not a conversion font)");
                                loggedFonts.Add(fontName);
                            }
                            continue;
                        }

                        var textElements = run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                        if (!textElements.Any()) continue;

                        string combinedText = string.Concat(textElements.Select(t => t.Text));
                        if (string.IsNullOrEmpty(combinedText)) continue;

                        string convertedText;
                        try
                        {
                            convertedText = converter(combinedText);
                            convertedRuns++;
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            // Should be rare now after SDK fixes for ProcessArkavattu and ProcessVattakshara
                            SimpleLogger.LogWarning($"Converter error: Skipping problematic text to preserve document : {ex.Message}");
                            convertedText = combinedText;
                            skippedRuns++;
                            continue;
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.LogWarning($"Text conversion failed ({ex.GetType().Name}): {ex.Message}");
                            convertedText = combinedText;
                            skippedRuns++;
                            continue;
                        }

                        textElements[0].Text = convertedText;
                        textElements[0].Space = SpaceProcessingModeValues.Preserve;
                        for (int i = 1; i < textElements.Count; i++)
                            textElements[i].Text = "";
                    }
                    paragraphCount++;
                }

                SimpleLogger.Log($"Document section processed - Paragraphs: {paragraphCount}, Converted runs: {convertedRuns}, Skipped runs: {skippedRuns}");
                return paragraphCount;
            }
        }
    }
}
