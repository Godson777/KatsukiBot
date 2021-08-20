using DSharpPlus.Entities;
using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Converters {
    public class EnumConverter<T> : IArgumentConverter<T> where T : struct, IComparable, IConvertible, IFormattable {
        Task<Optional<T>> IArgumentConverter<T>.ConvertAsync(string value, TwitchCommandContext ctx) {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (!ti.IsEnum)
                throw new InvalidOperationException("Cannot convert non-enum value to an enum.");

            return Enum.TryParse(value, !ctx.Config.CaseSensitive, out T ev)
                ? Task.FromResult(Optional.FromValue(ev))
                : Task.FromResult(Optional.FromNoValue<T>());
        }
    }
}
