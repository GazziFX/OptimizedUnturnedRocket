using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

namespace Rocket.Unturned.Commands
{
    public class CommandHome : IRocketCommand
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
            get { return "home"; }
        }

        public string Help
        {
            get { return "Teleports you to your last bed";}
        }

        public string Syntax
        {
            get { return ""; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.home" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (!BarricadeManager.tryGetBed(player.CSteamID, out Vector3 pos, out byte rot))
            {
                UnturnedChat.Say(caller, U.Translate("command_bed_no_bed_found_private"));
                return;
                //return; //throw new WrongUsageOfCommandException(caller, this);
            }
            else
            {
                if (player.Stance == EPlayerStance.DRIVING || player.Stance == EPlayerStance.SITTING)
                {
                    UnturnedChat.Say(caller, U.Translate("command_generic_teleport_while_driving_error"));
                    return;
                    //return; //throw new WrongUsageOfCommandException(caller, this);
                }
                else
                {
                    //player.Teleport(pos, rot * 2f);
                    pos.y += 0.5f; // Respawn uses this offset.
                    if (!player.Player.teleportToLocation(pos, rot * 2f))
                    {
                        UnturnedChat.Say(caller, U.Translate("command_bed_obstructed"));
                    }
                }
            }

        }
    }
}