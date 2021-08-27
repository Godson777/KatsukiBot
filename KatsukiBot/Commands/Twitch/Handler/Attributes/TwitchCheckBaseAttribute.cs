using KatsukiBot.Commands.Twitch.Handler;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Attributes {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class TwitchCheckBaseAttribute : Attribute {
        public abstract Task<bool> ExecuteCheckAsync(TwitchCommandContext ctx);
    }
}
