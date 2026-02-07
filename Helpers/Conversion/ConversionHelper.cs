using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Kannada.AsciiUnicode.Converters;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using Microsoft.Win32;
using System.Threading;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

namespace KannadaNudiEditor.Helpers.Conversion
{
    public static class ConversionHelper
    {
        private static readonly string TempFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"KannadaNudiBaraha\TempConverted");

        public static readonly KannadaConverter Converter;

        static ConversionHelper()
        {
            Directory.CreateDirectory(TempFolder);

            var customAsciiToUnicode = new System.Collections.Generic.Dictionary<string, string>
            {
                { "wÃPÀëÚ", "ತೀಕ್ಷ್ಣ" },
                { "PÀëÚ", "ಕ್ಷ್ಣ" },
                { "UÉÀ", "ಗೆ" }
            };

            var customUnicodeToAscii = new System.Collections.Generic.Dictionary<string, string>
            {
                { "ತೀಕ್ಷ್ಣ", "wÃPÀëÚ" },
                { "ಕ್ಷ್ಣ", "PÀëÚ" }
            };

            Converter = KannadaConverter.CreateWithCustomMapping(customAsciiToUnicode, customUnicodeToAscii);
        }

        // ============================================================
        // Convert file and return (tempPath, totalParagraphs)
        // ============================================================
        public static (string tempFile, int totalParagraphs) ConvertFileToTempWithParagraphCount(string inputPath, Func<string, string> converter)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Input file not found", inputPath);

            string fileName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath).ToLower();
            string uniqueFile = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmssfff}{ext}";
            string outputPath = Path.Combine(TempFolder, uniqueFile);

            SimpleLogger.Log($"Starting conversion: {inputPath} → {outputPath}");

            int paragraphCount = 0;

            switch (ext)
            {
                case ".txt":
                    ConvertTextFile(inputPath, outputPath, converter);
                    paragraphCount = 1; // single block for TXT
                    break;

                case ".docx":
                    paragraphCount = ConvertDocxFileWithParagraphCount(inputPath, outputPath, converter);
                    break;

                default:
                    throw new NotSupportedException($"File type {ext} is not supported.");
            }

            SimpleLogger.Log($"Conversion completed: {outputPath}, Paragraphs: {paragraphCount}");
            return (outputPath, paragraphCount);
        }

        private static void ConvertTextFile(string inputPath, string outputPath, Func<string, string> converter)
        {
            string content = File.ReadAllText(inputPath, Encoding.UTF8);
            string converted = converter(content);
            File.WriteAllText(outputPath, converted, Encoding.UTF8);
            SimpleLogger.Log($"TXT file converted. Length: {converted.Length} chars");
        }

        private static int ConvertDocxFileWithParagraphCount(string inputPath, string outputPath, Func<string, string> converter)
        {
            return DocxHelper.ConvertDocxWithParagraphCount(inputPath, outputPath, converter);
        }

        // ============================================================
        // Load file into SfRichTextBoxAdv
        // ============================================================
        public static async Task LoadFileIntoEditorAsync(string filePath, SfRichTextBoxAdv richTextBox, CancellationToken? cancellationToken = null)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Converted file not found", filePath);

            var ext = Path.GetExtension(filePath).ToLower();
            var formatType = GetFormatType(ext);

            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                if (cancellationToken.HasValue)
                    await richTextBox.LoadAsync(fs, formatType, cancellationToken.Value);
                else
                    await richTextBox.LoadAsync(fs, formatType);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, $"Failed to load {filePath} into editor");
                throw;
            }
        }

        private static FormatType GetFormatType(string ext)
        {
            return ext switch
            {
                ".txt" => FormatType.Txt,
                ".rtf" => FormatType.Rtf,
                ".xaml" => FormatType.Xaml,
                ".docx" => FormatType.Docx,
                ".doc" => FormatType.Doc,
                ".htm" or ".html" => FormatType.Html,
                _ => throw new NotSupportedException($"Unsupported file type: {ext}")
            };
        }

        // ============================================================
        // Internal DOCX helper with paragraph count
        // ============================================================
        private static class DocxHelper
        {
            public static int ConvertDocxWithParagraphCount(string inputPath, string outputPath, Func<string, string> converter)
            {
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException("Input DOCX not found", inputPath);

                File.Copy(inputPath, outputPath, true);
                var sw = Stopwatch.StartNew();

                using var doc = WordprocessingDocument.Open(outputPath, true);
                var mainPart = doc.MainDocumentPart;
                if (mainPart?.Document == null) return 0;

                int totalParagraphs = 0;
                totalParagraphs += ConvertBodyText(mainPart.Document.Body, converter);

                if (mainPart.HeaderParts != null)
                    foreach (var header in mainPart.HeaderParts)
                        totalParagraphs += ConvertBodyText(header.Header, converter);

                if (mainPart.FooterParts != null)
                    foreach (var footer in mainPart.FooterParts)
                        totalParagraphs += ConvertBodyText(footer.Footer, converter);

                if (mainPart.FootnotesPart != null)
                    totalParagraphs += ConvertBodyText(mainPart.FootnotesPart.Footnotes, converter);

                if (mainPart.EndnotesPart != null)
                    totalParagraphs += ConvertBodyText(mainPart.EndnotesPart.Endnotes, converter);

                mainPart.Document.Save();
                sw.Stop();

                SimpleLogger.Log($"DOCX converted. Paragraphs: {totalParagraphs}, Time: {sw.ElapsedMilliseconds} ms");
                return totalParagraphs;
            }

            private static int ConvertBodyText(OpenXmlElement? element, Func<string, string> converter)
            {
                if (element == null) return 0;

                int count = 0;
                foreach (var paragraph in element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                {
                    foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                    {
                        foreach (var text in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>())
                        {
                            if (string.IsNullOrWhiteSpace(text.Text)) continue;
                            text.Text = converter(text.Text);
                            text.Space = SpaceProcessingModeValues.Preserve;
                        }
                    }
                    count++;
                }
                return count;
            }
        }
    }
}
