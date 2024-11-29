using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.utils;
using System;
using System.Diagnostics;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BotListPage : Page
    {
        private Dialog _dialog;
        private SettingManager _settingManager;
        public static event EventHandler<string> NavigateToPageRequested;
        public static event EventHandler<RunBotInfo> RunQQbotRequested;
        public BotListPage()
        {
            this.InitializeComponent();
            _dialog = new Dialog();
            _settingManager = new SettingManager();
            Initialize();
        }

        private void Initialize()
        {
            Main.Children.Clear();
            try
            {
                string FolderPath = Path.Combine(_settingManager.GetValue<string>("QQBotPath"), "Bots");
                if (Directory.GetDirectories(FolderPath).Length == 0)
                {
                    TextBlock text = new TextBlock()
                    {
                        Text = "无机器人,快去添加吧",
                        FontSize = 25,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Main.Children.Add(text);
                    return;
                }
                foreach (var folder in Directory.GetDirectories(FolderPath))
                {
                    string botName = Path.GetFileName(folder);
                    string json = File.ReadAllText(Path.Combine(folder, "bot.json"));
                    JObject paris = JObject.Parse(json);
                    string description = (string)paris["Description"];
                    Main.Children.Add(CreateBotBorder(botName, description.Equals("") ? "无" : description, folder, json));
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
                _dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                return;
            }
            catch (Exception ex)
            {
                _dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                return;
            }
        }

        private Border CreateBotBorder(string botName, string description, string path, string jsonFile)
        {
            Border border = new Border
            {
                Background = (Brush)App.Current.Resources["CardBackgroundFillColorDefaultBrush"],
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(56, 5, 36, 0),
                Height = 67,
                CornerRadius = new CornerRadius(2)
            };

            Grid grid = new Grid();
            border.Child = grid;

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Grid leftGrid = new Grid();
            grid.Children.Add(leftGrid);
            Grid.SetColumn(leftGrid, 0);

            Viewbox viewbox = new Viewbox
            {
                Width = 25,
                Height = 25,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(15, 0, 0, 0)
            };
            viewbox.Child = new FontIcon() { Glyph = "\uE99A" };
            leftGrid.Children.Add(viewbox);

            TextBlock botNameTextBlock = new TextBlock
            {
                Text = botName,
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(50, 15, 0, 0)
            };
            leftGrid.Children.Add(botNameTextBlock);
            Grid.SetColumnSpan(botNameTextBlock, 2);

            TextBlock descriptionTextBlock = new TextBlock
            {
                Text = description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(50, 32, 0, 0),
                Height = 17
            };
            leftGrid.Children.Add(descriptionTextBlock);
            Grid.SetColumn(descriptionTextBlock, 0);

            string p1 = Path.Combine(path, "plugins");
            int c = 0;
            if (Directory.Exists(p1)) c = Directory.GetFiles(path,"*.dll", SearchOption.AllDirectories).Length;
            TextBlock pluginCountTextBlock = new TextBlock
            {
                Text = $"插件:{c}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(50, 46, 0, 0),
                Height = 17
            };
            leftGrid.Children.Add(pluginCountTextBlock);
            Grid.SetColumn(pluginCountTextBlock, 0);

            StackPanel actionsStackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Spacing = 10,
                Margin = new Thickness(0, 0, 10, 0)
            };
            grid.Children.Add(actionsStackPanel);
            Grid.SetColumn(actionsStackPanel, 1);

            Button openButton = new Button
            {
                Content = new StackPanel
                {
                    Children =
            {
                new FontIcon { Glyph = "\uE838" },
                new TextBlock { Text = "打开" }
            }
                }
            };
            actionsStackPanel.Children.Add(openButton);

            Button startButton = new Button
            {
                Content = new StackPanel
                {
                    Children =
            {
                new FontIcon { Glyph = "\uE768" },
                new TextBlock { Text = "启动" }
            }
                }
            };
            actionsStackPanel.Children.Add(startButton);

            Button deleteButton = new Button
            {
                Style = (Style)App.Current.Resources["AccentButtonStyle"],
                Content = new StackPanel
                {
                    Children =
            {
                new FontIcon { Glyph = "\uE74D"},
                new TextBlock { Text = "删除" }
            }
                }
            };
            actionsStackPanel.Children.Add(deleteButton);

            openButton.Tag = path;
            openButton.Click += OpenButton_Click;

            startButton.Tag = new Data { DirectoryPath = path, json = jsonFile };
            startButton.Click += StartButton_Click;


            deleteButton.Tag = path;
            deleteButton.Click += DeleteButton_Click;

            return border;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string path = button.Tag as string;
            Process.Start("explorer.exe", path);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Data data = button.Tag as Data;

            if (ConsolePage.Running)
            {
                _dialog.show("错误", "当前有一个BotMain实例正在运行请关闭后再试", "好的", null, null, this.XamlRoot);
                return;
            }

            BotConfig botConfig = JsonConvert.DeserializeObject<BotConfig>(data.json);
            RunBotInfo runBotInfo = new RunBotInfo()
            {
                BotPath = data.DirectoryPath,
                BotName = botConfig.Name,
                Plugin = botConfig.Plugin,
                BotDescription = botConfig.Description,
                IPAddress = botConfig.IP,
                ServerPort = botConfig.ServerPort,
                EventPort = botConfig.EventPort,
                EventData = new System.Collections.Generic.Dictionary<string, bool>() {
                    { "ba",botConfig.Abilities.Meme.Ba },
                    { "cat",botConfig.Abilities.Meme.Cat },
                    { "dragon",botConfig.Abilities.Meme.Dragon },
                    { "eat",botConfig.Abilities.Meme.Eat },
                    { "play",botConfig.Abilities.Meme.Play },
                    { "AIChat",botConfig.Abilities.AIChat },
                    { "AIChatPrivate",botConfig.Abilities.AIChatPrivate },
                    { "Help",botConfig.Abilities.Help },
                    { "KudosMe",botConfig.Abilities.KudosMe },
                    { "NumberBoom",botConfig.Abilities.NumberBoom },
                    { "onset",botConfig.Abilities.Onset },
                    { "Ping",botConfig.Abilities.Ping },
                    { "RunWindowsCommand",botConfig.Abilities.RunWindowsCommand },
                    { "Sky",botConfig.Abilities.Sky },
                    { "wife",botConfig.Abilities.Wife }
                }
            };
            NavigateToPageRequested(this, "Console");
            RunQQbotRequested(this, runBotInfo);
        }


        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string path = button.Tag as string;

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                _dialog.show("完成", $"成功删除{path}", "好的", null, null, this.XamlRoot);
                RefreshPage();
            }
            else
            {
                _dialog.show("错误", "路径不存在无法删除,请刷新此页面", "好的", null, null, this.XamlRoot);
                return;
            }
        }

        private void RefreshPage()
        {
            Main.Children.Clear();
            Initialize();
        }
    }
    public class Data
    {
        public string DirectoryPath { get; set; }
        public string json { get; set; }
    }
}
