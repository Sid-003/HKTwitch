using System;
using System.Collections;
using HollowTwitch.Entities.Attributes;
using JetBrains.Annotations;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace HollowTwitch.Commands
{
    [UsedImplicitly]
    public class Game
    {
        private readonly AudioClip[] _clips;

        public Game()
        {
            // Just for the side effects.
            Resources.LoadAll("");

            _clips = Resources.FindObjectsOfTypeAll<AudioClip>();

        }

        [HKCommand("sfxRando")]
        public IEnumerator SfxRando()
        {
            for (int i = 0; i < 20; i++)
            {
                Modding.Logger.Log("You are an idiot");
            }
            
            var oneShotHook = new Hook
            (
                typeof(AudioSource).GetMethod("PlayOneShot", new[] {typeof(AudioClip), typeof(float)}),
                new Action<Action<AudioSource, AudioClip, float>, AudioSource, AudioClip, float>(PlayOneShot)
            );
            
            var playHook = new Hook
            (
                typeof(AudioSource).GetMethod("Play", new Type[0]),
                new Action<Action<AudioSource>, AudioSource>(Play)
            );
            
            yield return new WaitForSecondsRealtime(60f);
            
            oneShotHook.Dispose();
            playHook.Dispose();
        }
        
        private void PlayOneShot(Action<AudioSource, AudioClip, float> orig, AudioSource self, AudioClip clip, float volumeScale)
        {
            orig(self, _clips[UnityEngine.Random.Range(0, _clips.Length - 1)], volumeScale);
        }

        private void Play(Action<AudioSource> orig, AudioSource self)
        {
            AudioClip orig_clip = self.clip;
            
            self.clip = _clips[UnityEngine.Random.Range(0, _clips.Length - 1)];
            
            orig(self);

            self.clip = orig_clip;
        }
    }
}