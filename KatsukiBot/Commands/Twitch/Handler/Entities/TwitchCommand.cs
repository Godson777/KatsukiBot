using KatsukiBot.Commands.Twitch.Handler.Attributes;
using KatsukiBot.Commands.Twitch.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Entities {
    /// <summary>
    /// Represents a command.
    /// </summary>
    public class TwitchCommand {
       
            /// <summary>
            /// Gets this command's name.
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            /// Gets this command's qualified name (i.e. one that includes all module names).
            /// </summary>
            public string QualifiedName
                => this.Parent != null ? string.Concat(this.Parent.QualifiedName, " ", this.Name) : this.Name;

            /// <summary>
            /// Gets this command's aliases.
            /// </summary>
            public IReadOnlyList<string> Aliases { get; internal set; }

            /// <summary>
            /// Gets this command's parent module, if any.
            /// </summary>
            public TwitchCommandGroup Parent { get; internal set; }

            /// <summary>
            /// Gets this command's description.
            /// </summary>
            public string Description { get; internal set; }

            /// <summary>
            /// Gets whether this command is hidden.
            /// </summary>
            public bool IsHidden { get; internal set; }

            /// <summary>
            /// Gets a collection of pre-execution checks for this command.
            /// </summary>
            public IReadOnlyList<TwitchCheckBaseAttribute> ExecutionChecks { get; internal set; }

            /// <summary>
            /// Gets a collection of this command's overloads.
            /// </summary>
            public IReadOnlyList<TwitchCommandOverload> Overloads { get; internal set; }

            /// <summary>
            /// Gets the module in which this command is defined.
            /// </summary>
            public ICommandModule Module { get; internal set; }

            /// <summary>
            /// Gets the custom attributes defined on this command.
            /// </summary>
            public IReadOnlyList<Attribute> CustomAttributes { get; internal set; }

            internal TwitchCommand() { }

        public virtual async Task<TwitchCommandResult> ExecuteAsync(TwitchCommandContext ctx) {
            TwitchCommandResult res = default;
            try {
                var executed = false;
                foreach (var ovl in this.Overloads.OrderByDescending(x => x.Priority)) {
                    ctx.Overload = ovl;
                    var args = await TwitchCommandUtils.BindArguments(ctx, ctx.Config.IgnoreExtraArguments).ConfigureAwait(false);

                    if (!args.IsSuccessful)
                        continue;

                    ctx.RawArguments = args.Raw;

                    var mdl = ovl.InvocationTarget ?? this.Module?.GetInstance(ctx.Services);
                    if (mdl is TwitchBaseCommandModule bcmBefore)
                        await bcmBefore.BeforeExecutionAsync(ctx).ConfigureAwait(false);

                    args.Converted[0] = mdl;
                    var ret = (Task)ovl.Callable.DynamicInvoke(args.Converted);
                    await ret.ConfigureAwait(false);
                    executed = true;
                    res = new TwitchCommandResult {
                        IsSuccessful = true,
                        Context = ctx
                    };

                    if (mdl is TwitchBaseCommandModule bcmAfter)
                        await bcmAfter.AfterExecutionAsync(ctx).ConfigureAwait(false);
                    break;
                }

                if (!executed)
                    throw new ArgumentException("Could not find a suitable overload for the command.");
            } catch (Exception ex) {
                res = new TwitchCommandResult {
                    IsSuccessful = false,
                    Exception = ex,
                    Context = ctx
                };
            }

            return res;
        }

        /// <summary>
        /// Runs pre-execution checks for this command and returns any that fail for given context.
        /// </summary>
        /// <param name="ctx">Context in which the command is executed.</param>
        /// <param name="help">Whether this check is being executed from help or not. This can be used to probe whether command can be run without setting off certain fail conditions (such as cooldowns).</param>
        /// <returns>Pre-execution checks that fail for given context.</returns>
        public async Task<IEnumerable<TwitchCheckBaseAttribute>> RunChecksAsync(TwitchCommandContext ctx) {
            var fchecks = new List<TwitchCheckBaseAttribute>();
            if (this.ExecutionChecks != null && this.ExecutionChecks.Any())
                foreach (var ec in this.ExecutionChecks)
                    if (!await ec.ExecuteCheckAsync(ctx).ConfigureAwait(false))
                        fchecks.Add(ec);

            return fchecks;
        }
    }
}
