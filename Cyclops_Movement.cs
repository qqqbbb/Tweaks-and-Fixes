using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Cyclops_Movement
    {
        static float cyclopsVerticalMod = 1;
        static float cyclopsBackwardMod = 1;
        static float cyclopsForwardOrig;

        [HarmonyPatch(typeof(SubControl))]
        class SubControl_patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SubControl __instance)
            {
                //if (__instance.name != "Cyclops-MainPrefab(Clone)")
                //    return;
                if (ConfigToEdit.cyclopsBackwardSpeedMod.Value > 0 && cyclopsBackwardMod == 1)
                    cyclopsBackwardMod = 1 - Mathf.Clamp(ConfigToEdit.cyclopsBackwardSpeedMod.Value, 1, 100) * .01f;

                if (ConfigToEdit.cyclopsVerticalSpeedMod.Value > 0 && cyclopsVerticalMod == 1)
                    cyclopsVerticalMod = 1 - Mathf.Clamp(ConfigToEdit.cyclopsVerticalSpeedMod.Value, 1, 100) * .01f;

            }
            [HarmonyPrefix, HarmonyPatch("FixedUpdate")]
            public static void FixedUpdatePrefix(SubControl __instance)
            {
                if (Main.gameLoaded == false || !__instance.LOD.IsFull() || __instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline || __instance.throttle == default)
                    return;
                //AddDebug("throttle.magnitude " + __instance.throttle.magnitude);
                //AddDebug("BaseForwardAccel " + __instance.BaseForwardAccel);
                //AddDebug("cyclopsBackwardMod " + cyclopsBackwardMod);
                //AddDebug($"VerticalMod {__instance.BaseVerticalAccel}  my VerticalMod {cyclopsVerticalMod}");
                if (cyclopsForwardOrig > 0)
                {
                    float mod = 1;
                    if (ConfigMenu.cyclopsSpeedMult.Value != 1)
                        mod = ConfigMenu.cyclopsSpeedMult.Value;

                    if (cyclopsBackwardMod > 0 && cyclopsBackwardMod < 1 && __instance.throttle.z < 0)
                        mod *= cyclopsBackwardMod;

                    __instance.BaseForwardAccel = cyclopsForwardOrig * mod;
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsMotorMode))]
        class CyclopsMotorMode_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(CyclopsMotorMode __instance)
            {
                cyclopsForwardOrig = __instance.motorModeSpeeds[(int)__instance.cyclopsMotorMode];
                //Main.logger.LogDebug($"CyclopsMotorMode Start cyclopsForwardOrig {cyclopsForwardOrig}");
            }
            [HarmonyPostfix, HarmonyPatch("ChangeCyclopsMotorMode")]
            public static void ChangeCyclopsMotorModePostfix(CyclopsMotorMode __instance, CyclopsMotorMode.CyclopsMotorModes newMode)
            {
                float motorModeSpeed = __instance.motorModeSpeeds[(int)__instance.cyclopsMotorMode];
                //Main.logger.LogDebug($"CyclopsMotorMode ChangeCyclopsMotorMode {newMode} {motorModeSpeed}");
                cyclopsForwardOrig = motorModeSpeed;
                if (ConfigToEdit.cyclopsVerticalSpeedMod.Value > 0)
                {
                    __instance.subController.BaseVerticalAccel = motorModeSpeed * cyclopsVerticalMod;
                    //AddDebug("motorModeSpeed " + motorModeSpeed);
                }
            }

        }

        static void SetCyclopsMotorMode(CyclopsMotorModeButton instance, CyclopsMotorMode.CyclopsMotorModes motorMode)
        {
            if (motorMode == instance.motorModeIndex)
            {
                instance.SendMessageUpwards("ChangeCyclopsMotorMode", instance.motorModeIndex, SendMessageOptions.RequireReceiver);
                instance.image.sprite = instance.activeSprite;
            }
            else
                instance.image.sprite = instance.inactiveSprite;
        }

        [HarmonyPatch(typeof(CyclopsMotorModeButton), "Start")]
        class CyclopsMotorModeButton_Start_Patch
        {
            public static void Postfix(CyclopsMotorModeButton __instance)
            {
                GameObject root = __instance.transform.parent.parent.parent.parent.parent.gameObject;
                if (root.name == "__LIGHTMAPPED_PREFAB__")
                    return;
                //Main.logger.LogMessage("CyclopsMotorModeButton Start " + __instance.transform.parent.parent.parent.parent.parent.name);
                int throttleIndex = Main.configMain.GetSubThrottleIndex(root);
                if (throttleIndex != -1)
                {
                    //AddDebug("restore  subThrottleIndex");
                    SetCyclopsMotorMode(__instance, (CyclopsMotorMode.CyclopsMotorModes)throttleIndex);
                }
            }
        }

        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnProtoSerialize")]
            public static void OnProtoSerializePostfix(SubRoot __instance)
            {
                CyclopsMotorMode cyclopsMotorMode = __instance.GetComponent<CyclopsMotorMode>();
                if (cyclopsMotorMode)
                {
                    Main.configMain.SaveSubThrottleIndex(__instance.gameObject, (int)cyclopsMotorMode.cyclopsMotorMode);
                }
            }
        }


    }
}
