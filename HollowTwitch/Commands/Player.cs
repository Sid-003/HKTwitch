using HollowTwitch.Commands.ModHelpers;
using HollowTwitch.Entities;
using HollowTwitch.Precondition;
using System.Collections;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Player
    {   
        //most(all) of the commands stolen from Chaos Mod by seanpr

        public Player()
        {
            On.HeroController.Move += InvertControls;
            On.NailSlash.StartSlash += ChangeNailScale;
        }

        [HKCommand("naildamage")]
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
        [Cooldown(60 * 2)]
        public IEnumerator ChangeTimescale(float scale, float seconds)
        {
            scale = Mathf.Clamp(scale, 1f, 3f);
            seconds = Mathf.Clamp(seconds, 0, 60);
            Time.timeScale = Time.timeScale == 0f ? 0f : scale;
            yield return new WaitForSecondsRealtime(seconds);
            Time.timeScale = Time.timeScale == 0f ? 0f : 1f;
        }

        

        [HKCommand("gravity")]
        [Cooldown(60 * 2)]
        public IEnumerator ChangeGravity(float scale)
        {
            scale = Mathf.Clamp(scale, 0.2f, 1.90f);
            var rigidBody = HeroController.instance.gameObject.GetComponent<Rigidbody2D>();
            float def = rigidBody.gravityScale;
            rigidBody.gravityScale = scale;
            yield return new WaitForSecondsRealtime(30);
            rigidBody.gravityScale = def;
        }


        private bool _inverted = false;

        [HKCommand("invertcontrols")]
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
        [Cooldown(60 * 2)]
        public IEnumerator NailScale(float nailScale)
        {
            _nailScale = Mathf.Clamp(nailScale, 1f, 5f);
            yield return new WaitForSecondsRealtime(30f);
            _nailScale = 1f;
        }

        private void ChangeNailScale(On.NailSlash.orig_StartSlash orig, NailSlash self)
        {
            orig(self);
            self.transform.localScale *= _nailScale;
        }


        [HKCommand("invisible")]
        [Cooldown(60)]
        public void SetInvisible()
        {
            var renderer = HeroController.instance.GetComponent<MeshRenderer>();
            renderer.enabled ^= true;
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

    }
}
