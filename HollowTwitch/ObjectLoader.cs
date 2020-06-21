using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HollowTwitch
{
    internal static class ObjectLoader
    {
        public static readonly Dictionary<(string, Func<GameObject, GameObject>), (string, string)> ObjectList = new Dictionary<(string, Func<GameObject, GameObject>), (string, string)>
        {
            {
                ("aspid", obj =>
                {
                    obj.LocateMyFSM("spitter").SetState("Init");
                    
                    Object.Destroy(obj.GetComponent<PersistentBoolItem>());
                    
                    return obj;
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
                ("spike", obj => 
                {
                    obj.AddComponent<DamageHero>().damageDealt = 1;
                    
                    return obj;
                }),
                ("Room_Colosseum_Bronze", "Colosseum Manager/Ground Spikes/Colosseum Spike")
            },
            {
                ("cave_spikes", null),
                ("Tutorial_01", "_Props/Cave Spikes")
            },
            {
                ("jar", null), ("GG_Collector", "Spawn Jar")
            },
            {
                ("roller", null), ("Crossroads_ShamanTemple", "_Enemies/Roller")
            },
            {
                ("buzzer", null), ("Crossroads_ShamanTemple", "_Enemies/Buzzer")
            },
            {
                ("prefab_jar", null), ("Ruins2_11", "Break Jar (6)")
            },
            {
                ("zap", go => go.LocateMyFSM("Mega Jellyfish").GetAction<SpawnObjectFromGlobalPool>("Gen", 2).gameObject.Value), ("GG_Uumuu", "Mega Jellyfish GG")
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
            static GameObject Spawnable(GameObject obj, Func<GameObject, GameObject> modify)
            {
                GameObject go = Object.Instantiate(obj);
                go = modify?.Invoke(go) ?? go;
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