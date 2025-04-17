using System.Windows;

namespace KannadaNudiEditor
{
    /// <summary>
    /// Interaction logic for PageSetupDialog.xaml
    /// </summary>
    public partial class PageSetupDialog : Window
    {
        #region property
        public double PageWidthInInches { get; private set; }
        public double PageHeightInInches { get; private set; }
        #endregion

        #region Constructor
        public PageSetupDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Called when OK button is clicked in page setup dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthBox.Text, out double width) &&
                double.TryParse(HeightBox.Text, out double height))
            {
                PageWidthInInches = width;
                PageHeightInInches = height;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter valid numeric values.");
            }
        }
        /// <summary>
        /// Called when Cancel button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        #endregion
    }
}
