using QQBotCodePlugin.QQBot.utils.AI;
using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.abilities.AI
{
    public class AIChatGroup : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.chatanywhere.tech/v1/chat/completions";
        private ChatService chatService;
        private string model;
        private List<string> modelList = new List<string>();
        public AIChatGroup()
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
            this.bot.GroupReceived += OnMessageReceived;
            chatService = new ChatService(url, bot.getConsole(), bot);
            List<string> description = bot.helpCommandHelper.getCommands("AI类");
            description.Add("/model - 切换模型,查看模型列表请输入/model查看");
            description.Add("/model current - 查看当前使用的模型");
            description.Add("使用 #+文本 来进行AI聊天");
            bot.helpCommandHelper.addCommands("AI类", description);
            bot.getLogger().Info($"{ToString()}注册完成");
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
                long id = e.GroupId;
                if (message.StartsWith("#"))
                {
                    if (message.Equals("#清空历史对话"))
                    {
                        chatService.ClearHistory(id);
                        await bot.Message.sendMessage(e.GroupId, e.MessageId, "清理完成");
                        return;
                    }

                    int hashIndex = message.IndexOf("#");
                    string answer = await Chat(model, id, "user", message.Substring(0, hashIndex) + message.Substring(hashIndex + 1));

                    await bot.Message.sendMessage(e.GroupId, e.MessageId, answer);

                }
                else if (message.StartsWith("/model"))
                {
                    string[] parts = message.Split(" ");
                    if (parts.Length != 2)
                    {
                        await bot.Message.sendMessage(e.GroupId, e.MessageId, "用法:/model [模型]\n模型有:gpt-4o-mini、gpt-3.5-turbo-0125、gpt-3.5-turbo-1106、gpt-3.5-turbo");
                        return;
                    }
                    if (parts[1].Equals("current"))
                    {
                        await bot.Message.sendMessage(e.GroupId, e.MessageId, $"当前模型为:{model}");
                        return;
                    }
                    if (!modelList.Contains(parts[1]))
                    {
                        await bot.Message.sendMessage(e.GroupId, e.MessageId, "用法:/model [模型]\n模型列表:gpt-4o-mini、gpt-3.5-turbo-0125、gpt-3.5-turbo-1106、gpt-3.5-turbo");
                        return;
                    }
                    model = parts[1];
                    chatService.ClearHistory(id);
                    await bot.Message.sendMessage(e.GroupId, e.MessageId, $"切换完成,当前模型为:{model}");
                }
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
