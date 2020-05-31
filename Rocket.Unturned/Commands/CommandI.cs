using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using System.Linq;

namespace Rocket.Unturned.Commands
{
    public class CommandI : IRocketCommand
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
            get { return "i"; }
        }

        public string Help
        {
            get { return "Gives yourself an item";}
        }

        public string Syntax
        {
            get { return "<id> [amount]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>() { "item" }; }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.item" , "rocket.i" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length == 0 || command.Length > 2 || string.IsNullOrEmpty(command[0].Trim()))
            {
                goto INVALID;
            }

            byte amount = 1;
            ItemAsset a;

            string itemString = command[0];

            if (!ushort.TryParse(itemString, out ushort id))
            {
                a = Assets.find(EAssetType.ITEM).Cast<ItemAsset>().Where(i => i.itemName != null).OrderBy(i => i.itemName.Length)
                    .FirstOrDefault(i => i.itemName.IndexOf(itemString, StringComparison.OrdinalIgnoreCase) != -1);
            }
            else
                a = (ItemAsset)SDG.Unturned.Assets.find(EAssetType.ITEM, id);

            if (a == null || a.id == 0 || command.Length == 2 && !byte.TryParse(command[1], out amount))
            {
                goto INVALID;
            }

            id = a.id;

            if (U.Settings.Instance.EnableItemBlacklist && !player.HasPermission("itemblacklist.bypass"))
            {
                if (player.HasPermission("item." + id)) {
                    UnturnedChat.Say(player, U.Translate("command_i_blacklisted"));
                    return;
                }
            }

            if (U.Settings.Instance.EnableItemSpawnLimit && !player.HasPermission("itemspawnlimit.bypass"))
            {
                if (amount > U.Settings.Instance.MaxSpawnAmount)
                {
                    UnturnedChat.Say(player, U.Translate("command_i_too_much", U.Settings.Instance.MaxSpawnAmount));
                    return;
                }
            }

            if (player.GiveItem(id, amount))
            {
                Logger.Log(U.Translate("command_i_giving_console", player.DisplayName, id, amount));
                UnturnedChat.Say(player, U.Translate("command_i_giving_private", amount, a.itemName, id));
            }
            else
            {
                UnturnedChat.Say(player, U.Translate("command_i_giving_failed_private", amount, a.itemName, id));
            }

            return;
            INVALID:
            UnturnedChat.Say(player, U.Translate("command_generic_invalid_parameter"));
            //throw new WrongUsageOfCommandException(caller, this);
        }
    }
}
