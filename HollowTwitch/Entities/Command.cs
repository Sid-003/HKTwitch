using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HollowTwitch.Entities
{
    public class Command
    {
        public string Name { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public int Priority { get; set; }
        public ParameterInfo[] Parameters { get; set; }
        public object ClassInstance { get; set; }
        public IEnumerable<PreconditionAttribute> Preconditions { get; set; }


        public Command(string name, MethodInfo method, object classInstance)
        {
            this.Name = name;
            this.MethodInfo = method;
            this.Parameters = this.MethodInfo.GetParameters();
            this.Priority = Parameters.Length;
            this.ClassInstance = classInstance;
            this.Preconditions = method.GetCustomAttributes(typeof(PreconditionAttribute), false).Cast<PreconditionAttribute>();
        }

    }
}
