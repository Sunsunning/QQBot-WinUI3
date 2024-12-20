using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddBotPage : Page
    {
        private SettingManager settingManager;
        private Dialog _dialog;
        private readonly List<CheckBox> checkBoxList;

        public AddBotPage()
        {
            this.InitializeComponent();
            settingManager = new SettingManager();
            _dialog = new Dialog();
            checkBoxList = new List<CheckBox>
            {
                    Plugin, ba, cat, dragon, eat, play, AIChat, AIChatPrivate, Help, KudosMe, NumberBoom, onset, ping, RunWindowsCommand, Sky, wife
            };
        }

        private void OptionsAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in checkBoxList)
            {
                checkBox.IsChecked = true;
            }
        }

        private void OptionsAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in checkBoxList)
            {
                checkBox.IsChecked = false;
            }
        }

        private void AreAllAbilitiesSelected()
        {
            bool allChecked = checkBoxList.All(checkBox => checkBox.IsChecked == true);
            OptionsAllCheckBox.IsChecked = allChecked ? true : (bool?)null;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AreAllAbilitiesSelected();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(BotName.Text))
            {
                _dialog.show("错误", "机器人名称不能为空", "好的", null, null, this.XamlRoot);
                return;
            }
            if (string.IsNullOrEmpty(IP.Text))
            {
                _dialog.show("错误", "IP地址不能为空", "好的", null, null, this.XamlRoot);
                return;
            }
            if (ServerPort.Value == 0 || EventPort.Value == 0)
            {
                _dialog.show("错误", "服务监听端口与事件上报端口数值错误", "好的", null, null, this.XamlRoot);
                return;
            }
            if (ServerPort.Value == EventPort.Value)
            {
                _dialog.show("错误", "服务监听端口与事件上报端口不能相同", "好的", null, null, this.XamlRoot);
                return;
            }
            if (AIChat.IsChecked == true && string.IsNullOrEmpty(key.Text))
            {
                _dialog.show("错误", "你已启用ChatGPT请填写GPT Key若无请去\nhttps://github.com/chatanywhere/GPT_API_free领取", "好的", null, null, this.XamlRoot);
                return;
            }
            try
            {
                string rootPath = settingManager.GetValue<string>("QQBotPath");
                string botsPath = Path.Combine(rootPath, "Bots");
                string botPath = Path.Combine(botsPath, BotName.Text);

                Directory.CreateDirectory(botPath);

                if (Directory.GetFiles(botPath).Any(f => Path.GetFileName(f).Equals("bot.json", StringComparison.OrdinalIgnoreCase)))
                {
                    _dialog.show("错误", $"{BotName.Text} 已存在，请换一个名称", "好的", null, null, this.XamlRoot);
                    return;
                }

                string botJsonPath = Path.Combine(botPath, "bot.json");
                var jsonContent = WriteJson(BotName.Text, Description.Text, IP.Text, (int)ServerPort.Value, (int)EventPort.Value);
                File.WriteAllText(botJsonPath, jsonContent);

                _dialog.show("完成", $"成功添加机器人 {BotName.Text}", "好的", null, null, this.XamlRoot);
            }
            catch (UnauthorizedAccessException ex)
            {
                App.GetAppLogger().Log($"Error: {ex.Message}", false);
                _dialog.show("错误", $"没有权限访问:\n{ex.Message}", "好的", null, null, this.XamlRoot);
            }
            catch (Exception ex)
            {
                App.GetAppLogger().Log($"Error: {ex.Message}", false);
                _dialog.show("错误", $"出现错误:\n{ex.Message}", "好的", null, null, this.XamlRoot);
            }
        }
        private string WriteJson(string _BotName, string _Description, string _IP, int _ServerPort, int _EventPort)
        {
            var json = new JObject
            {
                ["Name"] = _BotName,
                ["key"] = key.Text,
                ["Plugin"] = Plugin.IsChecked,
                ["Description"] = _Description,
                ["IP"] = _IP,
                ["ServerPort"] = _ServerPort,
                ["EventPort"] = _EventPort,
                ["abilities"] = new JObject
                {
                    ["Meme"] = new JObject
                    {
                        ["ba"] = ba.IsChecked,
                        ["cat"] = cat.IsChecked,
                        ["dragon"] = dragon.IsChecked,
                        ["eat"] = eat.IsChecked,
                        ["play"] = play.IsChecked,
                    },
                    ["AIChatPrivate"] = AIChat.IsChecked,
                    ["AIChat"] = AIChat.IsChecked,
                    ["Help"] = Help.IsChecked,
                    ["KudosMe"] = KudosMe.IsChecked,
                    ["NumberBoom"] = NumberBoom.IsChecked,
                    ["onset"] = onset.IsChecked,
                    ["Ping"] = ping.IsChecked,
                    ["RunWindowsCommand"] = RunWindowsCommand.IsChecked,
                    ["Sky"] = Sky.IsChecked,
                    ["wife"] = wife.IsChecked
                }
            };
            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }
    }
}
