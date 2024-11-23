using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using QQBotCodePlugin.QQBot;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConsolePage : Page
    {
        private Dialog _dialog;
        private BotMain main;
        public static bool Running = false;
        public ConsolePage()
        {
            this.InitializeComponent();
            BotListPage.RunQQbotRequested += HomePage_RunQQbotRequested;
            _dialog = new Dialog();
        }


        private async void HomePage_RunQQbotRequested(object sender, RunBotInfo info)
        {
            main = new BotMain(LogStackPanel, info);
            await main.RunBot();
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
                _dialog.show("����", "��û�������κ�����!", "�õ�", null, null, this.XamlRoot);
                return;
            }
            RunCommand(command);
        }

        private async void RunCommand(string command)
        {
            if (command.Equals("stop"))
            {
                if (main == null)
                {
                    _dialog.show("����", "QQBotMainʵ��Ϊnull�޷��ر�", "�õ�", null, null, this.XamlRoot);
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
            if (command.StartsWith("send "))
            {
                string[] parts = command.Split(' ');
                if (parts.Length != 3)
                {
                    _dialog.show("����", $"�÷�:send [GroupID] [message]", "�õ�", null, null, this.XamlRoot);
                    return;
                }
                if (BigInteger.TryParse(parts[1], out BigInteger bigNumber))
                {
                    long GroupID = (Int64)bigNumber;
                    string message = parts[2];
                    main.sendDirectMessage(GroupID, message);
                    return;
                }
                else
                {
                    _dialog.show("����", $"{parts[1]}����һ����Ч��Ⱥ��", "�õ�", null, null, this.XamlRoot);
                    return;
                }
            }
            if (command.Equals("restart"))
            {
                LogStackPanel.Children.Clear();
                if (main == null)
                {
                    _dialog.show("����", "QQBotMainʵ��Ϊnull�޷�����", "�õ�", null, null, this.XamlRoot);
                    return;
                }
                main.ReStart();
                return;
            }
            if (command.Equals("info"))
            {
                if (main == null)
                {
                    _dialog.show("����", "QQBotMainʵ��Ϊnull�޷���ʾ��������Ϣ", "�õ�", null, null, this.XamlRoot);
                    return;
                }
                long ReceivedCount = main.getReceivedCount();
                long ReceptionGroupMsgCount = main.getReceptionGroupMsgCount();
                long ReceptionPrivateMsgCount = main.getReceptionPrivateMsgCount();
                _dialog.show("����", $"�ɹ�����Ⱥ����Ϣ:{ReceptionGroupMsgCount}\n�ɹ�����˽����Ϣ:{ReceptionPrivateMsgCount}\n�ɹ��ظ�:{ReceivedCount}", "�õ�", null, null, this.XamlRoot);
                return;
            }
            _dialog.show("����", $"δ�ҵ�{command}����", "�õ�", null, null, this.XamlRoot);
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
            "clear",
            "send",
            "restart",
            "info"
        };

    }
}