using System;
using System.Windows;
using System.Windows.Controls;

namespace KannadaNudiEditor.Views.PageNumber
{
    public partial class PageNumberDialog : Window
    {
        public string PageNumberPlaceholder { get; private set; } = string.Empty;

        public PageNumberDialog()
        {
            try
            {
                InitializeComponent();
                this.Loaded += PageNumberDialog_Loaded;
                SimpleLogger.Log("PageNumberDialog constructor executed.");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Error initializing PageNumberDialog constructor");
                MessageBox.Show("Failed to initialize dialog constructor. See log for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PageNumberDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdatePreview();
                SimpleLogger.Log("PageNumberDialog Loaded event fired, preview updated.");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Error in PageNumberDialog Loaded event");
            }
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PageNumberPlaceholder = rbCurrentPage.IsChecked == true ? "{PAGE}" : "{NUMPAGES}";
                SimpleLogger.Log($"Insert clicked. Placeholder set to '{PageNumberPlaceholder}'.");

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Error in InsertButton_Click");
                MessageBox.Show("Failed to insert page number. See log for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SimpleLogger.Log("Cancel clicked. Closing dialog without inserting.");
                DialogResult = false;
                Close();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Error in CancelButton_Click");
                MessageBox.Show("Failed to close dialog. See log for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePreview()
        {
            try
            {
                if (tbPreview == null)
                {
                    SimpleLogger.Log("tbPreview is null in UpdatePreview.");
                    return;
                }

                tbPreview.Text = rbCurrentPage.IsChecked == true ? "Page 1" : "Page 1 of 10";
                SimpleLogger.Log($"Preview updated: '{tbPreview.Text}' at {(rbTop.IsChecked == true ? "Top" : "Bottom")} of page.");

                if (tbPreview.Parent is Grid parentGrid)
                {
                    parentGrid.RowDefinitions[0].Height = rbTop.IsChecked == true
                        ? new GridLength(0.0, GridUnitType.Star)
                        : new GridLength(1, GridUnitType.Star);
                    parentGrid.RowDefinitions[2].Height = rbTop.IsChecked == true
                        ? new GridLength(1, GridUnitType.Star)
                        : new GridLength(0.0, GridUnitType.Star);
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Error updating preview in PageNumberDialog");
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdatePreview();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Error in RadioButton_Checked");
            }
        }
    }
}
