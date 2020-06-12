using System;
using HollowTwitch.Entities.Attributes;
using UnityEngine;

namespace HollowTwitch.Precondition
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class EnsureFloatAttribute : EnsureParameterAttribute
    {
        private float _min;
        private float _max;


        public EnsureFloatAttribute(float min, float max)
            => (_min, _max) = (min, max);


        public override object Ensure(object value)
            => Mathf.Clamp((float)value, _min, _max);
    }
}
