using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.QQBot.abilities;
using QQBotCodePlugin.QQBot.abilities.Memes;
using QQBotCodePlugin.QQBot.utils;
using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QQBotCodePlugin.QQBot
{
    public class BotMain
    {
        private List<string> folders = new List<string> { "images", "logs", "config", "temp" };
        private List<string> files = new List<string> { ".wife" };
        private readonly Logger logger;
        private static string CurrentPath;
        private Bot bot;

        public BotMain(StackPanel logStackPanel,string QQBotPath) {
            logger = new Logger(logStackPanel);
            CurrentPath = QQBotPath;
            Initialized();
            // Task.Run(async () => await RunBot(logStackPanel, port, serviceport));
        }

        public void StopBot()
        {
            logger.Info("正在关闭");
            bot.Stop();
        }

        public async Task RunBot(StackPanel logStackPanel, int port, int serviceport)
        {
            Debug.WriteLine("RunBot");
            
            bot = new Bot("localhost", port, serviceport, logStackPanel);
            List<IEventHandler> eventHandlers = new List<IEventHandler>
                    {
                        new AIChat(),
                        new CheshirePicture(),
                        new DragonPicture(),
                        new Help(),
                        new KudosMe(),
                        new abilities.Ping(),
                        new RunWindowsCommand(),
                        new SearchSky(),
                        new voice(),
                        new wife(),
                        new onset(),
                        new eat(),
                        new play(),
                        new NumberBoom(),
                        new ba()
                    };
            bot.RegisterEventHandlers(eventHandlers);
            await bot.StartAsync();
        }

        private void Initialized()
        {

            foreach (var name in folders)
            {
                string path = Path.Combine(CurrentPath, name);
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                       logger.Info($"正在创建{path}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"创建文件夹{path}失败:{ex.Message}");
                    }
                }
            }
            foreach (var name in files)
            {
                string path = Path.Combine(Directories.ConfigPath, name);
                if (!File.Exists(path))
                {
                    try
                    {
                        File.Create(path);
                        logger.Info($"正在创建{path}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"创建文件{path}失败:{ex.Message}");
                    }
                }
            }
        }
        public static string getCurrentPath()
        {
            return CurrentPath;
        }
    }
}
