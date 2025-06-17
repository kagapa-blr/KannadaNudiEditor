using System.Windows;
using KannadaNudiEditor.Views.SortHelp; // ✅ Correct namespace

namespace KannadaNudiEditor.Views.Sort
{
    public partial class SortWindow : Window
    {
        private readonly bool isEnglish;
        private SortHelpWindow helpWindow;

        public SortWindow(bool isEnglishLanguage)
        {
            InitializeComponent();
            isEnglish = isEnglishLanguage;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string message = isEnglish
                ? "Sort settings applied (dummy action)."
                : "ವಿಂಗಡನಾ ಸೆಟ್ಟಿಂಗ್‌ಗಳು ಅನ್ವಯಿಸಲ್ಪಟ್ಟಿವೆ (ಡಮ್ಮಿ ಕ್ರಿಯೆ).";

            string caption = isEnglish ? "Sort" : "ವಿಂಗಡನೆ";

            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
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
