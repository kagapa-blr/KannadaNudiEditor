using System.Windows;

namespace KannadaNudiEditor.Views.HeaderFooter
{
    public partial class HeaderFooterEditor : Window
    {
        public string EditedText { get; private set; }

        public HeaderFooterEditor(string initialText = "")
        {
            InitializeComponent();
            TextInput.Text = initialText;
        }

        // OK button click - save the text and close the window
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            EditedText = TextInput.Text;
            DialogResult = true;
            Close();
        }
    }
}
