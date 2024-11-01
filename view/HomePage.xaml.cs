using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.utils;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private readonly DateTime CurrentTime;
        private readonly SettingManager _settingManager;
        private readonly Dialog _dialog;
        public static event EventHandler<string> NavigateToPageRequested;
        public static event EventHandler<RunBotInfo> RunQQbotRequested;

        public HomePage()
        {
            this.InitializeComponent();
            CurrentTime = DateTime.Now;
            _settingManager = new SettingManager();
            _dialog = new Dialog();
            UpdateTimeText();

        }

        private void UpdateTimeText()
        {
            if (CurrentTime.Hour < 12)
            {
                Time.Text = "早上好";
            }
            else if (CurrentTime.Hour < 18)
            {
                Time.Text = "下午好";
            }
            else
            {
                Time.Text = "晚上好";
            }
        }

        private void StartBot_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPageRequested(this, "BotList");
        }
    }
}
