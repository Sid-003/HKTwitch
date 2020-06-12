using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HollowTwitch.Commands;
using Modding;
using UnityEngine;
using Camera = HollowTwitch.Commands.Camera;
using Logger = Modding.Logger;

namespace HollowTwitch
{
    public class TwitchMod : Mod
    {
        private TwitchClient _client;
        private Thread _currentThread;
        private CommandProcessor _p;
        private TwitchConfig _config = new TwitchConfig();

        public override ModSettings GlobalSettings
        {
            get => _config;
            set => _config = value as TwitchConfig;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
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

            _p = new CommandProcessor();
            _p.RegisterCommands<Player>();
            _p.RegisterCommands<Enemies>();
            _p.RegisterCommands<Area>();
            _p.RegisterCommands<Camera>();
            _client = new TwitchClient(_config);
            _client.ChatMessageReceived += OnMessageReceived;
            _currentThread = new Thread(_client.StartReceive);
            _currentThread.Start();
            Logger.Log("started receiving");

            once = true;
        }

        private void OnQuit()
        {
            _currentThread.Abort();
            _client.Dispose();
        }

        private void OnMessageReceived(string message)
        {
            Logger.Log("Twitch chat: " + message);

            int index = message.IndexOf(_config.Prefix);

            if (index != 0) return;
            
            string command = message.Substring(_config.Prefix.Length).Trim();
            
            _p.Execute(command);
        }
    }
}