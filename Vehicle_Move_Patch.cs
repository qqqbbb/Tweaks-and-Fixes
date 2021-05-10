using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Vehicle))]
    [HarmonyPatch("OnHandHover")]
    class Vehicle_OnHandHover_patch
    {
        public static void Postfix(Vehicle __instance)
        {
            //AddDebug("onGround " + __instance.onGround);
            //EcoTarget ecoTarget = __instance.GetComponent<EcoTarget>();
            if (__instance.onGround && !Inventory.main.GetHeld() && __instance is SeaMoth && !__instance.docked && !Player.main.IsSwimming())
            {
                if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    Vector3 direction = new Vector3(MainCameraControl.main.transform.forward.x, .2f, MainCameraControl.main.transform.forward.z);
    
                    rb.AddForce(direction * 3333f, ForceMode.Impulse);
                }
            }
        }
    }

    //[HarmonyPatch(typeof(Vehicle))]
    //[HarmonyPatch("ApplyPhysicsMove")]
    class Exosuit_Update_Prefix_patch
    {
        //static float maxSpeed = 0;
        public static void Postfix(Vehicle __instance)
        {
            float movementSpeed = __instance.useRigidbody.velocity.magnitude / 5f;
            movementSpeed = (float)Math.Round(movementSpeed * 10f) / 10f;
            //Main.Message("movementSpeed  " + movementSpeed);
            //if (movementSpeed > maxSpeed)
            //{
            //    maxSpeed = movementSpeed;
            //    Main.Log("maxSpeed " + maxSpeed);
            //}
        }
    }

    [HarmonyPatch(typeof(SeaMoth), "Start")]
    class SeaMoth_Start_patch
    {
        public static void Postfix(SeaMoth __instance)
        {
            if (Main.config.seamothMoveTweaks) 
            { 
                __instance.sidewardForce = __instance.forwardForce * .5f;
                __instance.verticalForce = __instance.forwardForce * .5f;
                __instance.backwardForce = 0f;
            }
        }
    }

    [HarmonyPatch(typeof(SeaMoth), "UpdateSounds")]
    class SeaMoth_UpdateSounds_patch
    { // dont play sound when move backward is pressed
        public static bool Prefix(SeaMoth __instance)
        {
            if (!Main.config.seamothMoveTweaks)
                return true;

            Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
            input = new Vector3(input.x, input.y, input.z > 0f ? input.z : 0f);
            if (__instance.CanPilot() && input.magnitude > 0f && __instance.GetPilotingMode())
            {
                __instance.engineSound.AccelerateInput();
                if (__instance.fmodIndexSpeed < 0)
                    __instance.fmodIndexSpeed = __instance.ambienceSound.GetParameterIndex("speed");
                if (__instance.ambienceSound && __instance.ambienceSound.GetIsPlaying())
                    __instance.ambienceSound.SetParameterValue(__instance.fmodIndexSpeed, __instance.useRigidbody.velocity.magnitude);
            }
            bool flag = false;
            int index = 0;
            for (int length = __instance.quickSlotCharge.Length; index < length; ++index)
            {
                if (__instance.quickSlotCharge[index] > 0f)
                {
                    flag = true;
                    break;
                }
            }
            if (__instance.pulseChargeSound.GetIsStartingOrPlaying() == flag)
                return false; 
            if (flag)
                __instance.pulseChargeSound.StartEvent();
            else
                __instance.pulseChargeSound.Stop();

            return false;
        }
    }

    [HarmonyPatch(typeof(Vehicle), "ApplyPhysicsMove")]
    class Vehicle_ApplyPhysicsMove_Prefix_patch
    {      // fix seamoth move diagonally
        static void ApplyPhysicsMoveSeamoth(Vehicle __instance)
        {
            //AddDebug("ApplyPhysicsMoveSeamoth  " + __instance.controlSheme);
            if (__instance.worldForces.IsAboveWater() != __instance.wasAboveWater)
            {
                __instance.PlaySplashSound();
                __instance.wasAboveWater = __instance.worldForces.IsAboveWater();
            }
            if (!(__instance.moveOnLand | (__instance.transform.position.y < Ocean.main.GetOceanLevel() && __instance.transform.position.y < __instance.worldForces.waterDepth && !__instance.precursorOutOfWater)))
                return;

            Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
            input.Normalize();
            float z = input.z > 0 ? input.z * __instance.forwardForce : input.z * __instance.backwardForce;
            Vector3 acceleration = new Vector3(input.x * __instance.sidewardForce, input.y * __instance.verticalForce, z);
            acceleration = __instance.transform.rotation * acceleration * Time.deltaTime;
            for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                __instance.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
            __instance.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
        }
        //disable exosuit strafe
        static void ApplyPhysicsMoveExosuit(Vehicle __instance)
        {
            //AddDebug("ApplyPhysicsMoveExosuit  " + __instance.controlSheme);
            if (__instance.worldForces.IsAboveWater() != __instance.wasAboveWater)
            {
                __instance.PlaySplashSound();
                __instance.wasAboveWater = __instance.worldForces.IsAboveWater();
            }
            if (!(__instance.moveOnLand | (__instance.transform.position.y < Ocean.main.GetOceanLevel() && __instance.transform.position.y < __instance.worldForces.waterDepth && !__instance.precursorOutOfWater)))
                return;

            Vector3 inputRaw = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
            Vector3 input = new Vector3(0.0f, 0.0f, inputRaw.z);
            float num = Mathf.Abs(input.x) * __instance.sidewardForce + Mathf.Max(0.0f, input.z) * __instance.forwardForce + Mathf.Max(0.0f, -input.z) * __instance.backwardForce;
            Vector3 vector3_3 = __instance.transform.rotation * input;
            vector3_3.y = 0.0f;
            Vector3 vector = Vector3.Normalize(vector3_3);
            if (__instance.onGround)
            {
                vector = Vector3.ProjectOnPlane(vector, __instance.surfaceNormal);
                vector.y = Mathf.Clamp(vector.y, -0.5f, 0.5f);
                num *= __instance.onGroundForceMultiplier;
            }
            Vector3 vector3_4 = new Vector3(0.0f, inputRaw.y, 0.0f);
            vector3_4.y *= __instance.verticalForce * Time.deltaTime;
            Vector3 acceleration = num * vector * Time.deltaTime + vector3_4;
            __instance.OverrideAcceleration(ref acceleration);
            for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                __instance.accelerationModifiers[index].ModifyAcceleration(ref acceleration);
            __instance.useRigidbody.AddForce(acceleration, ForceMode.VelocityChange);
        }

        public static bool Prefix(Vehicle __instance)
        {
            //AddDebug("ControlSheme  " + __instance.controlSheme);
            if (!__instance.GetPilotingMode())
                return false;

            if (Main.config.seamothMoveTweaks && __instance.controlSheme == Vehicle.ControlSheme.Submersible)
            {
                ApplyPhysicsMoveSeamoth(__instance);
                return false;
            }
            else if (Main.config.exosuitMoveTweaks && __instance.controlSheme == Vehicle.ControlSheme.Mech)
            {
                ApplyPhysicsMoveExosuit(__instance);
                return false;
            }
            return true;
        }
    }

    // seamoth does not consume more energy when moving diagonally
    [HarmonyPatch(typeof(SeaMoth))]
    [HarmonyPatch("Update")]
    class SeaMoth_Update_patch
    {
        static void VehicleUpdate(Vehicle vehicle)
        {
            if (vehicle.GetPilotingMode() && vehicle.CanPilot() && (vehicle.moveOnLand || vehicle.transform.position.y < Ocean.main.GetOceanLevel()))
            {
                Vector2 vector2 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetLookDelta() : Vector2.zero;
                vehicle.steeringWheelYaw = Mathf.Clamp(vehicle.steeringWheelYaw + vector2.x * vehicle.steeringReponsiveness, -1f, 1f);
                vehicle.steeringWheelPitch = Mathf.Clamp(vehicle.steeringWheelPitch + vector2.y * vehicle.steeringReponsiveness, -1f, 1f);
                if (vehicle.controlSheme == Vehicle.ControlSheme.Submersible)
                {
                    float num = 3f;
                    vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
                    vehicle.useRigidbody.AddTorque(vehicle.transform.right * -vector2.y * vehicle.sidewaysTorque * 0.0015f * num, ForceMode.VelocityChange);
                    vehicle.useRigidbody.AddTorque(vehicle.transform.forward * -vector2.x * vehicle.sidewaysTorque * 0.0002f * num, ForceMode.VelocityChange);
                }
            }
            bool powered = vehicle.IsPowered();
            if (vehicle.wasPowered != powered)
            {
                vehicle.wasPowered = powered;
                vehicle.OnPoweredChanged(powered);
            }
            vehicle.ReplenishOxygen();
        }

        public static bool Prefix(SeaMoth __instance)
        {
            if (!Main.config.seamothMoveTweaks)
                return true;

            VehicleUpdate(__instance as Vehicle);

            __instance.UpdateSounds();
            if (__instance.GetPilotingMode())
            {
                string buttonFormat = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
                HandReticle.main.SetUseTextRaw(buttonFormat, string.Empty);
                Vector3 vector3 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                float magnitude = Mathf.Clamp(vector3.magnitude, 0f, 1f) ;
        
                if (magnitude > 0.1f)
                    __instance.ConsumeEngineEnergy(Time.deltaTime * __instance.enginePowerConsumption * magnitude);

                __instance.toggleLights.CheckLightToggle();
            }
            __instance.UpdateScreenFX();
            __instance.UpdateDockedAnim();
            return false;
        }

    }
}
