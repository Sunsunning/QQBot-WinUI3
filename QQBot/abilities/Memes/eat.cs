using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.abilities.Memes
{
    public class eat : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.lolimi.cn/API/face_suck/?QQ=";

        public eat()
        {
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            this.bot.GroupReceived += Bot_GroupReceived;
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/咬 [@某人] - 猫猫虫咬似ta~");
            bot.helpCommandHelper.addCommands("普通功能", description);
            bot.getLogger().Info($"{ToString()}注册完成");
        }

        private async void Bot_GroupReceived(object sender, MessageEvent @event)
        {
            string message = null;
            string user_id = null;
            if (@event.Messages[0].Type.Equals("text"))
            {
                message = @event.Messages[0].Data.Text;
            }
            else
            {
                return;
            }

            if (!message.StartsWith("/咬"))
            {
                return;
            }

            if (@event.Messages.Count > 1 && @event.Messages[1].Type == "at")
            {
                user_id = @event.Messages[1].Data.qq;
            }
            if (string.IsNullOrEmpty(user_id))
            {
                await bot.Message.sendMessage(@event.GroupId, @event.MessageId, "用法:/咬 [@某人]");
                return;
            }
            await bot.Message.sendImageMessage(@event.GroupId, @event.MessageId, $"{url}{user_id}", sendToConsole: false);
        }
    }
}
