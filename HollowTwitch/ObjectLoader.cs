using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HollowTwitch.Extensions;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Vasi;
using Object = UnityEngine.Object;

namespace HollowTwitch
{
    internal static class ObjectLoader
    {
        public static readonly Dictionary<(string, Func<GameObject, GameObject>), (string, string)> ObjectList = new() {
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
                ("Laser Turret", null), ("Mines_31", "Laser Turret")
            },
            {
                ("bee", null), ("GG_Hive_Knight", "Battle Scene/Droppers/Bee Dropper")
            },
            {
                ("nkgspike", (go =>
                {
                    go.SetActive(true);
                    
                    GameObject spike = Object.Instantiate(
                        go.GetComponentsInChildren<Transform>(true)
                          .First(x => x.name.Contains("Nightmare Spike"))
                          .gameObject
                    );

                    Object.DontDestroyOnLoad(spike);
                    
                    spike.LocateMyFSM("Control").ChangeTransition("Dormant", "SPIKES READY", "Ready");
                    
                    go.SetActive(false);
                    
                    return spike;
                })),
                ("GG_Grimm_Nightmare", "Grimm Spike Holder")
            },
            {
                ("AbsOrb",abs =>
                {
                    PlayMakerFSM fsm = abs.LocateMyFSM("Attack Commands");
                    
                    var spawn = fsm.GetAction<SpawnObjectFromGlobalPool>("Spawn Fireball", 1);
                    
                    GameObject orbPre = spawn.gameObject.Value;
                    
                    GameObject ShotCharge = abs.transform.Find("Shot Charge").gameObject;
                    GameObject ShotCharge2 = abs.transform.Find("Shot Charge 2").gameObject;

                    var orb = new GameObject("AbsOrb");
                    orbPre.transform.SetParent(orb.transform);
                    ShotCharge.transform.SetParent(orb.transform);
                    ShotCharge2.transform.SetParent(orb.transform);

                    Object.DontDestroyOnLoad(orb);
                    Object.Destroy(abs);
                    
                    return orb;
                }),
                ("GG_Radiance","Boss Control/Absolute Radiance")
            }
        };

        public static Dictionary<string, GameObject> InstantiableObjects { get; } = new();

        public static Dictionary<string, Shader> Shaders { get; } = new();

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

            // ReSharper disable once SuggestVarOrType_DeconstructionDeclarations
            foreach (var ((name, modify), (room, go_name)) in ObjectList)
            {
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
            var assembly = Assembly.GetExecutingAssembly();
            
            string bundleName = assembly.GetManifestResourceNames().First(x => x.Contains("shaders"));

            using Stream bundleStream = assembly.GetManifestResourceStream(bundleName);

            AssetBundle assetBundle = AssetBundle.LoadFromStream(bundleStream);

            if (assetBundle == null) return;

            Shader[] shaders = assetBundle.LoadAllAssets<Shader>();

            if (shaders == null || shaders.Length == 0) return;

            foreach (Shader shader in shaders)
            {
                Shaders.Add(shader.name, shader);
                Logger.Log(shader.name);
            }
        }
    }
}