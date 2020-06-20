using System;
using HollowTwitch.Entities;

namespace HollowTwitch.Precondition
{
    public class OwnerOnlyAttribute : PreconditionAttribute
    {
        public override bool Check(string user)
        {
            return string.Equals(user, TwitchMod.Instance.Config.Channel, StringComparison.OrdinalIgnoreCase);
        }
    }
}