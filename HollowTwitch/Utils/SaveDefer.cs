using System;
using Modding;
using UnityEngine;

namespace HollowTwitch.Utils
{
    public class SaveDefer : CustomYieldInstruction
    {
        private readonly float _start;
        private readonly float _len;

        private readonly Action<PlayerData> AfterWait;

        private bool _quit;

        public SaveDefer(float seconds, Action<PlayerData> a)
        {
            _start = Time.realtimeSinceStartup;

            _len = seconds;

            AfterWait = a;

            ModHooks.Instance.BeforeSavegameSaveHook += BeforeSave;
        }

        private void BeforeSave(SaveGameData data)
        {
            _quit = true;

            AfterWait(data.playerData);

            ModHooks.Instance.BeforeSavegameSaveHook -= BeforeSave;
        }

        public override bool keepWaiting
        {
            get
            {
                if (Time.realtimeSinceStartup - _start < _len && !_quit)
                    return true;

                if (!_quit)
                {
                    AfterWait(PlayerData.instance);
                }

                return false;
            }
        }
    }
}