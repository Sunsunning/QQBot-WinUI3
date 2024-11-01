using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using Windows.UI;

namespace QQBotCodePlugin.QQBot
{
    public class Logger
    {
        private string LogFilePath { get; set; }
        private readonly object LockObject = new object();
        private StackPanel _console;

        public Logger(StackPanel console, Bot bot)
        {
            _console = console;
            var logsPath = bot.getLogs();
            if (!Directory.Exists(logsPath))
            {
                Directory.CreateDirectory(logsPath);
            }
            var fileName = $"log-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
            LogFilePath = Path.Combine(logsPath, fileName);
        }

        private void WriteLog(string level, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(LogFilePath))
            {
                throw new InvalidOperationException("Log file path is not initialized.");
            }

            var logMessage = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss} {level} {message}";
            lock (LockObject)
            {
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            if (_console.Children.Count > 200)
            {
                _console.Children.Clear();
            }

            string dateTimeString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            Run dateTimeRun = new Run()
            {
                Text = dateTimeString,
                Foreground = new SolidColorBrush(Colors.Gray),
                FontWeight = FontWeights.Medium,
                FontSize = 15
            };
            Run infoRun = new Run()
            {
                Text = $" {level}",
                Foreground = new SolidColorBrush(GetConsoleColor(level)),
                FontWeight = FontWeights.Medium,
                FontSize = 15
            };
            Run messageRun = new Run()
            {
                Text = $" {message}",
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Medium,
                FontSize = 15
            };
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(dateTimeRun);
            paragraph.Inlines.Add(infoRun);
            paragraph.Inlines.Add(messageRun);
            RichTextBlock richTextBlock = new RichTextBlock();
            richTextBlock.Blocks.Add(paragraph);
            _console.Children.Add(richTextBlock);
        }

        private Color GetConsoleColor(string level)
        {
            switch (level.ToLower())
            {
                case "info":
                    return Colors.Gold;
                case "warning":
                    return Colors.Yellow;
                case "error":
                    return Colors.Red;
                case "debug":
                    return Colors.Magenta;
                default:
                    return Colors.Gray;
            }
        }

        public void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        public void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        public void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

    }
}
