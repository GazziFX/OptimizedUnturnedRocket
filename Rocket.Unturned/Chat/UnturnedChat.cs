using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Rocket.API;

namespace Rocket.Unturned.Chat
{
    public sealed class UnturnedChat
    {
        /*private void Awake()
        {
            SDG.Unturned.ChatManager.onChatted += handleChat;
        }*/

        internal static void handleChat(SteamPlayer steamPlayer, EChatMode chatMode, ref Color incomingColor, ref bool rich, string message, ref bool cancel)
        {
            cancel = false;
            try
            {
                UnturnedPlayer player = UnturnedPlayer.FromSteamPlayer(steamPlayer);
                incomingColor = UnturnedPlayerEvents.firePlayerChatted(player, chatMode, player.Color, message, ref cancel);
            }
            catch (Exception ex)
            {
                Core.Logging.Logger.LogException(ex);
            }

            cancel = !cancel;
        }

        public static Color GetColorFromName(string colorName, Color fallback)
        {
            switch (colorName.Trim().ToLower())
            {
                case "black": return Color.black;
                case "blue": return Color.blue;
                case "clear": return Color.clear;
                case "cyan": return Color.cyan;
                case "gray": return Color.gray;
                case "green": return Color.green;
                case "grey": return Color.grey;
                case "magenta": return Color.magenta;
                case "red": return Color.red;
                case "white": return Color.white;
                case "yellow": return Color.yellow;
                case "rocket": return GetColorFromRGB(90, 206, 205);
            }

            Color? color = GetColorFromHex(colorName);
            if (color.HasValue) return color.Value;

            return fallback;
        }

        public static Color? GetColorFromHex(string hexString)
        {
            if (string.IsNullOrEmpty(hexString))
                return null;
            hexString = hexString.Substring(1);
            if (hexString.Length == 3)
            { // #54A
                hexString = hexString.Insert(1, Convert.ToString(hexString[0]))  // #554A
                                     .Insert(3, Convert.ToString(hexString[1]))  // #5544A
                                     .Insert(5, Convert.ToString(hexString[2])); // #5544AA
            }
            if (hexString.Length != 6 || !int.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, null, out int argb))
            {
                return null;
            }
            byte r = (byte)((argb >> 16) & 0xff);
            byte g = (byte)((argb >> 8) & 0xff);
            byte b = (byte)(argb & 0xff);
            return GetColorFromRGB(r, g, b);
        }
        public static Color GetColorFromRGB(byte R, byte G, byte B)
        {
            return GetColorFromRGB(R, G, B, 100);
        }
        public static Color GetColorFromRGB(byte R, byte G, byte B, short A)
        {
            return new Color((1f / 255f) * R, (1f / 255f) * G, (1f / 255f) * B, (1f / 100f) * A);
        }

        public static void Say(string message, bool rich)
        {
            Say(message, Palette.SERVER, rich);
        }

        public static void Say(string message)
        {
            Say(message, false);
        }

        public static void Say(IRocketPlayer player, string message)
        {
            Say(player, message, false);
        }

        public static void Say(IRocketPlayer player, string message, Color color, bool rich)
        {
            if (player is ConsolePlayer)
            {
                Core.Logging.Logger.Log(message, ConsoleColor.Gray);
            }
            else
            {
                Say(((UnturnedPlayer)player).CSteamID, message, color, rich);
            }
        }

        public static void Say(IRocketPlayer player, string message, Color color)
        {
            Say(player, message, color, false);
        }

        public static void Say(string message, Color color, bool rich)
        {
            Core.Logging.Logger.Log("Broadcast: " + message, ConsoleColor.Gray);
            if (message.Length <= maxLength)
            {
                ChatManager.serverSendMessage(message, color, fromPlayer: null, toPlayer: null, mode: EChatMode.GLOBAL, iconURL: null, useRichTextFormatting: rich);
                return;
            }
            foreach (string m in wrapMessage(message))
            {
                ChatManager.serverSendMessage(m, color, fromPlayer: null, toPlayer: null, mode: EChatMode.GLOBAL, iconURL: null, useRichTextFormatting: rich);
            }
        }

        public static void Say(string message, Color color)
        {
            Say(message, color, false);
        }

        public static void Say(IRocketPlayer player, string message, bool rich)
        {
            Say(player, message, Palette.SERVER, rich);
        }

        public static void Say(CSteamID CSteamID, string message, bool rich)
        {
            Say(CSteamID, message, Palette.SERVER, rich);
        }


        public static void Say(CSteamID CSteamID, string message)
        {
            Say(CSteamID, message, false);
        }

        public static void Say(CSteamID CSteamID, string message, Color color, bool rich)
        {
            if (CSteamID == CSteamID.Nil)
            {
                Core.Logging.Logger.Log(message, ConsoleColor.Gray);
            }
            else
            {
                SteamPlayer toPlayer = PlayerTool.getSteamPlayer(CSteamID);
                if (message.Length <= maxLength)
                {
                    ChatManager.serverSendMessage(message, color, fromPlayer: null, toPlayer: toPlayer, mode: EChatMode.SAY, iconURL: null, useRichTextFormatting: rich);
                    return;
                }
                foreach (string m in wrapMessage(message))
                {
                    ChatManager.serverSendMessage(m, color, fromPlayer: null, toPlayer: toPlayer, mode: EChatMode.SAY, iconURL: null, useRichTextFormatting: rich);
                }
            }
        }

        public static void Say(CSteamID CSteamID, string message, Color color)
        {
            Say(CSteamID, message, color, false);
        }

        public static List<string> wrapMessage(string text)
        {
            if (text.Length == 0) return new List<string>();
            string[] words = text.Split(' ');
            List<string> lines = new List<string>();
            string currentLine = "";
            foreach (var currentWord in words)
            {

                if ((currentLine.Length > maxLength) ||
                    ((currentLine.Length + currentWord.Length) > maxLength))
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }

                if (currentLine.Length > 0)
                    currentLine += " " + currentWord;
                else
                    currentLine += currentWord;

            }

            if (currentLine.Length > 0)
                lines.Add(currentLine);
            return lines;
        }

        const int maxLength = 90;
    }
}