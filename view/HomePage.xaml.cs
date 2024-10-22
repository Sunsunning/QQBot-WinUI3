using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.utils;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

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
        public static event EventHandler<string> RunQQbotRequested;

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
            if (ConsolePage.Running)
            {
                PathError("你已经启动了一个QQBotMain实例请关闭后在启动其他实例");
                return;
            }
            NavigateToPageRequested(this, "Console");
            RunQQbotRequested(this, _settingManager.GetValue<string>("QQBotPath"));
            

        }

        private void PathError(string message)
        {
            _dialog.show("错误", message, "好的", "更好了", null, this.XamlRoot);
        }
    }
}
