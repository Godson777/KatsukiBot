using KatsukiBot.Commands.Twitch.Handler.Entities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Api.V5.Models.Channels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace KatsukiBot.Commands.Twitch.Handler.EventArgs {
    public sealed class TwitchCommandContext {
        /// <summary>
        /// Gets the client which received the message.
        /// </summary>
        public TwitchClient Client { get; internal set; }

        /// <summary>
        /// Gets the message that triggered the execution.
        /// </summary>
        public ChatMessage Message { get; internal set; }

        /// <summary>
        /// Gets the channel in which the execution was triggered,
        /// </summary>
        public string Channel
            => this.Message.Channel;

        /// <summary>
        /// Gets the user who triggered the execution.
        /// </summary>
        public string User
            => this.Message.Username;

        /// <summary>
        /// Gets the CommandsNext service instance that handled this command.
        /// </summary>
        public TwitchCommandExtension TwitchCommands { get; internal set; }

        /// <summary>
        /// Gets the service provider for this CNext instance.
        /// </summary>
        public IServiceProvider Services { get; internal set; }

        /// <summary>
        /// Gets the command that is being executed.
        /// </summary>
        public TwitchCommand Command { get; internal set; }

        /// <summary>
        /// Gets the overload of the command that is being executed.
        /// </summary>
        public TwitchCommandOverload Overload { get; internal set; }

        /// <summary>
        /// Gets the list of raw arguments passed to the command.
        /// </summary>
        public IReadOnlyList<string> RawArguments { get; internal set; }

        /// <summary>
        /// Gets the raw string from which the arguments were extracted.
        /// </summary>
        public string RawArgumentString { get; internal set; }

        /// <summary>
        /// Gets the prefix used to invoke the command.
        /// </summary>
        public string Prefix { get; internal set; }

        internal TwitchCommandConfiguration Config { get; set; }

        internal ServiceContext ServiceScopeContext { get; set; }

        public void Respond(string content) {
            Client.SendMessage(Channel, content);
        }

        internal struct ServiceContext : IDisposable {
            public IServiceProvider Provider { get; }
            public IServiceScope Scope { get; }
            public bool IsInitialized { get; }

            public ServiceContext(IServiceProvider services, IServiceScope scope) {
                this.Provider = services;
                this.Scope = scope;
                this.IsInitialized = true;
            }

            public void Dispose() => this.Scope?.Dispose();
        }
    }
}
