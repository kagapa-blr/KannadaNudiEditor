using System.Windows;

namespace KannadaNudiEditor.Views.HeaderFooter
{
    public partial class HeaderFooterEditor : Window
    {
        public string? HeaderText { get; private set; }
        public string? FooterText { get; private set; }

        public HeaderFooterEditor(string? initialHeader, string? initialFooter)
        {
            InitializeComponent();

            HeaderTextBox.Text = initialHeader ?? string.Empty;
            FooterTextBox.Text = initialFooter ?? string.Empty;
        }

        // Handle Apply button click to update the header/footer
        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the header and footer texts
            HeaderText = HeaderTextBox.Text;
            FooterText = FooterTextBox.Text;

            // Accept any combination (header only, footer only, both, or neither)
            this.DialogResult = true;
            this.Close();
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Optionally indicate cancellation
            this.Close();              // Close the window
        }


    }
}
