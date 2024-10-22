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
                Time.Text = "���Ϻ�";
            }
            else if (CurrentTime.Hour < 18)
            {
                Time.Text = "�����";
            }
            else
            {
                Time.Text = "���Ϻ�";
            }
        }

        private void StartBot_Click(object sender, RoutedEventArgs e)
        {
            if (ConsolePage.Running)
            {
                PathError("���Ѿ�������һ��QQBotMainʵ����رպ�����������ʵ��");
                return;
            }
            NavigateToPageRequested(this, "Console");
            RunQQbotRequested(this, _settingManager.GetValue<string>("QQBotPath"));
            

        }

        private void PathError(string message)
        {
            _dialog.show("����", message, "�õ�", "������", null, this.XamlRoot);
        }
    }
}
