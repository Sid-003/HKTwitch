using System.Collections;
using System.Collections.Generic;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HollowTwitch.Commands
{
    public class Area
    {
        private const int SPIKE_COUNT = 20;

        private readonly List<GameObject> _spikePool;

        public Area()
        {
            _spikePool = new List<GameObject>();

            GameObject cave_spike = ObjectLoader.InstantiableObjects["cave_spikes"];

            var orig_tink = cave_spike.GetComponent<TinkEffect>();

            for (int i = 0; i < SPIKE_COUNT; i++)
            {
                GameObject spike = Object.Instantiate(ObjectLoader.InstantiableObjects["spike"]);

                var tink = spike.AddComponent<TinkEffect>();

                tink.blockEffect = orig_tink.blockEffect;

                spike.AddComponent<AudioSource>();

                Object.DontDestroyOnLoad(spike);

                _spikePool.Add(spike);
            }

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (_spikePool is null || _spikePool.Count == 0) return;

            foreach (GameObject spike in _spikePool)
                spike.SetActive(false);
        }

        private IEnumerator AddSpikes(float x, float y, float angle, int start, float spacing)
        {
            for (int i = start; i < start + (_spikePool.Count / 2); i++)
            {
                _spikePool[i].transform.SetPosition2D(x, y);
                x += spacing;
            }

            yield break;
        }

        [HKCommand("lasers")]
        [Cooldown(60)]
        public void Lasers()
        {
            Vector3 pos = HeroController.instance.transform.position;
            
            for (int i = -2; i <= 2; i++)
            {
                GameObject turret = Object.Instantiate
                (
                    ObjectLoader.InstantiableObjects["Laser Turret"],
                    pos + new Vector3(i * 5, 10, 0),
                    Quaternion.Euler(0, 0, 180 + Random.Range(-30f, 30f))
                );

                turret.LocateMyFSM("Laser Bug").AddAction("Init", new WaitRandom()
                {
                    timeMax = .75f,
                    timeMin = 0
                });
                
                turret.SetActive(true);
            }
        }

        [HKCommand("spikefloor")]
        [Summary("Spawns spikes.")]
        [RequireSceneChange]
        public IEnumerator SpikeFloor()
        {
            (float x, float y, _) = HeroController.instance.transform.position;

            RaycastHit2D hit = Physics2D.Raycast(new Vector2(x, y), Vector2.down, 500, 1 << 8);

            if (hit)
                y -= hit.distance;

            GameManager.instance.StartCoroutine(AddSpikes(x, y + 2f, 0, 0, 2));
            GameManager.instance.StartCoroutine(AddSpikes(x, y + 2f, 0, 10, -2));

            foreach (GameObject spike in _spikePool)
            {
                spike.SetActive(true);

                spike.LocateMyFSM("Control").SendEvent("EXPAND");
            }

            Logger.Log("done epic gamer moment");

            yield break;
        }
    }
}