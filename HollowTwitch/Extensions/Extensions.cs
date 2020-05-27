using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Extensions
{
    public static class Extensions
    {
        public static void Deconstruct(this Vector3 v, out float x, out float y, out float z)
            => (x, y, z) = (v.x, v.y, v.z);
    }
}
