using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Databox_
    {
        [HarmonyPatch(typeof(BlueprintHandTarget))]
        class BlueprintHandTarget_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnTargetUsed")]
            public static void OnTargetUsedPostfix(BlueprintHandTarget __instance)
            {
                Transform light = __instance.transform.Find("DataboxLightContainer");
                if (light)
                    Util.DestroyEntity(light.gameObject);
            }
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(BlueprintHandTarget __instance)
            {
                //AddDebug("primaryTooltip " + Language.main.Get(__instance.primaryTooltip));
                //AddDebug("secondaryTooltip " + Language.main.Get(__instance.secondaryTooltip));
                //AddDebug("alreadyUnlockedTooltip " + Language.main.Get(__instance.alreadyUnlockedTooltip));
                __instance.secondaryTooltip = null;
                __instance.alreadyUnlockedTooltip = null;
            }
        }


    }
}
