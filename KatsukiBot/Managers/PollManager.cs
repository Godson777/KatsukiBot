using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using KatsukiBot.Exceptions;
using KatsukiBot.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KatsukiBot.Managers {
    class PollManager {

        private static PollManager _instance;
        //For reference: Dictionary<GuildID, Dictionary<MessageID, Poll>>
        public Dictionary<ulong, Dictionary<ulong, Poll>> polls { get; private set; } = new Dictionary<ulong, Dictionary<ulong, Poll>>();

        public static PollManager Get() {
            if (_instance == null) {
                _instance = new PollManager();
                //var e = Program.R.Table("Polls").Run(Program.Conn);
                //foreach (Object a in e) {
                    //do nothing for now bc debug
                //}
            }
            return _instance;
        }

        protected void RegisterPoll(ulong guildID, Poll poll) {
            if (!polls.TryGetValue(guildID, out var guildPolls)) {
                guildPolls = new Dictionary<ulong, Poll>();
                polls.TryAdd(guildID, guildPolls);
            }
            guildPolls.TryAdd(poll.MessageID, poll);
            //if ()
        }

        protected void UnregisterPoll(ulong guildID, Poll poll) {
            if (!polls.TryGetValue(guildID, out var guildPolls)) {
                //How the fuck did we get here then
                return;
            }

            guildPolls.Remove(poll.MessageID);
        }

        public static Poll.PollBuilder CreatePoll(DiscordGuild Guild) {
            return new Poll.PollBuilder(Guild);
        }

        public class Poll {
            public class PollBuilder {
                private ulong GuildID;
                private DiscordChannel Channel;
                private string Title;
                private List<string> Options = new List<string>();
                private List<DiscordSelectComponentOption> Selections = new List<DiscordSelectComponentOption>();
                //For Reference: Dictionary<UserID, VoteID>
                //"VoteID" is equivalent to the option in the array.
                public Dictionary<ulong, int> userVotes { get; private set; } = new Dictionary<ulong, int>();

                private DateTime CompletionTime;
                public PollBuilder(DiscordGuild Guild) {
                    GuildID = Guild.Id;
                }

                public PollBuilder SetTitle(string Title) {
                    this.Title = Title;
                    return this;
                }

                public PollBuilder SetDestination(DiscordChannel Destination) {
                    Channel = Destination;
                    return this;
                }

                public PollBuilder AddOption(string Option) {
                    if (Option == null) return this;
                    Options.Add(Option);
                    Selections.Add(new DiscordSelectComponentOption(Option, Selections.Count.ToString()));
                    return this;
                }

                public PollBuilder AddOptions(params string[] Options) {
                    foreach (var option in Options) {
                        AddOption(option);
                    }
                    return this;
                }

                public PollBuilder SetCompletionTime(DateTime ct) {
                    CompletionTime = ct;
                    return this;
                }

                public async Task Build() {
                    BuildChecks();
                    var msg = await Channel.SendMessageAsync(new DiscordMessageBuilder()
                        .WithContent($":bar_chart: {Title}")
                        .AddComponents(new DiscordSelectComponent("Poll", "Select an Option.", Selections, false, 1, 1)));
                    var poll = new Poll(Channel.Id, msg.Id, Title, Options.ToArray(), CompletionTime);
                    PollManager.Get().RegisterPoll(GuildID, poll);
                    await poll.Execute();
                }

                public async Task Build(InteractionContext ctx) {
                    //Setting channel since we skipped that with slash commands.
                    Channel = ctx.Channel;
                    //Skipping BuildChecks as the slash command has already ensured everything else was good.
                    var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .WithContent($":bar_chart: {Title}")
                        .AddComponents(new DiscordSelectComponent("Poll", "Select an Option.", Selections, false, 1, 1)));
                    var poll = new Poll(Channel.Id, msg.Id, Title, Options.ToArray(), CompletionTime);
                    PollManager.Get().RegisterPoll(GuildID, poll);
                    await poll.Execute();
                }

                private void BuildChecks() {
                    if (Channel == null) throw new PollBuildingException("Destination not set.");
                    if (Options.Count < 1) throw new PollBuildingException("Must have at least more than one option.");
                    if (Options.Count > 24) throw new PollBuildingException("Cannot have more than 24 options.");
                    if (Title == null) throw new PollBuildingException("Title not set.");
                    if (CompletionTime == null) throw new PollBuildingException("No Completion Time set.");
                }
            }
            [JsonProperty("Channel ID")]
            public ulong ChannelID { get; private set; }
            [JsonProperty("Message ID")]
            public ulong MessageID { get; private set; }
            public string Title { get; private set; }
            public string[] Options { get; private set; }
            public int[] Votes { get; private set; }
            [JsonProperty("Completion Time")]
            public DateTime CompletionTime { get; private set; }
            [JsonProperty("Cast Votes")]
            public Dictionary<ulong, int> UserVotes { get; private set; }

            private CancellationTokenSource CancelSource;

            private Poll(ulong chID, ulong mID, string title, string[] options, DateTime ct) {
                ChannelID = chID;
                MessageID = mID;
                Title = title;
                Options = options;
                Votes = new int[options.Length];
                CompletionTime = ct;
                UserVotes = new Dictionary<ulong, int>();
                CancelSource = new CancellationTokenSource();
            }

            public string CastVote(DiscordUser user, int option) {
                if (UserVotes.ContainsKey(user.Id)) {
                    var old = UserVotes[user.Id];
                    if (old == option) {
                        return $"You have already cast your vote for {Options[option]}";
                    }
                    //Remove the user's old vote.
                    Votes[old]--;
                    //Adjust the user's entry in the dictionary
                    UserVotes[user.Id] = option;
                    Votes[option]++;
                    return $"You've changed your vote from {Options[old]} to {Options[option]}.";
                } else {
                    UserVotes.Add(user.Id, option);
                    Votes[option]++;
                    return $"Your vote for {Options[option]} has been cast.";
                }
            }

            public async Task Execute() {
                var c = CompletionTime.Subtract(DateTime.Now);
                try {
                    await Task.Delay(c, CancelSource.Token);
                } catch (TaskCanceledException _) {
                    //understandable have a nice day
                }
                var m = await Program.FindMessageWithKatsuki(ChannelID, MessageID);
                if (m == null) return;
                await m.ModifyAsync(new DiscordMessageBuilder().WithContent(GetResults()));
                PollManager.Get().UnregisterPoll(m.Channel.Guild.Id, this);
            }

            public void CancelPls() {
                CancelSource.Cancel();
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

    

}
