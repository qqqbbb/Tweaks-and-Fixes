using FMOD;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(CyclopsLocker))]
    internal class CyclopsLocker_
    {
        static Vector3 defaultRotation;
        static Vector3 openAngles = new Vector3(0, 0, 170);

        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(CyclopsLocker __instance)
        {
            if (defaultRotation == default)
                defaultRotation = __instance.rotateTarget.transform.localEulerAngles;

            UnityEngine.Object.Destroy(__instance.rotateTo.gameObject);
        }
        [HarmonyPrefix, HarmonyPatch("ToggleDoor")]
        static bool ToggleDoorPostfix(CyclopsLocker __instance)
        { // open it all the way so it does not go thru PDA
          //AddDebug($"ToggleDoor {__instance.openState} {__instance.rotateTarget.transform.localEulerAngles}");
            __instance.openState = !__instance.openState;
            if (__instance.openState && __instance.openSound != null)
                Utils.PlayFMODAsset(__instance.openSound, __instance.transform, 0f);
            else if (!__instance.openState && __instance.closeSound != null)
                CoroutineHost.StartCoroutine(Util.PlayFMODAsset(__instance.closeSound, __instance.transform.position, .6f));

            __instance.StopAllCoroutines();
            if (__instance.openState)
            {
                __instance.rotateTarget.transform.localEulerAngles = defaultRotation;
                __instance.StartCoroutine(Util.Rotate(__instance.rotateTarget.transform, 1, openAngles));
            }
            else
            {
                __instance.rotateTarget.transform.localEulerAngles = defaultRotation + openAngles;
                __instance.StartCoroutine(Util.Rotate(__instance.rotateTarget.transform, 1, -openAngles));
            }
            return false;
        }
        [HarmonyPrefix, HarmonyPatch("Update")]
        static bool UpdatePrefix(CyclopsLocker __instance)
        {
            return false;
        }
    }
}
