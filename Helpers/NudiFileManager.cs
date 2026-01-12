using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.IO;
using System.Windows;

namespace KannadaNudiEditor.Helpers
{
    public static class NudiFileManager
    {


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
