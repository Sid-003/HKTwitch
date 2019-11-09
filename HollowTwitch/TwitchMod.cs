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
                Logger.Log("starting this shit");              
                _client = new TwitchClient("rnbl5u9yyomookje7dot24to0df91u", "sid0003", "5fiftysix6");
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
        }
    }
}
