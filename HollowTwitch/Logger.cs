namespace HollowTwitch
{
    public static class Logger
    {
        public static void Log(object obj) => TwitchMod.Instance.Log(obj);
        
        public static void LogWarn(object obj) => TwitchMod.Instance.LogWarn(obj);
        
        public static void LogError(object obj) => TwitchMod.Instance.LogError(obj);
    }
}