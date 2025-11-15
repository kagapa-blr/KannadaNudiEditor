using System.Windows;

namespace KannadaNudiEditor.Views.HeaderFooter
{
    public partial class ClearHeaderFooterDialog : Window
    {
        public enum DialogHeaderFooterType
        {
            AllPages,
            EvenPages,
            FirstPage
        }


        public DialogHeaderFooterType SelectedType { get; private set; }

        public ClearHeaderFooterDialog()
        {
            InitializeComponent();
            rbClearAll.IsChecked = true; // default selection

            SimpleLogger.Log("ClearHeaderFooterDialog initialized.");
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (rbClearAll.IsChecked == true)
                SelectedType = DialogHeaderFooterType.AllPages;
            else if (rbClearEven.IsChecked == true)
                SelectedType = DialogHeaderFooterType.EvenPages;
            else
                SelectedType = DialogHeaderFooterType.FirstPage;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
