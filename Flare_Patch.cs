using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
//using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Flare))]
    class Flare_Patch
    {
        //public static Dictionary<Pickupable, Light[]> playerToolLights = new Dictionary<Pickupable, Light[]>();
        //public static Dictionary<TechType, float> lightOrigIntensity = new Dictionary<TechType, float>();
        //public static Dictionary<TechType, float> lightIntensity = new Dictionary<TechType, float>();
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

        [HarmonyPatch(nameof(Flare.Awake))]
        [HarmonyPrefix]
        internal static bool AwakePrefix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return true;

            if (__instance.energyLeft <= 0f && !__instance.GetComponentInParent<Player>())
            { // destroy only when not in inventpry
                //AddDebug("Destroy flare ");
                UnityEngine.Object.Destroy(__instance.gameObject);
                return false;
            }
            if (originalIntensity == -1f && __instance.flareActivateTime == 0f)
            {
                originalIntensity = __instance.light.intensity;
                halfOrigIntensity = originalIntensity * .5f;
                originalEnergy = __instance.energyLeft;
                //originalEnergy = 10;
                lowEnergy = originalEnergy * .1f;
                originalRange = __instance.light.range;
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                Tools_Patch.lightOrigIntensity[TechType.Flare] = originalIntensity;
                Tools_Patch.lightIntensityStep[TechType.Flare] = originalIntensity * .05f;
                //Main.Log( "Flare lightOrigIntensity " + originalIntensity);
                //Main.Log("Flare lightIntensityStep " + originalIntensity * .05f);
                //AddDebug("Awake energyLeft " + __instance.energyLeft);
            }
            if (Main.config.lightIntensity.ContainsKey(TechType.Flare))
            {
                originalIntensity = Main.config.lightIntensity[TechType.Flare];
                halfOrigIntensity = originalIntensity * .5f;
            }
            PlayerToolAwake(__instance as PlayerTool);
            __instance.originalIntensity = __instance.light.intensity;
            __instance.originalrange = __instance.light.range;
            __instance.light.intensity = 0f;
            //this.light.range = 0.0f;
            //this.sequence = new Sequence();
            __instance.capRenderer.enabled = __instance.flareActivateTime == 0;

            if (__instance.flareActivateTime > 0f)
            {
                __instance.throwDuration = .1f;
                if (__instance.fxControl && !__instance.fxIsPlaying)
                {
                    __instance.fxControl.Play(1);
                    __instance.fxIsPlaying = true;
                    __instance.light.enabled = true;
                }
            }
            WorldForces wf = __instance.GetComponent<WorldForces>();
            wf.underwaterGravity = .5f;
            return false;
        }

        [HarmonyPatch(nameof(Flare.SetFlareActiveState))]
        [HarmonyPrefix]
        internal static bool SetFlareActiveStatePrefix(Flare __instance, bool newFlareActiveState)
        {
            //AddDebug("hasBeenThrown " + __instance.hasBeenThrown);
            //AddDebug("fxIsPlaying " + __instance.fxIsPlaying);
            if (Main.flareRepairLoaded)
                return true;

            if (__instance.flareActiveState == newFlareActiveState)
                return false;
            if (newFlareActiveState)
            {
                __instance.loopingSound.Play();
                if (__instance.fxControl)
                    __instance.fxControl.Play(0);
                __instance.capRenderer.enabled = false;
                __instance.light.enabled = true;
                __instance.isLightFadinfIn = true;
                __instance.hasBeenThrown = true;
                if (__instance.flareActivateTime == 0)
                    __instance.flareActivateTime = DayNightCycle.main.timePassedAsFloat;
            }
            __instance.flareActiveState = newFlareActiveState;
            return false;
        }
          
        [HarmonyPatch(nameof(Flare.OnDraw))]
        [HarmonyPrefix]
        internal static bool OnDrawPrefix(Flare __instance, Player p)
        {
            //AddDebug("originalEnergy " + originalEnergy);
            if (Main.flareRepairLoaded)
                return true;

            intensityChanged = false;
            PlayerToolOnDraw(p, __instance as PlayerTool);
            if (__instance.flareActivateTime == 0)
                return false;

            __instance.energyLeft = originalEnergy - (DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime);
            if (__instance.energyLeft < 0)
                __instance.energyLeft = 0;

            if (__instance.energyLeft > 0 && !__instance.fxIsPlaying)
            {
                __instance.SetFlareActiveState(true);
                __instance.fxControl.Play(1);
                __instance.fxIsPlaying = true;
            }
            else
            {
                __instance.flareActiveState = false;
                KillFlareLight(__instance);
            }
            return false;
        }

        [HarmonyPatch(nameof(Flare.OnHolster))]
        [HarmonyPostfix]
        internal static void OnHolsterPostfix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return;

            if (__instance.flareActivateTime > 0f)
                __instance.hasBeenThrown = true;
            //AddDebug("hasBeenThrown " + __instance.hasBeenThrown);
            //AddDebug("fxIsPlaying " + __instance.fxIsPlaying);
        }

        [HarmonyPatch(nameof(Flare.OnToolUseAnim))]
        [HarmonyPrefix]
        internal static bool OnToolUseAnimPrefix(Flare __instance, GUIHand hand)
        {
            if (Main.flareRepairLoaded)
                return true;

            //AddDebug("OnToolUseAnim " + __instance.isThrowing);
            if (__instance.isThrowing)
                return false;
            if (__instance.flareActivateTime == 0)
            {
                //AddDebug("SetFlareActiveState true");
                __instance.SetFlareActiveState(true);
            }
            //__instance.hasBeenThrown = true;
            //__instance.energyLeft = 0f;
            //__instance.isLightFadinfIn = false;
            //__instance.flareActiveState = true; // need this for Throw callback 
            //AddDebug("OnToolUseAnim flareActiveState " + __instance.flareActiveState);
            //KillFlareLight(__instance);
            //__instance.SetFlareActiveState(false);
            //Flare_OnDraw_Patch.Prefix(__instance, Player.main);
            //__instance.throwDuration = .1f;
            //__instance.sequence.Set(__instance.throwDuration, true, new SequenceCallback(__instance.Throw));
            __instance.Invoke("Throw", __instance.throwDuration);
            __instance.isThrowing = true;
            return false;
        }

        [HarmonyPatch(nameof(Flare.Throw))]
        [HarmonyPrefix]
        internal static bool ThrowPrefix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return true;

            //AddDebug("Throw " + __instance.throwDuration);
            //Inventory.main.quickSlots.SelectImmediate(1);
            __instance._isInUse = false;
            __instance.pickupable.Drop(__instance.transform.position);
            __instance.pickupable.isPickupable = false;
            //__instance.pickupable.enabled = false;
            __instance.transform.GetComponent<WorldForces>().enabled = true;
            __instance.throwSound.StartEvent();
            __instance.isThrowing = false;
            __instance.throwDuration = DayNightCycle.main.timePassedAsFloat;
            return false;
        }

        [HarmonyPatch(nameof(Flare.UpdateLight))]
        [HarmonyPrefix]
        internal static bool UpdateLightPrefix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return true;

            if (intensityChanged)
            {
                //AddDebug("intensityChaned ");
                __instance.light.intensity = Main.config.lightIntensity[TechType.Flare];
                return false;
            }
            else
            {
                float burnTime = DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime;
                if (burnTime < 0.1f)
                    return false;
                __instance.light.intensity = halfOrigIntensity + halfOrigIntensity * Mathf.PerlinNoise(burnTime * 6f, 0f);
                //AddDebug("energyLeft " + (int)__instance.energyLeft);
                if (__instance.energyLeft < lowEnergy)
                {
                    float f1 = __instance.energyLeft / lowEnergy;
                    //AddDebug("lowEnergy " + f.ToString("0.0"));
                    __instance.light.intensity = Mathf.Lerp(0f, __instance.light.intensity, f1);
                    __instance.light.range = Mathf.Lerp(0f, originalRange, f1);
                }
            }
            return false;
        }

        [HarmonyPatch(nameof(Flare.Update))]
        [HarmonyPrefix]
        internal static bool UpdatePrefix(Flare __instance)
        {
            if (Main.flareRepairLoaded)
                return true;

            //Main.Message("energyLeft " + __instance.energyLeft);
            if (__instance.throwDuration > 1 && DayNightCycle.main.timePassedAsFloat - __instance.throwDuration > 1f)
            {
                __instance.pickupable.isPickupable = true;
                __instance.throwDuration = .1f;
            }
            if (__instance.flareActiveState)
            {
                //__instance.energyLeft = Mathf.Max(__instance.energyLeft - Time.deltaTime, 0f);
                //AddDebug("update energyLeft");
                __instance.energyLeft = originalEnergy - (DayNightCycle.main.timePassedAsFloat - __instance.flareActivateTime);
                if (__instance.energyLeft < 0)
                    __instance.energyLeft = 0;

                if (__instance.energyLeft > 0)
                    __instance.UpdateLight();
            }
            else
                __instance.light.intensity = 0f;

            if (__instance.fxIsPlaying && __instance.energyLeft < 3f)
            {
                __instance.fxControl.StopAndDestroy(1, 2f);
                __instance.fxControl.Play(2);
                __instance.fxIsPlaying = false;
            }
            //if (__instance.energyLeft > 0)
            //    Main.Message("energyLeft " + (int)__instance.energyLeft);
            if (__instance.energyLeft > 0)
                return false;
            KillFlareLight(__instance);
            return false;
        }

        [HarmonyPatch(nameof(Flare.OnDrop))]
        [HarmonyPrefix]
        public static bool OnDropPrefix(Flare __instance)
            {
                if (Main.flareRepairLoaded)
                    return true;

                if (__instance.isThrowing)
                {
                    __instance.GetComponent<Rigidbody>().AddForce(MainCamera.camera.transform.forward * __instance.dropForceAmount * .5f);
                    __instance.GetComponent<Rigidbody>().AddTorque(MainCamera.camera.transform.forward * __instance.dropTorqueAmount * .5f);
                    __instance.isThrowing = false;
                }
                //AddDebug("energyLeft " + __instance.energyLeft);
                if (__instance.flareActivateTime > 0f && __instance.energyLeft > 0f)
                {
                    if (__instance.fxControl && !__instance.fxIsPlaying)
                    {
                        __instance.fxControl.Play(1);
                        __instance.fxIsPlaying = true;
                        __instance.loopingSound.Play();
                        __instance.light.enabled = true;
                        __instance.hasBeenThrown = true;
                        __instance.flareActiveState = true;
                    }
                }
                return false;
            }


    }
}