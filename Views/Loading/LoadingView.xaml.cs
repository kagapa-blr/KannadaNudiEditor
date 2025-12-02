using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KannadaNudiEditor.Views.Loading
{
    public partial class LoadingView : UserControl
    {
        private static Window? _popupWindow;
        private static EventHandler? _locationChangedHandler;
        private static SizeChangedEventHandler? _sizeChangedHandler;

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
                return;
            }

            var view = new LoadingView();
            var actualOwner = owner ?? Application.Current.MainWindow;

            _popupWindow = new Window
            {
                Owner = actualOwner,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Content = view,
                Topmost = true,
                SnapsToDevicePixels = true,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            // Safe width/height initialization
            _popupWindow.Width = actualOwner?.ActualWidth > 0 ? actualOwner.ActualWidth : actualOwner?.Width ?? 800;
            _popupWindow.Height = actualOwner?.ActualHeight > 0 ? actualOwner.ActualHeight : actualOwner?.Height ?? 600;

            if (actualOwner != null)
            {
                // Create handlers only once
                _locationChangedHandler = (_, _) => UpdatePosition(actualOwner);
                _sizeChangedHandler = (_, _) => UpdatePosition(actualOwner);

                actualOwner.LocationChanged += _locationChangedHandler;
                actualOwner.SizeChanged += _sizeChangedHandler;

                UpdatePosition(actualOwner);
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

            // Safe detach
            if (owner != null)
            {
                if (_locationChangedHandler != null)
                    owner.LocationChanged -= _locationChangedHandler;

                if (_sizeChangedHandler != null)
                    owner.SizeChanged -= _sizeChangedHandler;

                _locationChangedHandler = null;
                _sizeChangedHandler = null;
            }

            _popupWindow.Close();
            _popupWindow = null;

            SimpleLogger.Log("LoadingView hidden.");
        }

        private static void UpdatePosition(Window owner)
        {
            if (_popupWindow == null)
                return;

            _popupWindow.Left = owner.Left;
            _popupWindow.Top = owner.Top;
            _popupWindow.Width = owner.ActualWidth;
            _popupWindow.Height = owner.ActualHeight;

            SimpleLogger.Log($"LoadingView updated to {owner.Left},{owner.Top} size {owner.ActualWidth}x{owner.ActualHeight}");
        }
    }
}
