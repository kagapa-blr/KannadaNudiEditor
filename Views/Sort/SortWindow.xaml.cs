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
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBoxAdv.Selection;
            if (selection == null || selection.IsEmpty)
            {
                string msg = isEnglish ? "Please select paragraphs to sort." : "ದಯವಿಟ್ಟು ಸರಿಕ್ರಮಿಸಲು ಪ್ಯಾರಾಗ್ರಾಫ್‌ಗಳನ್ನು ಆಯ್ಕೆಮಾಡಿ.";
                MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            ParagraphAdv? startPara = selection.Start.Paragraph as ParagraphAdv;
            ParagraphAdv? endPara = selection.End.Paragraph as ParagraphAdv;

            if (startPara == null || endPara == null)
            {
                string msg = isEnglish ? "No paragraph found in selection." : "ಆಯ್ಕೆ ಪ್ರದೇಶದಲ್ಲಿ ಪ್ಯಾರಾಗ್ರಾಫ್ ಸಿಗಲಿಲ್ಲ.";
                MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            SectionAdv? targetSection = null;
            List<ParagraphAdv> paragraphs = new List<ParagraphAdv>();
            bool collecting = false;

            // Find the section that contains the start paragraph, and collect paragraphs until end
            foreach (var sec in richTextBoxAdv.Document.Sections)
            {
                if (sec is SectionAdv section)
                {
                    foreach (var block in section.Blocks)
                    {
                        if (block == startPara)
                        {
                            collecting = true;
                            targetSection = section;
                        }

                        if (collecting && block is ParagraphAdv para)
                        {
                            paragraphs.Add(para);
                        }

                        if (block == endPara)
                        {
                            collecting = false;
                            break;
                        }
                    }

                    if (targetSection != null)
                        break;
                }
            }

            if (targetSection == null || paragraphs.Count <= 1)
            {
                string msg = isEnglish ? "Need at least two paragraphs to sort." : "ಸರಿಕ್ರಮಿಸಲು ಕನಿಷ್ಠ ಎರಡು ಪ್ಯಾರಾಗ್ರಾಫ್ ಬೇಕು.";
                MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                return;
            }

            // Prepare sort data
            var paragraphData = paragraphs.Select(p =>
            {
                string text = string.Concat(p.Inlines.OfType<SpanAdv>().Select(s => s.Text)).Trim();
                char firstChar = text.FirstOrDefault(c => !char.IsWhiteSpace(c));
                bool isKannadaChar = firstChar >= '\u0C80' && firstChar <= '\u0CFF';

                CultureInfo culture = isKannadaChar
                    ? CultureInfo.GetCultureInfo("kn-IN")
                    : CultureInfo.GetCultureInfo("en-US");

                return new { Paragraph = p, Text = text, Culture = culture };
            }).ToList();

            var sorted = paragraphData
                .OrderBy(x => x.Text, Comparer<string>.Create((a, b) =>
                {
                    var cA = paragraphData.First(z => z.Text == a).Culture;
                    var cB = paragraphData.First(z => z.Text == b).Culture;
                    return StringComparer.Create(cA, true).Compare(a, b);
                }))
                .ToList();

            // Remove old paragraphs from the section
            foreach (var p in paragraphs)
            {
                targetSection.Blocks.Remove(p);
            }

            // Insert sorted paragraphs back in the same spot
            // Decide insertion index = original index of startPara in targetSection.Blocks
            int insertIndex = 0;
            for (int i = 0; i < targetSection.Blocks.Count; i++)
            {
                if (targetSection.Blocks[i] == startPara)
                {
                    insertIndex = i;
                    break;
                }
            }

            foreach (var item in sorted.Reverse<dynamic>())  // reverse so that insertion keeps correct order
            {
                targetSection.Blocks.Insert(insertIndex, item.Paragraph);
            }

            string msgDone = isEnglish
                ? "Paragraphs sorted successfully!"
                : "ಪ್ಯಾರಾಗ್ರಾಫ್‌ಗಳು ಯಶಸ್ವಿಯಾಗಿ ಸರಿಕ್ರಮಿಸಲ್ಪಟ್ಟಿವೆ!";
            MessageBox.Show(msgDone, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            this.Close();
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
