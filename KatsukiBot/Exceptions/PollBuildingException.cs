using System;
using System.Collections.Generic;
using System.Text;

namespace KatsukiBot.Exceptions {
    class PollBuildingException : Exception {
        public PollBuildingException() : base("The poll failed to build due to failing checks.") { 
        }

        public PollBuildingException(string? message) : base($"The poll failed to build due to a failing check. Message: {message}") {
        }

        public PollBuildingException(string? message, Exception? innerException) : base(message, innerException) {
        }
    }
}
