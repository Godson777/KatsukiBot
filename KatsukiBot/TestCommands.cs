using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using KatsukiBot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KatsukiBot.Managers.PollManager;

namespace KatsukiBot {
    public class TestCommands : BaseCommandModule {
        [Command("test")]
        public async Task Test(CommandContext ctx) {
            //var x = await ctx.RespondAsync("This is a test message, in 10 seconds, this will turn into fake results for a test poll.");
            await PollManager.CreatePoll(ctx.Guild)
                .SetTitle("Test")
                .SetDestination(ctx.Channel)
                .AddOption("peepee")
                .AddOption("poopoo")
                .AddOption("aha")
                .SetCompletionTime(DateTime.Now.AddSeconds(60))
                .Build();
        }

        [Command("cancel")]
        public async Task Testt(CommandContext ctx) {
            PollManager.Get().polls.TryGetValue(ctx.Guild.Id, out var e);
            e.Values.ToArray()[0].CancelPls();
        }
    }
}
