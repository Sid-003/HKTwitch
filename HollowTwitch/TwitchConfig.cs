using System;
using System.Collections.Generic;
using Modding;
using UnityEngine.Serialization;

namespace HollowTwitch
{
    [Serializable]
    public class TwitchConfig : ModSettings
    {
        public string Token;

        public string Username;

        [FormerlySerializedAs("Channel")]
        public string TwitchChannel;

        public int BilibiliRoomID;
        
        public string Prefix = "!";

        public List<string> BlacklistedCommands = new List<string>();

        public List<string> AdminUsers = new List<string>();

        public List<string> BannedUsers = new List<string>();

        public Dictionary<string, int> Cooldowns = new Dictionary<string, int>();
    }
}