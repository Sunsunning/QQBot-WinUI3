using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.IO;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class Help : IEventHandler
    {
        private Bot bot;
        private List<string> commands;
        public Help()
        {
            this.commands = new List<string>();
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += OnMessageReceived;
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!(message.StartsWith("/help") || message.StartsWith("/帮助") || message.StartsWith("/?") || message.StartsWith("/？")))
            {
                return;
            }
            if (commands.Count == 0)
            {
                commands.Add("----------------命令列表----------------\n");
                commands.Add("1./光遇活动 - 获取每天的光遇活动日历");
                commands.Add("2./光遇天气 - 获取每日的光遇天气");
                commands.Add("3./龙图 - 随机给出一张龙图");
                commands.Add("4./说 [角色] [文本] - 进行语音合成");
                commands.Add("5./随机柴郡 - 随机返回一张柴郡表情包");
                commands.Add("6./ping - Ping似AI");
                commands.Add("7./今日老婆 - 看看你今天的群老婆是谁");
                commands.Add("8./赞我 - 赞赞你的");
                commands.Add("9./发病 [名称] - 冰冰冰!");
                commands.Add("10./咬 [@某人] - 猫猫虫咬似ta~");
                commands.Add("11./玩 [@某人] - 玩玩你的");
                commands.Add("12./ba [左侧字符] [右侧字符] - 生成Ba标题");
                commands.Add("- 数字炸弹:");
                commands.Add("1./数字炸弹 start - 开始游戏(开始游戏后其他玩家无法加入");
                commands.Add("2./数字炸弹 stop - 停止游戏");
                commands.Add("3./数字炸弹 join - 加入游戏(开始游戏后无法加入");
                commands.Add("4./数字炸弹 leave - 退出游戏(开始游戏后无法退出");
                commands.Add("5./数字炸弹 list - 列出加入游戏的列表");
                commands.Add("tip:游戏开始后游玩的玩家可直接输入数字!");
                commands.Add("- AI类:");
                commands.Add("- 使用 #+文本 来进行AI聊天");
                commands.Add("----------------命令列表----------------");
            }

            string file = RenderCommands(commands, "JetBrains Mono", 35);
            await bot.Message.sendImageMessage(e.Message.GroupId, e.Message.MessageId, $"file://{file}", useFile: true);
        }

        private string RenderCommands(List<string> commands, string fontName, int fontSize)
        {
            int canvasWidth = 1120;
            int canvasHeight = commands.Count * (fontSize + 5) + 50;
            string outputPath = Path.Combine(Directories.TempPath, "commands.png");

            using (Bitmap bitmap = new Bitmap(canvasWidth, canvasHeight))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                using (Font font = new Font(fontName, fontSize, GraphicsUnit.Pixel))
                {
                    StringFormat stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near
                    };
                    for (int i = 0; i < commands.Count; i++)
                    {
                        graphics.DrawString(commands[i], font, Brushes.Black, new PointF(10, 10 + i * (fontSize + 5)), stringFormat);
                    }
                }
                bitmap.Save(outputPath, ImageFormat.Png);
            }

            return outputPath;
        }
    }
}
