using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using KannadaNudiEditor.Helpers.Conversion;

namespace KannadaNudiEditor.Views.CustomMappings
{
    public partial class CustomMappingsWindow : Window
    {
        private Dictionary<string, string> currentMappings = new();
        private List<MappingRowUI> mappingRows = new();

        public CustomMappingsWindow()
        {
            InitializeComponent();
            LoadMappingsFromFile();
            RefreshUI();
        }

        private void LoadMappingsFromFile()
        {
            try
            {
                currentMappings = CustomMappingsHelper.LoadMappings();
                SimpleLogger.Log($"Loaded {currentMappings.Count} custom mappings");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Failed to load custom mappings");
                currentMappings = new Dictionary<string, string>();
            }
        }

        private void RefreshUI()
        {
            mappingsContainer.Children.Clear();
            mappingRows.Clear();

            foreach (var kvp in currentMappings)
            {
                AddMappingRowUI(kvp.Key, kvp.Value);
            }

            UpdateStatus();
        }

        private void AddMappingRowUI(string asciiValue = "", string unicodeValue = "")
        {
            // Create a grid for this row
            var rowGrid = new Grid
            {
                Margin = new Thickness(0, 5, 0, 5),
                Height = 40
            };

            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            // ASCII TextBox
            var asciiBox = new TextBox
            {
                Text = asciiValue,
                Padding = new Thickness(8),
                FontSize = 12,
                MaxLength = 50,
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Enter ASCII character(s)"
            };
            Grid.SetColumn(asciiBox, 0);
            rowGrid.Children.Add(asciiBox);

            // Arrow label
            var arrowLabel = new TextBlock
            {
                Text = "→",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightGray),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(arrowLabel, 1);
            rowGrid.Children.Add(arrowLabel);

            // Unicode TextBox
            var unicodeBox = new TextBox
            {
                Text = unicodeValue,
                Padding = new Thickness(8),
                FontSize = 12,
                MaxLength = 50,
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Enter Unicode character(s)"
            };
            Grid.SetColumn(unicodeBox, 2);
            rowGrid.Children.Add(unicodeBox);

            // Remove Button
            var removeButton = new Button
            {
                Content = "✕",
                Width = 35,
                Height = 35,
                Margin = new Thickness(5, 0, 0, 0),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = "Remove this mapping"
            };
            Grid.SetColumn(removeButton, 3);
            rowGrid.Children.Add(removeButton);

            var row = new MappingRowUI
            {
                RowGrid = rowGrid,
                AsciiBox = asciiBox,
                UnicodeBox = unicodeBox,
                RemoveButton = removeButton
            };

            removeButton.Click += (s, e) => RemoveMapping_Click(row);

            mappingRows.Add(row);
            mappingsContainer.Children.Add(rowGrid);
            UpdateStatus();
        }

        private void AddMapping_Click(object sender, RoutedEventArgs e)
        {
            AddMappingRowUI("", "");
        }

        private void RemoveMapping_Click(MappingRowUI row)
        {
            mappingsContainer.Children.Remove(row.RowGrid);
            mappingRows.Remove(row);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            statusTextBlock.Text = $"Total mappings: {mappingRows.Count}";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newMappings = new Dictionary<string, string>();

                foreach (var row in mappingRows)
                {
                    string ascii = row.AsciiBox?.Text.Trim() ?? string.Empty;
                    string unicode = row.UnicodeBox?.Text.Trim() ?? string.Empty;

                    // Skip empty rows
                    if (string.IsNullOrWhiteSpace(ascii) || string.IsNullOrWhiteSpace(unicode))
                        continue;

                    // Avoid duplicates (use last one)
                    newMappings[ascii] = unicode;
                }

                CustomMappingsHelper.SaveMappings(newMappings);
                SimpleLogger.Log($"Saved {newMappings.Count} custom mappings");

                // Reset the converter cache to reload with new mappings
                ConversionHelper.ResetConverter();

                MessageBox.Show(
                    $"Successfully saved {newMappings.Count} custom mappings!\n\nThey will be used in the next conversion.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "Failed to save custom mappings");
                MessageBox.Show(
                    $"Failed to save mappings:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    internal class MappingRowUI
    {
        public Grid? RowGrid { get; set; }
        public TextBox? AsciiBox { get; set; }
        public TextBox? UnicodeBox { get; set; }
        public Button? RemoveButton { get; set; }
    }
}
