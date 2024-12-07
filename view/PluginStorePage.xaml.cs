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
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Application = Microsoft.UI.Xaml.Application;
using Exception = System.Exception;
using Path = System.IO.Path;

namespace QQBotCodePlugin.view
{
    public sealed partial class PluginStorePage : Page
    {
        private readonly SettingManager _settingsManager;
        private readonly StackPanel _pluginStackPanel;
        private readonly List<string> _botNames;
        private readonly Dialog _dialog;
        private readonly string _getPluginsUrl;
        private readonly string _uploadPluginUrl;
        private readonly string _downloadPluginUrl;
        private readonly string _getTokenFileUrl;
        private readonly string _updatePluginUrl;

        public PluginStorePage()
        {
            InitializeComponent();
            _dialog = new Dialog();
            _pluginStackPanel = new StackPanel() { Spacing = 10 };
            _settingsManager = new SettingManager();
            string httpBaseAddress = _settingsManager.GetValue<string>("HTTP");
            _getPluginsUrl = $"{httpBaseAddress}get_plugins";
            _uploadPluginUrl = $"{httpBaseAddress}upload";
            _downloadPluginUrl = $"{httpBaseAddress}download";
            _getTokenFileUrl = $"{httpBaseAddress}get_file_by_token";
            _updatePluginUrl = $"{httpBaseAddress}update";
            _botNames = GetBotNames();
            InitializePageAsync();
            TransitionCollection transitions = new TransitionCollection();
            EntranceThemeTransition entranceThemeTransition = new EntranceThemeTransition()
            {
                IsStaggeringEnabled = true
            };
            transitions.Add(entranceThemeTransition);
            _pluginStackPanel.ChildrenTransitions = transitions;
        }

        private List<string> GetBotNames()
        {
            List<string> botNames = new List<string>();
            string botsPath = Path.Combine(_settingsManager.GetValue<string>("QQBotPath"), "Bots");
            foreach (var directory in Directory.GetDirectories(botsPath))
            {
                botNames.Add(Path.GetFileName(directory));
            }
            return botNames;
        }

        private async Task InitializePageAsync()
        {
            TextBlock statusBlock = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, FontSize = 20, FontWeight = FontWeights.Bold };
            var (responseText, success) = await GetResponseAsync(_getPluginsUrl);
            if (!string.IsNullOrEmpty(responseText) && !success)
            {
                statusBlock.Text = responseText;
            }
            else if (string.IsNullOrEmpty(responseText))
            {
                statusBlock.Text = "null";
            }
            if (!string.IsNullOrEmpty(statusBlock.Text))
            {
                MainGrid.Children.Clear();
                MainGrid.Children.Add(statusBlock);
                return;
            }
            MainGrid.Children.Clear();
            StackPanel buttonPanel = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 5 };
            Button refreshButton = new Button() { Content = "刷新" };
            Button uploadButton = new Button() { Content = "上传插件" };
            Button updateButton = new Button() { Content = "更新插件" };
            Button tokenButton = new Button() { Content = "查看令牌" };
            refreshButton.Click += (s, e) => RefreshPage();
            updateButton.Click += UpdateButton_Click;
            tokenButton.Click += (s, e) =>
            {
                Dictionary<string, string> tokens = LoadTokensFromFile();
                List<string> tokenStrings = new List<string>();
                if (tokens.Count == 0)
                {
                    _dialog.show("你的Tokens(本地文件加载)", "null",null,null, "关闭", this.XamlRoot);
                    return;
                }
                foreach (var token in tokens)
                {
                    tokenStrings.Add($"{token.Key} = {token.Value}");
                }
                _dialog.show("你的Tokens(本地文件加载)", string.Join("\n", tokenStrings),null,null, "关闭", this.XamlRoot);
            };
            uploadButton.Click += UploadButton_Click;

