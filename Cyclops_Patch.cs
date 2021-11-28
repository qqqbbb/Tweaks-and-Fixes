using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ErrorMessage;

namespace Tweaks_Fixes
{  
    public class Cyclops_Patch
    {
        static Rigidbody cyclopsRB;
        public static CyclopsEntryHatch ceh;
        //public static CyclopsHelmHUDManager cyclopsHelmHUDManager;
        public static HashSet<Collider> collidersInSub = new HashSet<Collider>();

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

        static void AddCyclopsCollisionExclusion(GameObject go)
        {
            //AddDebug(__instance.name + " in cyclops");
            Collider[] myCols = go.GetAllComponentsInChildren<Collider>();
            foreach (Collider c in collidersInSub)
            {
                if (c == null)
                    continue;
                foreach (Collider myCol in myCols)
                {
                    Physics.IgnoreCollision(myCol, c);
                }
            }
            foreach (Collider c in myCols)
            {
                collidersInSub.Add(c);
                //AddDebug("add collider to collidersInSub");
            }
        }

        [HarmonyPatch(typeof(SubRoot), "ForceLightingState")]
        class SubRoot_ForceLightingState_Patch
        {
            public static void Postfix(SubRoot __instance, bool lightingOn)
            {
                __instance.interiorSky.affectedByDayNightCycle = Main.config.cyclopsSunlight && !lightingOn;
                //AddDebug("affectedByDayNightCycle " + __instance.interiorSky.affectedByDayNightCycle);
            }
        }

        [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
        class CyclopsHelmHUDManager_Patch
        {
            [HarmonyPatch(nameof(CyclopsHelmHUDManager.Update))]
            [HarmonyPostfix]
            public static void UpdatePostfix(CyclopsHelmHUDManager __instance)
            {
                //if (Player.main.currentSub && Player.main.currentSub == __instance.subRoot)
                {
                    __instance.hudActive = Player.main.currentSub && Player.main.currentSub == __instance.subRoot;
                    if (__instance.motorMode.engineOn) // hide speed selector when engine off
                        __instance.engineToggleAnimator.SetTrigger("EngineOn");
                    else
                        __instance.engineToggleAnimator.SetTrigger("EngineOff");
                    //cyclopsHelmHUDManager = __instance;
                    //AddDebug("hudActive " + __instance.hudActive);
                    //__instance.canvasGroup.alpha = 0f;
                }
            }
            [HarmonyPatch(nameof(CyclopsHelmHUDManager.StartPiloting))]
            [HarmonyPrefix]
            public static void StartPilotingPrefix(CyclopsHelmHUDManager __instance)
            {
                {
                    cyclopsRB = __instance.transform.parent.GetComponent<Rigidbody>();
                    Vehicle_patch.currentVehicleTT = TechType.Cyclops;
                    Vehicle_patch.currentLights = __instance.transform.parent.Find("Floodlights").GetComponentsInChildren<Light>(true);
                    //AddDebug("StartPiloting  " + rb.mass);
                    //AddDebug(" " + __instance.transform.parent.name);
                    //__instance.canvasGroup.alpha = 0f;
                }
            }
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
                    //AddDebug("save subThrottleIndex");
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
                    //AddDebug("restore  subThrottleIndex");
                    SetCyclopsMotorMode(__instance, (CyclopsMotorMode.CyclopsMotorModes)Main.config.subThrottleIndex);
                }
            }
        }

