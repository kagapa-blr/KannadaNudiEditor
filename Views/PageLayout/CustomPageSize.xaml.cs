using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace KannadaNudiEditor
{
    public partial class CustomPageSize : Window
    {
        public double PageWidth { get; private set; }
        public double PageHeight { get; private set; }
        public string SelectedUnit { get; private set; } = "in";

        public string Unit => SelectedUnit;

        public CustomPageSize(string width, string height, string unit)
        {
            InitializeComponent();

            WidthBox.Text = width;
            HeightBox.Text = height;

            foreach (ComboBoxItem item in UnitSelector.Items)
            {
                if (string.Equals(item.Tag?.ToString(), unit, StringComparison.OrdinalIgnoreCase))
                {
                    UnitSelector.SelectedItem = item;
                    break;
                }
            }

        }





        private static string NormalizeToEnglishNumbers(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (c >= 0x0CE6 && c <= 0x0CEF)   // Kannada digits
                    sb.Append((char)('0' + (c - 0x0CE6)));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }


        private void OK_Click(object sender, RoutedEventArgs e)
        {
            // string selectedUnit = (UnitSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "in";
            string selectedUnit = (UnitSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "in";

            string widthText = NormalizeToEnglishNumbers(WidthBox.Text);
            string heightText = NormalizeToEnglishNumbers(HeightBox.Text);

            if (double.TryParse(widthText, out double width) &&
                double.TryParse(heightText, out double height))
            {
                PageWidth = width;
                PageHeight = height;
                SelectedUnit = selectedUnit;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "ದಯವಿಟ್ಟು ಮಾನ್ಯವಾದ ಅಗಲ ಮತ್ತು ಎತ್ತರವನ್ನು ನಮೂದಿಸಿ.\n" +
                    "(Please enter valid width and height.)",
                    "Invalid Input",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
