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
            SimpleLogger.Log("LoadingView initialized.");
        }

        public static void Show(Window? owner = null)
        {
            if (_popupWindow != null)
            {
                SimpleLogger.Log("LoadingView already visible.");
                return; // Already visible
            }

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
            {
                actualOwner.IsEnabled = false;
                actualOwner.LocationChanged += (_, _) => UpdatePosition(actualOwner);
                actualOwner.SizeChanged += (_, _) => UpdatePosition(actualOwner);
                SimpleLogger.Log($"LoadingView attached to owner window at {actualOwner.Left},{actualOwner.Top}");
            }

            _popupWindow.Show();
            SimpleLogger.Log("LoadingView shown.");
        }

        public static void Hide()
        {
            if (_popupWindow == null)
            {
                SimpleLogger.Log("LoadingView already hidden or not shown.");
                return;
            }

            var owner = _popupWindow.Owner;
            if (owner != null)
            {
                owner.IsEnabled = true;
                owner.LocationChanged -= (_, _) => UpdatePosition(owner);
                owner.SizeChanged -= (_, _) => UpdatePosition(owner);
                SimpleLogger.Log($"LoadingView detached from owner window at {owner.Left},{owner.Top}");
            }

            _popupWindow.Close();
            _popupWindow = null;

            SimpleLogger.Log("LoadingView hidden.");
        }

        private static void UpdatePosition(Window owner)
        {
            if (_popupWindow == null)
            {
                return;
            }

            _popupWindow.Left = owner.Left;
            _popupWindow.Top = owner.Top;
            _popupWindow.Width = owner.ActualWidth;
            _popupWindow.Height = owner.ActualHeight;

            SimpleLogger.Log($"LoadingView position updated to {owner.Left},{owner.Top}, size {owner.ActualWidth}x{owner.ActualHeight}");
        }
    }
}
