using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Utils {
    public static class CommandExtensions {
        /// <summary>
        /// Delays the reply to the user.
        /// After doing so, you can reply within 15 minutes using <see cref="InteractionContext.FollowUpAsync(DiscordFollowupMessageBuilder)"/>
        /// </summary>
        /// <param name="ctx">The interaction context</param>
        /// <returns></returns>
        public static async Task SendThinking(this InteractionContext ctx) =>
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        /// <summary>
        /// Sends a response to the command recieved. For messages that consist of a basic string, see <seealso cref="ReplyBasicAsync(InteractionContext, string, bool)"/>
        /// </summary>
        /// <param name="ctx">The interaction context</param>
        /// <param name="builder">The interaction response builder</param>
        /// <returns></returns>
        public static async Task ReplyAsync(this InteractionContext ctx, DiscordInteractionResponseBuilder builder) =>
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);

        /// <summary>
        /// Sends a response to the command recieved.
        /// </summary>
        /// <param name="ctx">The interaction context</param>
        /// <param name="content">The content of the response</param>
        /// <param name="ephemeral">Whether or not the response is ephemeral</param>
        /// <returns></returns>
        public static async Task ReplyBasicAsync(this InteractionContext ctx, string content, bool ephemeral = false) =>
            await ctx.ReplyAsync(new DiscordInteractionResponseBuilder().WithContent(content).AsEphemeral(ephemeral));
    }
}
