using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HollowTwitch.Commands;
using Modding;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;

namespace HollowTwitch
{
    public class TwitchMod : Mod
    {
        private TwitchClient _client;
        private Thread _currentThread;
        
        internal TwitchConfig Config = new TwitchConfig();

        internal CommandProcessor Processor { get; private set; }

        public static TwitchMod Instance;
        
        public override ModSettings GlobalSettings
        {
            get => Config;
            set => Config = value as TwitchConfig;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Instance = this;
            
            ObjectLoader.Load(preloadedObjects);
            ObjectLoader.LoadAssets();
            
            ModHooks.Instance.AfterSavegameLoadHook += OnSaveGameLoad;
            ModHooks.Instance.NewGameHook += OnNewGame;
            ModHooks.Instance.ApplicationQuitHook += OnQuit;
        }

        public override List<(string, string)> GetPreloadNames() => ObjectLoader.ObjectList.Values.ToList();

        private void OnSaveGameLoad(SaveGameData data) => ReceiveCommands();

        private void OnNewGame() => ReceiveCommands();

        private static bool once;

        private void ReceiveCommands()
        {
            if (once) return;

            Processor = new CommandProcessor();
            
            Processor.RegisterCommands<Player>();
            Processor.RegisterCommands<Enemies>();
            Processor.RegisterCommands<Area>();
            Processor.RegisterCommands<Camera>();
            Processor.RegisterCommands<Game>();
            Processor.RegisterCommands<Meta>();
            
            _client = new TwitchClient(Config);
            _client.ChatMessageReceived += OnMessageReceived;
            
            _currentThread = new Thread(_client.StartReceive);
            _currentThread.Start();
            
            Log("Started receiving.");

            once = true;
        }

        private void OnQuit()
        {
            _currentThread.Abort();
            _client.Dispose();
        }

        private void OnMessageReceived(string user, string message)
        {
            Log($"Twitch chat: [{user}: {message}]");
            
            string trimmed = message.Trim();
            int index = trimmed.IndexOf(Config.Prefix);

            if (index != 0) return;
            
            string command = trimmed.Substring(Config.Prefix.Length).Trim();
            
            Processor.Execute(user, command, Config);
        }
    }
}