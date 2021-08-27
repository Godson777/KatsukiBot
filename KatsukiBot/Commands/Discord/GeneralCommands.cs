using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using KatsukiBot.API;
using KatsukiBot.Entities;
using KatsukiBot.Entities.Discord.Menu;
using KatsukiBot.Managers;
using KatsukiBot.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Discord {
    class GeneralCommands : ApplicationCommandModule {
        [SlashCommand("about", "Need to know who I am?")]
        async Task About(InteractionContext ctx) {
            await ctx.ReplyBasicAsync("Hey! This is a test!");
        }

        [SlashCommandGroup("quote", "Add, List, Remove, or Get quotes from a list of quotes made in your server or channel.")]
        sealed class QuoteCommand : ApplicationCommandModule {
            [SlashCommand("get", "Grabs a random quote from the list.")]
            async Task Get(InteractionContext ctx, [Option("quote_id", "A specific quote you want to get.")] long QuoteNumber = 0) {
                var set = await Settings.Get(ctx.Guild);
                if (set.Quotes.Count == 0) {
                    await ctx.ReplyBasicAsync("Hey, wait a second! There aren't any quotes in this server! <:godson2REE:642382389015740438>", true);
                    return;
                }

                if (QuoteNumber == 0) {
                    await ctx.ReplyBasicAsync(set.Quotes.RandomElement());
                    return;
                }

                var toGet = (int)QuoteNumber - 1;
                if (toGet < 0) {
                    await ctx.ReplyBasicAsync("Uhhh... Are you sure you typed that right?", true);
                    return;
                }
                if (set.Quotes.Count <= toGet) {
                    await ctx.ReplyBasicAsync($"Nya!? You only have {set.Quotes.Count} quote{(set.Quotes.Count > 1 ? "s" : "")}! Are you sure you typed the right number?", true);
                    return;
                }

                var quote = set.Quotes[toGet];
                await ctx.ReplyBasicAsync(quote, false);
            }

            [SlashCommand("add", "Adds a quote to your list of quotes."), SlashRequireUserPermissions(Permissions.ManageMessages)]
            async Task Add(InteractionContext ctx, [Option("quote", "The quote you want to add.")] string Quote) {
                var set = await Settings.Get(ctx.Guild);
                set.Quotes.Add(Quote);
                await set.Save();
                await ctx.ReplyBasicAsync("Got it! I've written it down for you, Nya! :pencil:", true);
            }

            [SlashCommand("remove", "Removes a quote from your list of quotes."), SlashRequireUserPermissions(Permissions.ManageMessages)]
            async Task Remove(InteractionContext ctx, [Option("quote_id", "The number of the quote you wish to remove.")] long QuoteNumber) {
                var set = await Settings.Get(ctx.Guild);
                var toGet = (int) QuoteNumber - 1;
                if (set.Quotes.Count == 0) {
                    await ctx.ReplyBasicAsync("Hey, wait a second! There aren't any quotes in this server! <:godson2REE:642382389015740438>", true);
                    return;
                }

                if (set.Quotes.Count < toGet) {
                    await ctx.ReplyBasicAsync($"Nya!? You only have {set.Quotes.Count} quote{(set.Quotes.Count > 1 ? "s" : "")}! Are you sure you typed the right number?", true);
                    return;
                }

                var quote = set.Quotes[toGet];
                set.Quotes.RemoveAt(toGet);
                await set.Save();
                await ctx.ReplyBasicAsync($"`{quote}`\n\nI've crumpled up this quote and thrown it away for you, Nya! :wastebasket:", true);
            }

            [SlashCommand("list", "Lists all of your quotes.")]
            async Task List(InteractionContext ctx) {
                var set = await Settings.Get(ctx.Guild);
                if (set.Quotes.Count == 0) {
                    await ctx.ReplyBasicAsync("Hey, wait a second! There aren't any quotes in this server! <:godson2REE:642382389015740438>", true);
                    return;
                }

                var builder = new Paginator(ctx.Client.GetInteractivity()) {
                    Strings = set.Quotes.Select(q => q.Length > 200 ? q.Substring(0, 200) + "..." : q).ToList()
                };
                builder.SetGenericColor(ctx.Member.Color);
                builder.SetGenericText("Here are your quotes:");
                builder.Users.Add(ctx.Member.Id);
                await ctx.SendThinking();
                await builder.Display(ctx);
            }
        }

        [SlashCommand("test", "this'll be deleted soonTM")]
        async Task Test(InteractionContext ctx, [Option("message", "the message to send to the API")] string msg) {
            /*var hub = Program.APIHost.Services.GetRequiredService<IHubContext<TestHub>>();
            await hub.Clients.Group("balls").SendAsync("UpdateLabel", msg);
            await ctx.ReplyBasicAsync("Test Message Sent", true);*/
            var quote = await Settings.GetQuoteForGuild(ctx.Guild.Id);
            await ctx.ReplyBasicAsync(quote);
        }
    }
}
