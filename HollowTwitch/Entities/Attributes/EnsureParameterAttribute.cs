using System;

namespace HollowTwitch.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class EnsureParameterAttribute : Attribute
    {
        public abstract object Ensure(object value);
    }
}
