using System;

namespace HollowTwitch.Entities
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public abstract class PreconditionAttribute : Attribute
    {            
        public abstract bool Check();    
    }
}