            buttonPanel.Children.Add(refreshButton);
            buttonPanel.Children.Add(uploadButton);
            buttonPanel.Children.Add(updateButton);
            buttonPanel.Children.Add(tokenButton);
            _pluginStackPanel.Children.Add(buttonPanel);
            MainGrid.Children.Add(_pluginStackPanel);
            await FillGridWithPluginsAsync(responseText);
        }

        private async Task FillGridWithPluginsAsync(string json)
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
                            _pluginStackPanel.Children.Add(currentStackPanel);
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

        private async Task<(string, bool)> GetResponseAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var cancellationTokenSource = new CancellationTokenSource())
                {
                    cancellationTokenSource.CancelAfter(10000);
                    try
                    {
                        HttpResponseMessage response = await client.GetAsync(url, cancellationTokenSource.Token);
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
                        return ($"Request to {url} timed out after 10000 milliseconds.", false);
                    }
                    catch (Exception ex)
                    {
                        App.GetAppLogger().Log($"Error sending request: {ex.Message}", true);
                        return ($"Error sending request: {ex.Message}", false);
                    }
                }
            }
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = null;
            StackPanel mainPanel = new StackPanel();

            StackPanel inputPanel = new StackPanel();
            TextBlock errorTextBlock = new TextBlock() { Visibility = Visibility.Collapsed };
            TextBox tokenTextBox = new TextBox() { Header = "输入你的Token" };
            TextBlock fileNameTextBlock = new TextBlock();
            Button searchButton = new Button() { Content = "查询此Token对应的文件", HorizontalAlignment = HorizontalAlignment.Stretch };
            Button fileButton = new Button() { Content = "选择文件", HorizontalAlignment = HorizontalAlignment.Stretch };
            inputPanel.Children.Add(errorTextBlock);
            inputPanel.Children.Add(fileNameTextBlock);
            inputPanel.Children.Add(tokenTextBox);
            inputPanel.Children.Add(searchButton);
            inputPanel.Children.Add(fileButton);

            StackPanel loadingPanel = new StackPanel() { Visibility = Visibility.Collapsed };
            ProgressRing progressRing = new ProgressRing() { IsActive = true };
            TextBlock loadingTextBlock = new TextBlock() { Text = "正在查询中……" };
            loadingPanel.Children.Add(progressRing);
            loadingPanel.Children.Add(loadingTextBlock);

            mainPanel.Children.Add(inputPanel);
            mainPanel.Children.Add(loadingPanel);

            searchButton.Click += async (s, e) =>
            {
                inputPanel.Visibility = Visibility.Collapsed;
                loadingPanel.Visibility = Visibility.Visible;

                if (string.IsNullOrEmpty(tokenTextBox.Text))
                {
                    errorTextBlock.Text = "Token值未填写!";
                    inputPanel.Visibility = Visibility.Visible;
                    loadingPanel.Visibility = Visibility.Collapsed;
                    return;
                }
                using (HttpClient client = new HttpClient())
                {
                    var jsonPayload = new
                    {
                        token = tokenTextBox.Text
                    };

                    string json = JsonConvert.SerializeObject(jsonPayload);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(_getTokenFileUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        fileNameTextBlock.Text = $"查询到的文件:{await response.Content.ReadAsStringAsync()}";
                    }
                    else
                    {
                        errorTextBlock.Text = "无法获取Token对应的文件";
                        errorTextBlock.Visibility = Visibility.Visible;
                    }
                    inputPanel.Visibility = Visibility.Visible;
                    loadingPanel.Visibility = Visibility.Collapsed;
                }
            };

            fileButton.Click += async (s, e) =>
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

            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = mainPanel
            };

            ContentDialog updateDialog = new ContentDialog
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "更新插件",
                PrimaryButtonText = "上传",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = scrollViewer,
                XamlRoot = this.XamlRoot
            };

            updateDialog.PrimaryButtonClick += async (s, e) =>
            {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileNameTextBlock.Text))
            {
                errorTextBlock.Text = "请填写完所有值";
                errorTextBlock.Visibility = Visibility.Visible;
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
                        filename = fileNameTextBlock.Text
                    };

                    string json = JsonConvert.SerializeObject(jsonPayload);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(_updatePluginUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _dialog.show("成功", "成功更新插件！", "好的", null, null, this.XamlRoot);
                    }
                    else
                    {
                        _dialog.show("错误", "上传文件失败", "好的", null, null, this.XamlRoot);
                    }
                }
                e.Cancel = false;
            };

            await updateDialog.ShowAsync();
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel inputPanel = new StackPanel();
            TextBox fileNameTextBox = new TextBox() { Header = "文件名" };
            TextBox pluginNameTextBox = new TextBox() { Header = "插件名" };
            TextBox versionTextBox = new TextBox() { Header = "版本号" };
            TextBox authorTextBox = new TextBox() { Header = "作者" };
            TextBox descriptionTextBox = new TextBox() { Header = "描述" };
            TextBox dependenciesTextBox = new TextBox() { Header = "依赖" };
            TextBox pathTextBox = new TextBox() { Header = "路径", IsReadOnly = true };
            Button fileButton = new Button() { Content = "选择文件", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 5, 0, 0) };
            fileButton.Click += async (s, e) =>
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
                    pathTextBox.Text = file.Path;
                }
            };

            inputPanel.Children.Add(fileNameTextBox);
            inputPanel.Children.Add(pluginNameTextBox);
            inputPanel.Children.Add(versionTextBox);
            inputPanel.Children.Add(authorTextBox);
            inputPanel.Children.Add(descriptionTextBox);
            inputPanel.Children.Add(dependenciesTextBox);
            inputPanel.Children.Add(pathTextBox);
            inputPanel.Children.Add(fileButton);
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = inputPanel
            };

            ContentDialog uploadDialog = new ContentDialog
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "上传插件",
                PrimaryButtonText = "上传",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary,
                Content = scrollViewer,
                XamlRoot = this.XamlRoot
            };
            uploadDialog.PrimaryButtonClick += async (s, e) =>
            {
                if (string.IsNullOrEmpty(pathTextBox.Text) || string.IsNullOrEmpty(fileNameTextBox.Text) || string.IsNullOrEmpty(pluginNameTextBox.Text) || string.IsNullOrEmpty(versionTextBox.Text) || string.IsNullOrEmpty(authorTextBox.Text) || string.IsNullOrEmpty(descriptionTextBox.Text) || string.IsNullOrEmpty(dependenciesTextBox.Text))
                {
                    e.Cancel = true;
                    return;
                }
                using (HttpClient client = new HttpClient())
                {
                    byte[] fileBytes = File.ReadAllBytes(pathTextBox.Text);
                    string base64FileContent = Convert.ToBase64String(fileBytes);
                    string token = GenerateToken();
                    var jsonPayload = new
                    {
                        filename = fileNameTextBox.Text,
                        pluginname = pluginNameTextBox.Text,
                        description = descriptionTextBox.Text,
                        author = authorTextBox.Text,
                        version = versionTextBox.Text,
                        dependencies = dependenciesTextBox.Text,
                        filecontent = base64FileContent,
                        token = token
                    };

                    string json = JsonConvert.SerializeObject(jsonPayload);

                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(_uploadPluginUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        SaveTokenToFile(pluginNameTextBox.Text, token);
                        _dialog.show("成功", $"成功上传文件\n更新令牌:{token}\n请保存好此Token如丢失可以找1686388268找回", "好的", null, null, this.XamlRoot);
                        RefreshPage();
                    }
                    else
                    {
                        _dialog.show("错误", "上传文件失败", "好的", null, null, this.XamlRoot);
                    }
                }
                e.Cancel = false;
            };

            await uploadDialog.ShowAsync();
        }

        private void SaveTokenToFile(string pluginName, string token)
        {
            string root = _settingsManager.GetValue<string>("QQBotPath");
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
            string root = _settingsManager.GetValue<string>("QQBotPath");
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
            _pluginStackPanel.Children.Clear();
            InitializePageAsync();
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

            StackPanel pluginStackPanel = new StackPanel();
            pluginStackPanel.Children.Add(new TextBlock { Text = plugin.PluginName, FontWeight = FontWeights.Bold });
            pluginStackPanel.Children.Add(new TextBlock { Text = "版本: " + plugin.version, TextWrapping = TextWrapping.Wrap });
            ToolTipService.SetToolTip(border, $"描述: {plugin.Description}");
            pluginStackPanel.Children.Add(new TextBlock { Text = "作者: " + plugin.Author });
            pluginStackPanel.Children.Add(new TextBlock { Text = "依赖: " + plugin.Dependencies, TextWrapping = TextWrapping.Wrap });

            Grid.SetRow(pluginStackPanel, 0);
            grid.Children.Add(pluginStackPanel);

            Grid bottomGrid = new Grid();
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            ComboBox comboBox = new ComboBox { Header = "下载到" };
            foreach (var botName in _botNames) comboBox.Items.Add(botName);
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
                            _dialog.show("缺少参数", "请选择安装到哪个机器人", "好的", null, null, this.XamlRoot);
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

                HttpResponseMessage d = await client.PostAsync(_downloadPluginUrl, c);

                if (d.IsSuccessStatusCode)
                {
                    string responseContent = await d.Content.ReadAsStringAsync();
                    try
                    {
                        JObject e = JObject.Parse(responseContent);
                        string data = (string)e["data"];
                        string root = Path.Combine(_settingsManager.GetValue<string>("QQBotPath"), $"Bots\\{QN}\\plugins");
                        string path = Path.Combine(root, $"{fn}.dll");

                        if (!Path.Exists(root)) Directory.CreateDirectory(root);
                        if (File.Exists(path))
                        {
                            _dialog.show("错误", $"文件{path}已存在", "好的", null, null, this.XamlRoot);
                            return;
                        }
                        if (string.IsNullOrEmpty(data))
                        {
                            _dialog.show("错误", $"从服务器获取到的数据为空,请重试", "好的", null, null, this.XamlRoot);
                            return;
                        }
                        using (var streamWriter = new FileStream(path, FileMode.Create))
                        {
                            byte[] _data = Convert.FromBase64String(data);
                            streamWriter.Write(_data, 0, _data.Length);
                        }
                        _dialog.show("完成", $"成功完成安装", "好的", null, null, this.XamlRoot);
                    }
                    catch (JsonException ex)
                    {
                        App.GetAppLogger().Log($"Error: {ex.Message}", false);
                        _dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                        return;
                    }
                    catch (Exception ex)
                    {
                        App.GetAppLogger().Log($"Error downloading file: {ex.Message}", false);
                        _dialog.show("错误", $"{ex.Message}", "好的", null, null, this.XamlRoot);
                        return;
                    }
                }
                else
                {
                    _dialog.show("错误", "Failed to download file", "好的", null, null, this.XamlRoot);
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