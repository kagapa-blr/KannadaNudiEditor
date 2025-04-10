using System.Threading.Tasks;
using System.Windows;
using KannadaNudiEditor.ViewModel;

namespace KannadaNudiEditor.Views
{
    public partial class BannerWindow : Window
    {
        public BannerWindow()
        {
            InitializeComponent();
            this.Loaded += BannerWindow_Loaded;
        }

        private async void BannerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(3000); // Show banner for 3 seconds

            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
