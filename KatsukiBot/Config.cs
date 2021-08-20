﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot {
    public class Config {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("database")]
        public string Db { get; private set; }
        [JsonProperty("dbUser")]
        public string DbUser { get; private set; }
        [JsonProperty("dbPassword")]
        public string DbPass { get; private set; }
        [JsonProperty("shards")]
        public int Shards { get; private set; } = 1;
        [JsonProperty("Twitch Username")]
        public string TwitchUsername { get; private set; }
        [JsonProperty("Twitch Access Token")]
        public string TwitchAccessToken { get; private set; }

        private static Config _instance;

        public static async Task<Config> Get() {
            if (_instance == null) {
                using var fs = File.OpenRead("Resource/Config/config.json");
                using var sr = new StreamReader(fs, new UTF8Encoding(false));
                return _instance = JsonConvert.DeserializeObject<Config>(await sr.ReadToEndAsync());
            } else return _instance;
        }

        public async void Save() {
            await File.WriteAllTextAsync("Resource/Config/config.json", JsonConvert.SerializeObject(this, Formatting.Indented), Encoding.UTF8);
        }
    }
}
