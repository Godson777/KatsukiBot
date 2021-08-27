using KatsukiBot.Commands.Twitch.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Core.Models.Undocumented.CSStreams;

namespace KatsukiBot.Commands.Twitch.Handler.Attributes {
    /// <summary>
    /// Defines that usage of this command is restricted to moderators (and the streamer).
    /// </summary>
    public sealed class RequireModeratorAttribute : TwitchCheckBaseAttribute {
        public override Task<bool> ExecuteCheckAsync(TwitchCommandContext ctx) {
            var msg = ctx.Message;
            if (msg.IsModerator) return Task.FromResult(true);
            if (msg.IsBroadcaster) return Task.FromResult(true);
            else return Task.FromResult(false);
        }
    }
}
