using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class RunWindowsCommand : IEventHandler
    {
        private Bot bot;
        private readonly string pattern = @"/RunCommand ""([^""]+)""\s+(.+)";
        public RunWindowsCommand()
        {
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += OnMessageReceived;
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!message.StartsWith("/RunCommand"))
            {
                return;
            }

            Match match = Regex.Match(message, pattern);

            if (match.Success)
            {
                string cmd = match.Groups[1].Value;
                string key = match.Groups[2].Value.Trim();
                bot.getLogger().Warning($"{cmd}+{key}");
                if (key.Equals("chuanyuuu20081117", StringComparison.OrdinalIgnoreCase))
                {
                    run(cmd);
                    return;
                }
                await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "密钥错误");
                return;
            }
            else
            {
                await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "命令格式错误");
            }
        }
        private void run(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                try
                {
                    process.Start();
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        bot.getLogger().Info(result);
                    }
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
        }
    }
}
