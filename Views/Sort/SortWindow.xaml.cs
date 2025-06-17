using System.Windows;

namespace KannadaNudiEditor.Views.Sort
{
    public partial class SortWindow : Window
    {
        public SortWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Sort settings applied (dummy action).");
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            string message =
        @"**Sort By**
If sorting a list, choose Paragraphs, Headings, or Fields.
If sorting a table and you have a header row, choose by header name.
If you don't have a header, choose by column number.

**Type**
Choose between Text, Number, or Date.

**Using**
Choose Paragraphs.

**Ascending or Descending**
Choose sorting method.

**My list has**
Choose whether your list or table has headers.

**Options**
Choose sort options.

• Where to separate fields (Tabs, Comma, Other)
• Sort options (Sort column only, Case sensitive)
• Sorting language";

            MessageBox.Show(message, "Sort Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }


    }
}
