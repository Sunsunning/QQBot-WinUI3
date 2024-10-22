using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class voice : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.lolimi.cn/API/yyhc/api.php";
        private readonly string pattern = @"^/说\s+(?<角色>[^\s]+)\s+(?<台词>.+)$";
        private HttpClient httpClient;
        private IHttpService httpService;
        private List<String> charcacters = new List<string>();
        public voice()
        {
            httpClient = new HttpClient();
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += OnMessageReceived;
            httpService = new HttpService(httpClient,bot.getConsole());
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }
        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!message.StartsWith("/说"))
            {
                return;
            }
            if (charcacters.Count == 0)
            {
                charcacters = await getCharcacters();
            }

            Match match = Regex.Match(message, pattern);

            if (match.Success)
            {
                string role = match.Groups["角色"].Value;
                string line = match.Groups["台词"].Value;

                if (!charcacters.Contains(role))
                {
                    await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "角色未找到\n输入 /说 查看角色列表");
                    return;
                }
                string mp3 = await getMP3Url(role, line);
                bot.getLogger().Debug(mp3);
                string request = await bot.Message.sendVoiceMessage(e.Message.GroupId, mp3);
                if (request.Contains("语音转换失败, 请检查语音文件是否正常"))
                {
                    await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "语音API出现错误请稍后再试");
                }
            }
            else
            {
                string charactersString = string.Join(", ", await getCharcacters());
                if (charactersString == null)
                {
                    await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "无法获取 characters 数组");
                    return;
                }
                await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, charactersString + "\n\n用法: /说 角色 台词");
            }
        }

        private async Task<string> getMP3Url(string role, string line)
        {
            string _url = url + $"?msg={line}&sp={role}";
            bot.getLogger().Debug(_url);
            string response = await httpService.SendGetRequestAsync(_url, false);
            VoiceApiResponse a = JsonConvert.DeserializeObject<VoiceApiResponse>(response);
            if (a != null && a.mp3 != null)
            {
                return a.mp3;
            }
            else
            {
                bot.getLogger().Warning("无法获取 mp3 地址");
            }
            return null;
        }

        private async Task<List<string>> getCharcacters()
        {

            string response = await httpService.SendGetRequestAsync(url, false);
            VoiceApiResponse data = JsonConvert.DeserializeObject<VoiceApiResponse>(response);
            if (data != null && data.Characters != null)
            {
                return data.Characters;
            }
            else
            {
                bot.getLogger().Warning("无法获取 characters 数组");
            }
            return null;

        }
    }
}
