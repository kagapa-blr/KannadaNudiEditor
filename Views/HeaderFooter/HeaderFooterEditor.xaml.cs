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
                editor.Save(ms, FormatType.Rtf);
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


        /// <summary>
        /// Apply header and footer text to the main SfRichTextBoxAdv document.
        /// </summary>
        public void ApplyHeaderFooterToDocument(SfRichTextBoxAdv mainEditor, Action richTextBoxUpdateCallback)
        {
            var section = mainEditor.Document.Sections[0];

            if (section.HeaderFooters == null)
            {
                section.HeaderFooters = new HeaderFooters();
                SimpleLogger.Log("HeaderFooters object created.");
            }

            section.HeaderFooters.Header.Blocks.Clear();
            section.HeaderFooters.Footer.Blocks.Clear();

            if (!string.IsNullOrWhiteSpace(HeaderText))
            {
                SimpleLogger.Log("Applying header text from editor.");
                var tempHeader = new SfRichTextBoxAdv();
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(HeaderText)))
                {
                    tempHeader.Load(ms, FormatType.Rtf);
                }
                var srcSection = tempHeader.Document.Sections.FirstOrDefault() as SectionAdv;
                foreach (var block in srcSection?.Blocks.OfType<BlockAdv>().ToList() ?? new List<BlockAdv>())
                {
                    section.HeaderFooters.Header.Blocks.Add(block);
                }
            }
            else
            {
                SimpleLogger.Log("Empty header text, skipping.");
            }

            if (!string.IsNullOrWhiteSpace(FooterText))
            {
                SimpleLogger.Log("Applying footer text from editor.");
                var tempFooter = new SfRichTextBoxAdv();
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(FooterText)))
                {
                    tempFooter.Load(ms, FormatType.Rtf);
                }
                var srcSection = tempFooter.Document.Sections.FirstOrDefault() as SectionAdv;
                foreach (var block in srcSection?.Blocks.OfType<BlockAdv>().ToList() ?? new List<BlockAdv>())
                {
                    section.HeaderFooters.Footer.Blocks.Add(block);
                }
            }
            else
            {
                SimpleLogger.Log("Empty footer text, skipping.");
            }

            if (section.HeaderFooters.Header.Blocks.Count == 0 && section.HeaderFooters.Footer.Blocks.Count == 0)
            {
                section.HeaderFooters = null;
                SimpleLogger.Log("No header or footer entered, HeaderFooters cleared.");
                MessageBox.Show("No header or footer text entered.");
            }
            else
            {
                section.SectionFormat.HeaderDistance = 50;
                section.SectionFormat.FooterDistance = 50;
                SimpleLogger.Log("Header/Footer distances set to 50.");
            }

            if (richTextBoxUpdateCallback != null)
                richTextBoxUpdateCallback();
            else
                SimpleLogger.Log("No UI refresh callback provided.");
        }
    }
}
