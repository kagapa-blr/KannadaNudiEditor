using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace KannadaNudiEditor
{
    /// <summary>
    /// Interaction logic for CustomPageSize.xaml
    /// </summary>
    public partial class CustomPageSize : Window
    {
        #region Properties ───────────────────────────────────────────────

        /// <summary>
        /// Page width exactly as the user typed it, in <see cref="SelectedUnit"/>.
        /// </summary>
        public double PageWidth { get; private set; }

        /// <summary>
        /// Page height exactly as the user typed it, in <see cref="SelectedUnit"/>.
        /// </summary>
        public double PageHeight { get; private set; }

        /// <summary>
        /// The unit selected by the user (e.g., "Inches", "Centimeters", "Millimeters").
        /// </summary>
        public string SelectedUnit { get; private set; } = "Inches";

        #endregion

        #region Constructor ──────────────────────────────────────────────

        /// <summary>
        /// Initializes the dialog with pre-filled values.
        /// </summary>
        /// <param name="width">Previously used width as string</param>
        /// <param name="height">Previously used height as string</param>
        /// <param name="unit">Previously selected unit</param>
        public CustomPageSize(string width, string height, string unit)
        {
            InitializeComponent();

            WidthBox.Text = width;
            HeightBox.Text = height;

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

        #region Helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Converts Kannada numerals to standard English numerals.
        /// </summary>
        private static string NormalizeToEnglishNumbers(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var builder = new StringBuilder(input.Length);

            foreach (char c in input)
            {
                if (c >= 0x0CE6 && c <= 0x0CEF) // Kannada 0–9
                    builder.Append((char)('0' + (c - 0x0CE6)));
                else
                    builder.Append(c);
            }

            return builder.ToString();
        }

        #endregion

        #region Event Handlers ──────────────────────────────────────────

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            string selectedUnit =
                (UnitSelector.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Inches";

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

        #endregion
    }
}
