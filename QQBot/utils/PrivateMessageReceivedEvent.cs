using QQBotCodePlugin.QQBot.utils.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils
{
    public class PrivateMessageReceivedEvent : EventArgs
    {
        public MessageEvent Message { get; }

        public PrivateMessageReceivedEvent(MessageEvent message)
        {
            Message = message;
        }
        public delegate void PrivateMessageReceivedEventHandler(object sender, PrivateMessageReceivedEvent e);
    }
}
