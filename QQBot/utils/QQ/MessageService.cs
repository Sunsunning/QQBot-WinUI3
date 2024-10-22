﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace QQBotCodePlugin.QQBot.utils.QQ
{
    public enum MessageType
    {
        Private,
        Group,
        Image
    }

    public class MessageService : IMessageService
    {
        private readonly string _servicePrefix;
        private readonly HttpClient _httpClient;
        private IHttpService _httpService;

        public MessageService(string servicePrefix,StackPanel console)
        {
            _servicePrefix = servicePrefix;
            _httpClient = new HttpClient();
            _httpService = new HttpService(this._httpClient,console);
        }

        public async Task<string> SendGroupDirectMessageAsync(long id, string message, bool autoEscape = false, bool sendToConsole = true)
        {
            using (HttpClient client = new HttpClient())
            {
                var messageData = new JObject
                {
                    ["group_id"] = id,
                    ["message"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "text",
                        ["data"] = new JObject
                        {
                            ["text"] = message + "\n"
                        }
                        }
                    }
                };
                string json = JsonConvert.SerializeObject(messageData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await _httpService.SendPostRequestAsync($"{_servicePrefix}send_group_msg", content, sendToConsole);
            }
        }

        public async Task<string> SendGroupMessageAsync(long id, long message_id, string message, bool autoEscape = false, bool sendToConsole = true)
        {
            using (HttpClient client = new HttpClient())
            {
                var messageData = new JObject
                {
                    ["group_id"] = id,
                    ["message"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "reply",
                        ["data"] = new JObject
                        {
                            ["id"] = message_id,
                        }
                    },
                    new JObject
                    {
                        ["type"] = "text",
                        ["data"] = new JObject
                        {
                            ["text"] = message + "\n"
                        }
                        }
                    }
                };
                string json = JsonConvert.SerializeObject(messageData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                return await _httpService.SendPostRequestAsync($"{_servicePrefix}send_group_msg", content, sendToConsole);
            }
        }


        public async Task<string> SendGroupImageMessageAsync(long groupId, long message_id, string url, string summary, bool sendToConsole = true)
        {

            var messageData = new JObject
            {
                ["group_id"] = groupId,
                ["message"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "reply",
                        ["data"] = new JObject
                        {
                            ["id"] = message_id,
                        }
                    },
                    new JObject
                    {
                        ["type"] = "image",
                        ["data"] = new JObject
                        {
                            ["url"] = url,
                            ["summary"] = summary
                        }
                        }
                    }
            };

            string json = JsonConvert.SerializeObject(messageData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpService.SendPostRequestAsync($"{_servicePrefix}send_group_msg", content, sendToConsole);
        }

        public async Task<string> SendGroupVoiceMessageAsync(long groupId, string url, bool sendToConsole = true)
        {
            var messageData = new
            {
                group_id = groupId,
                message = new[]
                {
                    new
                    {
                        type = "record",
                        data = new
                        {
                            file = url
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(messageData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpService.SendPostRequestAsync($"{_servicePrefix}send_group_msg", content, sendToConsole);
        }

        public async Task<string> SendGroupMessageAsync(long groupId, long message_id, string text, string imageUrl, long at_userId, string name, string summary, bool sendToConsole = true)
        {
            var messageData = new JObject
            {
                ["group_id"] = groupId,
                ["message"] = new JArray
                {
                    new JObject
                    {
                        ["type"] = "reply",
                        ["data"] = new JObject
                        {
                            ["id"] = message_id,
                        }
                    },
                    new JObject
                    {
                        ["type"] = "at",
                        ["data"] = new JObject
                        {
                            ["qq"] = at_userId,
                            ["name"] = name,
                        }
                    },
                    new JObject
                    {
                        ["type"] = "text",
                        ["data"] = new JObject
                        {
                            ["text"] = text + "\n"
                        }
                    },
                    new JObject
                    {
                        ["type"] = "image",
                        ["data"] = new JObject
                        {
                            ["summary"] = summary,
                            ["subType"] = 0,
                            ["url"] = imageUrl
                        }
                    }
                }
            };

            string json = JsonConvert.SerializeObject(messageData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpService.SendPostRequestAsync($"{_servicePrefix}send_group_msg", content, sendToConsole);
        }

        public async Task<string> sendLike(long user_id, int times, bool sendToConsole = true)
        {
            var messageData = new
            {
                user_id = user_id,
                times = times
            };
            string json = JsonConvert.SerializeObject(messageData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await _httpService.SendPostRequestAsync($"{_servicePrefix}send_like", content, sendToConsole);
        }
    }
}