using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Kannada.AsciiUnicode.Converters;
using KannadaNudiEditor.Helpers;

namespace KannadaNudiEditor.Views.LiveConversionEditor
{
    public partial class LiveConversionEditorWindow : Window
    {
        private bool _isAsciiToUnicode = true;
        private bool _isUpdatingFromCode = false;
        private KannadaConverter? _converter;
        private readonly Stopwatch _conversionTimer = new();
        private double _currentZoomLevel = 1.0;

        public LiveConversionEditorWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            try
            {
                _converter = KannadaConverter.Instance;
                SimpleLogger.Log("Kannada Conversion Editor initialized");
                SetAsciiToUnicodeMode();
                LeftTextBox.TextChanged += LeftTextBox_TextChanged;
                RightTextBox.TextChanged += RightTextBox_TextChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Initialization Failed");
                SimpleLogger.LogException(ex, "Converter initialization failed");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private string GetText(RichTextBox rtb)
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd).Text.TrimEnd();
        }

        private void SetText(RichTextBox rtb, string text)
        {
            rtb.Document.Blocks.Clear();
            rtb.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        private void ClearText(RichTextBox rtb)
        {
            rtb.Document.Blocks.Clear();
        }

        private void ModeToggle_Checked(object sender, RoutedEventArgs e)
        {
            SetUnicodeToAsciiMode();
        }

        private void ModeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            SetAsciiToUnicodeMode();
        }

        private void SetAsciiToUnicodeMode()
        {
            _isAsciiToUnicode = true;
            ModeToggle.IsChecked = false;
            ModeLabel.Text = "ASCII → Unicode";
            LeftPanelTitle.Text = "ASCII Input";
            RightPanelTitle.Text = "Unicode Output";
            LeftTextBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            RightTextBox.FontFamily = new System.Windows.Media.FontFamily("NudiParijatha");
            ClearAll();
        }

        private void SetUnicodeToAsciiMode()
        {
            _isAsciiToUnicode = false;
            ModeToggle.IsChecked = true;
            ModeLabel.Text = "Unicode → ASCII";
            LeftPanelTitle.Text = "Unicode Input";
            RightPanelTitle.Text = "ASCII Output";
            LeftTextBox.FontFamily = new System.Windows.Media.FontFamily("NudiParijatha");
            RightTextBox.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            ClearAll();
        }

