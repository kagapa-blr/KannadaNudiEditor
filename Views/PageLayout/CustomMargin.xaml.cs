using System.Text;
using System.Windows;
using System.Windows.Controls;

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
        public string SelectedUnit { get; set; } = "Inches"; // Default unit
        #endregion

        #region Constructor
        public CustomMargin()
        {
            InitializeComponent();
        }
        #endregion

        #region Implementation

        // Helper: Convert input Kannada digits (೦೧೨೩೪೫೬೭೮೯) to English digits (0123456789)
        private string NormalizeToEnglishNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var normalized = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                // Kannada digits Unicode range: 0x0CE6 (೦) to 0x0CEF (೯)
                if (c >= 0x0CE6 && c <= 0x0CEF)
                {
                    normalized.Append((char)('0' + (c - 0x0CE6)));
                }
                else
                {
                    normalized.Append(c);
                }
            }
            return normalized.ToString();
        }

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
            // Get the selected unit properly
            string selectedUnit = (MarginUnitSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Inches";

            // Normalize all TextBox inputs (to support Kannada numbers)
            string topText = NormalizeToEnglishNumbers(TopMarginTextBox.Text);
            string bottomText = NormalizeToEnglishNumbers(BottomMarginTextBox.Text);
            string leftText = NormalizeToEnglishNumbers(LeftMarginTextBox.Text);
            string rightText = NormalizeToEnglishNumbers(RightMarginTextBox.Text);

            // Try parsing
            if (double.TryParse(topText, out double top) &&
                double.TryParse(bottomText, out double bottom) &&
                double.TryParse(leftText, out double left) &&
                double.TryParse(rightText, out double right))
            {
                // Convert margins to inches
                TopMarginInInches = ConvertToInches(top, selectedUnit);
                BottomMarginInInches = ConvertToInches(bottom, selectedUnit);
                LeftMarginInInches = ConvertToInches(left, selectedUnit);
                RightMarginInInches = ConvertToInches(right, selectedUnit);

                SelectedUnit = selectedUnit;

                DialogResult = true;
                Close();
            }
            else
            {
                // Show bilingual error message
                MessageBox.Show("ದಯವಿಟ್ಟು ಎಲ್ಲಾ ಅಂಚುಗಳಿಗಾಗಿ ಮಾನ್ಯ ಸಂಖ್ಯಾ ಮೌಲ್ಯಗಳನ್ನು ನಮೂದಿಸಿ.\n(Please enter valid numerical values for all margins.)");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion
    }
}
