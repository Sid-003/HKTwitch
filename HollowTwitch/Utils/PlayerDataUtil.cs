using System.Collections;
using Modding;
using UnityEngine;

namespace HollowTwitch.Utils
{
    public static class PlayerDataUtil
    {
        internal static IEnumerator FakeSet(string name, bool val, float time)
        {
            bool GetBool(string bool_name, bool orig)
            {
                return bool_name == name 
                    ? val 
                    : orig;
            }
            
            ModHooks.GetPlayerBoolHook += GetBool;

            yield return new WaitForSeconds(time);
            
            ModHooks.GetPlayerBoolHook -= GetBool;
        }
    }
}