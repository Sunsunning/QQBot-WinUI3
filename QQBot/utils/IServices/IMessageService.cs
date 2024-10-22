using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.IServices
{
    internal interface IMessageService
    {
        Task<string> SendGroupDirectMessageAsync(long id, string message, bool autoEscape = false, bool sendToConsole = true);

        Task<string> SendGroupMessageAsync(long id, long message_id, string message, bool autoEscape = false, bool sendToConsole = true);

        Task<string> SendGroupImageMessageAsync(long groupId, long message_id, string url, string summary, bool sendToConsole = true);

        Task<string> SendGroupVoiceMessageAsync(long groupId, string url, bool sendToConsole = true);

        Task<string> SendGroupMessageAsync(long groupId, long message_id, string text, string imageUrl, long at_userId, string name, string summary, bool sendToConsole = true);

        Task<string> sendLike(long user_id, int times, bool sendToConsole = true);
    }
}
