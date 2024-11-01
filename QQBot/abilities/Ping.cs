using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using static PluginDLL.Class1;

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
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/ping - Ping似Bot");
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
            if (!message.StartsWith("/ping"))
            {
                return;
            }

            await bot.Message.sendMessage(e.GroupId, e.MessageId, "爷在线!");
        }
    }
}
