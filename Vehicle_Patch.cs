using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
           
    [HarmonyPatch(typeof(Vehicle))]
    class Vehicle_patch
    {
        public static Light[] currentLights = new Light[2];
        public static TechType currentVehicleTT;

        public static void UpdateLights()
        {
            //AddDebug("UpdateLights " + currentLights.Length);

            if (currentLights.Length == 0 || !currentLights[0].gameObject.activeInHierarchy)
                return;
            if (!Input.GetKey(Main.config.lightKey))
                return;

            //Light[] lights = __instance.GetComponentsInChildren<Light>();
            //AddDebug("lights.Length  " + currentLights[0].gameObject.activeInHierarchy);
            if (!Tools_Patch.lightIntensityStep.ContainsKey(currentVehicleTT))
            {
                AddDebug("lightIntensityStep missing " + currentVehicleTT);
                return;
            }
            if (!Tools_Patch.lightOrigIntensity.ContainsKey(currentVehicleTT))
            {
                AddDebug("lightOrigIntensity missing " + currentVehicleTT);
                return;
            }
            float step = 0f;
            //AddDebug("UpdateLights currentVehicleTT " + currentVehicleTT);
            if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
                step = Tools_Patch.lightIntensityStep[currentVehicleTT];
            else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                step = -Tools_Patch.lightIntensityStep[currentVehicleTT];

            if (step == 0f)
                return;

            foreach (Light l in currentLights)
            {
                if (step > 0 && l.intensity > Tools_Patch.lightOrigIntensity[currentVehicleTT])
                    return;

                l.intensity += step;
                //AddDebug("Light Intensity " + l.intensity);
                Main.config.lightIntensity[currentVehicleTT] = l.intensity;
            }
        }

        public static void VehicleUpdate(Vehicle vehicle)
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

        [HarmonyPatch(nameof(Vehicle.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(Vehicle __instance)
        {
            //Light l1 = __instance.transform.Find("lights_parent/light_left").gameObject.GetComponent<Light>();
            //Light l2 = __instance.transform.Find("lights_parent/light_right").gameObject.GetComponent<Light>();
            TechType tt = CraftData.GetTechType(__instance.gameObject);
            //if (l1)
            //AddDebug( " Vehicle.Awake " + tt);
            //if (l2)
            //    AddDebug(__instance.gameObject.name + " Awake light 2");
            Light[] lights = __instance.transform.Find("lights_parent").GetComponentsInChildren<Light>(true);
            //AddDebug(tt + " Awake lights " + lights.Length);
            Tools_Patch.lightOrigIntensity[tt] = lights[0].intensity;
            Tools_Patch.lightIntensityStep[tt] = lights[0].intensity * .1f;
            if (Main.config.lightIntensity.ContainsKey(tt))
            {
                foreach (Light l in lights)
                    l.intensity = Main.config.lightIntensity[tt];
            }
        }

        [HarmonyPatch(nameof(Vehicle.EnterVehicle))]
        [HarmonyPrefix]
        public static void EnterVehiclePrefix(Vehicle __instance)
        {
            currentVehicleTT = CraftData.GetTechType(__instance.gameObject);
            currentLights = __instance.transform.Find("lights_parent").GetComponentsInChildren<Light>(true);
            //AddDebug("EnterVehicle " + currentLights.Length);
        }

        //[HarmonyPatch(nameof(Vehicle.OnDockedChanged))]
        //[HarmonyPrefix]
        public static void OnDockedChangedPrefix(Vehicle __instance, bool docked, Vehicle.DockType dockType)
        {
            //currentVehicleTT = CraftData.GetTechType(__instance.gameObject);
            //currentLights = __instance.transform.Find("lights_parent").GetComponentsInChildren<Light>(true);
            AddDebug("OnDockedChanged " + docked + " " + dockType);
        }

        [HarmonyPatch(nameof(Vehicle.OnHandHover))]
        [HarmonyPostfix]
        public static void OnHandHoverPostfix(Vehicle __instance)
        {
            //AddDebug("handLabel " + __instance.handLabel);
            //EcoTarget ecoTarget = __instance.GetComponent<EcoTarget>();
            if (__instance.onGround && !Inventory.main.GetHeld() && __instance is SeaMoth && !__instance.docked && !Player.main.IsSwimming())
            {
                //string handLabel = Language.main.Get(__instance.handLabel);
                //HandReticle.main.SetInteractText("Push");
                if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    Vector3 direction = new Vector3(MainCameraControl.main.transform.forward.x, .2f, MainCameraControl.main.transform.forward.z);
    
                    rb.AddForce(direction * 3333f, ForceMode.Impulse);
                }
            }
        }
        // fix seamoth move diagonally
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
        //static float nextRaycastTime = 0;
        //static Vector3 groundPos = Vector3.zero;
        static void ApplyPhysicsMoveExosuit(Vehicle __instance)
        {
            //AddDebug("ApplyPhysicsMoveExosuit  " + __instance.controlSheme);
            //if (!__instance.onGround && Time.time > nextRaycastTime)
            //{
            //    Vector3 rayOrigin = new Vector3(groundPos.x, groundPos.y + 1f, groundPos.z);
            //    Vector3 rayTarget = new Vector3(__instance.transform.position.x, __instance.transform.position.y + 5f, __instance.transform.position.z);
            //    float rayDist = 100f;
            //    nextRaycastTime = Time.time + 1f;
            //    bool underg = Physics.Linecast(rayOrigin, __instance.transform.position, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore);
            //    Vector3 dir = __instance.transform.position - groundPos;
            //    underg = Physics.Raycast(new Ray(groundPos, Vector3.down), out RaycastHit hitInfo, rayDist, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore);
            //    if (underg)
            //    {
            //Main.Message("Raycast distance " + hitInfo.distance);
            //AddDebug("Prawn suit is under the ground! Resetting its position." + groundPos.y + " " + __instance.transform.position.y);
            //Main.Log("Prawn suit is under the ground! Resetting its position." + groundPos.y + " " + __instance.transform.position.y);
            //if (groundPos == Vector3.zero)
            //    groundPos = new Vector3(hitInfo.point.x, hitInfo.point.y + 11f, hitInfo.point.z);
            //__instance.transform.position = groundPos;
            //    }
            //}
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
                //groundPos = __instance.transform.position;
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

        [HarmonyPatch(nameof(Vehicle.ApplyPhysicsMove))]
        [HarmonyPrefix]
        public static bool ApplyPhysicsMovePrefix(Vehicle __instance)
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

    [HarmonyPatch(typeof(SeaMoth))]
    class SeaMoth_patch
    {
        [HarmonyPatch(nameof(SeaMoth.Start))]
        [HarmonyPostfix]
        public static void StartPostfix(SeaMoth __instance)
        {
            if (Main.config.seamothMoveTweaks) 
            { 
                __instance.sidewardForce = __instance.forwardForce * .5f;
                __instance.verticalForce = __instance.forwardForce * .5f;
                __instance.backwardForce = 0f;
            }
        }
       
        [HarmonyPatch(nameof(SeaMoth.UpdateSounds))]
        [HarmonyPrefix]
        public static bool UpdateSoundsPrefix(SeaMoth __instance)
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

        [HarmonyPatch(nameof(SeaMoth.Update))]
        [HarmonyPrefix]
        public static bool UpdatePrefix(SeaMoth __instance)
        {    // seamoth does not consume more energy when moving diagonally
            if (!Main.config.seamothMoveTweaks)
                return true;

            Vehicle_patch.VehicleUpdate(__instance as Vehicle);

            __instance.UpdateSounds();
            if (__instance.GetPilotingMode())
            {
                string buttonFormat = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
                HandReticle.main.SetUseTextRaw(buttonFormat, string.Empty);
                Vector3 vector3 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                float magnitude = Mathf.Clamp(vector3.magnitude, 0f, 1f);

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
