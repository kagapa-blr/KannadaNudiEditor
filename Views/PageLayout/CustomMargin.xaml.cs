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

        public double Top { get; private set; }
        public double Bottom { get; private set; }
        public double Left { get; private set; }
        public double Right { get; private set; }
        public string Unit { get; private set; } = "in"; // "in" | "cm" | "mm"

        #endregion

        #region Constructor

        public CustomMargin(string top, string bottom, string left, string right, string unit)
        {
            InitializeComponent();

            // Prefill user values
            TopMarginTextBox.Text = top;
            BottomMarginTextBox.Text = bottom;
            LeftMarginTextBox.Text = left;
            RightMarginTextBox.Text = right;

            // Set unit selection based on neutral tag
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

        #region Event Handlers

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string unitTag = (MarginUnitSelector.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "in";

            string topText = NormalizeToEnglishNumbers(TopMarginTextBox.Text);
            string bottomText = NormalizeToEnglishNumbers(BottomMarginTextBox.Text);
            string leftText = NormalizeToEnglishNumbers(LeftMarginTextBox.Text);
            string rightText = NormalizeToEnglishNumbers(RightMarginTextBox.Text);

            if (double.TryParse(topText, out double top) &&
                double.TryParse(bottomText, out double bottom) &&
                double.TryParse(leftText, out double left) &&
                double.TryParse(rightText, out double right))
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

        #region Helpers

        private string NormalizeToEnglishNumbers(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (c >= 0x0CE6 && c <= 0x0CEF) // Kannada digits
                    sb.Append((char)('0' + (c - 0x0CE6)));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        #endregion
    }
}
