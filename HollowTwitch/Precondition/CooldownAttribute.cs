using System;
using HollowTwitch.Entities.Attributes;

namespace HollowTwitch.Precondition
{
    public class CooldownAttribute : PreconditionAttribute
    {
        public int MaxUses { get; }

        public int Uses { get; private set; }

        public DateTimeOffset ResetTime { get; private set; }

        public TimeSpan Cooldown { get; internal set; }

        public CooldownAttribute(double seconds, int maxUses = 1) => (Cooldown, ResetTime, MaxUses) = (TimeSpan.FromSeconds(seconds), DateTimeOffset.Now + Cooldown, maxUses);

        public override bool Check(string user)
        {
            return Uses < MaxUses && DateTimeOffset.Now >= ResetTime;
        }

        public override void Use()
        {
            Uses++;
            
            // If we're beyond the cooldown time, reset it.
            if (DateTimeOffset.Now <= ResetTime) 
                return;
            
            ResetTime = DateTimeOffset.Now + Cooldown;

            Uses = 0;
        }
    }
}