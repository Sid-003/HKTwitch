using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Entities
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class SummaryAttribute : Attribute
    {
        public string Summary { get; }

        public SummaryAttribute(string summary)
            => this.Summary = summary;
    }
}
