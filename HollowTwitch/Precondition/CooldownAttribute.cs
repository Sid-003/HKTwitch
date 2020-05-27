using HollowTwitch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HollowTwitch.Precondition
{
    //barebones cooldown
    public class CooldownAttribute : PreconditionAttribute
    {
        public int MaxUses { get; private set; }

        private int _uses = 0;

        public int Uses { get => _uses;}

        public DateTimeOffset ResetTime { get; private set; }

        public TimeSpan Reset { get; private set; }

        public CooldownAttribute(double seconds, int maxUses = 1)
            => (Reset, ResetTime, MaxUses) = (TimeSpan.FromSeconds(seconds), DateTimeOffset.Now + Reset, maxUses);

        
        public override bool Check()
        {
            if(DateTimeOffset.Now > ResetTime)
            {
                ResetTime = DateTimeOffset.Now + Reset;

                //in case i mess with threads in the future, not necessary rn with corotines.
                Interlocked.Exchange(ref _uses, 0);
            }
            if(Uses < MaxUses)
            {
                //same thing here
                Interlocked.Increment(ref _uses);
                return true;
            }
           
            return false;
        }
    }
}
