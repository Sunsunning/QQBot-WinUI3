using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using static PluginDLL.Class1;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class NumberBoom : IEventHandler
    {
        private Bot bot;
        private readonly Random random;
        private Dictionary<long, GameState> gameStates;

        public NumberBoom()
        {
            random = new Random();
            gameStates = new Dictionary<long, GameState>();
        }

        public void Register(Bot bot)
        {
            this.bot = bot;
            bot.GroupReceived += Bot_GroupReceived;
            List<string> description = bot.helpCommandHelper.getCommands("数字炸弹");
            description.Add("/数字炸弹 start - 开始游戏(开始游戏后其他玩家无法加入");
            description.Add("/数字炸弹 stop - 停止游戏");
            description.Add("/数字炸弹 join - 加入游戏(开始游戏后无法加入");
            description.Add("/数字炸弹 leave - 退出游戏(开始游戏后无法退出");
            description.Add("/数字炸弹 list - 列出加入游戏的列表");
            description.Add("tip:游戏开始后游玩的玩家可直接输入数字!");
            bot.helpCommandHelper.addCommands("数字炸弹", description);
            bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void Bot_GroupReceived(object sender, MessageEvent e)
        {
            string message = e.Messages[0].Data.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            string[] parts = message.Split(' ');
            long groupId = e.GroupId;

            if (!message.StartsWith("/数字炸弹") && !IsGameRunning(groupId))
            {
                return;
            }

            switch (parts.Length)
            {
                case 1 when IsGameRunning(groupId):
                    await bot.Message.sendMessage(groupId, e.MessageId, ProcessGuess(e.Sender.UserId, groupId, e.Sender.Nickname, message));
                    break;
                case 2:
                    HandleCommand(groupId, e.MessageId, e.Sender.UserId, parts[1], e.Sender.Nickname);
                    break;
                default:
                    await bot.Message.sendMessage(groupId, e.MessageId, "用法:\n/数字炸弹 join\n/数字炸弹 leave\n/数字炸弹 list\n数字炸弹 start\n/数字炸弹 stop\n详细信息请输入/help");
                    break;
            }
        }

        private string ProcessGuess(long userId, long groupId, string name, string message)
        {
            if (!int.TryParse(message, out int guess))
            {
                return "输入错误，请输入一个有效的数字";
            }

            var gameState = GetGameState(groupId);
            if (gameState == null || !gameState.Players.ContainsKey(userId))
            {
                return "你不在游玩列表中哦~";
            }

            return MakeGuess(guess, groupId, name);
        }

        private string MakeGuess(int guess, long groupId, string name)
        {
            var gameState = GetGameState(groupId);
            if (gameState.Number == guess)
            {
                GameStop(groupId);
                return $"哇塞! {name} 猜对了数字！";
            }
            else if (guess < gameState.Number && guess < gameState.MaxNumber && guess > gameState.MinNumber)
            {
                gameState.MinNumber = guess;
                return $"现在的数字范围为({guess}-{gameState.MaxNumber})";
            }
            else if (guess > gameState.Number && guess < gameState.MaxNumber && guess > gameState.MinNumber)
            {
                gameState.MaxNumber = guess;
                return $"现在的数字范围为({gameState.MinNumber}-{guess})";
            }
            return "无效的猜测";
        }

        private async void HandleCommand(long groupId, long message_id, long userId, string command, string nickname)
        {
            switch (command)
            {
                case "join":
                    await bot.Message.sendMessage(groupId, message_id, JoinGame(groupId, userId, nickname));
                    break;
                case "leave":
                    await bot.Message.sendMessage(groupId, message_id, LeaveGame(groupId, userId));
                    break;
                case "list":
                    await bot.Message.sendMessage(groupId, message_id, GameList(groupId));
                    break;
                case "start":
                    await bot.Message.sendDirectMessage(groupId, GameStart(groupId));
                    break;
                case "stop":
                    await bot.Message.sendDirectMessage(groupId, GameStop(groupId));
                    break;
            }
        }

        private string JoinGame(long groupId, long userId, string nickname)
        {
            var gameState = GetGameState(groupId);
            if (gameState.Players.ContainsKey(userId))
            {
                return "你已经加入游戏了!";
            }
            gameState.Players[userId] = nickname;
            return $"{nickname}成功加入游戏~";
        }

        private string LeaveGame(long groupId, long userId)
        {
            var gameState = GetGameState(groupId);
            if (!gameState.Players.ContainsKey(userId))
            {
                return "你没有在游戏列表中";
            }
            gameState.Players.Remove(userId);
            return $"{gameState.Players[userId]}退出了游戏~";
        }

        private string GameList(long groupId)
        {
            var gameState = GetGameState(groupId);
            if (gameState.Players.Count == 0)
            {
                return "好像没有人玩呢(◞ ‸ ◟ㆀ)";
            }
            return $"{string.Join(", ", gameState.Players.Values)}\n正在玩哦~";
        }

        private string GameStart(long groupId)
        {
            var gameState = GetGameState(groupId);
            if (gameState.Players.Count == 0)
            {
                return "人都没有,开始什么嘛! ⁽⁽(੭ꐦ •̀Д•́ )੭*⁾⁾";
            }
            if (gameState.IsRunning)
            {
                return "游戏已经开始了";
            }
            gameState.IsRunning = true;
            gameState.Number = random.Next(gameState.MinNumber, gameState.MaxNumber);
            return $"游戏开始!\n现在玩家可以直接输入数字进行游戏!\n现在的数字范围为{gameState.MinNumber}-{gameState.MaxNumber}";
        }

        private string GameStop(long groupId)
        {
            var gameState = GetGameState(groupId);
            if (!gameState.IsRunning)
            {
                return "游戏并没有运行";
            }
            gameState.IsRunning = false;
            gameState.Number = -1;
            gameState.MinNumber = 0;
            gameState.MaxNumber = 100;
            gameState.Players.Clear();
            return "游戏结束!";
        }

        private GameState GetGameState(long groupId)
        {
            if (!gameStates.ContainsKey(groupId))
            {
                gameStates[groupId] = new GameState
                {
                    IsRunning = false,
                    Players = new Dictionary<long, string>(),
                    MinNumber = 0,
                    MaxNumber = 100
                };
            }
            return gameStates[groupId];
        }

        private bool IsGameRunning(long groupId)
        {
            var gameState = GetGameState(groupId);
            return gameState.IsRunning;
        }

        public class GameState
        {
            public bool IsRunning { get; set; }
            public Dictionary<long, string> Players { get; set; }
            public int Number { get; set; }
            public int MinNumber { get; set; }
            public int MaxNumber { get; set; }
        }
    }
}
