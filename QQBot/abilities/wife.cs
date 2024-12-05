using Newtonsoft.Json;
using PuppeteerSharp;
using QQBotCodePlugin.QQBot.utils.IServices;
using QQBotCodePlugin.QQBot.utils.Json;
using QQBotCodePlugin.QQBot.utils.QQ;
using QQBotCodePlugin.QQBot.utils.wife;
using QQBotCodePlugin.utils;
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
        private readonly SettingManager settingManager;
        private readonly string ChromePath;
        private Bot _bot;
        private List<User> _users = new List<User>();
        private WifeManager wifeManager;


        public wife()
        {
            this.httpClient = new HttpClient();
            this.settingManager = new SettingManager();
            ChromePath = settingManager.GetValue<string>("ChromePath");
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
                string path = await RenderHtmlToImageAsync(url, content[GenerateRandomNumber(1, content.Count)], $"{_bot.getImage()}/{getCurrentTime()}.png");
                await _bot.Message.sendGroupMessageAndImage(e.GroupId, e.MessageId, $"\n今天已经迎娶【{user.Nickname}】了哦~", path, senderId, e.Sender.Nickname);
            }
            else if (_users != null && _users.Count > 0)
            {
                User wife = _users[GenerateRandomNumber(0, _users.Count)];
                string avatar = $"http://q.qlogo.cn/headimg_dl?dst_uin={wife.UserId}&spec=640&img_type=jpg";
                string path = await RenderHtmlToImageAsync(avatar, content[GenerateRandomNumber(1, content.Count)], $"{_bot.getImage()}/{getCurrentTime()}.png");
                await _bot.Message.sendGroupMessageAndImage(e.GroupId, e.MessageId, $"\n{wife.Nickname}成为了你的新老婆哦~", path, senderId, e.Sender.Nickname);
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
                App.GetAppLogger().Log($"随机数最小值必须小于最大值", false);
                throw new ArgumentException("最小值必须小于最大值");
            }
            return _random.Next(minValue, maxValue);
        }

        private User GetUserByQid(long qid)
        {
            return _users.FirstOrDefault(u => u.UserId == qid);
        }

        private string getCurrentTime()
        {
            DateTime now = DateTime.Now;

            string str = now.ToString("yyyyMMdd_HHmmssfff");
            return str;
        }

        private async Task<string> RenderHtmlToImageAsync(string avatar, string content, string outputFile)
        {
            if (string.IsNullOrEmpty(avatar) || string.IsNullOrEmpty(ChromePath)) return null;
            string html = $"<html style=\"background: rgba(255, 255, 255, 0.6)\">\r\n    <head>\r\n    <style>\r\n    html, body {{\r\n        margin: 0;\r\n        padding: 0;\r\n    }}        \r\n    </style>\r\n    </head>\r\n    <div class=\"fortune\" style=\"width: 30%; height: 65rem; float: left; text-align: center; background: rgba(255, 255, 255, 0.6);\">\r\n    <h2>今日老婆</h2>\r\n    <br>\r\n    <div class=\"content\" style=\"margin: 0 auto; padding: 12px 12px; height: 49rem; max-width: 980px; max-height: 1024px; background: rgba(255, 255, 255, 0.6); border-radius: 15px; backdrop-filter: blur(3px); box-shadow: 0px 0px 15px rgba(0, 0, 0, 0.3); writing-mode: vertical-rl; text-orientation: mixed;\">\r\n        <p style=\"font-size: 2em\">{content}</p>\r\n    </div>\r\n    <br>\r\n    <br>\r\n    <br>\r\n    <p>HTML and Content From Logier-Plugin </p>\r\n    </div>\r\n    <div class=\"image\" style=\"height:65rem; width: 70%; float: right; box-shadow: 0px 0px 15px rgba(0, 0, 0, 0.3); text-align: center;\">\r\n    <img src={avatar} style=\"height: 100%; filter: brightness(100%); overflow: hidden; display: inline-block; vertical-align: middle; margin: 0; padding: 0;\"/>\r\n    </div>\r\n    </html>";

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, ExecutablePath = ChromePath });
            var page = await browser.NewPageAsync();

            await page.SetViewportAsync(new ViewPortOptions
            {
                Width = 1280,
                Height = 1000
            });

            await page.SetContentAsync(html);


            await page.ScreenshotAsync(outputFile, new ScreenshotOptions
            {
                Type = ScreenshotType.Png
            });

            await browser.CloseAsync();
            return outputFile;
        }

        private List<string> content = new List<string>() {
        "百年推甲子，福地在春申",
        "红毹拥出态娇妍，璧合珠联看并肩",
        "锦堂此夜春如海，瑞兆其昌五世绵",
        "喜溢重门迎凤侣，光增陋室迓宾车",
        "花好月圆庆佳期，鸟语芬芳喜事添",
        "蓬门且喜来珠履，侣伴从今到白头",
        "志同道合好伴侣，情深谊长新家庭",
        "连理枝头喜鹊闹，才子佳人信天缘",
        "百年恩爱双心结，千里姻缘一线牵",
        "琴韵谱成同梦语，灯花笑对含羞人",
        "佳偶天成心相印，百年好合乐无边",
        "洞房花烛交颈鸳鸯双得意，夫妻恩爱和鸣凤鸾两多情",
        "锋芒略敛夫妻和美，凡事无争伉俪温馨",
        "相亲相爱幸福永，同德同心幸福长",
        "鸳鸯璧合天缘定，龙凤呈祥喜气生",
        "百年修得同船渡，千年修得共枕眠",
        "良缘相遇情不禁，一种缘分两处思",
        "情投意合如芝兰，同心协力共克艰",
        "桃花潭水深千尺，不及汪伦送我情",
        "花开花落两相知，缘来缘去共相守"};
    }
}
