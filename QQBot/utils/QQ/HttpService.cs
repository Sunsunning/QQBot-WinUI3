using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.QQ
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly Logger logger;

        public HttpService(HttpClient httpClient, StackPanel console, Bot bot)
        {
            _httpClient = httpClient;
            logger = new Logger(console, bot);
        }

        public async Task<string> SendGetRequestAsync(string url, bool sendToConsole = true)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                if (sendToConsole) logger.Info(await response.Content.ReadAsStringAsync());
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                logger.Error($"请求发生错误:{e.Message}");
                return null;
            }
        }

        public async Task<string> SendPostRequestAsync(string url, HttpContent content, bool sendToConsole = true)
        {
            try
            {

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                if (sendToConsole) logger.Info(await response.Content.ReadAsStringAsync());
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                logger.Error($"请求发生错误:{e.Message}");
                return null;
            }
        }
    }
}
