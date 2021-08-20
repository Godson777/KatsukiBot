using KatsukiBot.Commands.Twitch;
using KatsukiBot.Commands.Twitch.Handler;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace KatsukiBot {
    class KatsukiTwitch {
        public TwitchClient client;
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

            client.OnLog += Client_OnLog;
            client.OnJoinedChannel += Client_OnJoinedChannel;

            CommandHandler = new TwitchCommandExtension(new TwitchCommandConfiguration() {
                CaseSensitive = false,
                IgnoreExtraArguments = true,

                StringPrefixes = new string[] { "!" }
            });
            CommandHandler.Setup(client);

            CommandHandler.RegisterCommands<TwitchTestCommands>();
        }

        public void Activate() {
            client.Connect();
        }

        private void Client_OnLog(object sender, OnLogArgs e) {
            //Console.WriteLine($"{e.DateTime.ToString()}: {e.BotUsername} - {e.Data}");
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e) {
            //client.SendMessage(e.Channel, "I LIVE BITCH");
        }
    }
}
