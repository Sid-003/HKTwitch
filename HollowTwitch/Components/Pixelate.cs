using UnityEngine;

namespace HollowTwitch.Components
{
    // Taken from https://github.com/danielshervheim/Image-Effects-for-Unity/blob/master/Assets/Image%20Effects/Scripts/Pixelate.cs except for few changes
    public class Pixelate : MonoBehaviour
    {
        public int height = 100;
        private int cachedHeight;

        public Camera mainCamera;
        private CustomRenderTexture screen;
        private ApplyShader _applyShader;

        private void OnValidate()
        {
            height = (int) Mathf.Max(height, 1f);

            if (cachedHeight == height) return;

            if (screen == null) return;

            screen.Release();

            screen = new CustomRenderTexture((int) (height * mainCamera.aspect), height)
            {
                filterMode = FilterMode.Point
            };
            screen.Create();

            cachedHeight = height;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (mainCamera == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            if (screen == null)
            {
                screen = new CustomRenderTexture((int) (height * mainCamera.aspect), height)
                {
                    filterMode = FilterMode.Point
                };
                screen.Create();

                Graphics.Blit(src, dest);
                return;
            }

            _applyShader ??= gameObject.GetComponent<ApplyShader>();
            
            if (_applyShader && _applyShader.enabled)
            {
                Material _customMat = _applyShader.CurrentMaterial;
                
                if (_customMat != null)
                {
                    screen.material = _customMat;
                }
            }


            Graphics.Blit(src, screen);
            Graphics.Blit(screen, dest);
        }

        private void OnDestroy()
        {
            if (screen != null)
            {
                screen.Release();
            }
        }
    }
}