using System;
using System.Linq;
using System.Windows;
using KannadaNudiEditor.Views.SortHelp;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.Globalization;
using System.Collections.Generic;
using KannadaNudiEditor.Views.Loading;

namespace KannadaNudiEditor.Views.Sort
{
    public partial class SortWindow : Window
    {
        private readonly bool isEnglish; // Only for labels/messages
        private readonly SfRichTextBoxAdv richTextBoxAdv;
        private SortHelpWindow? helpWindow;

        public SortWindow(bool isEnglishLanguage, SfRichTextBoxAdv richTextBox)
        {
            InitializeComponent();
            isEnglish = isEnglishLanguage;
            richTextBoxAdv = richTextBox;
            SimpleLogger.Log("SortWindow initialized.");
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingView.Show();
                StatusMessage.Text = ""; // Clear previous status

                var selection = richTextBoxAdv.Selection;
                if (selection == null || selection.IsEmpty)
                {
                    StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
                    StatusMessage.Text = isEnglish ? "Please select some words." : "ದಯವಿಟ್ಟು ಕೆಲವು ಪದಗಳನ್ನು ಆಯ್ಕೆಮಾಡಿ";
                    SimpleLogger.Log(StatusMessage.Text);
                    return;
                }

                string selectedText = selection.Text ?? string.Empty;
                SimpleLogger.Log("Raw selected text:");
                SimpleLogger.Log(selectedText);

                var lines = selectedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                bool allLinesValid = true;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine))
                        continue;

                    var wordsInLine = trimmedLine.Split(new[] { ' ', '\t', ',', '.', ';', ':', '!', '?', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    SimpleLogger.Log($"Line '{trimmedLine}' has {wordsInLine.Length} words.");
                    if (wordsInLine.Length != 1)
                    {
                        allLinesValid = false;
                        break;
                    }
                }

                if (!allLinesValid)
                {
                    StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
                    StatusMessage.Text = isEnglish ? "Each line must contain exactly one word." : "ಪ್ರತಿ ಸಾಲು ಒಂದು ಪದ ಮಾತ್ರ ಹೊಂದಿರಬೇಕು";
                    SimpleLogger.Log(StatusMessage.Text);
                    return;
                }

                var words = selectedText.Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (words.Count == 0)
                {
                    StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
                    StatusMessage.Text = isEnglish ? "No words found in selection." : "ಆಯ್ಕೆ ಮಾಡಿದ ಪ್ರದೇಶದಲ್ಲಿ ಯಾವುದೇ ಪದ ಸಿಗಲಿಲ್ಲ";
                    SimpleLogger.Log(StatusMessage.Text);
                    return;
                }

                if (words.Count == 1)
                {
                    StatusMessage.Foreground = System.Windows.Media.Brushes.Green;
                    StatusMessage.Text = isEnglish ? "Single word selected, no sorting/replacement needed." : "ಏಕ ಪದ ಆಯ್ಕೆಮಾಡಲಾಗಿದೆ, ಕ್ರಮವಿಧಾನ ಅಥವಾ ಬದಲಾವಣೆ ಅಗತ್ಯವಿಲ್ಲ.";
                    SimpleLogger.Log(StatusMessage.Text);
                    return;
                }

                List<string> sortedWords = sortByAsc.IsChecked == true
                    ? words.OrderBy(w => w, StringComparer.CurrentCulture).ToList()
                    : words.OrderByDescending(w => w, StringComparer.CurrentCulture).ToList();

                string replacementText = string.Join(Environment.NewLine, sortedWords);
                richTextBoxAdv.Selection.Delete();
                richTextBoxAdv.Selection.InsertText(replacementText);

                StatusMessage.Foreground = System.Windows.Media.Brushes.Green;
                StatusMessage.Text = isEnglish ? "Words logged and replaced successfully!" : "ಪದಗಳನ್ನು ಯಶಸ್ವಿಯಾಗಿ ದಾಖಲಿಸಲಾಗಿದೆ ಮತ್ತು ಬದಲಿಸಲಾಗಿದೆ!";
                SimpleLogger.Log(StatusMessage.Text);
            }
            catch (Exception ex)
            {
                StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
                StatusMessage.Text = "Error occurred: " + ex.Message;
                SimpleLogger.Log($"Error in OkButton_Click: {ex.Message}");
            }
            finally
            {
                LoadingView.Hide();
            }
        }





        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (helpWindow == null || !helpWindow.IsVisible)
            {
                helpWindow = new SortHelpWindow(isEnglish)
                {
                    Owner = this
                };
                helpWindow.ShowDialog();
            }
            else
            {
                helpWindow.Activate();
            }
        }
    }
}
