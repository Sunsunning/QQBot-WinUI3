using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        public static event EventHandler<string> NavigateToPageRequested;

        public HomePage()
        {
            this.InitializeComponent();
            CurrentTime = DateTime.Now;
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
