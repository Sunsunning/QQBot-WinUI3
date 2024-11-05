using QQBotCodePlugin.QQBot.utils.IServices;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static PluginDLL.Class1;

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
            List<string> description = bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/help|/?|/？|/帮助 - 查看所有功能");
            bot.helpCommandHelper.addCommands("普通功能", description);
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, MessageEvent e)
        {
            string message = e.Messages[0].Data.Text;
            if (message == null)
            {
                return;
            }
            if (!(message.Equals("/help") || message.Equals("/帮助") || message.Equals("/?") || message.Equals("/？")))
            {
                return;
            }
            if (commands.Count == 0)
            {
                commands.Add("----------------命令列表----------------\n");
                foreach (var header in bot.helpCommandHelper.commands.Keys)
                {
                    commands.Add($"- {header}:");
                    List<string> cmds = bot.helpCommandHelper.commands[header];
                    for (int i = 0; i < cmds.Count; i++)
                    {
                        int p_4145_ = i;
                        commands.Add($"{p_4145_ + 1}.{cmds[i]}");
                    }
                }
                if (bot.loadPlugin)
                {
                    foreach (var plugin in bot.GetPluginsList())
                    {
                        foreach (var des in plugin.description())
                        {
                            commands.Add(des);
                        }
                    }
                }
                commands.Add("----------------命令列表----------------");
            }

            string file = RenderCommands(commands, "JetBrains Mono", 35);
            await bot.Message.sendImageMessage(e.GroupId, e.MessageId, $"file://{file}", useFile: true);
        }

        private string RenderCommands(List<string> commands, string fontName, int fontSize)
        {
            int canvasWidth = 1120;
            int canvasHeight = commands.Count * (fontSize + 5) + 50;
            string outputPath = Path.Combine(bot.getTemp(), "commands.png");

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
