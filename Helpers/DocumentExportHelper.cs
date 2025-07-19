using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.Windows.Controls.RichTextBoxAdv;

namespace KannadaNudiEditor.Helpers
{
    public static class DocumentExportHelper
    {



        public static void ExportToPdf(SfRichTextBoxAdv richTextBox, string filePath)
        {
            using MemoryStream docStream = new MemoryStream();
            richTextBox.Save(docStream, Syncfusion.Windows.Controls.RichTextBoxAdv.FormatType.Docx);
            docStream.Position = 0;

            using WordDocument document = new(docStream, Syncfusion.DocIO.FormatType.Docx);
            using DocToPDFConverter converter = new DocToPDFConverter();
            using PdfDocument pdfDocument = converter.ConvertToPDF(document);
            pdfDocument.Save(filePath);

            ShowFileInExplorer(filePath);
        }


        public static void ExportToMarkdown(SfRichTextBoxAdv richTextBox, string filePath)
        {
            try
            {
                using MemoryStream docStream = new MemoryStream();
                richTextBox.Save(docStream, Syncfusion.Windows.Controls.RichTextBoxAdv.FormatType.Docx);
                docStream.Position = 0;

                using WordDocument document = new WordDocument(docStream, Syncfusion.DocIO.FormatType.Docx);
                document.Save(filePath, Syncfusion.DocIO.FormatType.Markdown);

                MessageBox.Show("Markdown exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                ShowFileInExplorer(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export Markdown:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private static void ShowFileInExplorer(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string args = $"/select,\"{filePath}\"";
                    Process.Start(new ProcessStartInfo("explorer.exe", args)
                    {
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open File Explorer:\n{ex.Message}", "Explorer Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
