using HollowTwitch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Commands
{
    public class Player
    {
        [HKCommand("naildamage")]
        public void SetNailDamage(int d)
        {
            Modding.Logger.Log("reached the actual command");
            PlayerData.instance.nailDamage = d;
            PlayMakerFSM.BroadcastEvent("UPDATE NAIL DAMANGE");
        }
    }
}
