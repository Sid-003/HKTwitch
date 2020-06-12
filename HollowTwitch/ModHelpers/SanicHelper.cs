using System.Collections;
using UnityEngine;

namespace HollowTwitch.ModHelpers
{
    public static class SanicHelper
    {
        // Stolen from Sanic Mod by Katie: https://github.com/fifty-six/HollowKnight.Sanic
        static SanicHelper()
        {
            On.GameManager.FreezeMoment_float_float_float_float += OnFreezeMoment;
            On.GameManager.SetTimeScale_float += GameManager_SetTimeScale_1;
        }


        public static float TimeScale;

        private static IEnumerator SetTimeScale(float newTimeScale, float duration)
        {
            float lastTimeScale = Time.timeScale;
            for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
            {
                float val = Mathf.Clamp01(timer / duration);
                SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
                yield return null;
            }

            SetTimeScale(newTimeScale);
        }

        private static void SetTimeScale(float newTimeScale)
        {
            Time.timeScale = (newTimeScale <= 0.01f ? 0f : newTimeScale) * TimeScale;
        }

        private static void GameManager_SetTimeScale_1(On.GameManager.orig_SetTimeScale_float orig, GameManager self, float newTimeScale)
        {
            Time.timeScale = (newTimeScale <= 0.01f ? 0f : newTimeScale) * TimeScale;
        }

        private static IEnumerator OnFreezeMoment
        (
            On.GameManager.orig_FreezeMoment_float_float_float_float orig,
            GameManager self,
            float rampDownTime,
            float waitTime,
            float rampUpTime,
            float targetSpeed
        )
        {
            yield return self.StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
            
            for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime * TimeScale)
                yield return null;
            
            yield return self.StartCoroutine(SetTimeScale(1f, rampUpTime));
        }
    }
}