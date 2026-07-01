using HarmonyLib;
using Story;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Pings
    {
        [HarmonyPatch(typeof(uGUI_Pings), "IsVisibleNow")]
        class DamageSystem_IsVisibleNow_Patch
        {
            public static void Postfix(uGUI_Pings __instance, ref bool __result)
            {
                //AddDebug("uGUI_Pings IsVisibleNow " + __result);
                if (Player.main == null || ConfigToEdit.disablePingsInSub.Value == false)
                    return;

                if (Player.main.currentEscapePod || Player.main.currentSub && Player.main.currentSub.isCyclops == false)
                {
                    __result = false;
                }
            }
        }

        //[HarmonyPatch(typeof(SignalPing), "Start")]
        public static class SignalPing_Start_Patch
        {
            public static void Prefix(SignalPing __instance)
            {
                {
                    //if (dealer == null)
                    //    AddDebug("cyclops TakeDamage dealer null");
                    //else
                    AddDebug("SignalPing Start " + __instance.name);
                }
            }
        }

        //[HarmonyPatch(typeof(UnlockSignalData), "Trigger")]
        public static class UnlockSignalData_Trigger_Patch
        {
            public static void Prefix(UnlockSignalData __instance)
            {
                AddDebug("UnlockSignalData Trigger " + __instance.targetDescription);
                Main.logger.LogDebug("UnlockSignalData Trigger " + __instance.targetDescription);
            }
        }

        //[HarmonyPatch(typeof(StoryGoal), "Trigger")]
        public static class StoryGoal_Trigger_Patch
        {
            public static void Prefix(StoryGoal __instance)
            {
                if (__instance == null)
                    return;

                AddDebug("StoryGoal Trigger " + __instance.ToString());
                Main.logger.LogDebug("StoryGoal Trigger " + __instance.ToString());
            }
        }

        //[HarmonyPatch(typeof(StoryGoal), "Execute")]
        public static class StoryGoal_Execute_Patch
        {
            public static void Prefix(StoryGoal __instance, string key, Story.GoalType goalType)
            {
                if (__instance == null)
                    return;

                AddDebug("StoryGoal Execute " + __instance.ToString());
                Main.logger.LogDebug("StoryGoal Execute " + __instance.ToString());
            }
        }

        //[HarmonyPatch(typeof(PingManager), "Register")]
        public static class PingManager_Register_Patch
        {
            public static bool Prefix(PingInstance instance)
            {
                if (instance == null)
                    return false;

                //AddDebug($"PingManager Register {instance._label} {instance.origin.position}");
                Main.logger.LogDebug($"PingManager Register {instance._label} {instance.origin.position}");
                return true;
            }
        }

        //[HarmonyPatch(typeof(uGUI_Pings), "OnAdd")]
        public static class uGUI_Pings_OnAdd_Patch
        {
            public static bool Prefix(uGUI_Pings __instance, PingInstance instance)
            {
                if (instance == null)
                    return false;

                //AddDebug($"PingManager Register {instance._label} {instance.origin.position}");
                Main.logger.LogDebug($"uGUI_Pings OnAdd {instance._label} {instance.origin.position}");
                return true;
            }
        }


    }
}
