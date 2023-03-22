using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using FMOD.Studio;
using System.Text;
using RootMotion.FinalIK;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class PDA_Patch
    { 
        //static ConditionRules conditionRules;
        //static int ruleToRemove;

        [HarmonyPatch(typeof(PDA))]
        class PDA_Open_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Open")]
            static bool OpenPrefix(PDA __instance, PDATab tab, Transform target, PDA.OnClose onCloseCallback, ref bool __result)
            {
                if (__instance.isInUse || __instance.ignorePDAInput)
                {
                    __result = false;
                    return false;
                }
                //AddDebug("PDA Open");
                uGUI.main.quickSlots.SetTarget(null);
                __instance.prevQuickSlot = Inventory.main.quickSlots.activeSlot;
                //int num1 = Inventory.main.ReturnHeld() ? 1 : 0;
                Player player = Player.main;
                if (!Inventory.main.ReturnHeld() || player.cinematicModeActive)
                {
                    __result = false;
                    return false;
                }
                MainCameraControl.main.SaveLockedVRViewModelAngle();
                Inventory.main.quickSlots.SetSuspendSlotActivation(true);
                __instance.isInUse = true;
                player.armsController.SetUsingPda(true);
                __instance.gameObject.SetActive(true);
                __instance.ui.OnOpenPDA(tab);
                //__instance.sequence.Set(.5f, true, new SequenceCallback(__instance.Activated));
                __instance.sequence.Set(0f, true, new SequenceCallback(__instance.Activated));
                GoalManager.main.OnCustomGoalEvent("Open_PDA");
                if (HandReticle.main != null)
                    HandReticle.main.RequestCrosshairHide();

                Inventory.main.SetViewModelVis(false);
                __instance.targetWasSet = target != null;
                __instance.target = target;
                __instance.onCloseCallback = onCloseCallback;
                if (__instance.targetWasSet)
                    __instance.activeSqrDistance = (target.transform.position - player.transform.position).sqrMagnitude + 1f;

                if (__instance.audioSnapshotInstance.isValid())
                {
                    int num2 = (int)__instance.audioSnapshotInstance.start();
                }
                UwePostProcessingManager.OpenPDA();
                __result = true;
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("Close")]
            static bool ClosePrefix(PDA __instance)
            {
                if (!__instance.isInUse || __instance.ignorePDAInput)
                    return false;

                Player player = Player.main;
                QuickSlots quickSlots = Inventory.main.quickSlots;
                quickSlots.EndAssign();
                MainCameraControl.main.ResetLockedVRViewModelAngle();
                Vehicle vehicle = player.GetVehicle();
                if (vehicle != null)
                    uGUI.main.quickSlots.SetTarget((IQuickSlots)vehicle);
                __instance.targetWasSet = false;
                __instance.target = null;
                player.armsController.SetUsingPda(false);
                quickSlots.SetSuspendSlotActivation(false);
                __instance.ui.OnClosePDA();
                if (HandReticle.main != null)
                    HandReticle.main.UnrequestCrosshairHide();

                Inventory.main.SetViewModelVis(true);
                __instance.sequence.Set(0f, false, new SequenceCallback(__instance.Deactivated));
                if (__instance.audioSnapshotInstance.isValid())
                {
                    int num1 = (int)__instance.audioSnapshotInstance.stop(STOP_MODE.ALLOWFADEOUT);
                    int num2 = (int)__instance.audioSnapshotInstance.release();
                }
                UwePostProcessingManager.ClosePDA();
                if (__instance.onCloseCallback == null)
                    return false;
                PDA.OnClose onCloseCallback = __instance.onCloseCallback;
                __instance.onCloseCallback = null;
                onCloseCallback(__instance);
                return false;
            }
        }

        //[HarmonyPatch(typeof(MainCameraControl), "OnUpdate")]
        class MainCameraControl_OnUpdate_Patch
        {
            static bool Prefix(MainCameraControl __instance)
            {
                float deltaTime = Time.deltaTime;
                __instance.swimCameraAnimation = !__instance.underWaterTracker.isUnderWater ? Mathf.Clamp01(__instance.swimCameraAnimation - deltaTime) : Mathf.Clamp01(__instance.swimCameraAnimation + deltaTime);
                double minimumY = __instance.minimumY;
                double maximumY = __instance.maximumY;
                Vector3 velocity = __instance.playerController.velocity;
                bool pdaInUse = false;
                bool fixedCam = false;
                bool flag3 = false;
                bool inExosuit = Player.main.inExosuit;
                bool flag4 = uGUI_BuilderMenu.IsOpen();
                if (Player.main != null)
                {
                    pdaInUse = Player.main.GetPDA().isInUse;
                    flag3 = Player.main.motorMode == Player.MotorMode.Vehicle;
                    fixedCam = pdaInUse | flag3 || __instance.cinematicMode;
                    if (UnityEngine.XR.XRSettings.enabled && VROptions.gazeBasedCursor)
                        fixedCam |= flag4;
                }
                if (fixedCam != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
                {
                    __instance.camRotationX = 0f;
                    __instance.camRotationY = 0f;
                    __instance.wasInLockedMode = fixedCam;
                    __instance.wasInLookAroundMode = __instance.lookAroundMode;
                }
                bool flag5 = (!__instance.cinematicMode || __instance.lookAroundMode && !pdaInUse) && __instance.mouseLookEnabled && (flag3 || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
                if (flag3 && !UnityEngine.XR.XRSettings.enabled && !inExosuit)
                    flag5 = false;
                Transform transform = __instance.transform;
                float num1 = pdaInUse || __instance.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting ? 1f : -1f;
                if (!fixedCam || __instance.cinematicMode && !__instance.lookAroundMode)
                {
                    __instance.cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraOffsetTransform.localEulerAngles, Vector3.zero, deltaTime * 5f);
                }
                else
                {
                    transform = __instance.cameraOffsetTransform;
                    __instance.rotationY = Mathf.LerpAngle(__instance.rotationY, 0.0f, PDA.deltaTime * 15f);
                    __instance.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(__instance.transform.localEulerAngles.x, 0.0f, PDA.deltaTime * 15f), __instance.transform.localEulerAngles.y, 0.0f);
                    __instance.cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraUPTransform.localEulerAngles, Vector3.zero, PDA.deltaTime * 15f);
                }
                if (!UnityEngine.XR.XRSettings.enabled)
                {
                    Vector3 localPosition = __instance.cameraOffsetTransform.localPosition;
                    localPosition.z = Mathf.Clamp(localPosition.z + (PDA.deltaTime * num1 * 0.25f), 0f + __instance.camPDAZStart, __instance.camPDAZOffset + __instance.camPDAZStart);
                    __instance.cameraOffsetTransform.localPosition = localPosition;
                }
                Vector2 vector2 = Vector2.zero;
                if (flag5 && FPSInputModule.current.lastGroup == null)
                {
                    Vector2 lookDelta = GameInput.GetLookDelta();
                    if (UnityEngine.XR.XRSettings.enabled && VROptions.disableInputPitch)
                        lookDelta.y = 0.0f;
                    if (inExosuit)
                        lookDelta.x = 0.0f;
                    vector2 = lookDelta * Player.main.mesmerizedSpeedMultiplier;
                }
                __instance.UpdateCamShake();
                if (__instance.cinematicMode && !__instance.lookAroundMode)
                {
                    __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, deltaTime * 2f);
                    __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, deltaTime * 2f);
                    __instance.transform.localEulerAngles = new Vector3(-__instance.camRotationY + __instance.camShake, __instance.camRotationX, 0f);
                }
                else if (fixedCam)
                {
                    if (!UnityEngine.XR.XRSettings.enabled)
                    {
                        bool flag6 = !__instance.lookAroundMode | pdaInUse;
                        int num2 = !__instance.lookAroundMode | pdaInUse ? 1 : 0;
                        Vehicle vehicle = Player.main.GetVehicle();
                        if (vehicle != null)
                            flag6 = vehicle.controlSheme != Vehicle.ControlSheme.Mech | pdaInUse;
                        __instance.camRotationX += vector2.x;
                        __instance.camRotationY += vector2.y;
                        __instance.camRotationX = Mathf.Clamp(__instance.camRotationX, -60f, 60f);
                        __instance.camRotationY = Mathf.Clamp(__instance.camRotationY, -60f, 60f);
                        if (num2 != 0)
                            __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0.0f, PDA.deltaTime * 10f);
                        if (flag6)
                            __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0.0f, PDA.deltaTime * 10f);
                        __instance.cameraOffsetTransform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX + __instance.camShake, 0.0f);
                    }
                }
                else
                {
                    __instance.rotationX += vector2.x;
                    __instance.rotationY += vector2.y;
                    __instance.rotationY = Mathf.Clamp(__instance.rotationY, __instance.minimumY, __instance.maximumY);
                    __instance.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, -__instance.rotationY + __instance.camShake), 0f, 0f);
                    transform.localEulerAngles = new Vector3(Mathf.Max(0.0f, -__instance.rotationY + __instance.camShake), __instance.rotationX, 0f);
                }
                __instance.UpdateStrafeTilt();
                Vector3 vector3_1 = __instance.transform.localEulerAngles + new Vector3(0f, 0f, (__instance.cameraAngleMotion.y * __instance.cameraTiltMod + __instance.strafeTilt + __instance.camShake * 0.5f));
                float num3 = 0.0f - __instance.skin;
                if (!fixedCam && __instance.GetCameraBob())
                {
                    __instance.smoothedSpeed = UWE.Utils.Slerp(__instance.smoothedSpeed, Mathf.Min(1f, velocity.magnitude / 5f), deltaTime);
                    num3 += ((Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + __instance.smoothedSpeed * 0.15f)) * __instance.swimCameraAnimation;
                }
                if (__instance.impactForce > 0.0)
                {
                    __instance.impactBob = Mathf.Min(0.9f, __instance.impactBob + __instance.impactForce * deltaTime);
                    __instance.impactForce -= (Mathf.Max(1f, __instance.impactForce) * deltaTime * 5f);
                }
                float y = num3 - __instance.impactBob - __instance.stepAmount;
                if (__instance.impactBob > 0.0)
                    __instance.impactBob = Mathf.Max(0f, __instance.impactBob - (Mathf.Pow(__instance.impactBob, 0f) * Time.deltaTime * 3f));
                __instance.stepAmount = Mathf.Lerp(__instance.stepAmount, 0f, deltaTime * Mathf.Abs(__instance.stepAmount));
                __instance.transform.localPosition = new Vector3(0f, y, 0f);
                __instance.transform.localEulerAngles = vector3_1;
                if (Player.main.motorMode == Player.MotorMode.Vehicle)
                    __instance.transform.localEulerAngles = Vector3.zero;
                Vector3 vector3_2 = new Vector3(0.0f, __instance.transform.localEulerAngles.y, 0.0f);
                Vector3 vector3_3 = __instance.transform.localPosition;
                if (UnityEngine.XR.XRSettings.enabled)
                {
                    vector3_2.y = !fixedCam || flag3 ? 0f : __instance.viewModelLockedYaw;
                    if (!flag3 && !__instance.cinematicMode)
                    {
                        if (!fixedCam)
                        {
                            Quaternion rotation = __instance.playerController.forwardReference.rotation;
                            Quaternion quaternion = __instance.gameObject.transform.parent.rotation.GetInverse() * rotation;
                            vector3_2.y = quaternion.eulerAngles.y;
                        }
                        vector3_3 = __instance.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(__instance.playerController.forwardReference.position);
                    }
                }
                __instance.viewModel.transform.localEulerAngles = vector3_2;
                __instance.viewModel.transform.localPosition = vector3_3;
                return false;
            }

        }

        /*
        static void showPDA(PDA pda)
        {// FOV 60 .165f
            //AddDebug("showPDA " + MainCamera.camera.fieldOfView + "  " + MiscSettings.fieldOfView);
            pda.transform.SetParent(Camera.main.transform);
            pda.transform.forward = Camera.main.transform.forward;
            pda.transform.Rotate(new Vector3(0f, 180f, 0f));
            Vector3 pos = Camera.main.transform.position;
            float f = Mathf.InverseLerp(40f, 90f, MiscSettings.fieldOfView);
             f = 1f - f;
            f = Mathf.Lerp(.095f, .26f, f);
            //float mult = Main.NormalizeToRange(MiscSettings.fieldOfView, 40f, 90f, .095f, .26f);
            pda.transform.position = pos + f * Camera.main.transform.forward;
            pda.transform.position = pda.transform.position - .2f * Camera.main.transform.right;
        }

        [HarmonyPatch(typeof(MainCameraControl), "Update")]
        class MainCameraControl_Update_Patch
        {
            static bool Prefix(MainCameraControl __instance)
            {
                {
                    if (!Main.config.instantPDA)
                        return true;

                    __instance.swimCameraAnimation = !__instance.underWaterTracker.isUnderWater ? Mathf.Clamp01(__instance.swimCameraAnimation - Time.deltaTime) : Mathf.Clamp01(__instance.swimCameraAnimation + Time.deltaTime);
                    //double minimumY = __instance.minimumY;
                    //double maximumY = __instance.maximumY;
                    Vector3 velocity = __instance.playerController.velocity;
                    bool pdaOpen = false;
                    bool wasInLockedMode = false;
                    bool inVehicle = false;
                    bool inExosuit = Player.main.inExosuit;
                    bool builderMenuOpen = uGUI_BuilderMenu.IsOpen();

                    if (Player.main != null)
                    {
                        pdaOpen = Player.main.GetPDA().isInUse;
                        inVehicle = Player.main.motorMode == Player.MotorMode.Vehicle;
                        wasInLockedMode = pdaOpen | inVehicle || __instance.cinematicMode;
                        if (UnityEngine.XR.XRSettings.enabled && VROptions.gazeBasedCursor)
                            wasInLockedMode |= builderMenuOpen;
                    }
                    if (wasInLockedMode != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
                    {
                        __instance.camRotationX = 0f;
                        __instance.camRotationY = 0f;
                        __instance.wasInLockedMode = wasInLockedMode;
                        __instance.wasInLookAroundMode = __instance.lookAroundMode;
                    }
                    bool flag5 = (!__instance.cinematicMode || __instance.lookAroundMode && !pdaOpen) && __instance.mouseLookEnabled && (inVehicle || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
                    if (inVehicle && !UnityEngine.XR.XRSettings.enabled && !inExosuit)
                        flag5 = false;
                    Transform transform = __instance.transform;
                    float num1 = pdaOpen || __instance.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting ? 1f : -1f;
                    if (!wasInLockedMode || __instance.cinematicMode && !__instance.lookAroundMode)
                    {
                        __instance.cameraOffsetTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraOffsetTransform.localEulerAngles, Vector3.zero, Time.deltaTime * 5f);
                    }
                    //else
                    else if (!Main.config.instantPDA)
                    {
                        //AddDebug("MainCameraControl Update 11");
                        transform = __instance.cameraOffsetTransform;
                        __instance.rotationY = Mathf.LerpAngle(__instance.rotationY, 0f, Time.deltaTime * 10f);
                        __instance.transform.localEulerAngles = new Vector3(-__instance.rotationY, __instance.rotationX, 0f);
                        __instance.cameraUPTransform.localEulerAngles = UWE.Utils.LerpEuler(__instance.cameraUPTransform.localEulerAngles, Vector3.zero, Time.deltaTime * 30f);
                    }
                    //else if (Main.config.instantPDA)
                    {

                    }
                    if (!UnityEngine.XR.XRSettings.enabled)
                    {
                        Vector3 localPosition = __instance.cameraOffsetTransform.localPosition;
                        localPosition.z = Mathf.Clamp(localPosition.z + (Time.deltaTime * num1 * 0.25f), 0f + __instance.camPDAZStart, __instance.camPDAZOffset + __instance.camPDAZStart);
                        __instance.cameraOffsetTransform.localPosition = localPosition;
                    }
                    Vector2 vector2 = Vector2.zero;
                    if (flag5 && FPSInputModule.current.lastGroup == null)
                    {
                        vector2 = GameInput.GetLookDelta();
                        if (UnityEngine.XR.XRSettings.enabled && VROptions.disableInputPitch)
                            vector2.y = 0f;
                        if (inExosuit)
                            vector2.x = 0f;
                        vector2 *= Player.main.mesmerizedSpeedMultiplier;
                    }
                    __instance.UpdateCamShake();
                    if (__instance.cinematicMode && !__instance.lookAroundMode)
                    {
                        __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0f, Time.deltaTime * 2f);
                        __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0f, Time.deltaTime * 2f);
                        __instance.transform.localEulerAngles = new Vector3(-__instance.camRotationY + __instance.camShake, __instance.camRotationX, 0f);
                    }
                    else if (wasInLockedMode)
                    {
                        if (!UnityEngine.XR.XRSettings.enabled)
                        {
                            bool flag6 = !__instance.lookAroundMode | pdaOpen;
                            int num2 = !__instance.lookAroundMode | pdaOpen ? 1 : 0;
                            Vehicle vehicle = Player.main.GetVehicle();
                            if (vehicle != null)
                                flag6 = vehicle.controlSheme != Vehicle.ControlSheme.Mech | pdaOpen;
                            __instance.camRotationX += vector2.x;
                            __instance.camRotationY += vector2.y;
                            __instance.camRotationX = Mathf.Clamp(__instance.camRotationX, -60f, 60f);
                            __instance.camRotationY = Mathf.Clamp(__instance.camRotationY, -60f, 60f);
                            if (num2 != 0)
                                __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0.0f, Time.deltaTime * 10f);
                            if (flag6)
                                __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0.0f, Time.deltaTime * 10f);
                            __instance.cameraOffsetTransform.localEulerAngles = new Vector3(-__instance.camRotationY, __instance.camRotationX + __instance.camShake, 0.0f);
                        }
                    }
                    else
                    {
                        __instance.rotationX += vector2.x;
                        __instance.rotationY += vector2.y;
                        __instance.rotationY = Mathf.Clamp(__instance.rotationY, __instance.minimumY, __instance.maximumY);
                        __instance.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0f, -__instance.rotationY + __instance.camShake), 0f, 0f);
                        transform.localEulerAngles = new Vector3(Mathf.Max(0f, -__instance.rotationY + __instance.camShake), __instance.rotationX, 0f);
                    }
                    __instance.UpdateStrafeTilt();
                    Vector3 vector3_1 = __instance.transform.localEulerAngles + new Vector3(0f, 0f, __instance.cameraAngleMotion.y * __instance.cameraTiltMod + __instance.strafeTilt + __instance.camShake * 0.5f);
                    float num3 = 0f - __instance.skin;
                    if (!wasInLockedMode && __instance.GetCameraBob())
                    {
                        __instance.smoothedSpeed = UWE.Utils.Slerp(__instance.smoothedSpeed, Mathf.Min(1f, velocity.magnitude / 5f), Time.deltaTime);
                        num3 += ((Mathf.Sin(Time.time * 6f) - 1f) * (0.0199999995529652f + __instance.smoothedSpeed * 0.150000005960464f)) * __instance.swimCameraAnimation;
                    }
                    if (__instance.impactForce > 0f)
                    {
                        __instance.impactBob = Mathf.Min(0.9f, __instance.impactBob + __instance.impactForce * Time.deltaTime);
                        __instance.impactForce -= Mathf.Max(1f, __instance.impactForce) * Time.deltaTime * 5f;
                    }

                    float y = num3 - __instance.impactBob;
                    if (__instance.impactBob > 0f)
                        __instance.impactBob = Mathf.Max(0f, __instance.impactBob - Mathf.Pow(__instance.impactBob, 0.5f) * Time.deltaTime * 3f);
                    __instance.transform.localPosition = new Vector3(0f, y, 0f);
                    __instance.transform.localEulerAngles = vector3_1;
                    if (Player.main.motorMode == Player.MotorMode.Vehicle)
                        __instance.transform.localEulerAngles = Vector3.zero;
                    Vector3 vector3_2 = new Vector3(0f, __instance.transform.localEulerAngles.y, 0f);
                    Vector3 vector3_3 = __instance.transform.localPosition;
                    if (UnityEngine.XR.XRSettings.enabled)
                    {
                        vector3_2.y = !wasInLockedMode || inVehicle ? 0f : __instance.viewModelLockedYaw;
                        if (!inVehicle && !__instance.cinematicMode)
                        {
                            if (!wasInLockedMode)
                            {
                                Quaternion rotation = __instance.playerController.forwardReference.rotation;
                                Quaternion quaternion = __instance.gameObject.transform.parent.rotation.GetInverse() * rotation;
                                vector3_2.y = quaternion.eulerAngles.y;
                            }
                            vector3_3 = __instance.gameObject.transform.parent.worldToLocalMatrix.MultiplyPoint(__instance.playerController.forwardReference.position);
                        }
                    }
                    __instance.viewModel.transform.localEulerAngles = vector3_2;
                    __instance.viewModel.transform.localPosition = vector3_3;
                }
                return false;
            }

        }

        [HarmonyPatch(typeof(PDA))]
        class PDA_Open_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Open")]
            static bool OpenPrefix(PDA __instance, PDATab tab, Transform target, PDA.OnClose onCloseCallback, float activeDistance, bool __result)
            {
                if (__instance.isInUse || __instance.ignorePDAInput)
                {
                    __result = false;
                    return false;
                }
                if (!Main.config.instantPDA)
                    return true;

                uGUI.main.quickSlots.SetTarget(null);
                __instance.prevQuickSlot = Inventory.main.quickSlots.activeSlot;
                Player player = Player.main;

                if (!Inventory.main.ReturnHeld() || player.cinematicModeActive)
                {
                    __result = false;
                    return false;
                }
                MainCameraControl.main.SaveLockedVRViewModelAngle();
                __instance.isInUse = true;
                __instance.gameObject.SetActive(true);
                if (__instance.shouldPlayIntro)
                {
                    //AddDebug("shouldPlayIntro");
                    __instance.shouldPlayIntro = false;
                    __instance.ui.PlayIntro();
                }
                else if (conditionRules)
                {
                    //AddDebug("conditionRules Remove");
                    conditionRules.rules.RemoveAt(ruleToRemove);
                    conditionRules = null;
                }
                uGUI_PopupNotification popupNotification = uGUI_PopupNotification.main;
                if (tab == PDATab.None && popupNotification.isShowingMessage)
                    tab = popupNotification.tabId;
                if (tab == PDATab.TimeCapsule)
                {
                    __instance.ui.SetTabs(null);
                    Inventory.main.SetUsedStorage(PlayerTimeCapsule.main.container);
                    (__instance.ui.GetTab(PDATab.Gallery) as uGUI_GalleryTab).SetSelectListener(new uGUI_GalleryTab.ImageSelectListener((__instance.ui.GetTab(PDATab.TimeCapsule) as uGUI_TimeCapsuleTab).SelectImage), "ScreenshotSelect", "ScreenshotSelectTooltip");
                }
                __instance.ui.OnOpenPDA(tab);
                __instance.sequence.Set(0.5f, true, new SequenceCallback(__instance.Activated));
                GoalManager.main.OnCustomGoalEvent("Open_PDA");
                UWE.Utils.lockCursor = false;
                if (HandReticle.main != null)
                    HandReticle.main.RequestCrosshairHide();
                Inventory.main.SetViewModelVis(false);
                __instance.screen.SetActive(true);
                __instance.targetWasSet = target != null;
                __instance.target = target;
                __instance.onClose = onCloseCallback;
                if (activeDistance < 0f)
                    activeDistance = 3f;
                __instance.activeSqrDistance = activeDistance * activeDistance;
                //UwePostProcessingManager.OpenPDA();
                if (Main.config.instantPDA)
                    showPDA(__instance);
                __result = true;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Close")]
            static bool ClosePrefix(PDA __instance)
            {
                if (!__instance.isInUse || __instance.ignorePDAInput)
                    return false;
                if (!Main.config.instantPDA)
                    return true;

                //AddDebug("Close");
                MainCameraControl.main.ResetLockedVRViewModelAngle();
                Vehicle vehicle = Player.main.GetVehicle();
                if (vehicle != null)
                    uGUI.main.quickSlots.SetTarget((IQuickSlots)vehicle);
                __instance.targetWasSet = false;
                __instance.target = null;
                __instance.isInUse = false;
                __instance.ui.OnClosePDA();
                MainGameController.Instance.PerformGarbageCollection();
                //if (HandReticle.main != null)
                HandReticle.main?.UnrequestCrosshairHide();
                Inventory.main.SetViewModelVis(true);
                __instance.sequence.Set(0.5f, false, new SequenceCallback(__instance.Deactivated));
                __instance.screen.SetActive(false);
                //UwePostProcessingManager.ClosePDA();
                __instance.gameObject.SetActive(false);
                Inventory.main.quickSlots.Select(__instance.prevQuickSlot);
                if (__instance.onClose == null)
                    return false;
                PDA.OnClose onClose = __instance.onClose;
                __instance.onClose = null;
                onClose(__instance);       
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool UpdatePrefix(PDA __instance)
            {
                if (!Main.config.instantPDA)
                    return true;
                __instance.sequence.Update();
                //AddDebug("ignorePDAInput " + __instance.ignorePDAInput);
                //if (__instance.sequence.active)
                //{
                //float b = SNCameraRoot.main.mainCamera.aspect > 1.5 ? __instance.cameraFieldOfView : __instance.cameraFieldOfViewAtFourThree;
                //SNCameraRoot.main.SetFov(Mathf.Lerp(MiscSettings.fieldOfView, b, __instance.sequence.t));
                //}
                Player main = Player.main;
                if (__instance.isInUse && __instance.isFocused && (GameInput.GetButtonDown(GameInput.Button.PDA) && !__instance.ui.introActive))
                {
                    __instance.Close();
                }
                else
                {
                    if (!__instance.targetWasSet || !(__instance.target == null) && (__instance.target.transform.position - main.transform.position).sqrMagnitude < __instance.activeSqrDistance)
                        return false;
                    __instance.Close();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(ArmsController))]
        class ArmsController_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("InstallAnimationRules")]
            static bool InstallAnimationRulesPrefix(ArmsController __instance)
            {
                ConditionRules cr = __instance.GetComponent<ConditionRules>();
                cr.AddCondition((() => __instance.player.GetPDA().isInUse)).WhenChanges((newValue => SafeAnimator.SetBool(__instance.animator, "using_pda", newValue)));
                conditionRules = cr;
                ruleToRemove = cr.rules.Count - 1;
                cr.AddCondition((ConditionRules.ConditionFunction)(() => (Inventory.main.GetHeldTool() as Welder) != null)).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "holding_welder", newValue)));
                cr.AddCondition((ConditionRules.ConditionFunction)(() =>
                {
                    float y = __instance.player.gameObject.transform.position.y;
                    return y > Ocean.main.GetOceanLevel() - 1f && y < Ocean.main.GetOceanLevel() + 1f && !Player.main.IsInside() && !Player.main.precursorOutOfWater;
                })).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "on_surface", newValue)));
                cr.AddCondition((ConditionRules.ConditionFunction)(() => __instance.player.GetInMechMode())).WhenChanges((ConditionRules.BoolHandlerFunction)(newValue => SafeAnimator.SetBool(__instance.animator, "using_mechsuit", newValue)));
                return false;
            }
        }

        //[HarmonyPatch(typeof(UwePostProcessingManager), "OpenPDA")]
        class UwePostProcessingManager_OpenPDA_Patch
        {
            static bool Prefix(UwePostProcessingManager __instance)
            {
                AddDebug("UwePostProcessingManager OpenPDA");
                return false;
            }
        }

        //[HarmonyPatch(typeof(UwePostProcessingManager), "ClosePDA")]
        class UwePostProcessingManager_ClosePDA_Patch
        {
            static bool Prefix(UwePostProcessingManager __instance)
            {
                AddDebug("UwePostProcessingManager ClosePDA");
                return false;
            }
        }

        //[HarmonyPatch(typeof(SNCameraRoot), "SetFov")]
        class CameraToPlayerManager_Update_Patch
        {
            static bool Prefix(SNCameraRoot __instance, float fov)
            {
                AddDebug("SNCameraRoot SetFov fov " + fov);
                //AddDebug("SNCameraRoot SetFov isInUse " + Main.pda.isInUse);
                return false;
            }
        }

        //[HarmonyPatch(typeof(OVRDebugInfo), "UpdateFOV")]
        class CameraToPlayerManager_SyncFieldOfView_Patch
        {
            static bool Prefix(OVRDebugInfo __instance)
            {
                AddDebug("OVRDebugInfo UpdateFOV ");
                //AddDebug("SNCameraRoot SetFov isInUse " + Main.pda.isInUse);
                return false;
            }
        }

        /*/
    }
}
