using System;

namespace HollowTwitch.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class SummaryAttribute : Attribute
    {
        public string Summary { get; }

        public SummaryAttribute(string summary)
            => Summary = summary;
    }
}
