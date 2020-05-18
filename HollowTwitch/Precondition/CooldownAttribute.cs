using HollowTwitch.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Precondition
{
    //barebones cooldown
    public class CooldownAttribute : PreconditionAttribute
    {
        private DateTimeOffset _reset;

        public CooldownAttribute(double seconds)
            => _reset = DateTimeOffset.Now + TimeSpan.FromSeconds(seconds);

        public CooldownAttribute(TimeSpan resetAfter)
            => _reset = DateTimeOffset.Now + resetAfter;

        public override bool Check()
        {
            if(DateTimeOffset.Now > _reset)
            {
                _reset = DateTimeOffset.Now;
                return true;
            }
            return false;
        }
    }
}
