using Microsoft.Win32;
using Syncfusion.Windows.Controls.RichTextBoxAdv;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using KannadaNudiEditor.Helpers;
namespace KannadaNudiEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        #region Feilds
#if !Framework3_5
        Task<bool> loadAsync = null;
        CancellationTokenSource cancellationTokenSource = null;
        private RibbonGallery ribbonGallery = null;
        private RibbonButton RibbonButton = null;
        Dictionary<string, List<double>> pageMarginsCollection = null;
        Dictionary<string, List<double>> pageSizesCollection = null;

#endif
        #endregion



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
        }
        #endregion



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
        private void OnSaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            WordExport(string.Empty);
            richTextBoxAdv.Focus();
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
        }
        /// <summary>
        /// On new executed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ExecutedRoutedEventArgs"/> instance containing the event data.</param>
        private void OnNewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            richTextBoxAdv.Document = new DocumentAdv();
            richTextBoxAdv.Focus();
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
        }
        /// <summary>
        /// On show encrypt document executed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Windows.Input.ExecutedRoutedEventArgs">ExecutedRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnShowEncryptDocumentExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SfRichTextBoxAdv.ShowEncryptDocumentDialogCommand.Execute(null, richTextBoxAdv);
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
                                if (ribbonBar.Header == "ClipBoard")
                                {
                                    foreach (var item in ribbonBar.Items)
                                    {
                                        RibbonButton button = item as RibbonButton;
                                        if (button != null && button.Label != "Copy")
                                        {
                                            if (richTextBoxAdv.IsReadOnly)
                                                button.IsEnabled = false;
                                            else
                                                button.IsEnabled = true;
                                        }
                                    }
                                }
                                else if (ribbonBar.Header == "Editing")
                                {
                                    foreach (var item in ribbonBar.Items)
                                    {
                                        RibbonButton button = item as RibbonButton;
                                        if (button != null && button.Label == "Replace")
                                        {
                                            if (richTextBoxAdv.IsReadOnly)
                                                button.IsEnabled = false;
                                            else
                                                button.IsEnabled = true;
                                        }
                                    }
                                }
                                else if (ribbonBar.Header == "Comments")
                                {
                                    foreach (var item in ribbonBar.Items)
                                    {
                                        RibbonButton button = item as RibbonButton;
                                        if (button != null && button.Label != "Show Comments")
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

        #region PageColor Implementation
        /// <summary>
        /// Updates the page color.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pageColorColorPicker_ColorChanged(object sender, SelectedBrushChangedEventArgs e)
        {
            if (e.NewColor != null)
            {
                richTextBoxAdv.Document.Background.Color = e.NewColor;
            }
        }
        #endregion

        #region PageMargins Implementation
        /// <summary>
        /// Initializes the page margins.
        /// </summary>
        private void InitializePageMargins()
        {
            List<PageMargins> items = new List<PageMargins> {
            new PageMargins { Key = "Normal", top = "Top: 72 pt", bottom = "Bottom: 72 pt", left = "Left: 72 pt", right = "Right: 72 pt" },
            new PageMargins { Key = "Narrow", top = "Top: 36 pt", bottom = "Bottom: 36 pt", left = "Left: 36 pt", right = "Right: 36 pt" },
            new PageMargins { Key = "Moderate", top = "Top: 72 pt", bottom = "Bottom: 72 pt", left = "Left: 54 pt", right = "Right: 54 pt" },
            new PageMargins { Key = "Wide", top = "Top: 72 pt", bottom = "Bottom: 72 pt", left = "Left: 144 pt", right = "Right: 144 pt" },
            new PageMargins { Key = "Mirrored", top = "Top: 72 pt", bottom = "Bottom: 72 pt", left = "Left: 90 pt", right = "Right: 72 pt" },
            new PageMargins { Key = "Office 2003 Default", top = "Top: 72 pt", bottom = "Bottom: 72 pt", left = "Left: 90 pt", right = "Right: 90 pt" }};
            pageMargins.ItemsSource = items;

            // Dictionary where key is a string and value is a list of strings
            pageMarginsCollection = new Dictionary<string, List<double>> {
                                   { "Normal", new List<double> { 72,72,72,72 } },
                                   { "Narrow", new List<double> { 36,36,36,36 } },
                                   { "Moderate", new List<double> { 72,72,54,54 } },
                                   { "Wide", new List<double> { 72,72,144,144 } },
                                   { "Mirrored", new List<double> { 72,72,90,72 } },
                                   { "Office 2003 Default", new List<double> { 72,72,90,90 } }};
        }

        /// <summary>
        /// Updates the page margins.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pageMargins_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedKey = (pageMargins.SelectedItem as PageMargins).Key;
            if (pageMarginsCollection.ContainsKey(selectedKey))
            {
                List<double> values = pageMarginsCollection[selectedKey];
                foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
                {
                    section.SectionFormat.PageMargin = new Thickness((values[2] * 96) / 72, (values[0] * 96) / 72, (values[3] * 96) / 72, (values[1] * 96) / 72);
                }
            }
        }
        #endregion

        #region PageSizes Implementation
        /// <summary>
        /// Initializes the page sizes.
        /// </summary>
        private void InitializePageSizes()
        {
            List<PageSize> items = new List<PageSize> {
                                   new PageSize { Key = "Letter",width="612 pt",height="792 pt" },
                                   new PageSize { Key = "Tabloid",width="792 pt",height="1224 pt" },
                                   new PageSize { Key = "Legal",width="612 pt",height="1008 pt" },
                                   new PageSize { Key = "Statement",width="396 pt",height="612 pt" },
                                   new PageSize { Key = "Executive",width="522 pt",height="756 pt" },
                                   new PageSize { Key = "A3",width="841.9 pt",height="1190.6 pt" },
                                   new PageSize { Key = "A4",width="595.3 pt",height="841.9 pt" },
                                   new PageSize { Key = "B4 (JIS)",width="728.5 pt",height="1031.8 pt" },
                                   new PageSize { Key = "B5 (JIS)",width="515.9 pt",height="728.5 pt" }};
            pageSize.ItemsSource = items;

            // Dictionary where key is a string and value is a list of strings
            pageSizesCollection = new Dictionary<string, List<double>> {
                                   { "Letter", new List<double> { 612,792} },
                                   { "Tabloid", new List<double> { 792,1224 } },
                                   { "Legal", new List<double> { 612,1008 } },
                                   { "Statement", new List<double> { 396,612 } },
                                   { "Executive", new List<double> { 522,756 } },
                                   { "A3", new List<double> { 841.9,1190.6 } },
                                   { "A4", new List<double> { 595.3,841.9} },
                                   { "B4 (JIS)", new List<double> { 728.5,1031.8 } },
                                   { "B5 (JIS)", new List<double> { 515.9,728.5 } }};
        }

        /// <summary>
        /// Updates the page sizes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pageSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string selectedKey = (pageSize.SelectedItem as PageSize).Key;
            if (pageSizesCollection.ContainsKey(selectedKey))
            {
                List<double> values = pageSizesCollection[selectedKey];
                foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
                {
                    section.SectionFormat.PageSize = new Size(
                (values[0] * 96) / 72,
                (values[1] * 96) / 72); ;
                }
            }
        }
        #endregion

        private void ApplyDefaultPageSettings()
        {
            // Default to A4 (595.3 x 841.9 pt → converted to pixels)
            double a4Width = (595.3 * 96) / 72;
            double a4Height = (841.9 * 96) / 72;

            // Default margin: 1 inch = 72 pt → converted to pixels
            double margin = (72 * 96) / 72;

            foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
            {
                section.SectionFormat.PageSize = new Size(a4Width, a4Height);
                section.SectionFormat.PageMargin = new Thickness(margin, margin, margin, margin);
            }

            // Optional: Set ComboBoxes to reflect default selections
            pageSize.SelectedIndex = pageSize.Items.Cast<PageSize>().ToList().FindIndex(p => p.Key == "A4");
            pageMargins.SelectedIndex = pageMargins.Items.Cast<PageMargins>().ToList().FindIndex(m => m.Key == "Normal");
        }

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

        /// <summary>
        /// Launches the page setup dialog on click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RibbonButton_Click(object sender, RoutedEventArgs e)
        {
            PageSetupDialog dialog = new PageSetupDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                double widthInInches = dialog.PageWidthInInches;
                double heightInInches = dialog.PageHeightInInches;
                double marginInInches = dialog.PageMarginInInches;

                // Convert the inches values to pixels, as SfRichTextBoxAdv preseve elements in pixels.
                const double dpi = 96;
                double widthInPixels = widthInInches * dpi;
                double heightInPixels = heightInInches * dpi;
                double marginInPixels = marginInInches * dpi;

                // Apply the converted values to the document
                foreach (SectionAdv section in richTextBoxAdv.Document.Sections)
                {
                    section.SectionFormat.PageSize = new Size(widthInPixels, heightInPixels);
                    section.SectionFormat.PageMargin = new Thickness(marginInPixels, marginInPixels, marginInPixels, marginInPixels);
                }
            }
        }

        /// <summary>
        /// Called on closing the ribbon window applictaion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ribbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Do you want to save changes to the document before exiting?",
                "Save Document",
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
    }

    /// <summary>
    /// Represents page margins with properties for top, bottom, left, and right margins, along with a key identifier.
    /// </summary>
    public class PageMargins
    {
        #region Fields
        public string Key { get; set; }
        public string top { get; set; }
        public string bottom { get; set; }
        public string left { get; set; }
        public string right { get; set; }
        public override string ToString()
        {
            return $"{Key}: {top}, {bottom}, {left}, {right}";
        }
        #endregion
    }

    /// <summary>
    /// Represents page size with properties for height, width, and a key identifier.
    /// </summary>
    public class PageSize
    {
        #region Fields
        public string Key { get; set; }
        public string height { get; set; }
        public string width { get; set; }
        public override string ToString()
        {
            return $"{Key}: {height}, {width}";
        }
        #endregion
    }
}