using HollowTwitch.Commands;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HollowTwitch
{
    public class TwitchMod : Mod
    {
        private TwitchClient _client;
        private Thread _currentThread;
        private CommandProcessor _p;
        public override void Initialize()
        {
            ModHooks.Instance.AfterSavegameLoadHook += OnSaveGameLoad;

            ModHooks.Instance.ApplicationQuitHook += OnQuit;
        }

        static bool once;
        private void OnSaveGameLoad(SaveGameData data)
        {
            if (!once)
            {
                _p = new CommandProcessor();
                _p.RegisterCommands<Player>();
                Logger.Log("starting this shit");              
                _client = new TwitchClient(new TwitchConfig("rnbl5u9yyomookje7dot24to0df91u", "sid0003", "sid0003"));
                _client.ChatMessageReceived += OnMessageReceived;
                _currentThread = new Thread(new ThreadStart(_client.StartReceive));
                _currentThread.Start();
                Logger.Log("started receiving");
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
            Logger.Log("Twitch chat: " + message);
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
