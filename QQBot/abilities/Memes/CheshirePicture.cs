using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using static PluginDLL.Class1;

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
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/随机柴郡 - 随机返回一张柴郡表情包");
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
            if (!message.StartsWith("/随机柴郡"))
            {
                return;
            }
            await bot.Message.sendImageMessage(e.GroupId, e.MessageId, url);

        }
    }
}
