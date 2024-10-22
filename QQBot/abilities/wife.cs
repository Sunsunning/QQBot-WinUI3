using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.QQBot.utils.wife;
using QQBotCodePlugin.QQBot.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.abilities
{
    public class wife : IEventHandler
    {
        private readonly HttpClient httpClient;
        private readonly Random _random = new Random();
        private Bot _bot;
        private List<User> _users = new List<User>();
        private WifeManager wifeManager;

        public wife()
        {
            this.httpClient = new HttpClient();
            this.wifeManager = new WifeManager();
        }

        public void Register(Bot bot)
        {
            _bot = bot;
            _bot.GroupReceived += OnMessageReceived;
            _bot.getLogger().Info($"{this.ToString()}注册完成");
        }

        private async void OnMessageReceived(object sender, GroupMessageReceivedEvent e)
        {
            string message = e.Message.Messages[0].Data.Text;
            if (string.IsNullOrEmpty(message) || !message.StartsWith("/今日老婆"))
            {
                return;
            }

            _users = await GetUsers(e.Message.GroupId);
            long senderId = e.Message.Sender.UserId;
            long target = wifeManager.GetTargetQQ(e.Message.GroupId, senderId);

            if (target != -1)
            {
                string url = $"http://q.qlogo.cn/headimg_dl?dst_uin={target}&spec=640&img_type=jpg";
                User user = GetUserByQid(target);
                await _bot.Message.sendGroupMessageAndImage(e.Message.GroupId, e.Message.MessageId, $"\n你今天有{user.Nickname}了你这个花心的人!", url, senderId, e.Message.Sender.Nickname);
            }
            else if (_users != null && _users.Count > 0)
            {
                User wife = _users[GenerateRandomNumber(0, _users.Count)];
                string avatar = $"http://q.qlogo.cn/headimg_dl?dst_uin={wife.UserId}&spec=640&img_type=jpg";
                await _bot.Message.sendGroupMessageAndImage(e.Message.GroupId, e.Message.MessageId, $"\n你今日的老婆是{wife.Nickname}!", avatar, senderId, e.Message.Sender.Nickname);
                wifeManager.AddOrUpdateWife(e.Message.GroupId, senderId, wife.UserId);
            }
            else
            {
                await _bot.Message.sendMessage(e.Message.GroupId, e.Message.MessageId, "发生错误:无法获取群列表");
            }
        }

        private async Task<List<User>> GetUsers(long groupId)
        {
            string jsonContent = $"{{\"group_id\": \"{groupId}\"}}";
            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
            {
                IHttpService httpService = new HttpService(httpClient,_bot.getConsole());
                string memberList = await httpService.SendPostRequestAsync($"{_bot.getServiceAddress()}/get_group_member_list", content, sendToConsole: false);
                return string.IsNullOrEmpty(memberList) ? new List<User>() : JsonConvert.DeserializeObject<GroupMemberList>(memberList).Data;
            }
        }

        private int GenerateRandomNumber(int minValue, int maxValue)
        {
            if (minValue >= maxValue)
            {
                throw new ArgumentException("最小值必须小于最大值");
            }
            return _random.Next(minValue, maxValue);
        }

        private User GetUserByQid(long qid)
        {
            return _users.FirstOrDefault(u => u.UserId == qid);
        }
    }
}
