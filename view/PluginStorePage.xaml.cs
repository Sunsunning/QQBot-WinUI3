using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.UI.Xaml.Application;
using Windows.Storage.Provider;
using ABI.System;
using QQBotCodePlugin.QQBot;
using Exception = System.Exception;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QQBotCodePlugin.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PluginStorePage : Page
    {
        private readonly SettingManager settingManager;
        private readonly StackPanel stackPanel;
        private readonly List<string> Bots = new List<string>();
        private readonly Dialog dialog;
        private readonly string get;
        private readonly string upload;
        private readonly string download;
        private string json = "";


        public PluginStorePage()
        {
            this.InitializeComponent();
            dialog = new Dialog();
            stackPanel = new StackPanel() { Spacing = 10 };
            settingManager = new SettingManager();
            get = $"{settingManager.GetValue<string>("HTTP")}get_plugins";
            upload = $"{settingManager.GetValue<string>("HTTP")}upload";
            download = $"{settingManager.GetValue<string>("HTTP")}download";
            Bots = getBots();
            GetResponseAsync();
        }

        private List<string> getBots()
        {
            List<string> list = new List<string>();
            string BotsPath = Path.Combine(settingManager.GetValue<string>("QQBotPath"), "Bots");
            foreach (var directory in Directory.GetDirectories(BotsPath))
            {
                list.Add(Path.GetFileName(directory));
            }
            return list;
        }

        private async Task GetResponseAsync()
        {
            TextBlock block = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 20, FontWeight = FontWeights.Bold };
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(get);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        json = responseContent;
                    }
                    else
                    {
                        block.Text = $"Failed to get response. Status code: {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    block.Text = $"Error sending request: {ex.Message}";
                }
            }
            MainGrid.Children.Clear();
            if (string.IsNullOrEmpty(block.Text) && !string.IsNullOrEmpty(json))
            {
                StackPanel s = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 5 };
                Button refresh = new Button() { Content = "刷新" };
                Button uploadBtn = new Button() { Content = "上传插件" };
                refresh.Click += (s, e) =>
                {
                    stackPanel.Children.Clear();
                    GetResponseAsync();
                };
                uploadBtn.Click += (s, e) =>
                {
                    StackPanel stack = new StackPanel();
                    TextBox FileName = new TextBox() { Header = "文件名" };
                    TextBox PluginName = new TextBox() { Header = "插件名" };
                    TextBox version = new TextBox() { Header = "版本号" };
                    TextBox author = new TextBox() { Header = "作者" };
                    TextBox description = new TextBox() { Header = "描述" };
                    TextBox dependencies = new TextBox() { Header = "依赖" };
                    TextBox path = new TextBox() { Header = "路径", IsReadOnly = true };
                    Button file = new Button() { Content = "选择文件",HorizontalAlignment = HorizontalAlignment.Stretch,Margin  = new Thickness(0,5,0,0) };
                    file.Click += (s, e) =>
                    {
                        async Task OpenFileAsync()
                        {
                            var openPicker = new FileOpenPicker();
                            var window = App.GetMainWindow();
                            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
                            openPicker.ViewMode = PickerViewMode.Thumbnail;
                            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                            openPicker.FileTypeFilter.Add(".dll");

                            var file = await openPicker.PickSingleFileAsync();
                            if (file != null)
                            {
                                path.Text = file.Path;
                            }
                        };
                        OpenFileAsync();
                    };

                        stack.Children.Add(FileName);
                        stack.Children.Add(PluginName);
                        stack.Children.Add(version);
                        stack.Children.Add(author);
                        stack.Children.Add(description);
                        stack.Children.Add(dependencies);
                        stack.Children.Add(path);
                        stack.Children.Add(file);
                        ScrollViewer scrollViewer = new ScrollViewer
                        {
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                            Content = stack
                        };

                        ContentDialog dialog = new ContentDialog
                        {
                                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                                Title = "上传插件",
                                PrimaryButtonText = "上传",
                                CloseButtonText = "取消",
                                DefaultButton = ContentDialogButton.Primary,
                                Content = scrollViewer,
                                XamlRoot = this.XamlRoot
                        };
                        dialog.PrimaryButtonClick += (s, e) =>
                        {
                           async Task sendRequest()
                            {
                                if (string.IsNullOrEmpty(path.Text) || string.IsNullOrEmpty(FileName.Text) || string.IsNullOrEmpty(PluginName.Text) || string.IsNullOrEmpty(version.Text) || string.IsNullOrEmpty(author.Text) || string.IsNullOrEmpty(description.Text) || string.IsNullOrEmpty(dependencies.Text))
                                {
                                    e.Cancel = true;
                                    return;
                                }
                                using (HttpClient client = new HttpClient())
                                {
                                    byte[] fileBytes = File.ReadAllBytes(path.Text);
                                    string base64FileContent = Convert.ToBase64String(fileBytes);

                                    var jsonPayload = new
                                    {
                                        filename = FileName.Text,
                                        pluginname = PluginName.Text,
                                        description = description.Text,
                                        author = author.Text,
                                        version = version.Text,
                                        dependencies = dependencies.Text,
                                        filecontent = base64FileContent
                                    };

                                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonPayload);

                                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                                    var response = await client.PostAsync(upload, content);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        this.dialog.show("成功", "成功上传文件", "好的", null, null, this.XamlRoot);
                                    }
                                    else
                                    {
                                        this.dialog.show("寄", "上传文件失败", "好的", null, null, this.XamlRoot);
                                    }
                                }
                                e.Cancel = false;
                            }
                            sendRequest();
                        };

                        dialog.ShowAsync();
                };

                s.Children.Add(refresh);
                s.Children.Add(uploadBtn);
                stackPanel.Children.Add(s);
                MainGrid.Children.Add(stackPanel);
                FillGridWithPlugins(stackPanel, json);
                return;
            }
            MainGrid.Children.Add(block);

        }

        private void FillGridWithPlugins(StackPanel stack, string json)
        {
            var pluginList = JsonConvert.DeserializeObject<PluginList>(json);

            if (pluginList == null || pluginList.Plugins == null)
            {
                throw new Exception("Invalid JSON data.");
            }
            int itemsPerRow = 6;
            int columnCount = 0;
            StackPanel currentStackPanel = null;

            foreach (var plugin in pluginList.Plugins)
            {
                if (columnCount % itemsPerRow == 0)
                {
                    currentStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    stack.Children.Add(currentStackPanel);
                }

                Border border = CreatePluginBorder(plugin);
                currentStackPanel.Children.Add(border);
                columnCount++;
            }
        }

        private Border CreatePluginBorder(PluginInfo plugin)
        {
            Border border = new Border
            {
                Background = Application.Current.Resources["CardBackgroundFillColorDefault"] as SolidColorBrush,
                CornerRadius = new CornerRadius(5),
                MaxWidth = 300,
                MaxHeight = 170,
                Margin = new Thickness(5)
            };

            ScrollViewer scrollViewer = new ScrollViewer();
            Grid grid = new Grid { Margin = new Thickness(5) };

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = plugin.PluginName, FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = "版本: " + plugin.version, TextWrapping = TextWrapping.Wrap });
            stackPanel.Children.Add(new TextBlock { Text = "描述: " + plugin.Description, TextWrapping = TextWrapping.Wrap });
            stackPanel.Children.Add(new TextBlock { Text = " " });
            stackPanel.Children.Add(new TextBlock { Text = "作者: " + plugin.Author });
            stackPanel.Children.Add(new TextBlock { Text = "依赖: " + plugin.Dependencies, TextWrapping = TextWrapping.Wrap });

            Grid.SetRow(stackPanel, 0);
            grid.Children.Add(stackPanel);

            Grid bottomGrid = new Grid();
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            ComboBox comboBox = new ComboBox { Header = "下载到" };
            foreach (var v in Bots) comboBox.Items.Add(v);
            Grid.SetColumn(comboBox, 0);
            bottomGrid.Children.Add(comboBox);

            Button downloadButton = new Button { Content = "下载", Style = (Style)Application.Current.Resources["AccentButtonStyle"], HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10, 25, 0, 0) };
            Grid.SetColumn(downloadButton, 1);
            bottomGrid.Children.Add(downloadButton);
            downloadButton.Tag = plugin.FileName;
            downloadButton.Click += DownloadButton_Click;

            Grid.SetRow(bottomGrid, 1);
            grid.Children.Add(bottomGrid);

            scrollViewer.Content = grid;
            border.Child = scrollViewer;

            return border;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.OriginalSource as Button;
            string fn = button.Tag as string;
            if (string.IsNullOrEmpty(fn)) return;
            if (button != null)
            {
                Grid parentGrid = VisualTreeHelper.GetParent(button) as Grid;

                int comboBoxColumn = 0;

                Grid parentRowGrid = parentGrid;
                for (int i = 0; i < parentRowGrid.ColumnDefinitions.Count; i++)
                {
                    if (parentRowGrid.Children[i] is ComboBox comboBox)
                    {
                        if (comboBox.SelectedItem == null)
                        {
                            dialog.show("缺少参数", "请选择安装到哪个机器人", "好的", null, null, this.XamlRoot);
                            return;
                        }
                        else
                        {
                            DoDownload(comboBox.SelectedItem.ToString(), fn);
                        }
                        break;
                    }
                }
            }
        }

        private async Task DoDownload(string QN, string fn)
        {
            using (HttpClient client = new HttpClient())
            {

                var a = new
                {
                    filename = fn
                };

                string b = JsonConvert.SerializeObject(a);

                var c = new StringContent(b, Encoding.UTF8, "application/json");

                var d = await client.PostAsync(download, c);

                if (d.IsSuccessStatusCode)
                {
                    string responseContent = await d.Content.ReadAsStringAsync();
                    try
                    {
                        JObject e = JObject.Parse(responseContent);
                        string data = (string)e["data"];
                        string root = Path.Combine(settingManager.GetValue<string>("QQBotPath"), $"Bots\\{QN}\\plugins");
                        string path = Path.Combine(root, $"{fn}.dll");

                        if (!Path.Exists(root)) Directory.CreateDirectory(root);
                        if (File.Exists(path))
                        {
                            dialog.show("错误", $"文件{path}已存在", "好的", null, null, this.XamlRoot);
                            return;
                        }
                        if (string.IsNullOrEmpty(data))
                        {
                            dialog.show("错误", $"从服务器获取到的数据为空,请重试", "好的", null, null, this.XamlRoot);
                            return;
                        }
                        using (var streamWriter = new FileStream(path, FileMode.Create))
                        {
                            byte[] _data = Convert.FromBase64String(data);
                            streamWriter.Write(_data, 0, _data.Length);
                        }
                        dialog.show("完成", $"成功完成安装", "好的", null, null, this.XamlRoot);
                    }
                    catch (JsonException ex)
                    {
                        dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                        return;
                    }
                    catch (Exception ex)
                    {
                        dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                        return;
                    }
                }
                else
                {
                    dialog.show("错误", "Failed to download file", "好的", null, null, this.XamlRoot);
                }
            }
        }
    }

    public class PluginInfo
    {
        public string FileName { get; set; }
        public string PluginName { get; set; }
        public string version { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Dependencies { get; set; }
    }

    public class PluginList
    {
        public List<PluginInfo> Plugins { get; set; }
    }

    public class DownloadFileInfo
    {
        public string path { get; set; }
        public string FileName { get; set; }
    }
}