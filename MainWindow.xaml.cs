using Microsoft.Win32;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using KannadaNudiEditor.Helpers;
using KannadaNudiEditor.Views.HeaderFooter;
using KannadaNudiEditor.Views.Sort;
using System.Collections.ObjectModel;
using PageSize = KannadaNudiEditor.Helpers.PageSize;
using KannadaNudiEditor.Views.Loading;
using KannadaNudiEditor.Views.PageNumber;
using KannadaNudiEditor.Views.Common;


namespace KannadaNudiEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        #region Feilds
#if !Framework3_5
        Task<bool>? loadAsync = null;
        CancellationTokenSource? cancellationTokenSource = null;
        private readonly RibbonGallery? ribbonGallery = null;
        private readonly RibbonButton? RibbonButton = null;
        Dictionary<string, List<double>>? pageMarginsCollection = null;
        Dictionary<string, List<double>>? pageSizesCollection = null;
        private string currentFilePath = string.Empty;

        #region Page Fields
        private string customTopMargin;
        private string customBottomMargin;
        private string customLeftMargin;
        private string customRightMargin;
        private string customMarginUnit;

        private string customPageHeight;
        private string customPageWidth;

        private string customSizeUnit = "in";   // default once at startup

        #endregion

#endif
        #endregion


        private Process? _speechProcess;
        private StreamWriter? _pythonInput;
        private Task? _outputReaderTask;


        private bool _isListeningKannada = false;
        private bool _isListeningEnglish = false;

        private SpellChecker spellChecker;




        // field so every handler can see it
        private PageMargins _customMarginsItem;
        private ObservableCollection<PageMargins> _marginItems;   // replaces List


        private PageSize? _customSizeItem; // Holds the "Custom" row instance
        private ObservableCollection<PageSize>? _sizeItems; // Binds to pageSize.ItemsSource

        // Near top of MainWindow class
        private bool _ignorePageSizeChange = false;

        private bool _isDocumentModified = false;


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
#if !Framework3_5
            // Enables touch manipulation.
            richTextBoxAdv.IsManipulationEnabled = true;
