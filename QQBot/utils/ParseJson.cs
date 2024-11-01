using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.utils
{
    public class ParseJson
    {
        private MessageEvent messageEvent;

        public ParseJson(string json, StackPanel console, Bot bot)
        {
            try
            {
                messageEvent = JsonConvert.DeserializeObject<MessageEvent>(json);
            }
            catch (JsonException ex)
            {
                new Logger(console, bot).Error($"发生错误:{ex.Message}");
                messageEvent = null;
            }
        }

        public MessageEvent Get()
        {
            return messageEvent;
        }
    }
}
