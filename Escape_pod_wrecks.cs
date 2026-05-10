using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Escape_pod_wrecks
    {
        [HarmonyPatch(typeof(ResourceTracker))]
        class ResourceTracker_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(ResourceTracker __instance)
            {
                if (__instance.techType == TechType.Wreck && __instance.name.StartsWith("life_pod_exploded_"))
                {
                    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
                }
            }
        }
    }
}
