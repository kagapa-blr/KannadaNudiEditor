using System;
using System.IO;
using System.Text;
using System.Windows;
using Syncfusion.Windows.Controls.RichTextBoxAdv;

namespace KannadaNudiEditor.Views.HeaderFooter
{
    public partial class HeaderFooterEditor : Window
    {
        public string HeaderText { get; private set; } = "";
        public string FooterText { get; private set; } = "";

        public HeaderFooterEditor(string? initialHeader, string? initialFooter)
        {
            InitializeComponent();
            SimpleLogger.Log("HeaderFooterEditor initialized.");

            try
            {
                if (!string.IsNullOrWhiteSpace(initialHeader))
                {
                    SimpleLogger.Log("Loading initial header...");
                    LoadTextIntoEditor(HeaderEditor, initialHeader);
                    SimpleLogger.Log("Header loaded successfully.");
                }

                if (!string.IsNullOrWhiteSpace(initialFooter))
                {
                    SimpleLogger.Log("Loading initial footer...");
                    LoadTextIntoEditor(FooterEditor, initialFooter);
                    SimpleLogger.Log("Footer loaded successfully.");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error initializing HeaderFooterEditor: {ex}");
            }
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HeaderText = GetTextFromEditor(HeaderEditor);
                FooterText = GetTextFromEditor(FooterEditor);
                SimpleLogger.Log("Apply clicked. Header and footer text captured.");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error applying header/footer: {ex}");
                MessageBox.Show("Failed to apply header/footer. See logs for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleLogger.Log("Cancel clicked. HeaderFooterEditor closed without saving.");
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Loads text into the SfRichTextBoxAdv. Supports RTF (preferred) or plain text fallback.
        /// </summary>
        private void LoadTextIntoEditor(SfRichTextBoxAdv editor, string text)
        {
            try
            {
                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));

                // Auto-detect format
                var format = text.TrimStart().StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase)
                    ? FormatType.Rtf
                    : FormatType.Txt;

                SimpleLogger.Log($"Loading text into editor. Format detected: {format}");
                editor.Load(ms, format);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error loading text into editor: {ex}");
            }
        }

        /// <summary>
        /// Gets text content from SfRichTextBoxAdv in RTF format (preserves formatting).
        /// </summary>
        private string GetTextFromEditor(SfRichTextBoxAdv editor)
        {
            try
            {
                using var ms = new MemoryStream();
                editor.Save(ms, FormatType.Rtf); // Always save as RTF to keep formatting
                ms.Position = 0;
                using var reader = new StreamReader(ms, Encoding.UTF8);
                var result = reader.ReadToEnd();
                SimpleLogger.Log($"Text retrieved from editor. Length: {result.Length} chars.");
                return result;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error retrieving text from editor: {ex}");
                return string.Empty;
            }
        }
    }
}
