using QQBotCodePlugin.QQBot.utils.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils
{
    public class GroupMessageReceivedEvent : EventArgs
    {
        public MessageEvent Message { get; }

        public GroupMessageReceivedEvent(MessageEvent message)
        {
            Message = message;
        }
        public delegate void GroupMessageReceivedEventHandler(object sender, GroupMessageReceivedEvent e);

    }
}
