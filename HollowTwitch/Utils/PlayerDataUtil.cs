using System.Collections;
using Modding;
using UnityEngine;

namespace HollowTwitch.Utils
{
    public static class PlayerDataUtil
    {
        internal static IEnumerator FakeSet(string name, bool val, float time)
        {
            bool GetBool(string orig)
            {
                return orig == name 
                    ? val 
                    : PlayerData.instance.GetBoolInternal(name);
            }
            
            ModHooks.Instance.GetPlayerBoolHook += GetBool;

            yield return new WaitForSeconds(time);
            
            ModHooks.Instance.GetPlayerBoolHook -= GetBool;
        }
    }
}