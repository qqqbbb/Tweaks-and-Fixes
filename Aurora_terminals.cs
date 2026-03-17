using HarmonyLib;
using Story;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Aurora_terminals
    {

        [HarmonyPatch(typeof(StoryHandTarget))]
        class StoryHandTarget_Patch
        {
            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(StoryHandTarget __instance, GUIHand hand)
            {
                //AddDebug(" isValidHandTarget " + __instance.isValidHandTarget);
                //return !StoryGoalManager.main.IsGoalComplete(__instance.goal.key);
                return __instance.isValidHandTarget;
            }
            //[HarmonyPostfix, HarmonyPatch("OnHandClick")]
            public static void OnHandClickPostfix(StoryHandTarget __instance, GUIHand hand)
            {
                AddDebug("OnHandClick  Postfix ");
            }
        }

        //[HarmonyPatch(typeof(GenericConsole), "Start")]
        class GenericConsole_Start_Patch
        {
            static void Ppstfix(GenericConsole __instance)
            {
                AddDebug(" GenericConsole Start " + __instance.name);
                if (__instance.name == "Aurora_DriveRoom_Console(Clone)" || __instance.name == "Aurora_LivingArea_Console(Clone)")
                {
                    Util.MakeUnmovable(__instance.gameObject);
                }
            }
        }

        //[HarmonyPatch(typeof(GenericConsole), "OnProtoDeserialize")]
        class GenericConsole_OnProtoDeserialize_Patch
        {
            static void Ppstfix(GenericConsole __instance)
            {
                AddDebug(" GenericConsole OnProtoDeserialize " + __instance.name);
                if (__instance.name == "Aurora_DriveRoom_Console(Clone)" || __instance.name == "Aurora_LivingArea_Console(Clone)")
                {
                    Util.MakeUnmovable(__instance.gameObject);
                }
            }
        }


    }
}
