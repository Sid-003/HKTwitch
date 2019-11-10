namespace HollowTwitch
{
    public class TwitchConfig
    {
        public string Token { get; }

        public string Username { get; }

        public string Channel { get; }

        public TwitchConfig(string oauth, string username, string channel)
        {
            Token = oauth;
            Username = username;
            Channel = channel;
        }
    }
}