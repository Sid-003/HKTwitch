using HollowTwitch.Commands.ModHelpers;
using HollowTwitch.Entities;
using HollowTwitch.ModHelpers;
using HollowTwitch.Precondition;
using ModCommon.Util;
using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
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

            IEnumerator GetMaggotPrime()
            {
                var www = UnityWebRequestTexture.GetTexture("https://cdn.discordapp.com/attachments/410556297046523905/715658438654558238/maggotprimebig.png");
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
        [Cooldown(30)]
        public IEnumerator Darken()
        {
            HeroController.instance.vignette.enabled = true;
            HeroController.instance.vignetteFSM.SetState("Dark 2");

            yield return new WaitForSecondsRealtime(20);

            HeroController.instance.vignetteFSM.SetState("Normal");
            HeroController.instance.vignette.enabled = false;
        }

        [HKCommand("timescale")]
        [Summary("Changes the timescale of the game for the time specified.\nTime Limit: [5, 60]\nScale Limit: [0.01, 2]")]
        [Cooldown(60 * 2)]
        public IEnumerator ChangeTimescale([EnsureFloat(0.01f, 2f)]float scale, [EnsureFloat(5, 60)]float seconds)
        {
            Modding.Logger.Log(scale);
            Modding.Logger.Log(seconds);

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
            rigidBody.gravityScale = def;
        }


        private bool _inverted = false;

        [HKCommand("invertcontrols")]
        [Summary("Inverts the move direction of the player.")]
        [Cooldown(30)]
        public void InvertControls()
            => _inverted = !_inverted;
        
        private void InvertControls(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (_inverted)
                move_direction = -move_direction;
            orig(self, move_direction);
        }

        private float _nailScale = 1f;

        [HKCommand("nailscale")]
        [Summary("Makes the nail huge or tiny.")]
        public IEnumerator NailScale([EnsureFloat(1f, 5f)]float nailScale)
        {
            Modding.Logger.Log(nailScale);
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
                self.TakeDamage(null, GlobalEnums.CollisionSide.bottom, 1, 69420);
            return touching;
        }



        [HKCommand("maggotPrime")]
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
        [Cooldown(60 * 10)]
        public IEnumerator ToggleAbility(string ability)
        {
            float time = 60 * 5;
            switch (ability)
            {
                case "dash":
                    PlayerData.instance.canDash ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.canDash ^= true;
                    break;
                case "superdash":
                    PlayerData.instance.canSuperDash ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.canSuperDash ^= true;
                    break;
                case "claw":
                    PlayerData.instance.canWallJump ^= true;
                    yield return new WaitForSecondsRealtime(time);
                    PlayerData.instance.canWallJump ^= true;
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
