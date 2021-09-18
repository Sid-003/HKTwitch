using System;
using System.Collections;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker;
using JetBrains.Annotations;
using Modding;
using MonoMod.RuntimeDetour;
using On.HutongGames.PlayMaker.Actions;
using UnityEngine;
using Random = UnityEngine.Random;
using UObject = UnityEngine.Object;

namespace HollowTwitch.Commands
{
    [UsedImplicitly]
    public class Game
    {
        internal static AudioClip[] Clips { get; private set; }

        public Game()
        {
            // Just for the side effects.
            Resources.LoadAll("");

            Clips = Resources.FindObjectsOfTypeAll<AudioClip>();
        }

        [HKCommand("setText")]
        [Summary("Sets every game text to the text provided.")]
        [Cooldown(80)]
        public IEnumerator Text([RemainingText]string msg)
        {
            string OnLangGet(string key, string title, string orig) => msg;

            ModHooks.LanguageGetHook += OnLangGet;

            yield return new WaitForSeconds(20f);

            ModHooks.LanguageGetHook -= OnLangGet;
        }

        [HKCommand("heal")]
        [Cooldown(60)]
        public void Heal()
        {
            if (Random.Range(1, 3) == 1)
            {
                HeroController.instance.MaxHealth();
                
                return;
            }

            foreach (HealthManager hm in UObject.FindObjectsOfType<HealthManager>())
            {
                hm.hp *= 3;
                hm.hp /= 2;
            }
        }

        [HKCommand("rng")]
        [Summary("YUP RNG.")]
        [Cooldown(120)]
        public IEnumerator RNG()
        {
            static void OnWait(Wait.orig_OnEnter orig, HutongGames.PlayMaker.Actions.Wait self)
            {
                FsmFloat orig_time = self.time;

                self.time = Random.Range(0, 3f * self.time.Value);

                orig(self);

                self.time = orig_time;
            }

            static void OnWaitRandom(WaitRandom.orig_OnEnter orig, HutongGames.PlayMaker.Actions.WaitRandom self)
            {
                FsmFloat orig_time_min = self.timeMin;
                FsmFloat orig_time_max = self.timeMax;

                self.timeMin = Random.Range(0, self.timeMin.Value * 3f);
                self.timeMax = Random.Range(0, self.timeMax.Value * 3f);

                orig(self);

                self.timeMin = orig_time_min;
                self.timeMax = orig_time_max;
            }

            static void AnimPlay
            (
                On.tk2dSpriteAnimator.orig_Play_tk2dSpriteAnimationClip_float_float orig,
                tk2dSpriteAnimator self,
                tk2dSpriteAnimationClip clip,
                float start,
                float fps
            )
            {
                float orig_fps = clip.fps;

                clip.fps = Random.Range(clip.fps / 4, clip.fps * 4);
                
                orig(self, clip, start, fps);

                clip.fps = orig_fps;
            }

            Wait.OnEnter += OnWait;
            WaitRandom.OnEnter += OnWaitRandom;
            On.tk2dSpriteAnimator.Play_tk2dSpriteAnimationClip_float_float += AnimPlay;

            for (int i = 0; i < 12; i++)
            {
                foreach (Rigidbody2D rb2d in UObject.FindObjectsOfType<Rigidbody2D>())
                {
                    Vector2 vel = rb2d.velocity;
                    
                    rb2d.velocity = new Vector2
                    (
                        Random.Range(-2 * vel.x, 2 * vel.y),
                        Random.Range(-2 * vel.y, 2 * vel.y)
                    );
                }

                yield return new WaitForSeconds(4f);
            }
            
            Wait.OnEnter -= OnWait;
            WaitRandom.OnEnter -= OnWaitRandom;
            On.tk2dSpriteAnimator.Play_tk2dSpriteAnimationClip_float_float -= AnimPlay;
        }


        [HKCommand("sfxRando")]
        [Cooldown(180)]
        [Summary("Randomizes sfx sounds.")]
        public IEnumerator SfxRando()
        {
            var oneShotHook = new Hook
            (
                typeof(AudioSource).GetMethod("PlayOneShot", new[] {typeof(AudioClip), typeof(float)}),
                new Action<Action<AudioSource, AudioClip, float>, AudioSource, AudioClip, float>(PlayOneShot)
            );

            var playHook = new Hook
            (
                typeof(AudioSource).GetMethod("Play", Type.EmptyTypes),
                new Action<Action<AudioSource>, AudioSource>(Play)
            );

            yield return new WaitForSecondsRealtime(60f);

            oneShotHook.Dispose();
            playHook.Dispose();
        }

        private static void PlayOneShot(Action<AudioSource, AudioClip, float> orig, AudioSource self, AudioClip clip, float volumeScale)
        {
            orig(self, Clips[Random.Range(0, Clips.Length - 1)], volumeScale);
        }

        private static void Play(Action<AudioSource> orig, AudioSource self)
        {
            AudioClip orig_clip = self.clip;

            self.clip = Clips[Random.Range(0, Clips.Length - 1)];

            orig(self);

            self.clip = orig_clip;
        }
    }
}