using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using static PluginDLL.Class1;

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
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/ba [左侧字符] [右侧字符] - 生成Ba标题");
            bot.helpCommandHelper.addCommands("普通功能", description);
            bot.getLogger().Info($"{ToString()}注册完成");
        }

        private async void Bot_GroupReceived(object sender, MessageEvent @event)
        {
            string message = null;
            if (@event.Messages[0].Type.Equals("text"))
            {
                message = @event.Messages[0].Data.Text;
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
                await bot.Message.sendMessage(@event.GroupId, @event.MessageId, "用法:/ba [左侧字符] [右侧字符]", sendToConsole: false);
                return;
            }
            await bot.Message.sendImageMessage(@event.GroupId, @event.MessageId, $"https://meme.yuki.sh/api/bluearchive?left={parts[1]}&right={parts[2]}", sendToConsole: false);
        }
    }
}
