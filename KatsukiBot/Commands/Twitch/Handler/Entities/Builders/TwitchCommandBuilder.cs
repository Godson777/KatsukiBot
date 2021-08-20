using KatsukiBot.Commands.Twitch.Handler.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Entities.Builders {
    public class TwitchCommandBuilder {
        /// <summary>
        /// Gets the name set for this command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the aliases set for this command.
        /// </summary>
        public IReadOnlyList<string> Aliases { get; }
        private List<string> AliasList { get; }

        /// <summary>
        /// Gets the description set for this command.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets whether this command will be hidden or not.
        /// </summary>
        public bool IsHidden { get; private set; }

        /// <summary>
        /// Gets the execution checks defined for this command.
        /// </summary>
        public IReadOnlyList<TwitchCheckBaseAttribute> ExecutionChecks { get; }
        private List<TwitchCheckBaseAttribute> ExecutionCheckList { get; }

        /// <summary>
        /// Gets the collection of this command's overloads.
        /// </summary>
        public IReadOnlyList<TwitchCommandOverloadBuilder> Overloads { get; }
        private List<TwitchCommandOverloadBuilder> OverloadList { get; }
        private HashSet<string> OverloadArgumentSets { get; }

        /// <summary>
        /// Gets the module on which this command is to be defined.
        /// </summary>
        public ICommandModule Module { get; }

        /// <summary>
        /// Gets custom attributes defined on this command.
        /// </summary>
        public IReadOnlyList<Attribute> CustomAttributes { get; }
        private List<Attribute> CustomAttributeList { get; }

        /// <summary>
        /// Creates a new module-less command builder.
        /// </summary>
        public TwitchCommandBuilder()
            : this(null) { }

        /// <summary>
        /// Creates a new command builder.
        /// </summary>
        /// <param name="module">Module on which this command is to be defined.</param>
        public TwitchCommandBuilder(ICommandModule module) {
            this.AliasList = new List<string>();
            this.Aliases = new ReadOnlyCollection<string>(this.AliasList);

            this.ExecutionCheckList = new List<TwitchCheckBaseAttribute>();
            this.ExecutionChecks = new ReadOnlyCollection<TwitchCheckBaseAttribute>(this.ExecutionCheckList);

            this.OverloadArgumentSets = new HashSet<string>();
            this.OverloadList = new List<TwitchCommandOverloadBuilder>();
            this.Overloads = new ReadOnlyCollection<TwitchCommandOverloadBuilder>(this.OverloadList);

            this.Module = module;

            this.CustomAttributeList = new List<Attribute>();
            this.CustomAttributes = new ReadOnlyCollection<Attribute>(this.CustomAttributeList);
        }

        /// <summary>
        /// Sets the name for this command.
        /// </summary>
        /// <param name="name">Name for this command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithName(string name) {
            if (name == null || name.ToCharArray().Any(xc => char.IsWhiteSpace(xc)))
                throw new ArgumentException("Command name cannot be null or contain any whitespace characters.", nameof(name));

            if (this.Name != null)
                throw new InvalidOperationException("This command already has a name.");

            if (this.AliasList.Contains(name))
                throw new ArgumentException("Command name cannot be one of its aliases.", nameof(name));

            this.Name = name;
            return this;
        }

        /// <summary>
        /// Adds aliases to this command.
        /// </summary>
        /// <param name="aliases">Aliases to add to the command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithAliases(params string[] aliases) {
            if (aliases == null || !aliases.Any())
                throw new ArgumentException("You need to pass at least one alias.", nameof(aliases));

            foreach (var alias in aliases)
                this.WithAlias(alias);

            return this;
        }

        /// <summary>
        /// Adds an alias to this command.
        /// </summary>
        /// <param name="alias">Alias to add to the command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithAlias(string alias) {
            if (alias.ToCharArray().Any(xc => char.IsWhiteSpace(xc)))
                throw new ArgumentException("Aliases cannot contain whitespace characters or null strings.", nameof(alias));

            if (this.Name == alias || this.AliasList.Contains(alias))
                throw new ArgumentException("Aliases cannot contain the command name, and cannot be duplicate.", nameof(alias));

            this.AliasList.Add(alias);
            return this;
        }

        /// <summary>
        /// Sets the description for this command.
        /// </summary>
        /// <param name="description">Description to use for this command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithDescription(string description) {
            this.Description = description;
            return this;
        }

        /// <summary>
        /// Sets whether this command is to be hidden.
        /// </summary>
        /// <param name="hidden">Whether the command is to be hidden.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithHiddenStatus(bool hidden) {
            this.IsHidden = hidden;
            return this;
        }

        /// <summary>
        /// Adds pre-execution checks to this command.
        /// </summary>
        /// <param name="checks">Pre-execution checks to add to this command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithExecutionChecks(params TwitchCheckBaseAttribute[] checks) {
            this.ExecutionCheckList.AddRange(checks.Except(this.ExecutionCheckList));
            return this;
        }

        /// <summary>
        /// Adds a pre-execution check to this command.
        /// </summary>
        /// <param name="check">Pre-execution check to add to this command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithExecutionCheck(TwitchCheckBaseAttribute check) {
            if (!this.ExecutionCheckList.Contains(check))
                this.ExecutionCheckList.Add(check);
            return this;
        }

        /// <summary>
        /// Adds overloads to this command. An executable command needs to have at least one overload.
        /// </summary>
        /// <param name="overloads">Overloads to add to this command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithOverloads(params TwitchCommandOverloadBuilder[] overloads) {
            foreach (var overload in overloads)
                this.WithOverload(overload);

            return this;
        }

        /// <summary>
        /// Adds an overload to this command. An executable command needs to have at least one overload.
        /// </summary>
        /// <param name="overload">Overload to add to this command.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithOverload(TwitchCommandOverloadBuilder overload) {
            if (this.OverloadArgumentSets.Contains(overload.ArgumentSet))
                throw new DuplicateOverloadException(this.Name, overload.Arguments.Select(x => x.Type).ToList(), overload.ArgumentSet);

            this.OverloadArgumentSets.Add(overload.ArgumentSet);
            this.OverloadList.Add(overload);

            return this;
        }

        /// <summary>
        /// Adds a custom attribute to this command. This can be used to indicate various custom information about a command.
        /// </summary>
        /// <param name="attribute">Attribute to add.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithCustomAttribute(Attribute attribute) {
            this.CustomAttributeList.Add(attribute);
            return this;
        }

        /// <summary>
        /// Adds multiple custom attributes to this command. This can be used to indicate various custom information about a command.
        /// </summary>
        /// <param name="attributes">Attributes to add.</param>
        /// <returns>This builder.</returns>
        public TwitchCommandBuilder WithCustomAttributes(params Attribute[] attributes) {
            foreach (var attr in attributes)
                this.WithCustomAttribute(attr);

            return this;
        }

        internal virtual TwitchCommand Build(TwitchCommandGroup parent) {
            var cmd = new TwitchCommand {
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

            return cmd;
        }
    }
}
