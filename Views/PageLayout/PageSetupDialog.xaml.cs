using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace KannadaNudiEditor
{
    /// <summary>
    /// Interaction logic for PageSetupDialog.xaml
    /// </summary>
    public partial class PageSetupDialog : Window
    {
        #region Properties
        public double PageWidthInInches { get; private set; }
        public double PageHeightInInches { get; private set; }
        public string SelectedUnit { get; private set; } = "Inches"; // Default to Inches
        #endregion

        #region Constructor
        public PageSetupDialog(string width, string height, string unit)
        {
            InitializeComponent();

            //Whenever dialog is laucnhed will display the predefined user values.
            WidthBox.Text = width;
            HeightBox.Text = height;

            // Set the selected unit in the ComboBox
            foreach (ComboBoxItem item in UnitSelector.Items)
            {
                if (item.Content.ToString().Equals(unit, StringComparison.OrdinalIgnoreCase))
                {
                    UnitSelector.SelectedItem = item;
                    break;
                }
            }
        }
        #endregion

        #region Implementation

        // Helper: Normalize Kannada digits to English digits
        private string NormalizeToEnglishNumbers(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var normalized = new StringBuilder(input.Length);
            foreach (char c in input)
            {
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

        // Convert entered value to inches based on selected unit
        private double ConvertToInches(double value, string unit)
        {
            switch (unit)
            {
                case "Centimeters":
                    return value / 2.54;
                case "Millimeters":
                    return value / 25.4;
                default:
                    return value; // Already inches
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // Get selected unit
            string selectedUnit = (UnitSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Inches";

            // Normalize width and height inputs
            string widthText = NormalizeToEnglishNumbers(WidthBox.Text);
            string heightText = NormalizeToEnglishNumbers(HeightBox.Text);

            if (double.TryParse(widthText, out double width) &&
                double.TryParse(heightText, out double height))
            {
                PageWidthInInches = ConvertToInches(width, selectedUnit);
                PageHeightInInches = ConvertToInches(height, selectedUnit);
                SelectedUnit = selectedUnit;

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("ದಯವಿಟ್ಟು ಮಾನ್ಯವಾದ ಅಗಲ ಮತ್ತು ಎತ್ತರವನ್ನು ನಮೂದಿಸಿ.\n(Please enter valid width and height.)");
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
