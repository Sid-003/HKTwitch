using System;
using System.Collections;
using System.Linq;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using Object = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace HollowTwitch.Commands
{
    public class Enemies
    {
        private readonly Type _palePrince;

        public Enemies()
        {
            if (!ModHooks.Instance.LoadedMods.Any(x => x.Contains("PalePrince"))) return;
            
            _palePrince = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("Pale_Prince"))?.GetTypes()?.FirstOrDefault(x => x.Name == "Prince");
        }

        [HKCommand("spawn")]
        [Cooldown(60, 3)]
        public IEnumerator SpawnEnemy(string name)
        {
            Logger.Log($"Trying to spawn enemy {name}");

            if (!ObjectLoader.InstantiableObjects.TryGetValue(name, out GameObject go))
                yield break;

            GameObject enemy = Object.Instantiate(go, HeroController.instance.gameObject.transform.position, Quaternion.identity);
            yield return new WaitForSecondsRealtime(1);

            enemy.SetActive(true);
        }


        [HKCommand("spawnpv")]
        [Cooldown(60)]
        public IEnumerator SpawnPureVessel()
        {
            // stolen from https://github.com/SalehAce1/PathOfPureVessel

            yield return null;

            var (x, y, _) = HeroController.instance.gameObject.transform.position;

            GameObject pv = Object.Instantiate
            (
                ObjectLoader.InstantiableObjects["pv"],
                HeroController.instance.gameObject.transform.position + new Vector3(0, 2.6f),
                Quaternion.identity
            );

            pv.SetActive(true);
            //    if (!(_palePrince is null)) pv.AddComponent(_palePrince);  //sad gamer moment

            RaycastHit2D castLeft = Physics2D.Raycast(new Vector2(x, y), Vector2.left, 1000, 1 << 8);
            RaycastHit2D castRight = Physics2D.Raycast(new Vector2(x, y), Vector2.right, 1000, 1 << 8);

            if (!castLeft)
                castLeft.distance = 30f;
            if (!castRight)
                castRight.distance = 30f;


            PlayMakerFSM control = pv.LocateMyFSM("Control");
            control.FsmVariables.FindFsmFloat("Left X").Value = x - castLeft.distance;
            control.FsmVariables.FindFsmFloat("Right X").Value = x + castRight.distance;
            control.FsmVariables.FindFsmFloat("TeleRange Max").Value = x - castLeft.distance;
            control.FsmVariables.FindFsmFloat("TeleRange Min").Value = x + castRight.distance;
            control.FsmVariables.FindFsmFloat("Plume Y").Value = y - 3.2f;
            control.FsmVariables.FindFsmFloat("Stun Land Y").Value = y + 3f;

            control.InsertMethod
            (
                "Plume Gen",
                3,
                () =>
                {
                    GameObject go = control.GetAction<SpawnObjectFromGlobalPool>("Plume Gen", 0).storeObject.Value;
                    PlayMakerFSM fsm = go.LocateMyFSM("FSM");
                    fsm.GetAction<FloatCompare>("Outside Arena?", 2).float2.Value = Mathf.Infinity;
                    fsm.GetAction<FloatCompare>("Outside Arena?", 3).float2.Value = -Mathf.Infinity;
                }
            );
            control.InsertMethod
            (
                "Plume Gen",
                5,
                () =>
                {
                    GameObject go = control.GetAction<SpawnObjectFromGlobalPool>("Plume Gen", 4).storeObject.Value;
                    PlayMakerFSM fsm = go.LocateMyFSM("FSM");
                    fsm.GetAction<FloatCompare>("Outside Arena?", 2).float2.Value = Mathf.Infinity;
                    fsm.GetAction<FloatCompare>("Outside Arena?", 3).float2.Value = -Mathf.Infinity;
                }
            );
            control.RemoveAction("HUD Out", 0);

            ConstrainPosition cp = pv.GetComponent<ConstrainPosition>();
            cp.xMax = x + castRight.distance;
            cp.xMin = x - castLeft.distance;
        }


        //very broken need to be fixed i think
        [HKCommand("spawncg2")]
        public void SpawnEnragedGuardian()
        {
            var position = HeroController.instance.gameObject.transform.position;
            var cg2 = Object.Instantiate(ObjectLoader.InstantiableObjects["cg2"], position, Quaternion.identity);
            cg2.SetActive(true);
            var miner = cg2.LocateMyFSM("Beam Miner");
            miner.SetState("Battle Init");
            miner.Fsm.GetFsmFloat("Jump Max X").Value = position.x + 20f;
            miner.Fsm.GetFsmFloat("Jump Min X").Value = position.x - 20f;


            //stolen from EnemyRandomizer by Kerr https://github.com/Kerr1291/EnemyRandomizer, thank you for saving my life.
            var actions = miner.FsmStates.Where(x => x.Name == "Aim" || x.Name == "Aim Right" || x.Name == "Aim Left").SelectMany(x => x.Actions);
            foreach (var action in actions)
            {
                if (action is GetPosition gp)
                {
                    if (gp.gameObject.GameObject.Name == "Beam Point R" || gp.gameObject.GameObject.Name == "Beam Point L")
                    {
                        GameObject beamPoint = Object.Instantiate(ObjectLoader.InstantiableObjects[gp.gameObject.GameObject.Name]);
                        FsmGameObject fsmGO = new FsmGameObject(beamPoint);

                        FsmOwnerDefault fsmOwnerDefault = new FsmOwnerDefault
                        {
                            GameObject = fsmGO,
                            OwnerOption = OwnerDefaultOption.UseOwner
                        };

                        gp.gameObject = fsmOwnerDefault;
                    }

                    if (gp.gameObject.GameObject.Name == "Beam Origin")
                    {
                        GameObject beamOrigin = cg2.FindGameObjectInChildren("Beam Origin");
                        FsmGameObject fsmGO = new FsmGameObject(beamOrigin);

                        FsmOwnerDefault fsmOwnerDefault = new FsmOwnerDefault
                        {
                            GameObject = fsmGO,
                            OwnerOption = OwnerDefaultOption.UseOwner
                        };

                        gp.gameObject = fsmOwnerDefault;
                    }
                }
                else if (action is SetPosition sp)
                {
                    if (sp.gameObject.GameObject.Name == "Beam Ball")
                    {
                        GameObject beamBall = Object.Instantiate(ObjectLoader.InstantiableObjects[sp.gameObject.GameObject.Name]);
                        FsmGameObject fsmGO = new FsmGameObject(beamBall);

                        FsmOwnerDefault fsmOwnerDefault = new FsmOwnerDefault
                        {
                            GameObject = fsmGO,
                            OwnerOption = OwnerDefaultOption.UseOwner
                        };

                        sp.gameObject = fsmOwnerDefault;
                    }

                    if (sp.gameObject.GameObject.Name == "Beam")
                    {
                        GameObject beam = Object.Instantiate(ObjectLoader.InstantiableObjects[sp.gameObject.GameObject.Name]);
                        FsmGameObject fsmGO = new FsmGameObject(beam);

                        FsmOwnerDefault fsmOwnerDefault = new FsmOwnerDefault
                        {
                            GameObject = fsmGO,
                            OwnerOption = OwnerDefaultOption.UseOwner
                        };

                        sp.gameObject = fsmOwnerDefault;
                    }
                }
            }
        }

        [HKCommand("revek")]
        [Cooldown(120)]
        public IEnumerator Revek()
        {
            GameObject revek = Object.Instantiate
            (
                ObjectLoader.InstantiableObjects["Revek"],
                HeroController.instance.gameObject.transform.position,
                Quaternion.identity
            );
            
            yield return new WaitForSecondsRealtime(1);

            Object.DontDestroyOnLoad(revek);
            
            revek.SetActive(true);
            
            PlayMakerFSM ctrl = revek.LocateMyFSM("Control");

            // Make sure init gets to run.
            yield return null;
            
            // Actually spawn.
            ctrl.SetState("Appear Pause");

            // ReSharper disable once ImplicitlyCapturedClosure
            void OnUnload() => revek.SetActive(false);

            void OnLoad(Scene a, Scene b)
            {
                revek.SetActive(true);
                
                ctrl.SetState("Appear Pause");
            }
            
            GameManager.instance.UnloadingLevel += OnUnload;
            USceneManager.activeSceneChanged += OnLoad;
            
            yield return new WaitForSecondsRealtime(30);
            
            Object.Destroy(revek);
            
            GameManager.instance.UnloadingLevel -= OnUnload;
            USceneManager.activeSceneChanged -= OnLoad;
        }


        [HKCommand("duplicateboss")]
        public IEnumerator DuplicateBoss()
        {
            if (BossSceneController.Instance == null || BossSceneController.Instance.bosses == null)
                yield break;

            foreach (HealthManager boss in BossSceneController.Instance.bosses)
            {
                Object.Instantiate
                (
                    boss.gameObject,
                    boss.gameObject.transform.position,
                    boss.gameObject.transform.rotation
                );
                
                yield return new WaitForSeconds(0.2f);
            }
        }


        
        [HKCommand("spawnshade")]
        public void SpawnShade()
        {
             Object.Instantiate(GameManager.instance.sm.hollowShadeObject, HeroController.instance.transform.position, Quaternion.identity);
        }
        
        
        
    }
}