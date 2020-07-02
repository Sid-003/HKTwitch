using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

namespace HollowTwitch.Commands
{
    public class Area
    {
        public Area()
        {

        }

        

        [HKCommand("bees")]
        [Cooldown(100)]
        public void Bees()
        {
            Vector3 pos = HeroController.instance.transform.position;
            
            RaycastHit2D floorHit = Physics2D.Raycast(pos, Vector2.down, 500, 1 << 8);

            if (floorHit && floorHit.point.y < pos.y)
                pos = floorHit.point;

            for (int i = 0; i < 7; i++)
            {
                GameObject bee = Object.Instantiate
                (
                    ObjectLoader.InstantiableObjects["bee"],
                    Vector3.zero,
                    Quaternion.Euler(0, 0, 180)
                );

                bee.SetActive(true);

                PlayMakerFSM ctrl = bee.LocateMyFSM("Control");

                // Set reset vars so they recycle properly
                ctrl.Fsm.GetFsmFloat("X Left").Value = pos.x - 10;
                ctrl.Fsm.GetFsmFloat("X Right").Value = pos.x + 10;
                ctrl.Fsm.GetFsmFloat("Start Y").Value = pos.y + 17 + Random.Range(-3f, 3f);

                // Despawn y
                ctrl.GetAction<FloatCompare>("Swarm", 3).float2.Value = pos.y - 5f;
                
                // Recycle after going oob
                ctrl.ChangeTransition("Reset", "FINISHED", "Pause");
                
                // Start the swarming
                ctrl.SendEvent("SWARM");
            }
        }

        [HKCommand("lasers")]
        [Cooldown(60)]
        public void Lasers()
        {
            Vector3 pos = HeroController.instance.transform.position;

            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 500, 1 << 8);

            // Take the minimum so that we go from the floor
            if (hit && hit.point.y < pos.y)
            {
                pos = hit.point;
            }

            const float MAX_ADD = 10;

            for (int i = -2; i <= 2; i++)
            {
                Vector3 turret_pos = pos + new Vector3(i * 5, MAX_ADD, 0);

                RaycastHit2D up = Physics2D.Raycast(pos, (turret_pos - pos).normalized, 500, 1 << 8);

                // If the ceiling is above where we're going to spawn, put it right beneath the ceiling.
                if (up.point.y > pos.y + 10)
                {
                    turret_pos = up.point + new Vector2(0, -0.5f);
                }

                GameObject turret = Object.Instantiate
                (
                    ObjectLoader.InstantiableObjects["Laser Turret"],
                    turret_pos,
                    Quaternion.Euler(0, 0, 180 + Random.Range(-30f, 30f))
                );

                turret.LocateMyFSM("Laser Bug").AddAction
                (
                    "Init",
                    new WaitRandom
                    {
                        timeMax = .75f,
                        timeMin = 0
                    }
                );

                turret.SetActive(true);
            }
        }

        [HKCommand("spikefloor")]
        [Summary("Spawns spikes.")]
        //[RequireSceneChange]
        public IEnumerator SpikeFloor()
        {
            (float x, float y, _) = HeroController.instance.transform.position;

            List<GameObject> spikes = new List<GameObject>();
 
            Logger.Log("lmao got here but at what cost");
            
            yield return null;

            Logger.Log("the shitty spike is null: " + (ObjectLoader.InstantiableObjects["nkgspike"] == null));

            IEnumerator AddSpikes(float xPos, float yPos, int count, float spacing)
            {
                for (int i = 0; i < count; i++)
                {
                    GameObject spike = Object.Instantiate(ObjectLoader.InstantiableObjects["nkgspike"]);
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(xPos, yPos), Vector2.down, 500, 1 << 8);
                    
                    spike.transform.SetPosition2D(xPos, hit? yPos - hit.distance : yPos);
                    xPos += spacing;

                    spikes.Add(spike);
                    
                    var fsm = spike.LocateMyFSM("Control");
                    fsm.SendEvent("SPIKES READY");
                }
                
                yield break;
            }
            
            
            GameManager.instance.StartCoroutine(AddSpikes(x, y, 5, 5));
            
            GameManager.instance.StartCoroutine(AddSpikes(x, y, 5, -5));

            yield return  new WaitForSeconds(0.55f);
            
            foreach (GameObject spike in spikes)
            {
                spike.LocateMyFSM("Control").SendEvent("SPIKES UP");
            }
            
            yield return new WaitForSeconds(0.2f);

            foreach (var spike in spikes)
            {
                spike.LocateMyFSM("Control").SendEvent("SPIKES DOWN");
            }
            
            Logger.Log("done epic gamer moment");

            yield break;
        }
    }
}