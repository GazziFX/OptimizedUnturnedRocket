using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using UnityEngine;
using Rocket.API;
using System.Text.RegularExpressions;
using System.Reflection;
using Rocket.Core.Utils;
using Rocket.Core.Serialization;
using Rocket.Core.Assets;
using Rocket.API.Serialisation;
using System.Threading;
using System.Threading.Tasks;

namespace Rocket.Core.Commands
{
    public class RocketCommandManager
    {
        private readonly List<RegisteredRocketCommand> commands = new List<RegisteredRocketCommand>();
        internal List<RocketCommandCooldown> cooldown = new List<RocketCommandCooldown>();
        public ReadOnlyCollection<RegisteredRocketCommand> Commands { get; internal set; }
        private XMLFileAsset<RocketCommands> commandMappings;

        public delegate void ExecuteCommand(IRocketPlayer player, IRocketCommand command, ref bool cancel);
        public event ExecuteCommand OnExecuteCommand;

        /*class AsyncCommand
        {
            internal bool Execute()
            {
                return R.Commands.Execute(caller, command);
            }
            internal IRocketPlayer caller;
            internal string command;
        }*/

        internal void Reload()
        {
            commandMappings.Load();
            checkCommandMappings();
        }

        internal void Awake()
        {
            Commands = commands.AsReadOnly();
            commandMappings = new XMLFileAsset<RocketCommands>(Environment.CommandsFile);
            checkCommandMappings();
            R.Plugins.OnPluginsLoaded += Plugins_OnPluginsLoaded;
        }

        private void checkCommandMappings()
        {
            commandMappings.Instance.CommandMappings = commandMappings.Instance.CommandMappings.Distinct(new CommandMappingComparer()).ToList();
            checkDuplicateCommandMappings();
        }

        private void checkDuplicateCommandMappings(string classname = null) {
            foreach (CommandMapping mapping in (classname == null) ? commandMappings.Instance.CommandMappings : commandMappings.Instance.CommandMappings.Where(cm => cm.Class == classname))
            {
                string n = mapping.Name.ToLower();
                string c = mapping.Class.ToLower();

                if (mapping.Enabled)
                    foreach (CommandMapping other in commandMappings.Instance.CommandMappings)
                    {
                        if (other.Enabled && other.Name.ToLower() == n && other.Class.ToLower() != c)
                        {
                            Logging.Logger.Log("Other mapping to: " + other.Class + " / " + mapping.Class);
                            if (other.Priority > mapping.Priority)
                            {
                                mapping.Enabled = false;
                            }
                            else
                            {
                                other.Enabled = false;
                            }
                        }
                    }
            }
            commandMappings.Save();
        }

        private void Plugins_OnPluginsLoaded()
        {
            commandMappings.Save();
        }

        private IRocketCommand GetCommand(IRocketCommand command)
        {
           return GetCommand(command.Name);
        }

        public IRocketCommand GetCommand(string command)
        {
            IRocketCommand foundCommand = commands.FirstOrDefault(c => c.Name.Equals(command, StringComparison.OrdinalIgnoreCase));
            if (foundCommand == null) return commands.FirstOrDefault(c => c.Aliases.Contains(command, StringComparer.OrdinalIgnoreCase));
            return foundCommand;
        }

        private static string getCommandIdentity(IRocketCommand command,string name)
        {
            if (command is RocketAttributeCommand)
            {
                return ((RocketAttributeCommand)command).Method.ReflectedType.FullName+"/"+ name;
            }
            else if(command.GetType().ReflectedType != null)
            {
                return command.GetType().ReflectedType.FullName + "/" + name;
            }
            else
            {
                return command.GetType().FullName+"/"+ name;
            }
        }

        private static Type getCommandType(IRocketCommand command)
        {
            if (command is RocketAttributeCommand)
            {
                return ((RocketAttributeCommand)command).Method.ReflectedType;
            }
            else if (command.GetType().ReflectedType != null)
            {
                return command.GetType().ReflectedType;
            }
            else
            {
                return command.GetType();
            }
        }



