using System.Globalization;
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
        #region Public Properties

        public new double Top { get; private set; }
        public double Bottom { get; private set; }
        public new double Left { get; private set; }
        public double Right { get; private set; }
        public string Unit { get; private set; } = "in";   // "in" | "cm" | "mm"

        #endregion

        #region Private State (fallback values)

        private readonly string _initialTop;
        private readonly string _initialBottom;
        private readonly string _initialLeft;
        private readonly string _initialRight;

        #endregion

        #region Constructor

        public CustomMargin(string top, string bottom, string left, string right, string unit)
        {
            InitializeComponent();

            _initialTop = top;
            _initialBottom = bottom;
            _initialLeft = left;
            _initialRight = right;

            TopMarginTextBox.Text = top;
            BottomMarginTextBox.Text = bottom;
            LeftMarginTextBox.Text = left;
            RightMarginTextBox.Text = right;

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

        #region Event Handlers

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // Selected unit tag ("in" | "cm" | "mm")
            Unit = (MarginUnitSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "in";

            bool isValid = true;

            Top = ParseOrFallback(TopMarginTextBox.Text, _initialTop, ref isValid);
            Bottom = ParseOrFallback(BottomMarginTextBox.Text, _initialBottom, ref isValid);
            Left = ParseOrFallback(LeftMarginTextBox.Text, _initialLeft, ref isValid);
            Right = ParseOrFallback(RightMarginTextBox.Text, _initialRight, ref isValid);

            if (isValid)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(
                    "ದಯವಿಟ್ಟು ಎಲ್ಲಾ ಅಂಚುಗಳಿಗಾಗಿ ಮಾನ್ಯ ಸಂಖ್ಯಾ ಮೌಲ್ಯಗಳನ್ನು ನಮೂದಿಸಿ.\n" +
                    "(Please enter valid numerical values for all margins.)");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts Kannada digits to ASCII digits.
        /// </summary>
       
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



        /// <summary>
        /// Tries to parse the text; if empty, uses fallback; sets flag false if invalid.
        /// </summary>
        private static double ParseOrFallback(string raw, string fallback, ref bool ok)
        {
            string text = string.IsNullOrWhiteSpace(raw) ? fallback : raw;
            text = NormalizeToEnglishNumbers(text).Trim();

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                return value;

            ok = false;
            return 0;
        }

        #endregion
    }
}
