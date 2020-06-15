﻿using System;
using System.Collections;
using HollowTwitch.Components;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using JetBrains.Annotations;
using ModCommon.Util;
using UnityEngine;
using UCamera = UnityEngine.Camera;

namespace HollowTwitch.Commands
{
    [UsedImplicitly]
    public class Camera
    {
        public Camera()
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

        private readonly Material _invertMat = new Material(ObjectLoader.Shaders["Custom/InvertColor"]);

        [HKCommand("cameffect")]
        public IEnumerator AddEffect(string effect)
        {
            const float time = 60f;

            CameraEffects camEffect;

            try
            {
                camEffect = (CameraEffects) Enum.Parse(typeof(CameraEffects), effect, true);
            }
            // Couldn't parse the effect, we'll go with a random one (at least for now).
            catch (ArgumentException)
            {
                CameraEffects[] values = (CameraEffects[]) Enum.GetValues(typeof(CameraEffects));

                camEffect = values[UnityEngine.Random.Range(0, values.Length)];
            }

            switch (camEffect)
            {
                case CameraEffects.Zoom:
                {
                    GameCameras.instance.tk2dCam.ZoomFactor = 5f;
                    _activeEffects |= camEffect;
                    yield return new WaitForSecondsRealtime(time);
                    GameCameras.instance.tk2dCam.ZoomFactor = 1f;
                    _activeEffects &= ~camEffect;
                    break;
                }
                case CameraEffects.Invert:
                {
                    UCamera cam = GameCameras.instance.tk2dCam.GetAttr<tk2dCamera, UCamera>("_unityCamera");
                    ApplyShader ivc = cam.gameObject.GetComponent<ApplyShader>() ?? cam.gameObject.AddComponent<ApplyShader>();
                    
                    ivc.CurrentMaterial = _invertMat;
                    ivc.enabled = true;
                    
                    yield return new WaitForSecondsRealtime(time);
                    
                    ivc.enabled = false;
                    
                    break;
                }
                case CameraEffects.Pixelate:
                {
                    UCamera cam = GameCameras.instance.tk2dCam.GetAttr<tk2dCamera, UCamera>("_unityCamera");
                    Pixelate pix = cam.gameObject.GetComponent<Pixelate>() ?? cam.gameObject.AddComponent<Pixelate>();
                    
                    pix.mainCamera ??= cam;
                    pix.enabled = true;
                    
                    yield return new WaitForSecondsRealtime(time);
                    
                    pix.enabled = false;
                    
                    break;
                }
                default:
                    _activeEffects |= camEffect;
                    yield return new WaitForSecondsRealtime(time);
                    _activeEffects &= ~camEffect;
                    break;
            }
        }

        private void OnUpdateCameraMatrix(On.tk2dCamera.orig_UpdateCameraMatrix orig, tk2dCamera self)
        {
            orig(self);
            
            // Can't use ?. on a Unity type because they override == to null.
            if (GameCameras.instance == null || GameCameras.instance.tk2dCam == null)
                return;

            UCamera cam = GameCameras.instance.tk2dCam.GetAttr<tk2dCamera, UCamera>("_unityCamera");

            if (cam == null)
                return;
            
            Matrix4x4 p = cam.projectionMatrix;
            
            if (_activeEffects.HasValue(CameraEffects.Nausea))
            {
                p.m01 += Mathf.Sin(Time.time * 1.2f) * 1f;
                p.m10 += Mathf.Sin(Time.time * 1.5f) * 1f;
            }

            if (_activeEffects.HasValue(CameraEffects.Flip))
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
        Pixelate = 32
    }
}