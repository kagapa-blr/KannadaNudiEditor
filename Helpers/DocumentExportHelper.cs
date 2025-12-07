using System;
using System.Diagnostics;
using System.IO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.Windows.Controls.RichTextBoxAdv;

namespace KannadaNudiEditor.Helpers
{
    public static class DocumentExportHelper
    {
        // -------------------------
        // EXPORT TO PDF
        // -------------------------
        public static void ExportToPdf(SfRichTextBoxAdv richTextBox, string filePath)
        {
            try
            {
                SimpleLogger.Log("ExportToPdf: Started");

                using MemoryStream docStream = new MemoryStream();
                richTextBox.Save(docStream, FormatType.Docx);
                docStream.Position = 0;

                using WordDocument document = new WordDocument(docStream, Syncfusion.DocIO.FormatType.Docx);
                using DocToPDFConverter converter = new DocToPDFConverter();
                using PdfDocument pdfDocument = converter.ConvertToPDF(document);

                pdfDocument.Save(filePath);

                SimpleLogger.Log($"ExportToPdf: Saved to {filePath}");
                ShowFileInExplorer(filePath);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ExportToPdf FAILED: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }


        // -------------------------
        // EXPORT TO MARKDOWN
        // -------------------------
        public static void ExportToMarkdown(SfRichTextBoxAdv richTextBox, string filePath)
        {
            try
            {
                SimpleLogger.Log("ExportToMarkdown: Started");

                using MemoryStream docStream = new MemoryStream();
                richTextBox.Save(docStream, FormatType.Docx);
                docStream.Position = 0;

                using WordDocument document = new WordDocument(docStream, Syncfusion.DocIO.FormatType.Docx);
                document.Save(filePath, Syncfusion.DocIO.FormatType.Markdown);

                SimpleLogger.Log($"ExportToMarkdown: Saved to {filePath}");
                ShowFileInExplorer(filePath);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ExportToMarkdown FAILED: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }


        // -------------------------
        // EXPORT TO RTF
        // -------------------------
        public static void ExportToRtf(SfRichTextBoxAdv richTextBox, string filePath)
        {
            try
            {
                SimpleLogger.Log("ExportToRtf: Started");

                using MemoryStream rtfStream = new MemoryStream();
                richTextBox.Save(rtfStream, FormatType.Rtf);

                File.WriteAllBytes(filePath, rtfStream.ToArray());

                SimpleLogger.Log($"ExportToRtf: Saved to {filePath}");
                ShowFileInExplorer(filePath);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ExportToRtf FAILED: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }


        // -------------------------
        // OPEN FILE IN EXPLORER
        // -------------------------
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

                    SimpleLogger.Log($"Explorer opened: {filePath}");
                }
                else
                {
                    SimpleLogger.Log($"ShowFileInExplorer: File not found {filePath}");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ShowFileInExplorer FAILED: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
