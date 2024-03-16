using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Flare))]
    class Flare_Patch
    {
        public static float originalIntensity = -1f;
        static float originalEnergy = -1f;
        public static float halfOrigIntensity;
        static float originalRange;
        static float lowEnergy;
        public static bool intensityChanged = false;
            
        public static void LightFlare(Flare flare)
        {
            //AddDebug("LightFlare ");
            //flare._isInUse = true;
            flare.loopingSound.Play();
            if (flare.fxControl)
            {
                flare.fxIsPlaying = true;
                flare.fxControl.Play(0);
                flare.fxControl.Play(1);
            }
            flare.capRenderer.enabled = false;
            flare.light.enabled = true;
            flare.isLightFadinfIn = true;
            //flare.isThrowing = true;
            flare.hasBeenThrown = true; 
            flare.flareActivateTime = DayNightCycle.main.timePassedAsFloat;
            flare.flareActiveState = true;
            flare.throwDuration = .1f;
        }

        public static void KillFlareLight(Flare flare)
        {
            //AddDebug("KillFlareLight ");
            if (flare.fxIsPlaying)
            {
                flare.fxControl.StopAndDestroy(1, 2f);
                flare.fxIsPlaying = false;
            }
            flare.light.enabled = false;
            flare.isLightFadinfIn = false;
            flare.hasBeenThrown = true;
            flare.loopingSound.Stop();
            flare.pickupable.isPickupable = false;
        }

        public static void PlayerToolAwake(PlayerTool playerTool)
        {
            playerTool.energyMixin = playerTool.GetComponent<EnergyMixin>();
            playerTool.savedRightHandIKTarget = playerTool.rightHandIKTarget;
            playerTool.savedLeftHandIKTarget = playerTool.leftHandIKTarget;
            playerTool.savedIkAimRightArm = playerTool.ikAimRightArm;
            playerTool.savedIkAimLeftArm = playerTool.ikAimLeftArm;
        }

        public static void PlayerToolOnDraw(Player p, PlayerTool playerTool)
        {
            playerTool.usingPlayer = p;
            playerTool.SetHandIKTargetsEnabled(true);
            LargeWorldEntity component = playerTool.GetComponent<LargeWorldEntity>();
            if (component != null && LargeWorldStreamer.main != null && LargeWorldStreamer.main.IsReady())
                LargeWorldStreamer.main.cellManager.UnregisterEntity(component);
            playerTool.isDrawn = true;
            playerTool.firstUseAnimationStarted = false;
            if (playerTool.hasFirstUseAnimation && playerTool.pickupable)
            {
                TechType techType = playerTool.pickupable.GetTechType();
                bool flag = Player.main.AddUsedTool(techType);
                if (GameOptions.GetVrAnimationMode())
                    flag = false;
                Player.main.playerAnimator.SetBool("using_tool_first", flag);
                playerTool.firstUseAnimationStarted = flag;
            }
            if (playerTool.firstUseAnimationStarted && playerTool.firstUseSound)
                playerTool.firstUseSound.Play();
            else
            {
                if (playerTool.drawSound)
                    Utils.PlayFMODAsset(playerTool.drawSound, playerTool.transform);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(Flare.UpdateLight))]
        static bool UpdateLightPrefix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return true;

            if (intensityChanged)
            {
                //AddDebug("intensityChaned ");
                __instance.light.intensity = Main.config.lightIntensity[TechType.Flare];
                __instance.originalIntensity = __instance.light.intensity;
                //intensityChanged = false;
                return false;
            }
            else
            {
                float burnTime = DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime;
                if (burnTime < 0.1f)
                    return false;

                float num2 = burnTime / __instance.flickerInterval;
                float num3 = __instance.originalIntensity * (0.45f + 0.55f * Mathf.PerlinNoise(num2, 0f));
                float num4 = (__instance.originalrange * 0.65f + 0.35f * Mathf.Sin(num2));
                if (burnTime < 0.43f)
                {
                    float t = (burnTime * 3.0f - 0.1f);
                    FlashingLightHelpers.SafeIntensityChangePerFrame(__instance.light, Mathf.Lerp(0f, num3, t));
                    FlashingLightHelpers.SafeRangeChangePreFrame(__instance.light, Mathf.Lerp(0f, num4, t));
                }
                else
                {
                    FlashingLightHelpers.SafeIntensityChangePerFrame(__instance.light, num3);
                    FlashingLightHelpers.SafeRangeChangePreFrame(__instance.light, num4);
                }
                //__instance.light.intensity = halfOrigIntensity + halfOrigIntensity * Mathf.PerlinNoise(Time.time * 6f, 0f);
                //AddDebug("energyLeft " + (int)__instance.energyLeft);
                //if (__instance.energyLeft < lowEnergy)
                {
                    //float f1 = __instance.energyLeft / lowEnergy;
                    //AddDebug("lowEnergy " + f.ToString("0.0"));
                    //__instance.light.intensity = Mathf.Lerp(0f, __instance.light.intensity, f1);
                    //__instance.light.range = Mathf.Lerp(0f, originalRange, f1);
                }
            }
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Flare.OnDraw))]
        static void OnDrawPostfix(Flare __instance, Player p)
        {
            //AddDebug("OnDraw originalRange " + originalRange);
            //AddDebug("OnDraw originalIntensity " + originalIntensity);
            if (!Main.flareRepairLoaded)
                intensityChanged = false;
        }
    }
}