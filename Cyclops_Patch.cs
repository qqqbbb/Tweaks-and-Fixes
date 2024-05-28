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
        static Rigidbody cyclopsRB; // vanilla code does not cache RB
        public static CyclopsEntryHatch ceh;
        //public static CyclopsHelmHUDManager cyclopsHelmHUDManager;
        public static HashSet<Collider> collidersInSub = new HashSet<Collider>();
        static float vertSpeedMult = .5f;
        static float backwardSpeedMult = .5f;
        

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
                    Physics.IgnoreCollision(myCol, c);
            }
            collidersInSub.AddRange(myCols);
            //foreach (Collider c in myCols)
            //AddDebug("add collider to collidersInSub");
        }

        static int GetFireCountInEngineRoom(SubFire subFire)
        {
            int fireCount = 0;
            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> roomFire in subFire.roomFires)
            {
                if (roomFire.Key != CyclopsRooms.EngineRoom)
                    continue;

                foreach (Transform spawnNode in roomFire.Value.spawnNodes)
                {
                    if (spawnNode.childCount != 0)
                        ++fireCount;
                }
            }
            //AddDebug("fire Count " + fireCount);
            return fireCount;
        }
      
        static int GetEngineOverheatMinValue(SubFire subFire)
        {
            int overheatValue = GetFireCountInEngineRoom(subFire) * 10;

            //if (subFire.cyclopsMotorMode.engineOn && subFire.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
            //    overheatValue += 10;

            //AddDebug("GetEngineOverheatMinValue " + overheatValue);
            return overheatValue;
        }

        [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
        class CyclopsHelmHUDManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            public static void UpdatePostfix(CyclopsHelmHUDManager __instance)
            {
                if (!__instance.LOD.IsFull())
                    return;
                //if (Player.main.currentSub && Player.main.currentSub == __instance.subRoot)
                {
                    //AddDebug("powerOn " + powerOn);
                    if (ConfigToEdit.cyclopsHUDalwaysOn.Value)
                    {
                        bool powerOn = __instance.subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline;
                        __instance.hudActive = powerOn;
                    }
                    if (__instance.motorMode.engineOn) // hide speed selector when engine off
                        __instance.engineToggleAnimator.SetTrigger("EngineOn");
                    else
                        __instance.engineToggleAnimator.SetTrigger("EngineOff");
                    //cyclopsHelmHUDManager = __instance;
                    //AddDebug("hudActive " + __instance.hudActive);
                    //__instance.canvasGroup.alpha = 0f;
                    //__instance.hornObject.SetActive(true);
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
                //__instance.hudActive = true;
                //__instance.hornObject.SetActive(true);
                //AddDebug("StopPiloting  ");
            }
        }
        
        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnCollisionEnter")]
            public static bool StartPostfix(SubRoot __instance, Collision col)
            {
                if (col.gameObject.CompareTag("Player"))
                    return false;

                Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
                if (rb && rb.mass <= 6) // fishschool mass is 6
                {
                    //AddDebug(col.gameObject.name + "OnCollisionEnter Rigidbody mass < 6");
                    return false;
                }
                GameObject root = Util.GetEntityRoot(col.gameObject);
                if (root == null)
                {
                    //AddDebug(col.gameObject.name + " OnCollisionEnter root null");
                    return true;
                }
                rb = root.GetComponent<Rigidbody>();
                if (rb && rb.mass <= 6)
                {
                    //AddDebug(col.gameObject.name + "OnCollisionEnter Rigidbody mass < 6");
                    return false;
                }
                //if (__instance.isCyclops)
                //{
                //    AddDebug(col.gameObject.name + " OnCollisionEnter " + rb.mass + " mag " + col.relativeVelocity.magnitude);
                //}
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnProtoSerialize")]
            public static void OnProtoSerializePostfix(SubRoot __instance)
            {
                CyclopsMotorMode cyclopsMotorMode = __instance.GetComponent<CyclopsMotorMode>();
                if (cyclopsMotorMode)
                {
                    Main.configMain.subThrottleIndex = (int)cyclopsMotorMode.cyclopsMotorMode;
                    //AddDebug("save subThrottleIndex");
                    //Main.configMenu.Save();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("ForceLightingState")]
            public static void ForceLightingStatePostfix(SubRoot __instance, bool lightingOn)
            {
                if (__instance.isCyclops)
                    __instance.interiorSky.affectedByDayNightCycle = ConfigToEdit.cyclopsSunlight.Value && !lightingOn;
                //AddDebug("SubRoot ForceLightingState " + lightingOn);
            }
        }
        
        [HarmonyPatch(typeof(CyclopsMotorModeButton), "Start")]
        class CyclopsMotorModeButton_Start_Patch
        {
            public static void Postfix(CyclopsMotorModeButton __instance)
            {
                if (Main.configMain.subThrottleIndex != -1)
                {
                    //AddDebug("restore  subThrottleIndex");
                    SetCyclopsMotorMode(__instance, (CyclopsMotorMode.CyclopsMotorModes)Main.configMain.subThrottleIndex);
                }
            }
        }
        
        [HarmonyPatch(typeof(SubControl))]
        class SubControl_Patch
        {
            static int numBallastWeight;

            [HarmonyPrefix]
            [HarmonyPatch("UpdateAnimation")]
            public static bool UpdateAnimationPrefix(SubControl __instance)
            { // fix steering wheel animation
                if (!Main.gameLoaded)
                    return false;

                float steeringWheelYaw = 0f;
                float steeringWheelPitch = 0f;
                //AddDebug("throttle x " + __instance.throttle.x.ToString("0.0"));
                //AddDebug("throttle y " + __instance.throttle.y.ToString("0.0"));
                float throttleX = __instance.throttle.x;
                float throttleY = __instance.throttle.y;
                if (Mathf.Abs(throttleX) > 0.0001)
                {
                    ShipSide useShipSide;
                    if (throttleX > 0)
                    {
                        useShipSide = ShipSide.Port;
                        steeringWheelYaw = throttleX;
                    }
                    else
                    {
                        useShipSide = ShipSide.Starboard;
                        steeringWheelYaw = throttleX;
                    }
                    if (throttleX < -0.1 || throttleX > 0.1)
                    {
                        for (int index = 0; index < __instance.turnHandlers.Length; ++index)
                            __instance.turnHandlers[index].OnSubTurn(useShipSide);
                    }
                }
                if (Mathf.Abs(throttleY) > 0.0001)
                    steeringWheelPitch = throttleY;

                __instance.steeringWheelYaw = Mathf.Lerp(__instance.steeringWheelYaw, steeringWheelYaw, Time.deltaTime * __instance.steeringReponsiveness);
                __instance.steeringWheelPitch = Mathf.Lerp(__instance.steeringWheelPitch, steeringWheelPitch, Time.deltaTime * __instance.steeringReponsiveness);
                if (__instance.mainAnimator)
                { 
                    __instance.mainAnimator.SetFloat("view_yaw", __instance.steeringWheelYaw * 100f);
                    __instance.mainAnimator.SetFloat("view_pitch", __instance.steeringWheelPitch * 100f);
                    //Player.main.playerAnimator.SetFloat("cyclops_yaw", __instance.steeringWheelYaw);
                    //Player.main.playerAnimator.SetFloat("cyclops_pitch", __instance.steeringWheelPitch);
                }
                return false;
            }
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SubControl __instance)
            {
                //if (Main.config.vehicleMoveTweaks) 
                //{ 
                //__instance.BaseVerticalAccel = __instance.BaseForwardAccel * .5f;
                //}
                TechTag techTag = __instance.gameObject.EnsureComponent<TechTag>();
                techTag.type = TechType.Cyclops;
                LargeWorldEntity_Patch.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
                Transform tr = __instance.transform.Find("CyclopsCollision/helmGroup");
                if (tr)
                    LargeWorldEntity_Patch.AddVFXsurfaceComponent(tr.gameObject, VFXSurfaceTypes.glass);

                WorldForces wf = __instance.GetComponent<WorldForces>();
                if (wf) // prevent it from jumping out of water when surfacing
                    wf.aboveWaterGravity = 30f;

                numBallastWeight = __instance.gameObject.GetComponentsInChildren<BallastWeight>().Length;
                //AddDebug("Start numBallastWeight " + numBallastWeight);
                tr = __instance.transform.Find("Headlights");
                if (tr) // not used
                    UnityEngine.Object.Destroy(tr.gameObject);

                Tools_Patch.lightOrigIntensity[TechType.Cyclops] = 2f;
                Tools_Patch.lightIntensityStep[TechType.Cyclops] = .2f;
                Light[] lights = __instance.transform.Find("Floodlights").GetComponentsInChildren<Light>(true);
                //AddDebug("SubControl.Start lights intensity " + lights[0].intensity);
                if (Main.configMain.lightIntensity.ContainsKey(TechType.Cyclops))
                {
                    foreach (Light l in lights)
                        l.intensity = Main.configMain.lightIntensity[TechType.Cyclops];
                }

            }

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static bool UpdatePrefix(SubControl __instance)
            { // fix diagonal speed 
                if (!Main.gameLoaded || !__instance.LOD.IsFull())
                    return false;

                if (!ConfigMenu.cyclopsMoveTweaks.Value || Main.cyclopsDockingLoaded)
                    return true;

                __instance.appliedThrottle = false;
                if (__instance.controlMode == SubControl.Mode.DirectInput)
                {
                    __instance.throttle = GameInput.GetMoveDirection();
                    __instance.throttle.Normalize(); //my
                    //AddDebug("throttle " + __instance.throttle);
                    //AddDebug(".magnitude " + __instance.throttle.magnitude);
                    if (__instance.canAccel && __instance.throttle.magnitude > 0.0001f)
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
                            __instance.transform.parent.BroadcastMessage("ToggleFloodlights", null, SendMessageOptions.DontRequireReceiver);

                        if (GameInput.GetButtonDown(GameInput.Button.Exit))
                            Player.main.TryEject();
                    }
                }
                if (!__instance.appliedThrottle)
                    __instance.throttle = new Vector3(0f, 0f, 0f);

                __instance.UpdateAnimation();
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("FixedUpdate")]
            public static bool FixedUpdatePrefix(SubControl __instance)
            {  // halve vertical and backward speed
                if (!ConfigMenu.cyclopsMoveTweaks.Value)
                    return true;

                if (!__instance.LOD.IsFull() || __instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;

                for (int index = 0; index < __instance.accelerationModifiers.Length; ++index)
                    __instance.accelerationModifiers[index].ModifyAcceleration(ref __instance.throttle);

                if (Ocean.GetDepthOf(__instance.gameObject) <= 0f)
                    return false;

                if (Mathf.Abs(__instance.throttle.x) > 0.0001f)
                {
                    float baseTurningTorque = __instance.BaseTurningTorque;
                    if (__instance.canAccel)
                        cyclopsRB.AddTorque(__instance.sub.subAxis.up * baseTurningTorque * __instance.turnScale * __instance.throttle.x, ForceMode.Acceleration);
                }
                if (Mathf.Abs(__instance.throttle.y) > 0.0001f)
                {
                    float num = __instance.BaseVerticalAccel * vertSpeedMult + numBallastWeight * __instance.AccelPerBallast;
                    if (__instance.canAccel)
                        cyclopsRB.AddForce(Vector3.up * num * __instance.accelScale * __instance.throttle.y, ForceMode.Acceleration);
                }
                if (__instance.canAccel)
                {
                     if (__instance.throttle.z > 0.0001f)
                        cyclopsRB.AddForce(__instance.sub.subAxis.forward * __instance.BaseForwardAccel *  __instance.accelScale * __instance.throttle.z, ForceMode.Acceleration);
                    else if (__instance.throttle.z < -0.0001f)
                        cyclopsRB.AddForce(__instance.sub.subAxis.forward * __instance.BaseForwardAccel * backwardSpeedMult * __instance.accelScale * __instance.throttle.z, ForceMode.Acceleration);
                }
                return false;
            }
        }
        
        [HarmonyPatch(typeof(CyclopsEntryHatch), "OnTriggerEnter")]
        class CyclopsEntryHatch_Start_Patch
        { // OnTriggerExit does not fire if you use closest ladder so hatch does not close
            static void Postfix(CyclopsEntryHatch __instance, Collider col)
            {
                if (col.gameObject != Player.main.gameObject)
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
                        //AddDebug("CLOSE ! " );
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
                {
                    //AddDebug("sub consume power");
                    return false;
                }
                __instance.TurnOffSilentRunning();
                return false;
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        public class VehicleDockingBay_LaunchbayAreaEnter_Patch
        { // dont play sfx if another vehicle docked
            [HarmonyPrefix]
            [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaEnter))]
            public static bool LaunchbayAreaEnterPrefix(VehicleDockingBay __instance)
            {
                if (__instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;
                return !__instance._dockedVehicle;
            }
            [HarmonyPrefix]
            [HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaExit))]
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
                //AddDebug("UpdateDockedPosition " + vehicle.transform.position);
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
                //AddDebug("SetVehicleDocked");
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.DockVehicle))]
            //[HarmonyPrefix]
            public static void DockVehiclePrefix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //AddDebug("DockVehicle");
            }
        }

        [HarmonyPatch(typeof(Constructable), "Start")]
        class Constructable_Start_Patch
        {
            public static void Postfix(Constructable __instance)
            {
                if (__instance.GetComponentInParent<SubControl>())
                {
                    //AddDebug("Constructable Start");
                    AddCyclopsCollisionExclusion(__instance.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(Plantable), "Spawn")]
        public class Plantable_Spawn_Patch
        {
            public static void Postfix(Plantable __instance, Transform parent, bool isIndoor, GameObject __result)
            {
                if (__result && __instance.GetComponentInParent<SubControl>())
                    AddCyclopsCollisionExclusion(__result);
            }
        }

        [HarmonyPatch(typeof(GrownPlant), "Awake")]
        public class GrownPlant_Awake_Patch
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
        public class CyclopsLightingPanel_Patch
        {
            static void TurnOnFloodlights(CyclopsLightingPanel clp)
            {
                if (clp.cyclopsRoot.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline || clp.cyclopsRoot.silentRunning)
                    return;

                clp.floodlightsOn = true;
                clp.SetExternalLighting(true);
                //FMODUWE.PlayOneShot(this.floodlightsOn ? this.vn_floodlightsOn : this.vn_floodlightsOff, this.transform.position);
                clp.UpdateLightingButtons();
            }

            static void TurnOffInternalLighting(CyclopsLightingPanel clp)
            {
                clp.lightingOn = false;
                clp.cyclopsRoot.ForceLightingState(false);
                //FMODUWE.PlayOneShot(this.lightingOn ? this.vn_lightsOn : this.vn_lightsOff, this.transform.position);
                clp.UpdateLightingButtons();
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CyclopsLightingPanel __instance)
            {
                //AddDebug("ToggleFloodlights " + __instance.floodlightsOn);
                //Main.config.cyclopsFloodtLights = __instance.floodlightsOn;
                if (Main.configMain.cyclopsFloodLights)
                    TurnOnFloodlights(__instance);

                if (!Main.configMain.cyclopsLighting)
                    TurnOffInternalLighting(__instance);
            }
            [HarmonyPostfix]
            [HarmonyPatch("ToggleFloodlights")]
            static void ToggleFloodlightsPostfix(CyclopsLightingPanel __instance)
            {
                //AddDebug("ToggleFloodlights " + __instance.floodlightsOn);
                Main.configMain.cyclopsFloodLights = __instance.floodlightsOn;
            }
            [HarmonyPostfix]
            [HarmonyPatch("ToggleInternalLighting")]
            static void ToggleInternalLightingPostfix(CyclopsLightingPanel __instance)
            {
                //AddDebug("ToggleFloodlights " + __instance.floodlightsOn);
                Main.configMain.cyclopsLighting = __instance.lightingOn;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SubConstructionComplete")]
            public static bool SubConstructionCompletePrefix(CyclopsLightingPanel __instance)
            { // fix: lights are on even if sub has no batteries
                //AddDebug("CyclopsLightingPanel SubConstructionComplete " + __instance.floodlightsOn);
                //AddDebug("CyclopsLightingPanel Powered " + __instance.CheckIsPowered());
                bool powered = __instance.CheckIsPowered();
                __instance.floodlightsOn = powered;
                __instance.SetExternalLighting(powered);
                __instance.UpdateLightingButtons();
                return false;
            }
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
                if (col.gameObject != Player.main.gameObject)
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
        public class CyclopsSonarDisplay_Patch
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
        public class CyclopsSubNameScreen_Patch
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
        public class PilotingChair_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("IsValidHandTarget")]
            public static bool IsValidHandTargetPrefix(PilotingChair __instance, GUIHand hand, ref bool __result)
            {
                __result = hand.IsFreeToInteract() && hand.player && hand.player.GetCurrentSub() == __instance.subRoot && hand.player.GetMode() == Player.Mode.Normal && __instance.subRoot.powerRelay.IsPowered();
                return false;
            }
        }

        [HarmonyPatch(typeof(CyclopsDestructionEvent))]
        class CyclopsDestructionEvent_SwapToDamagedModels_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("DestroyCyclops")]
            static void DestroyCyclopsPrefix(CyclopsDestructionEvent __instance)
            { // fix bug: when cyclops gets destroyed with player in it player respawns in it
                __instance.subLiveMixin.Kill();
                //AddDebug("CyclopsDestructionEvent DestroyCyclops IsAlive " + __instance.subLiveMixin.IsAlive());
            }
           
            [HarmonyPrefix]
            [HarmonyPatch("SwapToDamagedModels")]
            static bool SwapToDamagedModelsPrefix(CyclopsDestructionEvent __instance)
            {
                for (int index = 0; index < __instance.intact.Length; ++index)
                {
                    GameObject go = __instance.intact[index];
                    if (go) // NPE without this line 
                        go.SetActive(false);
                }
                for (int index = 0; index < __instance.destroyed.Length; ++index)
                    __instance.destroyed[index].SetActive(true);

                __instance.ToggleSinking(true);
                __instance.subRoot.subWarning = false;
                __instance.subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
                __instance.subRoot.subDestroyed = true;
                __instance.pingInstance.enabled = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(CyclopsProximitySensors), "OnPlayerModeChange")]
        class CyclopsProximitySensors_OnPlayerModeChange_Patch
        {
            public static bool Prefix(CyclopsProximitySensors __instance)
            {
                //Main.config.disableCyclopsProximitySensor = true;
                //AddDebug ("CyclopsProximitySensors OnPlayerModeChange " + ConfigToEdit.disableCyclopsProximitySensor.Value);
                return !ConfigToEdit.disableCyclopsProximitySensor.Value;
            }
        }

        [HarmonyPatch(typeof(SubFire))]
        class SubFire_EngineOverheatSimulation_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static bool StartPrefix(SubFire __instance)
            {
                //AddDebug("SubFire Start  " + __instance.transform.position);
                // prevent invoke in prefab
                if (__instance.transform.position == Vector3.zero)
                    return false;

                return true;
            }
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool UpdatePrefix(SubFire __instance)
            {
                return Main.gameLoaded;
            }
            [HarmonyPrefix]
            [HarmonyPatch("EngineOverheatSimulation")]
            static bool EngineOverheatSimulationPrefix(SubFire __instance)
            {
                //AddDebug("SubFire EngineOverheatSimulation activeInHierarchy " + __instance.gameObject.activeInHierarchy);
                //AddDebug("SubFire position " + __instance.transform.position);
                if (!__instance.gameObject.activeInHierarchy || !__instance.LOD.IsFull())
                    return false;
         
                //AddDebug("engineOverheatValue " + __instance.engineOverheatValue);
                //AddDebug("appliedThrottle " + __instance.subControl.appliedThrottle);
                //AddDebug("engineOn " + __instance.cyclopsMotorMode.engineOn);
                if (!__instance.cyclopsMotorMode.engineOn)
                {
                    //AddDebug("engine off");
                    __instance.engineOverheatValue = Mathf.Max(GetEngineOverheatMinValue(__instance), __instance.engineOverheatValue - 10);
                    return false;
                }
                if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
                {
                    if (__instance.subControl.appliedThrottle)
                        __instance.engineOverheatValue += 10;
                    else
                        __instance.engineOverheatValue -= 4;
                }
                else if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Standard)
                {
                    if (__instance.subControl.appliedThrottle)
                        __instance.engineOverheatValue -= 3;
                    else
                        __instance.engineOverheatValue -= 6;
                }
                else if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Slow)
                {
                    if (__instance.subControl.appliedThrottle)
                        __instance.engineOverheatValue -= 6;
                    else
                        __instance.engineOverheatValue -= 8;
                }
                if (__instance.subControl.appliedThrottle && __instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
                {
                    if (ConfigMenu.cyclopsFireChance.Value > 0)
                    {
                        if (__instance.engineOverheatValue > 75)
                        {
                            __instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatCriticalNotification);
                        }
                        else if (__instance.engineOverheatValue > 50)
                        {
                            __instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatNotification);
                        }
                    }
                }
                int overheatMinValue = GetEngineOverheatMinValue(__instance);
                __instance.engineOverheatValue = Mathf.Clamp(__instance.engineOverheatValue, overheatMinValue, 100);

                if (__instance.engineOverheatValue > 50)
                {
                    int fireChance = UnityEngine.Random.Range(0, __instance.engineOverheatValue + 50);
                    fireChance = Mathf.Clamp(fireChance, 0, 100);
                    int fireCheck = 100 - ConfigMenu.cyclopsFireChance.Value;
                    //AddDebug("fireChance " + fireChance);
                    if (fireChance > fireCheck)
                        __instance.CreateFire(__instance.roomFires[CyclopsRooms.EngineRoom]);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CyclopsExternalDamageManager), "UpdateOvershield")]
        class CyclopsExternalDamageManager_UpdateOvershield_Patch
        {
            static bool Prefix(CyclopsExternalDamageManager __instance)
            {
                if (ConfigMenu.cyclopsAutoHealHealthPercent.Value == 90)
                    return true;

                //AddDebug(__instance.name + " CyclopsExternalDamageManager UpdateOvershield " + __instance.overshieldPercentage);
                if (__instance.subFire.GetFireCount() > 0 || __instance.subLiveMixin.GetHealthFraction() <= ConfigMenu.cyclopsAutoHealHealthPercent.Value * .01f)
                    return false;

                __instance.subLiveMixin.AddHealth(3f);
                return false;

            }
        }


    }
}
