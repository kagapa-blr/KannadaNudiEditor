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
        public string SelectedUnit { get; set; } = "Inches"; // Default unit is inches
        #endregion

        #region Constructor
        public CustomMargin()
        {
            InitializeComponent();
        }
        #endregion

        #region Implementation

        // Convert from cm or mm to inches
        private double ConvertToInches(double value, string unit)
        {
            switch (unit)
            {
                case "Centimeters":
                    return value / 2.54; // 1 inch = 2.54 cm
                case "Millimeters":
                    return value / 25.4; // 1 inch = 25.4 mm
                default:
                    return value; // Already in inches
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected unit from the ComboBox
            string selectedUnit = MarginUnitSelector.SelectedItem?.ToString() ?? "Inches"; // Default to Inches if nothing selected

            // Try parsing the input values for margins
            if (double.TryParse(TopMarginTextBox.Text, out double top) &&
                double.TryParse(BottomMarginTextBox.Text, out double bottom) &&
                double.TryParse(LeftMarginTextBox.Text, out double left) &&
                double.TryParse(RightMarginTextBox.Text, out double right))
            {
                // Convert the margins to inches based on selected unit
                TopMarginInInches = ConvertToInches(top, selectedUnit);
                BottomMarginInInches = ConvertToInches(bottom, selectedUnit);
                LeftMarginInInches = ConvertToInches(left, selectedUnit);
                RightMarginInInches = ConvertToInches(right, selectedUnit);

                // Store the selected unit
                SelectedUnit = selectedUnit;

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
