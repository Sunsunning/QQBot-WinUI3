using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities.Memes
{
    public class ba : IEventHandler
    {
        private Bot bot;

        public ba()
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
            if (@event.Message.Messages[0].Type.Equals("text"))
            {
                message = @event.Message.Messages[0].Data.Text;
            }
            else
            {
                return;
            }
            if (!message.StartsWith("/ba"))
            {
                return;
            }
            string[] parts = message.Split(' ');
            if (parts.Length != 3)
            {
                await bot.Message.sendImageMessage(@event.Message.GroupId, @event.Message.MessageId, "用法:/ba [左侧字符] [右侧字符]", sendToConsole: false);
                return;
            }
            await bot.Message.sendImageMessage(@event.Message.GroupId, @event.Message.MessageId, $"https://meme.yuki.sh/api/bluearchive?left={parts[1]}&right={parts[2]}", sendToConsole: false);
        }
    }
}
