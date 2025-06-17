using System.Windows;

namespace KannadaNudiEditor.Views.Sort
{
    public partial class SortWindow : Window
    {
        private readonly bool isEnglish;

        public SortWindow(bool isEnglishLanguage)
        {
            InitializeComponent();
            isEnglish = isEnglishLanguage;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(isEnglish ? "Sort settings applied (dummy action)." : "ವಿಂಗಡನಾ ಸೆಟ್ಟಿಂಗ್‌ಗಳು ಅನ್ವಯಿಸಲ್ಪಟ್ಟಿವೆ (ಡಮ್ಮಿ ಕ್ರಿಯೆ).");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            string message;
            string caption;

            if (isEnglish)
            {
                caption = "Sort Help";
                message =
@"**Sort By**
If sorting a list, choose Paragraphs, Headings, or Fields.
If sorting a table and you have a header row, choose by header name.
If you don't have a header, choose by column number.

**Type**
Choose between Text, Number, or Date.

**Using**
Choose Paragraphs.

**Ascending or Descending**
Choose sorting method.

**My list has**
Choose whether your list or table has headers.

**Options**
Choose sort options.

• Where to separate fields (Tabs, Comma, Other)
• Sort options (Sort column only, Case sensitive)
• Sorting language";
            }
            else
            {
                caption = "ವಿಂಗಡನೆ ಸಹಾಯ";
                message =
@"**ವಿಂಗಡಿಸುವ ವಿಧಾನ**
ಪಟ್ಟಿ ಅಥವಾ ಟೇಬಲ್ ಅನ್ನು ವಿಂಗಡಿಸಲು 'ಪ್ಯಾರಾಗ್ರಾಫ್‌ಗಳು', 'ಶೀರ್ಷಿಕೆಗಳು', ಅಥವಾ 'ಕ್ಷೇತ್ರಗಳು' ಆಯ್ಕೆಮಾಡಿ.
ಹೆಡರ್ ಸಾಲು ಇದ್ದರೆ ಶೀರ್ಷಿಕೆಯ ಹೆಸರಿನಿಂದ ಆಯ್ಕೆಮಾಡಿ. ಇಲ್ಲದಿದ್ದರೆ ಕಾಲಮ್ ಸಂಖ್ಯೆಯಿಂದ ಆಯ್ಕೆಮಾಡಿ.

**ಪ್ರಕಾರ**
ಪಠ್ಯ, ಸಂಖ್ಯೆ, ಅಥವಾ ದಿನಾಂಕ ಆಯ್ಕೆಮಾಡಿ.

**ಬಳಕೆಮಾಡುವುದು**
ಪ್ಯಾರಾಗ್ರಾಫ್‌ಗಳನ್ನು ಆಯ್ಕೆಮಾಡಿ.

**ಏರಿಕ್ರಮ ಅಥವಾ ಇಳಿಕ್ರಮ**
ವಿಂಗಡಿಸುವ ಕ್ರಮವನ್ನು ಆಯ್ಕೆಮಾಡಿ.

**ನನ್ನ ಪಟ್ಟಿಯಲ್ಲಿ ಇದೆ**
ಪಟ್ಟಿಯಲ್ಲಿ ಹೆಡರ್ ಸಾಲು ಇದ್ದರೆ ಆಯ್ಕೆಮಾಡಿ.

**ಆಯ್ಕೆಗಳು**
• ಕ್ಷೇತ್ರಗಳನ್ನು ವಿಭಜಿಸುವ ವಿಧಾನ (ಟ್ಯಾಬ್, ಅಲ್ಪವಿರಾಮ, ಇತರೆ)
• ವಿಂಗಡನಾ ಆಯ್ಕೆಗಳು (ಕೇವಲ ಕಾಲಮ್, ಕೇಸ್ ಸೆನ್ಸಿಟಿವ್)
• ಭಾಷೆ ಆಧಾರಿತ ವಿಂಗಡನೆ";
            }

            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
