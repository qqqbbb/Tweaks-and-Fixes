using HarmonyLib;
using System;
using UnityEngine;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Flare))]
    internal class Flare_Patch
    {
        static float originalIntensity = -1f;
        [HarmonyPatch(nameof(Flare.OnDraw))] [HarmonyPostfix]
        internal static void OnDraw(Flare __instance)
        {
            if (originalIntensity == -1f)
                originalIntensity = __instance.originalIntensity;

            //ErrorMessage.AddDebug("throwDuration " + __instance.throwDuration);
            __instance.originalIntensity = originalIntensity * Main.config.flareIntensity;
        }

        [HarmonyPatch(typeof(Flare), "OnDrop")]
        class Flare_OnDrop_Patch
        {
            public static bool Prefix(Flare __instance)
            {
                if (__instance.isThrowing)
                {
                    __instance.GetComponent<Rigidbody>().AddForce(MainCamera.camera.transform.forward * __instance.dropForceAmount);
                    __instance.GetComponent<Rigidbody>().AddTorque(__instance.transform.right * __instance.dropTorqueAmount);
                    __instance.isThrowing = false;
                }
                //ErrorMessage.AddDebug("energyLeft " + __instance.energyLeft);
                if (__instance.energyLeft < 1800f)
                {
                    if (__instance.fxControl && !__instance.fxIsPlaying)
                        __instance.fxControl.Play(1);
                    __instance.fxIsPlaying = true;
                }
                return false;
            }
        }

    }
}
