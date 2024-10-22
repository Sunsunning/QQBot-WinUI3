using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using QQBotCodePlugin.QQBot;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Text;
using Windows.Web.UI;
using static System.Net.Mime.MediaTypeNames;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConsolePage : Page
    {
        private BotMain main;
        private Dialog _dialog;
        public static bool Running = false;
        public ConsolePage()
        {
            this.InitializeComponent();
            HomePage.RunQQbotRequested += HomePage_RunQQbotRequested;
            _dialog = new Dialog();
        }
     

        private async void HomePage_RunQQbotRequested(object sender, string e)
        {
            /*main = new BotMain(LogStackPanel);
            await main.RunBot(LogStackPanel, 8080, 3000);
            */
            _dialog.show("错误", "启动失败", "好的", null, null, this.XamlRoot);
        }


        private void send_Click(object sender, RoutedEventArgs e)
        {
            ProcessCommand(Command.Text);
            Command.Text = null;
        }

        private void Command_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ProcessCommand(Command.Text);
                Command.Text = null;
            }
        }

        private void ProcessCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                _dialog.show("错误", "你没有输入任何命令!", "好的", null, null, this.XamlRoot);
                return;
            }
            RunCommand(command);
        }

        private void RunCommand(string command)
        {
            if (command.Equals("stop"))
            {
                if (main == null)
                {
                    _dialog.show("错误", "QQBotMain实例为null无法关闭", "好的", null, null, this.XamlRoot);
                    return;
                }

                main.StopBot();
                main = null;
                LogStackPanel.Children.Clear();
                return;
            }
            if (command.Equals("clear"))
            {
                if (LogStackPanel.Children.Count > 0)
                {
                    LogStackPanel.Children.Clear();
                    return;
                }
                return;
            }
            _dialog.show("错误", $"未找到{command}命令", "好的", null, null, this.XamlRoot);
        }

        private void Command_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<string>();
                var splitText = sender.Text.ToLower().Split(" ");
                foreach (var cat in Commands)
                {
                    var found = splitText.All((key) =>
                    {
                        return cat.ToLower().Contains(key);
                    });
                    if (found)
                    {
                        suitableItems.Add(cat);
                    }
                }
                if (suitableItems.Count == 0)
                {
                    suitableItems.Add("No results found");
                }
                sender.ItemsSource = suitableItems;
            }

        }

        private void Command_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            Command.Text = args.SelectedItem.ToString();
        }

        private List<string> Commands = new List<string>()
        {
            "stop",
            "clear"
        };

    }
}
