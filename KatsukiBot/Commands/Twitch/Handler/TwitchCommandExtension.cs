using DSharpPlus.CommandsNext.Attributes;
using Emzi0767.Utilities;
using KatsukiBot.Commands.Twitch.Handler.Attributes;
using KatsukiBot.Commands.Twitch.Handler.Converters;
using KatsukiBot.Commands.Twitch.Handler.Entities;
using KatsukiBot.Commands.Twitch.Handler.Entities.Builders;
using KatsukiBot.Commands.Twitch.Handler;
using KatsukiBot.Commands.Twitch.Handler.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

namespace KatsukiBot.Commands.Twitch.Handler {
    /// <summary>
    /// This was haphazardly frankensteined together using some of CommandsNext's classes and shit.
    /// </summary>
    public class TwitchCommandExtension {
        public TwitchClient Client { get; internal set; }

        private TwitchCommandConfiguration Config { get; }

        private MethodInfo ConvertGeneric { get; }
        internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; }

        public IServiceProvider Services
            => this.Config.Services;

        internal TwitchCommandExtension(TwitchCommandConfiguration cfg) {
            this.Config = new TwitchCommandConfiguration(cfg);
            this.TopLevelCommands = new Dictionary<string, TwitchCommand>();
            this._registeredCommandsLazy = new Lazy<IReadOnlyDictionary<string, TwitchCommand>>(() => new ReadOnlyDictionary<string, TwitchCommand>(this.TopLevelCommands));

            this.ArgumentConverters = new Dictionary<Type, IArgumentConverter> {
                [typeof(string)] = new StringConverter(),
                [typeof(bool)] = new BoolConverter(),
                [typeof(sbyte)] = new Int8Converter(),
                [typeof(byte)] = new Uint8Converter(),
                [typeof(short)] = new Int16Converter(),
                [typeof(ushort)] = new Uint16Converter(),
                [typeof(int)] = new Int32Converter(),
                [typeof(uint)] = new Uint32Converter(),
                [typeof(long)] = new Int64Converter(),
                [typeof(ulong)] = new Uint64Converter(),
                [typeof(float)] = new Float32Converter(),
                [typeof(double)] = new Float64Converter(),
                [typeof(decimal)] = new Float128Converter(),
                [typeof(DateTime)] = new DateTimeConverter(),
                [typeof(DateTimeOffset)] = new DateTimeOffsetConverter(),
                [typeof(TimeSpan)] = new TimeSpanConverter(),
                [typeof(Uri)] = new UriConverter()
            };

            var ncvt = typeof(NullableConverter<>);
            var nt = typeof(Nullable<>);
            var cvts = this.ArgumentConverters.Keys.ToArray();
            foreach (var xt in cvts) {
                var xti = xt.GetTypeInfo();
                if (!xti.IsValueType)
                    continue;

                var xcvt = ncvt.MakeGenericType(xt);
                var xnt = nt.MakeGenericType(xt);
                if (this.ArgumentConverters.ContainsKey(xcvt))
                    continue;

                var xcv = Activator.CreateInstance(xcvt) as IArgumentConverter;
                this.ArgumentConverters[xnt] = xcv;
            }

            var t = typeof(TwitchCommandExtension);
            var ms = t.GetTypeInfo().DeclaredMethods;
            var m = ms.FirstOrDefault(xm => xm.Name == "ConvertArgument" && xm.ContainsGenericParameters && !xm.IsStatic && xm.IsPublic);
            this.ConvertGeneric = m;
        }

        #region TwitchClient Registration
        protected internal void Setup(TwitchClient client) {
            if (this.Client != null)
                throw new InvalidOperationException("What did I tell you?");

            this.Client = client;

            if (this.Config.UseDefaultCommandHandler)
                this.Client.OnMessageReceived += this.HandleCommandsAsync;
            //TODO: LOG SHIT I THINK?
            //else
                //this.Client.Logger.LogWarning(CommandsNextEvents.Misc, "Not attaching default command handler - if this is intentional, you can ignore this message");
        }
        #endregion

