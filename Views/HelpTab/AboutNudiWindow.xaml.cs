using System.Windows;

namespace KannadaNudiEditor.Views.HelpTab
{
    public partial class AboutNudiWindow : Window
    {
        public AboutNudiWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
