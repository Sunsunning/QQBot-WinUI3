using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.QQBot.abilities;
using QQBotCodePlugin.QQBot.abilities.Memes;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.utils;
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
            List<IEventHandler> eventHandlers = new List<IEventHandler>();
            foreach (var key in info.EventData)
            {
                if (key.Key == "ba" && key.Value == true)
                {
                    eventHandlers.Add(new ba());
                }
                if (key.Key == "cat" && key.Value == true)
                {
                    eventHandlers.Add(new CheshirePicture());
                }
                if (key.Key == "dragon" && key.Value == true)
                {
                    eventHandlers.Add(new DragonPicture());
                }
                if (key.Key == "eat" && key.Value == true)
                {
                    eventHandlers.Add(new eat());
                }
                if (key.Key == "play" && key.Value == true)
                {
                    eventHandlers.Add(new play());
                }
                if (key.Key == "AIChat" && key.Value == true)
                {
                    eventHandlers.Add(new AIChat());
                }
                if (key.Key == "Help" && key.Value == true)
                {
                    eventHandlers.Add(new Help());
                }
                if (key.Key == "KudosMe" && key.Value == true)
                {
                    eventHandlers.Add(new KudosMe());
                }
                if (key.Key == "NumberBoom" && key.Value == true)
                {
                    eventHandlers.Add(new NumberBoom());
                }
                if (key.Key == "onset" && key.Value == true)
                {
                    eventHandlers.Add(new onset());
                }
                if (key.Key == "Ping" && key.Value == true)
                {
                    eventHandlers.Add(new abilities.Ping());
                }
                if (key.Key == "RunWindowsCommand" && key.Value == true)
                {
                    eventHandlers.Add(new RunWindowsCommand());
                }

                if (key.Key == "Sky" && key.Value == true)
                {
                    eventHandlers.Add(new SearchSky());
                }

                if (key.Key == "voice" && key.Value == true)
                {
                    eventHandlers.Add(new voice());
                }

                if (key.Key == "wife" && key.Value == true)
                {
                    eventHandlers.Add(new wife());
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
    }
}
