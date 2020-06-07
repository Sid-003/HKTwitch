using HollowTwitch.Components;
using HollowTwitch.Entities;
using HollowTwitch.Extensions;
using ModCommon.Util;
using System;
using System.Collections;
using UnityEngine;

namespace HollowTwitch.Commands
{
    public class Caméra
    {
        public Caméra()
        {
            On.tk2dCamera.UpdateCameraMatrix += OnUpdateCameraMatrix;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (s1, s2) =>
            {
                if (s2.name == "Main_Menu")
                {
                    _activeEffects = default;
                }
            };
        }

        private Matrix4x4 _reflectMatrix = Matrix4x4.identity;
        private CameraEffects _activeEffects;

        private Material _defaultMat = new Material(Shader.Find("Sprites/Default-ColorFlash"));
        private Material _invertMat = new Material(ObjectLoader.Shaders["Custom/InvertColor"]);

        [HKCommand("cameffect")]
        public IEnumerator AddEffect(string effect)
        {
            float time = 60f;
            var e = Enum.Parse(typeof(CameraEffects), effect, true);
            if(e != null)
            {
                var cameffect = (CameraEffects)e;
                switch (cameffect)
                {
                    case CameraEffects.Zoom:             
                        GameCameras.instance.tk2dCam.ZoomFactor = 5f;
                        _activeEffects |= cameffect;
                        yield return new WaitForSecondsRealtime(time);
                        GameCameras.instance.tk2dCam.ZoomFactor = 1f;
                        _activeEffects &= ~cameffect;
                        break;
                    case CameraEffects.Invert:
                        {
                            var cam = GameCameras.instance.tk2dCam.GetAttr<tk2dCamera, Camera>("_unityCamera");
                            var ivc = cam.gameObject.GetComponent<ApplyShader>() ?? cam.gameObject.AddComponent<ApplyShader>();
                            ivc.CurrentMaterial = _invertMat;
                            ivc.enabled = true;
                            yield return new WaitForSecondsRealtime(time);
                            ivc.enabled = false;
                            break;
                        }
                    case CameraEffects.Pixelate:
                        {
                            var cam = GameCameras.instance.tk2dCam.GetAttr<tk2dCamera, Camera>("_unityCamera");
                            var pix = cam.gameObject.GetComponent<Pixelate>() ?? cam.gameObject.AddComponent<Pixelate>();
                            pix.mainCamera ??= cam;
                            pix.enabled = true;
                            yield return new WaitForSecondsRealtime(time);
                            pix.enabled = false;
                            break;
                        }
                    default:
                        _activeEffects |= cameffect;
                        yield return new WaitForSecondsRealtime(time);
                        _activeEffects &= ~cameffect;
                        break;
                }             
            }
            
        }

        private void OnUpdateCameraMatrix(On.tk2dCamera.orig_UpdateCameraMatrix orig, tk2dCamera self)
        {
            orig(self);
            var cam = GameCameras.instance?.tk2dCam?.GetAttr<tk2dCamera, Camera>("_unityCamera");
            if (cam == null)
                return;
            Matrix4x4 p = cam.projectionMatrix;
            if (_activeEffects.HasValue(CameraEffects.Nausea))
            {                   
                p.m01 += Mathf.Sin(Time.time * 1.2f) * 1f;
                p.m10 += Mathf.Sin(Time.time * 1.5f) * 1f;
            }
            if(_activeEffects.HasValue(CameraEffects.Flip))
            {
                _reflectMatrix[1, 1] = -1;
                p *= _reflectMatrix;        
            }
            if (_activeEffects.HasValue(CameraEffects.Mirror))
            {
                _reflectMatrix[0, 0] = -1;
                p *= _reflectMatrix;

            }
            if (_activeEffects.HasValue(CameraEffects.Zoom))
            {
                var (x, y, _) = HeroController.instance.gameObject.transform.position;
                GameManager.instance.cameraCtrl.SnapTo(x, y);
            }
            cam.projectionMatrix = p;
        }
    }

    [Flags]
    public enum CameraEffects
    {
        Flip = 1,
        Nausea = 2,
        Mirror = 4,
        Zoom = 8,
        Invert = 16,
        Pixelate = 32,
    }
}
