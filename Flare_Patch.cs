using HarmonyLib;
using System;
using UnityEngine;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Flare))]
    internal class Flare_Patch
    {
        [HarmonyPatch(nameof(Flare.Awake))] [HarmonyPostfix]
        internal static void Awake(Flare __instance)
        {
            //ErrorMessage.AddDebug("throwDuration " + __instance.throwDuration);
            __instance.originalIntensity *= Main.config.flareIntensity;
        }

    }
}
