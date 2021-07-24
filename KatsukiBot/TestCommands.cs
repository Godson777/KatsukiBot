using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot {
    public class TestCommands : BaseCommandModule {
        [Command("test")]
        public async Task Test(CommandContext ctx) {
            var x = await ctx.RespondAsync("This is a test message, in 10 seconds, this will turn into fake results for a test poll.");
            var poll = Poll.TestPoll(ctx.Channel.Id, x.Id);
            await poll.Execute();
        }
    }
}
