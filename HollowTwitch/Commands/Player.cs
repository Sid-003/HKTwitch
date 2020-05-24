using GlobalEnums;
using HollowTwitch.Entities;
using HollowTwitch.Precondition;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModCommon;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Player
    {
        [HKCommand("naildamage")]
        public void SetNailDamage(int d)
        {
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
                move_direction = -move_direction;
            orig(self, move_direction);
        }

        [HKCommand("spawn")]
        [Cooldown(60, 3)]
        public IEnumerator SpawnEnemy(string name)
        {    
            Modding.Logger.Log("spawn called");
            var enemy = UnityEngine.Object.Instantiate(ObjectLoader.InstantiableObjects[name], HeroController.instance.gameObject.transform.position, Quaternion.identity);
            UnityEngine.Object.DontDestroyOnLoad(enemy);
            yield return new WaitForSeconds(1);
            enemy.SetActive(true);
            Modding.Logger.Log("ended");
        }

        
      
    }
}
