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
            if (_popupWindow != null) return; // Already visible

            var loadingView = new LoadingView();
            var actualOwner = owner ?? Application.Current.MainWindow;

            _popupWindow = new Window
            {
                Owner = actualOwner,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = null,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Content = loadingView,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Width = actualOwner?.ActualWidth ?? 800,
                Height = actualOwner?.ActualHeight ?? 600
            };

            if (actualOwner != null)
                actualOwner.IsEnabled = false;

            _popupWindow.Show();

            // Keep overlay position in sync when owner moves/resizes
            if (actualOwner != null)
            {
                actualOwner.LocationChanged += (_, _) => UpdatePosition(actualOwner);
                actualOwner.SizeChanged += (_, _) => UpdatePosition(actualOwner);
            }
        }

        public static void Hide()
        {
            if (_popupWindow == null) return;

            var owner = _popupWindow.Owner;
            if (owner != null)
                owner.IsEnabled = true;

            _popupWindow.Close();
            _popupWindow = null;
        }

        private static void UpdatePosition(Window owner)
        {
            if (_popupWindow == null) return;

            _popupWindow.Left = owner.Left;
            _popupWindow.Top = owner.Top;
            _popupWindow.Width = owner.ActualWidth;
            _popupWindow.Height = owner.ActualHeight;
        }
    }
}
