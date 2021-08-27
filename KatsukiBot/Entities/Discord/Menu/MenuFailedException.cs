using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Entities.Discord.Menu {
    [Serializable]
    internal class MenuFailedException : Exception {
        public MenuFailedException() : base("The menu failed to display due to failing checks.") {
        }

        public MenuFailedException(string? message) : base($"The menu failed to display due to a failing check. Message: {message}") {
        }

        public MenuFailedException(string? message, Exception? innerException) : base(message, innerException) {
        }
    }
}
