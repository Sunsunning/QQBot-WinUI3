using Newtonsoft.Json;
using System.Collections.Generic;

namespace QQBotCodePlugin.QQBot.utils.Json
{
    public class VoiceApiResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("characters")]
        public List<string> Characters { get; set; }

        [JsonProperty("API")]
        public string Api { get; set; }

        [JsonProperty("mp3")]
        public string mp3 { get; set; }
    }
}
