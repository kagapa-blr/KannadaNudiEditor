using System.IO;
using System.Text;
using System.Text.Json;
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

        public static KannadaConverter Converter => _converter ??= LoadConverter();

        private static KannadaConverter LoadConverter()
        {
            Directory.CreateDirectory(TempFolder);

            string asciiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "AsciiToUnicodeMapping.json");
            string unicodePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "UnicodeToAsciiMapping.json");

            var asciiMap = LoadMapping(asciiPath);
            var unicodeMap = LoadMapping(unicodePath);

            SimpleLogger.Log($"Custom mappings loaded. ASCII→Unicode: {asciiMap.Count} mappings, Unicode→ASCII: {unicodeMap.Count} mappings");

            return KannadaConverter.CreateWithCustomMapping(asciiMap, unicodeMap);
        }

        private static Dictionary<string, string> LoadMapping(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SimpleLogger.LogWarning($"Mapping file not found: {filePath}. Using empty mapping.");
                return new Dictionary<string, string>();
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                SimpleLogger.Log($"Loaded mapping from {Path.GetFileName(filePath)}: {map?.Count ?? 0} entries");
                return map ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"Failed to load mapping file: {filePath}");
                return new Dictionary<string, string>();
            }
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

            if (cancellationToken.HasValue)
                await richTextBox.LoadAsync(fs, format, cancellationToken.Value);
            else
                await richTextBox.LoadAsync(fs, format);

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

                int totalParagraphs = 0;
                totalParagraphs += ConvertBody(main.Document.Body, converter);

                if (main.HeaderParts != null)
                {
                    foreach (var header in main.HeaderParts)
                        totalParagraphs += ConvertBody(header.Header, converter);
                }

                if (main.FooterParts != null)
                {
                    foreach (var footer in main.FooterParts)
                        totalParagraphs += ConvertBody(footer.Footer, converter);
                }

                if (main.FootnotesPart != null)
                    totalParagraphs += ConvertBody(main.FootnotesPart.Footnotes, converter);

                if (main.EndnotesPart != null)
                    totalParagraphs += ConvertBody(main.EndnotesPart.Endnotes, converter);

                main.Document.Save();
                SimpleLogger.Log($"DOCX conversion completed. Total paragraphs processed: {totalParagraphs}");
                return totalParagraphs;
            }

            private static int ConvertBody(OpenXmlElement? element, Func<string, string> converter, string[]? nudiFontKeywords = null)
            {
                if (element == null) return 0;
                nudiFontKeywords ??= new[] { "nudi", "parijatha", "lipi" };

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

                        if (string.IsNullOrWhiteSpace(fontName) || !nudiFontKeywords.Any(k => fontName.ToLowerInvariant().Contains(k)))
                        {
                            skippedRuns++;
                            SimpleLogger.Log($"Skipped run - Font: '{fontName ?? "none"}'");
                            continue;
                        }

                        var textElements = run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                        if (!textElements.Any()) continue;

                        string combinedText = string.Concat(textElements.Select(t => t.Text));
                        string convertedText;
                        try
                        {
                            convertedText = converter(combinedText);
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.LogWarning($"Text conversion failed for run, using original text: {ex.Message}");
                            convertedText = combinedText;
                        }

                        textElements[0].Text = convertedText;
                        textElements[0].Space = SpaceProcessingModeValues.Preserve;
                        for (int i = 1; i < textElements.Count; i++)
                            textElements[i].Text = "";

                        convertedRuns++;
                    }
                    paragraphCount++;
                }

                SimpleLogger.Log($"Document section processed - Paragraphs: {paragraphCount}, Converted runs: {convertedRuns}, Skipped runs: {skippedRuns}");
                return paragraphCount;
            }
        }
    }
}
