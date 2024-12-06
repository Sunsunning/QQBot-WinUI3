using ABI.Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Storage.Pickers;
using Application = Microsoft.UI.Xaml.Application;
using Exception = System.Exception;
using Path = System.IO.Path;

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
        private readonly string get_file_by_token;
        private readonly string update;


        public PluginStorePage()
        {
            this.InitializeComponent();
            dialog = new Dialog();
            stackPanel = new StackPanel() { Spacing = 10 };
            settingManager = new SettingManager();
            string http = settingManager.GetValue<string>("HTTP");
            get = $"{http}get_plugins";
            upload = $"{http}upload";
            download = $"{http}download";
            update = $"{http}update";
            get_file_by_token = $"{http}get_file_by_token";
            Bots = getBots();
            InitializePage();
            TransitionCollection transitions = new TransitionCollection();
            EntranceThemeTransition entranceThemeTransition = new EntranceThemeTransition()
            {
                IsStaggeringEnabled = true
            };
            transitions.Add(entranceThemeTransition);
            stackPanel.ChildrenTransitions = transitions;
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

        private async Task<(string, bool)> getResponse()
        {
            using (HttpClient client = new HttpClient())
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(10000);
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(get, cancellationTokenSource.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            return (responseContent, true);
                        }
                        else
                        {
                            return ($"Failed to get response. Status code: {response.StatusCode}", false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return ($"Request to {get} timed out after 20000 milliseconds.", false);
                    }
                    catch (Exception ex)
                    {
                        return ($"Error sending request: ({ex.Message}", false);
                    }
                }
            }
        }

        private async Task InitializePage()
        {
            TextBlock block = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 20, FontWeight = FontWeights.Bold };
            var (responseText, success) = await getResponse();
            if (!string.IsNullOrEmpty(responseText) && !success)
            {
                block.Text = responseText;
            }
            else if (string.IsNullOrEmpty(responseText))
            {
                block.Text = "null";
            }
            if (!string.IsNullOrEmpty(block.Text))
            {
                MainGrid.Children.Clear();
                MainGrid.Children.Add(block);
                return;
            }
            MainGrid.Children.Clear();
            StackPanel s = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 5 };
            Button refresh = new Button() { Content = "刷新" };
            Button uploadBtn = new Button() { Content = "上传插件" };
            Button updateBtn = new Button() { Content = "更新插件" };
            Button ltoken = new Button() { Content = "查看令牌" };
            refresh.Click += (s, e) =>
            {
                RefreshPage();
            };
            updateBtn.Click += UpdateBtn_Click;
            ltoken.Click += (s, e) =>
            {
                Dictionary<string,string> tokens = LoadTokensFromFile();
                List<string> strings = new List<string>();
                if (tokens.Count == 0)
                {
                    dialog.show("你的Tokens(本地文件加载)", "null", null, null, "关闭", this.XamlRoot);
                    return;
                }
                foreach (var token in tokens)
                {
                    strings.Add($"{token.Key} = {token.Value}");
                }
                dialog.show("你的Tokens(本地文件加载)", string.Join("\n", strings), null, null, "关闭", this.XamlRoot);
            };
            uploadBtn.Click += uploadButton_Click;

            s.Children.Add(refresh);
            s.Children.Add(uploadBtn);
            s.Children.Add(updateBtn);
            s.Children.Add(ltoken);
            stackPanel.Children.Add(s);
            MainGrid.Children.Add(stackPanel);
            FillGridWithPlugins(responseText);
            return;
        }

        private async void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            string? filePath = null;
            StackPanel mainPanel = new StackPanel();

            StackPanel stack = new StackPanel();
            TextBlock error = new TextBlock() {Visibility = Visibility.Collapsed};
            TextBox token = new TextBox() { Header = "输入你的Token" };
            TextBlock FileName = new TextBlock();
            Button search = new Button() { Content = "查询此Token对应的文件", HorizontalAlignment = HorizontalAlignment.Stretch };
            Button file = new Button() { Content = "选择文件", HorizontalAlignment = HorizontalAlignment.Stretch };
            stack.Children.Add(error);
            stack.Children.Add(FileName);
            stack.Children.Add(token);
            stack.Children.Add(search);
            stack.Children.Add(file);

            StackPanel loadPanel = new StackPanel() { Visibility = Visibility.Collapsed };
            ProgressRing progressRing = new ProgressRing() { IsActive = true };
            TextBlock loadingText = new TextBlock() { Text = "正在查询中……" };
            loadPanel.Children.Add (progressRing);
            loadPanel.Children.Add (loadingText);

            mainPanel.Children.Add(stack);
            mainPanel.Children.Add(loadPanel);

            search.Click +=(s,e)=>{
                async Task GetFileByToken()
                {
                    stack.Visibility = Visibility.Collapsed;
                    loadPanel.Visibility = Visibility.Visible;

                    if (string.IsNullOrEmpty(token.Text))
                    {
                        error.Text = "Token值未填写!";
                        error.Visibility = Visibility.Visible;
                        stack.Visibility = Visibility.Visible;
                        loadPanel.Visibility = Visibility.Collapsed;
                        return;
                    }
                    using (HttpClient client = new HttpClient())
                    {
                        var jsonPayload = new
                        {
                            token = token.Text
                        };

                        string json = JsonConvert.SerializeObject(jsonPayload);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(get_file_by_token, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            error.Text = "无法获取Token对应的文件";
                            error.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            FileName.Text = $"查询到的文件:{await response.Content.ReadAsStringAsync()}";
                        }
                        stack.Visibility = Visibility.Visible;
                        loadPanel.Visibility = Visibility.Collapsed;
                    }
                }
                GetFileByToken();
            };

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
                        filePath = file.Path;
                    }
                };
                OpenFileAsync();
            };

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = mainPanel
            };

            ContentDialog dialog = new ContentDialog
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "更新插件",
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
                    if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(FileName.Text))
                    {
                        error.Text = "请填写完所有值";
                        error.Visibility = Visibility.Visible;
                        e.Cancel = true;
                        return;
                    }
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] fileBytes = File.ReadAllBytes(filePath);
                        string base64FileContent = Convert.ToBase64String(fileBytes);
                        var jsonPayload = new
                        {
                            filecontent = fileBytes,
                            filename = FileName.Text
                        };

                        string json = JsonConvert.SerializeObject(jsonPayload);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(update, content);

                        if (response.IsSuccessStatusCode)
                        {
                            this.dialog.show("成功", "成功更新插件！", "好的", null,null,this.XamlRoot);
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

            await dialog.ShowAsync();

        }

        private async void uploadButton_Click(object sender,RoutedEventArgs args)
        {
            StackPanel stack = new StackPanel();
            TextBox FileName = new TextBox() { Header = "文件名" };
            TextBox PluginName = new TextBox() { Header = "插件名" };
            TextBox version = new TextBox() { Header = "版本号" };
            TextBox author = new TextBox() { Header = "作者" };
            TextBox description = new TextBox() { Header = "描述" };
            TextBox dependencies = new TextBox() { Header = "依赖" };
            TextBox path = new TextBox() { Header = "路径", IsReadOnly = true };
            Button file = new Button() { Content = "选择文件", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 5, 0, 0) };
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
                        string Token = GenerateToken();
                        var jsonPayload = new
                        {
                            filename = FileName.Text,
                            pluginname = PluginName.Text,
                            description = description.Text,
                            author = author.Text,
                            version = version.Text,
                            dependencies = dependencies.Text,
                            filecontent = base64FileContent,
                            token = Token
                        };

                        string json = JsonConvert.SerializeObject(jsonPayload);

                        var content = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(upload, content);

                        if (response.IsSuccessStatusCode)
                        {
                            SaveTokenToFile(PluginName.Text, Token);
                            this.dialog.show("成功", $"成功上传文件\n更新令牌:{Token}\n请保存好此Token如丢失可以找1686388268找回", "好的", null, null, this.XamlRoot);
                            RefreshPage();
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

            await dialog.ShowAsync();
        }

        private void SaveTokenToFile(string pluginName, string token)
        {
            string root = settingManager.GetValue<string>("QQBotPath");
            string tokenFileName = ".token";
            string tokenFilePath = Path.Combine(root, tokenFileName);

            if (string.IsNullOrEmpty(root))
            {
                App.GetAppLogger().Log($"Error: QQBotPath is not set.", false);
                throw new InvalidOperationException("QQBotPath is not set.");
            }

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            if (!File.Exists(tokenFilePath))
            {
                using (FileStream stream = new FileStream(tokenFilePath, FileMode.Create)) { }
            }

            string tokens = File.Exists(tokenFilePath) ? File.ReadAllText(tokenFilePath) : "{}";

            Dictionary<string, string> tokenDict;
            try
            {
                tokenDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokens);
            }
            catch (JsonException)
            {
                tokenDict = new Dictionary<string, string>();
            }

            if (tokenDict == null)
            {
                tokenDict = new Dictionary<string, string>();
            }

            tokenDict[pluginName] = token;

            string updatedTokens = JsonConvert.SerializeObject(tokenDict, Formatting.Indented);

            File.WriteAllText(tokenFilePath, updatedTokens);
        }

        private Dictionary<string, string> LoadTokensFromFile()
        {
            string root = settingManager.GetValue<string>("QQBotPath");
            string tokenFileName = ".token";
            string tokenFilePath = Path.Combine(root, tokenFileName);

            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }

            if (!File.Exists(tokenFilePath))
            {
                return new Dictionary<string, string>();
            }

            string tokens = File.ReadAllText(tokenFilePath);

            var tokenDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokens);

            return tokenDict ?? new Dictionary<string, string>();
        }

        private void RefreshPage()
        {
            stackPanel.Children.Clear();
            InitializePage();
        }

        private async void FillGridWithPlugins(string json)
        {
            await Task.Run(() =>
            {
                var pluginList = JsonConvert.DeserializeObject<PluginList>(json);

                if (pluginList == null || pluginList.Plugins == null)
                {
                    App.GetAppLogger().Log($"Error: Invalid JSON data.", false);
                    throw new Exception("Invalid JSON data.");
                }
                int itemsPerRow = 6;
                int columnCount = 0;
                StackPanel currentStackPanel = null;

                foreach (var plugin in pluginList.Plugins)
                {
                    if (columnCount % itemsPerRow == 0)
                    {
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            currentStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                            stackPanel.Children.Add(currentStackPanel);
                        });
                    }

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        Border border = CreatePluginBorder(plugin);
                        currentStackPanel.Children.Add(border);
                    });
                    columnCount++;
                }
            });
        }

        private Border CreatePluginBorder(PluginInfo plugin)
        {
            Border border = new Border
            {
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
            ToolTipService.SetToolTip(border, $"描述: {plugin.Description}");
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
                        App.GetAppLogger().Log($"Error: {ex.Message}", false);
                        dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                        return;
                    }
                    catch (Exception ex)
                    {
                        App.GetAppLogger().Log($"Error: {ex.Message}", false);
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

        private string GenerateToken()
        {
            return Guid.NewGuid().ToString();
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