#endif
            DataContext = richTextBoxAdv;

            richTextBoxAdv.DocumentTitle = "Untitled";
            richTextBoxAdv.RequestNavigate += RichTextBoxAdv_RequestNavigate;
            richTextBoxAdv.SelectionChanged += RichTextBoxAdv_SelectionChanged;
            richTextBoxAdv.DocumentChanged += RichTextBoxAdv_DocumentChanged;

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
            ribbon.DataContextChanged += Ribbon_DataContextChanged;

            ribbonGallery = PART_RibbonGallery;

            if (fontColorPicker != null)
            {
                fontColorPicker.SetBinding(ColorPickerPalette.ColorProperty,
                    new Binding()
                    {
                        Path = new PropertyPath("Selection.CharacterFormat.FontColor"),
                        Mode = BindingMode.TwoWay,
                        Converter = new FontColorConverter()
                    });
            }

            UpdateRichTextBoxAdvItems();
            InitializePageMargins();
            InitializePageSizes();
            ApplyDefaultPageSettings();
            ConfigureSpellChecker();
            SimpleLogger.Log("MainWindow initialized.");
        }
        #endregion



        private void ConfigureSpellChecker()
        {
            SimpleLogger.Log("=== SpellChecker Configuration Started ===");

            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                SimpleLogger.Log($"Base Directory: {basePath}");

                // Main dictionary (read-only)
                string dictionaryPath = Path.Combine(basePath, "Assets", "kn_IN.dic");
                SimpleLogger.Log($"Main Dictionary Path: {dictionaryPath}");

                if (!File.Exists(dictionaryPath))
                {
                    SimpleLogger.Log($"ERROR: Missing main dictionary at {dictionaryPath}");
                    MessageBox.Show("Missing main dictionary: " + dictionaryPath);
                }

                // Custom dictionaries — writable in AppData
                string customDictionaryPath1 = DictionaryHelper.GetWritableDictionaryPath("Custom_MyDictionary_kn_IN.dic");
                string customDictionaryPath2 = DictionaryHelper.GetWritableDictionaryPath("default.dic");

                SimpleLogger.Log($"Custom Dictionary 1 (AppData): {customDictionaryPath1}");
                SimpleLogger.Log($"Custom Dictionary 2 (AppData): {customDictionaryPath2}");

                // Check existence
                if (!File.Exists(customDictionaryPath1))
                    SimpleLogger.Log($"ERROR: Missing custom dictionary: {customDictionaryPath1}");

                if (!File.Exists(customDictionaryPath2))
                    SimpleLogger.Log($"ERROR: Missing custom dictionary: {customDictionaryPath2}");

                // Initialize SpellChecker
                spellChecker = new SpellChecker
                {
                    IsEnabled = false,
                    IgnoreUppercaseWords = false,
                    IgnoreAlphaNumericWords = true,
                    UseFrameworkSpellCheck = false,
                };

                SimpleLogger.Log("Adding dictionaries to SpellChecker...");

                spellChecker.Dictionaries.Add(dictionaryPath);
                SimpleLogger.Log("Main dictionary added.");

                spellChecker.CustomDictionaries.Add(customDictionaryPath1);
                SimpleLogger.Log("Custom dictionary 1 added.");

                spellChecker.CustomDictionaries.Add(customDictionaryPath2);
                SimpleLogger.Log("Custom dictionary 2 added.");

                richTextBoxAdv.SpellChecker = spellChecker;
                SimpleLogger.Log("Assigned SpellChecker to RichTextBoxAdv.");

                SimpleLogger.Log("=== SpellChecker Configuration Completed Successfully ===");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"EXCEPTION in ConfigureSpellChecker: {ex}");
                MessageBox.Show("SpellChecker failed to initialize:\n" + ex.Message);
            }
        }


        #region Events
        private void RichTextBoxAdv_DocumentChanged(object obj, DocumentChangedEventArgs args)
        {
            if (ribbonGallery != null && ribbonGallery.Items != null)
            {
                ribbonGallery.Items.Clear();
                AddRibbonGalleryItems();
            }
            _isDocumentModified = true;
            SimpleLogger.Log("RichTextBoxAdv_DocumentChanged called ...");
        }

        private void Ribbon_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SfRichTextBoxAdv sfRichTextBoxAdv = null;
            if ((sfRichTextBoxAdv = e.NewValue as SfRichTextBoxAdv) != null)
            {
                if (sfRichTextBoxAdv.Document != null)
                {
                    sfRichTextBoxAdv.Document.Styles.CollectionChanged += Styles_CollectionChanged; ;
                }
            }
        }

        private void Styles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ribbonGallery != null && ribbonGallery.Items != null)
            {
                DocumentStyle newStyle = null;
                if ((newStyle = richTextBoxAdv.Document.Styles[e.NewStartingIndex] as ParagraphStyle) != null)
                {
                    if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (string.IsNullOrEmpty(newStyle.LinkStyleName))
                        {
                            AddRibbonGalleryItem(newStyle, 0, false);
                        }
                        else
                        {
                            int i = 0;
                            foreach (RibbonGalleryItem ribItem in ribbonGallery.Items)
                            {
                                Grid grid = ribItem.Content as Grid;
                                TextBlock textBlock = grid.Children[1] as TextBlock;
                                DocumentStyle style = GetStyle(textBlock.Text);
                                if (!string.IsNullOrEmpty(style.LinkStyleName))
                                {
                                    AddRibbonGalleryItem(newStyle, i, false);
                                    break;
                                }
                                i++;
                            }
                        }
                    }
                    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                    {
                        ribbonGallery.Items.Clear();
                        AddRibbonGalleryItems();
                    }
                    else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                    {
                        int i = 0;
                        foreach (RibbonGalleryItem ribItem in ribbonGallery.Items)
                        {
                            Grid grid = ribItem.Content as Grid;
                            TextBlock textBlock = grid.Children[1] as TextBlock;

                            if (newStyle.Name == textBlock.Text)
                            {
                                AddRibbonGalleryItem(newStyle, i, true);
                                break;
                            }
                            i++;
                        }
                    }
                }
            }
        }



        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ribbon != null)
                ribbon.Loaded += Ribbon_Loaded;
        }

        void RichTextBoxAdv_RequestNavigate(object obj, Syncfusion.Windows.Controls.RichTextBoxAdv.RequestNavigateEventArgs args)
        {
            if (args.Hyperlink.LinkType == Syncfusion.Windows.Controls.RichTextBoxAdv.HyperlinkType.Webpage || args.Hyperlink.LinkType == Syncfusion.Windows.Controls.RichTextBoxAdv.HyperlinkType.Email)
                LaunchUri(new Uri(args.Hyperlink.NavigationLink).AbsoluteUri);
            else if (args.Hyperlink.LinkType == HyperlinkType.File && File.Exists(args.Hyperlink.NavigationLink))
                LaunchUri(args.Hyperlink.NavigationLink);
            _isDocumentModified = true;
        }

        private void LaunchUri(string navigationLink)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo(navigationLink) { UseShellExecute = true };
            process.Start();
        }

        /// <summary>
        /// Called on RichTextBoxAdv selection changes.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Syncfusion.Windows.Controls.RichTextBoxAdv.SelectionChangedEventArgs">SelectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void RichTextBoxAdv_SelectionChanged(object obj, Syncfusion.Windows.Controls.RichTextBoxAdv.SelectionChangedEventArgs args)
        {
            UpdateRichTextBoxAdvItems();
            SelectionAdv currentSelection = (obj as SfRichTextBoxAdv).Selection;
            if (ribbonGallery != null && !string.IsNullOrEmpty(currentSelection.Start.Paragraph.ParagraphFormat.StyleName))
            {
                int i = 0;
                foreach (RibbonGalleryItem ribItem in ribbonGallery.Items)
                {
                    i++;
                    Grid grid = ribItem.Content as Grid;
                    TextBlock textBlock = grid.Children[1] as TextBlock;
                    if (textBlock.Text == currentSelection.Start.Paragraph.ParagraphFormat.StyleName)
                    {
                        ribbonGallery.SelectedItem = ribbonGallery.Items[i - 1];
                    }
                }
            }
            _isDocumentModified = true;
        }
        /// <summary>
        /// update the page and word counts of RichTextBoxAdv
        /// </summary>
        private void UpdateRichTextBoxAdvItems()
        {
            pagecountRun.RunText = richTextBoxAdv.PageCount.ToString();
            currentPageNumberRun.RunText = richTextBoxAdv.CurrentPageNumber.ToString();
            totalWords.RunText = richTextBoxAdv.WordCount.ToString();
        }
        /// <summary>
        /// Calle when [unloaded].
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= OnUnloaded;
            this.Loaded -= OnLoaded;
            DisposeRibbon();
            if (richTextBoxAdv != null)
            {
                richTextBoxAdv.SelectionChanged -= RichTextBoxAdv_SelectionChanged;
                richTextBoxAdv.RequestNavigate -= RichTextBoxAdv_RequestNavigate;
                richTextBoxAdv.Dispose();
                richTextBoxAdv = null;
            }
        }
        #endregion

        #region Initiate Bindings
        /// <summary>
        /// Called on ribbon loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>



        private void Ribbon_Loaded(object sender, RoutedEventArgs e)
        {
            CommandBindings.Add(new CommandBinding(SfRichTextBoxAdv.SaveDocumentCommand, OnSaveExecuted));
            CommandBindings.Add(new CommandBinding(SfRichTextBoxAdv.SaveAsDocumentCommand, OnSaveAsExecuted));
            CommandBindings.Add(new CommandBinding(SfRichTextBoxAdv.PrintDocumentCommand, OnPrintExecuted));
            CommandBindings.Add(new CommandBinding(SfRichTextBoxAdv.OpenDocumentCommand, OnOpenExecuted));
            CommandBindings.Add(new CommandBinding(SfRichTextBoxAdv.NewDocumentCommand, OnNewExecuted));
            CommandBindings.Add(new CommandBinding(SfRichTextBoxAdv.ShowEncryptDocumentDialogCommand, OnShowEncryptDocumentExecuted));
            WireUpEvents();


            if (fontFamilyComboBox != null)
                fontFamilyComboBox.ItemsSource = GetFontFamilySource();
            fontFamilyComboBox.SelectedValue = "NudiParijatha"; // set default selected value

            if (fontSizeComboBox != null)
                fontSizeComboBox.ItemsSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 26, 28, 36, 48, 72, 96 };

            if (richTextBoxAdv != null)
            {
                // Set default font at the document level
                richTextBoxAdv.Document.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("NudiParijatha");

                // Optionally, apply it to current selection too
                richTextBoxAdv.Selection.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("NudiParijatha");

                richTextBoxAdv.Focus();
            }

            if (ribbonGallery != null && ribbonGallery.Items != null)
            {
                ribbonGallery.Items.Clear();
                AddRibbonGalleryItems();
            }
        }





        private async void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                LoadingView.Show();
                await Task.Delay(100);

                bool saveSuccess = await NudiFileManager.SaveToFileAsync(currentFilePath, richTextBoxAdv, () =>
                {
                    NudiFileManager.SaveAs(".docx", WordExport);
                });

                if (saveSuccess)
                {
                    _isDocumentModified = false;
                    SimpleLogger.Log($"Document saved successfully to '{currentFilePath ?? "new file"}'.");
                }
                else
                {
                    string msg = "Document save was canceled or failed. Please check file permissions or path.";
                    MessageBox.Show(msg, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SimpleLogger.Log(msg);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected error while saving:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                MessageBox.Show(errorMsg, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SimpleLogger.LogException(ex, "Unexpected error during Save operation");
            }
            finally
            {
                LoadingView.Hide();
                richTextBoxAdv.Focus();
                ribbon.IsBackStageVisible = false;
            }
        }
        private async void OnSaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                LoadingView.Show();
                await Task.Delay(100);

                string extension = e?.Parameter?.ToString() ?? ".docx";

                try
                {
                    NudiFileManager.SaveAs(extension, WordExport);
                    _isDocumentModified = false;
                    SimpleLogger.Log($"Document saved successfully using 'Save As' with extension '{extension}'.");
                }
                catch (Exception exSaveAs)
                {
                    string msg = $"Save As failed:\n{exSaveAs.Message}\n\nStack Trace:\n{exSaveAs.StackTrace}";
                    MessageBox.Show(msg, "Save As Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    SimpleLogger.LogException(exSaveAs, "Save As failed");
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected error during Save As:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                MessageBox.Show(errorMsg, "Save As Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SimpleLogger.LogException(ex, "Unexpected error during Save As operation");
            }
            finally
            {
                LoadingView.Hide();
                richTextBoxAdv.Focus();
                ribbon.IsBackStageVisible = false;
            }
        }

        private async void OnPrintExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Show loading view
                LoadingView.Show();

                await Task.Run(() =>
                {
                    // Run print operation on background thread
                    Dispatcher.Invoke(() =>
                    {
                        richTextBoxAdv.PrintDocument();
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Ensure UI restored
                LoadingView.Hide();
                richTextBoxAdv.Focus();
                ribbon.IsBackStageVisible = false;
            }
        }


        private async void OnNewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Show loading
                LoadingView.Show();

                await Task.Run(() =>
                {
                    // Simulate slight delay if needed (optional)
                    System.Threading.Thread.Sleep(100);
                });

                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open new window:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingView.Hide();
                richTextBoxAdv.Focus();
                ribbon.IsBackStageVisible = false;
            }
        }


        private async void OnOpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                LoadingView.Show(); // Show loading UI

                await Task.Run(() =>
                {
                    // Run WordImport in background thread
                    Application.Current.Dispatcher.Invoke(() => WordImport());
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while opening document:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingView.Hide(); // Hide loading in any case
                richTextBoxAdv.Focus();
                ribbon.IsBackStageVisible = false;
            }
        }


        private void OnShowEncryptDocumentExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CloseBackstage();
            SfRichTextBoxAdv.ShowEncryptDocumentDialogCommand.Execute(null, richTextBoxAdv);
        }




        private async void pdfSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseBackstage();
                SimpleLogger.Log("PDF Save initiated from backstage.");

                string currentFont = richTextBoxAdv.Selection.CharacterFormat.FontFamily.Source;

                SimpleLogger.Log("Current Font in RichTextBoxAdv: " + currentFont);

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Document (*.pdf)|*.pdf",
                    Title = "Save as PDF"
                };

                if (saveDialog.ShowDialog() != true)
                    return;

                string filePath = saveDialog.FileName;

                LoadingView.Show();

                await Task.Run(() =>
                {
                    DocumentExportHelper.ExportToPdf(
                        richTextBoxAdv,
                        filePath,
                        currentFont
                    );
                });

                SimpleLogger.Log($"PDF exported successfully: {filePath}");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"PDF export FAILED: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Failed to export PDF:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                LoadingView.Hide();
            }

            MessageBox.Show("PDF exported successfully!",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void mdSave_Click(object sender, RoutedEventArgs e)
        {
            CloseBackstage();

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Markdown File (*.md)|*.md",
                Title = "Save as Markdown"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            string filePath = saveDialog.FileName;

            try
            {
                LoadingView.Show();

                await Task.Run(() =>
                {
                    // Export Markdown directly, no need for Dispatcher.Invoke here
                    DocumentExportHelper.ExportToMarkdown(richTextBoxAdv, filePath);
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Markdown export FAILED: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Failed to export Markdown:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                LoadingView.Hide();
            }

            // Show success message after loading overlay is hidden
            MessageBox.Show("Markdown exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void rtfSave_Click(object sender, RoutedEventArgs e)
        {
            CloseBackstage();

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "RTF Document (*.rtf)|*.rtf",
                Title = "Save as RTF"
            };

            if (dialog.ShowDialog() != true)
                return;

            string filePath = dialog.FileName;

            try
            {
                LoadingView.Show();

                await Task.Run(() =>
                {
                    DocumentExportHelper.ExportToRtf(richTextBoxAdv, filePath);
                });
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"RTF export FAILED: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Failed to export RTF:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                LoadingView.Hide();
            }

            MessageBox.Show("RTF exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }





        void OnlineHelpButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingView.Show();

            try
            {
                LaunchUri(new Uri("https://kagapa.com/").AbsoluteUri);
            }
            finally
            {
                LoadingView.Hide();
                CloseBackstage();
            }
        }

        void GettingStartedButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingView.Show();

            try
            {
                LaunchUri(new Uri("https://kagapa.com/").AbsoluteUri);
            }
            finally
            {
                LoadingView.Hide();
                CloseBackstage();
            }
        }


        private void WireUpEvents()
        {
            increaseFontSizeButton.Click += IncreaseFontSizeButton_Click;
            decreaseFontSizeButton.Click += DecreaseFontSizeButton_Click;
            fontColorSplitButton.Click += FontColorSplitButton_Click;
            fontColorPicker.ColorChanged += FontColorPicker_ColorChanged;
            highlightColorSplitButton.Click += HighlightColorSplitButton_Click;
            noHighlightButton.Click += DropDownItem_Click;
            yellowHighlightButton.Click += DropDownItem_Click;
            brightGreenHighlightButton.Click += DropDownItem_Click;
            turquoiseHighlightButton.Click += DropDownItem_Click;
            pinkHighlightButton.Click += DropDownItem_Click;
            blueHighlightButton.Click += DropDownItem_Click;
            redHighlightButton.Click += DropDownItem_Click;
            darkBlueHighlightButton.Click += DropDownItem_Click;
            tealHighlightButton.Click += DropDownItem_Click;
            greenHighlightButton.Click += DropDownItem_Click;
            violetHighlightButton.Click += DropDownItem_Click;
            darkRedHighlightButton.Click += DropDownItem_Click;
            darkYellowHighlightButton.Click += DropDownItem_Click;
            darkGrayHighlightButton.Click += DropDownItem_Click;
            lightGrayHighlightButton.Click += DropDownItem_Click;
            blackHighlightButton.Click += DropDownItem_Click;
            foreach (MenuItem lineSpacingItem in lineSpacingMenuGroup.Items)
                lineSpacingItem.Click += LineSpacingItem_Click;
            restrictEditingButton.IsSelectedChanged += RestrictEditingButton_IsSelectedChanged;
            tablePicker.Click += TablePicker_Click;
            insertTableButton.Click += DropDownItem_Click;
            zoomInButton.Click += ZoomInButton_Click;
            zoomOutButton.Click += ZoomOutButton_Click;
            InitListOptions();
        }


        void IncreaseFontSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null)
            {
                double[] fontSizeSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 26, 28, 36, 48, 72, 96 };
                if (richTextBoxAdv.Selection.CharacterFormat.FontSize >= fontSizeSource[fontSizeSource.Length - 1])
                    richTextBoxAdv.Selection.CharacterFormat.FontSize += 10;
                else if (fontSizeComboBox.SelectedIndex < 0)
                {
                    if (richTextBoxAdv.Selection.CharacterFormat.FontSize < 1)
                        richTextBoxAdv.Selection.CharacterFormat.FontSize = 8;
                    else if (richTextBoxAdv.Selection.CharacterFormat.FontSize < 8)
                        richTextBoxAdv.Selection.CharacterFormat.FontSize += 1;
                    else
                        richTextBoxAdv.Selection.CharacterFormat.FontSize = fontSizeSource.OrderBy(d => Math.Abs(d - richTextBoxAdv.Selection.CharacterFormat.FontSize)).ElementAt(1);
                }
                else
                    fontSizeComboBox.SelectedIndex += 1;
            }
        }

        void DecreaseFontSizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null)
            {
                if (richTextBoxAdv.Selection.CharacterFormat.FontSize - 1 <= 0)
                    return;
                double[] fontSizeSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 26, 28, 36, 48, 72, 96 };
                if (fontSizeComboBox.SelectedIndex > 0)
                    fontSizeComboBox.SelectedIndex -= 1;
                else if (richTextBoxAdv.Selection.CharacterFormat.FontSize <= 8)
                    richTextBoxAdv.Selection.CharacterFormat.FontSize -= 1;
                else if (richTextBoxAdv.Selection.CharacterFormat.FontSize - 10 > fontSizeSource[fontSizeSource.Length - 1])
                    richTextBoxAdv.Selection.CharacterFormat.FontSize -= 10;
                else
                    richTextBoxAdv.Selection.CharacterFormat.FontSize = fontSizeSource.OrderBy(d => Math.Abs(d - richTextBoxAdv.Selection.CharacterFormat.FontSize)).ElementAt(0);
            }
        }
        void FontColorPicker_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitbutton = (SplitButton)fontColorPicker.Parent;
            if (splitbutton != null)
                splitbutton.IsDropDownOpen = false;
        }
        void FontColorSplitButton_Click(object sender, RoutedEventArgs e)
        {
            if (fontColorPicker != null && richTextBoxAdv != null)
            {
                if (fontColorPicker.RecentlyUsedCollection.Count > 0 && fontColorPicker.RecentlyUsedCollection[0].Color is SolidColorBrush)
                    richTextBoxAdv.Selection.CharacterFormat.FontColor = (fontColorPicker.RecentlyUsedCollection[0].Color as SolidColorBrush).Color;
                else
                    richTextBoxAdv.Selection.CharacterFormat.FontColor = Color.FromArgb(0x00, 0xff, 0x00, 0x00);
            }
        }


        void HighlightColorSplitButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null && richTextBoxAdv.Selection.CharacterFormat.HighlightColor != HighlightColor.NoColor)
                richTextBoxAdv.Selection.CharacterFormat.HighlightColor = HighlightColor.NoColor;
            else
                richTextBoxAdv.Selection.CharacterFormat.HighlightColor = HighlightColor.Yellow;
        }
        void BackstageButton_Click(object sender, RoutedEventArgs e)
        {
            CloseBackstage();
        }

        void NewDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            CloseBackstage();
            if (richTextBoxAdv != null)
                richTextBoxAdv.DocumentTitle = "Untitled";
        }
        /// <summary>
        /// Closed the backstage.
        /// </summary>
        /// <remarks></remarks>
        void CloseBackstage()
        {
            if (ribbon != null && ribbon.BackStageButton != null)
                ribbon.BackStageButton.IsOpen = false;
            if (richTextBoxAdv != null)
                richTextBoxAdv.Focus();
        }
        /// <summary>
        /// Called on backstage restrict editing button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void BackstageRestrictEditingButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null && !richTextBoxAdv.IsReadOnly)
            {
                richTextBoxAdv.IsReadOnly = true;
                OnRestrictEditing();
            }
            CloseBackstage();
        }
        /// <summary>
        /// Called on backstage encrypt document button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void BackstageEncryptDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            CloseBackstage();
        }
        /// <summary>
        /// Called on restrict editing button selection changed.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:System.Windows.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void RestrictEditingButton_IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnRestrictEditing();
        }
        /// <summary>
        /// On restrict editing.
        /// </summary>
        /// <remarks></remarks>


        private void OnRestrictEditing()
        {
            if (richTextBoxAdv != null)
            {
                IEnumerable tabs = ribbon.Items;
                string visual = SkinStorage.GetVisualStyle(this);
                foreach (var tab in tabs)
                {
                    RibbonTab ribbontab = tab as RibbonTab;
                    if (ribbontab != null)
                    {
                        foreach (var bar in ribbontab.Items)
                        {
                            RibbonBar ribbonBar = bar as RibbonBar;
                            if (ribbonBar != null)
                            {
                                // Options that should work on read only mode should not be disabled.
                                // Currently, Copy, Find and ShowComments options.
                                if (ribbonBar.Header == (string)FindResource("Clipboard"))
                                {
                                    foreach (var item in ribbonBar.Items)
                                    {
                                        RibbonButton button = item as RibbonButton;
                                        if (button != null && button.Label != (string)FindResource("Copy"))
                                        {
                                            if (richTextBoxAdv.IsReadOnly)
                                                button.IsEnabled = false;
                                            else
                                                button.IsEnabled = true;
                                        }
                                    }
                                }
                                else if (ribbonBar.Header == (string)FindResource("EditingHeader"))
                                {
                                    foreach (var item in ribbonBar.Items)
                                    {
                                        RibbonButton button = item as RibbonButton;
                                        if (button != null && button.Label == (string)FindResource("Replace"))
                                        {
                                            if (richTextBoxAdv.IsReadOnly)
                                                button.IsEnabled = false;
                                            else
                                                button.IsEnabled = true;
                                        }
                                    }
                                }
                                else if (ribbonBar.Header == (string)FindResource("Comments"))
                                {
                                    foreach (var item in ribbonBar.Items)
                                    {
                                        RibbonButton button = item as RibbonButton;
                                        if (button != null && button.Label != (string)FindResource("ShowComments"))
                                        {
                                            if (richTextBoxAdv.IsReadOnly)
                                                button.IsEnabled = false;
                                            else
                                                button.IsEnabled = true;
                                        }
                                    }
                                }
                                else if (ribbonBar.Header != "Page SetUp" && ribbonBar.Header != "Zoom" && ribbonBar.Header != "Color Scheme")
                                {
                                    if (richTextBoxAdv.IsReadOnly)
                                    {
                                        ribbonBar.IsEnabled = false;
                                        if (visual == "Office2010Blue" || visual == "Office2010Black" || visual == "Office2010Silver")
                                            ribbonBar.Effect = new DisableEffect();
                                    }
                                    else
                                    {
                                        ribbonBar.Effect = null;
                                        ribbonBar.IsEnabled = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// On table picker clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void TablePicker_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null && sender is TablePickerUI)
            {
                TablePickerUI? tablePicker = sender as TablePickerUI;
                int[] tableSize = new int[] { tablePicker.SelectedCell.Row + 1, tablePicker.SelectedCell.Column + 1 };
                tablePicker.CommandParameter = tableSize;
                CloseDropDown(tablePicker.Parent);
            }
        }
        /// <summary>
        /// Occurs when drop down item click
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void DropDownItem_Click(object sender, RoutedEventArgs e)
        {
            CloseDropDown((sender as FrameworkElement).Parent);
        }
        /// <summary>
        /// Closes the drop down.
        /// </summary>
        /// <param name="obj"></param>
        /// <remarks></remarks>
        void CloseDropDown(object obj)
        {
            while (!(obj is DropDownButton || obj is SplitButton))
            {
                obj = (obj as FrameworkElement).Parent;
            }
            // SplitButton is derived from DropDown only. Hence no need to handle it specifically.
            if (obj is DropDownButton)
                (obj as DropDownButton).IsDropDownOpen = false;
        }
        /// <summary>
        /// Initiates the List options
        /// </summary>
        /// <remarks></remarks>
        private void InitListOptions()
        {
            ListViewModel viewModel = new ListViewModel(richTextBoxAdv);
            noBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "NoList" });
            noBulletButton.Click += DropDownItem_Click;
            arrowBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Bullet_Arrow" });
            arrowBulletButton.Click += DropDownItem_Click;
            dotBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Bullet_Dot" });
            dotBulletButton.Click += DropDownItem_Click;
            flowerBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Bullet_Flower" });
            flowerBulletButton.Click += DropDownItem_Click;
            circleBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Bullet_Circle" });
            circleBulletButton.Click += DropDownItem_Click;
            squareBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Bullet_Square" });
            squareBulletButton.Click += DropDownItem_Click;
            tickBulletButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Bullet_Tick" });
            tickBulletButton.Click += DropDownItem_Click;
            noNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "NoList" });
            noNumberingButton.Click += DropDownItem_Click;
            numberDotNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_Number_Dot" });
            numberDotNumberingButton.Click += DropDownItem_Click;
            numberBraceNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_Number_Brace" });
            numberBraceNumberingButton.Click += DropDownItem_Click;
            lowLetterDotNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_LowLetter_Dot" });
            lowLetterDotNumberingButton.Click += DropDownItem_Click;
            lowLetterBraceNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_LowLetter_Brace" });
            lowLetterBraceNumberingButton.Click += DropDownItem_Click;
            upLetterNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_UpLetter" });
            upLetterNumberingButton.Click += DropDownItem_Click;
            upRomanNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_UpRoman" });
            upRomanNumberingButton.Click += DropDownItem_Click;
            lowRomanNumberingButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_Numbering_LowRoman" });
            lowRomanNumberingButton.Click += DropDownItem_Click;
            noListButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Source = viewModel, Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "NoList" });
            noListButton.Click += DropDownItem_Click;
            bulletedListButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Source = viewModel, Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_List_Bullet" });
            bulletedListButton.Click += DropDownItem_Click;
            normalListButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Source = viewModel, Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_List_Normal" });
            normalListButton.Click += DropDownItem_Click;
            multilevelListButton.SetBinding(ToggleButton.IsCheckedProperty, new Binding() { Source = viewModel, Path = new PropertyPath("ListName"), Converter = new ListToggleConverter(), ConverterParameter = "_List_Multilevel" });
            multilevelListButton.Click += DropDownItem_Click;
            bulletedListSplitButton.DataContext = viewModel;
            bulletedListSplitButton.Click += BulletedListSplitButton_Click;
            numberedListSplitButton.DataContext = viewModel;
            numberedListSplitButton.Click += NumberedListSplitButton_Click;
        }
        /// <summary>
        /// Called on bulleted list split button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void BulletedListSplitButton_Click(object sender, RoutedEventArgs e)
        {
            ListViewModel viewModel = (sender as FrameworkElement).DataContext as ListViewModel;
            if (string.IsNullOrEmpty(viewModel.ListName) || viewModel.ListName == "NoList" || viewModel.ListName == "Null" || !viewModel.ListName.StartsWith("_Bullet"))
                viewModel.ListName = "_Bullet_Dot";
            else
                viewModel.ListName = "NoList";
        }
        /// <summary>
        /// Called on numbered list split button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void NumberedListSplitButton_Click(object sender, RoutedEventArgs e)
        {
            ListViewModel viewModel = (sender as FrameworkElement).DataContext as ListViewModel;
            if (string.IsNullOrEmpty(viewModel.ListName) || viewModel.ListName == "NoList" || viewModel.ListName == "Null" || !viewModel.ListName.StartsWith("_Numbering"))
                viewModel.ListName = "_Numbering_Number_Dot";
            else
                viewModel.ListName = "NoList";
        }
        /// <summary>
        /// Called on line spacing item clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void LineSpacingItem_Click(object sender, RoutedEventArgs e)
        {
            CloseDropDown(sender);
        }
        /// <summary>
        /// Called on zoom in button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null)
            {
                if (richTextBoxAdv.ZoomFactor + 10 > 500)
                    richTextBoxAdv.ZoomFactor = 500;
                else
                    richTextBoxAdv.ZoomFactor += 10;
            }
        }
        /// <summary>
        /// Called on zoom out button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null)
            {
                if (richTextBoxAdv.ZoomFactor - 10 < 10)
                    richTextBoxAdv.ZoomFactor = 10;
                else
                    richTextBoxAdv.ZoomFactor -= 10;
            }
        }
        #endregion

        #region Dispose Bindings
        /// <summary>
        /// Unwires the events.
        /// </summary>
        /// <remarks></remarks>
        private void UnWireEvents()
        {
            increaseFontSizeButton.Click -= IncreaseFontSizeButton_Click;
            decreaseFontSizeButton.Click -= DecreaseFontSizeButton_Click;
            fontColorSplitButton.Click -= FontColorSplitButton_Click;
            fontColorPicker.ColorChanged -= FontColorPicker_ColorChanged;
            highlightColorSplitButton.Click -= HighlightColorSplitButton_Click;
            noHighlightButton.Click -= DropDownItem_Click;
            yellowHighlightButton.Click -= DropDownItem_Click;
            brightGreenHighlightButton.Click -= DropDownItem_Click;
            turquoiseHighlightButton.Click -= DropDownItem_Click;
            pinkHighlightButton.Click -= DropDownItem_Click;
            blueHighlightButton.Click -= DropDownItem_Click;
            redHighlightButton.Click -= DropDownItem_Click;
            darkBlueHighlightButton.Click -= DropDownItem_Click;
            tealHighlightButton.Click -= DropDownItem_Click;
            greenHighlightButton.Click -= DropDownItem_Click;
            violetHighlightButton.Click -= DropDownItem_Click;
            darkRedHighlightButton.Click -= DropDownItem_Click;
            darkYellowHighlightButton.Click -= DropDownItem_Click;
            darkGrayHighlightButton.Click -= DropDownItem_Click;
            lightGrayHighlightButton.Click -= DropDownItem_Click;
            blackHighlightButton.Click -= DropDownItem_Click;
            foreach (MenuItem lineSpacingItem in lineSpacingMenuGroup.Items)
                lineSpacingItem.Click -= LineSpacingItem_Click;
            restrictEditingButton.IsSelectedChanged -= RestrictEditingButton_IsSelectedChanged;
            tablePicker.Click -= TablePicker_Click;
            insertTableButton.Click -= DropDownItem_Click;
            zoomInButton.Click -= ZoomInButton_Click;
            zoomOutButton.Click -= ZoomOutButton_Click;
        }
        /// <summary>
        /// Disposes the list options.
        /// </summary>
        /// <remarks></remarks>
        private void DisposeListOptions()
        {
            BindingOperations.ClearAllBindings(noBulletButton);
            noBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(arrowBulletButton);
            arrowBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(dotBulletButton);
            dotBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(flowerBulletButton);
            flowerBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(circleBulletButton);
            circleBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(squareBulletButton);
            squareBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(tickBulletButton);
            tickBulletButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(noNumberingButton);
            noNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(numberDotNumberingButton);
            numberDotNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(numberBraceNumberingButton);
            numberBraceNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(lowLetterDotNumberingButton);
            lowLetterDotNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(lowLetterBraceNumberingButton);
            lowLetterBraceNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(upLetterNumberingButton);
            upLetterNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(upRomanNumberingButton);
            upRomanNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(lowRomanNumberingButton);
            lowRomanNumberingButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(noListButton);
            noListButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(bulletedListButton);
            bulletedListButton.Click -= DropDownItem_Click;
            BindingOperations.ClearAllBindings(normalListButton);
            normalListButton.Click -= DropDownItem_Click;
            if (multilevelListButton.DataContext is ListViewModel)
                (multilevelListButton.DataContext as ListViewModel).Dispose();
            BindingOperations.ClearAllBindings(multilevelListButton);
            multilevelListButton.Click -= DropDownItem_Click;
            if (bulletedListSplitButton.DataContext is ListViewModel)
                (bulletedListSplitButton.DataContext as ListViewModel).Dispose();
            bulletedListSplitButton.DataContext = null;
            bulletedListSplitButton.Click -= BulletedListSplitButton_Click;
            if (numberedListSplitButton.DataContext is ListViewModel)
                (numberedListSplitButton.DataContext as ListViewModel).Dispose();
            numberedListSplitButton.DataContext = null;
            numberedListSplitButton.Click -= NumberedListSplitButton_Click;
        }
        #endregion

        #region Implementation
        /// <summary>
        /// Words the import.
        /// </summary>
