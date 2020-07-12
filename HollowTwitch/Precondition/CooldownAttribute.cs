using System;
using System.Threading;
using HollowTwitch.Entities.Attributes;

namespace HollowTwitch.Precondition
{
    public class CooldownAttribute : PreconditionAttribute
    {
        public int MaxUses { get; }

        private int _uses;

        public int Uses => _uses;

        public DateTimeOffset ResetTime { get; private set; }

        public TimeSpan Reset { get; internal set; }

        public CooldownAttribute(double seconds, int maxUses = 1) => (Reset, ResetTime, MaxUses) = (TimeSpan.FromSeconds(seconds), DateTimeOffset.Now + Reset, maxUses);

        public override bool Check(string user)
        {
            if (DateTimeOffset.Now > ResetTime)
            {
                ResetTime = DateTimeOffset.Now + Reset;

                // In case i mess with threads in the future, not necessary rn with corotines.
                Interlocked.Exchange(ref _uses, 0);
            }

            if (Uses >= MaxUses) return false;
            
            // Same thing here
            Interlocked.Increment(ref _uses);
            
            return true;

        }
    }
}