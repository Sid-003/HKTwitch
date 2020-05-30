using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Entities.Attributes
{
    [System.AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = true)]
    public abstract class EnsureParameterAttribute : Attribute
    {
        public abstract object Ensure(object value);
    }
}
