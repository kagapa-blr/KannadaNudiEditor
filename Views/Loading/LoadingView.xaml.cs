using System.Windows;
using System.Windows.Controls;

namespace KannadaNudiEditor.Views.Loading
{
    public partial class LoadingView : UserControl
    {
        private static Window? _popupWindow;

        public LoadingView()
        {
            InitializeComponent();
        }

        public static void Show(Window? owner = null)
        {
            if (_popupWindow != null) return;

            var loadingView = new LoadingView();
            var actualOwner = owner ?? Application.Current.MainWindow;

            _popupWindow = new Window
            {
                Owner = actualOwner,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = null,
                IsHitTestVisible = false, // Allows mouse click pass-through
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Width = actualOwner?.ActualWidth ?? 800,
                Height = actualOwner?.ActualHeight ?? 600,
                Left = actualOwner?.Left ?? 0,
                Top = actualOwner?.Top ?? 0,
                Content = loadingView,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            // Block input to the main window
            if (actualOwner != null)
            {
                actualOwner.IsEnabled = false;
            }

            _popupWindow.Show();
        }

        public static void Hide()
        {
            if (_popupWindow != null)
            {
                if (_popupWindow.Owner != null)
                {
                    _popupWindow.Owner.IsEnabled = true;
                }

                _popupWindow.Close();
                _popupWindow = null;
            }
        }
    }
}
