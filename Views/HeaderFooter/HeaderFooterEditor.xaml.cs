using System.Windows;

namespace KannadaNudiEditor.Views.HeaderFooter
{
    public partial class HeaderFooterEditor : Window
    {
        public string HeaderText { get; private set; }
        public string FooterText { get; private set; }

        public HeaderFooterEditor()  // Default constructor (no parameters)
        {
            InitializeComponent();
        }

        // Handle Apply button click to update the header/footer
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the header and footer texts
            HeaderText = HeaderTextBox.Text;
            FooterText = FooterTextBox.Text;

            // Validate that both header and footer are not empty
            if (!string.IsNullOrEmpty(HeaderText) && !string.IsNullOrEmpty(FooterText))
            {
                this.DialogResult = true; // Close the dialog with success
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter both header and footer text.");
            }
        }
    }
}
