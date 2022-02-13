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
        static bool cyclopsHolographicHUDlastState = false;
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

        [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
        class CyclopsHelmHUDManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void UpdatePostfix(CyclopsHelmHUDManager __instance)
            {
                //if (Player.main.currentSub && Player.main.currentSub == __instance.subRoot)
                {
                    bool powerOn = __instance.subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline;
                    __instance.hudActive = powerOn && Player.main.currentSub && Player.main.currentSub == __instance.subRoot;
                    if (__instance.motorMode.engineOn) // hide speed selector when engine off
                        __instance.engineToggleAnimator.SetTrigger("EngineOn");
                    else
                        __instance.engineToggleAnimator.SetTrigger("EngineOff");
                    //cyclopsHelmHUDManager = __instance;
                    //AddDebug("hudActive " + __instance.hudActive);
                    //__instance.canvasGroup.alpha = 0f;
                }
            }
          
            [HarmonyPostfix]
            [HarmonyPatch("StartPiloting")]
            public static void StartPilotingPostfix(CyclopsHelmHUDManager __instance)
            {
                cyclopsRB = __instance.transform.parent.GetComponent<Rigidbody>();
                Vehicle_patch.currentVehicleTT = TechType.Cyclops;
                Vehicle_patch.currentLights = __instance.transform.parent.Find("Floodlights").GetComponentsInChildren<Light>(true);
                //AddDebug("StartPiloting  " + rb.mass);
                //AddDebug(" " + __instance.transform.parent.name);
                //__instance.canvasGroup.alpha = 0f;
            }

            [HarmonyPostfix]
            [HarmonyPatch("StopPiloting")]
            public static void StopPilotingPostfix(CyclopsHelmHUDManager __instance)
            {
                Vehicle_patch.currentLights[0] = null;
                //AddDebug("StopPiloting  ");
            }
        }

        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void StartPostfix(SubRoot __instance)
            {
                Transform upgradeConsole = __instance.transform.Find("submarine_engine_console_01_wide");
                if (upgradeConsole)
                {
                    AddDebug("upgradeConsole");
                    MeshRenderer[] mrs = upgradeConsole.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrs)
                    {
                        foreach (Material m in mr.materials)
                            m.DisableKeyword("MARMO_EMISSION");
                    }
                }
                else
                    AddDebug("no upgradeConsole");
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnProtoSerialize")]
            public static void OnProtoSerializePostfix(SubRoot __instance)
            {
                CyclopsMotorMode cyclopsMotorMode = __instance.GetComponent<CyclopsMotorMode>();
                if (cyclopsMotorMode)
                {
                    Main.config.subThrottleIndex = (int)cyclopsMotorMode.cyclopsMotorMode;
                    //AddDebug("save subThrottleIndex");
                    Main.config.Save();
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("ForceLightingState")]
            public static void ForceLightingStatePostfix(SubRoot __instance, bool lightingOn)
            {
                __instance.interiorSky.affectedByDayNightCycle = Main.config.cyclopsSunlight && !lightingOn;
                //AddDebug("affectedByDayNightCycle " + __instance.interiorSky.affectedByDayNightCycle);
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
                //TechTag techTag = __instance.gameObject.EnsureComponent<TechTag>();
                //techTag.type = TechType.Cyclops;
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
                // dont consume power when engine is off
                if (Player.main.currentSub && Player.main.currentSub.noiseManager && Player.main.currentSub.noiseManager.noiseScalar == 0f)
                    return false;

                if (__instance.subRoot.powerRelay.ConsumeEnergy(__instance.subRoot.silentRunningPowerCost, out float amountConsumed))
                    return false;
                __instance.TurnOffSilentRunning();

                return false;
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        internal class VehicleDockingBay_LaunchbayAreaEnter_Patch
        { // dont play sfx if another vehicle docked
            [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaEnter))]
            [HarmonyPrefix]
            public static bool LaunchbayAreaEnterPrefix(VehicleDockingBay __instance)
            {
                if (__instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;
                return !__instance._dockedVehicle;
            }
            [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaExit))]
            [HarmonyPrefix]
            public static bool LaunchbayAreaExitPrefix(VehicleDockingBay __instance)
            {
                if (__instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;
                return !__instance._dockedVehicle;
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.UpdateDockedPosition))]
            //[HarmonyPrefix]
            public static void UpdateDockedPositionPrefix(VehicleDockingBay __instance, Vehicle vehicle, ref float interpfraction)
            {
                AddDebug("UpdateDockedPosition " + vehicle.transform.position);
                //if (!properlyDocked)
                //interpfraction = 0;
                //AddDebug("interpfraction " + interpfraction);
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.SetVehicleDocked))]
            //[HarmonyPostfix]
            public static void SetVehicleDockedPrefix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //properlyDocked = false;
                //__instance.exosuitDockPlayerCinematic.StartCinematicMode(Player.main);
                //__instance.DockVehicle(vehicle);
                //__instance.GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);
                //SafeAnimator.SetBool(__instance.animator, "exosuit_docked", true);
                AddDebug("SetVehicleDocked");
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.DockVehicle))]
            //[HarmonyPrefix]
            public static void DockVehiclePrefix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                AddDebug("DockVehicle");
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
                if (__instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__instance.gameObject);
            }
        }

        [HarmonyPatch(typeof(CyclopsHolographicHUD))]
        class CyclopsHolographicHUD_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("UpdateFires")]
            public static void UpdateFiresPostfix(CyclopsHolographicHUD __instance)
            {
                //AddDebug("CyclopsHolographicHUD parent " + __instance.transform.parent.name);
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot && subRoot.isCyclops)
                {
                    bool isPowered = subRoot.powerRelay.IsPowered();
                    __instance.gameObject.SetActive(isPowered);
                    //AddDebug("CyclopsHolographicHUD UpdateFires isPowered " + isPowered);
                    //AddDebug("CyclopsHolographicHUD UpdateFires cyclopsHolographicHUDlastState " + cyclopsHolographicHUDlastState);
                    if (cyclopsHolographicHUDlastState && !isPowered)
                    {
                        //AddDebug("CyclopsHolographicHUD disable ");
                        //__instance.gameObject.SetActive(false);
                        CyclopsCompassHUD cyclopsCompassHUD = subRoot.GetComponentInChildren<CyclopsCompassHUD>();
                        foreach (Transform child in cyclopsCompassHUD.transform)
                                child.gameObject.SetActive(false);
                        CyclopsDecoyScreenHUDManager cdshudm = subRoot.GetComponentInChildren<CyclopsDecoyScreenHUDManager>();
                        cdshudm.gameObject.SetActive(false);
                        CyclopsVehicleStorageTerminalManager cvstm = subRoot.GetComponentInChildren<CyclopsVehicleStorageTerminalManager>();
                        Transform cvstmScreen = cvstm.transform.Find("GUIScreen");
                        cvstmScreen.gameObject.SetActive(false);
                        Transform uchTr = subRoot.transform.Find("UpgradeConsoleHUD");
                        uchTr.gameObject.SetActive(false);
                    }
                    else if (!cyclopsHolographicHUDlastState && isPowered)
                    {
                        //AddDebug("CyclopsHolographicHUD enable ");
                        //__instance.gameObject.SetActive(true);
                        CyclopsCompassHUD cyclopsCompassHUD = subRoot.GetComponentInChildren<CyclopsCompassHUD>();
                        foreach (Transform child in cyclopsCompassHUD.transform)
                                child.gameObject.SetActive(true);
                        CyclopsDecoyScreenHUDManager cdshudm = subRoot.GetComponentInChildren<CyclopsDecoyScreenHUDManager>(true);
                        cdshudm.gameObject.SetActive(true);
                        CyclopsVehicleStorageTerminalManager cvstm = subRoot.GetComponentInChildren<CyclopsVehicleStorageTerminalManager>();
                        Transform cvstmScreen = cvstm.transform.Find("GUIScreen");
                        cvstmScreen.gameObject.SetActive(true);
                        Transform uchTr = subRoot.transform.Find("UpgradeConsoleHUD");
                        uchTr.gameObject.SetActive(true);
                        
                    }
                    cyclopsHolographicHUDlastState = isPowered;
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsLightingPanel))]
        internal class CyclopsLightingPanel_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static bool UpdatePrefix(CyclopsLightingPanel __instance)
            {
                bool isPowered = __instance.CheckIsPowered();
                //if (!isPowered)
                //    __instance.gameObject.SetActive(false);
                //else
                //    __instance.gameObject.SetActive(true);
                //AddDebug("CheckIsPowered " + __instance.CheckIsPowered());
                if (__instance.prevPowerRelayState && !isPowered)
                {
                    //AddDebug("CyclopsLightingPanel not Powered");
                    __instance.SetExternalLighting(false);
                    __instance.uiPanel.SetBool("PanelActive", false);
                    __instance.Invoke("ButtonsOff", 0f);
                }
                else if (!__instance.prevPowerRelayState && isPowered)
                {
                    //AddDebug("CyclopsLightingPanel  Powered");
                    __instance.SetExternalLighting(__instance.floodlightsOn);
                    //__instance.uiPanel.SetBool("PanelActive", true);
                    //__instance.ButtonsOn();
                }
                __instance.prevPowerRelayState = isPowered;
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnTriggerEnter")]
            public static bool OnTriggerEnterPrefix(CyclopsLightingPanel __instance, Collider col)
            {
                if (!col.gameObject.Equals(Player.main.gameObject))
                    return false;
                if (__instance.CheckIsPowered())
                {
                    __instance.uiPanel.SetBool("PanelActive", true);
                    __instance.ButtonsOn();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CyclopsSonarDisplay))]
        internal class CyclopsSonarDisplay_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("DistanceCheck")]
            public static bool UpdatePrefix(CyclopsSonarDisplay __instance)
            {
                //__instance.gameObject.SetActive(false);
                //AddDebug("CyclopsSonarDisplay DistanceCheck ");
                //AddDebug("CyclopsSonarDisplay parent " + __instance.transform.parent.name);
                if (Player.main.currentSub && Player.main.currentSub.isCyclops)
                {
                    if (Player.main.currentSub.powerRelay.IsPowered())
                        __instance.gameObject.SetActive(true);
                    else
                        __instance.gameObject.SetActive(false);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CyclopsSubNameScreen))]
        internal class CyclopsSubNameScreen_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ContentOn")]
            public static bool ContentOnPrefix(CyclopsSubNameScreen __instance)
            {
                if (!Player.main.currentSub.powerRelay.IsPowered())
                    return false;

                __instance.content.SetActive(true);
                Transform lightTr = __instance.transform.parent.Find("VolumetricLight");
                if (lightTr)
                    lightTr.gameObject.SetActive(true);

                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("ContentOff")]
            public static bool ContentOffPrefix(CyclopsSubNameScreen __instance)
            {
                __instance.content.SetActive(false);
                Transform lightTr = __instance.transform.parent.Find("VolumetricLight");
                if (lightTr)
                    lightTr.gameObject.SetActive(false);

                return false;
            }
        }

        [HarmonyPatch(typeof(PilotingChair))]
        internal class PilotingChair_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("IsValidHandTarget")]
            public static bool IsValidHandTargetPrefix(PilotingChair __instance, GUIHand hand, ref bool __result)
            {
                __result = hand.IsFreeToInteract() && hand.player && hand.player.GetCurrentSub() == __instance.subRoot && hand.player.GetMode() == Player.Mode.Normal && __instance.subRoot.powerRelay.IsPowered();
                return false;
            }
        }


    }
}
