﻿using Newtonsoft.Json;

namespace QQBotCodePlugin.QQBot.utils.Json
{
    public class HttpResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("retcode")]
        public int Retcode { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("wording")]
        public string Wording { get; set; }
    }
}
