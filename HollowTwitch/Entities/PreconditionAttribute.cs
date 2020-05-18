using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Entities
{
    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public abstract class PreconditionAttribute : Attribute
    {            
        public abstract bool Check();    
    }
}
