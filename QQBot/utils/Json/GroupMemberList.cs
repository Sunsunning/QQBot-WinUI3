using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.Json
{
    public class GroupMemberList
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("retcode")]
        public int Retcode { get; set; }

        [JsonProperty("data")]
        public List<User> Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("wording")]
        public string Wording { get; set; }
    }

    public class User
    {
        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("card")]
        public string Card { get; set; }

        [JsonProperty("sex")]
        public string Sex { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("qq_level")]
        public int QqLevel { get; set; }

        [JsonProperty("join_time")]
        public long JoinTime { get; set; }

        [JsonProperty("last_sent_time")]
        public long LastSentTime { get; set; }

        [JsonProperty("title_expire_time")]
        public long TitleExpireTime { get; set; }

        [JsonProperty("unfriendly")]
        public bool Unfriendly { get; set; }

        [JsonProperty("card_changeable")]
        public bool CardChangeable { get; set; }

        [JsonProperty("is_robot")]
        public bool IsRobot { get; set; }

        [JsonProperty("shut_up_timestamp")]
        public long ShutUpTimestamp { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
