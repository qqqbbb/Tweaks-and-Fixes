using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Databox_Light
    {
        [HarmonyPatch(typeof(BlueprintHandTarget), "OnTargetUsed")]
        class BlueprintHandTarget_OnTargetUsed_Patch
        {
            public static void Postfix(BlueprintHandTarget __instance)
            {
                Transform light = __instance.transform.Find("DataboxLightContainer");
                if (light)
                    Util.DestroyEntity(light.gameObject);
            }
        }


    }
}
