using Emzi0767.Utilities;
using KatsukiBot.Commands.Twitch.Handler.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler {
    public class TwitchCommandEventArgs : EventArgs {
        public TwitchCommandContext Context { get; internal set; }
        public TwitchCommand Command => this.Context.Command;
    }
}
