using QQBotCodePlugin.QQBot.utils.AI;
using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.abilities.AI
{
    public class AIChatPrivate : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.chatanywhere.tech/v1/chat/completions";
        private ChatService chatService;
        private string model;
        private List<string> modelList = new List<string>();
        public AIChatPrivate()
        {
            model = "gpt-4o-mini";
            string[] models = {
                "gpt-4o-mini",
                "gpt-3.5-turbo-0125",
                "gpt-3.5-turbo-1106",
                "gpt-3.5-turbo"
            };
            modelList.AddRange(models);
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            this.bot.PrivateReceived += OnMessageReceived;
            chatService = new ChatService(url, bot.getConsole(), bot);
            bot.getLogger().Info($"{ToString()}(私聊)注册完成");
        }

        private async void OnMessageReceived(object sender, MessageEvent e)
        {
            string message = e.Messages[0].Data.Text;
            try
            {
                if (message == null)
                {
                    return;
                }
                long id = e.Sender.UserId;
                if (message.StartsWith("/model"))
                {
                    string[] parts = message.Split(" ");
                    if (parts.Length != 2)
                    {
                        await bot.Message.sendPrivateMessage(id, "用法:/model [模型]\n模型有:gpt-4o-mini、gpt-3.5-turbo-0125、gpt-3.5-turbo-1106、gpt-3.5-turbo");
                        return;
                    }
                    if (parts[1].Equals("current"))
                    {
                        await bot.Message.sendPrivateMessage(id, $"当前模型为:{model}");
                        return;
                    }
                    if (!modelList.Contains(parts[1]))
                    {
                        await bot.Message.sendPrivateMessage(id, "用法:/model [模型]\n模型列表:gpt-4o-mini、gpt-3.5-turbo-0125、gpt-3.5-turbo-1106、gpt-3.5-turbo");
                        return;
                    }
                    model = parts[1];
                    chatService.ClearHistory(id);
                    await bot.Message.sendPrivateMessage(id, $"切换完成,当前模型为:{model}");
                }

                if (message.Equals("#清空历史对话"))
                {
                    chatService.ClearHistory(id);
                    await bot.Message.sendPrivateMessage(id, "清理完成");
                    return;
                }

                string answer = await Chat(model, id, "user", message);

                await bot.Message.sendPrivateMessage(id, answer);


            }
            catch (NullReferenceException ex)
            {
                bot.getLogger().Warning($"发生错误:{ex.Message}");
                return;
            }
        }

        private async Task<string> Chat(string model, long id, string role, string question)
        {

            return await chatService.PostChatDataAsync(model, id, role, question);
        }
    }
}
