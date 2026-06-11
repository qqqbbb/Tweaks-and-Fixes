using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(WaterPlane))]
    internal class WaterPlane_
    {
        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(WaterPlane __instance)
        { // water surface in Aurora
            //GameObject root = Util.GetEntityRoot(__instance.gameObject);
            //AddDebug("WaterPlane Start " + root.name);
            __instance.transform.DisableShadowCasting();
        }
    }
}
