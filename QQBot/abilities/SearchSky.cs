using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class SearchSky : IEventHandler
    {
        private Bot bot;
        private readonly string activities_url = "https://api.lolimi.cn/API/gy/ril.php";
        private readonly string weather_url = "https://api.lolimi.cn/API/gy/tq.php";
        public SearchSky()
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
            if (!message.StartsWith("/光遇"))
            {
                return;
            }

            if (message.Equals("/光遇活动"))
            {
                await bot.Message.sendImageMessage(e.Message.GroupId, e.Message.MessageId, activities_url);
                return;
            }
            await bot.Message.sendImageMessage(e.Message.GroupId, e.Message.MessageId, weather_url);

        }
    }
}
