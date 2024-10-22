using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.IServices
{
    internal interface IHttpService
    {
        Task<string> SendGetRequestAsync(string url, bool sendToConsole = true);

        Task<string> SendPostRequestAsync(string url, HttpContent content, bool sendToConsole = true);
    }
}
