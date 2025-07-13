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



        public double Top { get; private set; }
        public double Bottom { get; private set; }
        public double Left { get; private set; }
        public double Right { get; private set; }
        public string Unit { get; private set; } = "in";   // “in” | “cm” | “mm”

        #region Constructor
        public CustomMargin(string top, string bottom, string left, string right, string unit)
        {
            InitializeComponent();

            //Whenever dialog is laucnhed will display the predefined user values.
            TopMarginTextBox.Text = top;
            BottomMarginTextBox.Text = bottom;
            LeftMarginTextBox.Text = left;
            RightMarginTextBox.Text = right;

            // Set the selected unit in the ComboBox
            foreach (ComboBoxItem item in MarginUnitSelector.Items)
            {
                if (item.Tag?.ToString() == unit.ToLower())
                {
                    MarginUnitSelector.SelectedItem = item;
                    break;
                }
            }

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
        private double ConvertToInches1(double value, string unit)
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
            // get neutral unit ("in" | "cm" | "mm")
            string unitTag = (MarginUnitSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "in";

            // Kannada → ASCII digits
            string topTxt = NormalizeToEnglishNumbers(TopMarginTextBox.Text);
            string bottomTxt = NormalizeToEnglishNumbers(BottomMarginTextBox.Text);
            string leftTxt = NormalizeToEnglishNumbers(LeftMarginTextBox.Text);
            string rightTxt = NormalizeToEnglishNumbers(RightMarginTextBox.Text);

            if (double.TryParse(topTxt, out double top) &&
                double.TryParse(bottomTxt, out double bottom) &&
                double.TryParse(leftTxt, out double left) &&
                double.TryParse(rightTxt, out double right))
            {
                Top = top;
                Bottom = bottom;
                Left = left;
                Right = right;
                Unit = unitTag;

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("ದಯವಿಟ್ಟು ಎಲ್ಲಾ ಅಂಚುಗಳಿಗಾಗಿ ಮಾನ್ಯ ಸಂಖ್ಯಾ ಮೌಲ್ಯಗಳನ್ನು ನಮೂದಿಸಿ.\n" +
                                "(Please enter valid numerical values for all margins.)");
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
