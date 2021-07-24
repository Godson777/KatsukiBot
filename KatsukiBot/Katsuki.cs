using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot { 
    class Katsuki {
        private const string Name = "Katsuki";
        private const string Version = "1.0";

        /// <summary>
        /// The tag used when emitting log events from the bot.
        /// </summary>
        public const string LOGTAG = "Katsuki";

        /// <summary>
        /// Gets the Discord client instance for this shard.
        /// </summary>
        public DiscordClient Discord { get; }

        /// <summary>
        /// Gets the CommandsNext instance.
        /// </summary>
        public CommandsNextExtension CommandsNext { get; }

        /// <summary>
        /// Gets the Interactivity instnace.
        /// </summary>
        public InteractivityExtension Interactivity { get; }

        /// <summary>
        /// Gets the ID of this shard.
        /// </summary>
        public int ShardID { get; }

        public Katsuki(Config config, int shardID) {
            ShardID = shardID;

            Discord = new DiscordClient(new DiscordConfiguration() {
                Token = config.Token,
                TokenType = TokenType.Bot,
                ShardCount = config.Shards,
                ShardId = ShardID,
                Intents = DiscordIntents.AllUnprivileged,

                AutoReconnect = true,
                ReconnectIndefinitely = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                LargeThreshold = 1500,

                MinimumLogLevel = LogLevel.Debug
            });

            CommandsNext = Discord.UseCommandsNext(new CommandsNextConfiguration {
                CaseSensitive = false,
                IgnoreExtraArguments = true,

                EnableMentionPrefix = true,
                StringPrefixes = new string[] { "k!" },

                EnableDefaultHelp = false
            });

            CommandsNext.RegisterCommands<TestCommands>();
        }

        public async Task StartAsync() {
            Discord.Logger.Log(LogLevel.Information, $"[{LOGTAG}-{ShardID}] Booting KekBot Shard.");
            await Discord.ConnectAsync();
        }
    }
}
