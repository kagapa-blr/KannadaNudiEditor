using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KannadaNudiEditor.Helpers
{
    public static class NudiFileManager
    {
        // ============================================================
        // Convert result model (used by MainWindow.ConvertFileAsync)
        // ============================================================
        public sealed record ConvertResult(DocumentAdv Document, int ConvertedParagraphs);

        // ============================================================
        // Convert: file -> DocumentAdv (UI-thread safe)
        // ============================================================
        public static Task<ConvertResult> ConvertFileToDocumentAsync(
            string filePath,
            Func<string, string> converter,
            string fontFamilyName = "NudiParijatha",
            bool applyA4NormalMargins = true)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));
            ArgumentNullException.ThrowIfNull(converter);

            string fileName = Path.GetFileName(filePath);
            SimpleLogger.Log($"Convert - started: {fileName}");

            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var tempEditor = new SfRichTextBoxAdv();

                // Load
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var format = GetFormatType(Path.GetExtension(filePath));
                    SimpleLogger.Log($"Convert - loading: {fileName} | format={format}");
                    tempEditor.Load(fs, format);
                }

                // Convert
                int paraCount = ConvertDocumentInPlace(tempEditor.Document, converter, fontFamilyName);

                // Page setup
                if (applyA4NormalMargins)
                    ApplyA4NormalMargins(tempEditor.Document);

                SimpleLogger.Log($"Convert - completed: {fileName} | paragraphs={paraCount}");
                return new ConvertResult(tempEditor.Document, paraCount);

            }).Task;
        }

        // ============================================================
        // Save: editor -> file
        // ============================================================
        public static async Task<bool> SaveToFileAsync(
            string filePath,
            SfRichTextBoxAdv richTextBoxAdv,
            Action triggerSaveAs)
        {
            ArgumentNullException.ThrowIfNull(richTextBoxAdv);

            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    SimpleLogger.Log("[Save] File path is empty. Triggering Save As...");
                    triggerSaveAs?.Invoke();
                    return false;
                }

                string displayName = Path.GetFileName(filePath);
                var formatType = GetFormatType(Path.GetExtension(filePath));

                SimpleLogger.Log($"[Save] Saving file: {filePath} | format={formatType}");

                await using var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.ReadWrite,
                    bufferSize: 64 * 1024,
                    options: FileOptions.Asynchronous);

                await richTextBoxAdv.SaveAsync(stream, formatType);

                // Update document title in UI
                richTextBoxAdv.DocumentTitle = displayName;

                SimpleLogger.Log($"[Save] Completed: {displayName}");
                return true;
            }
            catch (IOException ioEx)
            {
                SimpleLogger.LogException(ioEx, "[Save] File in use");
                MessageBox.Show($"File is being used by another process:\n{ioEx.Message}",
                    "File In Use", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[Save] Failed");
                MessageBox.Show($"Failed to save the file:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ============================================================
        // SaveAs helper (keeps your existing pattern)
        // ============================================================
        public static void SaveAs(string extension, Action<string> exportAction)
        {
            try
            {
                string ext = string.IsNullOrWhiteSpace(extension) ? ".docx" : extension;
                SimpleLogger.Log($"[SaveAs] Requested extension: {ext}");
                exportAction(ext);
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[SaveAs] Failed");
                MessageBox.Show($"Save As failed:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // Internals: paragraph conversion (your existing behavior)
        // ============================================================
        private static int ConvertDocumentInPlace(
            DocumentAdv doc,
            Func<string, string> converter,
            string fontFamilyName)
        {
            int paraCount = 0;

            foreach (SectionAdv section in doc.Sections)
            {
                foreach (BlockAdv block in section.Blocks)
                {
                    if (block is not ParagraphAdv para || para.Inlines.Count == 0)
                        continue;

                    var spans = para.Inlines
                        .OfType<SpanAdv>()
                        .Where(s => !string.IsNullOrEmpty(s.Text))
                        .ToList();

                    if (spans.Count == 0) continue;

                    string originalText = string.Concat(spans.Select(s => s.Text));
                    if (string.IsNullOrEmpty(originalText)) continue;

                    string converted = converter(originalText);

                    var firstSpan = spans[0];
                    para.Inlines.Clear();

                    var newSpan = new SpanAdv { Text = converted };

                    // preserve formatting from first span
                    newSpan.CharacterFormat.FontSize = firstSpan.CharacterFormat.FontSize;
                    newSpan.CharacterFormat.Bold = firstSpan.CharacterFormat.Bold;
                    newSpan.CharacterFormat.Italic = firstSpan.CharacterFormat.Italic;
                    newSpan.CharacterFormat.Underline = firstSpan.CharacterFormat.Underline;
                    newSpan.CharacterFormat.FontColor = firstSpan.CharacterFormat.FontColor;

                    // Kannada font
                    newSpan.CharacterFormat.FontFamily =
                        new System.Windows.Media.FontFamily(fontFamilyName);

                    para.Inlines.Add(newSpan);
                    paraCount++;
                }
            }

            return paraCount;
        }

        // ============================================================
        // Internals: A4 + normal margins (1 inch)
        // ============================================================
        private static void ApplyA4NormalMargins(DocumentAdv doc)
        {
            const double dpi = 96.0;
            double a4WidthPx = 8.3 * dpi;
            double a4HeightPx = 11.7 * dpi;
            double marginPx = 1.0 * dpi;

            if (doc.Sections.Count == 0)
                doc.Sections.Add(new SectionAdv());

            foreach (SectionAdv s in doc.Sections.OfType<SectionAdv>())
            {
                s.SectionFormat.PageSize = new Size(a4WidthPx, a4HeightPx);
                s.SectionFormat.PageMargin = new Thickness(marginPx);
            }

            SimpleLogger.Log("PageSetup - applied A4 + normal margins");
        }

        // ============================================================
        // Internals: extension -> Syncfusion FormatType
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
                _ => FormatType.Docx,
            };
        }
    }
}
