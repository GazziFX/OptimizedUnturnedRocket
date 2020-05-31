using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Core;
using Rocket.Unturned.Chat;
using System.Collections.Generic;
using Rocket.API.Serialisation;

namespace Rocket.Unturned.Commands
{
    public class CommandP : IRocketCommand
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
            get { return "p"; }
        }

        public string Help
        {
            get { return "Sets a Rocket permission group of a specific player"; }
        }

        public string Syntax
        {
            get { return "<player> [group] | reload"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>() { "permissions" }; }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.p", "rocket.permissions" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if(command.Length == 1 && command[0].ToLower() == "reload" && caller.HasPermission("p.reload"))
            {
                R.Permissions.Reload();
                UnturnedChat.Say(caller, U.Translate("command_p_permissions_reload"));
                return;
            }




            int i;
            List<RocketPermissionsGroup> groups;
            string[] text;
            IRocketPlayer player;
            List<Permission> permissions;
            if (command.Length == 0 && !(caller is ConsolePlayer))
            {
                groups = R.Permissions.GetGroups(caller, true);
                text = new string[groups.Count];
                for (i = 0; i < text.Length; i++)
                {
                    text[i] = groups[i].DisplayName;
                }
                UnturnedChat.Say(caller, U.Translate("command_p_groups_private", "Your", string.Join(", ", text)));

                permissions = R.Permissions.GetPermissions(caller);
                text = new string[permissions.Count];
                for (i = 0; i < text.Length; i++)
                {
                    text[i] = permissions[i].Name + (permissions[i].Cooldown != 0 ? "(" + permissions[i].Cooldown + ")" : "");
                }
                UnturnedChat.Say(caller, U.Translate("command_p_permissions_private", "Your", string.Join(", ", text)));
            }
            else if (command.Length == 1)
            {
                player = command.GetUnturnedPlayerParameter(0);
                if (player == null) player = command.GetRocketPlayerParameter(0);
                if (player != null)
                {
                    groups = R.Permissions.GetGroups(caller, true);
                    text = new string[groups.Count];
                    for (i = 0; i < text.Length; i++)
                    {
                        text[i] = groups[i].DisplayName;
                    }
                    UnturnedChat.Say(caller, U.Translate("command_p_groups_private", player.DisplayName + "s", string.Join(", ", text)));

                    permissions = R.Permissions.GetPermissions(caller);
                    text = new string[permissions.Count];
                    for (i = 0; i < text.Length; i++)
                    {
                        text[i] = permissions[i].Name + (permissions[i].Cooldown != 0 ? "(" + permissions[i].Cooldown + ")" : "");
                    }
                    UnturnedChat.Say(caller, U.Translate("command_p_permissions_private", player.DisplayName + "s", string.Join(", ", text)));
                }
                else
                {
                    UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                    return;
                }
            }
            else if (command.Length == 3)
            {
                string c = command.GetStringParameter(0).ToLower();

                player = command.GetUnturnedPlayerParameter(1);
                if (player == null) player = command.GetRocketPlayerParameter(1);

                string groupName = command.GetStringParameter(2);

                switch (c)
                {
                    case "add":
                        if (caller.HasPermission("p.add") && player != null && groupName != null)
                        {
                            switch (Core.R.Permissions.AddPlayerToGroup(groupName, player))
                            {
                                case RocketPermissionsProviderResult.Success:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_player_added", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.DuplicateEntry:
                                    UnturnedChat.Say(caller, U.Translate("command_p_duplicate_entry", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.GroupNotFound:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_not_found", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.PlayerNotFound:
                                    UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", player.DisplayName, groupName));
                                    return;
                                default:
                                    UnturnedChat.Say(caller, U.Translate("command_p_unknown_error", player.DisplayName, groupName));
                                    return;
                            }
                        }
                        return;
                    case "remove":
                        if (caller.HasPermission("p.remove") && player != null && groupName != null)
                        {
                            switch (Core.R.Permissions.RemovePlayerFromGroup(groupName, player))
                            {
                                case RocketPermissionsProviderResult.Success:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_player_removed", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.DuplicateEntry:
                                    UnturnedChat.Say(caller, U.Translate("command_p_duplicate_entry", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.GroupNotFound:
                                    UnturnedChat.Say(caller, U.Translate("command_p_group_not_found", player.DisplayName, groupName));
                                    return;
                                case RocketPermissionsProviderResult.PlayerNotFound:
                                    UnturnedChat.Say(caller, U.Translate("command_p_player_not_found", player.DisplayName, groupName));
                                    return;
                                default:
                                    UnturnedChat.Say(caller, U.Translate("command_p_unknown_error", player.DisplayName, groupName));
                                    return;
                            }
                        }
                        return;
                    default:
                        UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                        return; //throw new WrongUsageOfCommandException(caller, this);
                }


            }
            else
            {
                UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
                return; //throw new WrongUsageOfCommandException(caller, this);
            }

            
         }
    }
}
