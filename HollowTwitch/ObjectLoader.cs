using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Logger = HollowTwitch.Logger;
using Object = UnityEngine.Object;

namespace HollowTwitch
{
    internal static class ObjectLoader
    {
        public static readonly Dictionary<(string, Action<GameObject>), (string, string)> ObjectList = new Dictionary<(string, Action<GameObject>), (string, string)>
        {
            {
                ("aspid", (GameObject obj) =>
                {
                    obj.LocateMyFSM("spitter").SetState("Init");
                    Object.Destroy(obj.GetComponent<PersistentBoolItem>());
                }),
                ("Deepnest_East_11", "Super Spitter")
            },
            {
                ("Revek", null),
                ("RestingGrounds_08", "Ghost Battle Revek")
            },
            {
                ("pv", null),
                ("GG_Hollow_Knight", "Battle Scene/HK Prime")
            },
            {
                ("spike", (GameObject obj) => { obj.AddComponent<DamageHero>().damageDealt = 1; }),
                ("Room_Colosseum_Bronze", "Colosseum Manager/Ground Spikes/Colosseum Spike")
            },
            {
                ("cg2", (GameObject obj) => { obj.LocateMyFSM("Beam Miner").SetState("Battle Init"); }), ("Mines_18_boss", "Mega Zombie Beam Miner (1)")
            },
            {
                ("Beam Point R", null), ("Mines_18_boss", "Beam Point R")
            },
            {
                ("Beam Point L", null), ("Mines_18_boss", "Beam Point L")
            },
            {
                ("Beam", null), ("Mines_18_boss", "Beam")
            },
            {
                ("Beam Ball", null), ("Mines_18_boss", "Beam Ball")
            }
        };

        public static Dictionary<string, GameObject> InstantiableObjects { get; } = new Dictionary<string, GameObject>();

        public static Dictionary<string, Shader> Shaders { get; } = new Dictionary<string, Shader>();

        public static void Load(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            static GameObject Spawnable(GameObject obj, Action<GameObject> modify)
            {
                GameObject go = Object.Instantiate(obj);
                modify?.Invoke(go);
                Object.DontDestroyOnLoad(go);
                go.SetActive(false);
                return go;
            }

            foreach (var kvp in ObjectList)
            {
                var (name, modify) = kvp.Key;
                var (room, go_name) = kvp.Value;

                if (!preloadedObjects[room].TryGetValue(go_name, out GameObject go))
                {
                    Logger.LogWarn($"[HollowTwitch]: Unable to load GameObject {go_name}");
                    
                    continue;
                }

                InstantiableObjects.Add(name, Spawnable(go, modify));
            }
        }

        public static void LoadAssets()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string bundleName = assembly.GetManifestResourceNames().First(x => x.Contains("shaders"));

            using Stream bundleStream = assembly.GetManifestResourceStream(bundleName);

            AssetBundle assetBundle = AssetBundle.LoadFromStream(bundleStream);

            if (assetBundle == null) return;

            Shader[] shaders = assetBundle.LoadAllAssets<Shader>();

            if (shaders == null || shaders?.Count() == 0) return;

            foreach (Shader shader in shaders)
            {
                Shaders.Add(shader.name, shader);
                Logger.Log(shader.name);
            }
        }
    }
}