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
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Linq;

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
        private RibbonGallery? ribbonGallery = null;
        private RibbonButton? RibbonButton = null;
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
        private string customSizeUnit;
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
        }
        #endregion






        private void ConfigureSpellChecker()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string dictionaryPath = Path.Combine(basePath, "Assets", "kn_IN.dic");
            string customDictionaryPath1 = Path.Combine(basePath, "Assets", "Custom_MyDictionary_kn_IN.dic");
            string customDictionaryPath2 = Path.Combine(basePath, "Assets", "default.dic");

            // Optional: Debug missing files
            if (!File.Exists(dictionaryPath)) MessageBox.Show("Missing: " + dictionaryPath);
            if (!File.Exists(customDictionaryPath1)) MessageBox.Show("Missing: " + customDictionaryPath1);
            if (!File.Exists(customDictionaryPath2)) MessageBox.Show("Missing: " + customDictionaryPath2);

            // Create spell checker instance (but don't enable it)
            spellChecker = new SpellChecker
            {
                IsEnabled = false, // Initially off
                IgnoreUppercaseWords = false,
                IgnoreAlphaNumericWords = true,
                UseFrameworkSpellCheck = false,
            };

            spellChecker.Dictionaries.Add(dictionaryPath);
            spellChecker.CustomDictionaries.Add(customDictionaryPath1);
            spellChecker.CustomDictionaries.Add(customDictionaryPath2);

            // Assign to RichTextBoxAdv
            richTextBoxAdv.SpellChecker = spellChecker;
        }







        #region Events
        private void RichTextBoxAdv_DocumentChanged(object obj, DocumentChangedEventArgs args)
        {
            if (ribbonGallery != null && ribbonGallery.Items != null)
            {
                ribbonGallery.Items.Clear();
                AddRibbonGalleryItems();
            }
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
        /// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ribbon != null)
                ribbon.Loaded += Ribbon_Loaded;
        }
        /// <summary>
        /// Handles the RequestNavigate event of the richTextBoxAdv control.
        /// </summary>
        /// <param name="obj">The source of the event.</param>
        /// <param name="args">The <see cref="Syncfusion.Windows.Controls.RichTextBoxAdv.RequestNavigateEventArgs"/> instance containing the event data.</param>
        void RichTextBoxAdv_RequestNavigate(object obj, Syncfusion.Windows.Controls.RichTextBoxAdv.RequestNavigateEventArgs args)
        {
            if (args.Hyperlink.LinkType == Syncfusion.Windows.Controls.RichTextBoxAdv.HyperlinkType.Webpage || args.Hyperlink.LinkType == Syncfusion.Windows.Controls.RichTextBoxAdv.HyperlinkType.Email)
                LaunchUri(new Uri(args.Hyperlink.NavigationLink).AbsoluteUri);
            else if (args.Hyperlink.LinkType == HyperlinkType.File && File.Exists(args.Hyperlink.NavigationLink))
                LaunchUri(args.Hyperlink.NavigationLink);
        }

        /// <summary>
        /// Launches the URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
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
            fontFamilyComboBox.SelectedValue = "NudiParijataha"; // set default selected value

            if (fontSizeComboBox != null)
                fontSizeComboBox.ItemsSource = new double[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 26, 28, 36, 48, 72, 96 };

            if (richTextBoxAdv != null)
            {
                // Set default font at the document level
                richTextBoxAdv.Document.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("NudiParijataha");

                // Optionally, apply it to current selection too
                richTextBoxAdv.Selection.CharacterFormat.FontFamily = new System.Windows.Media.FontFamily("NudiParijataha");

                richTextBoxAdv.Focus();
            }

            if (ribbonGallery != null && ribbonGallery.Items != null)
            {
                ribbonGallery.Items.Clear();
                AddRibbonGalleryItems();
            }
        }




        /// <summary>
        /// On save executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private async void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFilePath) && File.Exists(currentFilePath))
            {
                using (Stream stream = File.Open(currentFilePath, FileMode.Create))
                {
                    FormatType formatType = GetFormatType(Path.GetExtension(currentFilePath));
                    await richTextBoxAdv.SaveAsync(stream, formatType);
                }
            }
            richTextBoxAdv.Focus();
            ribbon.IsBackStageVisible = false;
        }
        /// <summary>
        /// On save as executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>

        private void OnSaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string extension = string.Empty;
            if (e.Parameter == null)
                extension = ".docx";
            else
                extension = e.Parameter.ToString();
            WordExport(extension);
            ribbon.IsBackStageVisible = false;
        }

        /// <summary>
        /// On print executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void OnPrintExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            richTextBoxAdv.PrintDocument();
            richTextBoxAdv.Focus();
            ribbon.IsBackStageVisible = false;
        }
        /// <summary>
        /// On new executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void OnNewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            richTextBoxAdv.Focus();
            ribbon.IsBackStageVisible = false;
        }
        /// <summary>
        /// On open executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void OnOpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            WordImport();
            richTextBoxAdv.Focus();
            ribbon.IsBackStageVisible = false;
        }
        /// <summary>
        /// On show encrypt document executed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.Input.ExecutedRoutedEventArgs">ExecutedRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnShowEncryptDocumentExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CloseBackstage();
            SfRichTextBoxAdv.ShowEncryptDocumentDialogCommand.Execute(null, richTextBoxAdv);
        }




        void OnlineHelpButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(new Uri("https://help.syncfusion.com/wpf").AbsoluteUri);
            CloseBackstage();
        }
        /// <summary>
        /// On getting started button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void GettingStartedButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchUri(new Uri("https://help.syncfusion.com/wpf/sfrichtextboxadv/getting-started").AbsoluteUri);
            CloseBackstage();
        }


        /// <summary>
        /// Wires up the events.
        /// </summary>
        /// <remarks></remarks>
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
        /// <summary>
        /// Called on increase font size button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
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
        /// <summary>
        /// Called on decrease font size button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
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
        /// <summary>
        /// Called on font color picker color changed.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:System.Windows.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void FontColorPicker_ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SplitButton splitbutton = (SplitButton)fontColorPicker.Parent;
            if (splitbutton != null)
                splitbutton.IsDropDownOpen = false;
        }
        /// <summary>
        /// Called on font color split button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
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
        /// <summary>
        /// Called on highlight color split button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void HighlightColorSplitButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBoxAdv != null && richTextBoxAdv.Selection.CharacterFormat.HighlightColor != HighlightColor.NoColor)
                richTextBoxAdv.Selection.CharacterFormat.HighlightColor = HighlightColor.NoColor;
            else
                richTextBoxAdv.Selection.CharacterFormat.HighlightColor = HighlightColor.Yellow;
        }
        /// <summary>
        /// Called on backstage button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void BackstageButton_Click(object sender, RoutedEventArgs e)
        {
            CloseBackstage();
        }
        /// <summary>
        /// Called on new document button clicked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
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
                TablePickerUI tablePicker = sender as TablePickerUI;
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
            styleFormat.Text = "AaBbCcDdEe";

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
        /// <summary>
        /// Initializes the page margins.
        /// </summary>

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

            //  DO NOT open dialog here — it’s already handled in PreviewMouseDown
            if (selectedKey == "Custom")
                return;

            if (!string.IsNullOrEmpty(selectedKey) &&
                pageMarginsCollection.TryGetValue(selectedKey, out var values))
            {
                customTopMargin = customBottomMargin = customLeftMargin = customRightMargin = customMarginUnit = string.Empty;

                _customMarginsItem.top = LanguageToggleButton.IsChecked == true
                                            ? "Set custom margins"
                                            : "ಗ್ರಾಹಕೀಯ ಅಂಚುಗಳು";
                _customMarginsItem.bottom = _customMarginsItem.left = _customMarginsItem.right = "";

                CollectionViewSource.GetDefaultView(_marginItems).Refresh();

                foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
                {
                    section.SectionFormat.PageMargin = new Thickness(
                        values[2] * 96,
                        values[0] * 96,
                        values[3] * 96,
                        values[1] * 96);
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
            const double dpi = 96.0;   // 1 inch = 96 device‑independent pixels

            // 1️⃣ —— Get current page margins (first section) ————————————————
            var firstSection = richTextBoxAdv.Document?.Sections?.FirstOrDefault() as SectionAdv;

            if (firstSection == null) return;

            Thickness current = firstSection.SectionFormat.PageMargin;

            double leftInches = current.Left / dpi;
            double topInches = current.Top / dpi;
            double rightInches = current.Right / dpi;
            double bottomInches = current.Bottom / dpi;

            // 2️⃣ —— Determine which unit we want to display in the dialog ——————
            string unit = string.IsNullOrWhiteSpace(customMarginUnit) ? "in" : customMarginUnit.ToLower();

            double toUnitFactor = unit switch
            {
                "cm" => 2.54,
                "mm" => 25.4,
                _ => 1.0            // inches
            };

            // 3️⃣ —— Convert current margins into that unit ————————————————
            string leftStr = (leftInches * toUnitFactor).ToString("0.##");
            string topStr = (topInches * toUnitFactor).ToString("0.##");
            string rightStr = (rightInches * toUnitFactor).ToString("0.##");
            string bottomStr = (bottomInches * toUnitFactor).ToString("0.##");

            // 4️⃣ —— Launch dialog pre‑filled with detected values ————————————
            var dlg = new CustomMargin(topStr, bottomStr, leftStr, rightStr, unit);

            if (dlg.ShowDialog() != true) return;   // user hit Cancel

            // 5️⃣ —— Remember what the user typed (for next launch & ribbon row) —
            customTopMargin = dlg.TopMarginTextBox.Text;
            customBottomMargin = dlg.BottomMarginTextBox.Text;
            customLeftMargin = dlg.LeftMarginTextBox.Text;
            customRightMargin = dlg.RightMarginTextBox.Text;
            customMarginUnit = dlg.Unit;          // "in" | "cm" | "mm"

            // 6️⃣ —— Update the “Custom” row text in the ComboBox ————————————
            string unitLabel = dlg.Unit switch { "cm" => "cm", "mm" => "mm", _ => "in" };
            _customMarginsItem.top = $"Top: {customTopMargin} {unitLabel}";
            _customMarginsItem.bottom = $"Bottom: {customBottomMargin} {unitLabel}";
            _customMarginsItem.left = $"Left: {customLeftMargin} {unitLabel}";
            _customMarginsItem.right = $"Right: {customRightMargin} {unitLabel}";
            CollectionViewSource.GetDefaultView(_marginItems).Refresh();
            pageMargins.SelectedItem = _customMarginsItem;

            // 7️⃣ —— Apply new margins to every section ————————————————
            double dipFactor = UnitToDipFactor(dlg.Unit);      // helper from earlier

            foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
            {
                section.SectionFormat.PageMargin = new Thickness(
                    dlg.Left * dipFactor,
                    dlg.Top * dipFactor,
                    dlg.Right * dipFactor,
                    dlg.Bottom * dipFactor);
            }
        }


        #endregion

        private void ribbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

            MessageBoxResult result = MessageBox.Show(
                message,
                caption,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);


            if (result == MessageBoxResult.Yes)
            {
                // Call the saving dialog of SfRichTextBoxAdv.
                SfRichTextBoxAdv.SaveDocumentCommand.Execute(null, richTextBoxAdv);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true; // Cancel closing
            }
        }








        #region PageSizes Implementation

        private (double widthIn, double heightIn) GetCurrentSizeInInches()
        {
            const double dpi = 96.0;
            var first = richTextBoxAdv.Document.Sections.OfType<SectionAdv>().FirstOrDefault();
            return first == null
                ? (8.3, 11.7)                                   // fallback = A4
                : (first.SectionFormat.PageSize.Width / dpi,
                   first.SectionFormat.PageSize.Height / dpi);
        }

        /// <summary>Populate the ComboBox from <see cref="PageSizeHelper"/> and set up lookups.</summary>
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

            double wIn, hIn;
            string dlgUnit = string.IsNullOrWhiteSpace(customSizeUnit) ? "in" : customSizeUnit.ToLower();

            if (!double.TryParse(customPageWidth, out wIn) ||
                !double.TryParse(customPageHeight, out hIn))
            {
                (wIn, hIn) = GetCurrentSizeInInches();
                dlgUnit = "in";
            }

            double toUnitFactor = dlgUnit switch
            {
                "cm" => 2.54,
                "mm" => 25.4,
                _ => 1.0
            };

            string wStr = (wIn * toUnitFactor).ToString("0.###");
            string hStr = (hIn * toUnitFactor).ToString("0.###");

            var dlg = new CustomPageSize(wStr, hStr, dlgUnit) { Owner = this };
            if (dlg.ShowDialog() != true) return;

            customPageWidth = dlg.WidthBox.Text;
            customPageHeight = dlg.HeightBox.Text;
            customSizeUnit = dlg.Unit.ToLower();  // "cm", "mm", or "in"

            double dipFactor = dlg.Unit switch
            {
                "cm" => dpi / 2.54,
                "mm" => dpi / 25.4,
                _ => dpi
            };

            double widthPx = dlg.PageWidth * dipFactor;
            double heightPx = dlg.PageHeight * dipFactor;

            foreach (SectionAdv s in richTextBoxAdv.Document.Sections.OfType<SectionAdv>())
                s.SectionFormat.PageSize = new Size(widthPx, heightPx);

            if (_customSizeItem != null)
            {
                _customSizeItem.width = $"{dlg.PageWidth:0.###} {dlg.Unit}";
                _customSizeItem.height = $"{dlg.PageHeight:0.###} {dlg.Unit}";
                CollectionViewSource.GetDefaultView(_sizeItems!).Refresh();
            }

            pageSize.SelectedItem = _customSizeItem;
        }











        #endregion

        // Handle Edit Header button click to show the Header/Footer editor
        private void EditHeader_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve the current header and footer text
            string? existingHeader = GetCurrentHeaderText();
            string? existingFooter = GetCurrentFooterText();

            // Open the editor window
            HeaderFooterEditor headerFooterEditor = new HeaderFooterEditor(existingHeader, existingFooter);
            bool? result = headerFooterEditor.ShowDialog();

            if (result == true)
            {
                string headerText = headerFooterEditor.HeaderText ?? string.Empty;
                string footerText = headerFooterEditor.FooterText ?? string.Empty;

                // Create new HeaderFooters
                HeaderFooters headerFooters = new HeaderFooters();

                // Apply header if provided
                if (!string.IsNullOrWhiteSpace(headerText))
                {
                    ParagraphAdv headerParagraph = new ParagraphAdv();
                    SpanAdv headerSpan = new SpanAdv { Text = headerText };
                    headerParagraph.Inlines.Add(headerSpan);
                    headerFooters.Header.Blocks.Add(headerParagraph);
                }

                // Apply footer if provided
                if (!string.IsNullOrWhiteSpace(footerText))
                {
                    ParagraphAdv footerParagraph = new ParagraphAdv();
                    SpanAdv footerSpan = new SpanAdv { Text = footerText };
                    footerParagraph.Inlines.Add(footerSpan);
                    headerFooters.Footer.Blocks.Add(footerParagraph);
                }

                // Apply to the document only if header or footer is present
                if (headerFooters.Header.Blocks.Count > 0 || headerFooters.Footer.Blocks.Count > 0)
                {
                    SectionAdv sectionAdv = richTextBoxAdv.Document.Sections[0];
                    sectionAdv.HeaderFooters = headerFooters;
                    sectionAdv.SectionFormat.HeaderDistance = 50;
                    sectionAdv.SectionFormat.FooterDistance = 50;
                }
                else
                {
                    MessageBox.Show("No header or footer text entered.");
                }
            }
        }


        // Get the current header text dynamically from the document
        private string? GetCurrentHeaderText()
        {
            // Retrieve the first block of the header (if any)
            var header = richTextBoxAdv.Document.Sections[0].HeaderFooters.Header;
            if (header.Blocks.Count > 0)
            {
                var firstBlock = header.Blocks[0] as ParagraphAdv;
                if (firstBlock != null && firstBlock.Inlines.Count > 0)
                {
                    var span = firstBlock.Inlines[0] as SpanAdv;
                    return span?.Text;  // Return the current header text
                }
            }
            return null;  // Return null if no header text exists
        }

        // Get the current footer text dynamically from the document
        private string? GetCurrentFooterText()
        {
            // Retrieve the first block of the footer (if any)
            var footer = richTextBoxAdv.Document.Sections[0].HeaderFooters.Footer;
            if (footer.Blocks.Count > 0)
            {
                var firstBlock = footer.Blocks[0] as ParagraphAdv;
                if (firstBlock != null && firstBlock.Inlines.Count > 0)
                {
                    var span = firstBlock.Inlines[0] as SpanAdv;
                    return span?.Text;  // Return the current footer text
                }
            }
            return null;  // Return null if no footer text exists
        }


        private void evenHeaderFooter_Click(object sender, RoutedEventArgs e)
        {
            // Defines the header and footer. 
            HeaderFooters headerFooters = new HeaderFooters();

            //// Defines the Even header. 
            ParagraphAdv headerParagraph = new ParagraphAdv();
            SpanAdv headerSpan = new SpanAdv();
            headerSpan.Text = "Even Page Header";
            headerParagraph.Inlines.Add(headerSpan);
            headerFooters.EvenHeader.Blocks.Add(headerParagraph);

            // Defines the Even footer. 
            ParagraphAdv footerParagraph = new ParagraphAdv();
            SpanAdv footerSpan = new SpanAdv();
            footerSpan.Text = "Even Page Footer";
            footerParagraph.Inlines.Add(footerSpan);
            headerFooters.EvenFooter.Blocks.Add(footerParagraph);

            SectionAdv sectionAdv = richTextBoxAdv.Document.Sections[0];
            sectionAdv.HeaderFooters = headerFooters;
            // sets this bool value for preservation of Even Header and Footer.
            sectionAdv.SectionFormat.DifferentOddAndEvenPages = true;
            sectionAdv.SectionFormat.HeaderDistance = 50;
            sectionAdv.SectionFormat.FooterDistance = 50;
        }
        /// <summary>
        ///  Adds First Page Header and Footer into the SfRichTextBoxAdv.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void firstPageHeaderFooter_Click(object sender, RoutedEventArgs e)
        {
            // Defines the header and footer. 
            HeaderFooters headerFooters = new HeaderFooters();

            //// Defines the First page header. 
            ParagraphAdv headerParagraph = new ParagraphAdv();
            SpanAdv headerSpan = new SpanAdv();
            headerSpan.Text = "First Page Header";
            headerParagraph.Inlines.Add(headerSpan);
            headerFooters.FirstPageHeader.Blocks.Add(headerParagraph);

            // Defines the First page footer. 
            ParagraphAdv footerParagraph = new ParagraphAdv();
            SpanAdv footerSpan = new SpanAdv();
            footerSpan.Text = "First Page Footer";
            footerParagraph.Inlines.Add(footerSpan);
            headerFooters.FirstPageFooter.Blocks.Add(footerParagraph);

            SectionAdv sectionAdv = richTextBoxAdv.Document.Sections[0];
            sectionAdv.HeaderFooters = headerFooters;
            // sets this bool value for preservation of First page Header and Footer.
            sectionAdv.SectionFormat.DifferentFirstPage = true;
            sectionAdv.SectionFormat.HeaderDistance = 50;
            sectionAdv.SectionFormat.FooterDistance = 50;
        }






        private void DifferentFirstPage_Checked(object sender, RoutedEventArgs e)
        {
            // Handle checked logic for "Different First Page"
        }

        private void DifferentFirstPage_Unchecked(object sender, RoutedEventArgs e)
        {
            // Handle unchecked logic for "Different First Page"
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

        private void ShowSupport_Click(object sender, RoutedEventArgs e)
        {
            // Open support window or link to support page
            MessageBox.Show("Support link or window here.");
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

























        private void removeHeaderFooter_Click(object sender, RoutedEventArgs e)
        {
            foreach (SectionAdv sectionAdv in richTextBoxAdv.Document.Sections)
            {
                HeaderFooters headerFooters = sectionAdv.HeaderFooters;

                ClearBlocks(
                    headerFooters.Header,
                    headerFooters.Footer,
                    headerFooters.EvenHeader,
                    headerFooters.EvenFooter,
                    headerFooters.FirstPageHeader,
                    headerFooters.FirstPageFooter
                );
            }
        }

        /// <summary>
        /// Clears the blocks of Header or Footer.
        /// </summary>
        /// <param name="headerFooters"></param>
        void ClearBlocks(params HeaderFooter[] headerFooters)
        {
            foreach (var headerFooter in headerFooters)
            {
                if (headerFooter == null)
                    continue;

                for (int i = headerFooter.Blocks.Count - 1; i >= 0; i--)
                {
                    headerFooter.Blocks.RemoveAt(i);
                }
            }
        }





        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            bool isEnglish = LanguageToggleButton.IsChecked == true;
            var sortWindow = new SortWindow(isEnglish, richTextBoxAdv);
            sortWindow.Owner = this;
            sortWindow.ShowDialog();

        }







        private void EnableSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            if (spellChecker != null)
            {
                spellChecker.IsEnabled = true;
                MessageBox.Show("Spell check enabled");
            }
        }

        private void DisableSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            if (spellChecker != null)
            {
                spellChecker.IsEnabled = false;
                MessageBox.Show("Spell check disabled");
            }
        }









    }





}