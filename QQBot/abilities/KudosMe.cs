using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using System.Collections.Generic;
using static PluginDLL.Class1;

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
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/赞我 - 赞赞你的");
            bot.helpCommandHelper.addCommands("普通功能", description);
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, MessageEvent e)
        {
            string message = e.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!message.StartsWith("/赞我"))
            {
                return;
            }
            string response = await bot.Message.sendLike(e.Sender.UserId, 10);
            HttpResponse errorResponse = JsonConvert.DeserializeObject<HttpResponse>(response);
            if (!string.IsNullOrEmpty(errorResponse.Message) && errorResponse.Message.StartsWith("Error"))
            {
                await bot.Message.sendMessage(e.GroupId, e.MessageId, errorResponse.Message.Replace("Error: ", ""));
                return;
            }
            await bot.Message.sendMessage(e.GroupId, e.MessageId, "成功为你点赞十次~");
            await bot.Message.sendMessage(e.GroupId, e.MessageId, "火速回赞");
        }
    }
}
