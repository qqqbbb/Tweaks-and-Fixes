using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class TimeCapsule_
    {
        //[HarmonyPatch(typeof(TimeCapsule), "Start")]
        class TimeCapsule_Start_patch
        {
            public static void Postfix(TimeCapsule __instance)
            {
                //AddDebug("TimeCapsule Start " + __instance.content.activeSelf);
                Util.AttachPing(__instance.gameObject, PingType.Signal, "time capsule");
            }
        }

        [HarmonyPatch(typeof(TimeCapsule), "UpdateVisibility")]
        class TimeCapsule_UpdateVisibility_patch
        {
            public static void Prefix(TimeCapsule __instance)
            {
                //AddDebug("TimeCapsule UpdateVisibility " + __instance.content.activeSelf);
                __instance.visible = !ConfigToEdit.disableTimeCapsule.Value;
            }
        }
    }
}
