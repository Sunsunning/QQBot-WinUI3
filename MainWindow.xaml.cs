using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using QQBotCodePlugin.utils;
using QQBotCodePlugin.view;
using System;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        SettingManager settingManager;
        WindowsSystemDispatcherQueueHelper m_wsdqHelper; // See separate sample below for implementation
        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController m_acrylicController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;
        public MainWindow()
        {
            this.InitializeComponent();
            settingManager = new SettingManager();
            viewer.SelectedItem = HomePage;
            view.HomePage.NavigateToPageRequested += _homePage_SelectionFrame;
            view.BotListPage.NavigateToPageRequested += _homePage_SelectionFrame;
            view.SettingPage.ChangeBackground += _settingPage_ChangeBackground;
            Initialize();
        }

        public void _settingPage_ChangeBackground(object sender, string background)
        {
            if (background.Equals("Acrylic(Thin)"))
            {
                TrySetAcrylicBackdrop(true);
                return;
            }
            TrySetMicaBackdrop(true);
        }

        void Initialize()
        {
            if (!settingManager.ContainsKey("BackGround")) return;
            if (settingManager.GetValue<string>("BackGround").Equals("Acrylic(Thin)"))
            {
                TrySetAcrylicBackdrop(true);
                return;
            }
            TrySetMicaBackdrop(true);
        }

        bool TrySetMicaBackdrop(bool useMicaAlt)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                Microsoft.UI.Xaml.Media.MicaBackdrop micaBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop();
                micaBackdrop.Kind = useMicaAlt ? Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt : Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
                this.SystemBackdrop = micaBackdrop;

                return true;
            }

            return false;
        }

        bool TrySetAcrylicBackdrop(bool useAcrylicThin)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
                m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

                // Hooking up the policy object
                m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
                this.Activated += Window_Activated;
                this.Closed += Window_Closed;
                ((FrameworkElement)this.Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
                m_configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();

                m_acrylicController.Kind = useAcrylicThin ? Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Thin : Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicKind.Base;

                // Enable the system backdrop.
                // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
                m_acrylicController.AddSystemBackdropTarget(this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                return true; // Succeeded.
            }

            return false; // Acrylic is not supported on this system.
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
            // use this closed window.
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            this.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (m_configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }

        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
                case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
            }
        }

        private void viewer_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                sender.Header = "ÉèÖÃ";
                PagesContent.Navigate(typeof(SettingPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                return;
            }

            var selectedItem = args.SelectedItem as NavigationViewItem;
            sender.Header = selectedItem?.Content;

            Type pageType = selectedItem?.Tag.ToString() switch
            {
                "Home" => typeof(HomePage),
                "Console" => typeof(ConsolePage),
                "AddBot" => typeof(AddBotPage),
                "BotList" => typeof(BotListPage),
                "PluginStore" => typeof(PluginStorePage),
                _ => typeof(NotFoundPage)
            };
            PagesContent.Navigate(pageType, null, new DrillInNavigationTransitionInfo());
        }

        public void _homePage_SelectionFrame(object sender, string @name)
        {
            if (name.Equals("Console"))
            {
                viewer.SelectedItem = ConsolePage;
                return;
            }
            if (name.Equals("Setting"))
            {
                viewer.SelectedItem = viewer.SettingsItem;
                return;
            }
            if (name.Equals("BotList"))
            {
                viewer.SelectedItem = BotListPage;
                return;
            }
        }
    }
}
