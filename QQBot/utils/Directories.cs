using QQBotCodePlugin.QQBot;
using System.IO;

namespace QQBotCodePlugin.QQBot.utils
{
    public class Directories
    {
        public static readonly string ImagesPath = Path.Combine(BotMain.getCurrentPath(), "images");
        public static readonly string LogsPath = Path.Combine(BotMain.getCurrentPath(), "logs");
        public static readonly string ConfigPath = Path.Combine(BotMain.getCurrentPath(), "config");
        public static readonly string TempPath = Path.Combine(BotMain.getCurrentPath(), "temp");
    }
}
