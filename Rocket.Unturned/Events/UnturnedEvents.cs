using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Rocket.Core.Extensions;
using Rocket.API;
using Rocket.API.Extensions;

namespace Rocket.Unturned.Events
{
    public sealed class UnturnedEvents : IRocketImplementationEvents
    {
        private static UnturnedEvents Instance;
        internal void Awake()
        {
            Instance = this;
            Provider.onServerDisconnected += (CSteamID r) => 
            {
                if (r != CSteamID.Nil)
                {
                    OnPlayerDisconnected.TryInvoke(UnturnedPlayer.FromCSteamID(r));
                }
            };
            Provider.onServerShutdown += () => { OnShutdown.TryInvoke(); };
            Provider.onServerConnected += (CSteamID r) => 
            {
                if (r != CSteamID.Nil)
                {
                    UnturnedPlayer p = UnturnedPlayer.FromCSteamID(r);
                    var go = p.Player.gameObject;
                    go.TryAddComponent<UnturnedPlayerFeatures>();
                    go.TryAddComponent<UnturnedPlayerMovement>();
                    go.TryAddComponent<UnturnedPlayerEvents>();
                    OnBeforePlayerConnected.TryInvoke(p);
                }
            };
            DamageTool.damagePlayerRequested += newOnDamage;
        }

        static void newOnDamage(ref DamagePlayerParameters p, ref bool shouldAllow)
        {
            if (p.player)
            {
                var f = p.player.GetComponent<UnturnedPlayerFeatures>();
                if (f && f.GodMode)
                {
                    shouldAllow = false;
                    return;
                }    
            }
            onDamage(p.player, ref p.cause, ref p.limb, ref p.killer, ref p.direction, ref p.damage, ref p.times, ref shouldAllow);
        }

        static void onDamage(SDG.Unturned.Player player, ref EDeathCause cause, ref ELimb limb, ref CSteamID killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage)
        {
            if (OnPlayerDamaged != null && player != null && killer != CSteamID.Nil && killer != null)
            {
                UnturnedPlayer getterDamage = UnturnedPlayer.FromPlayer(player);
                UnturnedPlayer senderDamage = UnturnedPlayer.FromCSteamID(killer);
                OnPlayerDamaged.TryInvoke(getterDamage, cause, limb, senderDamage, direction, damage, times, canDamage);
            }
        }

        public delegate void PlayerDisconnected(UnturnedPlayer player);
        public event PlayerDisconnected OnPlayerDisconnected;

        public delegate void OnPlayerGetDamage(UnturnedPlayer player, ref EDeathCause cause, ref ELimb limb, ref UnturnedPlayer killer, ref Vector3 direction, ref float damage, ref float times, ref bool canDamage);
        public static event OnPlayerGetDamage OnPlayerDamaged;

        public event ImplementationShutdown OnShutdown;

        internal static void triggerOnPlayerConnected(UnturnedPlayer player)
        {
            Instance.OnPlayerConnected.TryInvoke(player);
        }

        public delegate void PlayerConnected(UnturnedPlayer player);
        public event PlayerConnected OnPlayerConnected;
        public event PlayerConnected OnBeforePlayerConnected;
    }
}