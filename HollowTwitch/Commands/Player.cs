using GlobalEnums;
using HollowTwitch.Commands.ModHelpers;
using HollowTwitch.Entities;
using HollowTwitch.ModHelpers;
using HollowTwitch.Precondition;
using HutongGames.PlayMaker;
using ModCommon.Util;
using Modding;
using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace HollowTwitch.Commands
{
    public class Player
    {
        //most(all) of the commands stolen from Chaos Mod by seanpr
        private GameObject _maggot;
     
        public Player()
        {
            On.HeroController.Move += InvertControls;
            On.NailSlash.StartSlash += ChangeNailScale;
            On.HeroController.CheckTouchingGround += OnTouchingGround;
            ModHooks.Instance.DashVectorHook += OnDashVector;
            IEnumerator GetMaggotPrime()
            {
                var www = UnityWebRequestTexture.GetTexture("https://cdn.discordapp.com/attachments/410556297046523905/716824653280313364/hwurmpU.png");
                yield return www.SendWebRequest();

                Texture texture = DownloadHandlerTexture.GetContent(www);
                var maggotPrime = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                Modding.Logger.Log("createed texture");
                _maggot = new GameObject("maggot");
                _maggot.AddComponent<SpriteRenderer>().sprite = maggotPrime;
                _maggot.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(_maggot);
            
            }

            GameManager.instance.StartCoroutine(GetMaggotPrime());
        }

        [HKCommand("naildamage")]
        [Summary("Allows users to set the nail damage for 30 seconds.")]
        [Cooldown(60)]
        public IEnumerator SetNailDamage(int d)
        {
            var defNailDamage = PlayerData.instance.nailDamage;
            PlayerData.instance.nailDamage = d;       
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMANGE");
            yield return new WaitForSecondsRealtime(30);
            PlayerData.instance.nailDamage = defNailDamage;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMANGE");
        }
        
        
        [HKCommand("ax2uBlind")]
        [Summary("Enables darkness for some time.")]
        [Cooldown(60)]
        public IEnumerator Blind()
        {
            DarknessHelper.Darken();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoad;
            yield return new WaitForSecondsRealtime(30);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoad;
            DarknessHelper.Lighten();
        }

        private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
        {
            if (HeroController.instance == null) return;
            DarknessHelper.Darken();
        }

        [HKCommand("timescale")]
        [Summary("Changes the timescale of the game for the time specified.")]
        [Cooldown(60 * 2)]
        public IEnumerator ChangeTimescale([EnsureFloat(0.01f, 2f)]float scale, [EnsureFloat(5, 60)]float seconds)
        {
            SanicHelper.TimeScale = scale;
            Time.timeScale = Time.timeScale == 0 ? 0 : scale;
            yield return new WaitForSecondsRealtime(seconds);
            Time.timeScale = Time.timeScale == 0 ? 0f : 1;
            SanicHelper.TimeScale = 1;
        }

        [HKCommand("gravity")]
        [Summary("Changes the gravity to the specified scale. Scale Limit: [0.2, 1.9]")]
        [Cooldown(60 * 2)]
        public IEnumerator ChangeGravity([EnsureFloat(0.2f, 1.90f)]float scale)
        {
            var rigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
            float def = rigidBody.gravityScale;
            rigidBody.gravityScale = scale;
            yield return new WaitForSecondsRealtime(30);
            rigidBody.gravityScale = 0.7f;
        }


        private bool _inverted = false;
        private bool _slippery = false;
        private float _lastMoveDir = 0;

        [HKCommand("invertcontrols")]
        [Summary("Inverts the move direction of the player.")]
        [Cooldown(60)]
        public IEnumerator InvertControls()
        {
            _inverted = true;
            yield return new WaitForSecondsRealtime(60);
            _inverted = false;
        }
        
        [HKCommand("slippery")]
        [Summary("Makes the floor have no friction at all. Lasts for 60 seconds.")]
        public IEnumerator Slipery()
        {
            _slippery = true;
            yield return new WaitForSecondsRealtime(60);
            _slippery = false;
        }

        private void InvertControls(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
            {
                orig(self, move_direction);
                return;
            }

            if (move_direction == 0f && _slippery) 
                move_direction = _lastMoveDir == 0f ? 1f : _lastMoveDir;

            if (_inverted)
                move_direction = -move_direction;
            orig(self, move_direction);

            _lastMoveDir = move_direction;
            
        }
        private Vector2 OnDashVector(Vector2 change)
        {
            if (_inverted)
                return -1 * change;
            return change;
        }

        private float _nailScale = 1f;

        [HKCommand("nailscale")]
        [Summary("Makes the nail huge or tiny.")]
        public IEnumerator NailScale([EnsureFloat(1f, 5f)]float nailScale)
        {
            _nailScale = nailScale;
            yield return new WaitForSecondsRealtime(30f);
            _nailScale = 1f;
        }

        private void ChangeNailScale(On.NailSlash.orig_StartSlash orig, NailSlash self)
        {
            orig(self);
            self.transform.localScale *= _nailScale;
        }

        [HKCommand("bindings")]
        [Cooldown(60 * 5)]
        public IEnumerator EnableBindings()
        {
            BindingsHelper.AddDetours();
            On.BossSceneController.RestoreBindings += BindingsHelper.NoOp;
            On.GGCheckBoundSoul.OnEnter += BindingsHelper.CheckBoundSoulEnter;
            BindingsHelper.ShowIcons();
            yield return new WaitForSecondsRealtime(60);     
            BindingsHelper.Unload();
        }

        private bool _floorislava = false;
        [HKCommand("floorislava")]
        [Cooldown(60 * 2)]
        public IEnumerator FloorIsLava([EnsureFloat(10, 60)]float seconds)
        {
            //stolen from zaliant
            _floorislava = true;
            yield return new WaitForSecondsRealtime(seconds);
            _floorislava = false;
        }

        private bool OnTouchingGround(On.HeroController.orig_CheckTouchingGround orig, HeroController self)
        {
            var touching = orig(self);
            if (touching && _floorislava && !GameManager.instance.IsInSceneTransition)
                self.TakeDamage(null, CollisionSide.bottom, 1, 69420);
            return touching;
        }

        [HKCommand("hwurmpU")]
        [Summary("I don't even know honestly.")]
        [Cooldown(60 * 5)]
        public IEnumerator EnableMaggotPrimeSkin()
        {
            var go = UnityEngine.Object.Instantiate(_maggot, HeroController.instance.transform);
            go.SetActive(true);
            var renderer = HeroController.instance.GetComponent<MeshRenderer>();
            renderer.enabled = false;
            yield return new WaitForSecondsRealtime(60 * 2);
            UnityEngine.Object.DestroyImmediate(go);
            renderer.enabled = true;
        }

        [HKCommand("toggle")]
        [Summary("Toogles an ability for 45 seconds.")]
        [Cooldown(60 * 4)]
        public IEnumerator ToggleAbility(string ability)
        {
            float time = 45;
            switch (ability)
            {
                case "dash":
                    PlayerData.instance.hasDash ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.hasDash ^= true;
                    break;
                case "superdash":
                    PlayerData.instance.hasSuperDash ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.hasSuperDash ^= true;
                    break;
                case "claw":
                    PlayerData.instance.hasWalljump ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.hasWalljump ^= true;
                    break;
                case "wings":
                    PlayerData.instance.hasDoubleJump ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.hasDoubleJump ^= true;
                    break;
            }

            yield break;
        }
    }
}
