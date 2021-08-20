using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Entities.Builders {
    public sealed class TwitchCommandGroupBuilder : TwitchCommandBuilder {
        /// <summary>
        /// Gets the list of child commands registered for this group.
        /// </summary>
        public IReadOnlyList<TwitchCommandBuilder> Children { get; }
        private List<TwitchCommandBuilder> ChildrenList { get; }

        /// <summary>
        /// Creates a new module-less command group builder.
        /// </summary>
        public TwitchCommandGroupBuilder()
            : this(null) { }

        /// <summary>
        /// Creates a new command group builder.
        /// </summary>
        /// <param name="module">Module on which this group is to be defined.</param>
        public TwitchCommandGroupBuilder(ICommandModule module)
            : base(module) {
            this.ChildrenList = new List<TwitchCommandBuilder>();
            this.Children = new ReadOnlyCollection<TwitchCommandBuilder>(this.ChildrenList);
        }

        /// <summary>
        /// Adds a command to the collection of child commands for this group.
        /// </summary>
        /// <param name="child">Command to add to the collection of child commands for this group.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandGroupBuilder WithChild(TwitchCommandBuilder child) {
            this.ChildrenList.Add(child);
            return this;
        }

        internal override TwitchCommand Build(TwitchCommandGroup parent) {
            var cmd = new TwitchCommandGroup {
                Name = this.Name,
                Description = this.Description,
                Aliases = this.Aliases,
                ExecutionChecks = this.ExecutionChecks,
                IsHidden = this.IsHidden,
                Parent = parent,
                Overloads = new ReadOnlyCollection<TwitchCommandOverload>(this.Overloads.Select(xo => xo.Build()).ToList()),
                Module = this.Module,
                CustomAttributes = this.CustomAttributes
            };

            var cs = new List<TwitchCommand>();
            foreach (var xc in this.Children)
                cs.Add(xc.Build(cmd));

            cmd.Children = new ReadOnlyCollection<TwitchCommand>(cs);
            return cmd;
        }
    }
}