        [HarmonyPatch(typeof(SubControl))]
        class SubControl_Patch
        {
            [HarmonyPatch(nameof(SubControl.Start))]
            [HarmonyPostfix]
            public static void StartPostfix(SubControl __instance)
            {
                //if (Main.config.vehicleMoveTweaks) 
                //{ 
                //__instance.BaseVerticalAccel = __instance.BaseForwardAccel * .5f;
                //}
                TechTag techTag = __instance.gameObject.EnsureComponent<TechTag>();
                techTag.type = TechType.Cyclops;

                Transform tr = __instance.transform.Find("Headlights");
                if (tr)
                    UnityEngine.Object.Destroy(tr.gameObject);

                Tools_Patch.lightOrigIntensity[TechType.Cyclops] = 2f;
                Tools_Patch.lightIntensityStep[TechType.Cyclops] = .2f;
                Light[] lights = __instance.transform.Find("Floodlights").GetComponentsInChildren<Light>(true);
                //AddDebug("SubControl.Start lights intensity " + lights[0].intensity);
                if (Main.config.lightIntensity.ContainsKey(TechType.Cyclops))
                {
                    foreach (Light l in lights)
                        l.intensity = Main.config.lightIntensity[TechType.Cyclops];
                }

            }
            [HarmonyPatch(nameof(SubControl.Update))]
            [HarmonyPrefix]
            public static bool UpdatePrefix(SubControl __instance)
            { // fix max diagonal speed 
                if (!__instance.LOD.IsFull())
                    return false;

                if (!Main.config.cyclopsMoveTweaks || Main.cyclopsDockingLoaded)
                    return true;

                __instance.appliedThrottle = false;
                if (__instance.controlMode == SubControl.Mode.DirectInput)
                {
                    __instance.throttle = GameInput.GetMoveDirection();
                    __instance.throttle.Normalize();
                    //AddDebug("throttle " + __instance.throttle);
                    //AddDebug(".magnitude " + __instance.throttle.magnitude);
                    if (__instance.canAccel && __instance.throttle.magnitude > 0.0001)
                    {
                        float amountConsumed = 0f;
                        float amount = __instance.throttle.magnitude * __instance.cyclopsMotorMode.GetPowerConsumption() * Time.deltaTime / __instance.sub.GetPowerRating();
                        //cyclopsPowerCons = true;
                        if (!GameModeUtils.RequiresPower() || __instance.powerRelay.ConsumeEnergy(amount, out amountConsumed))
                        {
                            __instance.lastTimeThrottled = Time.time;
                            __instance.appliedThrottle = true;
                        }
                    }
                    if (__instance.appliedThrottle && __instance.canAccel)
                    {
                        //AddDebug("throttleHandlers.Length " + __instance.throttleHandlers.Length);
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

            [HarmonyPatch(nameof(SubControl.FixedUpdate))]
            [HarmonyPrefix]
            public static bool FixedUpdatePrefix(SubControl __instance)
            {  // halve vertical and backward speed
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
                        cyclopsRB.AddTorque(__instance.sub.subAxis.up * baseTurningTorque * __instance.turnScale * __instance.throttle.x, ForceMode.Acceleration);

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
                    //AddDebug("BaseVerticalAccel  " + __instance.BaseVerticalAccel);
                    //AddDebug("accelScale  " + __instance.accelScale);
                    b2 = __instance.throttle.y <= 0f ? -90f : 90f;
                    float num = __instance.BaseVerticalAccel * .5f + __instance.gameObject.GetComponentsInChildren<BallastWeight>().Length * __instance.AccelPerBallast;
                    Vector3 accel = Vector3.up * num * __instance.accelScale * __instance.throttle.y;
                    //AddDebug("accel  " + accel);
                    if (__instance.canAccel)
                        cyclopsRB.AddForce(accel, ForceMode.Acceleration);
                }
                if (__instance.canAccel)
                {
                    if (__instance.throttle.z > 0.0001f)
                    {
                        cyclopsRB.AddForce(__instance.sub.subAxis.forward * __instance.BaseForwardAccel * __instance.accelScale * __instance.throttle.z, ForceMode.Acceleration);
                    }
                    else if (__instance.throttle.z < -0.0001f)
                    {
                        cyclopsRB.AddForce(__instance.sub.subAxis.forward * __instance.BaseForwardAccel * .5f * __instance.accelScale * __instance.throttle.z, ForceMode.Acceleration);
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

        [HarmonyPatch(typeof(CyclopsEntryHatch), "OnTriggerEnter")]
        class CyclopsEntryHatch_Start_Patch
        { // OnTriggerExit does not fire if you use closest ladder so hatch does not close
            static void Postfix(CyclopsEntryHatch __instance, Collider col)
            {
                if (!col.gameObject.Equals(Player.main.gameObject))
                    return;
                ceh = __instance;
                //AddDebug("OnTriggerEnter " + __instance.hatchOpen);
                //cyclopsHelmHUDManager.hudActive = true;
            }
        }

        [HarmonyPatch(typeof(CinematicModeTriggerBase), "OnHandClick")]
        class CinematicModeTriggerBase_OnHandClick_Patch
        {
            static void Postfix(CinematicModeTriggerBase __instance, GUIHand hand)
            {
                if (ceh && ceh.hatchOpen && Player.main.IsInSubmarine())
                {
                    CinematicModeTrigger cmt = __instance as CinematicModeTrigger;
                    if (cmt && cmt.handText == "ClimbLadder")
                    {
                        //AddDebug("CLOSE !!! " );
                        ceh.hatchOpen = false;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsSilentRunningAbilityButton), "SilentRunningIteration")]
        class CyclopsSilentRunningAbilityButton_SilentRunningIteration_Patch
        {
            public static bool Prefix(CyclopsSilentRunningAbilityButton __instance)
            {
                float amountConsumed = 0f;
                // dont consume power when engine is off
                if (Player.main.currentSub && Player.main.currentSub.noiseManager &&  Player.main.currentSub.noiseManager.noiseScalar == 0f)
                    return false;

                if (__instance.subRoot.powerRelay.ConsumeEnergy(__instance.subRoot.silentRunningPowerCost, out amountConsumed))
                    return false;
                __instance.TurnOffSilentRunning();
  
                return false;
            }
        }

        [HarmonyPatch(typeof(Constructable), "Start")]
        class Constructable_Start_Patch
        {
            public static void Postfix(Constructable __instance)
            {
                if (__instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__instance.gameObject);
            }
        }

        [HarmonyPatch(typeof(Plantable), "Spawn")]
        internal class Plantable_Spawn_Patch
        {
            public static void Postfix(Plantable __instance, Transform parent, bool isIndoor, GameObject __result)
            {
                if (__result && __instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__result);
            }
        }

        [HarmonyPatch(typeof(GrownPlant), "Awake")]
        internal class GrownPlant_Awake_Patch
        {
            public static void Postfix(GrownPlant __instance)
            {
                if ( __instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__instance.gameObject);
            }
        }


    }
}
