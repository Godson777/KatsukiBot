using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Utils {
    static class Util {
        /// <summary>
        /// This only exists for type inference.
        /// </summary>
        [Serializable]
        public class PanicException : Exception {
            public PanicException() { }
            public PanicException(string message) : base(message) { }
            public PanicException(string message, Exception inner) : base(message, inner) { }
            protected PanicException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        /// <summary>
        /// Terminate the program, optionally with a message.
        /// Throw the returned exception to get better type inference from C#.
        /// </summary>
        internal static PanicException Panic(string msg = "") {
            Console.Error.WriteLine(msg);
            Environment.Exit(1);
            return new PanicException(message: msg);
        }

        /// <summary>
        /// Converts the string to a fixed-width string.
        /// </summary>
        /// <param name="s">String to fix the width of.</param>
        /// <param name="targetLength">Length that the string should be.</param>
        /// <returns>Adjusted string.</returns>
        public static string ToFixedWidth(this string s, int targetLength) {
            if (s == null)
                throw new NullReferenceException();

            if (s.Length < targetLength)
                return s.PadRight(targetLength, ' ');

            if (s.Length > targetLength)
                return s.Substring(0, targetLength);

            return s;
        }

        /// <summary>
        /// Use an arg resolver to parse an argument.
        /// </summary>
        internal static async Task<Arg?> ConvertArgAsync<Arg>(string value, CommandContext ctx) where Arg : struct {
            try {
                // God, this method sucks. And there's no alternative, as far as I can tell;
                // the property that contains the registered converters is private.
                return (Arg)await ctx.CommandsNext.ConvertArgument<Arg>(value, ctx);
            } catch (ArgumentException e) {
                if (e.Message != "Could not convert specified value to given type. (Parameter 'value')") {
                    Console.WriteLine($"Caught error from ConvertArgument: {e}");
                }
                return null;
            }
        }

        internal static IEnumerable<int> Range(int start = 0, int end = int.MaxValue, int step = 1) {
            for (int n = start; n < end; n += step) {
                yield return n;
            }
        }

        /// <summary>
        /// Use an arg resolver to parse an argument. Just make sure to include whatever it needs.
        /// </summary>
        internal static Task<Arg?> ConvertArgAsync<Arg>(string value, CommandsNextExtension cnext,
            DiscordMessage? msg = null, string prefix = "", Command? cmd = null, string? rawArgs = null)
            where Arg : struct => ConvertArgAsync<Arg>(value,
                cnext.CreateContext(msg: msg, prefix: prefix, cmd: cmd, rawArguments: rawArgs));

    }
}
