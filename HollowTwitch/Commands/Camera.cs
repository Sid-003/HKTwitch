using System;
using System.Collections;
using HollowTwitch.Components;
using HollowTwitch.Entities.Attributes;
using HollowTwitch.Extensions;
using HollowTwitch.Precondition;
using JetBrains.Annotations;
using Vasi;
using On.HutongGames.PlayMaker.Actions;
using UnityEngine;
using Random = UnityEngine.Random;
using UCamera = UnityEngine.Camera;
using UObject = UnityEngine.Object;

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

        private readonly Material _invertMat = new(ObjectLoader.Shaders["Custom/InvertColor"]);

        [HKCommand("cameffect")]
        [Summary("Applies various effects to the camera.\nEffects: Invert, Flip, Nausea, Backwards, Mirror, Pixelate, and Zoom.")]
        [Cooldown(30, 4)]
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
                var values = (CameraEffects[]) Enum.GetValues(typeof(CameraEffects));

                camEffect = values[Random.Range(0, values.Length)];
            }

            tk2dCamera tk2dCam = GameCameras.instance.tk2dCam;
            
            UCamera cam = Mirror.GetField<tk2dCamera, UCamera>(GameCameras.instance.tk2dCam, "_unityCamera");

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (camEffect)
            {
                case CameraEffects.Zoom:
                {
                    tk2dCam.ZoomFactor = 5f;
                    _activeEffects |= camEffect;

                    yield return new WaitForSecondsRealtime(time / 6);

                    tk2dCam.ZoomFactor = 1f;
                    _activeEffects &= ~camEffect;

                    break;
                }
                case CameraEffects.Invert:
                {
                    ApplyShader ivc = cam.gameObject.GetComponent<ApplyShader>() ?? cam.gameObject.AddComponent<ApplyShader>();

                    ivc.CurrentMaterial = _invertMat;
                    ivc.enabled = true;

                    yield return new WaitForSecondsRealtime(time);

                    ivc.enabled = false;

                    break;
                }
                case CameraEffects.Pixelate:
                {
                    Pixelate pix = cam.gameObject.GetComponent<Pixelate>() ?? cam.gameObject.AddComponent<Pixelate>();

                    pix.mainCamera ??= cam;
                    pix.enabled = true;

                    yield return new WaitForSecondsRealtime(time);

                    pix.enabled = false;

                    break;
                }
                case CameraEffects.Backwards:
                {
                    float prev_z = cam.transform.position.z;
                    float new_z = cam.transform.position.z + 80;

                    /*
                     * When you get hit, spell control tries to reset the camera.
                     * This camera reset moves the camera super far back in z
                     * and as a result you get an unusable black screen.
                     *
                     * This prevents that.
                     */
                    void PreventCameraReset(SetPosition.orig_DoSetPosition orig, HutongGames.PlayMaker.Actions.SetPosition self)
                    {
                        if (self.Fsm.Name == "Spell Control" && self.Fsm.ActiveState.Name == "Reset Cam Zoom")
                            return;

                        orig(self);
                    }

                    SetPosition.DoSetPosition += PreventCameraReset;

                    cam.transform.SetPositionZ(new_z);

                    Quaternion prev_rot = cam.transform.rotation;

                    // Rotate around the y-axis to flip the vector.
                    cam.transform.Rotate(Vector3.up, 180);

                    _activeEffects |= CameraEffects.Mirror;

                    // Much shorter than the other effects due to it being a lot harder to play around
                    yield return new WaitForSecondsRealtime(time / 4);

                    SetPosition.DoSetPosition -= PreventCameraReset;

                    _activeEffects ^= CameraEffects.Mirror;

                    // Reset the camera.
                    cam.transform.rotation = prev_rot;
                    cam.transform.SetPositionZ(prev_z);

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

            UCamera cam = Mirror.GetField<tk2dCamera, UCamera>(GameCameras.instance.tk2dCam, "_unityCamera");

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
                // ReSharper disable once SuggestVarOrType_DeconstructionDeclarations
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
        Nausea = 1 << 1,
        Mirror = 1 << 2,
        Zoom = 1 << 3,
        Invert = 1 << 4,
        Pixelate = 1 << 5,
        Backwards = 1 << 6
    }
}