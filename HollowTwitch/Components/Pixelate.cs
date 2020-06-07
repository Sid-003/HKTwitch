using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HollowTwitch.Components
{
    //taken from https://github.com/danielshervheim/Image-Effects-for-Unity/blob/master/Assets/Image%20Effects/Scripts/Pixelate.cs except for few changes
    public class Pixelate : MonoBehaviour
    {
        public int height = 100;
        int cachedHeight;

        public Camera mainCamera;
        private CustomRenderTexture screen;
        private ApplyShader _applyShader;

        private void OnValidate()
        {
            height = (int)Mathf.Max(height, 1f);

            if (cachedHeight != height)
            {
                if (screen != null)
                {  
                    screen.Release();

                    screen = new CustomRenderTexture((int)(height * mainCamera.aspect), height);
                    screen.filterMode = FilterMode.Point;
                    screen.Create();

                    cachedHeight = height;
                }
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (mainCamera == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            if (screen == null )
            {
                screen = new CustomRenderTexture((int)(height * mainCamera.aspect), height);
                screen.filterMode = FilterMode.Point;
                screen.Create();

                Graphics.Blit(src, dest);
                return;
            }

            _applyShader ??= gameObject.GetComponent<ApplyShader>();
            if (_applyShader?.enabled == true)
            {
                var _customMat = _applyShader?.CurrentMaterial;
                if(_customMat != null)
                {
                    screen.material = _customMat;
                }
            }
          

            Graphics.Blit(src, screen);
            Graphics.Blit(screen, dest);
        }

        void OnDestroy()
        {
            if (screen != null)
            {
                screen.Release();
            }
        }
    }
}
