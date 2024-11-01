using Microsoft.UI.Xaml.Controls;
using QQBotCodePlugin.QQBot.utils.IServices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.QQ
{
    public class HttpWithHeaderService : HttpService, IHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly Logger logger;

        public HttpWithHeaderService(HttpClient httpClient, StackPanel console, Bot bot) : base(httpClient, console, bot)
        {
            _httpClient = httpClient;
            logger = new Logger(console, bot);
        }

        public async Task<string> SendHeaderPostRequestAsync(string url, HttpContent content, Dictionary<string, string> headers = null, string authorizationToken = null, bool sendToConsole = true)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken);
                }

                request.Content = content;
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                if (sendToConsole)
                {
                    logger.Info(await response.Content.ReadAsStringAsync());
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                logger.Error($"请求发生错误: {e.Message}");
                return null;
            }
            catch (Exception e) when (e is not HttpRequestException)
            {
                logger.Error($"请求发生未知错误: {e.Message}");
                return null;
            }
        }
    }
}
