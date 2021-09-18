using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using Vasi;

namespace HollowTwitch.Commands
{
    public class Area
    {
        [HKCommand("bees")]
        [Cooldown(120)]
        [Summary("Hive knight bees.")]
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
        [Summary("Summons crystal peak lasers.")]
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

                turret.LocateMyFSM("Laser Bug").GetState("Init").AddAction
                (
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
        [Summary("Spawns nkg spikes.")]
        public IEnumerator SpikeFloor()
        {
            Vector3 hero_pos = HeroController.instance.transform.position;

            var audio_player = new GameObject().AddComponent<AudioSource>();

            audio_player.volume = GameManager.instance.GetImplicitCinematicVolume();

            var spike_fsms = new List<PlayMakerFSM>();

            const float SPACING = 2.5f;
 
            for (int i = -8; i <= 8; i++)
            {
                GameObject spike = Object.Instantiate(ObjectLoader.InstantiableObjects["nkgspike"]);

                spike.SetActive(true);

                Vector3 pos = hero_pos + new Vector3(i * SPACING, 0);
                
                RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 500, 1 << 8);
                
                pos.y -= hit ? hit.distance : 0;
                
                spike.transform.position = pos;

                PlayMakerFSM ctrl = spike.LocateMyFSM("Control");
                
                spike_fsms.Add(ctrl);
                
                ctrl.SendEvent("SPIKES READY");
            }
            
            audio_player.PlayOneShot(Game.Clips.FirstOrDefault(x => x.name == "grimm_spikes_pt_1_grounded"));
            
            yield return new WaitForSeconds(0.55f);
            
            foreach (PlayMakerFSM spike in spike_fsms)
            {
                spike.SendEvent("SPIKES UP");
            }
            
            yield return new WaitForSeconds(0.15f);
            
            GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
            
            audio_player.PlayOneShot(Game.Clips.FirstOrDefault(x => x.name == "grimm_spikes_pt_2_shoot_up"));
            
            yield return new WaitForSeconds(0.45f);
            
            foreach (PlayMakerFSM spike in spike_fsms)
            {
                spike.SendEvent("SPIKES DOWN");
            }
            
            audio_player.PlayOneShot(Game.Clips.FirstOrDefault(x => x.name == "grimm_spikes_pt_3_shrivel_back"));
            
            yield return new WaitForSeconds(0.5f);

            foreach (GameObject go in spike_fsms.Select(x => x.gameObject))
            {
                Object.Destroy(go);
            }
        }

        [HKCommand("orb")]
        [Cooldown(2)]
        [Summary("Spawns an abs orb.")]
        public IEnumerator SpawnAbsOrb()
        {
            if (HeroController.instance == null)
                yield break;

            GameObject orbgroup = ObjectLoader.InstantiableObjects["AbsOrb"]; // get an go contains orb and it's effect

            GameObject orbPre = orbgroup.transform.Find("Radiant Orb").gameObject;
            
            GameObject ShotCharge_Pre = orbgroup.transform.Find("Shot Charge").gameObject; //get charge effect
            GameObject ShotCharge2_Pre = orbgroup.transform.Find("Shot Charge 2").gameObject;
            
            GameObject ShotCharge = Object.Instantiate(ShotCharge_Pre);
            GameObject ShotCharge2 = Object.Instantiate(ShotCharge2_Pre);


            for (int i = 0; i < 2; i++)
            {
                float x = HeroController.instance.transform.position.x + Random.Range(-7f, 8f);
                float y = HeroController.instance.transform.position.y + Random.Range(4f, 8f);
                var spawnPoint = new Vector3(x, y);

                ShotCharge.transform.position = spawnPoint;
                ShotCharge2.transform.position = spawnPoint;

                ShotCharge.SetActive(true);
                ShotCharge2.SetActive(true);

                ParticleSystem.EmissionModule em = ShotCharge.GetComponent<ParticleSystem>().emission;
                ParticleSystem.EmissionModule em2 = ShotCharge2.GetComponent<ParticleSystem>().emission;

                em.enabled = true; // emit some effect 
                em2.enabled = true;

                yield return new WaitForSeconds(1);

                GameObject orb = orbPre.Spawn(spawnPoint); // Spawn Orb

                orb.GetComponent<Rigidbody2D>().isKinematic = false;
                orb.LocateMyFSM("Orb Control").SetState("Chase Hero");

                em.enabled = false;
                em2.enabled = false;
            }

            Object.Destroy(ShotCharge);
            Object.Destroy(ShotCharge2);
        }
    }
}