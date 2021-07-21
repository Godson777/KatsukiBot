﻿using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
