using System;
using JetBrains.Annotations;

namespace HollowTwitch.Entities.Attributes
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class HKCommandAttribute : Attribute
    {
        public string Name { get; }

        public HKCommandAttribute(string name)
        {
            Name = name;
        }
    }

}
