﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Flare))]
    class Flare_
    {
        private static float defaultFlickerInterval;

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
            flare.hasBeenThrown = true;// removing cap animation will not play when throwing
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

        [HarmonyPrefix, HarmonyPatch("UpdateLight")]
        static bool UpdateLightPrefix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return true;

            if (Input.GetKey(ConfigMenu.lightButton.Value) || GameInput.GetButtonHeld(Light_Control.lightButton))
                return false;

            return true;
        }

        [HarmonyPatch("UpdateLight"), HarmonyPostfix]
        static void UpdateLightPostfix(Flare __instance)
        {
            //if (Input.GetKeyDown(KeyCode.LeftShift))
            //    AddDebug($"energy {__instance.energyLeft.ToString("0.0")} intensity {__instance.light.intensity.ToString("0.0")} range {__instance.light.range.ToString("0.0")}");

            if (Main.flareRepairLoaded || ConfigToEdit.flareFlicker.Value)
                return;

            __instance.light.intensity = __instance.originalIntensity;
            __instance.light.range = __instance.originalrange;
        }

        [HarmonyPostfix, HarmonyPatch("OnDraw")]
        static void OnDrawPostfix(Flare __instance, Player p)
        {
            //AddDebug("OnDraw originalRange " + originalRange);
            if (Main.flareRepairLoaded)
                return;

            if (defaultFlickerInterval == 0)
                defaultFlickerInterval = __instance.flickerInterval;

            __instance.flickerInterval = defaultFlickerInterval * DayNightCycle.main._dayNightSpeed;
            //AddDebug($"OnDraw origFlickerInterval {origFlickerInterval}  flickerInterval {__instance.flickerInterval} ");
        }

        [HarmonyPrefix, HarmonyPatch(nameof(Flare.OnRightHandDown))]
        static bool OnRightHandDownPostfix(Flare __instance)
        { // fix: can throw flare in base
            bool canThrow = ConfigToEdit.dropItemsAnywhere.Value || Inventory.CanDropItemHere(__instance.GetComponent<Pickupable>(), false);
            //AddDebug("OnRightHandDown CanDropItemHere " + canThrow);
            return canThrow;
        }

        [HarmonyPrefix, HarmonyPatch("OnToolUseAnim")]
        static bool OnToolUseAnimPostfix(Flare __instance)
        { // fix: can throw flare in base
            bool canThrow = ConfigToEdit.dropItemsAnywhere.Value || Inventory.CanDropItemHere(__instance.GetComponent<Pickupable>(), false);
            //AddDebug("OnToolUseAnim CanDropItemHere " + canThrow);
            return canThrow;
        }


    }
}