using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler {
    public class TwitchCommandErrorEventArgs : TwitchCommandEventArgs {
        public Exception Exception { get; internal set; }
    }
}
