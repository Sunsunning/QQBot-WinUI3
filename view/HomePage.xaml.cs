using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.utils;
using System;
using System.Net.Http;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private readonly SettingManager settingManager;
        private readonly DateTime CurrentTime;
        public static event EventHandler<string> NavigateToPageRequested;

        public HomePage()
        {
            this.InitializeComponent();
            CurrentTime = DateTime.Now;
            settingManager = new SettingManager();
            UpdateTimeText();
            UpdateMessageText($"{settingManager.GetValue<string>("HTTP")}get_message");
        }

        private async void UpdateMessageText(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        message.Text = responseContent;
                    }
                    else
                    {
                        message.Text += "(其实没有拉取到信息……)";
                    }
                }
                catch (Exception ex)
                {
                    ToolTip tip = new ToolTip() { Content = ex.Message };
                    ToolTipService.SetToolTip(message, tip);
                }
            }
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
