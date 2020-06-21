using System.Collections;
using System.Collections.Generic;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
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

            for (int i = 0; i < SPIKE_COUNT; i++)
            {
                GameObject spike = Object.Instantiate(ObjectLoader.InstantiableObjects["spike"]);
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