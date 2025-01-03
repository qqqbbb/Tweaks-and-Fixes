using HarmonyLib;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(DayNightCycle))]
    class DayNightCycle_Patch
    {
        static bool skipTimeModeStopped;

        [HarmonyPostfix, HarmonyPatch("Awake")]
        static void AwakePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPrefix, HarmonyPatch("Update")]
        static void UpdatePrefix(DayNightCycle __instance)
        {
            if (!__instance.debugFreeze && __instance.skipTimeMode)
            {
                double timePassed = __instance.timePassedAsDouble + __instance.deltaTime;
                if (timePassed >= __instance.skipModeEndTime)
                    skipTimeModeStopped = true;
            }
        }
        [HarmonyPostfix, HarmonyPatch("Update")]
        static void UpdatePostfix(DayNightCycle __instance)
        {
            if (skipTimeModeStopped)
            {
                skipTimeModeStopped = false;
                __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
            }
        }
        [HarmonyPostfix, HarmonyPatch("Resume")]
        static void ResumePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("OnConsoleCommand_night")]
        static void OnConsoleCommand_nightPostfix(DayNightCycle __instance, NotificationCenter.Notification n)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("OnConsoleCommand_day")]
        static void OnConsoleCommand_dayPostfix(DayNightCycle __instance, NotificationCenter.Notification n)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("OnConsoleCommand_daynight")]
        static void OnConsoleCommand_daynightPostfix(DayNightCycle __instance, NotificationCenter.Notification n)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }
        [HarmonyPostfix, HarmonyPatch("StopSkipTimeMode")]
        static void StopSkipTimeModePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }



    }
}
