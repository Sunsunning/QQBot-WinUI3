using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.QQBot.abilities;
using QQBotCodePlugin.QQBot.abilities.AI;
using QQBotCodePlugin.QQBot.abilities.Memes;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot
{
    public class BotMain
    {
        private Logger logger;
        private readonly StackPanel _logStackPanel;
        private readonly RunBotInfo info;
        private Bot bot;

        public BotMain(StackPanel logStackPanel, RunBotInfo info)
        {
            _logStackPanel = logStackPanel;
            this.info = info;
        }


        public long getReceivedCount() => bot.ReceivedCount;
        public long getReceptionGroupMsgCount() => bot.ReceptionGroupMsgCount;
        public long getReceptionPrivateMsgCount() => bot.ReceptionPrivateMsgCount;

        public async void ReStart()
        {
            logger.Info($"正在重新启动机器人{info.BotName}");
            bot.Stop();
            await RunBot();

        }

        public void StopBot()
        {
            logger.Info("正在关闭");
            bot.Stop();
        }

        public async Task RunBot()
        {
            bot = new Bot(info.IPAddress, info.ServerPort, info.EventPort, info.BotPath, _logStackPanel, info.Plugin);
            logger = new Logger(_logStackPanel, bot);

            logger.Info($"正在启动{info.BotName} 在 {info.IPAddress}:{info.ServerPort}与{info.IPAddress}:{info.EventPort}");
            Dictionary<string, bool> eventHandlersConfig = info.EventData;
            List<IEventHandler> eventHandlers = new List<IEventHandler>();
            foreach (var handlerConfig in eventHandlersConfig)
            {
                if (handlerConfig.Value && eventHandlerTypes.ContainsKey(handlerConfig.Key))
                {
                    eventHandlers.Add((IEventHandler)Activator.CreateInstance(eventHandlerTypes[handlerConfig.Key]));
                }
            }
            bot.RegisterEventHandlers(eventHandlers);
            await bot.StartAsync();
        }

        public async void sendDirectMessage(long @id, string @message, bool @autoEscape = false, bool @sendToConsole = true)
        {
            if (bot == null)
            {
                logger.Error("BotMain实例不存在无法发送");
                return;
            }
            await bot.Message.sendDirectMessage(id, message, autoEscape, sendToConsole);
        }

        private Dictionary<string, Type> eventHandlerTypes = new Dictionary<string, Type>
        {
            { "ba", typeof(ba) },
            { "cat", typeof(CheshirePicture) },
            { "dragon", typeof(DragonPicture) },
            { "eat", typeof(eat) },
            { "play", typeof(play) },
            { "AIChat", typeof(AIChatGroup) }, // 注意：这里假设 AIChatGroup 是第一个需要添加的
            { "AIChatPrivate", typeof(AIChatPrivate) },
            { "Help", typeof(Help) },
            { "KudosMe", typeof(KudosMe) },
            { "NumberBoom", typeof(NumberBoom) },
            { "onset", typeof(onset) },
            { "Ping", typeof(abilities.Ping) },
            { "RunWindowsCommand", typeof(RunWindowsCommand) },
            { "Sky", typeof(SearchSky) },
            { "wife", typeof(wife) }
        };

    }
}
