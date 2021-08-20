using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Entities {
    public struct TwitchCommandResult {
        /// <summary>
        /// Gets whether the command execution succeeded.
        /// </summary>
        public bool IsSuccessful { get; internal set; }

        /// <summary>
        /// Gets the exception (if any) that occurred when executing the command.
        /// </summary>
        public Exception Exception { get; internal set; }

        /// <summary>
        /// Gets the context in which the command was executed.
        /// </summary>
        public TwitchCommandContext Context { get; internal set; }
    }
}
