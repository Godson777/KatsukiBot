using KatsukiBot.Commands.Twitch.Handler.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client.Models;

namespace KatsukiBot.Commands.Twitch.Handler {

    public delegate Task<int> PrefixResolverDelegate(ChatMessage msg);

    /// <summary>
    /// Represents a configuration for <see cref="CommandsNextExtension"/>.
    /// </summary>
    public sealed class TwitchCommandConfiguration {
        /// <summary>
        /// <para>Sets the string prefixes used for commands.</para>
        /// <para>Defaults to no value (disabled).</para>
        /// </summary>
        public IEnumerable<string> StringPrefixes { internal get; set; }

        /// <summary>
        /// <para>Sets the custom prefix resolver used for commands.</para>
        /// <para>Defaults to none (disabled).</para>
        /// </summary>
        public PrefixResolverDelegate PrefixResolver { internal get; set; } = null;

        /// <summary>
        /// <para>Sets whether strings should be matched in a case-sensitive manner.</para>
        /// <para>This switch affects the behaviour of default prefix resolver, command searching, and argument conversion.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool CaseSensitive { internal get; set; } = false;

        /// <summary>
        /// <para>Sets the service provider for this CommandsNext instance.</para>
        /// <para>Objects in this provider are used when instantiating command modules. This allows passing data around without resorting to static members.</para>
        /// <para>Defaults to null.</para>
        /// </summary>
        public IServiceProvider Services { internal get; set; } = new ServiceCollection().BuildServiceProvider(true);

        /// <summary>
        /// <para>Gets whether any extra arguments passed to commands should be ignored or not. If this is set to false, extra arguments will throw, otherwise they will be ignored.</para>
        /// <para>Defaults to false.</para>
        /// </summary>
        public bool IgnoreExtraArguments { internal get; set; } = false;

        /// <summary>
        /// <para>Gets or sets whether to automatically enable handling commands.</para>
        /// <para>If this is set to false, you will need to manually handle each incoming message and pass it to CommandsNext.</para>
        /// <para>Defaults to true.</para>
        /// </summary>
        public bool UseDefaultCommandHandler { internal get; set; } = true;

        /// <summary>
        /// Creates a new instance of <see cref="CommandsNextConfiguration"/>.
        /// </summary>
        public TwitchCommandConfiguration() { }

        /// <summary>
        /// Creates a new instance of <see cref="CommandsNextConfiguration"/>, copying the properties of another configuration.
        /// </summary>
        /// <param name="other">Configuration the properties of which are to be copied.</param>
        public TwitchCommandConfiguration(TwitchCommandConfiguration other) {
            this.CaseSensitive = other.CaseSensitive;
            this.PrefixResolver = other.PrefixResolver;
            this.IgnoreExtraArguments = other.IgnoreExtraArguments;
            this.UseDefaultCommandHandler = other.UseDefaultCommandHandler;
            this.Services = other.Services;
            this.StringPrefixes = other.StringPrefixes?.ToArray();
        }
    }
}
