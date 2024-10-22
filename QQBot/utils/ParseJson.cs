using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils
{
    public class ParseJson
    {
        private MessageEvent messageEvent;

        public ParseJson(string json,StackPanel console)
        {
            try
            {
                messageEvent = JsonConvert.DeserializeObject<MessageEvent>(json);
            }
            catch (JsonException ex)
            {
                new Logger(console).Error($"发生错误:{ex.Message}");
                messageEvent = null;
            }
        }

        public MessageEvent Get()
        {
            return messageEvent;
        }
    }
}
