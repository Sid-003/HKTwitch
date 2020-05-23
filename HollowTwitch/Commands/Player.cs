using GlobalEnums;
using HollowTwitch.Entities;
using HollowTwitch.Precondition;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Player
    {
        [HKCommand("naildamage")]
        public void SetNailDamage(int d)
        {
            Modding.Logger.Log("reached the actual command");
            Modding.Logger.Log("waited and did shit");
            PlayerData.instance.nailDamage = d;       
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMANGE");
        }
        
        [HKCommand("ax2uBlind")]
        [Cooldown(30)]
        public IEnumerator Darken()
        {
            HeroController.instance.vignette.enabled = true;
            HeroController.instance.vignetteFSM.SetState("Dark 2");

            yield return new WaitForSeconds(20);

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
            yield return new WaitForSeconds(seconds * scale);
            Time.timeScale = Time.timeScale == 0f ? 0f : 1f;
        }

        

        [HKCommand("gravity")]
        public IEnumerator ChangeGravity(float scale)
        {
            scale = Mathf.Clamp(scale, 0.2f, 1.69f);
            var rigidBody = ReflectionHelper.GetAttr<HeroController, Rigidbody2D>(HeroController.instance, "rb2d");
            float def = rigidBody.gravityScale;
            rigidBody.gravityScale = scale;
            yield return new WaitForSeconds(30);
            rigidBody.gravityScale = def;
        }


        private bool _inverted = false;

        [HKCommand("invertcontrols")]
        [Cooldown(30)]
        public void InvertControls() 
        {
            if (!_inverted)
            {
                On.HeroController.Move += InvertControls;
                _inverted = true;
            }
            else
                On.HeroController.Move -= InvertControls;
        }
        
        private void InvertControls(On.HeroController.orig_Move orig, HeroController self, float move_direction)
        {
            if (_inverted)
            {
                move_direction = -move_direction;
                Modding.Logger.Log("inverted the ddirection");
            }
            orig(self, move_direction);
        }
    }
}
