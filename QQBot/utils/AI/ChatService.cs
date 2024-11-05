using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using QQBotCodePlugin.QQBot.utils.QQ;
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
        private readonly Dictionary<long, List<ChatHistory>> _histories;
        private readonly StackPanel _console;

        public class ChatHistory
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        public ChatService(string url, StackPanel console, Bot bot)
        {
            _url = url;
            _console = console;
            _httpClient = new HttpClient();
            _histories = new Dictionary<long, List<ChatHistory>>();
            _httpService = new HttpWithHeaderService(this._httpClient, console, bot);
        }

        public void AddToHistory(long id, string role, string content)
        {
            if (!_histories.ContainsKey(id))
            {
                _histories[id] = new List<ChatHistory>();
            }
            _histories[id].Add(new ChatHistory { role = role, content = content });
        }

        public void ClearHistory(long id)
        {
            if (_histories.ContainsKey(id))
            {
                _histories[id].Clear();
            }
        }

        public async Task<string> PostChatDataAsync(string model, long id, string role, string question)
        {
            AddToHistory(id, role, question);
            var json = new JObject
            {
                ["model"] = model,
                ["messages"] = new JArray(_histories[id].Select(historyItem => new JObject
                {
                    ["role"] = historyItem.role,
                    ["content"] = historyItem.content
                }))
            };

            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            string response = await _httpService.SendHeaderPostRequestAsync(_url, content, null, "sk-7disM0OwJrd96Y8CWDwVHXcE4X8IECk8iFCkP1NGQTq2mTfb", false);
            string message = (string)JObject.Parse(response)["choices"][0]["message"]["content"];
            return message;
        }
    }
}