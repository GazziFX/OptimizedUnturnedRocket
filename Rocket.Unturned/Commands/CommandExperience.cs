using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Rocket.Unturned.Commands
{
    public class CommandExperience : IRocketCommand
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
            get { return "exp"; }
        }

        public string Help
        {
            get { return "Give experience"; }
        }

        public string Syntax
        {
            get { return ""; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "rocket.experience" };
            }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player;
            if (command.Length == 1)
            {
                player = caller as UnturnedPlayer;
            }
            else if (command.Length == 2)
            {
                player = command.GetUnturnedPlayerParameter(0);
            }
            else
                goto ERR;

            if (player == null || !int.TryParse(command[command.Length - 1], out var count))
                goto ERR;

            player.Experience = (uint)(player.Experience + count);

            return;
            ERR:
            UnturnedChat.Say(caller, U.Translate("command_generic_invalid_parameter"));
        }
    }
}