using System.Collections.Generic;

namespace QQBotCodePlugin.utils
{
    public class RunBotInfo
    {
        public string BotPath { get; set; }
        public bool Plugin { get; set; }
        public string BotName { get; set; }
        public string BotDescription { get; set; }
        public string IPAddress { get; set; }
        public int ServerPort { get; set; }
        public int EventPort { get; set; }
        public Dictionary<string, bool> EventData { get; set; }
    }
}
