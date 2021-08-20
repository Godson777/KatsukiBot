﻿using DSharpPlus.Entities;
using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler {

    /// <summary>
    /// Argument converter abstract.
    /// </summary>
    public interface IArgumentConverter { }

    /// <summary>
    /// Represents a converter for specific argument type.
    /// </summary>
    /// <typeparam name="T">Type for which the converter is to be registered.</typeparam>
    public interface IArgumentConverter<T> : IArgumentConverter {
        /// <summary>
        /// Converts the raw value into the specified type.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="ctx">Context in which the value will be converted.</param>
        /// <returns>A structure containing information whether the value was converted, and, if so, the converted value.</returns>
        Task<Optional<T>> ConvertAsync(string value, TwitchCommandContext ctx);
    }
}
