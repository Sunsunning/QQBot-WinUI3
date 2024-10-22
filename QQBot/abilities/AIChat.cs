using QQBotCodePlugin.QQBot.utils.AI;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class AIChat : IEventHandler
    {
        private Bot bot;
        private readonly string url = "https://api.chatanywhere.tech/v1/chat/completions";
        private ChatService chatService;
        public AIChat(){}

        public void Register(Bot bot)
        {
            this.bot = bot;
            this.bot.GroupReceived += OnMessageReceived;
            chatService = new ChatService(url, bot.getConsole()); 
            
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            try
            {
                if (message == null)
                {
                    return;
                }

                if (message.StartsWith("#"))
                {
                    if (message.Equals("#清空历史对话"))
                    {
                        chatService.ClearHistory();
                        await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "清理完成");
                        return;
                    }

                    int hashIndex = message.IndexOf("#");
                    string answer = await Chat("user", message.Substring(0, hashIndex) + message.Substring(hashIndex + 1));

                    await bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, answer);

                }
            }
            catch (NullReferenceException ex)
            {
                bot.getLogger().Warning($"发生错误:{ex.Message}");
                return;
            }
        }

        private async Task<string> Chat(string role, string question)
        {

            return await chatService.PostChatDataAsync(role, question);
        }
    }
}
