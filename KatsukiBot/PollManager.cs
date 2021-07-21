using KatsukiBot.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace KatsukiBot {
    class PollManager {

        private static PollManager _instance;
        private Dictionary<ulong, List<Poll>> polls = new Dictionary<ulong, List<Poll>>();

        public static PollManager Get() {
            if (_instance == null) _instance = new PollManager();
            return _instance;
        }
    }

    class Poll {
        [JsonProperty("Channel ID")]
        public ulong ChannelID { get; private set; }
        public string Title { get; private set; }
        public string[] Options { get; private set; }
        public int[] Votes { get; private set; }
        [JsonProperty("Completion Time")]
        public DateTime CompletionTime { get; private set; }

        public Poll(ulong chID, string title, string[] options, DateTime ct) {
            ChannelID = chID;
            Title = title;
            Options = options;
            CompletionTime = ct;
        }

        public async Task Execute() {
            await Task.Delay(CompletionTime.Subtract(DateTime.Now));
        }

        private string GetResults() {
            StringBuilder builder = new StringBuilder();
            var totalVotes = 0;
            var longestOption = 0;
            var resultsBarSize = 15;
            for (int i = 0; i < Options.Length; i++) {
                totalVotes += Votes[i];
                if (Options[i].Length > longestOption) longestOption = Options[i].Length;
            }
            //loop again dumbass lmaoooooo
            for (int i = 0; i < Options.Length; i++) {
                builder.AppendLine(Util.ToFixedWidth(Options[i], longestOption));
                var e = (int)Math.Ceiling(Convert.ToDouble(Votes[i] / totalVotes));
                builder.AppendLine($"[ {new String('#', e)} {new String(' ', resultsBarSize - e)}]");
            }

            return builder.ToString();
        }
    }
}
