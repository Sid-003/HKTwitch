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
    public class Config
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

        public List<string> BlacklistedCommands = new();

        public List<string> AdminUsers = new();

        public List<string> BannedUsers = new();

        public Dictionary<string, int> Cooldowns = new();
    }
}