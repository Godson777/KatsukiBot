using DSharpPlus.Entities;
using KatsukiBot.Commands.Twitch.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Converters {
    public class NullableConverter<T> : IArgumentConverter<Nullable<T>> where T : struct {
        async Task<Optional<Nullable<T>>> IArgumentConverter<Nullable<T>>.ConvertAsync(string value, TwitchCommandContext ctx) {
            if (!ctx.Config.CaseSensitive)
                value = value.ToLowerInvariant();

            if (value == "null")
                return Optional.FromValue<Nullable<T>>(null);

            if (ctx.TwitchCommands.ArgumentConverters.TryGetValue(typeof(T), out var cv)) {
                var cvx = cv as IArgumentConverter<T>;
                var val = await cvx.ConvertAsync(value, ctx).ConfigureAwait(false);
                return val.HasValue ? Optional.FromValue<Nullable<T>>(val.Value) : Optional.FromNoValue<Nullable<T>>();
            }

            return Optional.FromNoValue<Nullable<T>>();
        }
    }
}
