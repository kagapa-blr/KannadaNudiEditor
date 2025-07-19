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
                IsHitTestVisible = false,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Width = actualOwner?.ActualWidth ?? 800,
                Height = actualOwner?.ActualHeight ?? 600,
                Content = loadingView,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            _popupWindow.Show();
        }

        public static void Hide()
        {
            if (_popupWindow != null)
            {
                _popupWindow.Close();
                _popupWindow = null;
            }
        }
    }
}
