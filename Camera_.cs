using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Camera_
    {
        [HarmonyPatch(typeof(MainCameraControl), "ShakeCamera")]
        class MainCameraControl_ShakeCamera_Patch
        {
            static bool Prefix(MainCameraControl __instance)
            {
                //AddDebug("MainCameraControl ShakeCamera");
                return ConfigToEdit.cameraShake.Value;
            }
        }

        [HarmonyPatch(typeof(DamageFX), "AddHudDamage")]
        class DamageFX_AddHudDamage_Patch
        {
            public static bool Prefix(DamageFX __instance, float damageScalar, Vector3 damageSource, DamageInfo damageInfo)
            {
                //Main.config.crushDamageScreenEffect = false;
                //AddDebug("AddHudDamage " + damageInfo.type);
                if (!ConfigToEdit.crushDamageScreenEffect.Value && damageInfo.type == DamageType.Pressure)
                    return false;

                if (ConfigMenu.damageImpactEffect.Value)
                    __instance.CreateImpactEffect(damageScalar, damageSource, damageInfo.type);

                if (ConfigMenu.damageScreenFX.Value)
                    __instance.PlayScreenFX(damageInfo);

                return false;
            }
        }


        [HarmonyPatch(typeof(MainCameraControl), "OnUpdate")]
        class MainCameraControl_Patch
        {
            static readonly float highFOVseaglideCameraOffset = 0.02f;

            public static bool Prefix(MainCameraControl __instance)
            {
                if (XRSettings.enabled || MiscSettings.fieldOfView < 75 || Inventory.main.quickSlots.activeToolName != "seaglide")
                    return true;

                HighFOVseaglideOnUpdate(__instance);
                return false;
            }

            private static void HighFOVseaglideOnUpdate(MainCameraControl mainCameraControl)
            {// fix: can see your neck wnen using seaglide and high FOV
                //AddDebug("MainCameraControl OnUpdate " + Inventory.main.quickSlots.activeToolName);
                float deltaTime = Time.deltaTime;
                mainCameraControl.swimCameraAnimation = !mainCameraControl.underWaterTracker.isUnderWater ? Mathf.Clamp01(mainCameraControl.swimCameraAnimation - deltaTime) : Mathf.Clamp01(mainCameraControl.swimCameraAnimation + deltaTime);
                Vector3 velocity = mainCameraControl.playerController.velocity;
                bool pdaInUse = false;
                bool flag2 = false;
                bool inVehicle = false;
                bool inExosuit = Player.main.inExosuit;
                bool builderMenuOpen = uGUI_BuilderMenu.IsOpen();
                if (Player.main != null)
                {
                    pdaInUse = Player.main.GetPDA().isInUse;
                    inVehicle = Player.main.motorMode == Player.MotorMode.Vehicle;
                    flag2 = pdaInUse | inVehicle || mainCameraControl.cinematicMode;
                    if (XRSettings.enabled && VROptions.gazeBasedCursor)
                        flag2 |= builderMenuOpen;
                }
                if (flag2 != mainCameraControl.wasInLockedMode || mainCameraControl.lookAroundMode != mainCameraControl.wasInLookAroundMode)
                {
                    mainCameraControl.camRotationX = 0.0f;
                    mainCameraControl.camRotationY = 0.0f;
                    mainCameraControl.wasInLockedMode = flag2;
                    mainCameraControl.wasInLookAroundMode = mainCameraControl.lookAroundMode;
                }
                bool flag5 = (!mainCameraControl.cinematicMode || mainCameraControl.lookAroundMode && !pdaInUse) && mainCameraControl.mouseLookEnabled && (inVehicle || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
                if (inVehicle && !XRSettings.enabled && !inExosuit)
                    flag5 = false;

                Transform transform = mainCameraControl.transform;
                float num1 = pdaInUse || mainCameraControl.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting ? 1f : -1f;
                if (!flag2 || mainCameraControl.cinematicMode && !mainCameraControl.lookAroundMode)
                {
                    mainCameraControl.cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(mainCameraControl.cameraOffsetTransform.localEulerAngles, Vector3.zero, deltaTime * 5f);
                }
                else
                {
                    transform = mainCameraControl.cameraOffsetTransform;
                    mainCameraControl.rotationY = Mathf.LerpAngle(mainCameraControl.rotationY, 0.0f, PDA.deltaTime * 15f);
                    mainCameraControl.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(mainCameraControl.transform.localEulerAngles.x, 0.0f, PDA.deltaTime * 15f), mainCameraControl.transform.localEulerAngles.y, 0.0f);
                    mainCameraControl.cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(mainCameraControl.cameraUPTransform.localEulerAngles, Vector3.zero, PDA.deltaTime * 15f);
                }
                if (!XRSettings.enabled)
                {
                    Vector3 localPosition = mainCameraControl.cameraOffsetTransform.localPosition;
                    localPosition.z = highFOVseaglideCameraOffset;
                    //localPosition.z = Mathf.Clamp(localPosition.z + (PDA.deltaTime * num1 * 0.25f), __instance.camPDAZStart, __instance.camPDAZOffset + __instance.camPDAZStart);
                    //AddDebug("  localPosition.z " + localPosition.z.ToString("0.00"));
                    mainCameraControl.cameraOffsetTransform.localPosition = localPosition;
                }
                Vector2 vector2 = Vector2.zero;
                if (flag5 && FPSInputModule.current.lastGroup == null)
                {
                    Vector2 lookDelta = GameInput.GetLookDelta();
                    if (XRSettings.enabled && VROptions.disableInputPitch)
                        lookDelta.y = 0.0f;
                    if (inExosuit)
                        lookDelta.x = 0.0f;
                    vector2 = lookDelta * Player.main.mesmerizedSpeedMultiplier;
                }
                mainCameraControl.UpdateCamShake();
                if (mainCameraControl.cinematicMode && !mainCameraControl.lookAroundMode)
                {
                    mainCameraControl.camRotationX = Mathf.LerpAngle(mainCameraControl.camRotationX, 0.0f, deltaTime * 2f);
                    mainCameraControl.camRotationY = Mathf.LerpAngle(mainCameraControl.camRotationY, 0.0f, deltaTime * 2f);
                    mainCameraControl.transform.localEulerAngles = new Vector3(-mainCameraControl.camRotationY + mainCameraControl.camShake, mainCameraControl.camRotationX, 0.0f);
                }
                else if (flag2)
                {
                    if (!XRSettings.enabled)
                    {
                        bool flag6 = !mainCameraControl.lookAroundMode | pdaInUse;
                        int num2 = !mainCameraControl.lookAroundMode | pdaInUse ? 1 : 0;
                        Vehicle vehicle = Player.main.GetVehicle();
                        if (vehicle != null)
                            flag6 = vehicle.controlSheme != Vehicle.ControlSheme.Mech | pdaInUse;
                        mainCameraControl.camRotationX += vector2.x;
                        mainCameraControl.camRotationY += vector2.y;
                        mainCameraControl.camRotationX = Mathf.Clamp(mainCameraControl.camRotationX, -60f, 60f);
                        mainCameraControl.camRotationY = Mathf.Clamp(mainCameraControl.camRotationY, -60f, 60f);
                        if (num2 != 0)
                            mainCameraControl.camRotationX = Mathf.LerpAngle(mainCameraControl.camRotationX, 0.0f, PDA.deltaTime * 10f);
                        if (flag6)
                            mainCameraControl.camRotationY = Mathf.LerpAngle(mainCameraControl.camRotationY, 0.0f, PDA.deltaTime * 10f);
                        mainCameraControl.cameraOffsetTransform.localEulerAngles = new Vector3(-mainCameraControl.camRotationY, mainCameraControl.camRotationX + mainCameraControl.camShake, 0.0f);
                    }
                }
                else
                {
                    mainCameraControl.rotationX += vector2.x;
                    mainCameraControl.rotationY += vector2.y;
                    mainCameraControl.rotationY = Mathf.Clamp(mainCameraControl.rotationY, mainCameraControl.minimumY, mainCameraControl.maximumY);
                    mainCameraControl.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0.0f, -mainCameraControl.rotationY + mainCameraControl.camShake), 0.0f, 0.0f);
                    transform.localEulerAngles = new Vector3(Mathf.Max(0.0f, -mainCameraControl.rotationY + mainCameraControl.camShake), mainCameraControl.rotationX, 0.0f);
                }
                mainCameraControl.UpdateStrafeTilt();
                Vector3 vector3_1 = mainCameraControl.transform.localEulerAngles + new Vector3(0.0f, 0.0f, (mainCameraControl.cameraAngleMotion.y * mainCameraControl.cameraTiltMod + mainCameraControl.strafeTilt + mainCameraControl.camShake * 0.5f));
                float num3 = 0.0f - mainCameraControl.skin;
                if (!flag2 && mainCameraControl.GetCameraBob())
                {
                    mainCameraControl.smoothedSpeed = UWE.Utils.Slerp(mainCameraControl.smoothedSpeed, Mathf.Min(1f, velocity.magnitude / 5f), deltaTime);
                    num3 += ((Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + mainCameraControl.smoothedSpeed * 0.15f)) * mainCameraControl.swimCameraAnimation;
                }
                if (mainCameraControl.impactForce > 0)
                {
                    mainCameraControl.impactBob = Mathf.Min(0.9f, mainCameraControl.impactBob + mainCameraControl.impactForce * deltaTime);
                    mainCameraControl.impactForce -= (Mathf.Max(1f, mainCameraControl.impactForce) * deltaTime * 5f);
                }
                float y = num3 - mainCameraControl.impactBob - mainCameraControl.stepAmount;
                if (mainCameraControl.impactBob > 0.0)
                    mainCameraControl.impactBob = Mathf.Max(0.0f, mainCameraControl.impactBob - (Mathf.Pow(mainCameraControl.impactBob, 0.5f) * Time.deltaTime * 3f));
                mainCameraControl.stepAmount = Mathf.Lerp(mainCameraControl.stepAmount, 0f, deltaTime * Mathf.Abs(mainCameraControl.stepAmount));
                mainCameraControl.transform.localPosition = new Vector3(0f, y, 0f);
                mainCameraControl.transform.localEulerAngles = vector3_1;
                if (Player.main.motorMode == Player.MotorMode.Vehicle)
                    mainCameraControl.transform.localEulerAngles = Vector3.zero;
                Vector3 vector3_2 = new Vector3(0f, mainCameraControl.transform.localEulerAngles.y, 0.0f);
                Vector3 vector3_3 = mainCameraControl.transform.localPosition;
                if (XRSettings.enabled)
                {
                    vector3_2.y = !flag2 || inVehicle ? 0f : mainCameraControl.viewModelLockedYaw;
                    if (!inVehicle && !mainCameraControl.cinematicMode)
                    {
                        if (!flag2)
                        {
                            Quaternion rotation = mainCameraControl.playerController.forwardReference.rotation;
                            Quaternion quaternion = mainCameraControl.gameObject.transform.parent.rotation.GetInverse() * rotation;
                            vector3_2.y = quaternion.eulerAngles.y;
                        }
                        vector3_3 = mainCameraControl.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(mainCameraControl.playerController.forwardReference.position);
                    }
                }
                mainCameraControl.viewModel.transform.localEulerAngles = vector3_2;
                mainCameraControl.viewModel.transform.localPosition = vector3_3;
            }
        }
    }

}
