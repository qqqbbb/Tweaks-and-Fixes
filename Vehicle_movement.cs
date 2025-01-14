using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Vehicle_movement
    {
        static float cyclopsVerticalMod;
        static float cyclopsBackwardMod;
        static float cyclopsForwardOrig;

        [HarmonyPatch(typeof(GameInput), "GetMoveDirection")]
        class GameInput_GetMoveDirection_Patch
        {
            static void Postfix(GameInput __instance, ref Vector3 __result)
            {
                //AddDebug("GetMoveDirection " + __result);
                if (ConfigToEdit.fixSeamothMove.Value && Player.main.currentMountedVehicle is SeaMoth)
                { // fix power consumption
                    __result = __result.normalized;
                    //AddDebug("fix seamoth Move Direction " + __result);
                }
                if (ConfigToEdit.disableExosuitSidestep.Value && Player.main.currentMountedVehicle is Exosuit)
                {
                    __result = new Vector3(0, __result.y, __result.z);
                }
                if (ConfigToEdit.fixCyclopsMove.Value && Player.main.mode == Player.Mode.Piloting && Player.main.currentSub && Player.main.currentSub.isCyclops)
                {
                    __result = __result.normalized;
                }
            }

        }

        static void ApplyPhysicsMoveSeamoth(Vehicle __instance)
        {
            //AddDebug("ApplyPhysicsMoveSeamoth  " + __instance.controlSheme);
            if (__instance.worldForces.IsAboveWater() != __instance.wasAboveWater)
            {
                __instance.PlaySplashSound();
                __instance.wasAboveWater = __instance.worldForces.IsAboveWater();
            }
            if (!(__instance.moveOnLand | (__instance.transform.position.y < Ocean.GetOceanLevel() && __instance.transform.position.y < __instance.worldForces.waterDepth && !__instance.precursorOutOfWater)))
                return;

            Vector3 input;
            if (__instance.IsAutopilotEnabled)
                input = __instance.CalculateAutopilotLocalWishDir();
            else
                input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;

            if (input == Vector3.zero)
                return;

            //AddDebug("ApplyPhysicsMoveSeamoth input " + input);
            input.Normalize();
            float z = input.z > 0 ? input.z * __instance.forwardForce : input.z * __instance.backwardForce;
            Vector3 acceleration = new Vector3(input.x * __instance.sidewardForce, input.y * __instance.verticalForce, z);
            //Vector3 acceleration = __instance.transform.rotation * (((Mathf.Abs(input.x) * __instance.sidewardForce + z) + Mathf.Abs(input.y * __instance.verticalForce)) * input) * Time.deltaTime;
            acceleration = __instance.transform.rotation * acceleration * Time.deltaTime;
            for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                __instance.accelerationModifiers[index].ModifyAcceleration(ref acceleration);

            __instance.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
        }

        [HarmonyPatch(typeof(Vehicle))]
        public class Vehicle_patch
        {
            [HarmonyPrefix, HarmonyPatch("ApplyPhysicsMove")]
            public static bool ApplyPhysicsMovePrefix(Vehicle __instance)
            {
                if (!__instance.GetPilotingMode())
                    return false;

                if (ConfigToEdit.fixSeamothMove.Value && __instance is SeaMoth)
                {
                    ApplyPhysicsMoveSeamoth(__instance);
                    return false;
                }
                return true;
            }
            //[HarmonyPrefix, HarmonyPatch("ConsumeEngineEnergy")]
            public static void ConsumeEngineEnergyPrefix(Vehicle __instance, float energyCost)
            {
                //AddDebug("ConsumeEngineEnergy " + energyCost);
            }
        }


        [HarmonyPatch(typeof(SeaMoth))]
        class SeaMoth_patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SeaMoth __instance)
            {
                //AddDebug("SeaMoth Start seamothSidewardSpeedMod " + ConfigToEdit.seamothSidewardSpeedMod.Value);
                if (ConfigToEdit.seamothSidewardSpeedMod.Value > 0)
                {
                    float mod = 1 - Mathf.Clamp(ConfigToEdit.seamothSidewardSpeedMod.Value, 1, 100) * .01f;
                    __instance.sidewardForce = __instance.sidewardForce * mod;
                }
                if (ConfigToEdit.seamothBackwardSpeedMod.Value > 0)
                {
                    float mod = 1 - Mathf.Clamp(ConfigToEdit.seamothBackwardSpeedMod.Value, 1, 100) * .01f;
                    __instance.backwardForce = __instance.backwardForce * mod;
                }
                if (ConfigToEdit.seamothVerticalSpeedMod.Value > 0)
                {
                    float mod = 1 - Mathf.Clamp(ConfigToEdit.seamothVerticalSpeedMod.Value, 1, 100) * .01f;
                    __instance.verticalForce = __instance.verticalForce * mod;
                }
            }
        }

        [HarmonyPatch(typeof(Exosuit), "Update")]
        class Exosuit_Patch
        {
            public static void Postfix(Exosuit __instance)
            {
                if (!Main.gameLoaded || !ConfigToEdit.exosuitThrusterWithoutLimit.Value)
                    return;

                Vector3 input;
                if (__instance.IsAutopilotEnabled)
                    input = __instance.CalculateAutopilotLocalWishDir();
                else
                    input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;

                if (input == Vector3.zero)
                    return;

                bool thrusterOn = input.y > 0f;
                bool hasPower = __instance.IsPowered() && __instance.liveMixin.IsAlive();
                __instance.GetEnergyValues(out float charge, out float capacity);
                __instance.thrustPower = Util.NormalizeTo01range(charge, 0f, capacity);
                if (thrusterOn && hasPower && GameModeUtils.RequiresPower())
                {
                    float energyCost = __instance.thrustConsumption * Time.deltaTime;
                    //AddDebug("energyCost " + energyCost);
                    __instance.ConsumeEngineEnergy(energyCost);
                }
            }
        }

        [HarmonyPatch(typeof(SubControl))]
        class SubControl_patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SubControl __instance)
            {
                //if (__instance.name != "Cyclops-MainPrefab(Clone)")
                //    return;

                if (ConfigToEdit.cyclopsBackwardSpeedMod.Value > 0 && cyclopsBackwardMod == 0)
                {
                    cyclopsBackwardMod = 1 - Mathf.Clamp(ConfigToEdit.cyclopsBackwardSpeedMod.Value, 1, 100) * .01f;
                }
                if (ConfigToEdit.cyclopsVerticalSpeedMod.Value > 0 && cyclopsVerticalMod == 0)
                {
                    cyclopsVerticalMod = 1 - Mathf.Clamp(ConfigToEdit.cyclopsVerticalSpeedMod.Value, 1, 100) * .01f;
                }
            }
            [HarmonyPrefix, HarmonyPatch("FixedUpdate")]
            public static void FixedUpdatePrefix(SubControl __instance)
            {
                if (cyclopsForwardOrig > 0)
                {
                    if (__instance.throttle.z < 0)
                    {
                        //AddDebug("SubControl move back ");
                        __instance.BaseForwardAccel = cyclopsForwardOrig * cyclopsBackwardMod;
                    }
                    else
                        __instance.BaseForwardAccel = cyclopsForwardOrig;
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsMotorMode), "ChangeCyclopsMotorMode")]
        class CyclopsMotorMode_ChangeCyclopsMotorMode_Patch
        {
            public static void Postfix(CyclopsMotorMode __instance, CyclopsMotorMode.CyclopsMotorModes newMode)
            {
                //AddDebug("ChangeCyclopsMotorMode " + newMode);
                if (ConfigToEdit.cyclopsVerticalSpeedMod.Value > 0)
                {
                    float motorModeSpeed = __instance.motorModeSpeeds[(int)__instance.cyclopsMotorMode];
                    __instance.subController.BaseVerticalAccel = motorModeSpeed * cyclopsVerticalMod;
                }
                if (ConfigToEdit.cyclopsBackwardSpeedMod.Value > 0)
                {
                    float motorModeSpeed = __instance.motorModeSpeeds[(int)__instance.cyclopsMotorMode];
                    cyclopsForwardOrig = motorModeSpeed;
                    //__instance.subController.BaseForwardAccel = cyclopsForwardOrig * cyclopsVerticalMod;
                }


            }
        }





    }
}
