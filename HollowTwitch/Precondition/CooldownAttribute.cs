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
        private readonly int _maxUses;

        private int _uses;

        private DateTimeOffset _resetTime;

        private TimeSpan _reset;

        public CooldownAttribute(double seconds, int maxUses = 1)
            => (_reset, _resetTime, _maxUses) = (TimeSpan.FromSeconds(seconds), DateTimeOffset.Now + _reset, maxUses);

        public CooldownAttribute(TimeSpan resetAfter, int maxUses = 1)
            =>  (_reset, _resetTime, _maxUses) = (resetAfter, DateTimeOffset.Now + _reset, maxUses);

        public override bool Check()
        {
            if(DateTimeOffset.Now > _resetTime)
            {
                _resetTime = DateTimeOffset.Now + _reset;

                //in case i mess with threads in the future, not necessary rn with corotines.
                Interlocked.Exchange(ref _uses, _maxUses);
                return true;
            }
            else if(_uses < _maxUses)
            {
                //same thing here
                Interlocked.Increment(ref _uses);
                return true;
            }
            return false;
        }
    }
}
