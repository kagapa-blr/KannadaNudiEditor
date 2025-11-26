using System;
using System.Linq;
using System.Windows;
using KannadaNudiEditor.Views.SortHelp;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using System.Globalization;
using System.Collections.Generic;

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
                var selection = richTextBoxAdv.Selection;

                if (selection == null || selection.IsEmpty)
                {
                    string msg = isEnglish
                        ? "Please select some words."
                        : "ದಯವಿಟ್ಟು ಕೆಲವು ಪದಗಳನ್ನು ಆಯ್ಕೆಮಾಡಿ";

                    MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string selectedText = selection.Text ?? string.Empty;

                // Log raw selected text before sorting
                SimpleLogger.Log("Raw selected text:");
                SimpleLogger.Log(selectedText);

                var lines = selectedText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                if (lines.Length > 1)
                {
                    bool allLinesValid = true;

                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        if (string.IsNullOrEmpty(trimmedLine))
                            continue;

                        var wordsInLine = trimmedLine
                            .Split(new[] { ' ', '\t', ',', '.', ';', ':', '!', '?', '(', ')', '[', ']' },
                                   StringSplitOptions.RemoveEmptyEntries);

                        SimpleLogger.Log($"Line '{trimmedLine}' has {wordsInLine.Length} words.");

                        if (wordsInLine.Length != 1)
                        {
                            allLinesValid = false;
                            break;
                        }
                    }

                    if (!allLinesValid)
                    {
                        SimpleLogger.Log("One or more lines contain multiple words.");
                        string multiWordLineMsg = isEnglish
                            ? "Each line must contain exactly one word."
                            : "ಪ್ರತಿ ಸಾಲು ಒಂದು ಪದ ಮಾತ್ರ ಹೊಂದಿರಬೇಕು";
                        MessageBox.Show(multiWordLineMsg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                var words = selectedText
                    .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?', '(', ')', '[', ']' },
                           StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                if (words.Count == 0)
                {
                    string msg = isEnglish
                        ? "No words found in selection."
                        : "ಆಯ್ಕೆ ಮಾಡಿದ ಪ್ರದೇಶದಲ್ಲಿ ಯಾವುದೇ ಪದ ಸಿಗಲಿಲ್ಲ";

                    MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Log all words before sorting
                SimpleLogger.Log($"Total words found: {words.Count}");
                foreach (var w in words)
                {
                    SimpleLogger.Log($"Word: {w}");
                }

                // Sort words alphabetically and log sorted words
                var sortedWords = words.OrderBy(w => w, StringComparer.CurrentCulture).ToList();
                SimpleLogger.Log("Words sorted alphabetically:");
                foreach (var w in sortedWords)
                {
                    SimpleLogger.Log($"Sorted Word: {w}");
                }

                // Join with newline to keep each word on its own line
                string replacementText = string.Join(Environment.NewLine, sortedWords);

                // Replace selection by deleting it and inserting the sorted text
                richTextBoxAdv.Selection.Delete();
                richTextBoxAdv.Selection.InsertText(replacementText);

                string doneMsg = isEnglish
                    ? "Words logged and replaced successfully!"
                    : "ಪದಗಳನ್ನು ಯಶಸ್ವಿಯಾಗಿ ದಾಖಲಾಗಿದೆ ಮತ್ತು ಬದಲಿಸಲಾಗಿದೆ!";
                MessageBox.Show(doneMsg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error in OkButton_Click: {ex.Message}");
                MessageBox.Show("Error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
