using System;
using System.Collections.Generic;
using HollowTwitch.Clients;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

namespace HollowTwitch
{
    [Serializable]
    public class Config : ModSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ClientType Client = ClientType.Twitch;
        
        [FormerlySerializedAs("Token")]
        public string TwitchToken;

        [FormerlySerializedAs("Username")]
        public string TwitchUsername;

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