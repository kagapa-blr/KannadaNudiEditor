using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;

namespace KannadaNudiEditor.Views.SortHelp
{
    public partial class SortHelpWindow : Window
    {
        public SortHelpWindow(bool isEnglish)
        {
            InitializeComponent();

            if (isEnglish)
            {
                Title = "Sort Help";
                SetFormattedHelpText(
@"**SORT BY:**
If sorting a list, choose Paragraphs, Headings, or Fields.
If sorting a table and you have a header row, choose by header name.
If you don't have a header, choose by column number.

**TYPE:**
Choose between Text, Number, or Date.

**USING:**
Choose Paragraphs.

**ASCENDING OR DESCENDING:**
Choose sorting method.

**MY LIST HAS:**
Choose whether your list or table has headers.

**OPTIONS:**
• Where to separate fields (Tabs, Comma, Other)
• Sort options (Sort column only, Case sensitive)
• Sorting language");
            }
            else
            {
                Title = "ವಿಂಗಡನೆ ಸಹಾಯ";
                SetFormattedHelpText(
@"**ವಿಂಗಡಿಸುವ ವಿಧಾನ:**
ಪಟ್ಟಿ ಅಥವಾ ಟೇಬಲ್ ಅನ್ನು ವಿಂಗಡಿಸಲು ಪ್ಯಾರಾಗ್ರಾಫ್‌ಗಳು, ಶೀರ್ಷಿಕೆಗಳು, ಅಥವಾ ಕ್ಷೇತ್ರಗಳು ಆಯ್ಕೆಮಾಡಿ.
ಹೆಡರ್ ಸಾಲು ಇದ್ದರೆ ಶೀರ್ಷಿಕೆಯ ಹೆಸರಿನಿಂದ ಆಯ್ಕೆಮಾಡಿ. ಇಲ್ಲದಿದ್ದರೆ ಕಾಲಮ್ ಸಂಖ್ಯೆಯಿಂದ ಆಯ್ಕೆಮಾಡಿ.

**ಪ್ರಕಾರ:**
ಪಠ್ಯ, ಸಂಖ್ಯೆ, ಅಥವಾ ದಿನಾಂಕ ಆಯ್ಕೆಮಾಡಿ.

**ಬಳಕೆಮಾಡುವುದು:**
ಪ್ಯಾರಾಗ್ರಾಫ್‌ಗಳನ್ನು ಆಯ್ಕೆಮಾಡಿ.

**ಏರಿಕ್ರಮ ಅಥವಾ ಇಳಿಕ್ರಮ:**
ವಿಂಗಡಿಸುವ ಕ್ರಮವನ್ನು ಆಯ್ಕೆಮಾಡಿ.

**ನನ್ನ ಪಟ್ಟಿಯಲ್ಲಿ ಇದೆ:**
ಪಟ್ಟಿಯಲ್ಲಿ ಹೆಡರ್ ಸಾಲು ಇದ್ದರೆ ಆಯ್ಕೆಮಾಡಿ.

**ಆಯ್ಕೆಗಳು:**
• ಕ್ಷೇತ್ರಗಳನ್ನು ವಿಭಜಿಸುವ ವಿಧಾನ (ಟ್ಯಾಬ್, ಅಲ್ಪವಿರಾಮ, ಇತರೆ)
• ವಿಂಗಡನಾ ಆಯ್ಕೆಗಳು (ಕೇವಲ ಕಾಲಮ್, ಕೇಸ್ ಸೆನ್ಸಿಟಿವ್)
• ಭಾಷೆ ಆಧಾರಿತ ವಿಂಗಡನೆ");
            }
        }

        private void SetFormattedHelpText(string formattedText)
        {
            var doc = new FlowDocument();
            var lines = formattedText.Split(new[] { '\n' }, System.StringSplitOptions.None);
            var boldRegex = new Regex(@"\*\*(.+?)\*\*");

            foreach (var line in lines)
            {
                var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 5) };
                var lastIndex = 0;

                foreach (Match match in boldRegex.Matches(line))
                {
                    // Add text before bold
                    if (match.Index > lastIndex)
                    {
                        paragraph.Inlines.Add(new Run(line.Substring(lastIndex, match.Index - lastIndex)));
                    }

                    // Add bold text
                    var boldText = match.Groups[1].Value;
                    paragraph.Inlines.Add(new Bold(new Run(boldText)));

                    lastIndex = match.Index + match.Length;
                }

                // Add remaining text after last bold
                if (lastIndex < line.Length)
                {
                    paragraph.Inlines.Add(new Run(line.Substring(lastIndex)));
                }

                doc.Blocks.Add(paragraph);
            }

            HelpViewer.Document = doc;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
