using System;
using System.Diagnostics;
using System.IO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Office;
using Syncfusion.Pdf;
using Syncfusion.Windows.Controls.RichTextBoxAdv;

namespace KannadaNudiEditor.Helpers
{
    public static class DocumentExportHelper
    {
        // -------------------------
        // EXPORT TO PDF
        // -------------------------
        public static void ExportToPdf(SfRichTextBoxAdv richTextBox, string filePath, string primaryFont)
        {
            SimpleLogger.Log("========== ExportToPdf: START ==========");

            try
            {
                // -------------------------
                // STEP 1 — Save editor content to DOCX stream
                // -------------------------
                SimpleLogger.Log("STEP 1: Saving editor content to DOCX MemoryStream...");
                using MemoryStream docStream = new MemoryStream();
                richTextBox.Save(docStream, FormatType.Docx);
                SimpleLogger.Log($"STEP 1 DONE: DOCX stream size = {docStream.Length} bytes");

                docStream.Position = 0;
                SimpleLogger.Log("MemoryStream.Position reset to 0.");

                // -------------------------
                // STEP 2 — Load DOCX into WordDocument
                // -------------------------
                SimpleLogger.Log("STEP 2: Loading WordDocument from stream...");
                using WordDocument document =
                    new WordDocument(docStream, Syncfusion.DocIO.FormatType.Docx);
                SimpleLogger.Log("STEP 2 DONE: WordDocument loaded.");

                // -------------------------
                // STEP 3 — Apply dynamic Kannada fallback fonts
                // -------------------------
                SimpleLogger.Log("STEP 3: Adding Kannada fallback fonts...");

                string fallbackFonts = string.IsNullOrWhiteSpace(primaryFont)
                    ? "Nirmala UI, Tunga, Noto Sans Kannada"
                    : $"{primaryFont}, Nirmala UI, Tunga, Noto Sans Kannada";

                document.FontSettings.FallbackFonts.Add(
                    new FallbackFont(0x0C80, 0x0CFF, fallbackFonts)
                );

                SimpleLogger.Log($"STEP 3 DONE: Fallback fonts applied: {fallbackFonts}");

                // -------------------------
                // STEP 4 — Initialize PDF converter
                // -------------------------
                SimpleLogger.Log("STEP 4: Initializing DocToPDFConverter...");
                using DocToPDFConverter converter = new DocToPDFConverter();

                converter.Settings.AutoDetectComplexScript = true;
                SimpleLogger.Log("STEP 4 DONE: AutoDetectComplexScript = true");

                // -------------------------
                // STEP 5 — Convert DOCX → PDF
                // -------------------------
                SimpleLogger.Log("STEP 5: Converting DOCX to PDF...");
                using PdfDocument pdfDocument = converter.ConvertToPDF(document);
                SimpleLogger.Log("STEP 5 DONE: PDF document created in memory.");

                // -------------------------
                // STEP 6 — Save PDF to file
                // -------------------------
                SimpleLogger.Log($"STEP 6: Saving PDF to: {filePath}");
                pdfDocument.Save(filePath);
                SimpleLogger.Log("STEP 6 DONE: PDF saved successfully.");

                // -------------------------
                // STEP 7 — Open in Explorer
                // -------------------------
                SimpleLogger.Log("STEP 7: Opening PDF in Windows Explorer...");
                ShowFileInExplorer(filePath);
                SimpleLogger.Log("STEP 7 DONE: Explorer opened.");

                SimpleLogger.Log("========== ExportToPdf: COMPLETED SUCCESSFULLY ==========");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("========== ExportToPdf: ERROR OCCURRED ==========");
                SimpleLogger.Log($"EXCEPTION: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    SimpleLogger.Log(ex.StackTrace);
                throw;
            }
        }

        // -------------------------
        // EXPORT TO MARKDOWN
        // -------------------------
        public static void ExportToMarkdown(SfRichTextBoxAdv richTextBox, string filePath)
        {
            SimpleLogger.Log("========== ExportToMarkdown: START ==========");

            try
            {
                // STEP 1 — Save editor content to DOCX stream
                SimpleLogger.Log("STEP 1: Saving editor content to DOCX MemoryStream...");
                using MemoryStream docStream = new MemoryStream();
                richTextBox.Save(docStream, FormatType.Docx);
                SimpleLogger.Log($"STEP 1 DONE: DOCX stream size = {docStream.Length} bytes");

                docStream.Position = 0;
                SimpleLogger.Log("MemoryStream.Position reset to 0.");

                // STEP 2 — Load DOCX into WordDocument
                SimpleLogger.Log("STEP 2: Loading WordDocument from stream...");
                using WordDocument document = new WordDocument(docStream, Syncfusion.DocIO.FormatType.Docx);
                SimpleLogger.Log("STEP 2 DONE: WordDocument loaded.");

                // STEP 3 — Save WordDocument as Markdown
                SimpleLogger.Log($"STEP 3: Saving WordDocument as Markdown to: {filePath}");
                document.Save(filePath, Syncfusion.DocIO.FormatType.Markdown);
                SimpleLogger.Log("STEP 3 DONE: Markdown file saved successfully.");

                // STEP 4 — Open in Explorer
                SimpleLogger.Log("STEP 4: Opening Markdown file in Windows Explorer...");
                ShowFileInExplorer(filePath);
                SimpleLogger.Log("STEP 4 DONE: Explorer opened.");

                SimpleLogger.Log("========== ExportToMarkdown: COMPLETED SUCCESSFULLY ==========");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("========== ExportToMarkdown: ERROR OCCURRED ==========");
                SimpleLogger.Log($"EXCEPTION: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    SimpleLogger.Log(ex.StackTrace);
                throw;
            }
        }

        // -------------------------
        // EXPORT TO RTF
        // -------------------------
        public static void ExportToRtf(SfRichTextBoxAdv richTextBox, string filePath)
        {
            SimpleLogger.Log("========== ExportToRtf: START ==========");

            try
            {
                // STEP 1 — Save editor content to RTF MemoryStream
                SimpleLogger.Log("STEP 1: Saving editor content to RTF MemoryStream...");
                using MemoryStream rtfStream = new MemoryStream();
                richTextBox.Save(rtfStream, FormatType.Rtf);
                SimpleLogger.Log($"STEP 1 DONE: RTF stream size = {rtfStream.Length} bytes");

                // STEP 2 — Write RTF stream to file
                SimpleLogger.Log($"STEP 2: Writing RTF stream to file: {filePath}");
                File.WriteAllBytes(filePath, rtfStream.ToArray());
                SimpleLogger.Log("STEP 2 DONE: RTF file saved successfully.");

                // STEP 3 — Open the file in Explorer
                SimpleLogger.Log("STEP 3: Opening RTF file in Windows Explorer...");
                ShowFileInExplorer(filePath);
                SimpleLogger.Log("STEP 3 DONE: Explorer opened.");

                SimpleLogger.Log("========== ExportToRtf: COMPLETED SUCCESSFULLY ==========");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("========== ExportToRtf: ERROR OCCURRED ==========");
                SimpleLogger.Log($"EXCEPTION: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    SimpleLogger.Log(ex.StackTrace);
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
