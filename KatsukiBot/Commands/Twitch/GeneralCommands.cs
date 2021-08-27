using DSharpPlus.CommandsNext.Attributes;
using KatsukiBot.Commands.Twitch.Handler;
using KatsukiBot.Commands.Twitch.Handler;
using KatsukiBot.Commands.Twitch.Handler.Attributes;
using KatsukiBot.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch {
    class GeneralCommands : TwitchBaseCommandModule {
        [Group("quote"), Aliases("q")]
        sealed class QuoteCommand : TwitchBaseCommandModule {
            [GroupCommand, Priority(0)]
            async Task Quote(TwitchCommandContext ctx) {
                var channel = ctx.Channel;
                var streamer = await Streamer.GetStreamerByChannelName(channel);
                if (streamer == null) {
                    ctx.Respond("This streamer hasn't linked their channel to their discord server!");
                    return;
                }
                var quote = await Settings.GetQuoteForGuild(streamer.GuildID);
                if (quote == null) {
                    ctx.Respond("Hey, wait a second! There aren't any quotes!");
                    return;
                }
                ctx.Respond(quote);
            }

            [GroupCommand, Priority(1)]
            async Task Quote(TwitchCommandContext ctx, int QuoteNumber) {
                var channel = ctx.Channel;
                var streamer = await Streamer.GetStreamerByChannelName(channel);
                if (streamer == null) {
                    ctx.Respond("This streamer hasn't linked their channel to their discord server!");
                    return;
                }
                var set = await Settings.Get(streamer.GuildID);
                if (set.Quotes.Count == 0) {
                    ctx.Respond("Hey, wait a second! There aren't any quotes!");
                    return;
                }

                var toGet = QuoteNumber - 1;
                if (toGet < 0) {
                    ctx.Respond("Uhhh... Are you sure you typed that right?");
                    return;
                }
                if (set.Quotes.Count > toGet) {
                    ctx.Respond($"Nya!? You only have {set.Quotes.Count} quote{(set.Quotes.Count > 1 ? "s" : "")}! Are you sure you typed the right number?");
                    return;
                }

                ctx.Respond(set.Quotes[toGet]);
            }

            [Command("add"), RequireModerator]
            async Task Add(TwitchCommandContext ctx, [RemainingText] string Quote) {
                var channel = ctx.Channel;
                var streamer = await Streamer.GetStreamerByChannelName(channel);
                if (streamer == null) {
                    ctx.Respond("This streamer hasn't linked their channel to their discord server!");
                    return;
                }
                var set = await Settings.Get(streamer.GuildID);
                set.Quotes.Add(Quote);
                await set.Save();
                ctx.Respond("Got it! I've written it down for you, Nya!");
            }
        }
    }
}