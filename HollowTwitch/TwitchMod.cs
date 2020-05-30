using HollowTwitch.Commands;
using Modding;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace HollowTwitch
{
    public class TwitchMod : Mod
    {
        private TwitchClient _client;
        private Thread _currentThread;
        private CommandProcessor _p;

      
        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            ObjectLoader.Load(preloadedObjects);
            ModHooks.Instance.AfterSavegameLoadHook += OnSaveGameLoad;
            ModHooks.Instance.NewGameHook += OnNewGame;
            ModHooks.Instance.ApplicationQuitHook += OnQuit;
        }

        public override List<(string, string)> GetPreloadNames()
               => ObjectLoader.ObjectList.Values.ToList();

        
        private void OnSaveGameLoad(SaveGameData data)
            => ReceiveCommands();

        private void OnNewGame()
            => ReceiveCommands();

        private static bool once;
        
        private void ReceiveCommands()
        {
            if (!once)
            {
                _p = new CommandProcessor();
                _p.RegisterCommands<Player>();
                _p.RegisterCommands<Enemies>();
                _p.RegisterCommands<Area>();
                _client = new TwitchClient(new TwitchConfig("lmao gottem", "sid0003", "sid0003"));
                _client.ChatMessageReceived += OnMessageReceived;
                _currentThread = new Thread(new ThreadStart(_client.StartReceive));
                _currentThread.Start();
                Modding.Logger.Log("started receiving");
                once = true;
            }
        }

        private void OnQuit()
        {
            _currentThread.Abort();
            _client.Dispose();
        }

        private void OnMessageReceived(string message)
        {
            Modding.Logger.Log("Twitch chat: " + message);
            //handle commands here
            char prefix = '$';
            if (message.StartsWith(prefix.ToString()))
            {
                var command = message.TrimStart(prefix);
                _p.Execute(command);
            }
        }
    }
}
