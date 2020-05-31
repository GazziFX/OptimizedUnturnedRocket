using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Reflection;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Core;
using Rocket.Unturned.Chat;

namespace Rocket.Unturned.Commands
{
    public class CommandRocket : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public string Name
        {
            get { return "rocket"; }
        }

        public string Help
        {
            get { return "Reloading Rocket or individual plugins"; }
        }

        public string Syntax
        {
            get { return "<plugins | reload> | <reload | unload | load> <plugin>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.info", "rocket.rocket" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                UnturnedChat.Say(caller, "Rocket v" + Assembly.GetExecutingAssembly().GetName().Version + " for Unturned v" + Provider.APP_VERSION);
                UnturnedChat.Say(caller, "https://rocketmod.net - 2017");
                return;
            }

            if (command.Length == 1)
            {
                switch (command[0].ToLower()) {
                    case "plugins":
                        if (caller != null && !caller.HasPermission("rocket.plugins")) return;
                        List<IRocketPlugin> plugins = R.Plugins.GetPlugins();
                        List<string> loaded = new List<string>();
                        List<string> unloaded = new List<string>();
                        List<string> failure = new List<string>();
                        List<string> cancelled = new List<string>();
                        foreach (var p in plugins)
                        {
                            var st = p.GetType().Assembly.GetName().Name;
                            switch (p.State)
                            {
                                case PluginState.Loaded:
                                    loaded.Add(st);
                                    break;
                                case PluginState.Unloaded:
                                    unloaded.Add(st);
                                    break;
                                case PluginState.Failure:
                                    failure.Add(st);
                                    break;
                                case PluginState.Cancelled:
                                    cancelled.Add(st);
                                    break;
                            }
                        }
                        UnturnedChat.Say(caller, U.Translate("command_rocket_plugins_loaded", string.Join(", ", loaded.ToArray())));
                        UnturnedChat.Say(caller, U.Translate("command_rocket_plugins_unloaded", string.Join(", ", unloaded.ToArray())));
                        UnturnedChat.Say(caller, U.Translate("command_rocket_plugins_failure", string.Join(", ", failure.ToArray())));
                        UnturnedChat.Say(caller, U.Translate("command_rocket_plugins_cancelled", string.Join(", ", cancelled.ToArray())));
                        break;
                    case "reload":
                        if (caller!=null && !caller.HasPermission("rocket.reload")) return;
                            UnturnedChat.Say(caller, U.Translate("command_rocket_reload"));
                            R.Reload();
                        break;
                }
            }

            if (command.Length == 2)
            {
                foreach (var pl in R.Plugins.GetPlugins())
                {
                    if (pl.Name.IndexOf(command[1], StringComparison.OrdinalIgnoreCase) == -1)
                        continue;
                    RocketPlugin p = (RocketPlugin)pl;
                    var name = p.GetType().Assembly.GetName().Name;
                    switch (command[0].ToLower())
                    {
                        case "reload":
                            if (caller != null && !caller.HasPermission("rocket.reloadplugin")) return;
                            if (p.State == PluginState.Loaded)
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_reload_plugin", name));
                                p.ReloadPlugin();
                            }
                            else
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_not_loaded", name));
                            }
                            break;
                        case "unload":
                            if (caller != null && !caller.HasPermission("rocket.unloadplugin")) return;
                            if (p.State == PluginState.Loaded)
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_unload_plugin", name));
                                p.UnloadPlugin();
                            }
                            else
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_not_loaded", name));
                            }
                            break;
                        case "load":
                            if (caller != null && !caller.HasPermission("rocket.loadplugin")) return;
                            if (p.State != PluginState.Loaded)
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_load_plugin", name));
                                p.LoadPlugin();
                            }
                            else
                            {
                                UnturnedChat.Say(caller, U.Translate("command_rocket_already_loaded", name));
                            }
                            break;
                    }
                    return;
                }
                UnturnedChat.Say(caller, U.Translate("command_rocket_plugin_not_found", command[1]));
            }
        }
    }
}
