using DSharpPlus.Entities;
using KatsukiBot.Commands.Twitch.Handler.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Converters {
    public class StringConverter : IArgumentConverter<string> {
        Task<Optional<string>> IArgumentConverter<string>.ConvertAsync(string value, TwitchCommandContext ctx)
            => Task.FromResult(Optional.FromValue(value));
    }

    public class UriConverter : IArgumentConverter<Uri> {
        Task<Optional<Uri>> IArgumentConverter<Uri>.ConvertAsync(string value, TwitchCommandContext ctx) {
            try {
                if (value.StartsWith("<") && value.EndsWith(">"))
                    value = value.Substring(1, value.Length - 2);

                return Task.FromResult(Optional.FromValue(new Uri(value)));
            } catch {
                return Task.FromResult(Optional.FromNoValue<Uri>());
            }
        }
    }
}
