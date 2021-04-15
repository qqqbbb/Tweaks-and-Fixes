
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(CollisionSound), "OnCollisionEnter")]
    class CollisionSound_OnCollisionEnter_Patch
    { // fix fish splat sound whem colliding with rocks
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
                //ErrorMessage.AddDebug("col magnitude " + magnitude);
                float soundRadiusObsolete = Mathf.Clamp01(magnitude / 8f);
                Utils.PlayFMODAsset(asset, col.contacts[0].point, soundRadiusObsolete);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Exosuit), "Start")]
    class Exosuit_Start_Patch
    {
        static void Postfix(Exosuit __instance)
        {
            CollisionSound collisionSound = __instance.gameObject.EnsureComponent<CollisionSound>();

            FMODAsset s = ScriptableObject.CreateInstance<FMODAsset>();
            s.path = "event:/sub/common/fishsplat";
            s.id = "{0e47f1c6-6178-41bd-93bf-40bfca179cb6}";
            collisionSound.hitSoundSmall = s;
            s = ScriptableObject.CreateInstance<FMODAsset>();
            s.path = "event:/sub/seamoth/impact_solid_hard";
            s.id = "{ed65a390-2e80-4005-b31b-56380500df33}";
            collisionSound.hitSoundFast = s;
            s = ScriptableObject.CreateInstance<FMODAsset>();
            s.path = "event:/sub/seamoth/impact_solid_medium";
            s.id = "{cb2927bf-3f8d-45d8-afe2-c82128f39062}";
            collisionSound.hitSoundMedium = s;
            s = ScriptableObject.CreateInstance<FMODAsset>();
            s.path = "event:/sub/seamoth/impact_solid_soft";
            s.id = "{15dc7344-7b0a-4ffd-9b5c-c40f923e4f4d}";
            collisionSound.hitSoundSlow = s;
        }
    }

    // thrusters consumes 2x energy
    // no limit on thrusters
    //  strafing disabled in SeamothHandlingFix
    [HarmonyPatch(typeof(Exosuit), "Update")]
    class Exosuit_Update_Patch
    {
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

        public static bool Prefix(Exosuit __instance)
        {
            //Vehicle vehicle = __instance as Vehicle;
            //vehicle.Update();
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
                //ErrorMessage.AddDebug("jetsActive" + __instance.jetsActive);
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
    }

    [HarmonyPatch(typeof(ExosuitDrillArm))]
    [HarmonyPatch("OnHit")]
    class ExosuitDrillArm_OnHit_Patch
    { // fix not showing particles when start drilling
        public static bool Prefix(ExosuitDrillArm __instance)
        {
            //ErrorMessage.AddDebug("OnHit");
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
                    //ErrorMessage.AddDebug("emission.enabled " + __instance.fxControl.emitters[0].fxPS.emission.enabled);
                    //ErrorMessage.AddDebug("IsAlive " + __instance.fxControl.emitters[0].fxPS.IsAlive());
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
}

