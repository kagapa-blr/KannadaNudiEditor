using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.IO;
using System.Windows;

namespace KannadaNudiEditor.Helpers
{
    public static class NudiFileManager
    {

        public static async Task<bool> SaveToFileAsync(
            string filePath,
            SfRichTextBoxAdv richTextBoxAdv,
            Action triggerSaveAs)
        {
            FileStream? stream = null;

            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    Console.WriteLine("[Save] File path is empty. Triggering Save As...");
                    triggerSaveAs?.Invoke();
                    return false;
                }

                string displayName = Path.GetFileName(filePath);
                Console.WriteLine($"[Save] Current file: {displayName}");

                GC.Collect();
                GC.WaitForPendingFinalizers();

                stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                FormatType formatType = GetFormatType(Path.GetExtension(filePath));
                await richTextBoxAdv.SaveAsync(stream, formatType);

                // ✅ Update document title in UI
                richTextBoxAdv.DocumentTitle = displayName;

                return true;
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"File is being used by another process:\n{ioEx.Message}", "File In Use", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save the file:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                stream?.Dispose();
            }
        }

        public static void SaveAs(string extension, Action<string> exportAction)
        {
            try
            {
                string ext = string.IsNullOrWhiteSpace(extension) ? ".docx" : extension;
                exportAction(ext);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save As failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static FormatType GetFormatType(string extension)
        {
            return extension.ToLower() switch
            {
                ".rtf" => FormatType.Rtf,
                ".txt" => FormatType.Txt,
                ".html" => FormatType.Html,
                _ => FormatType.Docx,
            };
        }
    }




}
