using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQBotCodePlugin.QQBot.utils.IServices
{
    public interface IEventHandler
    {
        void Register(Bot bot);
    }
}
