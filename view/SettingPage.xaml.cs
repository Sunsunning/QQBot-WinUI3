using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.utils;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private readonly SettingManager _settingManager;
        private readonly Dialog _dialog;
        public static EventHandler<string> ChangeBackground;
        public SettingPage()
        {
            this.InitializeComponent();
            _settingManager = new SettingManager();
            _dialog = new Dialog();
            Initialize();
        }

        private void Initialize()
        {
            if (_settingManager.ContainsKey("QQBotPath") && !string.IsNullOrEmpty(_settingManager.GetValue<string>("QQBotPath")))
            {
                DirectoryPath.Text = _settingManager.GetValue<string>("QQBotPath");
            }
            if (_settingManager.ContainsKey("BackGround") && !string.IsNullOrEmpty(_settingManager.GetValue<string>("BackGround")))
            {
                background.SelectedItem = _settingManager.GetValue<string>("BackGround");
            }
            if (_settingManager.ContainsKey("HTTP") && !string.IsNullOrEmpty(_settingManager.GetValue<string>("HTTP")))
            {
                HTTP.Text = _settingManager.GetValue<string>("HTTP");
            }
            if (_settingManager.ContainsKey("ChromePath") && !string.IsNullOrEmpty(_settingManager.GetValue<string>("ChromePath")))
            {
                ChromePath.Text = _settingManager.GetValue<string>("ChromePath");
            }
        }

        private async void SlecetedPath_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new Windows.Storage.Pickers.FolderPicker();
            var window = App.GetMainWindow();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await openPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                _settingManager.SetValue("QQBotPath", folder.Path);
                DirectoryPath.Text = folder.Path;
                _dialog.show("完成", $"成功设置目录为:\n{folder.Path}", "Ok", null, null, this.XamlRoot);
                _settingManager.InitializeFolder();
            }

        }

        private void background_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string background_item = _settingManager.GetValue<string>("BackGround");
            if (background.SelectedValue.ToString() == background_item) return;
            _settingManager.SetValue<string>("BackGround", background.SelectedValue.ToString());
            ChangeBackground(this, background.SelectedValue.ToString());
        }

        private void HTTP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (HTTP.Text.Equals(_settingManager.GetValue<string>("HTTP")))
            {
                return;
            }
            _settingManager.SetValue<string>("HTTP", HTTP.Text);
        }

        private void ChromePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ChromePath.Text.Equals(_settingManager.GetValue<string>("ChromePath")))
            {
                return;
            }
            _settingManager.SetValue<string>("ChromePath", ChromePath.Text);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            _settingManager.ResetSettings();
            Initialize();
        }
    }
}
