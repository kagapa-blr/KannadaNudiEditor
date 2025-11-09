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

            if (!string.IsNullOrWhiteSpace(initialHeader))
                LoadTextIntoEditor(HeaderEditor, initialHeader);

            if (!string.IsNullOrWhiteSpace(initialFooter))
                LoadTextIntoEditor(FooterEditor, initialFooter);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            HeaderText = GetTextFromEditor(HeaderEditor);
            FooterText = GetTextFromEditor(FooterEditor);

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Loads text into the SfRichTextBoxAdv. Supports RTF (preferred) or plain text fallback.
        /// </summary>
        private void LoadTextIntoEditor(SfRichTextBoxAdv editor, string text)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(text));

            // Auto-detect format
            var format = text.TrimStart().StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase)
                ? FormatType.Rtf
                : FormatType.Txt;

            editor.Load(ms, format);
        }

        /// <summary>
        /// Gets text content from SfRichTextBoxAdv in RTF format (preserves formatting).
        /// </summary>
        private string GetTextFromEditor(SfRichTextBoxAdv editor)
        {
            using var ms = new MemoryStream();
            editor.Save(ms, FormatType.Rtf); // Always save as RTF to keep formatting
            ms.Position = 0;
            using var reader = new StreamReader(ms, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
