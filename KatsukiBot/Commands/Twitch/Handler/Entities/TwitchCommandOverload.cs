using System;
using System.Collections.Generic;
using System.Text;

namespace KatsukiBot.Commands.Twitch.Handler.Entities {
    /// <summary>
    /// Represents a specific overload of a command.
    /// </summary>
    public sealed class TwitchCommandOverload {
        /// <summary>
        /// Gets this command overload's arguments.
        /// </summary>
        public IReadOnlyList<TwitchCommandArgument> Arguments { get; internal set; }

        /// <summary>
        /// Gets this command overload's priority.
        /// </summary>
        public int Priority { get; internal set; }

        /// <summary>
        /// Gets this command overload's delegate.
        /// </summary>
        internal Delegate Callable { get; set; }

        internal object InvocationTarget { get; set; }

        internal TwitchCommandOverload() { }
    }
}
