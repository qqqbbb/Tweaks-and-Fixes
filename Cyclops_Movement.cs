using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UWE;
using static ErrorMessage;
using static GameInput;

namespace Tweaks_Fixes
{
    internal class Cyclops_Movement
    {
        static float cyclopsVerticalMod = 1;
        static float cyclopsBackwardMod = 1;
        static float cyclopsForwardOrig;
        static bool autoMove = false;
        static int collisionLayerMask = Voxeland.GetTerrainLayerMask();

        [HarmonyPatch(typeof(GameInput))]
        internal class GameInput_
        {
            [HarmonyPatch("UpdateMove"), HarmonyPrefix]
            static bool UpdateMovePrefix()
            {
                Vector2 moveVector2 = GetVector2(Button.Move);
                float y = 0f;
                y += (GetButtonHeld(Button.MoveUp) ? 1f : 0f);
                y -= (GetButtonHeld(Button.MoveDown) ? 1f : 0f);
                if (GameInput.autoMove && moveVector2.y != 0)
                {
                    GameInput.autoMove = false;
                }
                if (GameInput.autoMove)
                    moveDirection.Set(moveVector2.x, y, 1f);
                else
                    moveDirection.Set(moveVector2.x, y, moveVector2.y);

                if (IsPrimaryDeviceGamepad())
                {
                    if (GameInput.autoMove)
                        isRunningMoveThreshold = false;
                    else
                    {
                        isRunningMoveThreshold = moveDirection.sqrMagnitude > 0.8f;
                        if (!isRunningMoveThreshold)
                            moveDirection /= 0.9f;
                    }
                }
                if (runMode == RunModeOption.PressToToggle && GetButtonDown(Button.Sprint))
                    isRunning = !isRunning;

                return false;
            }
            //[HarmonyPatch("UpdateMove"), HarmonyTranspiler]
            static IEnumerable<CodeInstruction> UpdateMoveTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions);

                // Try to find stsfld for autoMove regardless of what's being stored
                codeMatcher.MatchForward(false,
                    new CodeMatch(OpCodes.Stsfld, AccessTools.Field(typeof(GameInput), "autoMove"))
                )
                .ThrowIfInvalid("Could not find autoMove field store in UpdateMove");
                // Go back one instruction to find what's being stored
                codeMatcher.Advance(-1);
                // Replace the loading instruction (ldc.i4.0 or whatever) with your delegate
                codeMatcher.SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<bool>>(GetAutoMove));

