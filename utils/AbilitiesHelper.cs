using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.utils
{
    public class AbilitiesHelper
    {
        private Dictionary<string, string> _abilities = new Dictionary<string, string>() {
            { "AIChat", "ChatGPT聊天" },
            { "Help", "帮助" },
            { "KudosMe", "赞我" },
            { "NumberBoom", "数字炸弹" },
            { "onset", "发病" },
            { "ping", "Ping" },
            { "RunWindowsCommand", "运行windows命令" },
            { "Sky", "光遇查询" },
            { "voice", "语音合成" },
            { "wife", "今日辣泼" },
            { "ba", "Ba标题表情包" },
            { "cat", "柴郡表情包" },
            { "eat", "猫猫虫吃表情包" },
            { "play", "猫猫虫玩表情包" }
        };

        public string getAbilitiesHelp(string key) { 
            return _abilities[key];
        }
    }
}
