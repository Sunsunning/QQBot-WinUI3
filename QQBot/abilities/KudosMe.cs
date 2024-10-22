using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class KudosMe : IEventHandler
    {
        private Bot bot;
        public KudosMe()
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
            if (!message.StartsWith("/赞我"))
            {
                return;
            }
            string response = await bot.Message.sendLike(e.Message.Sender.UserId, 10);
            HttpResponse errorResponse = JsonConvert.DeserializeObject<HttpResponse>(response);
            if (!string.IsNullOrEmpty(errorResponse.Message) && errorResponse.Message.StartsWith("Error"))
            {
                await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, errorResponse.Message.Replace("Error: ", ""));
                return;
            }
            await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "成功为你点赞十次~");
            await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "火速回赞");
        }
    }
}
