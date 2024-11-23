using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.QQBot.utils.wife;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static PluginDLL.Class1;

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
        }

        public void Register(Bot bot)
        {
            _bot = bot;
            _bot.GroupReceived += OnMessageReceived;
            _bot.getLogger().Info($"{this.ToString()}注册完成");
            List<string> description = _bot.helpCommandHelper.getCommands("普通功能");
            description.Add("/今日老婆 - 看看你今天的群老婆是谁");
            _bot.helpCommandHelper.addCommands("普通功能", description);
            this.wifeManager = new WifeManager(_bot.getLogger(), _bot);
        }

        private async void OnMessageReceived(object sender, MessageEvent e)
        {
            string message = e.Messages[0].Data.Text;
            if (string.IsNullOrEmpty(message) || !(message.Equals("/今日老婆")))
            {
                return;
            }

            _users = await GetUsers(e.GroupId);
            long senderId = e.Sender.UserId;
            long target = wifeManager.GetTargetQQ(e.GroupId, senderId);

            if (target != -1)
            {
                User user = GetUserByQid(target);
                string url = $"http://q.qlogo.cn/headimg_dl?dst_uin={target}&spec=640&img_type=jpg";
                await _bot.Message.sendGroupMessageAndImage(e.GroupId, e.MessageId, $"\n你今天有{user.Nickname}({user.UserId})了你这个花心的人!", url, senderId, e.Sender.Nickname);
            }
            else if (_users != null && _users.Count > 0)
            {
                User wife = _users[GenerateRandomNumber(0, _users.Count)];
                string avatar = $"http://q.qlogo.cn/headimg_dl?dst_uin={wife.UserId}&spec=640&img_type=jpg";
                await _bot.Message.sendGroupMessageAndImage(e.GroupId, e.MessageId, $"\n你今日的老婆是\n{wife.Nickname}({wife.UserId})!", avatar, senderId, e.Sender.Nickname);
                wifeManager.AddOrUpdateWife(e.GroupId, senderId, wife.UserId);
            }
            else
            {
                await _bot.Message.sendMessage(e.GroupId, e.MessageId, "发生错误:无法获取群列表");
            }
        }

        private async Task<List<User>> GetUsers(long groupId)
        {
            string jsonContent = $"{{\"group_id\": \"{groupId}\"}}";
            using (var content = new StringContent(jsonContent, Encoding.UTF8, "application/json"))
            {
                IHttpService httpService = new HttpService(httpClient, _bot.getConsole(), _bot);
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
