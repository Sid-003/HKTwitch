using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HollowTwitch.Entities
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class HKCommandAttribute : Attribute
    {
        public string Name { get; private set; }

        public HKCommandAttribute(string name)
        {
            this.Name = name;
        }
    }

}
