using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities.Memes
{
    public class play : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.lolimi.cn/API/face_play/?QQ=";

        public play()
        {
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            this.bot.GroupReceived += Bot_GroupReceived;
            bot.getLogger().Info($"{ToString()}注册完成");
        }

        private async void Bot_GroupReceived(object sender, utils.GroupMessageReceivedEvent @event)
        {
            string message = null;
            string? user_id = null;
            if (@event.Message.Messages[0].Type.Equals("text"))
            {
                message = @event.Message.Messages[0].Data.Text;
            }
            else
            {
                return;
            }
            if (!message.StartsWith("/玩"))
            {
                return;
            }

            if (@event.Message.Messages.Count > 1 && @event.Message.Messages[1].Type == "at")
            {
                user_id = @event.Message.Messages[1].Data.qq;
            }
            if (string.IsNullOrEmpty(user_id))
            {
                await bot.Message.sendMessage(@event.Message.GroupId, @event.Message.MessageId, "用法:/玩 [@某人]");
                return;
            }
            await bot.Message.sendImageMessage(@event.Message.GroupId, @event.Message.MessageId, $"{url}{user_id}", sendToConsole: false);
        }
    }
}
