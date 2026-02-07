using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Kannada.AsciiUnicode.Converters;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Windows;
using System.Windows.Media;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class ConversionHelper
    {
        private static readonly string TempFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"KannadaNudiBaraha\TempConverted");

        // Singleton converter
        private static KannadaConverter? _converter;

        /// <summary>
        /// Get singleton converter instance (loads JSON once)
        /// </summary>
        public static KannadaConverter Converter => _converter ??= LoadConverter();

        // ============================================================
        // Load KannadaConverter with JSON mappings
        // ============================================================
        private static KannadaConverter LoadConverter()
        {
            Directory.CreateDirectory(TempFolder);

            string asciiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "AsciiToUnicodeMapping.json");
            string unicodePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "UnicodeToAsciiMapping.json");

            var asciiMap = LoadMapping(asciiPath);
            var unicodeMap = LoadMapping(unicodePath);

            SimpleLogger.Log($"Custom mappings loaded | ASCII→Unicode={asciiMap.Count}, Unicode→ASCII={unicodeMap.Count}");

            return KannadaConverter.CreateWithCustomMapping(asciiMap, unicodeMap);
        }

        // ============================================================
        // Load JSON mapping into dictionary
        // ============================================================
        private static Dictionary<string, string> LoadMapping(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SimpleLogger.Log($"Mapping JSON not found: {filePath}, returning empty mapping");
                return new Dictionary<string, string>();
            }

            try
            {
                string json = File.ReadAllText(filePath, Encoding.UTF8);
                var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                return map ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"Failed to load mapping JSON: {filePath}");
                return new Dictionary<string, string>();
            }
        }

        // ============================================================
        // Convert file (TXT/DOCX) and return temp path + paragraph count
        // ============================================================
        public static (string tempFile, int totalParagraphs) ConvertFileToTempWithParagraphCount(
            string inputPath, Func<string, string> converter)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Input file not found", inputPath);

            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath).ToLower();
            string tempFile = Path.Combine(TempFolder, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmssfff}{ext}");

            SimpleLogger.Log($"Starting conversion: {inputPath} → {tempFile}");

            int paragraphCount = ext switch
            {
                ".txt" => ConvertTextFile(inputPath, tempFile, converter),
                ".docx" => ConvertDocxFileWithParagraphCount(inputPath, tempFile, converter),
                _ => throw new NotSupportedException($"File type {ext} is not supported.")
            };

            SimpleLogger.Log($"Conversion completed: {tempFile}, Paragraphs: {paragraphCount}");
            return (tempFile, paragraphCount);
        }

        private static int ConvertTextFile(string inputPath, string outputPath, Func<string, string> converter)
        {
            string content = File.ReadAllText(inputPath, Encoding.UTF8);
            string converted = converter(content);
            File.WriteAllText(outputPath, converted, Encoding.UTF8);
            return 1; // single block for TXT
        }

        private static int ConvertDocxFileWithParagraphCount(string inputPath, string outputPath, Func<string, string> converter)
        {
            return DocxHelper.ConvertDocxWithParagraphCount(inputPath, outputPath, converter);
        }

        // ============================================================
        // Async load into SfRichTextBoxAdv (preserves formatting)
        // ============================================================
        public static async Task LoadFileIntoEditorAsync(string filePath, SfRichTextBoxAdv richTextBox, CancellationToken? cancellationToken = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Converted file not found", filePath);

            var format = GetFormatType(Path.GetExtension(filePath));
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            if (cancellationToken.HasValue)
                await richTextBox.LoadAsync(fs, format, cancellationToken.Value);
            else
                await richTextBox.LoadAsync(fs, format);
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

        // ============================================================
        // DOCX helper with font/format preservation
        // ============================================================
        private static class DocxHelper
        {
            public static int ConvertDocxWithParagraphCount(string inputPath, string outputPath, Func<string, string> converter)
            {
                File.Copy(inputPath, outputPath, true);
                using var doc = WordprocessingDocument.Open(outputPath, true);
                var main = doc.MainDocumentPart;
                if (main?.Document == null) return 0;

                int totalParagraphs = 0;
                totalParagraphs += ConvertBody(main.Document.Body, converter);

                if (main.HeaderParts != null)
                    foreach (var header in main.HeaderParts)
                        totalParagraphs += ConvertBody(header.Header, converter);

                if (main.FooterParts != null)
                    foreach (var footer in main.FooterParts)
                        totalParagraphs += ConvertBody(footer.Footer, converter);

                if (main.FootnotesPart != null)
                    totalParagraphs += ConvertBody(main.FootnotesPart.Footnotes, converter);

                if (main.EndnotesPart != null)
                    totalParagraphs += ConvertBody(main.EndnotesPart.Endnotes, converter);

                main.Document.Save();
                return totalParagraphs;
            }
            private static int ConvertBody(OpenXmlElement? element, Func<string, string> converter, string[]? nudiFontKeywords = null)
            {
                if (element == null) return 0;
                nudiFontKeywords ??= new[] { "nudi", "parijatha", "lipi" };

                int paragraphCount = 0;

                foreach (var para in element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    foreach (var run in para.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                    {
                        var runProps = run.RunProperties;
                        string? fontName = runProps?.RunFonts?.Ascii?.Value;

                        // Skip runs that are not Nudi font
                        if (string.IsNullOrWhiteSpace(fontName) || !nudiFontKeywords.Any(k => fontName.ToLower().Contains(k)))
                            continue;

                        var textElements = run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                        if (!textElements.Any()) continue;

                        // Combine all text in this run
                        string combinedText = string.Concat(textElements.Select(t => t.Text));

                        // Convert only Nudi text
                        string convertedText;
                        try
                        {
                            convertedText = converter(combinedText);
                        }
                        catch
                        {
                            convertedText = combinedText; // fallback if converter fails
                        }

                        // Assign converted text back to first Text element, clear others
                        textElements[0].Text = convertedText;
                        textElements[0].Space = SpaceProcessingModeValues.Preserve;
                        for (int i = 1; i < textElements.Count; i++)
                            textElements[i].Text = "";
                    }

                    paragraphCount++;
                }

                return paragraphCount;
            }


        }
    }
}
