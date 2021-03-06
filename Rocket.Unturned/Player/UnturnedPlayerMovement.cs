using Rocket.Core.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using UnityEngine;

namespace Rocket.Unturned
{
    public class UnturnedPlayerMovement : UnturnedPlayerComponent
    {
        public bool VanishMode = false;
        DateTime lastUpdate = DateTime.Now;
        Vector3 lastVector = new Vector3(0,-1,0);

        /*DateTime? requested = null;
        string webClientResult = null;

        private void OnEnable()
        {
            if (U.Settings.Instance.RocketModObservatory.CommunityBans)
            {
                using (RocketWebClient webClient = new RocketWebClient())
                {
                    try
                    {
                        webClient.DownloadStringCompleted += (object sender, System.Net.DownloadStringCompletedEventArgs e) =>
                        {
                            if (e.Error == null)
                            {
                                if (e.Result.Contains(",")){
                                    string[] result = e.Result.Split(',');
                                    long age;
                                    if (result[0] == "true")
                                    {
                                        Core.Logging.Logger.Log("[RocketMod Observatory] Kicking Player " + Player.CharacterName + "because he is banned:" + result[1]);
                                        webClientResult = result[1];
                                        requested = DateTime.Now;
                                        Player.Kick("you are banned from observatory: " + result[1]);
                                    }
                                    else if (U.Settings.Instance.RocketModObservatory.KickLimitedAccounts && result.Length >= 2 && result[1] == "true")
                                    {
                                        Core.Logging.Logger.Log("[RocketMod Observatory] Kicking Player " + Player.CharacterName + " because his account is limited");
                                        Player.Kick("your Steam account is limited");
                                    }
                                    else if (U.Settings.Instance.RocketModObservatory.KickTooYoungAccounts && result.Length == 3 && long.TryParse(result[2].ToString(),out age))
                                    {
                                        long epochTicks = new DateTime(1970, 1, 1).Ticks;
                                        long unixTime = ((DateTime.UtcNow.Ticks - epochTicks) / TimeSpan.TicksPerSecond);
                                        long d = (unixTime - age);
                                        if (d < U.Settings.Instance.RocketModObservatory.MinimumAge)
                                        {
                                            Core.Logging.Logger.Log("[RocketMod Observatory] Kicking Player " + Player.CharacterName + " because his account is younger then "+ U.Settings.Instance.RocketModObservatory.MinimumAge+" seconds ("+d+" seconds)");
                                            Player.Kick("your Steam account is not old enough");
                                        } 
                                    }
                                }
                            }
                        };
                        webClient.DownloadStringAsync(new Uri(string.Format("http://banlist.observatory.rocketmod.net/?steamid={0}", Player.CSteamID)));
                    }
                    catch (Exception)
                    {
                        //                        
                    }
                }
            }
        }*/

        private void FixedUpdate()
        {
            /*if (requested.HasValue && (DateTime.Now - requested.Value).TotalSeconds >= 2){
                Provider.kick(Player.CSteamID, webClientResult);
                requested = null;
            }*/

            PlayerMovement movement = Player.GetComponent<PlayerMovement>();

            if (!VanishMode)
            {
                if (U.Settings.Instance.LogSuspiciousPlayerMovement && lastUpdate.AddSeconds(1) < DateTime.Now)
                {
                    lastUpdate = DateTime.Now;

                    Vector3 positon = movement.real;

                    if (lastVector.y != -1)
                    {
                        //float x = System.Math.Abs(lastVector.x - positon.x);
                        float y = positon.y - lastVector.y;
                        //float z = System.Math.Abs(lastVector.z - positon.z);
                        if (y > 15)
                        {
                            Physics.Raycast(positon, Vector3.down, out var raycastHit);
                            float distance = System.Math.Abs(raycastHit.point.y - positon.y);
                            Core.Logging.Logger.Log(Player.DisplayName + " moved x:" + positon.x + " y:" + positon.y + "(+" + y + ") z:" + positon.z + " in the last second (" + distance + ")");
                        }
                    }
                    lastVector = movement.real;
                }
            }
        }
    }
}