#if Framework3_5 || Framework4_0
        private void WordImport()
#else
        private async void WordImport()
#endif
        {
            OpenFileDialog openDialog = new OpenFileDialog()
            {
                Filter = "All supported files (*.docx,*.doc,*.htm,*.html,*.rtf,*.txt,*.xaml)|*.docx;*.doc;*.htm;*.html;*.rtf,*.txt;*.xaml|Word Document (*.docx)|*.docx|Word 97 - 2003 Document (*.doc)|*.doc|Web Page (*.htm,*.html)|*.htm;*.html|Rich Text File (*.rtf)|*.rtf|Text File (*.txt)|*.txt|Xaml File (*.xaml)|*.xaml",
                FilterIndex = 1,
                Multiselect = false
            };
            if ((bool)openDialog.ShowDialog() && richTextBoxAdv != null)
            {
                Stream fileStream = openDialog.OpenFile();
                FileInfo file = new FileInfo(openDialog.FileName);
                string fileName = file.Name;
                string fileExtension = file.Extension;
                if (!string.IsNullOrEmpty(fileExtension) && fileStream != null)
                {
                    FormatType formatType = GetFormatType(fileExtension);
#if Framework3_5
                    RichTextBoxAdv.Load(fileStream, formatType);
#else
                    if (loadAsync != null && !loadAsync.IsCompleted && !loadAsync.IsFaulted && cancellationTokenSource != null)
                    {
                        cancellationTokenSource.Cancel();
                        try
                        {
                            if (!loadAsync.IsCanceled)
#if Framework4_0
                                loadAsync.Wait();
#else
                                await loadAsync;
#endif
                        }
                        catch
                        { }
                    }
                    try
                    {
                        cancellationTokenSource = new CancellationTokenSource();
#if Framework4_0
                        loadAsync = RichTextBoxAdv.LoadAsync(fileStream, formatType, cancellationTokenSource.Token);
#else
                        loadAsync = richTextBoxAdv.LoadAsync(fileStream, formatType, cancellationTokenSource.Token);
                        await loadAsync;
#endif
                        if (cancellationTokenSource != null)
                            cancellationTokenSource.Dispose();
                        cancellationTokenSource = null;
                        loadAsync = null;
                    }
                    catch
                    { }
#endif
                    richTextBoxAdv.DocumentTitle = fileName.Remove(fileName.LastIndexOf("."));
                    currentFilePath = openDialog.FileName;

                }
            }
        }
        /// <summary>
        /// Words the export.
        /// </summary>
        /// <param name="extension">The extension.</param>