        public class CommandMappingComparer : IEqualityComparer<CommandMapping>
        {
            public bool Equals(CommandMapping x, CommandMapping y)
            {
                return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(x.Class, y.Class, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(CommandMapping obj)
            {
                return (obj.Name+obj.Class).ToLower().GetHashCode();
            }
        }
        public bool Register(IRocketCommand command)
        {
            Register(command, null);
            return true;
        }

        public void Register(IRocketCommand command, string alias)
        {
            Register(command, alias, CommandPriority.Normal);
        }

        public void Register(IRocketCommand command, string alias, CommandPriority priority)
        {
            string name = command.Name;
            if (alias != null) name = alias;
            string className = getCommandIdentity(command,name);


            //Add CommandMapping if not already existing
            /*if(commandMappings.Instance.CommandMappings.Where(m => m.Class == className && m.Name == name).FirstOrDefault() == null){
                commandMappings.Instance.CommandMappings.Add(new CommandMapping(name,className,true,priority));
            }*/
            var mappings = commandMappings.Instance.CommandMappings;
            foreach (var map in mappings)
            {
                if (map.Class == className && map.Name == name)
                    goto BREAK;
            }
            mappings.Add(new CommandMapping(name, className, true, priority));
            BREAK:
            checkDuplicateCommandMappings(className);

            foreach(CommandMapping mapping in mappings)
            {
                if (mapping.Enabled && mapping.Class == className)
                {
                    commands.Add(new RegisteredRocketCommand(mapping.Name.ToLower(), command));
                    Logging.Logger.Log("[registered] /" + mapping.Name.ToLower() + " (" + mapping.Class + ")", ConsoleColor.Green);
                }
            }
        }

        public void DeregisterFromAssembly(Assembly assembly)
        {
            var cmds = commands;
            for (int i = cmds.Count - 1; i >= 0; i--)
            {
                if (cmds[i].Command.GetType().Assembly.Equals(assembly))
                    cmds.RemoveAt(i);
            }
        }

        public double GetCooldown(IRocketPlayer player, IRocketCommand command)
        {
            //RocketCommandCooldown c = cooldown.Where(rc => rc.Command == command && rc.Player.Equals(player)).FirstOrDefault();
            foreach (var c in cooldown)
            {
                if (c.Command == command && c.Player.Equals(player))
                {
                    double timeSinceExecution = (DateTime.Now - c.CommandRequested).TotalSeconds;
                    if (c.ApplyingPermission.Cooldown <= timeSinceExecution)
                    {
                        //Cooldown has it expired
                        cooldown.Remove(c);
                        return -1;
                    }
                    else
                    {
                        return c.ApplyingPermission.Cooldown - timeSinceExecution;
                    }
                }
            }
            return -1;
        }

        public void SetCooldown(IRocketPlayer player, IRocketCommand command)
        {
            /*List<Permission> applyingPermissions = R.Permissions.GetPermissions(player, command);
            Permission cooldownPermission = applyingPermissions.Where(p => p.Cooldown != 0).OrderByDescending(p => p.Cooldown).FirstOrDefault();*/
            Permission perm = null;
            foreach (var p in R.Permissions.GetPermissions(player, command))
            {
                if (p.Cooldown != 0 && (perm == null || p.Cooldown > perm.Cooldown))
                {
                    perm = p;
                }
            }
            if (perm != null)
            {
                cooldown.Add(new RocketCommandCooldown(player, command, perm));
            }
        }

        public bool Execute(IRocketPlayer player, string command)
        {
            /*if (Thread.CurrentThread != SDG.Unturned.ThreadUtil.gameThread)
            {
                var cmd = new AsyncCommand();
                cmd.caller = player;
                cmd.command = command;
                var task = new Task<bool>(cmd.Execute);
                TaskDispatcher.QueueOnMainThread(task.Start);
                return task.Result;
            }*/
            command = command.TrimStart('/');
            string[] commandParts = Regex.Matches(command, @"[\""](.+?)[\""]|([^ ]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture).Cast<Match>().Select(m => m.Value.Trim('"').Trim()).ToArray();

            if (commandParts.Length != 0)
            {
                IRocketCommand rocketCommand = GetCommand(commandParts[0]);
                if (rocketCommand != null)
                {
                    if (player == null) player = new ConsolePlayer();

                    if (rocketCommand.AllowedCaller == AllowedCaller.Player && player is ConsolePlayer)
                    {
                        Logging.Logger.Log("This command can't be called from console");
                        return false;
                    }
                    if (rocketCommand.AllowedCaller == AllowedCaller.Console && !(player is ConsolePlayer))
                    {
                        Logging.Logger.Log("This command can only be called from console");
                        return false;
                    }
                    if (GetCooldown(player, rocketCommand) != -1)
                    {
                        Logging.Logger.Log("This command is still on cooldown");
                        return false;
                    }

                    string[] parameters;
                    if (commandParts.Length == 1)
                        parameters = Array.Empty<string>();
                    else
                    {
                        parameters = new string[commandParts.Length - 1];
                        Array.Copy(commandParts, 1, parameters, 0, parameters.Length);
                    }

                    try
                    {
                        bool cancelCommand = false;
                        if (OnExecuteCommand != null)
                        {
                            foreach (var handler in OnExecuteCommand.GetInvocationList())
                            {
                                try
                                {
                                    ((ExecuteCommand)handler)(player, rocketCommand, ref cancelCommand);
                                }
                                catch (Exception ex)
                                {
                                    Logging.Logger.LogException(ex);
                                }
                            }
                        }
                        if (!cancelCommand)
                        {
                            try
                            {
                                rocketCommand.Execute(player, parameters);
                                if (!player.HasPermission("*")) { SetCooldown(player, rocketCommand); }
                            }
                            catch (NoPermissionsForCommandException ex)
                            {
                                Logging.Logger.LogWarning(ex.Message);
                            }
                            catch (WrongUsageOfCommandException)
                            {
                                //
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Logger.LogError("An error occured while executing " + rocketCommand.Name + " [" + string.Join(", ", parameters) + "]: " + ex.ToString());
                    }
                    return true;
                }
            }

            return false;
        }

        public void RegisterFromAssembly(Assembly assembly)
        {
            foreach (Type commandType in RocketHelper.NewGetTypesFromInterface(assembly, typeof(IRocketCommand)))
            {
                if(commandType.GetConstructor(Type.EmptyTypes) != null)
                {
                    IRocketCommand command = (IRocketCommand)Activator.CreateInstance(commandType);
                    Register(command);

                    foreach(string alias in command.Aliases)
                    {
                        Register(command,alias);
                    }
                }
            }

            Type plugin = RocketHelper.NewGetTypeFromInterface(assembly, typeof(IRocketPlugin));
            if (plugin != null)
            {
                MethodInfo[] methodInfos = plugin.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (MethodInfo method in methodInfos)
                {
                    RocketCommandAttribute commandAttribute = (RocketCommandAttribute)Attribute.GetCustomAttribute(method, typeof(RocketCommandAttribute));
                    RocketCommandAliasAttribute[] commandAliasAttributes = (RocketCommandAliasAttribute[])Attribute.GetCustomAttributes(method, typeof(RocketCommandAliasAttribute));
                    RocketCommandPermissionAttribute[] commandPermissionAttributes = (RocketCommandPermissionAttribute[])Attribute.GetCustomAttributes(method, typeof(RocketCommandPermissionAttribute));

                    if (commandAttribute != null)
                    {
                        List<string> Permissions = new List<string>();
                        List<string> Aliases = new List<string>();

                        if (commandAliasAttributes != null)
                        {
                            foreach (RocketCommandAliasAttribute commandAliasAttribute in commandAliasAttributes)
                            {
                                Aliases.Add(commandAliasAttribute.Name);
                            }
                        }

                        if (commandPermissionAttributes != null)
                        {
                            foreach (RocketCommandPermissionAttribute commandPermissionAttribute in commandPermissionAttributes)
                            {
                                Aliases.Add(commandPermissionAttribute.Name);
                            }
                        }

                        IRocketCommand command = new RocketAttributeCommand(commandAttribute.Name, commandAttribute.Help, commandAttribute.Syntax, commandAttribute.AllowedCaller, Permissions, Aliases, method);
                        Register(command);
                        foreach (string alias in command.Aliases)
                        {
                            Register(command, alias);
                        }
                    }
                }
            }
        }
        
        public class RegisteredRocketCommand : IRocketCommand
        {
            public Type Type;
            public IRocketCommand Command;
            private string name;

            public RegisteredRocketCommand(string name,IRocketCommand command)
            {
                this.name = name;
                Command = command;
                Type = getCommandType(command);
            }

            public List<string> Aliases
            {
                get
                {
                    return Command.Aliases;
                }
            }

            public AllowedCaller AllowedCaller
            {
                get
                {
                    return Command.AllowedCaller;
                }
            }

            public string Help
            {
                get
                {
                    return Command.Help;
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public List<string> Permissions
            {
                get
                {
                    return Command.Permissions;
                }
            }

            public string Syntax
            {
                get
                {
                    return Command.Syntax;
                }
            }

            public void Execute(IRocketPlayer caller, string[] command)
            {

                Command.Execute(caller, command);
            }
        }

        internal class RocketAttributeCommand : IRocketCommand
        {
            internal RocketAttributeCommand(string Name,string Help,string Syntax,AllowedCaller AllowedCaller,List<string>Permissions,List<string>Aliases,MethodInfo Method)
            {
                name = Name;
                help = Help;
                syntax = Syntax;
                permissions = Permissions;
                aliases = Aliases;
                method = Method;
                allowedCaller = AllowedCaller;
            }

            private List<string> aliases;
            public List<string> Aliases{ get { return aliases; } }

            private AllowedCaller allowedCaller;
            public AllowedCaller AllowedCaller { get { return allowedCaller; } }

            private string help;
            public string Help { get { return help; } }

            private string name;
            public string Name { get { return name; } }

            private string syntax;
            public string Syntax { get { return syntax; } }

            private List<string> permissions;
            public List<string> Permissions { get { return permissions; } }

            private MethodInfo method;
            public MethodInfo Method { get { return method; } }
            public void Execute(IRocketPlayer caller, string[] parameters)
            {
                ParameterInfo[] methodParameters = method.GetParameters();
                switch (methodParameters.Length)
                {
                    case 0:
                        method.Invoke(R.Plugins.GetPlugin(method.ReflectedType.Assembly), null);
                        break;
                    case 1:
                        if (methodParameters[0].ParameterType == typeof(IRocketPlayer))
                            method.Invoke(R.Plugins.GetPlugin(method.ReflectedType.Assembly), new object[] { caller });
                        else if (methodParameters[0].ParameterType == typeof(string[]))
                            method.Invoke(R.Plugins.GetPlugin(method.ReflectedType.Assembly), new object[] { parameters });
                        break;
                    case 2:
                        if (methodParameters[0].ParameterType == typeof(IRocketPlayer) && methodParameters[1].ParameterType == typeof(string[]))
                            method.Invoke(R.Plugins.GetPlugin(method.ReflectedType.Assembly), new object[] { caller, parameters });
                        else if (methodParameters[0].ParameterType == typeof(string[]) && methodParameters[1].ParameterType == typeof(IRocketPlayer))
                            method.Invoke(R.Plugins.GetPlugin(method.ReflectedType.Assembly), new object[] { parameters, caller });
                        break;
                }
            }
        }
    }
}
