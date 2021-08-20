using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler {
    /// <summary>
    /// Represents a base class for all command modules.
    /// </summary>
    public abstract class TwitchBaseCommandModule {
        /// <summary>
        /// Called before a command in the implementing module is executed.
        /// </summary>
        /// <param name="ctx">Context in which the method is being executed.</param>
        /// <returns></returns>
        public virtual Task BeforeExecutionAsync(TwitchCommandContext ctx)
            => Task.Delay(0);

        /// <summary>
        /// Called after a command in the implementing module is successfully executed.
        /// </summary>
        /// <param name="ctx">Context in which the method is being executed.</param>
        /// <returns></returns>
        public virtual Task AfterExecutionAsync(TwitchCommandContext ctx)
            => Task.Delay(0);
    }
}
