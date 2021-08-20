using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using KatsukiBot.API;
using KatsukiBot.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Discord {
    class GeneralCommands : SlashCommandModule {
        [SlashCommand("about", "Need to know who I am?")]
        async Task About(InteractionContext ctx) {
            await ctx.ReplyBasicAsync("Hey! This is a test!");
        }

        [SlashCommand("test", "this'll be deleted soonTM")]
        async Task Test(InteractionContext ctx, [Option("message", "the message to send to the API")] string msg) {
            var hub = Program.APIHost.Services.GetRequiredService< IHubContext<TestHub>>();
            await hub.Clients.Group("balls").SendAsync("UpdateLabel", msg);
            await ctx.ReplyBasicAsync("Test Message Sent", true);
        }
    }
}
