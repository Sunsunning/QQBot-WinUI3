using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.Json
{
    public class MessageEvent
    {
        [JsonConstructor]
        public MessageEvent() { }

        [JsonProperty("self_id")]
        public long SelfId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("message_id")]
        public long MessageId { get; set; }

        [JsonProperty("real_id")]
        public long RealId { get; set; }

        [JsonProperty("message_seq")]
        public long MessageSeq { get; set; }

        [JsonProperty("message_type")]
        public string MessageType { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("raw_message")]
        public string RawMessage { get; set; }

        [JsonProperty("font")]
        public int Font { get; set; }

        [JsonProperty("sub_type")]
        public string SubType { get; set; }

        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        [JsonProperty("message")]
        public List<Message> Messages { get; set; }

        [JsonProperty("message_format")]
        public string MessageFormat { get; set; }

        [JsonProperty("post_type")]
        public string PostType { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("raw_info")]
        public List<RawInfo> RawInfo { get; set; }
    }

    public class RawInfo
    {
        [JsonProperty("col")]
        public string Col { get; set; }

        [JsonProperty("nm")]
        public string Nm { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("jp")]
        public string Jp { get; set; }

        [JsonProperty("src")]
        public string Src { get; set; }

        [JsonProperty("txt")]
        public string Txt { get; set; }

        [JsonProperty("tp")]
        public string Tp { get; set; }
    }

    public class Sender
    {
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("card")]
        public string Card { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        public Sender() { }
    }

    public class Message
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("qq")]
        public string qq { get; set; }
    }
}
