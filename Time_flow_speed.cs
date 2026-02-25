using HarmonyLib;
using Nautilus.Options;
using Nautilus.Utility;
using Story;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(DayNightCycle))]
    class DayNightCycle_
    {
        static bool skipTimeMode;

        public static void UpdateNightDuration()
        {
            if (DayNightCycle.main == null)
                return;

            DayNightCycle dnc = DayNightCycle.main;
            float nightScalar = Util.NormalizeTo01range(ConfigMenu.nightDuration.Value, 0, 24);
            //AddDebug("nightScalar " + nightScalar);
            dnc.sunRiseTime = nightScalar * .5f;
            dnc.sunSetTime = 1 - dnc.sunRiseTime;
            dnc.UpdateAtmosphere();
        }

        [HarmonyPostfix, HarmonyPatch("Awake")]
        static void AwakePostfix(DayNightCycle __instance)
        {
            __instance._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
            UpdateNightDuration();
        }
        [HarmonyPrefix, HarmonyPatch("Update")]
        static void UpdatePrefix(DayNightCycle __instance)
        {
            skipTimeMode = __instance.skipTimeMode;
        }
        [HarmonyPostfix, HarmonyPatch("Update")]
        static void UpdatePostfix(DayNightCycle __instance)
        {
            if (skipTimeMode && __instance.skipTimeMode == false)
            {
                skipTimeMode = false;
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

    [HarmonyPatch(typeof(StoryGoalScheduler), "Schedule")]
    class StoryGoalScheduler_Schedule_patch
    {
        public static void Prefix(StoryGoalScheduler __instance, StoryGoal goal)
        {
            if (ConfigMenu.timeFlowSpeed.Value == 1)
                return;

            goal.delay *= ConfigMenu.timeFlowSpeed.Value;
            //AddDebug("StoryGoalScheduler Schedule " + goal.key + " delay " + goal.delay);
        }
    }

    [HarmonyPatch(typeof(CrashedShipExploder))]
    class CrashedShipExploder_
    {
        [HarmonyPatch("Update"), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions)
         .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, CrashedShipExploder.delayBeforeSwap))
         .ThrowIfInvalid("Could not find Ldc_R4 CrashedShipExploder.delayBeforeSwap in CrashedShipExploder.Update")
         .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<float>>(GetDelayBeforeSwap))
         .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, CrashedShipExploder.delayBeforeExplosionFX))
         .ThrowIfInvalid("Could not find Ldc_R4 CrashedShipExploder.delayBeforeExplosionFX in CrashedShipExploder.Update")
         .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<float>>(GetDelayBeforeExplosionFX))
        .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, CrashedShipExploder.delayBeforeExplosionSound))
         .ThrowIfInvalid("Could not find Ldc_R4 CrashedShipExploder.delayBeforeExplosionSound in CrashedShipExploder.Update")
         .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<float>>(GetDelayBeforeExplosionSound))
         .InstructionEnumeration();
            return codeMatcher;
        }

        public static float GetDelayBeforeExplosionFX()
        {
            return CrashedShipExploder.delayBeforeExplosionFX * ConfigMenu.timeFlowSpeed.Value;
        }
        public static float GetDelayBeforeSwap()
        {
            return CrashedShipExploder.delayBeforeSwap * ConfigMenu.timeFlowSpeed.Value;
        }
        public static float GetDelayBeforeExplosionSound()
        {
            return CrashedShipExploder.delayBeforeExplosionSound * ConfigMenu.timeFlowSpeed.Value;
        }
    }

    //[HarmonyPatch(typeof(CrashHome))]
    class CrashHome_
    {
        //[HarmonyPatch("Update"), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions)
        .MatchForward(false, new CodeMatch(OpCodes.Ldc_R8, CrashHome.respawnDelay))
         .ThrowIfInvalid("Could not find Ldc_R8 CrashHome.respawnDelay in CrashHome.Update")
         .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<double>>(GetRespawnDelay))
         .InstructionEnumeration();
            return codeMatcher;
        }
        public static double GetRespawnDelay()
        {
            return CrashHome.respawnDelay / ConfigMenu.timeFlowSpeed.Value;
        }
    }


}
