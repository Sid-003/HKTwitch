using HollowTwitch.Entities;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using Modding;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Enemies
    {
        private readonly Type _palePrince;

        public Enemies()
        {
            if (ModHooks.Instance.LoadedMods.Any(x => x.Contains("PalePrince")))
            {
                Modding.Logger.Log(string.Join(", ", AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("Pale_Prince")).GetTypes().Select(x => x.Name).ToArray()));
                _palePrince = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.Contains("Pale_Prince")).GetTypes().FirstOrDefault(x => x.Name == "Prince");

            }
        }

        [HKCommand("spawn")]
        [Cooldown(60, 3)]
        public IEnumerator SpawnEnemy(string name)
        {
            Modding.Logger.Log("spawn called");
            var enemy = UnityEngine.Object.Instantiate(ObjectLoader.InstantiableObjects[name], HeroController.instance.gameObject.transform.position, Quaternion.identity);
            UnityEngine.Object.DontDestroyOnLoad(enemy);
            yield return new WaitForSecondsRealtime(1);
            enemy.SetActive(true);
            Modding.Logger.Log("ended");
        }


        [HKCommand("spawnpv")]
        public IEnumerator SpawnPureVessel()
        {
            //stolen from https://github.com/SalehAce1/PathOfPureVessel

            yield return null;
            var (x, y, _) = HeroController.instance.gameObject.transform.position;
            var pv = UnityEngine.Object.Instantiate(ObjectLoader.InstantiableObjects["pv"], HeroController.instance.gameObject.transform.position + new Vector3(0, 2.6f), Quaternion.identity);

            pv.SetActive(true);
            //    if (!(_palePrince is null)) pv.AddComponent(_palePrince);  //sad gamer moment

            var castLeft = Physics2D.Raycast(new Vector2(x, y), Vector2.left, 1000, 1 << 8);
            var castRight = Physics2D.Raycast(new Vector2(x, y), Vector2.right, 1000, 1 << 8);

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

            control.InsertMethod("Plume Gen", 3, (() =>
            {
                GameObject go = control.GetAction<SpawnObjectFromGlobalPool>("Plume Gen", 0).storeObject.Value;
                PlayMakerFSM fsm = go.LocateMyFSM("FSM");
                fsm.GetAction<FloatCompare>("Outside Arena?", 2).float2.Value = Mathf.Infinity;
                fsm.GetAction<FloatCompare>("Outside Arena?", 3).float2.Value = -Mathf.Infinity;
            }));
            control.InsertMethod("Plume Gen", 5, (() =>
            {
                GameObject go = control.GetAction<SpawnObjectFromGlobalPool>("Plume Gen", 4).storeObject.Value;
                PlayMakerFSM fsm = go.LocateMyFSM("FSM");
                fsm.GetAction<FloatCompare>("Outside Arena?", 2).float2.Value = Mathf.Infinity;
                fsm.GetAction<FloatCompare>("Outside Arena?", 3).float2.Value = -Mathf.Infinity;
            }));
            control.RemoveAction("HUD Out", 0);

            ConstrainPosition cp = pv.GetComponent<ConstrainPosition>();
            cp.xMax = x + castRight.distance;
            cp.xMin = x - castLeft.distance;
        }


        [HKCommand("duplicateboss")]
        public IEnumerator DuplicateBoss()
        {
            if (BossSceneController.Instance == null || BossSceneController.Instance?.bosses == null)
                yield break;
            foreach(var boss in BossSceneController.Instance?.bosses)
            {
                _ = UnityEngine.Object.Instantiate(boss.gameObject, boss.gameObject.transform.position, boss.gameObject.transform.rotation);
                yield return new WaitForSeconds(0.2f);
            }
            yield break;
        }
    }
}
