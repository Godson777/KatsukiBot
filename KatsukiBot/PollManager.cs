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
        [JsonProperty("Message ID")]
        public ulong MessageID { get; private set; }
        public string Title { get; private set; }
        public string[] Options { get; private set; }
        public int[] Votes { get; private set; }
        [JsonProperty("Completion Time")]
        public DateTime CompletionTime { get; private set; }

        public Poll(ulong chID, ulong mID, string title, string[] options, DateTime ct) {
            ChannelID = chID;
            MessageID = mID;
            Title = title;
            Options = options;
            Votes = new int[options.Length];
            CompletionTime = ct;
        }

        public static Poll TestPoll(ulong chID, ulong mID) {
            var p = new Poll(chID, mID, "test", new string[] { "shit", "fuck", "piss", "wheee" }, DateTime.Now.AddSeconds(10));
            p.Votes[0] = 5;
            p.Votes[1] = 10;
            p.Votes[2] = 3;
            return p;
        }

        public async Task Execute() {
            var c = CompletionTime.Subtract(DateTime.Now);
            await Task.Delay(c);
            var m = await Program.FindMessageWithKatsuki(ChannelID, MessageID);
            if (m == null) return;
            await m.ModifyAsync(GetResults());
        }

        private string GetResults() {
            StringBuilder builder = new StringBuilder();
            var totalVotes = 0;
            var longestOption = 0;
            var resultsBarSize = 15;
            //loop to initialize some variables
            for (int i = 0; i < Options.Length; i++) {
                totalVotes += Votes[i];
                if (Options[i].Length > longestOption) longestOption = Options[i].Length;
            }
            //and now we loop again for other shit lmaooooooo
            for (int i = 0; i < Options.Length; i++) {
                var e = (int)Math.Ceiling(resultsBarSize * Convert.ToDouble(Votes[i]) / Convert.ToDouble(totalVotes));
                builder.AppendLine($"`{Util.ToFixedWidth(Options[i], longestOption)}` - `[{new String('#', e)}{new String(' ', resultsBarSize - e)}]` - {Votes[i]} Votes");
            }
            builder.AppendLine();
            builder.AppendLine($"Total Votes: {totalVotes}");

            return builder.ToString();
        }
    }
}
