using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KatsukiBot.Commands.Twitch.Handler.Entities {
    public class TwitchCommandGroup : TwitchCommand {

        /// <summary>
        /// Gets all the commands that belong to this module.
        /// </summary>
        public IReadOnlyList<TwitchCommand> Children { get; internal set; }

        /// <summary>
        /// Gets whether this command is executable without subcommands.
        /// </summary>
        public bool IsExecutableWithoutSubcommands => this.Overloads?.Any() == true;

        internal TwitchCommandGroup() : base() { }
    }
}
