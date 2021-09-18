using System.Collections.Generic;
using HollowTwitch.Commands;
using UnityEngine;

namespace HollowTwitch.Extensions
{
    public static class Extensions
    {
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value) 
            => (key, value) = (self.Key, self.Value);

        public static bool HasValue(this CameraEffects @enum, CameraEffects toCheck)
            => (@enum & toCheck) != 0;
        
        public static GameObject GetChild(this GameObject go, string child)
        {
            return go.transform.Find(child).gameObject;
        }
    }
}
