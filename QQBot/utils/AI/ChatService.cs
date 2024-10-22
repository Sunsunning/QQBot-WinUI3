using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.QQBot.utils.QQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.AI
{
    public class ChatService
    {
        private readonly string _url;
        private readonly HttpClient _httpClient;
        private readonly HttpWithHeaderService _httpService;
        private readonly List<ChatHistory> _history;
        private readonly StackPanel _console;

        public class ChatHistory
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public ChatService(string url,StackPanel console)
        {
            _url = url;
            _console = console;
            _httpClient = new HttpClient();
            _history = new List<ChatHistory>();
            _httpService = new HttpWithHeaderService(this._httpClient,console);
        }

        public void AddToHistory(string role, string content)
        {
            _history.Add(new ChatHistory { role = role, content = content });
        }

        public void ClearHistory()
        {
            _history.Clear();
        }

        public async Task<string> PostChatDataAsync(string role, string question)
        {
            AddToHistory(role, question);
            // var jsonString = JsonConvert.SerializeObject(_history.ToArray(), Formatting.Indented);

            var json = new JObject
            {
                ["model"] = "gpt-3.5-turbo",
                ["messages"] = new JArray(_history.Select(historyItem => new JObject
                {
                    ["role"] = historyItem.role,
                    ["content"] = historyItem.content
                }))
            };

            // Logger.Warning(jsonString);
            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            string response = await _httpService.SendHeaderPostRequestAsync(_url, content, null, "sk-7disM0OwJrd96Y8CWDwVHXcE4X8IECk8iFCkP1NGQTq2mTfb", false);
            string message = (string)JObject.Parse(response)["choices"][0]["message"]["content"];
            return message;
        }
    }
}
