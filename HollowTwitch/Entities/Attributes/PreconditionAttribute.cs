using System;

namespace HollowTwitch.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public abstract class PreconditionAttribute : Attribute
    {            
        public abstract bool Check(string user);

        public virtual void Use() {}
    }
}