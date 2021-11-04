using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Vehicle))]
    class Vehicle_patch
    {
        public static Light[] currentLights = new Light[2];
        public static TechType currentVehicleTT;
        public static Dictionary<Vehicle, Vehicle.DockType> dockedVehicles = new Dictionary<Vehicle, Vehicle.DockType>();

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

        [HarmonyPatch(nameof(Vehicle.OnUpgradeModuleChange))]
        [HarmonyPostfix]
        public static void OnUpgradeModuleChangePostfix(Vehicle __instance, TechType techType)
        {
            if (techType != TechType.VehicleArmorPlating)
                return;

            int armorUpgrades = 0;
            for (int i = 0; i < __instance.slotIDs.Length; ++i)
            {
                TechType tt = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[i]);
                if (tt == TechType.VehicleArmorPlating)
                    armorUpgrades++;
            }
            if (armorUpgrades == 1)
                AddDebug("Incoming physical damage will be reduced to 70%");
            else if (armorUpgrades == 2)
                AddDebug("Incoming physical damage will be reduced to 50%");
            else if (armorUpgrades > 2)
                AddDebug("Incoming physical damage will be reduced to 40%");
        }

        [HarmonyPatch(nameof(Vehicle.OnKill))]
        [HarmonyPrefix]
        public static void OnKillPrefix(Vehicle __instance)
        {
            StorageContainer sc = __instance.GetComponentInChildren<StorageContainer>();
            if (sc != null && sc.container != null)
                Main.DropItems(sc.container);
            else
            {
                SeamothStorageContainer ssc = __instance.GetComponentInChildren<SeamothStorageContainer>(true);
                if (ssc != null && ssc.container != null)
                    Main.DropItems(ssc.container);
            }
        }

        [HarmonyPatch("OnDockedChanged")]
        [HarmonyPostfix]
        public static void OnDockedChangedPrefix(Vehicle __instance, bool docked, Vehicle.DockType dockType)
        {
            if (docked)
                dockedVehicles[__instance] = dockType;
            else
                dockedVehicles[__instance] = Vehicle.DockType.None;
        }
    }

    [HarmonyPatch(typeof(SeaMoth))]
    class SeaMoth_patch
    {
        static string exitButton;
        static TechType currentModule;
        static string useButton;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix(SeaMoth __instance)
        {
            if (Vehicle_patch.dockedVehicles.ContainsKey(__instance) && Vehicle_patch.dockedVehicles[__instance] == Vehicle.DockType.Cyclops)
                __instance.animator.Play("seamoth_cyclops_launchbay_dock");

            if (Main.config.seamothMoveTweaks) 
            { 
                __instance.sidewardForce = __instance.forwardForce * .5f;
                __instance.verticalForce = __instance.forwardForce * .5f;
                __instance.backwardForce = 0f;
            }
        }

        [HarmonyPatch("OnPilotModeBegin")]
        [HarmonyPostfix]
        public static void OnPilotModeBeginPostfix(SeaMoth __instance)
        {
            exitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " Toggle lights " + TooltipFactory.stringRightHand;
        }

        [HarmonyPatch("UpdateSounds")]
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

        [HarmonyPatch("OnUpgradeModuleToggle")]
        [HarmonyPostfix]
        public static void OnUpgradeModuleTogglePostfix(SeaMoth __instance, int slotID, bool active)
        {
            currentModule = __instance.modules.GetTechTypeInSlot(__instance.slotIDs[slotID]);
            if (currentModule == TechType.SeamothElectricalDefense)
                useButton = Language.main.Get(currentModule) + ". Press and hold " + TooltipFactory.stringLeftHand + " to charge the shot.";
            else
                useButton = Language.main.Get(currentModule) + " " + TooltipFactory.stringLeftHand;
            ItemsContainer storageInSlot = __instance.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
            //AddDebug("OnUpgradeModuleToggle " + currentModule + " " + active);
            if (currentModule == TechType.SeamothTorpedoModule)
            {
                for (int index = 0; index < __instance.torpedoTypes.Length; ++index)
                {
                    TechType torpedoType = __instance.torpedoTypes[index].techType;
                    if (storageInSlot.Contains(torpedoType))
                    {
                        useButton = Language.main.Get(torpedoType) + " x" + storageInSlot.GetCount(torpedoType) + " " + TooltipFactory.stringLeftHand;
                        break;
                    }
                }
            }
        }

        [HarmonyPatch("OnUpgradeModuleUse")]
        [HarmonyPostfix]
        public static void OnUpgradeModuleUsePostfix(SeaMoth __instance, int slotID, TechType techType)
        {
            if (techType == TechType.SeamothTorpedoModule)
            {
                ItemsContainer storageInSlot = __instance.GetStorageInSlot(slotID, TechType.SeamothTorpedoModule);
                for (int index = 0; index < __instance.torpedoTypes.Length; ++index)
                {
                    if (storageInSlot.Contains(__instance.torpedoTypes[index].techType))
                    {
                        TechType torpedoType = __instance.torpedoTypes[index].techType;
                        useButton = Language.main.Get(torpedoType) + " x" + storageInSlot.GetCount(torpedoType) + " " + TooltipFactory.stringLeftHand ;
                        break;
                    }
                    useButton = Language.main.Get("VehicleTorpedoNoAmmo");
                }
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool UpdatePrefix(SeaMoth __instance)
        {    // seamoth does not consume more energy when moving diagonally
            //if (!Main.config.seamothMoveTweaks)
            //    return true;

            Vehicle_patch.VehicleUpdate(__instance as Vehicle);

            __instance.UpdateSounds();
            if (__instance.GetPilotingMode())
            {

                HandReticle.main.SetUseTextRaw(useButton, exitButton);
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

    [HarmonyPatch(typeof(Exosuit))]
    class Exosuit_Patch
    {
        public static Exosuit myExosuit = null;


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartPostfix(Exosuit __instance)
        {
            //__instance.StartCoroutine(PlayClip(__instance.mainAnimator, "exo_docked"));
            if (Vehicle_patch.dockedVehicles.ContainsKey(__instance))
            {
                Vehicle.DockType dockType = Vehicle_patch.dockedVehicles[__instance];
                if (dockType == Vehicle.DockType.Cyclops)
                {
                    //AddDebug("Play exo_docked Cyclops");
                    __instance.mainAnimator.Play("exo_docked");
                }
                //else if (dockType == Vehicle.DockType.Base)
                //{
                    //AddDebug("Exosuit Start DockType.Base)");
                    //__instance.StartCoroutine(PlayClip(__instance.mainAnimator, "exo_docked", 11f));
                    //AddDebug("Play exoMoonp_docked");
                    //__instance.mainAnimator.Play("exo_docked");
                    //__instance.mainAnimator.Play("exoMoonp_docked");
                //}
            }
            //AddDebug("Exosuit Start " + __instance.docked);
            Main.Log("Exosuit start pos " + __instance.transform.position);
            Main.Log("Exosuit start locpos " + __instance.transform.localPosition);
            CollisionSound collisionSound = __instance.gameObject.EnsureComponent<CollisionSound>();
            FMODAsset so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/common/fishsplat";
            so.id = "{0e47f1c6-6178-41bd-93bf-40bfca179cb6}";
            collisionSound.hitSoundSmall = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_hard";
            so.id = "{ed65a390-2e80-4005-b31b-56380500df33}";
            collisionSound.hitSoundFast = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_medium";
            so.id = "{cb2927bf-3f8d-45d8-afe2-c82128f39062}";
            collisionSound.hitSoundMedium = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_soft";
            so.id = "{15dc7344-7b0a-4ffd-9b5c-c40f923e4f4d}";
            collisionSound.hitSoundSlow = so;
        }

        static void VehicleUpdate(Vehicle vehicle)
        {
            if (vehicle.GetPilotingMode() && vehicle.CanPilot() && (vehicle.moveOnLand || vehicle.transform.position.y < Ocean.main.GetOceanLevel()))
            {
                Vector2 vector2 = AvatarInputHandler.main.IsEnabled() ? GameInput.GetLookDelta() : Vector2.zero;
                vehicle.steeringWheelYaw = Mathf.Clamp(vehicle.steeringWheelYaw + vector2.x * vehicle.steeringReponsiveness, -1f, 1f);
                vehicle.steeringWheelPitch = Mathf.Clamp(vehicle.steeringWheelPitch + vector2.y * vehicle.steeringReponsiveness, -1f, 1f);

                if (vehicle.controlSheme == Vehicle.ControlSheme.Mech)
                {
                    if (vehicle.rotationLocked)
                    {
                        vehicle.angularVelocity.y = UWE.Utils.Slerp(vehicle.angularVelocity.y, 0.0f, Mathf.Max(vehicle.useRigidbody.angularDrag, Mathf.Abs(vehicle.angularVelocity.y) * vehicle.useRigidbody.angularDrag) * Time.deltaTime);
                        if (vector2.x != 0.0f)
                            vehicle.angularVelocity.y += vector2.x * vehicle.sidewaysTorque;
                        vehicle.transform.localEulerAngles = vehicle.transform.localEulerAngles + vehicle.angularVelocity * Time.deltaTime;
                    }
                    else if (vector2.x != 0.0f)
                        vehicle.useRigidbody.AddTorque(vehicle.transform.up * vector2.x * vehicle.sidewaysTorque, ForceMode.VelocityChange);
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

        //[HarmonyPrefix]
        //[HarmonyPatch("FixedUpdate")]
        public static bool FixedUpdatePrefix(Exosuit __instance)
        {
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(Exosuit __instance)
        {    // thrusters consumes 2x energy
             // no limit on thrusters
             //  strafing disabled in SeamothHandlingFix
            if (!Main.config.exosuitMoveTweaks)
                return true;

            VehicleUpdate(__instance);

            __instance.UpdateThermalReactorCharge();
            __instance.openedFraction = !__instance.storageContainer.GetOpen() ? Mathf.Clamp01(__instance.openedFraction - Time.deltaTime * 2f) : Mathf.Clamp01(__instance.openedFraction + Time.deltaTime * 2f);
            __instance.storageFlap.localEulerAngles = new Vector3(__instance.startFlapPitch + __instance.openedFraction * 80f, 0.0f, 0.0f);
            bool pilotingMode = __instance.GetPilotingMode();
            bool onGround = __instance.onGround || Time.time - __instance.timeOnGround <= 0.5f;
            __instance.mainAnimator.SetBool("sit", !pilotingMode & onGround && !__instance.IsUnderwater());
            bool inUse = pilotingMode && !__instance.docked;
            if (pilotingMode)
            {
                Player.main.transform.localPosition = Vector3.zero;
                Player.main.transform.localRotation = Quaternion.identity;
                Vector3 input = AvatarInputHandler.main.IsEnabled() ? GameInput.GetMoveDirection() : Vector3.zero;
                bool thrusterOn = input.y > 0f;
                bool hasPower = __instance.IsPowered() && __instance.liveMixin.IsAlive();

                __instance.GetEnergyValues(out float charge, out float capacity);
                __instance.thrustPower = Main.NormalizeTo01range(charge, 0f, capacity);
                //Main.Message("thrustPower " + __instance.thrustPower);
                if (thrusterOn & hasPower)
                {
                    if ((__instance.onGround || Time.time - __instance.timeOnGround <= 1f) && !__instance.jetDownLastFrame)
                        __instance.ApplyJumpForce();
                    __instance.jetsActive = true;
                }
                else
                {
                    __instance.jetsActive = false;
                }
                //AddDebug("jetsActive" + __instance.jetsActive);
                __instance.jetDownLastFrame = thrusterOn;

                if (__instance.timeJetsActiveChanged + 0.3f < Time.time)
                {
                    if (__instance.jetsActive && __instance.thrustPower > 0.0f)
                    {
                        __instance.loopingJetSound.Play();
                        __instance.fxcontrol.Play(0);
                        __instance.areFXPlaying = true;
                    }
                    else if (__instance.areFXPlaying)
                    {
                        __instance.loopingJetSound.Stop();
                        __instance.fxcontrol.Stop(0);
                        __instance.areFXPlaying = false;
                    }
                }
                float energyCost = __instance.thrustConsumption * Time.deltaTime;
                if (thrusterOn)
                {
                    __instance.ConsumeEngineEnergy(energyCost * 2f);
                    //Main.Message("Consume Energy thrust" + energyCost * 2f);
                }
                else if (input.z != 0f)
                {
                    __instance.ConsumeEngineEnergy(energyCost);
                    //Main.Message("Consume Energy move" + energyCost);
                }
                if (__instance.jetsActive)
                    __instance.thrustIntensity += Time.deltaTime / __instance.timeForFullVirbation;
                else
                    __instance.thrustIntensity -= Time.deltaTime * 10f;

                __instance.thrustIntensity = Mathf.Clamp01(__instance.thrustIntensity);
                if (AvatarInputHandler.main.IsEnabled())
                {
                    Vector3 eulerAngles = __instance.transform.eulerAngles;
                    eulerAngles.x = MainCamera.camera.transform.eulerAngles.x;
                    Quaternion aimDirection1 = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    Quaternion aimDirection2 = aimDirection1;
                    __instance.leftArm.Update(ref aimDirection1);
                    __instance.rightArm.Update(ref aimDirection2);
                    if (inUse)
                    {
                        __instance.aimTargetLeft.transform.position = MainCamera.camera.transform.position + aimDirection1 * Vector3.forward * 100f;
                        __instance.aimTargetRight.transform.position = MainCamera.camera.transform.position + aimDirection2 * Vector3.forward * 100f;
                    }
                    __instance.UpdateUIText(__instance.rightArm is ExosuitPropulsionArm || __instance.leftArm is ExosuitPropulsionArm);
                    if (GameInput.GetButtonDown(GameInput.Button.AltTool) && !__instance.rightArm.OnAltDown())
                        __instance.leftArm.OnAltDown();
                }
                __instance.UpdateActiveTarget(__instance.HasClaw(), __instance.HasDrill());
                __instance.UpdateSounds();
            }
            if (!inUse)
            {
                bool flag3 = false;
                bool flag4 = false;
                if (!Mathf.Approximately(__instance.aimTargetLeft.transform.localPosition.y, 0.0f))
                    __instance.aimTargetLeft.transform.localPosition = new Vector3(__instance.aimTargetLeft.transform.localPosition.x, UWE.Utils.Slerp(__instance.aimTargetLeft.transform.localPosition.y, 0.0f, Time.deltaTime * 50f), __instance.aimTargetLeft.transform.localPosition.z);
                else
                    flag3 = true;
                if (!Mathf.Approximately(__instance.aimTargetRight.transform.localPosition.y, 0.0f))
                    __instance.aimTargetRight.transform.localPosition = new Vector3(__instance.aimTargetRight.transform.localPosition.x, UWE.Utils.Slerp(__instance.aimTargetRight.transform.localPosition.y, 0.0f, Time.deltaTime * 50f), __instance.aimTargetRight.transform.localPosition.z);
                else
                    flag4 = true;
                if (flag3 & flag4)
                    __instance.SetIKEnabled(false);
            }
            __instance.UpdateAnimations();
            if (__instance.armsDirty)
                __instance.UpdateExosuitArms();

            if (!__instance.cinematicMode && __instance.rotationDirty)
            {
                Vector3 localEulerAngles = __instance.transform.localEulerAngles;
                Quaternion b = Quaternion.Euler(0.0f, localEulerAngles.y, 0.0f);
                if ((double)Mathf.Abs(localEulerAngles.x) < 1.0 / 1000.0 && (double)Mathf.Abs(localEulerAngles.z) < 1.0 / 1000.0)
                {
                    __instance.rotationDirty = false;
                    __instance.transform.localRotation = b;
                }
                else
                    __instance.transform.localRotation = Quaternion.Lerp(__instance.transform.localRotation, b, Time.deltaTime * 3f);
            }
            return false;
        }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdatePostfix(Exosuit __instance)
        {
            if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
            {
                if (Main.vehicleLightsImprovedLoaded)
                    return;

                Transform lightsT = __instance.transform.Find("lights_parent");
                if (lightsT)
                {
                    if (!lightsT.gameObject.activeSelf && __instance.energyInterface.hasCharge)
                        lightsT.gameObject.SetActive(true);
                    else if (lightsT.gameObject.activeSelf)
                        lightsT.gameObject.SetActive(false);
                    //AddDebug("lights " + lightsT.gameObject.activeSelf);
                }
            }
        }
        [HarmonyPatch("UpdateUIText")]
        [HarmonyPrefix]
        public static bool UpdateUITextPrefix(Exosuit __instance, bool hasPropCannon)
        {
            if (Main.vehicleLightsImprovedLoaded)
                return true;

            if (!__instance.hasInitStrings || __instance.lastHasPropCannon != hasPropCannon)
            {
                __instance.sb.Length = 0;
                __instance.sb.AppendLine(LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + " Toggle lights (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")");
                //__instance.sb.AppendLine("Toggle lights (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")");
                if (hasPropCannon)
                    __instance.sb.AppendLine(LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool));
                __instance.lastHasPropCannon = hasPropCannon;
                __instance.uiStringPrimary = __instance.sb.ToString();
            }
            HandReticle.main.SetUseTextRaw(__instance.uiStringPrimary, string.Empty);
            __instance.hasInitStrings = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(ExosuitDrillArm))]
    [HarmonyPatch("OnHit")]
    class ExosuitDrillArm_OnHit_Patch
    { // fix not showing particles when start drilling
        public static bool Prefix(ExosuitDrillArm __instance)
        {
            //AddDebug("OnHit");
            if (!__instance.exosuit.CanPilot() || !__instance.exosuit.GetPilotingMode())
                return false;
            Vector3 zero = Vector3.zero;
            GameObject closestObj = null;
            __instance.drillTarget = null;
            UWE.Utils.TraceFPSTargetPosition(__instance.exosuit.gameObject, 5f, ref closestObj, ref zero);
            if (closestObj == null)
            {
                InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                if (component != null && component.GetMostRecent() != null)
                    closestObj = component.GetMostRecent().gameObject;
            }
            if (closestObj && __instance.drilling)
            {
                Drillable ancestor1 = closestObj.FindAncestor<Drillable>();
                __instance.loopHit.Play();
                if (ancestor1)
                {
                    GameObject hitObject;
                    ancestor1.OnDrill(__instance.fxSpawnPoint.position, __instance.exosuit, out hitObject);
                    __instance.drillTarget = hitObject;
                    //if (__instance.fxControl.emitters[0].fxPS == null || __instance.fxControl.emitters[0].fxPS.emission.enabled) 
                    //AddDebug("emission.enabled " + __instance.fxControl.emitters[0].fxPS.emission.enabled);
                    //AddDebug("IsAlive " + __instance.fxControl.emitters[0].fxPS.IsAlive());
                    if (__instance.fxControl.emitters[0].fxPS != null && (!__instance.fxControl.emitters[0].fxPS.IsAlive() || !__instance.fxControl.emitters[0].fxPS.emission.enabled))
                    {
                        __instance.fxControl.Play(0);
                    }

                }
                else
                {
                    LiveMixin ancestor2 = closestObj.FindAncestor<LiveMixin>();
                    if (ancestor2)
                    {
                        ancestor2.IsAlive();
                        ancestor2.TakeDamage(4f, zero, DamageType.Drill);
                        __instance.drillTarget = closestObj;
                    }
                    VFXSurface component = closestObj.GetComponent<VFXSurface>();
                    if (__instance.drillFXinstance == null)
                        __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(component, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                    else if (component != null && __instance.prevSurfaceType != component.surfaceType)
                    {
                        __instance.drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
                        UnityEngine.Object.Destroy(__instance.drillFXinstance.gameObject, 1.6f);
                        __instance.drillFXinstance = VFXSurfaceTypeManager.main.Play(component, __instance.vfxEventType, __instance.fxSpawnPoint.position, __instance.fxSpawnPoint.rotation, __instance.fxSpawnPoint);
                        __instance.prevSurfaceType = component.surfaceType;
                    }
                    closestObj.SendMessage("BashHit", __instance, SendMessageOptions.DontRequireReceiver);
                }
            }
            else
                __instance.StopEffects();

            return false;
        }

    }

    [HarmonyPatch(typeof(CollisionSound), "OnCollisionEnter")]
    class CollisionSound_OnCollisionEnter_Patch
    { // fix fish splat sound when colliding with rocks
        static bool Prefix(CollisionSound __instance, Collision col)
        {
            //SeaMoth seaMoth = __instance.GetComponent<SeaMoth>();
            //if (seaMoth)
            //{
            //    Main.Log(" hitSoundSmall path " + __instance.hitSoundSmall.path);
            //    Main.Log(" hitSoundSmall id " + __instance.hitSoundSmall.id);
            //    Main.Log(" hitSoundFast path " + __instance.hitSoundFast.path);
            //    Main.Log(" hitSoundFast id " + __instance.hitSoundFast.id);
            //    Main.Log(" hitSoundMedium path " + __instance.hitSoundMedium.path);
            //    Main.Log(" hitSoundMedium id " + __instance.hitSoundMedium.id);
            //    Main.Log(" hitSoundSlow path " + __instance.hitSoundSlow.path);
            //    Main.Log(" hitSoundSlow id " + __instance.hitSoundSlow.id);
            //}
            Exosuit exosuit = __instance.GetComponent<Exosuit>();
            Rigidbody rb = UWE.Utils.GetRootRigidbody(col.gameObject);
            if (exosuit && !rb)
                return false;// no sounds when walking on ground

            float magnitude = col.relativeVelocity.magnitude;
            //FMODAsset asset = !rootRigidbody || rootRigidbody.mass >= 10.0 ? (magnitude <= 8.0 ? (magnitude <= 4.0 ? __instance.hitSoundSlow : __instance.hitSoundMedium) : __instance.hitSoundFast) : __instance.hitSoundSmall;
            FMODAsset asset = null;
            if (!rb || rb.mass >= 10.0f)
            {
                if (magnitude < 4f)
                    asset = __instance.hitSoundSlow;
                else if (magnitude < 8f)
                    asset = __instance.hitSoundMedium;
                else
                    asset = __instance.hitSoundFast;
            }
            else if (col.gameObject.GetComponent<Creature>())
                asset = __instance.hitSoundSmall;// fish splat sound
            else
                asset = __instance.hitSoundSlow;

            if (asset)
            {
                //AddDebug("col magnitude " + magnitude);
                float soundRadiusObsolete = Mathf.Clamp01(magnitude / 8f);
                Utils.PlayFMODAsset(asset, col.contacts[0].point, soundRadiusObsolete);
            }

            return false;
        }
    }

}
