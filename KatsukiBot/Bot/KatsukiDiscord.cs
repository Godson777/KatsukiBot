using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using KatsukiBot.Commands;
using KatsukiBot.Commands.Discord;
using KatsukiBot.Managers;
using KatsukiBot.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot { 
    class KatsukiDiscord {
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
        /// Gets the Slash Commands instance.
        /// </summary>
        public SlashCommandsExtension SlashCommands { get; }

        /// <summary>
        /// Gets the Interactivity instnace.
        /// </summary>
        public InteractivityExtension Interactivity { get; }

        /// <summary>
        /// Gets the ID of this shard.
        /// </summary>
        public int ShardID { get; }

        public KatsukiDiscord(Config config, int shardID) {
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

            Interactivity = Discord.UseInteractivity();
            CommandsNext.RegisterCommands<TestCommands>();
            CommandsNext.CommandErrored += HandleError;
            SlashCommands = Discord.UseSlashCommands();
            SlashCommands.RegisterCommands<SlashCommands>(441499112899084288L);
            SlashCommands.RegisterCommands<GeneralCommands>(441499112899084288L);
            SlashCommands.SlashCommandErrored += HandleError;
            Discord.ComponentInteractionCreated += HandlePoll;
        }
        private async Task HandleError(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs errorArgs) {
            var error = errorArgs.Exception;
            var ctx = errorArgs.Context;
            switch (error) {
                case ArgumentException _: {
                        // Remove cuz this swallows important errors >:(
                        break;
                        //var cmd = CommandsNext.FindCommand($"help {errorArgs.Command.QualifiedName}", out var args);
                        //var fakectx = CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, ctx.Message.Content, ctx.Prefix, cmd, args);
                        //await CommandsNext.ExecuteCommandAsync(fakectx);
                        //return;
                    }
                case SlashExecutionChecksFailedException e when e.FailedChecks.OfType<SlashRequireUserPermissionsAttribute>().Any(): {
                        var permCheck = e.FailedChecks.OfType<SlashRequireUserPermissionsAttribute>().First();
                        await ctx.ReplyBasicAsync($"Nya!? Hey there, you don't have the right permissions to do that! Try asking someone with the `{permCheck.Permissions.ToPermissionString()}` permission to do this!", true);
                        return;
                    }
            }

            await ctx.Channel.SendMessageAsync($"An error occured: {error.Message}");
            ctx.Client.Logger.Log(LogLevel.Error, $"[{LOGTAG}-{ShardID}] User '{ctx.User.Username}#{ctx.User.Discriminator}' ({ctx.User.Id}) tried to execute '{errorArgs.Context.CommandName ?? "UNKNOWN COMMAND?"}' " +
                $"but failed with {error}");
        }

        private async Task HandlePoll(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e) {
            if (e.Id == "Poll") {
                var manager = PollManager.Get();
                //This will almost always return true
                if (manager.polls.TryGetValue(e.Guild.Id, out var guildPolls)) {
                    if (guildPolls.TryGetValue(e.Message.Id, out var poll)) {
                        //Because polls can only have one option selected, we only grab the first value in e.Values
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().WithContent(poll.CastVote(e.User, int.Parse(e.Values[0]))).AsEphemeral(true));
                    }
                }
            }
        }

        private async Task HandleError(CommandsNextExtension cnext, CommandErrorEventArgs errorArgs) {
            var error = errorArgs.Exception;
            var ctx = errorArgs.Context;
            switch (error) {
                case ArgumentException _: {
                        // Remove cuz this swallows important errors >:(
                        break;
                        //var cmd = CommandsNext.FindCommand($"help {errorArgs.Command.QualifiedName}", out var args);
                        //var fakectx = CommandsNext.CreateFakeContext(ctx.Member, ctx.Channel, ctx.Message.Content, ctx.Prefix, cmd, args);
                        //await CommandsNext.ExecuteCommandAsync(fakectx);
                        //return;
                    }
            }

            await ctx.Channel.SendMessageAsync($"An error occured: {error.Message}");
            ctx.Client.Logger.Log(LogLevel.Error, $"[{LOGTAG}-{ShardID}] User '{ctx.User.Username}#{ctx.User.Discriminator}' ({ctx.User.Id}) tried to execute '{errorArgs.Command?.QualifiedName ?? "UNKNOWN COMMAND?"}' " +
                $"but failed with {error}");
        }

        public async Task StartAsync() {
            Discord.Logger.Log(LogLevel.Information, $"[{LOGTAG}-{ShardID}] Booting Katsuki Shard.");
            await Discord.ConnectAsync();
        }
    }
}
