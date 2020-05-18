using HollowTwitch.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Player
    {
        [HKCommand("naildamage")]
        public IEnumerator SetNailDamage(int d)
        {
            Modding.Logger.Log("reached the actual command");
            yield return new WaitForSeconds(10f);
            Modding.Logger.Log("waited and did shit");
            PlayerData.instance.nailDamage = d;
            
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMANGE");
        }
        
        [HKCommand("ax2uBlind")]
        public void Darkmod()
        {
            IEnumerator Darken()
            {
                HeroController.instance.vignette.enabled = true;
                HeroController.instance.vignetteFSM.SetState("Dark 2");
                
                yield return new WaitForSeconds(10);
                
                HeroController.instance.vignetteFSM.SetState("Normal");
                HeroController.instance.vignette.enabled = false;
            }
            
            GameManager.instance.StartCoroutine(Darken());
        }
    }
}
