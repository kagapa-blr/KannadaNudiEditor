using System;
using System.Windows;
using System.Windows.Controls;
using KannadaNudiEditor.Helpers.Conversion;

namespace KannadaNudiEditor.Views.LiveConversionEditor
{
    public partial class LiveConversionEditorWindow : Window
    {
        private bool _isAsciiToUnicode = true;
        private bool _isUpdatingFromCode = false;

        public LiveConversionEditorWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Set initial mode to ASCII → Unicode
            SetAsciiToUnicodeMode();
        }

        /// <summary>
        /// Switch to ASCII → Unicode mode
        /// </summary>
        private void AsciiToUnicodeMode_Click(object sender, RoutedEventArgs e)
        {
            SetAsciiToUnicodeMode();
        }

        private void SetAsciiToUnicodeMode()
        {
            _isAsciiToUnicode = true;

            // Update UI
            AsciiToUnicodeButton.Background = System.Windows.Media.Brushes.LightBlue;
            AsciiToUnicodeButton.Foreground = System.Windows.Media.Brushes.DarkBlue;
            AsciiToUnicodeButton.BorderThickness = new Thickness(2);

            UnicodeToAsciiButton.Background = System.Windows.Media.Brushes.White;
            UnicodeToAsciiButton.Foreground = System.Windows.Media.Brushes.Black;
            UnicodeToAsciiButton.BorderThickness = new Thickness(1);

            // Update panel titles
            LeftPanelTitle.Text = "ASCII";
            LeftPanelTitle.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)); // Blue

            RightPanelTitle.Text = "Unicode";
            RightPanelTitle.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0)); // Orange

            // Set appropriate fonts
            LeftTextBox.FontFamily = new System.Windows.Media.FontFamily("Courier New");
            RightTextBox.FontFamily = new System.Windows.Media.FontFamily("NudiParijatha");

            // Clear and convert
            ClearAll();
            InfoTextBlock.Text = "Mode: ASCII → Unicode (type ASCII on the left)";
        }

        /// <summary>
        /// Switch to Unicode → ASCII mode
        /// </summary>
        private void UnicodeToAsciiMode_Click(object sender, RoutedEventArgs e)
        {
            SetUnicodeToAsciiMode();
        }

        private void SetUnicodeToAsciiMode()
        {
            _isAsciiToUnicode = false;

            // Update UI
            UnicodeToAsciiButton.Background = System.Windows.Media.Brushes.LightYellow;
            UnicodeToAsciiButton.Foreground = System.Windows.Media.Brushes.DarkOrange;
            UnicodeToAsciiButton.BorderThickness = new Thickness(2);

            AsciiToUnicodeButton.Background = System.Windows.Media.Brushes.White;
            AsciiToUnicodeButton.Foreground = System.Windows.Media.Brushes.Black;
            AsciiToUnicodeButton.BorderThickness = new Thickness(1);

            // Update panel titles
            LeftPanelTitle.Text = "Unicode";
            LeftPanelTitle.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0)); // Orange

            RightPanelTitle.Text = "ASCII";
            RightPanelTitle.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243)); // Blue

            // Set appropriate fonts
            LeftTextBox.FontFamily = new System.Windows.Media.FontFamily("NudiParijatha");
            RightTextBox.FontFamily = new System.Windows.Media.FontFamily("Courier New");

            // Clear and convert
            ClearAll();
            InfoTextBlock.Text = "Mode: Unicode → ASCII (type Unicode on the left)";
        }

        /// <summary>
        /// Handle text changes in the left text box
        /// </summary>
        private void LeftTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode)
                return;

            try
            {
                string leftText = LeftTextBox.Text;
                UpdateCharCount(leftText.Length);

                if (string.IsNullOrEmpty(leftText))
                {
                    _isUpdatingFromCode = true;
                    RightTextBox.Clear();
                    _isUpdatingFromCode = false;
                    return;
                }

                // Perform conversion based on mode
                string convertedText = _isAsciiToUnicode
                    ? ConversionHelper.Converter.ConvertAsciiToUnicode(leftText)
                    : ConversionHelper.Converter.ConvertUnicodeToAscii(leftText);

                // Update right panel without triggering its TextChanged event
                _isUpdatingFromCode = true;
                RightTextBox.Text = convertedText;
                _isUpdatingFromCode = false;

                InfoTextBlock.Text = $"Converted: {leftText.Length} character(s)";
            }
            catch (Exception ex)
            {
                InfoTextBlock.Text = $"Error: {ex.Message}";
                SimpleLogger.LogException(ex, "LiveConversionEditor: Error in left text conversion");
            }
        }

        /// <summary>
        /// Handle text changes in the right text box
        /// </summary>
        private void RightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode)
                return;

            try
            {
                string rightText = RightTextBox.Text;

                if (string.IsNullOrEmpty(rightText))
                {
                    _isUpdatingFromCode = true;
                    LeftTextBox.Clear();
                    _isUpdatingFromCode = false;
                    UpdateCharCount(0);
                    return;
                }

                // Perform reverse conversion based on mode
                string convertedText = _isAsciiToUnicode
                    ? ConversionHelper.Converter.ConvertUnicodeToAscii(rightText)
                    : ConversionHelper.Converter.ConvertAsciiToUnicode(rightText);

                // Update left panel without triggering its TextChanged event
                _isUpdatingFromCode = true;
                LeftTextBox.Text = convertedText;
                _isUpdatingFromCode = false;

                InfoTextBlock.Text = $"Reverse converted: {rightText.Length} character(s)";
            }
            catch (Exception ex)
            {
                InfoTextBlock.Text = $"Error: {ex.Message}";
                SimpleLogger.LogException(ex, "LiveConversionEditor: Error in right text conversion");
            }
        }

        /// <summary>
        /// Copy left text to clipboard
        /// </summary>
        private void CopyLeft_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(LeftTextBox.Text))
                {
                    Clipboard.SetText(LeftTextBox.Text);
                    InfoTextBlock.Text = "Left text copied to clipboard!";
                }
                else
                {
                    InfoTextBlock.Text = "Nothing to copy";
                }
            }
            catch (Exception ex)
            {
                InfoTextBlock.Text = "Failed to copy to clipboard";
                SimpleLogger.LogException(ex, "LiveConversionEditor: Copy error");
            }
        }

        /// <summary>
        /// Clear all text boxes
        /// </summary>
        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void ClearAll()
        {
            _isUpdatingFromCode = true;
            LeftTextBox.Clear();
            RightTextBox.Clear();
            _isUpdatingFromCode = false;
            UpdateCharCount(0);
            InfoTextBlock.Text = "Cleared all text";
        }

        /// <summary>
        /// Update character count display
        /// </summary>
        private void UpdateCharCount(int count)
        {
            CharCountTextBlock.Text = $"Characters: {count}";
        }
    }
}
