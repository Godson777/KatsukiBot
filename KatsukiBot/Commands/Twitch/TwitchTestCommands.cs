using DSharpPlus.CommandsNext.Attributes;
using KatsukiBot.Commands.Twitch.Handler;
using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch {
    public class TwitchTestCommands : TwitchBaseCommandModule {
        [Command("test")]
        Task Test(TwitchCommandContext ctx) {
            ctx.Respond("Holy shit this works");
            return Task.CompletedTask;
        }
    }
}
