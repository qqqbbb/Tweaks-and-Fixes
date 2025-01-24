using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Vehicle_movement
    {
        static float cyclopsVerticalMod;
        static float cyclopsBackwardMod;
        static float cyclopsForwardOrig;
        public static Vector3 moveDir;
        static float seamothForwardForce;
        static float seamothBackwardForce;
        static float seamothVerticalForce;
        static float seamothSidewardForce;


        [HarmonyPatch(typeof(GameInput), "GetMoveDirection")]
        class GameInput_GetMoveDirection_Patch
        {
            static void Postfix(GameInput __instance, ref Vector3 __result)
            {
                if (!Main.gameLoaded || __result == Vector3.zero || moveDir == __result)
                    return;

                //AddDebug("GetMoveDirection " + __result);
                if (Player.main.currentMountedVehicle is Exosuit)
                {
                    __result *= ConfigMenu.exosuitSpeedMult.Value;
                    if (ConfigToEdit.disableExosuitSidestep.Value)
                        __result.x = 0;
                }
                else if (Player.main.currentMountedVehicle is SeaMoth)
                {
                    __result *= ConfigMenu.exosuitSpeedMult.Value;
                }
                //else if (Player.main.currentMountedVehicle != null)
                //{
                //AddDebug("mod vehicle");
                //Player.main.currentMountedVehicle.forwardForce *= 2;
                //__result *= ConfigMenu.exosuitSpeedMult.Value;
                //}
                else if (ConfigToEdit.fixCyclopsMove.Value && Player.main.mode == Player.Mode.Piloting && Player.main.currentSub && Player.main.currentSub.isCyclops)
                {
                    __result = __result.normalized;
                }
                moveDir = __result;
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
            Vector3 inputRaw = input;
            input = input.normalized;
            float z = input.z > 0 ? input.z * __instance.forwardForce : input.z * __instance.backwardForce;
            Vector3 acceleration = new Vector3(input.x * __instance.sidewardForce, input.y * __instance.verticalForce, z);
            acceleration = __instance.transform.rotation * acceleration * Time.deltaTime;
            float max = Mathf.Max(Mathf.Abs(inputRaw.x), Mathf.Abs(inputRaw.y), Mathf.Abs(inputRaw.z));
            acceleration *= max; // fix analog values from controller stick
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

                if (__instance is SeaMoth)
                {
                    seamothForwardForce = __instance.forwardForce;
                    seamothBackwardForce = __instance.backwardForce;
                    seamothVerticalForce = __instance.verticalForce;
                    seamothSidewardForce = __instance.sidewardForce;
                    __instance.forwardForce *= ConfigMenu.seamothSpeedMult.Value;
                    __instance.backwardForce *= ConfigMenu.seamothSpeedMult.Value;
                    __instance.verticalForce *= ConfigMenu.seamothSpeedMult.Value;
                    __instance.sidewardForce *= ConfigMenu.seamothSpeedMult.Value;
                    if (ConfigToEdit.fixSeamothMove.Value)
                    {
                        ApplyPhysicsMoveSeamoth(__instance);
                        return false;
                    }
                }
                return true;
            }
            [HarmonyPostfix, HarmonyPatch("ApplyPhysicsMove")]
            public static void ApplyPhysicsMovePostfix(Vehicle __instance)
            {
                //AddDebug("ApplyPhysicsMove " + __instance.name);
                if (__instance is SeaMoth)
                {
                    __instance.forwardForce = seamothForwardForce;
                    __instance.backwardForce = seamothSidewardForce;
                    __instance.verticalForce = seamothVerticalForce;
                    __instance.sidewardForce = seamothSidewardForce;
                }
            }
            [HarmonyPrefix, HarmonyPatch("ConsumeEngineEnergy")]
            public static void ConsumeEngineEnergyPrefix(Vehicle __instance, float energyCost)
            {
                //AddDebug("ConsumeEngineEnergy " + energyCost);
                if (ConfigToEdit.fixSeamothMove.Value && __instance is SeaMoth)
                {
                    SeaMoth seaMoth = __instance as SeaMoth;
                    Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                    float f = Mathf.Min(input.magnitude, 1);
                    energyCost = Time.deltaTime * seaMoth.enginePowerConsumption * f;
                }
            }
        }


        [HarmonyPatch(typeof(SeaMoth))]
        class SeaMoth_patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SeaMoth __instance)
            {
                //AddDebug("SeaMoth Start seamothSidewardSpeedMod " + ConfigToEdit.seamothSidewardSpeedMod.Value);
                if (ConfigToEdit.fixSeamothMove.Value)
                {
                    WorldForces worldForces = __instance.GetComponent<WorldForces>();
                    if (worldForces)
                        worldForces.aboveWaterDrag = 2;
                }
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

        //[HarmonyPatch(typeof(Vehicle), "ConsumeEngineEnergy")]
        class Vehicle_ConsumeEngineEnergy_Patch
        {
            //[HarmonyPrefix, HarmonyPatch("ConsumeEngineEnergy")]
            public static void Prefix(Vehicle __instance, ref float energyCost)
            {

            }
        }

        [HarmonyPatch(typeof(Exosuit))]
        class Exosuit_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(Exosuit __instance)
            {
                if (!Main.gameLoaded || !ConfigToEdit.exosuitThrusterWithoutLimit.Value || !__instance.GetPilotingMode())
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
                //AddDebug("throttle.magnitude " + __instance.throttle.magnitude);
                __instance.BaseForwardAccel = cyclopsForwardOrig * ConfigMenu.cyclopsSpeedMult.Value;
                if (ConfigToEdit.cyclopsBackwardSpeedMod.Value > 0)
                {
                    if (__instance.throttle.z < 0)
                        __instance.BaseForwardAccel *= cyclopsBackwardMod;
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
                    //AddDebug("motorModeSpeed " + motorModeSpeed);
                }
                if (ConfigToEdit.cyclopsBackwardSpeedMod.Value > 0)
                {
                    float motorModeSpeed = __instance.motorModeSpeeds[(int)__instance.cyclopsMotorMode];
                    cyclopsForwardOrig = motorModeSpeed;
                }


            }
        }





    }
}
