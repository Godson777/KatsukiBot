using KatsukiBot.Commands.Twitch;
using KatsukiBot.Commands.Twitch.Handler;
using KatsukiBot.Commands.Twitch.Handler.Attributes;
using KatsukiBot.Commands.Twitch.Handler;
using KatsukiBot.Commands.Twitch.Handler.Exceptions;
using KatsukiBot.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;
using TwitchLib.Client.Enums;

namespace KatsukiBot {
    class KatsukiTwitch {
        public TwitchClient client;
        //TODO: PubSub
        TwitchCommandExtension CommandHandler;

        public KatsukiTwitch(Config config) {
            var credentials = new ConnectionCredentials(config.TwitchUsername, config.TwitchAccessToken);
            var clientOptions = new ClientOptions {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, "GodsonTM");

            client.OnLog += Logger;
            client.OnJoinedChannel += MiscOnJoinedChannel;
            client.OnNewSubscriber += MiscOnNewSub;
            client.OnReSubscriber += MiscOnReSub;
            client.OnGiftedSubscription += MiscOnGiftSub;
            client.OnCommunitySubscription += MiscOnCommunitySub;
            client.OnRaidNotification += MiscOnRaid;
            client.OnBeingHosted += MiscOnHosted;
            client.OnMessageReceived += PhraseDetection;

            CommandHandler = new TwitchCommandExtension(new TwitchCommandConfiguration() {
                CaseSensitive = false,
                IgnoreExtraArguments = true,

                StringPrefixes = new string[] { "!" }
            });
            CommandHandler.Setup(client);
            CommandHandler.CommandErrored += OnError;

            CommandHandler.RegisterCommands<TwitchTestCommands>();
            CommandHandler.RegisterCommands<GeneralCommands>();
        }

        #region Event Handlers
        private void MiscOnHosted(object? sender, OnBeingHostedArgs e) {
            if (!e.BeingHostedNotification.IsAutoHosted) {
                client.SendMessage(e.BeingHostedNotification.Channel, $"Thanks for the host, {e.BeingHostedNotification.HostedByChannel}!");
            }
        }

        private void MiscOnRaid(object? sender, OnRaidNotificationArgs e) {
            client.SendMessage(e.Channel, $"N-Nya!? {e.RaidNotification.DisplayName} and their {e.RaidNotification.MsgParamViewerCount} viewers are raiding us! TAKE COVER!!!");
        }

        private void MiscOnCommunitySub(object? sender, OnCommunitySubscriptionArgs e) {
            var amount = e.GiftedSubscription.MsgParamMassGiftCount;
            client.SendMessage(e.Channel, $"{(e.GiftedSubscription.IsAnonymous ? "Someone" : e.GiftedSubscription.DisplayName)} just gifted {amount} {(amount > 1 ? "subs" : "sub")} to the community! How generous of them!");
        }

        private void MiscOnGiftSub(object? sender, OnGiftedSubscriptionArgs e) {
            if (e.GiftedSubscription.MsgParamRecipientUserName == client.TwitchUsername) {
                client.SendMessage(e.Channel, $"Nya!? {e.GiftedSubscription.DisplayName} has gifted a sub to... ME!? I CAN'T BELIEVE IT YOU'RE SO NICE AAAAAAA");
            } else {
                client.SendMessage(e.Channel, $"Nya!? {e.GiftedSubscription.DisplayName} has gifted a sub to{e.GiftedSubscription.MsgParamRecipientUserName}!");
            }
        }

        private void MiscOnReSub(object? sender, OnReSubscriberArgs e) {
            client.SendMessage(e.Channel, $"Nya!? {e.ReSubscriber.DisplayName} has resubbed{(e.ReSubscriber.SubscriptionPlan == SubscriptionPlan.Prime ? " using Prime Gaming" : "")}! They've been subbed for {e.ReSubscriber.Months} months.");
        }

        private void MiscOnNewSub(object? sender, OnNewSubscriberArgs e) {
            client.SendMessage(e.Channel, $"Nya!? {e.Subscriber.DisplayName} has subscribed{(e.Subscriber.SubscriptionPlan == SubscriptionPlan.Prime ? " using Prime Gaming" : "")}!");
        }

        private void OnError(object? sender, TwitchCommandErrorEventArgs errorArgs) {
            var error = errorArgs.Exception;
            var ctx = errorArgs.Context;
            switch (error) {
                case ChecksFailedException e when e.FailedChecks.OfType<RequireModeratorAttribute>().Any(): {
                        var permCheck = e.FailedChecks.OfType<RequireModeratorAttribute>().First();
                        ctx.Respond($"Nya!? Hey there, you can't do that! This command is for mods!");
                        return;
                    }
            }
        }

        private void PhraseDetection(object? sender, OnMessageReceivedArgs e) {
            var man = PhraseManager.Get();
            if (man.DetectPhrase(e.ChatMessage.Channel, e.ChatMessage.Message, out var response)) {
                client.SendMessage(e.ChatMessage.Channel, response);
            }
        }

        private void Logger(object? sender, OnLogArgs e) {
            //Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void MiscOnJoinedChannel(object? sender, OnJoinedChannelArgs e) {
            //client.SendMessage(e.Channel, "I LIVE BITCH");
        }
        #endregion

        public void Activate() {
            client.Connect();
        }
    }
}
