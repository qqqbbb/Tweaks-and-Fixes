using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Cyclops_Patch
    {
        static Rigidbody rb;

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

        [HarmonyPatch(typeof(SubRoot), "OnProtoSerialize")]
        class SubRoot_OnProtoSerialize_Patch
        {
            public static void Postfix(SubRoot __instance)
            {
                CyclopsMotorMode cyclopsMotorMode = __instance.GetComponent<CyclopsMotorMode>();
                if (cyclopsMotorMode)
                {
                    Main.config.subThrottleIndex = (int)cyclopsMotorMode.cyclopsMotorMode;
                    //ErrorMessage.AddDebug("save subThrottleIndex");
                    Main.config.Save();
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsMotorModeButton), "Start")]
        class CyclopsMotorModeButton_Start_Patch
        {
            public static void Postfix(CyclopsMotorModeButton __instance)
            {
                if (Main.config.subThrottleIndex != -1)
                {
                    //ErrorMessage.AddDebug("restore  subThrottleIndex");
                    SetCyclopsMotorMode(__instance, (CyclopsMotorMode.CyclopsMotorModes)Main.config.subThrottleIndex);
                }
            }
        }

        [HarmonyPatch(typeof(SubControl), "Update")]
        class SubControl_Update_Patch
        { // fix max diagonal speed
            public static bool Prefix(SubControl __instance)
            {
                if (!Main.config.cyclopsMoveTweaks)
                    return true;

                if (!__instance.LOD.IsFull())
                    return false;

                __instance.appliedThrottle = false;
                if (__instance.controlMode == SubControl.Mode.DirectInput)
                {
                    __instance.throttle = GameInput.GetMoveDirection();
                    __instance.throttle.Normalize();
                    //ErrorMessage.AddDebug("throttle " + __instance.throttle);
                    //ErrorMessage.AddDebug(".magnitude " + __instance.throttle.magnitude);
                    if (__instance.canAccel && __instance.throttle.magnitude > 0.0001)
                    {
                        float amountConsumed = 0.0f;
                        float amount = __instance.throttle.magnitude * __instance.cyclopsMotorMode.GetPowerConsumption() * Time.deltaTime / __instance.sub.GetPowerRating();
                        if (!GameModeUtils.RequiresPower() || __instance.powerRelay.ConsumeEnergy(amount, out amountConsumed))
                        {
                            __instance.lastTimeThrottled = Time.time;
                            __instance.appliedThrottle = true;
                        }
                    }
                    if (__instance.appliedThrottle && __instance.canAccel)
                    {
                        //ErrorMessage.AddDebug("throttleHandlers.Length " + __instance.throttleHandlers.Length);
                        float topClamp = 0.33f;
                        if (__instance.useThrottleIndex == 1)
                            topClamp = 0.66f;
                        if (__instance.useThrottleIndex == 2)
                            topClamp = 1f;
                        __instance.engineRPMManager.AccelerateInput(topClamp);
                        for (int index = 0; index < __instance.throttleHandlers.Length; ++index)
                            __instance.throttleHandlers[index].OnSubAppliedThrottle();
                        if (__instance.lastTimeThrottled < Time.time - 5f)
                            Utils.PlayFMODAsset(__instance.engineStartSound, MainCamera.camera.transform);
                    }
                    if (AvatarInputHandler.main.IsEnabled())
                    {
                        if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                            __instance.transform.parent.BroadcastMessage("ToggleFloodlights", (object)null, SendMessageOptions.DontRequireReceiver);
                        if (GameInput.GetButtonDown(GameInput.Button.Exit))
                            Player.main.TryEject();
                    }
                }
                if (__instance.appliedThrottle)
                    return false;

                __instance.throttle = new Vector3(0.0f, 0.0f, 0.0f);

                return false;
                }
            }

        [HarmonyPatch(typeof(SubControl), "Start")]
        class SubControl_Start_Patch
        {
            public static void Postfix(SubControl __instance)
            {
                //if (Main.config.vehicleMoveTweaks) 
                //{ 
                    rb = __instance.GetComponent<Rigidbody>();
                    //__instance.BaseVerticalAccel = __instance.BaseForwardAccel * .5f;
                //}
            }
        }
        
        [HarmonyPatch(typeof(SubControl), "FixedUpdate")]
        class SubControl_FixedUpdate_Patch
        {// halve vertical and backward speed
            public static bool Prefix(SubControl __instance)
            {
            if (!Main.config.cyclopsMoveTweaks)
                return true;

                if (!__instance.LOD.IsFull() || __instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;

                for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                __instance.accelerationModifiers[index].ModifyAcceleration(ref __instance.throttle);
                if (Ocean.main.GetDepthOf(__instance.gameObject) < 0f)
                    return false;

                float b1 = 0.0f;
                float b2 = 0.0f;

                if (Mathf.Abs(__instance.throttle.x) > 0.0001f)
                {
                    float baseTurningTorque = __instance.BaseTurningTorque;
                    if (__instance.canAccel)
                        rb.AddTorque(__instance.sub.subAxis.up * baseTurningTorque * __instance.turnScale * __instance.throttle.x, ForceMode.Acceleration);

                    ShipSide useShipSide;
                    if (__instance.throttle.x > 0.0)
                    {
                        useShipSide = ShipSide.Port;
                        b1 = 90f;
                    }
                    else
                    {
                        useShipSide = ShipSide.Starboard;
                        b1 = -90f;
                    }
                    if (__instance.throttle.x < -0.1f || __instance.throttle.x > 0.1f)
                    {
                        for (int index = 0; index < __instance.turnHandlers.Length; ++index)
                        __instance.turnHandlers[index].OnSubTurn(useShipSide);
                    }
                }
                if (Mathf.Abs(__instance.throttle.y) > 0.0001f)
                {
                    //ErrorMessage.AddDebug("BaseVerticalAccel  " + __instance.BaseVerticalAccel);
                    //ErrorMessage.AddDebug("accelScale  " + __instance.accelScale);
                    b2 = __instance.throttle.y <= 0f ? -90f : 90f;
                    float num = __instance.BaseVerticalAccel * .5f + __instance.gameObject.GetComponentsInChildren<BallastWeight>().Length * __instance.AccelPerBallast;
                    Vector3 accel = Vector3.up * num * __instance.accelScale * __instance.throttle.y;
                    //ErrorMessage.AddDebug("accel  " + accel);
                    if (__instance.canAccel)
                        rb.AddForce(accel, ForceMode.Acceleration);
                }
                if (__instance.canAccel)
                { 
                    if (__instance.throttle.z > 0.0001f)
                    {
                        rb.AddForce(__instance.sub.subAxis.forward * __instance.BaseForwardAccel * __instance.accelScale * __instance.throttle.z, ForceMode.Acceleration);
                    }
                    else if (__instance.throttle.z < -0.0001f)
                    {
                        rb.AddForce(__instance.sub.subAxis.forward * __instance.BaseForwardAccel * .5f * __instance.accelScale * __instance.throttle.z, ForceMode.Acceleration);
                    }
                }
                __instance.steeringWheelYaw = Mathf.Lerp(__instance.steeringWheelYaw, b1, Time.deltaTime * __instance.steeringReponsiveness);
            __instance.steeringWheelPitch = Mathf.Lerp(__instance.steeringWheelPitch, b2, Time.deltaTime * __instance.steeringReponsiveness);
                if (!__instance.mainAnimator)
                    return false;

            __instance.mainAnimator.SetFloat("view_yaw", __instance.steeringWheelYaw);
            __instance.mainAnimator.SetFloat("view_pitch", __instance.steeringWheelPitch);
                Player.main.playerAnimator.SetFloat("cyclops_yaw", __instance.steeringWheelYaw);
                Player.main.playerAnimator.SetFloat("cyclops_pitch", __instance.steeringWheelPitch);

            return false;
            }
        }

        //[HarmonyPatch(typeof(VoiceNotificationManager), "PlayVoiceNotification")]
        class VoiceNotificationManager_PlayVoiceNotification_Prefix_Patch
        {
            public static bool Prefix(VoiceNotificationManager __instance, VoiceNotification vo)
            {
                //ErrorMessage.AddDebug("PlayVoiceNotification Prefix " + vo.GetCanPlay());
                //ErrorMessage.AddDebug("PlayVoiceNotification Prefix " + vo.timeNextPlay);
                ErrorMessage.AddDebug("PlayVoiceNotification Prefix " + vo.minInterval);
                return true;
            }
        }

        //[HarmonyPatch(typeof(VoiceNotificationManager), "PlayVoiceNotification")]
        class VoiceNotificationManager_PlayVoiceNotification_Patch
        {
            public static void Postfix(VoiceNotificationManager __instance, VoiceNotification vo)
            {

                ErrorMessage.AddDebug("PlayVoiceNotification " + vo.GetCanPlay());
            }
        }

    }
}
