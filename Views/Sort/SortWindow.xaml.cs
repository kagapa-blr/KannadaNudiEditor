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
    }
}
