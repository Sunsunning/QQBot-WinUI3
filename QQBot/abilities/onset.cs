using Newtonsoft.Json.Linq;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class onset : IEventHandler
    {
        private Bot bot;
        private string url = "https://api.lolimi.cn/API/fabing/fb.php?name=";
        private HttpClient client;
        private IHttpService httpService;
        public onset()
        {
            client = new HttpClient();
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += OnMessageReceived;
            httpService = new HttpService(client,bot.getConsole());
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            string[] parts = message.Split(' ');
            if (!message.StartsWith("/发病"))
            {
                return;
            }
            if (parts.Length != 2)
            {
                await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "使用方法:/发病 [名称]");
                return;
            }
            string text = await httpService.SendGetRequestAsync($"{url}{parts[1]}", false);
            JObject json = JObject.Parse(text);
            string onesetText = (string)json["data"];
            await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, onesetText);
        }
    }
}
