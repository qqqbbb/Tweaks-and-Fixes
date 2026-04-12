using FMOD;
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
using UnityEngine.InputSystem;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(DayNightCycle))]
    class DayNightCycle_
    {
        static bool skipTimeMode;

        public static void UpdateNightDuration()
        {
            DayNightCycle dnc = DayNightCycle.main;
            if (dnc == null)
                return;

            float nightScalar = Util.NormalizeTo01range(ConfigMenu.nightDuration.Value, 0, 24);
            //AddDebug("nightScalar " + nightScalar);
            dnc.sunRiseTime = nightScalar * .5f;
            dnc.sunSetTime = 1 - dnc.sunRiseTime;
            //dnc.UpdateAtmosphere();
            //dnc.UpdateDayNightMessage();
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
            //if (goal.key == "PrecursorGunAimCheck")
            //    goal.delay = 111f;

            goal.delay *= ConfigMenu.timeFlowSpeed.Value;
            //AddDebug("StoryGoalScheduler Schedule " + goal.key + " delay " + goal.delay);
        }
    }

    [HarmonyPatch(typeof(StoryGoalCustomEventHandler), "NotifyGoalComplete")]
    class StoryGoalCustomEventHandler_NotifyGoalComplete_patch
    {
        public static void Postfix(StoryGoalCustomEventHandler __instance, string key)
        {
            //AddDebug("StoryGoalCustomEventHandler NotifyGoalComplete " + key);
            //Main.logger.LogMessage("StoryGoalCustomEventHandler NotifyGoalComplete " + key);
            if (ConfigToEdit.sunbeamTimerShowsGameTime.Value && key == "OnPlayRadioSunbeam4")
            {
                //AddDebug("StoryGoalCustomEventHandler NotifyGoalComplete OnPlayRadioSunbeam4");
                StoryGoalCustomEventHandler.main.countdownStartingTime = DayNightCycle.main.timePassedAsFloat + 2400f * ConfigMenu.timeFlowSpeed.Value;
                //StoryGoalCustomEventHandler.main.countdownStartingTime = DayNightCycle.main.timePassedAsFloat + 141f * ConfigMenu.timeFlowSpeed.Value;
            }
        }
    }


    [HarmonyPatch(typeof(CrashedShipExploder))]
    class CrashedShipExploder_
    {
        //[HarmonyPrefix, HarmonyPatch("SetExplodeTime")]
        public static bool SetExplodeTimePrefix(CrashedShipExploder __instance)
        {
            AddDebug("CrashedShipExploder  SetExplodeTime ");
            Main.logger.LogMessage("CrashedShipExploder  SetExplodeTime timePassed " + DayNightCycle.main.timePassedAsFloat);
            __instance.timeToStartWarning = DayNightCycle.main.timePassedAsFloat;
            __instance.timeToStartCountdown = __instance.timeToStartWarning + 30;
            return false;
        }
        [HarmonyPatch("Update"), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            if (ConfigMenu.timeFlowSpeed.Value == 1)
                return new CodeMatcher(instructions).InstructionEnumeration();

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

    [HarmonyPatch(typeof(uGUI_SunbeamCountdown), "UpdateInterface")]
    public static class uGUI_SunbeamCountdown_UpdateInterface_Patch
    {
        public static bool Prefix(uGUI_SunbeamCountdown __instance)
        { // runs in main menu
            if (Main.gameLoaded == false)
                return false;

            if (ConfigToEdit.sunbeamTimerShowsGameTime.Value == false)
                return true;

            StoryGoalCustomEventHandler sgceh = StoryGoalCustomEventHandler.main;
            if (sgceh == null)
                return false;

            //AddDebug($"countdownActive {sgceh.countdownActive} countdownStartingTime {sgceh.countdownStartingTime}");
            if (sgceh.countdownActive == false)
            {
                __instance.HideInterface();
                if (sgceh.countdownStartingTime > 0)
                    __instance.CancelInvoke();

                return false;
            }
            DateTime eventDT = DayNightCycle.ToGameDateTime(sgceh.countdownStartingTime);
            DateTime now = DayNightCycle.ToGameDateTime(DayNightCycle.main.timePassedAsFloat);
            TimeSpan timeLeft = eventDT - now;
            //AddDebug($"Days {timeLeft.Days} Hours {timeLeft.Hours}");
            if (timeLeft.Days > 0 || now > eventDT)
            {
                __instance.HideInterface();
                return false;
            }
            string text = $"{timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
            __instance.countdownText.text = text;
            __instance.ShowInterface();
            return false;
        }
    }

    //[HarmonyPatch(typeof(StoryGoal), "Execute")]
    class StoryGoal_Execute_patch
    {
        public static void Prefix(StoryGoal __instance, string key, GoalType goalType)
        {
            AddDebug("StoryGoal Execute " + key + " Type " + goalType);
            //if (key == "PrecursorGunAimCheck")
            //    Main.logger.LogMessage("Execute PrecursorGunAimCheck " + DayNightCycle.main.timePassedAsFloat);
        }
    }

    //[HarmonyPatch(typeof(StoryGoalManager), "OnGoalComplete")]
    class StoryGoalManager_OnGoalComplete_patch
    {
        public static void Prefix(StoryGoalManager __instance, string key)
        {
            AddDebug("StoryGoalManager OnGoalComplete " + key);
        }
    }

    //[HarmonyPatch(typeof(VFXSunbeam), "Update")]
    class VFXSunbeam_Update_patch
    {
        public static bool Prefix(VFXSunbeam __instance)
        {
            if (Keyboard.current.spaceKey.IsPressed())
            {
                AddDebug("return");
                return false;
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(VFXSunbeam), "UpdateSequence")]
    class VFXSunbeam_UpdateSequence_patch
    {
        public static bool Prefix(VFXSunbeam __instance)
        {

            AddDebug($"VFXSunbeam isPlaying {__instance.isPlaying} animTime {__instance.animTime}");
            float num = __instance.shipApproachDuration + __instance.warmupDuration;
            float num2 = num + __instance.explosionDelay;
            if (__instance.shipTransform != null)
            {
                __instance.targetInitPos = __instance.shipTransform.position;
                if (__instance.shipAnimeTime < 1f)
                {
                    __instance.shipAnimeTime += Time.deltaTime / num;
                    if (__instance.shipAnimeTime >= 1f)
                    {
                        //__instance.StartCoroutine(DestroySunbeam(__instance.shipTransform.gameObject));
                        //__instance.Invoke(DestroySunbeam(__instance.shipTransform.gameObject ),5f);
                        UnityEngine.Object.Destroy(__instance.shipTransform.gameObject, 5f);
                    }
                }
            }
            if (!__instance.warmedUp && __instance.animTime >= num)
            {
                if (__instance.warmupTransform != null)
                {
                    __instance.warmupTransform.GetComponent<ParticleSystem>().Stop();
                }
                __instance.gunSpawnPoint.transform.localScale = Vector3.one;
                __instance.Invoke("CreateBeam", __instance.beamDelay);
                if (PrecursorGunStoryEvents.main != null)
                {
                    __instance.muzzleTransform = __instance.SpawnFXAndPlay(__instance.muzzlePrefab, __instance.gunSpawnPoint.transform);
                }
                __instance.warmedUp = true;
            }
            else if (!__instance.exploded && __instance.animTime >= num2)
            {
                if (__instance.beamMats != null)
                {
                    if (__instance.beamMats.Length > 1)
                    {
                        float t = Mathf.Clamp01((__instance.animTime - num2) * 3f);
                        __instance.beamMats[1].SetColor(ShaderPropertyID._Color, Color.Lerp(Color.clear, __instance.beamMatColor, t));
                    }
                    //else
                    //{
                    //    Debug.Log("VFXSunbeam.beamMats[1] is null");
                    //}
                }
                __instance.rectifiedTarget.transform.localScale = Vector3.one;
                __instance.explosionTransform = __instance.SpawnFXAndPlay(__instance.explosionPrefab, __instance.rectifiedTarget.transform);
                __instance.explosionTransform.eulerAngles = new Vector3(-90f, 0f, 0f);
                if (PrecursorGunStoryEvents.main != null)
                {
                    __instance.SpawnFXAndPlay(__instance.groundShockwavePrefab, PrecursorGunStoryEvents.main.transform);
                }
                for (int i = 0; i < __instance.chunksAmount; i++)
                {
                    __instance.Invoke("SpawnBurningChunks", __instance.chunksSpawnDelay * (float)i + 2f);
                }
                __instance.exploded = true;
            }
            if (__instance.exploded)
            {
                __instance.cloudsAnimTime += Time.deltaTime / __instance.cloudsColorDuration;
            }
            __instance.animTime += Time.deltaTime;
            return false;
        }
    }

}
