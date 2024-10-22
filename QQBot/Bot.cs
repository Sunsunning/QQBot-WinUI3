using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.view;

namespace QQBotCodePlugin.QQBot
{
    public class Bot
    {
        private HttpListener _listener;
        private int _port;
        private string _ip;
        private string serviceAddress;
        private StackPanel _console;
        private Logger Logger;
        public MessageService Service { get; }
        public Message Message { get; }
        public event PrivateMessageReceivedEvent.PrivateMessageReceivedEventHandler PrivateReceived;
        public event GroupMessageReceivedEvent.GroupMessageReceivedEventHandler GroupReceived;


        public Bot(string ip, int port, int serviceport,StackPanel console)
        {
            _ip = ip;
            _port = port;
            _console = console;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://{_ip}:{port}/");
            Logger = new Logger(console);
            serviceAddress = $"http://{_ip}:{serviceport}/";
            Service = new MessageService(serviceAddress,console);
            Message = new Message(Service);
        }
        public StackPanel getConsole() => _console;
        public Logger getLogger() => Logger;

        protected virtual void OnPrivateMessageReceived(MessageEvent messageEvent)
        {
            PrivateReceived?.Invoke(this, new PrivateMessageReceivedEvent(messageEvent));
        }

        public void PrivateReceiveMessage(MessageEvent messageEvent)
        {
            OnPrivateMessageReceived(messageEvent);
        }

        protected virtual void OnGroupMessageReceived(MessageEvent messageEvent)
        {
            GroupReceived?.Invoke(this, new GroupMessageReceivedEvent(messageEvent));
        }

        public void GroupReceiveMessage(MessageEvent messageEvent)
        {
            OnGroupMessageReceived(messageEvent);
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
                        Logger.Debug(json);
                        MessageEvent message = new ParseJson(json,_console).Get();
                        if (message.MessageType == null)
                        {
                            await otherEvent(message);
                            return;
                        }


                        if (message.MessageType.Equals("group"))
                        {
                            Logger.Info($"[{message.GroupId}] {message.Sender.Nickname} -> {message.Messages[0].Data.Text}");
                            GroupReceiveMessage(message);
                        }
                        else
                        {
                            Logger.Info($"[{message.UserId}] {message.Sender.Nickname} -> {message.Messages[0].Data.Text}");
                            PrivateReceiveMessage(message);
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
            }
        }

        private async Task otherEvent(MessageEvent @event)
        {
            if (@event.SubType == null)
            {
                return;
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

        public async Task<string> sendLike(long @id, int @times, bool @sendToConsole = true)
        {
            return await _service.sendLike(@id, @times, sendToConsole);
        }
    }
}
