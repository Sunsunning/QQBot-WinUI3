using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using QQBotCodePlugin.utils;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            var ScreenHeight = DisplayArea.Primary.WorkArea.Height;
            m_window.AppWindow.MoveAndResize(new RectInt32(500, (int)(ScreenHeight - 40 - 700), 1011, 560));
            m_window.ExtendsContentIntoTitleBar = true;
            m_window.Activate();
            manager.InitializeFolder();
            appLogger = new AppLogger(Path.Combine(manager.GetValue<string>("QQBotPath"), "logs"));
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                StackTrace stackTrace = new StackTrace(args.Exception, true);
                appLogger.Log(args.Exception.Message,true,stackTrace);
                args.SetObserved();
            };
            appLogger.Log("MainWindow Initialized Successfully!");
            appLogger.Log("Application Launched and Initialized Successfully!");
        }

        private SettingManager manager = new SettingManager();
        public static AppLogger appLogger { get; set; }
        public static AppLogger GetAppLogger() => appLogger;
        public static Window GetMainWindow() => m_window;
        private static Window m_window;
    }
}
