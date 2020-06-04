using SDG.Unturned;
using UnityEngine;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.API.Extensions;

namespace Rocket.Unturned.Commands
{
    public class CommandTp : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }

        public string Name
        {
            get { return "tp"; }
        }

        public string Help
        {
            get { return "Teleports you to another player or location";}
        }

        public string Syntax
        {
            get { return "<player | place | x y z>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.tp", "rocket.teleport" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length != 1 && command.Length != 3)
            {
                UnturnedChat.Say(player, U.Translate("command_generic_invalid_parameter"));
                return; //throw new WrongUsageOfCommandException(caller, this);
            }

            if (player.Stance == EPlayerStance.DRIVING || player.Stance == EPlayerStance.SITTING)
            {
                UnturnedChat.Say(player, U.Translate("command_generic_teleport_while_driving_error"));
                return; //throw new WrongUsageOfCommandException(caller, this);
            }

            string cords;
            if (command.Length == 3)
            {
                var x = command.GetFloatParameter(0);
                var y = command.GetFloatParameter(1);
                var z = command.GetFloatParameter(2);
                if (x != null && y != null && z != null)
                {
                    player.Teleport(new Vector3((float)x, (float)y, (float)z), player.Rotation);
                    cords = (int)x + "," + (int)y + "," + (int)z;
                    Core.Logging.Logger.Log(U.Translate("command_tp_teleport_console", player.CharacterName, cords));
                    UnturnedChat.Say(player, U.Translate("command_tp_teleport_private", cords));
                    return;
                }
            }
            var quest = player.Player.quests;
            if (quest.isMarkerPlaced && command[0].Equals("wp", System.StringComparison.OrdinalIgnoreCase))
            {
                var pos = quest.markerPosition;
                pos.y = 1024f;
                RaycastHit raycastHit;
                if (Physics.Raycast(pos, Vector3.down, out raycastHit, 2048f, RayMasks.WAYPOINT))
                {
                    pos = raycastHit.point + Vector3.up;
                    player.Teleport(pos, player.Rotation);
                    cords = (int)pos.x + "," + (int)pos.y + "," + (int)pos.z;
                    Core.Logging.Logger.Log(U.Translate("command_tp_teleport_console", player.CharacterName, "Waypoint " + cords));
                    UnturnedChat.Say(player, U.Translate("command_tp_teleport_private", cords));
                    return;
                }
            }

            UnturnedPlayer otherplayer = UnturnedPlayer.FromName(command[0]);
            if (otherplayer != null && otherplayer != player)
            {
                player.Teleport(otherplayer);
                Core.Logging.Logger.Log(U.Translate("command_tp_teleport_console", player.CharacterName, otherplayer.CharacterName));
                UnturnedChat.Say(player, U.Translate("command_tp_teleport_private", otherplayer.CharacterName));
            }
            else
            {
                foreach (var item in LevelNodes.nodes)
                {
                    if (item != null && item.type == ENodeType.LOCATION)
                    {
                        var location = (LocationNode)item;
                        if (location.name.IndexOf(command[0], System.StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            Vector3 c = item.point + new Vector3(0f, 0.5f, 0f);
                            player.Teleport(c, player.Rotation);
                            Core.Logging.Logger.Log(U.Translate("command_tp_teleport_console", player.CharacterName, location.name));
                            UnturnedChat.Say(player, U.Translate("command_tp_teleport_private", location.name));
                            return;
                        }
                    }
                }

                UnturnedChat.Say(player, U.Translate("command_tp_failed_find_destination"));
            }
        }
    }
}