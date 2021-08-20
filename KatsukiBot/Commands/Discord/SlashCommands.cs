using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using KatsukiBot.Managers;
using KatsukiBot.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands {
    class SlashCommands : SlashCommandModule {
        // The following is the single most gross thing I've ever written for the sake of a slash command.
        [SlashCommand("poll", "Create a poll.")]
        async Task Poll(InteractionContext ctx, 
            [Option("question", "Type your question. (Example: \"What topping should I get on my pizza?\")")] string title,
            [Option("time", "How long the poll is active, must be longer than 30 mins. (Example: \"1d2h3m\" = 1 day, 2 hrs, 3 mins)")] string Time,
            [Option("choice_1", "Type your choice. (Example: \"Pineapple\")")] string choice1,
            [Option("choice_2", "Type your choice. (Example: \"Pineapple\")")] string choice2,
            [Option("choice_3", "Type your choice. (Example: \"Pineapple\")")] string choice3 = null,
            [Option("choice_4", "Type your choice. (Example: \"Pineapple\")")] string choice4 = null,
            [Option("choice_5", "Type your choice. (Example: \"Pineapple\")")] string choice5 = null,
            [Option("choice_6", "Type your choice. (Example: \"Pineapple\")")] string choice6 = null,
            [Option("choice_7", "Type your choice. (Example: \"Pineapple\")")] string choice7 = null,
            [Option("choice_8", "Type your choice. (Example: \"Pineapple\")")] string choice8 = null,
            [Option("choice_9", "Type your choice. (Example: \"Pineapple\")")] string choice9 = null,
            [Option("choice_10", "Type your choice. (Example: \"Pineapple\")")] string choice10 = null,
            [Option("choice_11", "Type your choice. (Example: \"Pineapple\")")] string choice11 = null,
            [Option("choice_12", "Type your choice. (Example: \"Pineapple\")")] string choice12 = null,
            [Option("choice_13", "Type your choice. (Example: \"Pineapple\")")] string choice13 = null,
            [Option("choice_14", "Type your choice. (Example: \"Pineapple\")")] string choice14 = null,
            [Option("choice_15", "Type your choice. (Example: \"Pineapple\")")] string choice15 = null,
            [Option("choice_16", "Type your choice. (Example: \"Pineapple\")")] string choice16 = null,
            [Option("choice_17", "Type your choice. (Example: \"Pineapple\")")] string choice17 = null,
            [Option("choice_18", "Type your choice. (Example: \"Pineapple\")")] string choice18 = null,
            [Option("choice_19", "Type your choice. (Example: \"Pineapple\")")] string choice19 = null,
            [Option("choice_20", "Type your choice. (Example: \"Pineapple\")")] string choice20 = null) {
            var timespan = await Util.ConvertArgAsync<TimeSpan>(Time, ctx.Client.GetCommandsNext());
            if (timespan == null) {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I'm sorry, I did not recognize that time. (Did you make sure to read the example?)").AsEphemeral(true));
                return;
            }
            if (timespan < TimeSpan.FromMinutes(30)) {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Polls cannot be shorter than 30 minutes.").AsEphemeral(true));
                return;
            }
            //var embedbuilder = new DiscordEmbedBuilder();
            //embedbuilder.AddField("Title", title);
            //embedbuilder.AddField("Time", $"{timespan}");
            //var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Review your poll, does this look correct?").AddEmbed(embedbuilder).AddComponents(new DiscordComponent[] { new DiscordButtonComponent(ButtonStyle.Success, "confirm", "Yes"), new DiscordButtonComponent(ButtonStyle.Danger, "deny", "No") }));
            //var result = await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, TimeSpan.FromMinutes(5));
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await PollManager.CreatePoll(ctx.Guild)
                .SetTitle(title)
                .AddOptions(choice1, choice2, choice3, choice4, choice5, choice6, choice7, choice8, choice9, choice10, choice11, choice12, choice13, choice14, choice15, choice16, choice17, choice18, choice19, choice20)
                .SetCompletionTime(DateTime.Now.Add(timespan.GetValueOrDefault()))
                .Build(ctx);
        }
    }
}
