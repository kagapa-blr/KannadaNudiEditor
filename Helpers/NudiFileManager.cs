using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.IO;
using System.Windows;

namespace KannadaNudiEditor.Helpers
{
    public static class NudiFileManager
    {
        // ============================================================
        // Save file (supports .nudi using OpenXML)
        // ============================================================
        public static async Task<bool> SaveToFileAsync(
            string filePath,
            SfRichTextBoxAdv richTextBoxAdv,
            Action? triggerSaveAs) // nullable

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

                if (Path.GetExtension(filePath).Equals(".nudi", StringComparison.OrdinalIgnoreCase))
                {
                    // Save as .nudi using OpenXML
                    await SaveNudiOpenXmlAsync(filePath, richTextBoxAdv);
                }
                else
                {
                    // Use Syncfusion SaveAsync for other formats
                    var formatType = GetFormatType(Path.GetExtension(filePath));
                    await using var stream = new FileStream(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.ReadWrite,
                        64 * 1024,
                        FileOptions.Asynchronous);
                    await richTextBoxAdv.SaveAsync(stream, formatType);
                }

                richTextBoxAdv.DocumentTitle = displayName;
                SimpleLogger.Log($"[Save] Completed: {displayName}");
                return true;
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "[Save] Failed");
                MessageBox.Show($"Failed to save the file:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // ============================================================
        // SaveAs helper
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
                MessageBox.Show($"Save As failed:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        // OpenXML save for .nudi
        // ============================================================
        private static async Task SaveNudiOpenXmlAsync(string filePath, SfRichTextBoxAdv richTextBoxAdv)
        {
            using MemoryStream ms = new MemoryStream();
            // Save content as DOCX into memory stream
            await richTextBoxAdv.SaveAsync(ms, FormatType.Docx);
            ms.Position = 0;

            // Write memory stream to file with .nudi extension
            await using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 64 * 1024, FileOptions.Asynchronous);
            await ms.CopyToAsync(fs);
        }

        // ============================================================
        // Map file extension -> Syncfusion FormatType
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
                ".nudi" => FormatType.Docx,
                _ => FormatType.Docx,
            };
        }
    }
}
