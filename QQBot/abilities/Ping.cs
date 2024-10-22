using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class Ping : IEventHandler
    {
        private Bot bot;
        public Ping()
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
            if (!message.StartsWith("/ping"))
            {
                return;
            }

            await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "爷在线!");
        }
    }
}
