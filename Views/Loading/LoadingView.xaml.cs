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
                IsHitTestVisible = false, // Optional if you want mouse passthrough
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Width = actualOwner?.ActualWidth ?? 800,
                Height = actualOwner?.ActualHeight ?? 600,
                Content = loadingView,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            if (actualOwner != null)
            {
                actualOwner.IsEnabled = false;

                // Center overlay on the owner window
                _popupWindow.Left = actualOwner.Left + (actualOwner.ActualWidth - _popupWindow.Width) / 2;
                _popupWindow.Top = actualOwner.Top + (actualOwner.ActualHeight - _popupWindow.Height) / 2;
            }

            _popupWindow.Show();
        }

        public static void Hide()
        {
            if (_popupWindow != null)
            {
                if (_popupWindow.Owner != null)
                    _popupWindow.Owner.IsEnabled = true;

                _popupWindow.Close();
                _popupWindow = null;
            }
        }
    }
}
