using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using static PluginDLL.Class1;

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
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/光遇活动 - 获取每天的光遇活动日历");
            description.Add("/光遇天气 - 获取每日的光遇天气");
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
            if (!message.StartsWith("/光遇"))
            {
                return;
            }

            if (message.Equals("/光遇活动"))
            {
                await bot.Message.sendImageMessage(e.GroupId, e.MessageId, activities_url);
                return;
            }
            await bot.Message.sendImageMessage(e.GroupId, e.MessageId, weather_url);

        }
    }
}