#if Framework3_5 || Framework4_0
        private void WordExport(string extension)
#else
        private async void WordExport(string extension)
#endif
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            {
                saveDialog.Filter = "Word Document (*.docx)|*.docx|Word 97 - 2003 Document (*.doc)|*.doc|Web Page (*.htm,*.html)|*.htm;*.html|Rich Text File (*.rtf)|*.rtf|Text File (*.txt)|*.txt|Xaml File (*.xaml)|*.xaml";
            }
            ;
            if ((bool)saveDialog.ShowDialog())
            {
                Stream fileStream = saveDialog.OpenFile();
                FileInfo file = new FileInfo(saveDialog.FileName);
                string fileExtension = file.Extension;
                if (!string.IsNullOrEmpty(fileExtension))
                {
                    FormatType formatType = GetFormatType(fileExtension);
#if Framework3_5
                    RichTextBoxAdv.Save(fileStream, formatType);
#elif Framework4_0
                    RichTextBoxAdv.SaveAsync(fileStream, formatType);
#else
                    await richTextBoxAdv.SaveAsync(fileStream, formatType);
                    currentFilePath = saveDialog.FileName;

#endif
                }
                fileStream.Close();
            }
        }
        /// <summary>
        /// Gets the format type.
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private FormatType GetFormatType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".doc":
                    return FormatType.Doc;
                case ".rtf":
                    return FormatType.Rtf;
                case ".txt":
                    return FormatType.Txt;
                case ".htm":
                case ".html":
                    return FormatType.Html;
                case ".xaml":
                    return FormatType.Xaml;
                default:
                    return FormatType.Docx;
            }
        }
        /// <summary>
        /// Adds the font family source.
        /// </summary>
        private List<string> GetFontFamilySource()
        {
            List<string> fontFamilySource = new List<string>();
            foreach (FontFamily fontfamily in Fonts.SystemFontFamilies)
            {
                fontFamilySource.Add(fontfamily.Source);
            }

            // Sort alphabetically
            fontFamilySource.Sort();

            return fontFamilySource;
        }
        internal DocumentStyle GetStyle(string styleName)
        {
            foreach (DocumentStyle style in richTextBoxAdv.Document.Styles)
            {
                if (style.Name == styleName)
                {
                    return style;
                }
            }

            return null;
        }

        private void AddRibbonGalleryItems()
        {
            if (richTextBoxAdv == null || richTextBoxAdv.Document == null)
                return;
            foreach (DocumentStyle style in richTextBoxAdv.Document.Styles)
            {
                if (style.Type == StyleType.ParagraphStyle)
                {
                    AddRibbonGalleryItem(style, null, false);
                }
            }
        }

        private void AddRibbonGalleryItem(DocumentStyle style, int? index, bool isReplace)
        {
            ParagraphStyle paragraphStyle = (ParagraphStyle)style;
            Grid grid = new Grid();
            grid.Background = new SolidColorBrush(Colors.White);
            RowDefinition rowDefinition1 = new RowDefinition();
            rowDefinition1.Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions.Add(rowDefinition1);

            RowDefinition rowDefinition2 = new RowDefinition();
            rowDefinition2.Height = new GridLength(1, GridUnitType.Star);
            grid.RowDefinitions.Add(rowDefinition2);

            TextBlock styleFormat = new TextBlock();
            //styleFormat.Text = "AaBbCcDdEe";
            styleFormat.Text = "ಅಆಇಈಉಊಋಎ";

            styleFormat.FontWeight = paragraphStyle.CharacterFormat.Bold ? FontWeights.Bold : FontWeights.Normal;
            styleFormat.FontStyle = paragraphStyle.CharacterFormat.Italic ? FontStyles.Italic : FontStyles.Normal;
            if (paragraphStyle.CharacterFormat.Underline != Syncfusion.Windows.Controls.RichTextBoxAdv.Underline.None)
                styleFormat.TextDecorations = TextDecorations.Underline;
            styleFormat.VerticalAlignment = VerticalAlignment.Bottom;
            styleFormat.FontSize = paragraphStyle.CharacterFormat.FontSize;

            Color fontColor = paragraphStyle.CharacterFormat.FontColor;
            //Gets the font color compared to the background color, if font color is empty(auto).
            if (fontColor == Color.FromArgb(0, 0, 0, 0))
            {
                styleFormat.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                styleFormat.Foreground = new SolidColorBrush(paragraphStyle.CharacterFormat.FontColor);
            }
            styleFormat.Margin = new Thickness(1);

            TextBlock styleName = new TextBlock();
            styleName.Text = style.Name;
            styleName.VerticalAlignment = VerticalAlignment.Bottom;
            styleName.TextAlignment = TextAlignment.Center;
            styleName.FontSize = 10;
            styleName.Margin = new Thickness(1, 1, 1, 7);
            styleName.Foreground = new SolidColorBrush(Colors.Black);

            Grid.SetRow(styleFormat, 0);
            Grid.SetRow(styleName, 1);

            grid.Children.Add(styleFormat);
            grid.Children.Add(styleName);

            RibbonGalleryItem ribbonGalleryItem = new RibbonGalleryItem();
            ribbonGalleryItem.Command = SfRichTextBoxAdv.ApplyStyleCommand;
            ribbonGalleryItem.CommandTarget = richTextBoxAdv;
            ribbonGalleryItem.CommandParameter = paragraphStyle.Name;
            ribbonGalleryItem.Content = grid;
            ribbonGalleryItem.Margin = new Thickness(2, 2, 2, 2);
            if (index == null)
            {
                ribbonGallery.Items.Add(ribbonGalleryItem);
            }
            else
            {
                if (isReplace)
                {
                    ribbonGallery.Items[index.GetValueOrDefault()] = ribbonGalleryItem;
                }
                else
                {
                    ribbonGallery.Items.Insert(index.GetValueOrDefault(), ribbonGalleryItem);
                }
            }
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void DisposeRibbon()
        {
            DataContext = null;
            UnWireEvents();
            fontFamilyComboBox = null;
            fontSizeComboBox = null;
            fontColorPicker = null;
            restrictEditingButton = null;
#if !Framework3_5 
#if !Framework4_0
            //Handled to cancel the asynchronous load operation.
            if (loadAsync != null && !loadAsync.IsCompleted && !loadAsync.IsFaulted && cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                try
                {
                    //await loadAsync;
                }
                catch
                { }
            }
#endif
            loadAsync = null;
            cancellationTokenSource = null;
#endif
            if (ribbon != null)
            {
                ribbon.Loaded -= Ribbon_Loaded;
                ribbon.Dispose();
                ribbon = null;
            }
        }
        #endregion




        private void LanguageToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            LanguageManager.SwitchLanguage("en-US");
            LanguageToggleButton.Content = "EN";
        }

        private void LanguageToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            LanguageManager.SwitchLanguage("kn-IN");
            LanguageToggleButton.Content = "ಕ";
        }




        #region PageColor Implementation

        private void pageColorColorPicker_ColorChanged(object sender, SelectedBrushChangedEventArgs e)
        {
            if (e.NewColor != null)
            {
                richTextBoxAdv.Document.Background.Color = e.NewColor;
            }
            else
            {
                richTextBoxAdv.Document.Background.Color = Colors.White;
            }
        }
        private void NoColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the background to no color (transparent)
            richTextBoxAdv.Document.Background.Color = Colors.White;
        }

        #endregion
        #region PageMargins Implementation

        private void InitializePageMargins()
        {
            // ——— live custom row ———
            _customMarginsItem = PageMarginHelper.GetPresetMargins(LanguageToggleButton.IsChecked == true)
                .FirstOrDefault(p => p.Key == "Custom");

            // ——— all items ———
            _marginItems = new ObservableCollection<PageMargins>(
                PageMarginHelper.GetPresetMargins(LanguageToggleButton.IsChecked == true)
            );

            pageMargins.ItemsSource = _marginItems;  // <‑‑ bind

            // preset numeric lookup (inches)
            pageMarginsCollection = PageMarginHelper.GetMarginValues();
        }

        private void pageMargins_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject? current = e.OriginalSource as DependencyObject;

            // Traverse up the visual tree until we find a ComboBoxItem or reach the root
            while (current != null && current is not ComboBoxItem)
            {
                current = VisualTreeHelper.GetParent(current);
            }

            // If we found a ComboBoxItem and its data context is a PageMargins object with Key == "Custom"
            if (current is ComboBoxItem marginComboBoxItem &&
                marginComboBoxItem.DataContext is PageMargins { Key: "Custom" })
            {
                CustomMarginButton_Click(sender, null);
                e.Handled = true;
            }
            else if (current is ComboBoxItem sizeComboBoxItem &&
                sizeComboBoxItem.DataContext is PageSize { Key: "Custom" })
            {
                RibbonButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void pageMargins_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedKey = (pageMargins.SelectedItem as PageMargins)?.Key;

            // Skip if "Custom" was selected, already handled in mouse preview
            if (selectedKey == "Custom")
                return;

            if (!string.IsNullOrEmpty(selectedKey) &&
                pageMarginsCollection.TryGetValue(selectedKey, out var values))
            {
                // Clear any previously saved custom values
                customTopMargin = customBottomMargin = customLeftMargin = customRightMargin = customMarginUnit = string.Empty;

                // Reset the "Custom" label
                _customMarginsItem.top = LanguageToggleButton.IsChecked == true
                                            ? "Set custom margins"
                                            : "ಗ್ರಾಹಕೀಯ ಅಂಚುಗಳು";
                _customMarginsItem.bottom = _customMarginsItem.left = _customMarginsItem.right = "";

                CollectionViewSource.GetDefaultView(_marginItems).Refresh();

                // Apply selected margins to all sections with loading indicator
                try
                {
                    LoadingView.Show();
                    foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
                    {
                        section.SectionFormat.PageMargin = new Thickness(
                            values[2] * 96, // Left
                            values[0] * 96, // Top
                            values[3] * 96, // Right
                            values[1] * 96  // Bottom
                        );
                    }
                }
                finally
                {
                    LoadingView.Hide();
                }
            }
        }

        private static double UnitToDipFactor(string unit)
        {
            const double dpi = 96.0;            // 1 inch = 96 device‑independent pixels
            return unit switch
            {
                "cm" => dpi / 2.54,
                "mm" => dpi / 25.4,
                _ => dpi                   // inches
            };
        }


        private void CustomMarginButton_Click(object sender, RoutedEventArgs e)
        {
            const double dpi = 96.0;

            // Get the current page margins from the first section
            var firstSection = richTextBoxAdv.Document?.Sections?.FirstOrDefault() as SectionAdv;
            if (firstSection == null) return;

            Thickness current = firstSection.SectionFormat.PageMargin;

            double leftInches = current.Left / dpi;
            double topInches = current.Top / dpi;
            double rightInches = current.Right / dpi;
            double bottomInches = current.Bottom / dpi;

            // Determine the display unit
            string unit = string.IsNullOrWhiteSpace(customMarginUnit) ? "in" : customMarginUnit.ToLower();

            double toUnitFactor = unit switch
            {
                "cm" => 2.54,
                "mm" => 25.4,
                _ => 1.0
            };

            // Convert margins to display unit strings
            string leftStr = (leftInches * toUnitFactor).ToString("0.##");
            string topStr = (topInches * toUnitFactor).ToString("0.##");
            string rightStr = (rightInches * toUnitFactor).ToString("0.##");
            string bottomStr = (bottomInches * toUnitFactor).ToString("0.##");

            // Launch the custom margin dialog
            var dlg = new CustomMargin(topStr, bottomStr, leftStr, rightStr, unit);
            if (dlg.ShowDialog() != true) return;

            // Save the entered values
            customTopMargin = dlg.TopMarginTextBox.Text;
            customBottomMargin = dlg.BottomMarginTextBox.Text;
            customLeftMargin = dlg.LeftMarginTextBox.Text;
            customRightMargin = dlg.RightMarginTextBox.Text;
            customMarginUnit = dlg.Unit;

            // Update the ComboBox display text for the "Custom" option
            string unitLabel = dlg.Unit switch { "cm" => "cm", "mm" => "mm", _ => "in" };
            _customMarginsItem.top = $"Top: {customTopMargin} {unitLabel}";
            _customMarginsItem.bottom = $"Bottom: {customBottomMargin} {unitLabel}";
            _customMarginsItem.left = $"Left: {customLeftMargin} {unitLabel}";
            _customMarginsItem.right = $"Right: {customRightMargin} {unitLabel}";
            CollectionViewSource.GetDefaultView(_marginItems).Refresh();
            pageMargins.SelectedItem = _customMarginsItem;

            // Apply margins to all sections with loading indicator
            double dipFactor = UnitToDipFactor(dlg.Unit);
            try
            {
                LoadingView.Show();
                foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
                {
                    section.SectionFormat.PageMargin = new Thickness(
                        dlg.Left * dipFactor,
                        dlg.Top * dipFactor,
                        dlg.Right * dipFactor,
                        dlg.Bottom * dipFactor);
                }
            }
            finally
            {
                LoadingView.Hide();
            }
        }


        #endregion

        private void ribbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // If no changes, just close
            if (_isDocumentModified == false)
            {
                SimpleLogger.Log("Window closing: no unsaved changes.");
                return;
            }

            string message;
            string caption;

            if (LanguageToggleButton.IsChecked == true) // English
            {
                message = "Do you want to save changes to the document before exiting?";
                caption = "Save Document";
            }
            else // Kannada
            {
                message = "ನಿರ್ಗಮಿಸುವ ಮೊದಲು ಬದಲಾವಣೆಗಳನ್ನು ಉಳಿಸಲು ನೀವು ಬಯಸುವಿರಾ?";
                caption = "ಕಡತವನ್ನು ಉಳಿಸಿ";
            }

            // Show prompt to user
            MessageBoxResult result = MessageBox.Show(
                message,
                caption,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SimpleLogger.Log("User chose to save changes before closing.");
                    try
                    {
                        // Call the saving command
                        SfRichTextBoxAdv.SaveDocumentCommand.Execute(null, richTextBoxAdv);

                        // Reset the flag after save
                        _isDocumentModified = false;

                        SimpleLogger.Log("Document saved successfully before closing.");
                    }
                    catch (Exception ex)
                    {
                        SimpleLogger.LogException(ex, "Error saving document before closing");
                        MessageBox.Show($"Failed to save document:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        // Cancel closing if save failed
                        e.Cancel = true;
                    }
                    break;

                case MessageBoxResult.No:
                    SimpleLogger.Log("User chose not to save changes and closed the window.");
                    break;

                case MessageBoxResult.Cancel:
                    SimpleLogger.Log("User canceled the window close operation.");
                    e.Cancel = true;
                    break;
            }
        }






        #region PageSizes Implementation

        private (double widthIn, double heightIn) GetCurrentSizeInInches()
        {
            const double dpi = 96.0;
            var first = richTextBoxAdv.Document.Sections.OfType<SectionAdv>().FirstOrDefault();
            return first == null
                ? (8.3, 11.7)                                   // fallback = A4
                : (first.SectionFormat.PageSize.Width / dpi,
                   first.SectionFormat.PageSize.Height / dpi);
        }


        private void InitializePageSizes()
        {
            // 1️⃣  Build rows from helper
            var predefined = PageSizeHelper.All
                .Select(ps => new PageSize
                {
                    Key = ps.Key,
                    width = $"{ps.WidthInInches} in",
                    height = $"{ps.HeightInInches} in"
                })
                .ToList();

            // 2️⃣  Live custom row
            predefined.Add(new PageSize { Key = "Custom", width = "", height = "" });

            _customSizeItem = predefined.Last();
            _sizeItems = new ObservableCollection<PageSize>(predefined);
            pageSize.ItemsSource = _sizeItems;

            // 3️⃣  Quick‑lookup dictionary (inches)
            pageSizesCollection = PageSizeHelper.All
                .ToDictionary(p => p.Key,
                              p => new List<double> { p.WidthInInches, p.HeightInInches },
                              StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Called when the user selects a predefined size in the ComboBox.</summary>
        private void pageSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_ignorePageSizeChange) return;                       // programmatic change

            string key = (pageSize.SelectedItem as PageSize)?.Key;
            if (key == "Custom" || string.IsNullOrWhiteSpace(key)) return;

            if (pageSizesCollection.TryGetValue(key, out var v))
            {
                customPageWidth = customPageHeight = customSizeUnit = string.Empty;

                foreach (SectionAdv s in richTextBoxAdv.Document.Sections.OfType<SectionAdv>())
                    s.SectionFormat.PageSize = new Size(v[0] * 96, v[1] * 96);
            }
        }

        /// <summary>Applies defaults (A4 + normal margins) to a new/empty document.</summary>
        private void ApplyDefaultPageSettings()
        {
            const double dpi = 96.0;
            double a4WidthPx = 8.3 * dpi;           // 8.3 in → px
            double a4HeightPx = 11.7 * dpi;          // 11.7 in → px
            double marginPx = dpi;                 // 1 inch

            if (richTextBoxAdv.Document.Sections.Count == 0)
                richTextBoxAdv.Document.Sections.Add(new SectionAdv());

            foreach (SectionAdv s in richTextBoxAdv.Document.Sections.OfType<SectionAdv>())
            {
                s.SectionFormat.PageSize = new Size(a4WidthPx, a4HeightPx);
                s.SectionFormat.PageMargin = new Thickness(marginPx);
            }

            // sync UI without triggering SelectionChanged
            _ignorePageSizeChange = true;
            pageSize.SelectedItem = _sizeItems?.FirstOrDefault(p => p.Key == "A4");
            pageMargins.SelectedIndex = pageMargins.Items
                .Cast<PageMargins>()
                .ToList()
                .FindIndex(m => m.Key == "Normal");
            _ignorePageSizeChange = false;
        }

        private void RibbonButton_Click(object? sender, RoutedEventArgs? e)
        {
            const double dpi = 96.0;

            // Step 1: Start with current page size in inches
            var (wIn, hIn) = GetCurrentSizeInInches();

            // Step 2: If user had manually entered custom values earlier, override
            if (double.TryParse(customPageWidth, out double inputWidth) &&
                double.TryParse(customPageHeight, out double inputHeight) &&
                !string.IsNullOrWhiteSpace(customSizeUnit))
            {
                string unit = customSizeUnit.ToLower();
                double factor = unit switch
                {
                    "cm" => 1.0 / 2.54,
                    "mm" => 1.0 / 25.4,
                    _ => 1.0
                };

                wIn = inputWidth * factor;
                hIn = inputHeight * factor;
            }

            // Step 3: Show dialog in selected unit (default to inches)
            string dlgUnit = string.IsNullOrWhiteSpace(customSizeUnit) ? "in" : customSizeUnit.ToLower();
            double toUnitFactor = dlgUnit switch
            {
                "cm" => 2.54,
                "mm" => 25.4,
                _ => 1.0
            };

            string wStr = (wIn * toUnitFactor).ToString("0.###");
            string hStr = (hIn * toUnitFactor).ToString("0.###");

            var dlg = new CustomPageSize(wStr, hStr, dlgUnit) { Owner = this };
            if (dlg.ShowDialog() != true)
                return;

            // Step 4: Save user input
            customPageWidth = dlg.WidthBox.Text;
            customPageHeight = dlg.HeightBox.Text;
            customSizeUnit = dlg.Unit.ToLower(); // cm / mm / in

            // Step 5: Convert to DIP (pixels)
            double dipFactor = customSizeUnit switch
            {
                "cm" => dpi / 2.54,
                "mm" => dpi / 25.4,
                _ => dpi
            };

            double widthPx = dlg.PageWidth * dipFactor;
            double heightPx = dlg.PageHeight * dipFactor;

            foreach (SectionAdv s in richTextBoxAdv.Document.Sections.OfType<SectionAdv>())
            {
                s.SectionFormat.PageSize = new Size(widthPx, heightPx);
            }

            richTextBoxAdv.UpdateLayout();

            // Step 6: Update the "Custom" row in dropdown
            if (_customSizeItem != null)
            {
                _customSizeItem.width = $"{dlg.PageWidth:0.###} {dlg.Unit}";
                _customSizeItem.height = $"{dlg.PageHeight:0.###} {dlg.Unit}";
                CollectionViewSource.GetDefaultView(_sizeItems!).Refresh();
            }

            pageSize.SelectedItem = _customSizeItem;
        }



        #endregion




        public enum HeaderFooterType
        {
            Default,
            EvenPage,
            FirstPage
        }

        private async void EditHeader_Click(object sender, RoutedEventArgs e)
        {
            await OpenHeaderFooterEditorAsync(HeaderFooterType.Default);
        }

        private async void evenHeaderFooter_Click(object sender, RoutedEventArgs e)
        {
            await OpenHeaderFooterEditorAsync(HeaderFooterType.EvenPage);
        }

        private async void firstPageHeaderFooter_Click(object sender, RoutedEventArgs e)
        {
            await OpenHeaderFooterEditorAsync(HeaderFooterType.FirstPage);
        }

        /// <summary>
        /// Centralized method for opening and applying any header/footer editor.
        /// </summary>
        private async Task OpenHeaderFooterEditorAsync(HeaderFooterType type)
        {
            try
            {
                SimpleLogger.Log($"Opening {type} Header/Footer editor dialog.");

                string? currentHeader = GetCurrentHeaderFooterText(type, true);
                string? currentFooter = GetCurrentHeaderFooterText(type, false);
                SimpleLogger.Log($"Current {type} header length: {currentHeader?.Length ?? 0}, footer length: {currentFooter?.Length ?? 0}");

                var editor = new HeaderFooterEditor(currentHeader, currentFooter);

                bool? dialogResult = editor.ShowDialog();
                if (dialogResult != true)
                {
                    SimpleLogger.Log($"{type} Header/Footer editor dialog cancelled.");
                    return;
                }

                SimpleLogger.Log($"{type} Header/Footer editor dialog closed with Apply.");
                await Task.Delay(50);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadingView.Show();

                    var sections = richTextBoxAdv?.Document?.Sections;
                    if (sections == null || sections.Count == 0)
                    {
                        SimpleLogger.Log("No sections found – aborting header/footer apply.");
                        LoadingView.Hide();
                        return;
                    }

                    foreach (var section in sections.OfType<SectionAdv>())
                    {
                        if (section.HeaderFooters == null)
                            section.HeaderFooters = new HeaderFooters();

                        ApplyHeaderFooterBlocks(section, editor, type);
                    }

                    // Refresh layout
                    // Refresh visual + internal layout
                    richTextBoxAdv.InvalidateVisual();
                    richTextBoxAdv.UpdateLayout();

                    // Full logical refresh
                    ForceDocumentRefresh(richTextBoxAdv);
                    SimpleLogger.Log($"{type} Header/Footer applied and refreshed successfully.");
                    LoadingView.Hide();

                });
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error in {type} Header/Footer operation: {ex}");
                MessageBox.Show($"Error applying {type} Header/Footer:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadingView.Hide();
            }
        }

        /// <summary>
        /// Applies header and footer content blocks to the section based on the header/footer type.
        /// </summary>
        private void ApplyHeaderFooterBlocks(SectionAdv section, HeaderFooterEditor editor, HeaderFooterType type)
        {
            HeaderFooter? header = null;
            HeaderFooter? footer = null;

            switch (type)
            {
                case HeaderFooterType.Default:
                    header = section.HeaderFooters.Header ??= new HeaderFooter();
                    footer = section.HeaderFooters.Footer ??= new HeaderFooter();
                    section.SectionFormat.DifferentFirstPage = false;
                    section.SectionFormat.DifferentOddAndEvenPages = false;
                    break;

                case HeaderFooterType.EvenPage:
                    header = section.HeaderFooters.EvenHeader ??= new HeaderFooter();
                    footer = section.HeaderFooters.EvenFooter ??= new HeaderFooter();
                    section.SectionFormat.DifferentOddAndEvenPages = true;
                    section.SectionFormat.DifferentFirstPage = false;
                    break;

                case HeaderFooterType.FirstPage:
                    header = section.HeaderFooters.FirstPageHeader ??= new HeaderFooter();
                    footer = section.HeaderFooters.FirstPageFooter ??= new HeaderFooter();
                    section.SectionFormat.DifferentFirstPage = true;
                    section.SectionFormat.DifferentOddAndEvenPages = false;
                    break;
            }

            header.Blocks.Clear();
            footer.Blocks.Clear();

            if (!string.IsNullOrWhiteSpace(editor.HeaderText))
            {
                var headerBlocks = LoadBlocksFromRtf(editor.HeaderText);
                foreach (var block in headerBlocks)
                    header.Blocks.Add(block);
                SimpleLogger.Log($"Loaded {type} header with {header.Blocks.Count} blocks.");
            }

            if (!string.IsNullOrWhiteSpace(editor.FooterText))
            {
                var footerBlocks = LoadBlocksFromRtf(editor.FooterText);
                foreach (var block in footerBlocks)
                    footer.Blocks.Add(block);
                SimpleLogger.Log($"Loaded {type} footer with {footer.Blocks.Count} blocks.");
            }

            section.SectionFormat.HeaderDistance = 50;
            section.SectionFormat.FooterDistance = 50;
        }

        /// <summary>
        /// Extracts the current RTF content from the document’s header/footer by type.
        /// </summary>
        private string? GetCurrentHeaderFooterText(HeaderFooterType type, bool isHeader)
        {
            try
            {
                if (richTextBoxAdv?.Document?.Sections == null || richTextBoxAdv.Document.Sections.Count == 0)
                {
                    SimpleLogger.Log("Document or section is null - cannot get header/footer text.");
                    return null;
                }

                HeaderFooter? target = null;
                var hf = richTextBoxAdv.Document.Sections[0].HeaderFooters;
                if (hf == null)
                    return null;

                target = type switch
                {
                    HeaderFooterType.Default => isHeader ? hf.Header : hf.Footer,
                    HeaderFooterType.EvenPage => isHeader ? hf.EvenHeader : hf.EvenFooter,
                    HeaderFooterType.FirstPage => isHeader ? hf.FirstPageHeader : hf.FirstPageFooter,
                    _ => null
                };

                if (target == null || target.Blocks.Count == 0)
                {
                    SimpleLogger.Log($"GetCurrentHeaderFooterText found no {type} {(isHeader ? "header" : "footer")} blocks.");
                    return null;
                }

                var tempEditor = new SfRichTextBoxAdv();
                var tempSection = tempEditor.Document.Sections[0];

                // Clone blocks into temp editor
                foreach (var block in target.Blocks.ToList())
                    tempSection.Blocks.Add(block);

                using var ms = new MemoryStream();
                tempEditor.Save(ms, FormatType.Rtf);
                ms.Position = 0;

                using var reader = new StreamReader(ms, Encoding.UTF8);
                string result = reader.ReadToEnd();

                SimpleLogger.Log($"GetCurrentHeaderFooterText retrieved {result.Length} chars for {type} {(isHeader ? "header" : "footer")}.");
                return result;
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error in GetCurrentHeaderFooterText ({type}, {(isHeader ? "header" : "footer")}): {ex}");
                return null;
            }
        }

        /// <summary>
        /// Converts RTF text into a list of BlockAdv elements.
        /// </summary>
        private static BlockAdv[] LoadBlocksFromRtf(string rtf)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(rtf));
            var tempEditor = new SfRichTextBoxAdv();
            tempEditor.Load(ms, FormatType.Rtf);
            return tempEditor.Document.Sections[0].Blocks.OfType<BlockAdv>().ToArray();
        }



        private void RefreshAllPagesEditor()
        {
            try
            {
                if (richTextBoxAdv == null || richTextBoxAdv.Document == null)
                    return;

                using var ms = new MemoryStream();
                richTextBoxAdv.Save(ms, FormatType.Rtf);
                ms.Position = 0;
                richTextBoxAdv.Load(ms, FormatType.Rtf);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ForceDocumentRefresh failed: {ex}");
            }
        }




        private void removeHeaderFooter_Click(object sender, RoutedEventArgs e)
        {
            // Show dialog to let user select which headers/footers to clear
            var dialog = new ClearHeaderFooterDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() != true)
            {
                SimpleLogger.Log("User cancelled clearing headers/footers.");
                return;
            }

            SimpleLogger.Log($"User selected to clear: {dialog.SelectedType}");

            try
            {
                LoadingView.Show(this);
                SimpleLogger.Log("LoadingView shown. Clearing headers/footers...");

                // Clear headers/footers on the UI thread
                ClearHeaderFooter(richTextBoxAdv, dialog.SelectedType);

                SimpleLogger.Log("Header/footer clearing completed.");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"Error while clearing headers/footers: {ex}");
                MessageBox.Show($"Error clearing headers/footers:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingView.Hide();
                SimpleLogger.Log("LoadingView hidden.");
            }
        }

        /// <summary>
        /// Clears headers/footers on the provided editor based on the dialog selection.
        /// </summary>
        private void ClearHeaderFooter(SfRichTextBoxAdv editor, ClearHeaderFooterDialog.DialogHeaderFooterType type)
        {
            if (editor?.Document?.Sections == null || editor.Document.Sections.Count == 0)
            {
                SimpleLogger.Log("No document or sections found to clear headers/footers.");
                return;
            }

            int sectionIndex = 1;
            foreach (var section in editor.Document.Sections.OfType<SectionAdv>())
            {
                SimpleLogger.Log($"Processing Section #{sectionIndex}");

                if (section.HeaderFooters == null)
                {
                    SimpleLogger.Log("Section has no headers/footers. Skipping.");
                    sectionIndex++;
                    continue;
                }

                switch (type)
                {
                    case ClearHeaderFooterDialog.DialogHeaderFooterType.AllPages:
                        section.HeaderFooters.Header?.Blocks.Clear();
                        section.HeaderFooters.Footer?.Blocks.Clear();
                        section.HeaderFooters.EvenHeader?.Blocks.Clear();
                        section.HeaderFooters.EvenFooter?.Blocks.Clear();
                        section.HeaderFooters.FirstPageHeader?.Blocks.Clear();
                        section.HeaderFooters.FirstPageFooter?.Blocks.Clear();
                        section.SectionFormat.DifferentFirstPage = false;
                        section.SectionFormat.DifferentOddAndEvenPages = false;
                        SimpleLogger.Log("Cleared headers/footers on all pages.");
                        break;

                    case ClearHeaderFooterDialog.DialogHeaderFooterType.EvenPages:
                        section.HeaderFooters.EvenHeader?.Blocks.Clear();
                        section.HeaderFooters.EvenFooter?.Blocks.Clear();
                        section.SectionFormat.DifferentOddAndEvenPages = true;
                        SimpleLogger.Log("Cleared headers/footers on even pages.");
                        break;

                    case ClearHeaderFooterDialog.DialogHeaderFooterType.FirstPage:
                        section.HeaderFooters.FirstPageHeader?.Blocks.Clear();
                        section.HeaderFooters.FirstPageFooter?.Blocks.Clear();
                        section.SectionFormat.DifferentFirstPage = true;
                        SimpleLogger.Log("Cleared headers/footers on first page.");
                        break;
                }

                // Remove HeaderFooters object if all blocks are empty
                if (IsHeaderFooterEmpty(section.HeaderFooters))
                {
                    section.HeaderFooters = null;
                    SimpleLogger.Log("HeaderFooters removed from section as all blocks are empty.");
                }

                sectionIndex++;
            }

            // Refresh the editor to apply changes
            Application.Current.Dispatcher.Invoke(() => ForceDocumentRefresh(editor));
        }

        /// <summary>
        /// Checks if a HeaderFooters object has any blocks.
        /// </summary>
        private bool IsHeaderFooterEmpty(Syncfusion.Windows.Controls.RichTextBoxAdv.HeaderFooters hf)
        {
            return (hf.Header?.Blocks.Count ?? 0) == 0 &&
                   (hf.Footer?.Blocks.Count ?? 0) == 0 &&
                   (hf.EvenHeader?.Blocks.Count ?? 0) == 0 &&
                   (hf.EvenFooter?.Blocks.Count ?? 0) == 0 &&
                   (hf.FirstPageHeader?.Blocks.Count ?? 0) == 0 &&
                   (hf.FirstPageFooter?.Blocks.Count ?? 0) == 0;
        }

        /// <summary>
        /// Forces the RichTextBoxAdv to refresh the document after clearing headers/footers.
        /// </summary>
        private void ForceDocumentRefresh(SfRichTextBoxAdv editor)
        {
            try
            {
                if (editor?.Document == null)
                {
                    SimpleLogger.Log("Cannot refresh document: RichTextBoxAdv or Document is null.");
                    return;
                }

                using var ms = new MemoryStream();
                editor.Save(ms, FormatType.Rtf);
                ms.Position = 0;
                editor.Load(ms, FormatType.Rtf);
                SimpleLogger.Log("Document refresh completed successfully.");
            }
            catch (Exception ex)
            {
                SimpleLogger.Log($"ForceDocumentRefresh failed: {ex}");
            }
        }




        private void InsertPageNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PageNumberDialog dlg = new();
                if (dlg.ShowDialog() == false || string.IsNullOrWhiteSpace(dlg.PageNumberPlaceholder))
                    return;

                SimpleLogger.Log($"User selected placeholder: {dlg.PageNumberPlaceholder}");

                // Read actual page count from RichTextBoxAdv (REAL pages)
                int totalPages = richTextBoxAdv.PageCount;

                SimpleLogger.Log($"Document has {totalPages} real pages (from RichTextBoxAdv).");

                bool insertAtTop = dlg.rbTop.IsChecked == true;
                string target = insertAtTop ? "HEADER" : "FOOTER";

                // Log one line per REAL PAGE
                for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
                {
                    SimpleLogger.Log(
                        $"Simulated: Insert page number '{dlg.PageNumberPlaceholder}' " +
                        $"into {target} for Page {pageNumber} of {totalPages}"
                    );
                }

                MessageBox.Show(
                    "Note: Page Number Insertion is not yet fully implemented.\nIt will be available very soon.",
                    "Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
                SimpleLogger.Log("Simulation complete. No document update done.");
            }
            catch (Exception ex)
            {
                SimpleLogger.LogException(ex, "InsertPageNumber_Click error");
                MessageBox.Show("Failed to insert page number.", "Error");
            }
        }


        private void StartNudiEngine_Click(object sender, RoutedEventArgs e)
        {
            string exeFileName = "kannadaKeyboard.exe";
            string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", exeFileName);

            // Check if the process is already running
            if (IsProcessRunning(Path.GetFileNameWithoutExtension(exeFileName)))
            {
                MessageBox.Show("The Nudi Engine is already running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Check if the executable exists at the given path
                if (File.Exists(exePath))
                {
                    try
                    {
                        // Start the process
                        Process.Start(exePath);
                        MessageBox.Show("The Nudi Engine has started.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error starting Nudi Engine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Executable not found at path: {exePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopNudiEngine_Click(object sender, RoutedEventArgs e)
        {
            // Check if the "kannadaKeyboard.exe" process is running
            if (IsProcessRunning("kannadaKeyboard"))
            {
                // Stop the process (assuming the name of the process is "kannadaKeyboard")
                try
                {
                    foreach (var process in Process.GetProcessesByName("kannadaKeyboard"))
                    {
                        process.Kill();
                    }
                    MessageBox.Show("The Nudi Engine has stopped.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error stopping Nudi Engine: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("The Nudi Engine is not running.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }


        private void AboutNudi_Click(object sender, RoutedEventArgs e)
        {
            var window = new Views.HelpTab.AboutNudiWindow();
            window.ShowDialog();
        }


        private void ProvideFeedback_Click(object sender, RoutedEventArgs e)
        {
            // Create and show the HelpFeedbackWindow when the "Feedback" button is clicked
            var feedbackWindow = new Views.HelpTab.HelpFeedbackWindow();
            feedbackWindow.ShowDialog(); // Show the feedback form as a modal window
        }


        private void ShowDocumentation_Click(object sender, RoutedEventArgs e)
        {
            // Open the documentation window or link
            MessageBox.Show("Documentation link or window here.");
        }







        // Add these class-level fields in MainWindow.xaml.cs

        private string? _activeLanguage = null;



        private void StartPythonProcess(string languageCode)
        {
            if (_speechProcess != null && !_speechProcess.HasExited && _activeLanguage == languageCode)
                return; // Already running with the same language

            StopPythonProcess(); // Stop if another language is running

            var exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "recognize_mic.exe");

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = languageCode,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _speechProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
            _speechProcess.Start();
            _pythonInput = _speechProcess.StandardInput;
            _activeLanguage = languageCode;

            _outputReaderTask = Task.Run(async () =>
            {
                while (!_speechProcess.HasExited && !_speechProcess.StandardOutput.EndOfStream)
                {
                    var line = await _speechProcess.StandardOutput.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            richTextBoxAdv.Selection.InsertText(line + " ");
                        });
                    }
                }
            });

            Task.Run(async () =>
            {
                var errorBuilder = new StringBuilder();

                while (!_speechProcess.StandardError.EndOfStream)
                {
                    var err = await _speechProcess.StandardError.ReadLineAsync();
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        errorBuilder.AppendLine(err);
                    }
                }

                string errorMessage = errorBuilder.ToString().Trim();
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(errorMessage, "Speech Recognition Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });
        }

        private void ToggleKannadaSpeech_Click(object sender, RoutedEventArgs e)
        {
            if (!_isListeningKannada)
            {
                StartPythonProcess("kn-IN");
                _pythonInput?.WriteLine("start");
                KannadaSpeechButton.Content = "Stop Kannada";
            }
            else
            {
                _pythonInput?.WriteLine("stop");
                KannadaSpeechButton.Content = "Start Kannada";
            }
            _isListeningKannada = !_isListeningKannada;
            _isListeningEnglish = false;
        }

        private void ToggleEnglishSpeech_Click(object sender, RoutedEventArgs e)
        {
            if (!_isListeningEnglish)
            {
                StartPythonProcess("en-IN");
                _pythonInput?.WriteLine("start");
                EnglishSpeechButton.Content = "Stop English";
            }
            else
            {
                _pythonInput?.WriteLine("stop");
                EnglishSpeechButton.Content = "Start English";
            }
            _isListeningEnglish = !_isListeningEnglish;
            _isListeningKannada = false;
        }

        private void StopPythonProcess()
        {
            try
            {
                if (_speechProcess != null && !_speechProcess.HasExited)
                {
                    _pythonInput?.WriteLine("exit");
                    _speechProcess.WaitForExit(2000);
                    _speechProcess.Kill(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping Python process: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _speechProcess?.Dispose();
                _speechProcess = null;
                _pythonInput = null;
                _outputReaderTask = null;
                _activeLanguage = null;
                _isListeningKannada = false;
                _isListeningEnglish = false;
            }
        }












        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            bool isEnglish = LanguageToggleButton.IsChecked == true;

            var sortWindow = new SortWindow(isEnglish, richTextBoxAdv)
            {
                Owner = this
            };
            sortWindow.ShowDialog(); // Modal — blocks until SortWindow closes
        }

        private void EnableSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            if (spellChecker == null)
            {
                SimpleLogger.Log("Spell checker not initialized.");
                NudiInfoDialog.Show("Spell checker not initialized.");
                return;
            }

            if (spellChecker.IsEnabled)
            {
                SimpleLogger.Log("Spell check is already enabled.");
                NudiInfoDialog.Show("Spell check is already enabled.");
                return;
            }

            spellChecker.IsEnabled = true;
            SimpleLogger.Log("Spell check enabled.");
            NudiInfoDialog.Show("Spell check enabled.");
        }

        private void DisableSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            if (spellChecker == null)
            {
                SimpleLogger.Log("Spell checker not initialized.");
                NudiInfoDialog.Show("Spell checker not initialized.");
                return;
            }

            if (!spellChecker.IsEnabled)
            {
                SimpleLogger.Log("Spell check is already disabled.");
                NudiInfoDialog.Show("Spell check is already disabled.");
                return;
            }

            spellChecker.IsEnabled = false;
            SimpleLogger.Log("Spell check disabled.");
            NudiInfoDialog.Show("Spell check disabled.");
        }







    }


}