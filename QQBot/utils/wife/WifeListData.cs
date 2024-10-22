﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.wife
{
    public class WifeListData
    {
        [JsonProperty("Time")]
        public DateTime Time { get; set; }

        [JsonProperty("Wife")]
        public Dictionary<long, Dictionary<long, long>> Wife { get; set; }

        public WifeListData()
        {
            Wife = new Dictionary<long, Dictionary<long, long>>();
        }
    }
}