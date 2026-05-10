using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text.Json;

namespace KannadaNudiEditor.Views
{
    public partial class NudiGuideWindow : Window
    {
        public NudiGuideWindow()
        {
            InitializeComponent();
            LoadGuideFromJson();
            LoadKeyboardImage();
        }

        private void LoadGuideFromJson()
        {
            try
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UserGuide", "KeyboardGuide.json");
                SimpleLogger.Log($"Loading keyboard guide from: {jsonPath}");

                if (!File.Exists(jsonPath))
                {
                    SimpleLogger.LogWarning($"Guide file not found at: {jsonPath}");
                    GuideContent.Children.Add(new TextBlock { Text = "Guide file not found", FontSize = 14, Foreground = System.Windows.Media.Brushes.Red });
                    return;
                }

                string jsonContent = File.ReadAllText(jsonPath);
                SimpleLogger.Log($"JSON file loaded successfully. Content length: {jsonContent.Length} bytes");

                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    SimpleLogger.Log($"JSON root element has {root.GetProperty("KeyboardGuide").ValueKind} properties");

                    JsonElement guide = root.GetProperty("KeyboardGuide");
                    SimpleLogger.Log($"KeyboardGuide element loaded successfully");

                    // Title
                    var title = new TextBlock
                    {
                        Text = guide.GetProperty("Title").GetString(),
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 71, 136)),
                        Margin = new Thickness(0, 0, 0, 10),
                        TextAlignment = TextAlignment.Center
                    };
                    GuideContent.Children.Add(title);

                    // Description
                    var description = new TextBlock
                    {
                        Text = guide.GetProperty("Description").GetString(),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 15)
                    };
                    GuideContent.Children.Add(description);

                    // Instructions Header
                    var instructionsHeader = new TextBlock
                    {
                        Text = "ಚಿಹ್ನೆಗಳು ಮತ್ತು ಕೀಬೋರ್ಡ್ ಮ್ಯಾಪಿಂಗ್:",
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(31, 71, 136)),
                        Margin = new Thickness(0, 10, 0, 10)
                    };
                    GuideContent.Children.Add(instructionsHeader);

                    // Symbols Table
                    var symbolsArray = guide.GetProperty("SymbolsWithCapsLock");
                    SimpleLogger.Log($"Processing {symbolsArray.GetArrayLength()} symbols");

                    int rowIndex = 0;
                    foreach (var item in symbolsArray.EnumerateArray())
                    {
                        try
                        {
                            // Each item is an object with one property: the symbol string as key, and the key press as value
                            using (var enumerator = item.EnumerateObject())
                            {
                                if (enumerator.MoveNext())
                                {
                                    var property = enumerator.Current;
                                    string symbolText = property.Name;
                                    string keyValue = property.Value.GetRawText().Trim('"');

                                    // Create a grid for each symbol row
                                    Grid symbolGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                                    symbolGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                                    symbolGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                    // Symbol and keys
                                    var symbolBlock = new TextBlock
                                    {
                                        Text = symbolText,
                                        FontSize = 14,
                                        Padding = new Thickness(10),
                                        Background = rowIndex % 2 == 0 ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)) : System.Windows.Media.Brushes.White,
                                        TextWrapping = TextWrapping.Wrap
                                    };
                                    Grid.SetColumn(symbolBlock, 0);
                                    symbolGrid.Children.Add(symbolBlock);

                                    // Key press info
                                    string keyText = keyValue;
                                    var keyBlock = new TextBlock
                                    {
                                        Text = keyText,
                                        FontSize = 14,
                                        Padding = new Thickness(10),
                                        Background = rowIndex % 2 == 0 ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(240, 240, 240)) : System.Windows.Media.Brushes.White,
                                        TextWrapping = TextWrapping.Wrap
                                    };
                                    Grid.SetColumn(keyBlock, 1);
                                    symbolGrid.Children.Add(keyBlock);

                                    GuideContent.Children.Add(symbolGrid);
                                    rowIndex++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SimpleLogger.LogError($"Error processing symbol row {rowIndex}: {ex.Message}");
                            SimpleLogger.LogError($"Stack trace: {ex.StackTrace}");
                        }
                    }

                    SimpleLogger.Log("Guide loaded successfully");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"Error loading guide JSON: {ex.Message}");
                SimpleLogger.LogError($"Stack trace: {ex.StackTrace}");

                var errorText = new TextBlock
                {
                    Text = $"Error loading guide: {ex.Message}",
                    FontSize = 14,
                    Foreground = System.Windows.Media.Brushes.Red
                };
                GuideContent.Children.Add(errorText);
            }
        }

        private void LoadKeyboardImage()
        {
            try
            {
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "UserGuide", "KannadaNewKeyboard.gif");
                SimpleLogger.Log($"Loading keyboard image from: {imagePath}");

                if (File.Exists(imagePath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    KeyboardImage.Source = bitmap;
                    SimpleLogger.Log($"Keyboard image loaded successfully");
                }
                else
                {
                    SimpleLogger.LogWarning($"Keyboard image file not found at: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                SimpleLogger.LogError($"Failed to load keyboard image: {ex.Message}");
                SimpleLogger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}

