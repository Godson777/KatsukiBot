using KatsukiBot.API;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using KatsukiBot.Utils;
using DSharpPlus.Entities;

namespace KatsukiBot {
    class Program { 
        public static RethinkDB R = RethinkDB.R;
        public static Connection Conn;
        public static Dictionary<int, Katsuki> Shards { get; private set; }


        static void Main(string[] args) {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] _) {
            var host = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder => {
                webBuilder.UseStartup<WebStartup>();
                webBuilder.UseUrls("http://*:6969");
            }).Build();

            Console.WriteLine("Katsuki is starting...");
            Console.WriteLine("[1/4] Loading Config... (Not Really, this was just stolen from KekBot lmao)");
            var config = await Config.Get();

            Console.WriteLine("[2/4] Connecting to RethinkDB...");
            try {
                Conn = R.Connection().User(config.DbUser, config.DbPass).Connect();
            } catch (Exception e) when (e is ReqlDriverError || e is System.Net.Sockets.SocketException) {
                Util.Panic("[RethinkDB] There was an error logging in, are you sure that RethinkDB is on, or that you typed the info correctly?");
            }
            Console.WriteLine("[RethinkDB] Connection Success!");

            Console.WriteLine("[RethinkDB] Checking Config for DB...");
            if (config.Db == null) {
                Util.Panic("[RethinkDB] There was no database to use provided. Make sure \"database\" is in your config.json.");
            }

            if (!(bool)R.DbList().Contains(config.Db).Run(Conn)) {
                R.DbCreate(config.Db).Run(Conn);
                Console.WriteLine("[RethinkDB] Database wasn't found, so it was created.");
            }
            Conn.Use(config.Db);
            Console.WriteLine("[RethinkDB] Connected to Database!");
            VerifyTables();

            Console.WriteLine("[3/4] Creating shards...");
            /* 
             * Realistically, we probably won't even need to shard. But it's basically just future-proofing in case I decide to make Katsuki public,
             * which, let's be real, I really have no intention of doing so.
             * If anything, I'd *maybe* let friends add her to their server. *Maybe.*
             */
            Shards = new Dictionary<int, Katsuki>();
            for (int i = 0; i < config.Shards; i++) {
                Shards[i] = new Katsuki(config, i);
            }

            Console.WriteLine("[4/4] Starting Web API...");
            await host.StartAsync();

            Console.WriteLine("Loading Completed! Booting Shards!");
            Console.WriteLine("----------------------------------");

            foreach (var (_, shard) in Shards) {
                await shard.StartAsync();
            }

            GC.Collect();

            await Task.Delay(-1);
        }

        private static void VerifyTables() {
            Console.WriteLine("[RethinkDB] Verifying that all required tables have been created...");
            if (!R.TableList().Contains("Settings").Run<bool>(Conn)) {
                Console.WriteLine("[RethinkDB] \"Settings\" table was not found, so it is being made.");
                R.TableCreate("Settings").OptArg("primary_key", "Guild ID").Run(Conn);
            }
            //if (!R.TableList().Contains("Polls").Run<bool>(Conn)) {
            //    Console.WriteLine("[RethinkDB] \"Polls\" table was not found, so it is being made.");
            //    R.TableCreate("Polls").OptArg("primary_key", "Channel ID");
            //}
            Console.WriteLine("[RethinkDB] Tables verified!");
        }

        /// <summary>
        /// This method searches for a message, specified with its ID.
        /// This method is also sharding safe, as it loops through each shard until the message has been successfully found.
        /// </summary>
        /// <param name="cID">The channel ID in which to find the message.</param>
        /// <param name="mID">The message ID used to find the message object.</param>
        /// <returns></returns>
        public async static Task<DiscordMessage> FindMessageWithKatsuki(ulong cID, ulong mID) { 
            foreach (Katsuki Kat in Shards.Values) {
                var ch = await Kat.Discord.GetChannelAsync(cID);
                if (ch == null) continue;
                var msg = await ch.GetMessageAsync(mID);
                return msg;
            }
            return null;
        }
    }
}
