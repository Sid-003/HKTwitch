using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ModCommon.Util;
using ModCommon;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace HollowTwitch
{
    internal static class ObjectLoader
    {
        public static Dictionary<(string, Action<GameObject>), (string, string)> ObjectList { get; } = new Dictionary<(string, Action<GameObject>), (string, string)>()
        {
            { ("aspid", (GameObject obj) =>
            {
                obj.LocateMyFSM("spitter").SetState("Init");
                UnityEngine.Object.Destroy(obj.GetComponent<PersistentBoolItem>());
            }), ("Deepnest_East_11", "Super Spitter")
            },
            { ("pv", null), ("GG_Hollow_Knight","Battle Scene/HK Prime")},
            { ("spike", (GameObject obj) =>
            {
                  obj.AddComponent<DamageHero>().damageDealt = 1;
            }), ("Room_Colosseum_Bronze", "Colosseum Manager/Ground Spikes/Colosseum Spike")},
            {("cg2", (GameObject obj) =>
            {
                obj.LocateMyFSM("Beam Miner").SetState("Battle Init");
            }), ("Mines_18_boss", "Mega Zombie Beam Miner (1)") },
            {("Beam Point R", null), ("Mines_18_boss", "Beam Point R")},
            {("Beam Point L", null), ("Mines_18_boss", "Beam Point L")},
             {("Beam", null), ("Mines_18_boss", "Beam")},
            {("Beam Ball", null), ("Mines_18_boss", "Beam Ball")}
        };

        public static Dictionary<string, GameObject> InstantiableObjects { get; private set; } = new Dictionary<string, GameObject>();

        public static void Load(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            static GameObject Spawnable(GameObject obj, Action<GameObject> modify)
            {
                var go = UnityEngine.Object.Instantiate(obj);
                modify?.Invoke(go);
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.SetActive(false);
                return go;
            }

            foreach (var kvp in ObjectList)
            {
                var (name, modify) = kvp.Key;
                var (room, go) = kvp.Value;
                InstantiableObjects.Add(name, Spawnable(preloadedObjects[room][go], modify));
            }
        }

    }
}
