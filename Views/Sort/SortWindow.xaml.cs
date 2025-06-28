using System.Windows;
using KannadaNudiEditor.Views.SortHelp;
using Syncfusion.Windows.Controls.RichTextBoxAdv; // ✅ Correct namespace for WPF SfRichTextBoxAdv

namespace KannadaNudiEditor.Views.Sort
{
    public partial class SortWindow : Window
    {
        private readonly bool isEnglish;
        private readonly SfRichTextBoxAdv richTextBoxAdv; // ✅ Using correct type
        private SortHelpWindow? helpWindow;

        public SortWindow(bool isEnglishLanguage, SfRichTextBoxAdv richTextBox)
        {
            InitializeComponent();
            isEnglish = isEnglishLanguage;
            richTextBoxAdv = richTextBox;
        }



        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string paragraphText = "";

            // Get the start position of the selection
            TextPosition start = richTextBoxAdv.Selection.Start;

            // Get the paragraph at the start of the selection
            ParagraphAdv paragraph = start.Paragraph as ParagraphAdv;

            if (paragraph != null)
            {
                foreach (var inline in paragraph.Inlines)
                {
                    if (inline is SpanAdv span)
                        paragraphText += span.Text;
                }
            }

            string caption = isEnglish ? "Selected Paragraph" : "ಆಯ್ದ ಪ್ಯಾರಾಗ್ರಾಫ್";
            MessageBox.Show(paragraphText, caption, MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
        }











        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (helpWindow == null || !helpWindow.IsVisible)
            {
                helpWindow = new SortHelpWindow(isEnglish)
                {
                    Owner = this
                };
                helpWindow.ShowDialog();
            }
            else
            {
                helpWindow.Activate();
            }
        }
    }
}
