using SDG.Unturned;
using UnityEngine;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.API.Extensions;

namespace Rocket.Unturned.Commands
{
    public class CommandJump : IRocketCommand
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
            get { return "jump"; }
        }

        public string Help
        {
            get { return "";}
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
            get { return new List<string>() { "rocket.jump" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            var stance = player.Player.stance.stance;
            if (stance == EPlayerStance.DRIVING || stance == EPlayerStance.SITTING)
            {
                UnturnedChat.Say(player, U.Translate("command_generic_teleport_while_driving_error"));
                return;
            }
            var aim = player.Player.look.aim;
            if (Physics.Raycast(aim.position, aim.forward, out var hit, 2048f, RayMasks.DAMAGE_SERVER, QueryTriggerInteraction.Ignore))
            {
                var point = hit.point;
                point.y += 0.5f;
                player.Player.movement.isAllowed = true;
                player.Player.transform.localPosition = point;
                player.Player.movement.channel.send("tellRecov", ESteamCall.OWNER, ESteamPacket.UPDATE_UNRELIABLE_INSTANT, new object[]
                {
                    point,
                    player.Player.input.recov
                });
            }
        }
    }
}