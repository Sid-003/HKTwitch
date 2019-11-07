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
        public override void Initialize()
        {

            ModHooks.Instance.AfterSavegameLoadHook += OnSaveGameLoad;


        }

        static bool once;
        private void OnSaveGameLoad(SaveGameData data)
        {
            if (!once)
            {
                Console.WriteLine("starting this shit");
                var client = new TwitchClient("jhixoaj5iqbpeahtjsm4uji3hqbi82", "sid0003", "gradow");
                client.ChatMessageReceived += OnMessageReceived;
                var thread = new Thread(new ThreadStart(client.StartReceive));
                thread.Start();
                Console.WriteLine("started receiving");
                once = true;
            }
        }

        private void OnMessageReceived(string message)
        {
            Modding.Logger.Log(message);
        }
    }
}
