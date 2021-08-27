using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Entities {
    public class Streamer {
        public string ApiKey { get; private set; }
        public ulong DiscordID { get; private set; }
        public ulong GuildID { get; private set; }
        /// <summary>
        /// The name of the streamer's channel.
        /// </summary>
        public string ChannelName { get; private set; }

        [JsonConstructor]
        private Streamer(string ApiKey, ulong DiscordID, ulong GuildID, string ChannelName) {
            this.ApiKey = ApiKey;
            this.DiscordID = DiscordID;
            this.GuildID = GuildID;
            this.ChannelName = ChannelName;
        }
        
        public async static Task<Streamer> GetStreamer(string ApiKey) {
            var table = Program.R.Table("Streamers");
            return await table.Get(ApiKey).RunAsync<Streamer>(Program.Conn);
        }

        public async static Task<Streamer> GetStreamerByDiscordID(ulong DiscordID) {
            var table = Program.R.Table("Streamers");
            var filtered = await table.Filter(e => e.GetField("DiscordID").Eq(DiscordID)).RunCursorAsync<Streamer>(Program.Conn);
            var array = filtered.ToList();
            filtered.Close();
            if (array.Count > 0) return array[0]; //There should never be two entries belonging to the same Discord ID. 
            else return null;
        }

        public async static Task<Streamer> GetStreamerByChannelName(string Channel) {
            var table = Program.R.Table("Streamers");
            var filtered = await table.Filter(e => e.GetField("ChannelName").Eq(Channel)).RunCursorAsync<Streamer>(Program.Conn);
            var array = filtered.ToList();
            filtered.Close();
            if (array.Count > 0) return array[0]; //There should never be two entries belonging to the same Channel. 
            else return null;
        }

        //public async static Task<List<String>>

        public async static void RegisterStreamer(string ApiKey, ulong DiscordID, ulong GuildID, string ChannelName) {
            var streamer = new Streamer(ApiKey, DiscordID, GuildID, ChannelName);
            var table = Program.R.Table("Streamers");
            await table.Insert(streamer).RunAsync(Program.Conn);
        }
    }
}
