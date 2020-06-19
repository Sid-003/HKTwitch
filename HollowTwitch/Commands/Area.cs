using System.Collections;
using System.Collections.Generic;
using HollowTwitch.Entities;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HollowTwitch.Commands
{
    public class Area
    {
        private readonly List<GameObject> _spikePool;

        public Area()
        {
            _spikePool = new List<GameObject>();

            for (int i = 0; i < 20; i++)
            {
                GameObject spike = Object.Instantiate(ObjectLoader.InstantiableObjects["spike"]);
                spike.SetActive(false);
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

        private IEnumerator AddSpikes(float x, float y, float angle, int n, float spacing)
        {
            for (int i = 0; i < n; i++)
            {
                _spikePool[i].SetActive(true);
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

            GameManager.instance.StartCoroutine(AddSpikes(x, y, 0, 10, 2));
            GameManager.instance.StartCoroutine(AddSpikes(x, y, 0, 10, -2));
            
            foreach (GameObject spike in _spikePool)
                spike.LocateMyFSM("Control").SendEvent("EXPAND");
                
            yield break;
        }
    }
}