                return codeMatcher.InstructionEnumeration();
            }

            static bool GetAutoMove()
            {
                //AddDebug("GetAutoMove");
                return true;
            }
        }


        static IEnumerator DetectCollision(CyclopsProximitySensors sensors)
        { // detects only terrain
            while (autoMove)
            {
                yield return new WaitForSeconds(.1f);
                //AddDebug("my DetectCollision");
                var data = sensors.sensorCastData[0];
                float distance = data.distance * 3;
                float radius = data.radius * 2;
                var sensor = sensors.sensor[0];
                Vector3 position = sensor.transform.position + new Vector3(0, 3, 0);
                Vector3 dir = sensor.transform.forward;
                if (Physics.SphereCast(position, radius, dir, out var hitInfo, distance, collisionLayerMask))
                {
                    //AddDebug("my DetectCollision hit " + hitInfo.transform.name);
                    autoMove = false;
                    yield break;
                }
            }
        }

        //[HarmonyPatch(typeof(CyclopsProximitySensors), "DetectCollision")]
        class CyclopsProximitySensors_DetectCollision_Patch
        {
            public static void Postfix(CyclopsProximitySensors __instance)
            {
                AddDebug("DetectCollision");
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
                if (ConfigToEdit.cyclopsBackwardSpeedMod.Value > 0 && cyclopsBackwardMod == 1)
                    cyclopsBackwardMod = 1 - Mathf.Clamp(ConfigToEdit.cyclopsBackwardSpeedMod.Value, 1, 100) * .01f;

                if (ConfigToEdit.cyclopsVerticalSpeedMod.Value > 0 && cyclopsVerticalMod == 1)
                    cyclopsVerticalMod = 1 - Mathf.Clamp(ConfigToEdit.cyclopsVerticalSpeedMod.Value, 1, 100) * .01f;
            }

            [HarmonyPostfix, HarmonyPatch("Set")]
            public static void SetPostfix(SubControl __instance, Player.Mode newMode)
            {
                //AddDebug("Set mode " + newMode);
                if (newMode == Player.Mode.LockedPiloting)
                {
                    if (autoMove)
                    {
                        GameInput.autoMove = true;
                        autoMove = false;
                    }
                }
                else if (newMode == Player.Mode.Piloting)
                {
                    if (GameInput.autoMove)
                    {
                        autoMove = true;
                        StartCollisionDetection(__instance);
                    }
                }
            }

            private static void StartCollisionDetection(SubControl __instance)
            {
                //if (ConfigToEdit.disableCyclopsProximitySensor.Value == false)
                {
                    Transform t = __instance.transform.Find("ProximityWarning");
                    CyclopsProximitySensors sensors = t.GetComponent<CyclopsProximitySensors>();
                    //sensors.InvokeRepeating("DetectCollision", sensors.sensorTime, sensors.sensorTime);
                    CoroutineHost.StartCoroutine(DetectCollision(sensors));
                }
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(SubControl __instance)
            {
                if (__instance.sub != Player.main.currentSub)
                    return;

                //if (Keyboard.current.zKey.wasPressedThisFrame)
                {
                    //StartCollisionDetection(__instance);
                    //ShowSensors(__instance);
                }
                //AddDebug($"GameInput.autoMove {GameInput.autoMove} autoMove {autoMove}");
                if (__instance.controlMode != SubControl.Mode.GameObjects || GameModeUtils.RequiresPower() && __instance.powerRelay.isPowered == false)
                    return;

                //if (GameInput.GetButtonDown(GameInput.Button.AutoMove))
                //{
                //    GameInput.autoMove = false;
                //    autoMove = !autoMove;
                //    if (autoMove)
                //        AddDebug("AutoMove On ");
                //    else
                //        AddDebug("AutoMove Off ");
                //}
                if (autoMove)
                {
                    MoveCyclopsForward(__instance);
                }
            }

            private static void MoveCyclopsForward(SubControl __instance)
            {
                //AddDebug("MoveCyclopsForward useThrottleIndex " + __instance.useThrottleIndex);
                if (__instance.canAccel == false)
                {
                    autoMove = false;
                    return;
                }
                __instance.throttle = Vector3.forward;
                float amountConsumed = 0f;
                float amount = __instance.throttle.magnitude * __instance.cyclopsMotorMode.GetPowerConsumption() * Time.deltaTime / __instance.sub.GetPowerRating();
                if (!GameModeUtils.RequiresPower() || __instance.powerRelay.ConsumeEnergy(amount, out amountConsumed))
                    __instance.lastTimeThrottled = Time.time;

                float topClamp = 0.33f;
                if (__instance.useThrottleIndex == 1)
                    topClamp = 0.66f;
                else if (__instance.useThrottleIndex == 2)
                    topClamp = 1f;

                __instance.engineRPMManager.AccelerateInput(topClamp);
                for (int i = 0; i < __instance.throttleHandlers.Length; i++)
                    __instance.throttleHandlers[i].OnSubAppliedThrottle();

                if (__instance.lastTimeThrottled < Time.time - 5f)
                    Utils.PlayFMODAsset(__instance.engineStartSound, MainCamera.camera.transform);
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
            [HarmonyPostfix, HarmonyPatch("OnPlayerExited")]
            public static void OnPlayerExitedPostfix(SubRoot __instance)
            {
                //AddDebug("OnPlayerExited ");
                autoMove = false;
            }
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

        static void ShowSensors(CyclopsProximitySensors sensors)
        {
            for (int i = 0; i < CyclopsProximitySensors.totalSensors; i++)
            {
                float distance = sensors.sensorCastData[i].distance;
                float radius = sensors.sensorCastData[i].radius;
                Vector3 position = sensors.sensor[i].transform.position;
                Vector3 forward = sensors.sensor[i].transform.forward;
                SphereCollider collider = sensors.sensor[i].gameObject.AddComponent<SphereCollider>();
                collider.radius = radius;
                collider.center = Vector3.forward * distance;
                Testing.ShowDebugCollider(collider);
            }
        }

        static void ShowSensors(SubControl subControl)
        {
            AddDebug("ShowSensors");
            Transform t = subControl.transform.Find("ProximityWarning");
            CyclopsProximitySensors sensors = t.GetComponent<CyclopsProximitySensors>();
            //var data = sensors.sensorCastData[3];

            //for (int i = 0; i < CyclopsProximitySensors.totalSensors; i++)
            {
                int i = 0;
                float distance = sensors.sensorCastData[i].distance;
                float radius = sensors.sensorCastData[i].radius;
                Vector3 position = sensors.sensor[i].transform.position + new Vector3(0, 3, 0);
                Vector3 forward = sensors.sensor[i].transform.forward;
                SphereCollider collider = sensors.sensor[i].gameObject.AddComponent<SphereCollider>();
                collider.isTrigger = true;
                collider.center = Vector3.forward * distance * 3;
                collider.radius = radius * 2f;
                Testing.ShowDebugCollider(collider);
                AddDebug("ShowSensors ShowDebugCollider " + sensors.sensor[i].transform.name);
            }
        }


    }
}
