using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Components
{
    public class ApplyShader : MonoBehaviour
    {
        public Material CurrentMaterial;

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, CurrentMaterial);
        }
    }
}
