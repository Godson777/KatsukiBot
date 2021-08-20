using KatsukiBot.Commands.Twitch.Handler.Attributes;
using KatsukiBot.Commands.Twitch.Handler.Entities;
using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace KatsukiBot.Commands.Twitch.Handler.Exceptions {
    [Serializable]
    /// <summary>
    /// Indicates that one or more checks for given command have failed.
    /// </summary>
    public class ChecksFailedException : Exception {
        /// <summary>
        /// Gets the command that was executed.
        /// </summary>
        public TwitchCommand Command { get; }

        /// <summary>
        /// Gets the context in which given command was executed.
        /// </summary>
        public TwitchCommandContext Context { get; }

        /// <summary>
        /// Gets the checks that failed.
        /// </summary>
        public IReadOnlyList<TwitchCheckBaseAttribute> FailedChecks { get; }

        /// <summary>
        /// Creates a new <see cref="ChecksFailedException"/>.
        /// </summary>
        /// <param name="command">Command that failed to execute.</param>
        /// <param name="ctx">Context in which the command was executed.</param>
        /// <param name="failedChecks">A collection of checks that failed.</param>
        public ChecksFailedException(TwitchCommand command, TwitchCommandContext ctx, IEnumerable<TwitchCheckBaseAttribute> failedChecks)
            : base("One or more pre-execution checks failed.") {
            this.Command = command;
            this.Context = ctx;
            this.FailedChecks = new ReadOnlyCollection<TwitchCheckBaseAttribute>(new List<TwitchCheckBaseAttribute>(failedChecks));
        }
    }
}