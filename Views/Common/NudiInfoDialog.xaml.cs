using System.Windows;
using System.Windows.Controls;

namespace KannadaNudiEditor.Views.Common
{
    public partial class NudiInfoDialog : Window
    {
        public NudiInfoDialog(string message, string header = "Information", Window? owner = null)
        {
            InitializeComponent();

            MessageText.Text = message;
            HeaderText.Text = header;

            // Only set Owner if it is not this window itself
            if (owner != null && owner != this)
            {
                Owner = owner;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Static helper
        public static void Show(string message, string header = "Information", Window? owner = null)
        {
            var dlg = new NudiInfoDialog(message, header, owner);
            dlg.ShowDialog();
        }
    }
}
