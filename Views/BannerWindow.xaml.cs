using System.Windows;

namespace KannadaNudiEditor.Views
{
    public partial class BannerWindow : Window
    {
        public BannerWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the banner window.
        /// </summary>
        public void ShowBanner()
        {
            this.Show();
        }

        /// <summary>
        /// Closes the banner window.
        /// </summary>  
        public void CloseBanner()
        {
            if (this.IsVisible)
                this.Close();
        }
    }
}