        private void LeftTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode || _converter == null) return;

            try
            {
                string leftText = GetText(LeftTextBox);
                UpdateCharCount(leftText.Length);

                if (string.IsNullOrEmpty(leftText))
                {
                    _isUpdatingFromCode = true;
                    ClearText(RightTextBox);
                    _isUpdatingFromCode = false;
                    UpdateStatus("Ready", "neutral");
                    return;
                }

                _conversionTimer.Restart();
                string convertedText = _isAsciiToUnicode
                    ? _converter.ConvertAsciiToUnicode(leftText)
                    : _converter.ConvertUnicodeToAscii(leftText);
                _conversionTimer.Stop();

                _isUpdatingFromCode = true;
                SetText(RightTextBox, convertedText);
                _isUpdatingFromCode = false;

                UpdateStatus($"Converted {leftText.Length} chars", "success", _conversionTimer.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}", "error");
                SimpleLogger.LogException(ex, "Conversion error");
            }
        }

        private void RightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingFromCode || _converter == null) return;

            try
            {
                string rightText = GetText(RightTextBox);

                if (string.IsNullOrEmpty(rightText))
                {
                    _isUpdatingFromCode = true;
                    ClearText(LeftTextBox);
                    _isUpdatingFromCode = false;
                    UpdateCharCount(0);
                    UpdateStatus("Ready", "neutral");
                    return;
                }

                _conversionTimer.Restart();
                string convertedText = _isAsciiToUnicode
                    ? _converter.ConvertUnicodeToAscii(rightText)
                    : _converter.ConvertAsciiToUnicode(rightText);
                _conversionTimer.Stop();

                _isUpdatingFromCode = true;
                SetText(LeftTextBox, convertedText);
                _isUpdatingFromCode = false;

                UpdateStatus($"Converted {rightText.Length} chars", "success", _conversionTimer.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}", "error");
                SimpleLogger.LogException(ex, "Conversion error");
            }
        }

        private void CopyLeft_Click(object sender, RoutedEventArgs e)
        {
            string text = GetText(LeftTextBox);
            if (string.IsNullOrEmpty(text))
            {
                UpdateStatus("Nothing to copy", "warning");
                return;
            }
            try
            {
                Clipboard.SetText(text);
                UpdateStatus($"Copied {text.Length} chars", "success");
            }
            catch
            {
                UpdateStatus("Copy failed", "error");
            }
        }

        private void CopyRight_Click(object sender, RoutedEventArgs e)
        {
            string text = GetText(RightTextBox);
            if (string.IsNullOrEmpty(text))
            {
                UpdateStatus("Nothing to copy", "warning");
                return;
            }
            try
            {
                Clipboard.SetText(text);
                UpdateStatus($"Copied {text.Length} chars", "success");
            }
            catch
            {
                UpdateStatus("Copy failed", "error");
            }
        }

        private void Swap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isUpdatingFromCode = true;
                string temp = GetText(LeftTextBox);
                SetText(LeftTextBox, GetText(RightTextBox));
                SetText(RightTextBox, temp);
                _isUpdatingFromCode = false;
                UpdateStatus("Panels swapped", "success");
            }
            catch (Exception ex)
            {
                UpdateStatus("Swap failed", "error");
                SimpleLogger.LogException(ex, "Swap error");
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }

        private void ClearAll()
        {
            _isUpdatingFromCode = true;
            ClearText(LeftTextBox);
            ClearText(RightTextBox);
            _isUpdatingFromCode = false;
            UpdateCharCount(0);
            UpdateStatus("Ready", "neutral");
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _currentZoomLevel = Math.Min(_currentZoomLevel + 0.1, 3.0);
            ApplyZoom();
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _currentZoomLevel = Math.Max(_currentZoomLevel - 0.1, 0.5);
            ApplyZoom();
        }

        private void ApplyZoom()
        {
            LeftTextBox.FontSize = 12 * _currentZoomLevel;
            RightTextBox.FontSize = 13 * _currentZoomLevel;
            ZoomDisplayLabel.Text = $"{(int)(_currentZoomLevel * 100)}%";
            UpdateStatus($"Zoom: {(int)(_currentZoomLevel * 100)}%", "neutral");
        }

        private void UpdateCharCount(int count)
        {
            CharCountTextBlock.Text = count == 0 ? "No text" : $"{count} chars";
            LeftCharCount.Text = GetText(LeftTextBox).Length > 0 ? $"({GetText(LeftTextBox).Length})" : "";
            RightCharCount.Text = GetText(RightTextBox).Length > 0 ? $"({GetText(RightTextBox).Length})" : "";
        }

        private void UpdateStatus(string message, string type, long? elapsedMs = null)
        {
            StatusTextBlock.Text = message;
            StatusTextBlock.Foreground = type switch
            {
                "success" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 124, 16)),
                "error" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(209, 52, 56)),
                "warning" => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 152, 0)),
                _ => new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(102, 102, 102))
            };

            ConversionTimeTextBlock.Text = elapsedMs.HasValue
                ? (elapsedMs.Value < 1 ? "< 1ms" : $"{elapsedMs.Value}ms")
                : "";
        }

        private void TextBox_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.Text))
            {
                string text = (string)e.Data.GetData(System.Windows.DataFormats.Text);
                if (sender is RichTextBox rtb)
                {
                    rtb.AppendText(text);
                    e.Handled = true;
                }
            }
        }
    }
}
