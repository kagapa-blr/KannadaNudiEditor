using System.Windows;

namespace KannadaNudiEditor
{
    /// <summary>
    /// Interaction logic for CustomMargin.xaml
    /// </summary>
    public partial class CustomMargin : Window
    {
        #region Fields
        // Properties to hold the margin values
        public double TopMarginInInches { get; set; }
        public double BottomMarginInInches { get; set; }
        public double LeftMarginInInches { get; set; }
        public double RightMarginInInches { get; set; }
        #endregion

        #region Constructor
        public CustomMargin()
        {
            InitializeComponent();
        }
        #endregion

        #region Implementation
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Try parsing the input values for margins (in inches)
            if (double.TryParse(TopMarginTextBox.Text, out double top) &&
                double.TryParse(BottomMarginTextBox.Text, out double bottom) &&
                double.TryParse(LeftMarginTextBox.Text, out double left) &&
                double.TryParse(RightMarginTextBox.Text, out double right))
            {
                // Store the margin values
                TopMarginInInches = top;
                BottomMarginInInches = bottom;
                LeftMarginInInches = left;
                RightMarginInInches = right;

                // Close the dialog and return success
                DialogResult = true;
                Close();
            }
            else
            {
                // Show a message if any input is invalid
                MessageBox.Show("Please enter valid numerical values for all margins.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        #endregion
    }
}
