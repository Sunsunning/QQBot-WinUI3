using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.abilities.Memes
{
    public class DragonPicture : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.lolimi.cn/API/longt/l.php";
        public DragonPicture()
        {
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += OnMessageReceived;
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/龙图 - 随机给出一张龙图");
            bot.helpCommandHelper.addCommands("普通功能", description);
            bot.getLogger().Info($"{ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, MessageEvent e)
        {
            string message = e.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!message.StartsWith("/龙图"))
            {
                return;
            }

            await bot.Message.sendImageMessage(e.GroupId, e.MessageId, url);
        }
    }
}
