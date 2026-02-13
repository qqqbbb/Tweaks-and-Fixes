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
                {
                    LargeWorldEntity lwe = light.GetComponentInChildren<LargeWorldEntity>();
                    if (lwe)
                    {
                        LargeWorldStreamer lws = LargeWorldStreamer.main;
                        if (lws && lws.IsReady())
                        {
                            //AddDebug("UnregisterEntity");
                            lws.cellManager.UnregisterEntity(lwe);
                        }
                    }
                    UnityEngine.Object.Destroy(light.gameObject);
                }
            }
        }


    }
}
