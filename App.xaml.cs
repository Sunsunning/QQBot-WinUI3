using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.IO;
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
            this.Initialized();
            m_window = new MainWindow();
            var ScreenHeight = DisplayArea.Primary.WorkArea.Height;
            m_window.AppWindow.MoveAndResize(new RectInt32(500, (int)(ScreenHeight - 40 - 700), 1011, 533));
            m_window.ExtendsContentIntoTitleBar = true;
            m_window.Activate();
        }
        private void Initialized()
        {
            string RootPath = manager.GetValue<string>("QQBotPath");
            if (string.IsNullOrEmpty(RootPath))
            {
                return;
            }
            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
            }
            try
            {
                foreach (var folder in folders)
                {
                    string path = Path.Combine(RootPath, folder);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                foreach (var file in files)
                {
                    string path = Path.Combine(RootPath, file);
                    if (!File.Exists(path))
                    {
                        File.Create(path);
                    }
                }
            }
            catch (IOException ex)
            {
                return;
            }
            catch (Exception exc)
            {
                return;
            }
        }

        public static Window GetMainWindow() => m_window;
        private List<string> folders = new List<string>() { "Bots" };
        private List<string> files = new List<string>();
        private SettingManager manager = new SettingManager();
        private static Window m_window;
    }
}
