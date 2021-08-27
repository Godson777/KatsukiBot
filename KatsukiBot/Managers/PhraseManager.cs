using RethinkDb.Driver.Ast;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Managers {
    class PhraseManager {
        private static PhraseManager _instance;
        private Dictionary<string, Dictionary<string, string>> TwitchPhrases = new Dictionary<string, Dictionary<string, string>>(); //"TwitchPhrases" implying there'll be phrases for the Discord half. Maybe one day, but for now it's just futureproofing.
        
        
        private static readonly string Phrases = "Phrases"; //With the number of times this gets typed, it's just better to make it a variable to avoid mispelling it in the future.

        public static PhraseManager Get() {
            if (_instance == null) {
                _instance = new PhraseManager();
                var entries = Program.R.Table("TwitchPhrases").RunCursor<Entry>(Program.Conn);
                foreach (Entry e in entries) {
                    _instance.TwitchPhrases.Add(e.Channel, e.Phrases);
                }
                entries.Close();
            }
            return _instance;
        }
        /// <summary>
        /// Adds a phrase to Katsuki's phrase detection.
        /// </summary>
        /// <param name="Channel">The channel to add the phrase to.</param>
        /// <param name="Phrase">The phrase to respond to.</param>
        /// <param name="Response">The response for the phase.</param>
        public async void AddPhraseToChannel(string Channel, string Phrase, string Response) {
            var table = Program.R.Table("TwitchPhrases");
            var toAdd = Program.R.HashMap(Phrase, Response);
            if (await table.Contains(Channel).RunAsync<bool>(Program.Conn)) {
                await table.Get(Channel).Update(e => Program.R.HashMap(Phrases, e.G(Phrases).Append(toAdd))).RunAsync(Program.Conn);
            } else {
                await table.Insert(Program.R.HashMap("Channel", Channel)
                .With(Phrases, toAdd)).RunAsync(Program.Conn);
                TwitchPhrases.TryAdd(Channel, new Dictionary<string, string>());
            }
            TwitchPhrases[Channel].Add(Phrase, Response);
        }

        public async void RemovePhraseFromChannel(string Channel, string Phrase) {
            var table = Program.R.Table("TwitchPhrases");
            if (await table.Contains(Channel).RunAsync<bool>(Program.Conn)) {
                if (await table.Get(Channel).GetField(Phrases).HasFields(Phrase).RunAsync<bool>(Program.Conn)) {
                    await table.Get(Channel).Update(e => Program.R.HashMap(Phrases, e.G(Phrases).Without(Phrase))).RunAsync(Program.Conn);
                } //This could probably be more efficient, but for readability's sake, let's leave it this way.
            } //There should be no reason this method is called if there is no entry in the database.
        }

        public Dictionary<string, string> TestPull() {
            return Program.R.Table("TwitchPhrases").Get("GodsonTM").GetField(Phrases).Run<Dictionary<string, string>>(Program.Conn);
        }

        public bool DetectPhrase(string Channel, string Message, out string Response) {
            if (TwitchPhrases.TryGetValue(Channel, out var phrases)) {
                if (phrases.Keys.Any(p => Message.StartsWith(p))) {
                    var phrase = phrases.Keys.First(p => Message.StartsWith(p));
                    Response = phrases[phrase];
                    return true;
                }
            }
            Response = null;
            return false;
        }

        //This is literally only used just to pull entries out of the table so we can load the info into memory.
        private protected struct Entry {
            public string Channel;
            public Dictionary<string, string> Phrases;
        }
    }

    
}
