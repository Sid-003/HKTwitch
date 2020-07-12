using System;
using System.Collections.Generic;
using Modding;

namespace HollowTwitch
{
    [Serializable]
    public class TwitchConfig : ModSettings
    {
        public string Token;

        public string Username;

        public string Channel;

        public string Prefix = "!";

        public List<string> BlacklistedCommands = new List<string>();

        public List<string> AdminUsers = new List<string>();

        public List<string> BannedUsers = new List<string>();

        public Dictionary<string, int> Cooldowns = new Dictionary<string, int>();
    }
}