using System;
using Modding;

namespace HollowTwitch
{
    [Serializable]
    public class TwitchConfig : ModSettings
    {
        public string Token;

        public string Username;

        public string Channel;

        public string Prefix;
    }
}