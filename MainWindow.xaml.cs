using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using QQBotCodePlugin.view;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.UserDataAccounts.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            TrySetDesktopAcrylicBackdrop();
            this.viewer.SelectedItem = HomePage;
            view.HomePage.NavigateToPageRequested += _homePage_SelectionFrame;
        }

        bool TrySetDesktopAcrylicBackdrop()
        {
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop DesktopAcrylicBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop();
                this.SystemBackdrop = DesktopAcrylicBackdrop;
                return true;
            }
            return false;
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
        }
    }
}
