using System;

namespace HollowTwitch.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class RemainingTextAttribute : Attribute { }

}