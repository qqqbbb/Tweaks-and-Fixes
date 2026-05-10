using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Telemetry_
    {
        [HarmonyPatch(typeof(Telemetry))]
        class Telemetry_patch
        {
            [HarmonyPrefix, HarmonyPatch("Awake")]
            public static bool AwakePrefix(Telemetry __instance)
            {
                //Main.logger.LogInfo("Telemetry Awake");
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("Start")]
            public static bool StartPrefix(Telemetry __instance)
            {
                //Main.logger.LogInfo("Telemetry Start");
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("OnEnable")]
            public static bool OnEnablePrefix(Telemetry __instance)
            {
                //Main.logger.LogInfo("Telemetry OnEnable");
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("ScheduledUpdate")]
            public static bool ScheduledUpdatePrefix(Telemetry __instance)
            {
                //Main.logger.LogInfo("Telemetry ScheduledUpdate");
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("OnApplicationQuit")]
            public static bool OnApplicationQuitPrefix(Telemetry __instance)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(AnalyticsController))]
        public static class AnalyticsController_Patch
        {
            [HarmonyPatch("Awake"), HarmonyPrefix]
            public static bool AwakePrefix()
            {
                //Main.logger.LogInfo("AnalyticsController Awake");
                return false;
            }
            [HarmonyPatch("OnEnable"), HarmonyPrefix]
            public static bool OnEnablePrefix()
            {
                //Main.logger.LogInfo("AnalyticsController OnEnable");
                return false;
            }
        }

        [HarmonyPatch(typeof(GameAnalytics.EventData))]
        class GameAnalytics_EventData_patch
        {
            [HarmonyPrefix, HarmonyPatch("Dispose")]
            public static bool DisposePrefix(GameAnalytics.EventData __instance)
            {
                //Main.logger.LogInfo("GameAnalytics.EventData Dispose");
                return false;
            }
        }

        [HarmonyPatch(typeof(GameAnalytics))]
        class GameAnalytics_patch
        {
            [HarmonyPrefix, HarmonyPatch("Send", new Type[] { typeof(GameAnalytics.Event), typeof(bool), typeof(string) })]
            public static bool SendPrefix(GameAnalytics __instance)
            {
                //Main.logger.LogInfo("GameAnalytics Send");
                return false;
            }
            [HarmonyPrefix, HarmonyPatch("Send", new Type[] { typeof(GameAnalytics.EventInfo), typeof(bool), typeof(string) })]
            public static bool SendPrefix_(GameAnalytics __instance)
            {
                //Main.logger.LogInfo("GameAnalytics Send");
                return false;
            }
        }

        [HarmonyPatch(typeof(SentrySdkManager))]
        public static class SentrySdkPatch
        {
            [HarmonyPatch("Awake"), HarmonyPrefix]
            public static bool AwakePrefix()
            {
                //Main.logger.LogInfo("SentrySdkManager Awake");
                return false;
            }
            [HarmonyPatch("OnEnable"), HarmonyPrefix]
            public static bool OnEnablePrefix()
            {
                //Main.logger.LogInfo("SentrySdkManager OnEnable");
                return false;
            }
            [HarmonyPatch("OnDestroy"), HarmonyPrefix]
            public static bool OnDestroyPrefix()
            {
                //Main.logger.LogInfo("SentrySdkManager OnDestroy");
                return false;
            }
        }

    }
}
