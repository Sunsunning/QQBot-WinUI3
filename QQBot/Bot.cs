using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.Plugin;
using QQBotCodePlugin.QQBot.abilities.utils;
using QQBotCodePlugin.QQBot.utils;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.utils;
using QQBotCodePlugin.view;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot
{
    public class Bot
    {
        private HttpListener _listener;
        private int _port;
        private string _ip;
        private RunBotInfo _info;
        private StackPanel _console;
        private Logger Logger;
        private PluginHost plugin;
        public string serviceAddress { get; }
        public bool loadPlugin { get; }
        private readonly string BotPath;
        public HelpCommandHelper helpCommandHelper { get; }
        public IConfigService configService { get; }
        public MessageService Service { get; }
        public Message Message { get; }
        private List<string> folders = new List<string> { "images", "logs", "config", "temp", "plugins" };
        private List<string> files = new List<string> { ".wife" };
        public event EventHandler<MessageEvent> PrivateReceived;
        public event EventHandler<MessageEvent> GroupReceived;
        public long ReceivedCount { get; private set; } = 0;
        public long ReceptionGroupMsgCount { get; private set; } = 0;
        public long ReceptionPrivateMsgCount { get; private set; } = 0;

        public Bot(RunBotInfo info, StackPanel console, bool loadPlugin = false)
        {
            helpCommandHelper = new HelpCommandHelper();
            this.loadPlugin = loadPlugin;
            _ip = info.IPAddress ;
            _port = info.ServerPort;
            _console = console;
            _info = info;
            BotPath = info.BotPath;
            Logger = new Logger(console, this);
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{_ip}:{_port}/");
            serviceAddress = $"http://{_ip}:{info.EventPort}/";
            Service = new MessageService(serviceAddress, console, this);
            Message = new Message(Service);
            configService = new ConfigService(getPlugins());
            Initialized();
            if (loadPlugin) LoadingPlugin();
        }

        public List<IPlugin> GetPluginsList() => plugin.GetPlugins();
        public StackPanel getConsole() => _console;
        public Logger getLogger() => Logger;
        public RunBotInfo GetRunBotInfo() => _info;
        public string getImage() => Path.Combine(BotPath, "images");
        public string getConfig() => Path.Combine(BotPath, "config");
        public string getLogs() => Path.Combine(BotPath, "logs");
        public string getTemp() => Path.Combine(BotPath, "temp");
        public string getPlugins() => Path.Combine(BotPath, "plugins");
        public string getGPTKey()
        {
            string json = File.ReadAllText(Path.Combine(BotPath, "bot.json"));
            return (string)JObject.Parse(json)["key"];
        }

        private void LoadingPlugin()
        {
            plugin = new PluginHost(Logger, this);
            plugin.LoadPlugins(getPlugins());
        }

        private void DeleteImage()
        {
            if (Directory.Exists(this.getImage()))
            {
                string[] files = Directory.GetFiles(this.getImage());
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }

        private void Initialized()
        {
            DeleteImage();
            foreach (var name in folders)
            {
                string path = Path.Combine(BotPath, name);
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                        Logger.Info($"正在创建{path}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"创建文件夹{path}失败:{ex.Message}");
                    }
                }
            }
            foreach (var name in files)
            {
                string path = Path.Combine(this.getConfig(), name);
                if (!File.Exists(path))
                {
                    try
                    {
                        using (FileStream fs = File.Create(path)) { }
                        Logger.Info($"正在创建{path}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"创建文件{path}失败:{ex.Message}");
                    }
                }
            }
        }

        public long AddReceivedCount()
        {
            return ReceivedCount++;
        }

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
                Logger.Info($"正在监听{_ip}:{_port}...");
                Logger.Info($"机器人启动成功");
                ConsolePage.Running = true;

                while (true)
                {
                    var context = await _listener.GetContextAsync();
                    await HandleRequestAsync(context);
                }
            }
            catch (HttpListenerException ex)
            {
                Logger.Error("发生错误:" + ex.Message);
            }
        }

        public void RegisterEventHandlers(IEnumerable<IEventHandler> eventHandlers)
        {
            foreach (var handler in eventHandlers)
            {
                handler.Register(this);
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                if (context.Request.HttpMethod == "POST" && context.Request.RawUrl == "/")
                {
                    using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        string json = await reader.ReadToEndAsync();
                        // Logger.Debug(json);
                        MessageEvent message = new ParseJson(json, _console, this).Get();

                        if (message.MessageType == null)
                        {
                            return;
                        }
                        if (message.MessageType.Equals("group"))
                        {
                            Logger.Info($"[{message.GroupId}] {message.Sender.Nickname}: {message.RawMessage}");
                            ReceptionGroupMsgCount++;
                            GroupReceived(this, message);
                        }
                        else
                        {
                            Logger.Info($"[{message.Sender.UserId}] {message.Sender.Nickname}: {message.RawMessage}");
                            ReceptionPrivateMsgCount++;
                            PrivateReceived(this, message);
                        }

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        context.Response.ContentType = "application/json";
                        byte[] buffer = Encoding.UTF8.GetBytes("{}");
                        context.Response.ContentLength64 = buffer.Length;
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.ToString());
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                context.Response.Close();
            }
        }

        public void Stop()
        {
            if (_listener == null)
            {
                Logger.Info("HTTP listener is already disposed.");
                return;
            }

            try
            {
                if (_listener.IsListening)
                {
                    _listener.Stop();
                    Logger.Info("HTTP listener stopped.");
                    ConsolePage.Running = true;
                }
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"HTTP listener has been disposed: {ex.Message}");
            }
            finally
            {
                _listener.Stop();
                _listener.Close();
                _listener = null;
                Logger.Info("HTTP listener resources released.");
                ConsolePage.Running = false;
                DeleteImage();
            }
        }

        public string getServiceAddress() => serviceAddress;
    }

    public class Message
    {
        private readonly MessageService _service;

        public Message(MessageService service)
        {
            _service = service;
        }

        public async Task<string> sendDirectMessage(long @id, string @message, bool @autoEscape = false, bool @sendToConsole = true)
        {
            return await _service.SendGroupDirectMessageAsync(@id, @message, @autoEscape, @sendToConsole);
        }

        public async Task<string> sendMessage(long @id, long @message_id, string @message, bool @autoEscape = false, bool @sendToConsole = true)
        {
            return await _service.SendGroupMessageAsync(@id, @message_id, @message, @autoEscape, @sendToConsole);
        }

        public async Task<string> sendImageMessage(long @id, long @message_id, string @url, string @summary = "喵喵喵", bool @sendToConsole = true, bool @useFile = false)
        {
            return await _service.SendGroupImageMessageAsync(id, message_id, url, summary, sendToConsole);
        }

        public async Task<string> sendVoiceMessage(long @id, string @url, bool @sendToConsole = true)
        {
            return await _service.SendGroupVoiceMessageAsync(id, url, sendToConsole);
        }

        public async Task<string> sendGroupMessageAndImage(long @id, long @message_id, string @text, string @url, long @at_userId, string @name, string @summary = "喵喵喵", bool @sendToConsole = true)
        {
            return await _service.SendGroupMessageAsync(@id, @message_id, text, url, at_userId, name, summary, sendToConsole);
        }
        public async Task<string> sendPrivateMessage(long @id, string @message, bool @autoEscape = false, bool @sendToConsole = true)
        {
            return await _service.SendPrivateMesageDirectMessageAsync(id, message, autoEscape, sendToConsole);
        }
        public async Task<string> sendLike(long @id, int @times, bool @sendToConsole = true)
        {
            return await _service.sendLike(@id, @times, sendToConsole);
        }
    }
}