        #region Command Handling
        private async void HandleCommandsAsync(object sender, OnMessageReceivedArgs e) {
            var mpos = -1;

            if (this.Config.StringPrefixes?.Any() == true)
                foreach (var pfix in this.Config.StringPrefixes)
                    if (mpos == -1 && !string.IsNullOrWhiteSpace(pfix))
                        mpos = e.ChatMessage.GetStringPrefixLength(pfix, this.Config.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

            if (mpos == -1 && this.Config.PrefixResolver != null)
                mpos = await this.Config.PrefixResolver(e.ChatMessage).ConfigureAwait(false);

            if (mpos == -1)
                return;

            var pfx = e.ChatMessage.Message.Substring(0, mpos);
            var cnt = e.ChatMessage.Message.Substring(mpos);

            var __ = 0;
            var fname = cnt.ExtractNextArgument(ref __);

            var cmd = this.FindCommand(cnt, out var args);
            var ctx = this.CreateContext(e.ChatMessage, pfx, cmd, args);
            if (cmd == null) {
                //TODO: Log error when failing to find a command
                this.CommandErrored?.Invoke(this, new TwitchCommandErrorEventArgs { Context = ctx, Exception = new DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException(fname) });
                return;
            }

            _ = Task.Run(async () => await this.ExecuteCommandAsync(ctx).ConfigureAwait(false));
        }

        /// <summary>
        /// Finds a specified command by its qualified name, then separates arguments.
        /// </summary>
        /// <param name="commandString">Qualified name of the command, optionally with arguments.</param>
        /// <param name="rawArguments">Separated arguments.</param>
        /// <returns>Found command or null if none was found.</returns>
        public TwitchCommand FindCommand(string commandString, out string rawArguments) {
            rawArguments = null;

            var ignoreCase = !this.Config.CaseSensitive;
            var pos = 0;
            var next = commandString.ExtractNextArgument(ref pos);
            if (next == null)
                return null;

            if (!this.RegisteredCommands.TryGetValue(next, out var cmd)) {
                if (!ignoreCase)
                    return null;

                next = next.ToLowerInvariant();
                var cmdKvp = this.RegisteredCommands.FirstOrDefault(x => x.Key.ToLowerInvariant() == next);
                if (cmdKvp.Value == null)
                    return null;

                cmd = cmdKvp.Value;
            }

            if (!(cmd is TwitchCommandGroup)) {
                rawArguments = commandString.Substring(pos).Trim();
                return cmd;
            }

            while (cmd is TwitchCommandGroup) {
                var cm2 = cmd as TwitchCommandGroup;
                var oldPos = pos;
                next = commandString.ExtractNextArgument(ref pos);
                if (next == null)
                    break;

                if (ignoreCase) {
                    next = next.ToLowerInvariant();
                    cmd = cm2.Children.FirstOrDefault(x => x.Name.ToLowerInvariant() == next || x.Aliases?.Any(xx => xx.ToLowerInvariant() == next) == true);
                } else {
                    cmd = cm2.Children.FirstOrDefault(x => x.Name == next || x.Aliases?.Contains(next) == true);
                }

                if (cmd == null) {
                    cmd = cm2;
                    pos = oldPos;
                    break;
                }
            }

            rawArguments = commandString.Substring(pos).Trim();
            return cmd;
        }

        /// <summary>
        /// Creates a command execution context from specified arguments.
        /// </summary>
        /// <param name="msg">Message to use for context.</param>
        /// <param name="prefix">Command prefix, used to execute commands.</param>
        /// <param name="cmd">Command to execute.</param>
        /// <param name="rawArguments">Raw arguments to pass to command.</param>
        /// <returns>Created command execution context.</returns>
        public TwitchCommandContext CreateContext(ChatMessage msg, string prefix, TwitchCommand cmd, string rawArguments = null) {
            var ctx = new TwitchCommandContext {
                Client = this.Client,
                Command = cmd,
                Message = msg,
                Config = this.Config,
                RawArgumentString = rawArguments ?? "",
                Prefix = prefix,
                TwitchCommands = this,
                Services = this.Services
            };

            if (cmd != null && (cmd.Module is TransientCommandModule || cmd.Module == null)) {
                var scope = ctx.Services.CreateScope();
                ctx.ServiceScopeContext = new TwitchCommandContext.ServiceContext(ctx.Services, scope);
                ctx.Services = scope.ServiceProvider;
            }

            return ctx;
        }

        /// <summary>
        /// Executes specified command from given context.
        /// </summary>
        /// <param name="ctx">Context to execute command from.</param>
        /// <returns></returns>
        public async Task ExecuteCommandAsync(TwitchCommandContext ctx) {
            try {
                var cmd = ctx.Command;
                await this.RunAllChecksAsync(cmd, ctx).ConfigureAwait(false);

                var res = await cmd.ExecuteAsync(ctx).ConfigureAwait(false);

                //Dunno if this is necessary for the Twitch side? Needs investigating tbh
                if (res.IsSuccessful)
                    CommandExecuted?.Invoke(this, new TwitchCommandExecutionEventArgs { Context = res.Context });
                else
                    CommandErrored?.Invoke(this, new TwitchCommandErrorEventArgs { Context = res.Context, Exception = res.Exception });
            } catch (Exception ex) {
                //TODO: Error Logging again
                CommandErrored?.Invoke(this, new TwitchCommandErrorEventArgs { Context = ctx, Exception = ex });
            } finally {
                if (ctx.ServiceScopeContext.IsInitialized)
                    ctx.ServiceScopeContext.Dispose();
            }
        }

        private async Task RunAllChecksAsync(TwitchCommand cmd, TwitchCommandContext ctx) {
            if (cmd.Parent != null)
                await this.RunAllChecksAsync(cmd.Parent, ctx).ConfigureAwait(false);

            var fchecks = await cmd.RunChecksAsync(ctx).ConfigureAwait(false);
            if (fchecks.Any())
                throw new ChecksFailedException(cmd, ctx, fchecks);
        }
        #endregion

        #region Type Conversion
        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <param name="ctx">Context in which to convert to.</param>
        /// <returns>Converted object.</returns>
        public async Task<object> ConvertArgument<T>(string value, TwitchCommandContext ctx) {
            var t = typeof(T);
            if (!this.ArgumentConverters.ContainsKey(t))
                throw new ArgumentException("There is no converter specified for given type.", nameof(T));

            if (this.ArgumentConverters[t] is not IArgumentConverter<T> cv)
                throw new ArgumentException("Invalid converter registered for this type.", nameof(T));

            var cvr = await cv.ConvertAsync(value, ctx).ConfigureAwait(false);
            return !cvr.HasValue ? throw new ArgumentException("Could not convert specified value to given type.", nameof(value)) : cvr.Value;
        }

        /// <summary>
        /// Converts a string to specified type.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="ctx">Context in which to convert to.</param>
        /// <param name="type">Type to convert to.</param>
        /// <returns>Converted object.</returns>
        public async Task<object> ConvertArgument(string value, TwitchCommandContext ctx, Type type) {
            var m = this.ConvertGeneric.MakeGenericMethod(type);
            try {
                return await (m.Invoke(this, new object[] { value, ctx }) as Task<object>).ConfigureAwait(false);
            } catch (TargetInvocationException ex) {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Registers an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to register the converter.</typeparam>
        /// <param name="converter">Converter to register.</param>
        public void RegisterConverter<T>(IArgumentConverter<T> converter) {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter), "Converter cannot be null.");

            var t = typeof(T);
            var ti = t.GetTypeInfo();
            this.ArgumentConverters[t] = converter;

            if (!ti.IsValueType)
                return;

            var nullableConverterType = typeof(NullableConverter<>).MakeGenericType(t);
            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            if (this.ArgumentConverters.ContainsKey(nullableType))
                return;

            var nullableConverter = Activator.CreateInstance(nullableConverterType) as IArgumentConverter;
            this.ArgumentConverters[nullableType] = nullableConverter;
        }

        /// <summary>
        /// Unregisters an argument converter for specified type.
        /// </summary>
        /// <typeparam name="T">Type for which to unregister the converter.</typeparam>
        public void UnregisterConverter<T>() {
            var t = typeof(T);
            var ti = t.GetTypeInfo();
            if (this.ArgumentConverters.ContainsKey(t))
                this.ArgumentConverters.Remove(t);

            if (!ti.IsValueType)
                return;

            var nullableType = typeof(Nullable<>).MakeGenericType(t);
            if (!this.ArgumentConverters.ContainsKey(nullableType))
                return;

            this.ArgumentConverters.Remove(nullableType);
        }
        #endregion

        #region Command Registration
        /// <summary>
        /// Gets a dictionary of registered top-level commands.
        /// </summary>
        public IReadOnlyDictionary<string, TwitchCommand> RegisteredCommands
            => this._registeredCommandsLazy.Value;

        private Dictionary<string, TwitchCommand> TopLevelCommands { get; set; }
        private readonly Lazy<IReadOnlyDictionary<string, TwitchCommand>> _registeredCommandsLazy;

        /// <summary>
        /// Registers all commands from a given assembly. The command classes need to be public to be considered for registration.
        /// </summary>
        /// <param name="assembly">Assembly to register commands from.</param>
        public void RegisterCommands(Assembly assembly) {
            var types = assembly.ExportedTypes.Where(xt => {
                var xti = xt.GetTypeInfo();
                return xti.IsModuleCandidateType() && !xti.IsNested;
            });
            foreach (var xt in types)
                this.RegisterCommands(xt);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <typeparam name="T">Class which holds commands to register.</typeparam>
        public void RegisterCommands<T>() where T : TwitchBaseCommandModule {
            var t = typeof(T);
            this.RegisterCommands(t);
        }

        /// <summary>
        /// Registers all commands from a given command class.
        /// </summary>
        /// <param name="t">Type of the class which holds commands to register.</param>
        public void RegisterCommands(Type t) {
            if (t == null)
                throw new ArgumentNullException(nameof(t), "Type cannot be null.");

            if (!t.IsModuleCandidateType())
                throw new ArgumentNullException(nameof(t), "Type must be a class, which cannot be abstract or static.");

            this.RegisterCommands(t, null, null, out var tempCommands);

            if (tempCommands != null)
                foreach (var command in tempCommands)
                    this.AddToCommandDictionary(command.Build(null));
        }

        private void RegisterCommands(Type t, TwitchCommandGroupBuilder currentParent, IEnumerable<TwitchCheckBaseAttribute> inheritedChecks, out List<TwitchCommandBuilder> foundCommands) {
            var ti = t.GetTypeInfo();

            var lifespan = ti.GetCustomAttribute<ModuleLifespanAttribute>();
            var moduleLifespan = lifespan != null ? lifespan.Lifespan : ModuleLifespan.Singleton;

            var module = new TwitchCommandModuleBuilder()
                .WithType(t)
                .WithLifespan(moduleLifespan)
                .Build(this.Services);

            // restrict parent lifespan to more or equally restrictive
            if (currentParent?.Module is TransientCommandModule && moduleLifespan != ModuleLifespan.Transient)
                throw new InvalidOperationException("In a transient module, child modules can only be transient.");

            // check if we are anything
            var groupBuilder = new TwitchCommandGroupBuilder(module);
            var isModule = false;
            var moduleAttributes = ti.GetCustomAttributes();
            var moduleHidden = false;
            var moduleChecks = new List<TwitchCheckBaseAttribute>();

            foreach (var xa in moduleAttributes) {
                switch (xa) {
                    case GroupAttribute g:
                        isModule = true;
                        var moduleName = g.Name;
                        if (moduleName == null) {
                            moduleName = ti.Name;

                            if (moduleName.EndsWith("Group") && moduleName != "Group")
                                moduleName = moduleName.Substring(0, moduleName.Length - 5);
                            else if (moduleName.EndsWith("Module") && moduleName != "Module")
                                moduleName = moduleName.Substring(0, moduleName.Length - 6);
                            else if (moduleName.EndsWith("Commands") && moduleName != "Commands")
                                moduleName = moduleName.Substring(0, moduleName.Length - 8);
                        }

                        if (!this.Config.CaseSensitive)
                            moduleName = moduleName.ToLowerInvariant();

                        groupBuilder.WithName(moduleName);

                        if (inheritedChecks != null)
                            foreach (var chk in inheritedChecks)
                                groupBuilder.WithExecutionCheck(chk);

                        foreach (var mi in ti.DeclaredMethods.Where(x => x.IsCommandCandidate(out _) && x.GetCustomAttribute<GroupCommandAttribute>() != null))
                            groupBuilder.WithOverload(new TwitchCommandOverloadBuilder(mi));
                        break;

                    case AliasesAttribute a:
                        foreach (var xalias in a.Aliases)
                            groupBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                        break;

                    case HiddenAttribute h:
                        groupBuilder.WithHiddenStatus(true);
                        moduleHidden = true;
                        break;

                    case DescriptionAttribute d:
                        groupBuilder.WithDescription(d.Description);
                        break;

                    case TwitchCheckBaseAttribute c:
                        moduleChecks.Add(c);
                        groupBuilder.WithExecutionCheck(c);
                        break;

                    default:
                        groupBuilder.WithCustomAttribute(xa);
                        break;
                }
            }

            if (!isModule) {
                groupBuilder = null;
                if (inheritedChecks != null)
                    moduleChecks.AddRange(inheritedChecks);
            }

            // candidate methods
            var methods = ti.DeclaredMethods;
            var commands = new List<TwitchCommandBuilder>();
            var commandBuilders = new Dictionary<string, TwitchCommandBuilder>();
            foreach (var m in methods) {
                if (!m.IsCommandCandidate(out _))
                    continue;

                var attrs = m.GetCustomAttributes();
                if (attrs.FirstOrDefault(xa => xa is CommandAttribute) is not CommandAttribute cattr)
                    continue;

                var commandName = cattr.Name;
                if (commandName == null) {
                    commandName = m.Name;
                    if (commandName.EndsWith("Async") && commandName != "Async")
                        commandName = commandName.Substring(0, commandName.Length - 5);
                }

                if (!this.Config.CaseSensitive)
                    commandName = commandName.ToLowerInvariant();

                if (!commandBuilders.TryGetValue(commandName, out var commandBuilder)) {
                    commandBuilders.Add(commandName, commandBuilder = new TwitchCommandBuilder(module).WithName(commandName));

                    if (!isModule)
                        if (currentParent != null)
                            currentParent.WithChild(commandBuilder);
                        else
                            commands.Add(commandBuilder);
                    else
                        groupBuilder.WithChild(commandBuilder);
                }

                commandBuilder.WithOverload(new TwitchCommandOverloadBuilder(m));

                if (!isModule && moduleChecks.Any())
                    foreach (var chk in moduleChecks)
                        commandBuilder.WithExecutionCheck(chk);

                foreach (var xa in attrs) {
                    switch (xa) {
                        case AliasesAttribute a:
                            foreach (var xalias in a.Aliases)
                                commandBuilder.WithAlias(this.Config.CaseSensitive ? xalias : xalias.ToLowerInvariant());
                            break;

                        case TwitchCheckBaseAttribute p:
                            commandBuilder.WithExecutionCheck(p);
                            break;

                        case DescriptionAttribute d:
                            commandBuilder.WithDescription(d.Description);
                            break;

                        case HiddenAttribute h:
                            commandBuilder.WithHiddenStatus(true);
                            break;

                        default:
                            commandBuilder.WithCustomAttribute(xa);
                            break;
                    }
                }

                if (!isModule && moduleHidden)
                    commandBuilder.WithHiddenStatus(true);
            }

            // candidate types
            var types = ti.DeclaredNestedTypes
                .Where(xt => xt.IsModuleCandidateType() && xt.DeclaredConstructors.Any(xc => xc.IsPublic));
            foreach (var type in types) {
                this.RegisterCommands(type.AsType(),
                    groupBuilder,
                    !isModule ? moduleChecks : null,
                    out var tempCommands);

                if (isModule)
                    foreach (var chk in moduleChecks)
                        groupBuilder.WithExecutionCheck(chk);

                if (isModule && tempCommands != null)
                    foreach (var xtcmd in tempCommands)
                        groupBuilder.WithChild(xtcmd);
                else if (tempCommands != null)
                    commands.AddRange(tempCommands);
            }

            if (isModule && currentParent == null)
                commands.Add(groupBuilder);
            else if (isModule)
                currentParent.WithChild(groupBuilder);
            foundCommands = commands;
        }

        /// <summary>
        /// Builds and registers all supplied commands.
        /// </summary>
        /// <param name="cmds">Commands to build and register.</param>
        public void RegisterCommands(params TwitchCommandBuilder[] cmds) {
            foreach (var cmd in cmds)
                this.AddToCommandDictionary(cmd.Build(null));
        }

        /// <summary>
        /// Unregisters specified commands from CommandsNext.
        /// </summary>
        /// <param name="cmds">Commands to unregister.</param>
        public void UnregisterCommands(params TwitchCommand[] cmds) {
            if (cmds.Any(x => x.Parent != null))
                throw new InvalidOperationException("Cannot unregister nested commands.");

            var keys = this.RegisteredCommands.Where(x => cmds.Contains(x.Value)).Select(x => x.Key).ToList();
            foreach (var key in keys)
                this.TopLevelCommands.Remove(key);
        }

        private void AddToCommandDictionary(TwitchCommand cmd) {
            if (cmd.Parent != null)
                return;

            if (this.TopLevelCommands.ContainsKey(cmd.Name) || (cmd.Aliases != null && cmd.Aliases.Any(xs => this.TopLevelCommands.ContainsKey(xs))))
                throw new DuplicateCommandException(cmd.QualifiedName);

            this.TopLevelCommands[cmd.Name] = cmd;
            if (cmd.Aliases != null)
                foreach (var xs in cmd.Aliases)
                    this.TopLevelCommands[xs] = cmd;
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggered whenever a command executes successfully.
        /// </summary>
        public event EventHandler<TwitchCommandExecutionEventArgs> CommandExecuted;

        /// <summary>
        /// Triggered whenever a command throws an exception during execution.
        /// </summary>
        public event EventHandler<TwitchCommandErrorEventArgs> CommandErrored;

        private void OnCommandExecuted(TwitchCommandExecutionEventArgs e)
            => CommandExecuted.Invoke(this, e);

        private void OnCommandErrored(TwitchCommandErrorEventArgs e)
            => CommandErrored.Invoke(this, e);
        #endregion
    }
}
