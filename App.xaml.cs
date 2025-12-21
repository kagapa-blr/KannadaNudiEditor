using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using KannadaNudiEditor.Helpers;
using KannadaNudiEditor.Views;

namespace KannadaNudiEditor
{
    public partial class App : Application
    {
        private readonly ProcessHelper _processHelper;

        public App()
        {
            SimpleLogger.Log("=== Application Started ===");

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
                SimpleLogger.Log("FirstChanceException: " + e.Exception.Message);

            _processHelper = new ProcessHelper();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                LicenseHelper.RegisterSyncfusionLicense();

                _processHelper.LaunchKannadaKeyboard();

                var banner = new BannerWindow();
                banner.Show();
                await Dispatcher.Yield(DispatcherPriority.Render);

                await Task.Run(EditorInitializer.Initialize);

                var mainWindow = new MainWindow();
                mainWindow.Show();

                AnimationHelper.FadeOutAndClose(banner, 600);
            }
            catch (Exception ex)
            {
                SimpleLogger.Log("Startup exception: " + ex);
                KillKeyboardProcess();
                ErrorHelper.ShowError("Unexpected error during startup", ex);
                Shutdown();
            }
        }

        // ✅ Make this method public so windows can call it
        public void KillKeyboardProcess()
        {
            _processHelper.KillKeyboardProcess();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            KillKeyboardProcess(); // Ensure keyboard is killed on exit
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SimpleLogger.Log("DispatcherUnhandledException: " + e.Exception);
            KillKeyboardProcess();
            ErrorHelper.ShowError("A UI thread error occurred", e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                SimpleLogger.Log("UnhandledException: " + ex);
                KillKeyboardProcess();
                ErrorHelper.ShowError("A background thread error occurred", ex);
            }
            else
            {
                SimpleLogger.Log("UnhandledException: Unknown exception");
                KillKeyboardProcess();
                ErrorHelper.ShowError("A fatal unknown error occurred", null);
            }
        }
    }
}
