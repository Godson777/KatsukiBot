using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Entities {
    class Settings {
        public Settings(ulong GuildID) {
            this.GuildID = GuildID;
        }

        /// <summary>
        /// The ID for the guild these settings are bound to.
        /// </summary>
        [JsonProperty("Guild ID")]
        private ulong GuildID;
        /// <summary>
        /// The list of quotes belonging to this guild.
        /// </summary>
        [JsonProperty]
        public List<String> Quotes { get; set; } = new List<String>();

        /// <summary>
        /// Returns the settings of a specified guild.
        /// </summary>
        /// <param name="GuildID">The ID of the guild to get settings for.</param>
        /// <returns></returns>
        public static async Task<Settings> Get(ulong GuildID) => await Program.R.Table("Settings").Get(GuildID).RunAsync<Settings>(Program.Conn) ?? new Settings(GuildID);

        /// <summary>
        /// Returns the settings of a specified guild.
        /// </summary>
        /// <param name="Guild">The guild to get settings for.</param>
        public static async Task<Settings> Get(DiscordGuild Guild) => await Get(Guild.Id);

        /// <summary>
        /// Saves the settings back into the db.
        /// </summary>
        public async Task Save() {
            var r = Program.R;
            var table = r.Table("Settings");
            await r.Branch(
                //if table has entry for guild
                table.Get(GuildID).Eq(null).Not(),
                //then update old document with new data
                table.Get(GuildID).Update(this),
                //else insert new document
                table.Insert(this)
                ).RunAsync(Program.Conn);
        }

        public static async Task<string> GetQuoteForGuild(ulong GuildID) {
            var r = Program.R;
            var table = r.Table("Settings");
            return await r.Branch(
                //if table has entry for guild
                table.Get(GuildID).Eq(null).Not(),
                //then branch
                r.Branch(
                    //if entry has any quotes
                    table.Get(GuildID).GetField("Quotes").IsEmpty().Not(),
                    //then return a random quote
                    table.Get(GuildID).GetField("Quotes").Sample(1)[0],
                    //else return null
                    null),
                //else return null
                null
                ).RunAsync<string>(Program.Conn);
        }
    }
}
