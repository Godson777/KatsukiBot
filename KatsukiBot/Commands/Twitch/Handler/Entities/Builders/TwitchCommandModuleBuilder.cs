using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KatsukiBot.Commands.Twitch.Handler.Entities.Builders {
    public sealed class TwitchCommandModuleBuilder {
        public Type Type { get; private set; }

        public ModuleLifespan Lifespan { get; private set; }

        public TwitchCommandModuleBuilder() { }

        public TwitchCommandModuleBuilder WithType(Type t) {
            if (!t.IsModuleCandidateType())
                throw new ArgumentException("Specified type is not a valid module type.", nameof(t));

            this.Type = t;
            return this;
        }

        public TwitchCommandModuleBuilder WithLifespan(ModuleLifespan lifespan) {
            this.Lifespan = lifespan;
            return this;
        }

        internal ICommandModule Build(IServiceProvider services) {
            return this.Lifespan switch {
                ModuleLifespan.Singleton => new SingletonCommandModule(this.Type, services),
                ModuleLifespan.Transient => new TransientCommandModule(this.Type),
                _ => throw new NotSupportedException("Module lifespans other than transient and singleton are not supported."),
            };
        }
    }
}
