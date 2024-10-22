using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities.Memes
{
    public class CheshirePicture : IEventHandler
    {
        private Bot bot;
        private readonly string url = "http://api.yujn.cn/api/chaijun.php?";
        public CheshirePicture()
        {
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += OnMessageReceived;
            bot.getLogger().Info($"{ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!message.StartsWith("/随机柴郡"))
            {
                return;
            }
            await bot.Message.sendImageMessage(e.Message.GroupId, e.Message.MessageId, url);

        }
    }
}
