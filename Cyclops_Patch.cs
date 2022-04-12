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
        //public static BoxCollider leftUpperWall;
        //public static BoxCollider rightUpperWall;
        //static bool upperWallMoved = false;

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
                //FixCollision(__instance);
            }

            private static void FixCollision(SubRoot __instance)
            {
                if (__instance.isCyclops && __instance.name == "Cyclops-MainPrefab(Clone)")
                {// Start runs for prefab too
                    Transform outerCol = __instance.transform.Find("CyclopsCollision/zOuterGroup");
                    if (outerCol)
                    {
                        //AddDebug("SubRoot Start " );
                        foreach (Transform child in outerCol)
                        { // ignore cyclops outer colliders when building in cyclops
                            //AddDebug("outerCol child " + child.name);
                            child.gameObject.layer = LayerID.NotUseable;
                            //child.gameObject.SetActive(false);
                        }
                    }
                    Transform rightLowerWall = __instance.transform.Find("CyclopsCollision/keelFrontGroup/right_wall");
                    if (rightLowerWall)
                    {
                        rightLowerWall.localPosition = new Vector3(-.25f, 0f, 0f);
                        Vector3 rot = rightLowerWall.eulerAngles;
                        rightLowerWall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform leftLowerWall = __instance.transform.Find("CyclopsCollision/keelFrontGroup/left_wall");
                    if (leftLowerWall)
                    {
                        leftLowerWall.localPosition = new Vector3(-.15f, 0f, 0f);
                        Vector3 rot = leftLowerWall.eulerAngles;
                        leftLowerWall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform launchBayright_wall = __instance.transform.Find("CyclopsCollision/launchBayright_wall");
                    if (launchBayright_wall)
                    {
                        launchBayright_wall.localPosition = new Vector3(-.125f, 0f, 0f);
                        Vector3 rot = launchBayright_wall.eulerAngles;
                        launchBayright_wall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform launchBayleft_wall = __instance.transform.Find("CyclopsCollision/launchBayleft_wall");
                    if (launchBayleft_wall)
                    {
                        launchBayleft_wall.localPosition = new Vector3(-.04f, 0f, 0f);
                        Vector3 rot = launchBayleft_wall.eulerAngles;
                        launchBayleft_wall.eulerAngles = new Vector3(80f, rot.y, rot.z);
                    }
                    Transform secondRoomGroup = __instance.transform.Find("CyclopsCollision/secondRoomGroup");
                    Transform secondRoomRight_wall = __instance.transform.Find("CyclopsCollision/secondRoomGroup/right_wall");
                    BoxCollider[] colliders = secondRoomRight_wall.GetComponents<BoxCollider>();
                    //AddDebug("secondRoomRight_wall size " + colliders[0].size);
                    GameObject leftWall = new GameObject("leftWall");
                    leftWall.transform.SetParent(secondRoomGroup);
                    Vector3 leftWallPos = secondRoomRight_wall.transform.position;
                    leftWall.transform.position = new Vector3(leftWallPos.x + .15f, leftWallPos.y, leftWallPos.z);
                    Vector3 leftWallRot = secondRoomRight_wall.transform.eulerAngles;
                    Vector3 leftWallCenter = colliders[0].center;
                    leftWall.transform.eulerAngles = new Vector3(leftWallRot.x - 3.5f, leftWallRot.y + 1f, leftWallRot.z);
                    BoxCollider leftWallСol = leftWall.AddComponent<BoxCollider>();
                    leftWallСol.size = colliders[0].size;
                    leftWallСol.center = new Vector3(leftWallCenter.x + .4f, leftWallCenter.y, leftWallCenter.z);
                    UnityEngine.Object.Destroy(colliders[0]);
                    Vector3 rightWallPos = colliders[1].transform.position;
                    colliders[1].transform.position = new Vector3(rightWallPos.x - 1.05f, rightWallPos.y, rightWallPos.z);
                    Vector3 rightWallRot = colliders[1].transform.eulerAngles;
                    colliders[1].transform.eulerAngles = new Vector3(rightWallRot.x + 3.5f, rightWallRot.y + .9f, rightWallRot.z);

                    Transform controlRoomGroup = __instance.transform.Find("CyclopsCollision/controlRoomGroup");
                    Transform controlRoomRightWall = __instance.transform.Find("CyclopsCollision/controlRoomGroup/right_wall");
                    BoxCollider[] controlRoomСolliders = controlRoomRightWall.GetComponents<BoxCollider>();
                    GameObject controlRoomLeftWall = new GameObject("leftWall");
                    controlRoomLeftWall.transform.eulerAngles = controlRoomRightWall.transform.eulerAngles;
                    controlRoomLeftWall.transform.SetParent(controlRoomGroup);
                    Vector3 controlRoomLeftWallPos = controlRoomRightWall.transform.position;
                    controlRoomLeftWall.transform.position = new Vector3(controlRoomLeftWallPos.x + .35f, controlRoomLeftWallPos.y, controlRoomLeftWallPos.z);
                    BoxCollider controlRoomLeftWallСol = controlRoomLeftWall.AddComponent<BoxCollider>();
                    controlRoomLeftWallСol.size = controlRoomСolliders[0].size;
                    controlRoomLeftWallСol.center = controlRoomСolliders[0].center;
                    UnityEngine.Object.Destroy(controlRoomСolliders[0]);
                    Vector3 controlRoomRightWallPos = controlRoomСolliders[1].transform.position;
                    controlRoomСolliders[1].transform.position = new Vector3(controlRoomRightWallPos.x - .5f, controlRoomRightWallPos.y, controlRoomRightWallPos.z);

                    Transform engineRoomLeftWall = __instance.transform.Find("CyclopsCollision/engineRoomGroup/right_wall");
                    engineRoomLeftWall.name = "leftWall";
                    Transform engineRoomRightWall = __instance.transform.Find("CyclopsCollision/engineRoomGroup/right_wall");
                    Vector3 engineRoomLeftWallPos = engineRoomLeftWall.transform.position;
                    Vector3 engineRoomLeftWallRot = engineRoomLeftWall.transform.eulerAngles;
                    engineRoomLeftWall.transform.eulerAngles = new Vector3(engineRoomLeftWallRot.x, engineRoomLeftWallRot.y - 1f, engineRoomLeftWallRot.z);
                    engineRoomLeftWall.transform.position = new Vector3(engineRoomLeftWallPos.x + 1f, engineRoomLeftWallPos.y, engineRoomLeftWallPos.z);
                    Vector3 engineRoomRightWallPos = engineRoomRightWall.transform.position;

                    engineRoomRightWall.transform.position = new Vector3(engineRoomRightWallPos.x - 1.0f, engineRoomRightWallPos.y, engineRoomRightWallPos.z);
                    Vector3 engineRoomRightWallRot = engineRoomRightWall.transform.eulerAngles;
                    engineRoomRightWall.transform.eulerAngles = new Vector3(engineRoomRightWallRot.x, engineRoomRightWallRot.y + .75f, engineRoomRightWallRot.z);
                }
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


        //[HarmonyPatch(typeof(GUIHand), "UpdateActiveTarget")]
        class GUIHand_UpdateActiveTarget_PostfixPatch
        {
            public static bool Prefix(GUIHand __instance)
            {
                if (!Player.main.currentSub || !Player.main.currentSub.isCyclops)
                    return true;

                PlayerTool tool = __instance.GetTool();
                if (tool != null && tool.GetComponent<PropulsionCannon>() != null && tool.GetComponent<PropulsionCannon>().IsGrabbingObject())
                {
                    __instance.activeTarget = tool.GetComponent<PropulsionCannon>().GetNearbyGrabbedObject();
                    __instance.suppressTooltip = true;
                }
                else if (tool != null && tool.DoesOverrideHand() || !Targeting.GetTarget(Player.main.gameObject, 2f, out __instance.activeTarget, out __instance.activeHitDistance))
                {
                    __instance.activeTarget = null;
                    __instance.activeHitDistance = 0.0f;
                }
                else if (__instance.activeTarget.layer == LayerID.NotUseable)
                {
         
                    if (__instance.activeTarget.transform.parent && __instance.activeTarget.transform.parent.name == "zOuterGroup")
                    {
                        //AddDebug(" NotUseable + " + __instance.activeTarget.transform.parent.name);
                    }
                    else
                        __instance.activeTarget = null;
                }
                else
                {
                    IHandTarget handTarget = null;
                    for (Transform transform = __instance.activeTarget.transform; transform != null; transform = transform.parent)
                    {
                        handTarget = transform.GetComponent<IHandTarget>();
                        if (handTarget != null)
                        {
                            __instance.activeTarget = transform.gameObject;
                            break;
                        }
                    }
                    if (handTarget == null)
                    {
                        switch (CraftData.GetHarvestTypeFromTech(CraftData.GetTechType(__instance.activeTarget)))
                        {
                            case HarvestType.None:
                                __instance.activeTarget = null;
                                break;
                            case HarvestType.Pick:
                                if (Utils.FindAncestorWithComponent<Pickupable>(__instance.activeTarget) == null)
                                {
                                    LargeWorldEntity ancestorWithComponent = Utils.FindAncestorWithComponent<LargeWorldEntity>(__instance.activeTarget);
                                    ancestorWithComponent.gameObject.AddComponent<Pickupable>();
                                    ancestorWithComponent.gameObject.AddComponent<WorldForces>().useRigidbody = ancestorWithComponent.GetComponent<Rigidbody>();
                                    break;
                                }
                                break;
                        }
                    }
                }
                if (!IntroVignette.isIntroActive)
                    return false;
                __instance.activeTarget = __instance.FilterIntroTarget(__instance.activeTarget);
                //AddDebug(" UpdateActiveTarget + " + __instance.activeTarget);
                return false;
            }

            public static void Postfix(GUIHand __instance)
            {
                //AddDebug(" UpdateActiveTarget + " + __instance.activeTarget);
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

        //[HarmonyPatch(typeof(Builder), "IsObstacle", new Type[]{typeof(Collider)})]
        //[HarmonyPatch(typeof(Builder), "CheckTag")]
        class Builder_IsObstacle_Patch
        {
            public static void Postfix(Collider c, ref bool __result)
            {
                //AddDebug("Builder CheckTag " + c.name);
            }
        }

        [HarmonyPatch(typeof(Builder), "Initialize")]
        class Builder_Initialize_Patch
        {
            public static void Postfix()
            { // ignore cyclops outer colliders when building in cyclops
              //Builder.placeLayerMask = (LayerMask)~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Trigger") | 1 << LayerMask.NameToLayer("NotUseable"));
                Builder.placeLayerMask = -6815745;
               //AddDebug("Builder Initialize ");
               // Main.Log("Builder Initialize " + Builder.placeLayerMask.value);
            }
        }

        [HarmonyPatch(typeof(Targeting), "GetTarget", new Type[] { typeof(float), typeof(GameObject), typeof(float), typeof(Targeting.FilterRaycast) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal })]
        class Targeting_GetTarget_PrefixPatch
        {
            public static bool Prefix(ref GameObject result, ref bool __result, float maxDistance, Targeting.FilterRaycast filter, out float distance)
            {
                //AddDebug(" Targeting GetTarget  " + result.name);
                if (!Player.main.currentSub || !Player.main.currentSub.isCyclops)
                {
                    distance = 0f;
                    return true;
                }
                bool flag = false;
                Transform transform = MainCamera.camera.transform;
                Vector3 position = transform.position;
                Vector3 forward = transform.forward;
                Ray ray = new Ray(position, forward);
                //int layerMask = -2097153;
                int layerMask = Builder.placeLayerMask;
                QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
                int numHits1 = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, Builder.placeLayerMask, queryTriggerInteraction);
                RaycastHit resultHit;
                if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, filter, out resultHit))
                    flag = true;
                if (!flag)
                {
                    for (int index = 0; index < Targeting.radiuses.Length; ++index)
                    {
                        float radiuse = Targeting.radiuses[index];
                        ray.origin = position + forward * radiuse;
                        int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radiuse, maxDistance, layerMask, queryTriggerInteraction);
                        if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, filter, out resultHit))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                Targeting.Reset();
                result = resultHit.collider != null ? resultHit.collider.gameObject : null;
                distance = resultHit.distance;
                __result = flag;
                return false;
            }
        }

        [HarmonyPatch(typeof(Fabricator), "Start")]
        class Fabricator_Start_Patch
        {
            public static void Postfix(Fabricator __instance)
            { 
                if (Main.loadingDone && __instance.transform.parent && __instance.transform.parent.name == "Cyclops-MainPrefab(Clone)")
                { // collision does not match mesh. Can see it after fixing cyclops collision. move it so cant see it when outside
                  //AddDebug("Fabricator Start parent " + __instance.transform.parent.name);
                    __instance.transform.position += __instance.transform.forward * .11f;
                }
            }
        }

        //[HarmonyPatch(typeof(Builder), "CheckAsSubModule")]
        class Builder_CheckAsSubModule_Patch
        {
            //static int colliderLayerID = LayerID.NotUseable;
            //static GameObject outerCol = null;
            static Dictionary<GameObject, int> outerColliders = new Dictionary<GameObject, int>();

            public static bool Prefix(ref bool __result)
            {
                //AddDebug("Builder CheckAsSubModule " + __result);
                if (!Constructable.CheckFlags(Builder.allowedInBase, Builder.allowedInSub, Builder.allowedOutside))
                {
                    __result = false;
                    return false;
                }
                Transform aimTransform = Builder.GetAimTransform();
                Builder.placementTarget = null;
                RaycastHit hitInfo;
                if (!Physics.Raycast(aimTransform.position, aimTransform.forward, out hitInfo, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
                {
                    __result = false;
                    return false;
                }
                //AddDebug("Builder CheckAsSubModule " + hitInfo.collider.name + " layer " + hitInfo.collider.gameObject.layer);
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot && subRoot.isCyclops)
                {
                    //while (subOuterCol.Contains(hitInfo.collider))
                    //{
                    //    AddDebug("Builder CheckAsSubModule outer col " + hitInfo.collider.name + " parent " + hitInfo.collider.gameObject.transform.parent);
                    //    outerColliders[hitInfo.collider.gameObject] = hitInfo.collider.gameObject.layer;
                    //    hitInfo.collider.gameObject.layer = LayerID.Player;
                    //    if (!Physics.Raycast(aimTransform.position, aimTransform.forward, out hitInfo, Builder.placeMaxDistance, Builder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
                    //    {
                    //        __result = false;
                    //        return false;
                    //    }
                    //    AddDebug("Raycast again " + hitInfo.collider.name + " parent " + hitInfo.collider.gameObject.transform.parent);
                    //}
                }
                Builder.placementTarget = hitInfo.collider.gameObject;
                Builder.SetPlaceOnSurface(hitInfo, ref Builder.placePosition, ref Builder.placeRotation);
                if (!Builder.CheckTag(hitInfo.collider) || !Builder.CheckSurfaceType(Builder.GetSurfaceType(hitInfo.normal)) || !Builder.CheckDistance(hitInfo.point, Builder.placeMinDistance) || !Builder.allowedOnConstructables && Builder.HasComponent<Constructable>(hitInfo.collider.gameObject))
                {
                    __result = false;
                    return false;
                }
                if (!Player.main.IsInSub())
                {
                    GameObject hitObject = UWE.Utils.GetEntityRoot(Builder.placementTarget);
                    if (!hitObject)
                        hitObject = Builder.placementTarget;
                    if (!Builder.ValidateOutdoor(hitObject))
                    {
                        __result = false;
                        return false;
                    }
                }
                __result = Builder.CheckSpace(Builder.placePosition, Builder.placeRotation, Builder.bounds, Builder.placeLayerMask.value, hitInfo.collider);
                return false;
            }

            public static void Postfix(ref bool __result)
            {
                foreach (var pair in outerColliders)
                {
                    //AddDebug("restore layer " + pair.Key.name);
                    pair.Key.layer = pair.Value;
                }
                outerColliders = new Dictionary<GameObject, int>();
            }
        }

        [HarmonyPatch(typeof(BuilderTool), "HandleInput")]
        class BuilderTool_HandleInput_Patch
        { // ignore cyclops outer colliders when building in cyclops
            //static readonly Targeting.FilterRaycast filter = hit => hit.collider != null && hit.collider.gameObject.layer == LayerID.NotUseable;
            public static bool Prefix(BuilderTool __instance )
            {
                if (__instance.handleInputFrame == Time.frameCount)
                    return false;
                //AddDebug("BuilderTool HandleInput ");
                __instance.handleInputFrame = Time.frameCount;
                if (!__instance.isDrawn || Builder.isPlacing || (!AvatarInputHandler.main.IsEnabled() || __instance.TryDisplayNoPowerTooltip()))
                    return false;
                //AddDebug("BuilderTool HandleInput placeLayerMask " + Builder.placeLayerMask.value);
                RaycastHit hitInfo;
                if (!Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 30f, Builder.placeLayerMask.value, QueryTriggerInteraction.Collide))
                    return false;

                //AddDebug("BuilderTool HandleInput Target " + hitInfo.collider.name + " parent " + hitInfo.collider.transform.parent.name);
                bool buttonHeld1 = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
                bool buttonDown = GameInput.GetButtonDown(GameInput.Button.Deconstruct);
                bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.Deconstruct);
                Constructable constructable = hitInfo.collider.GetComponentInParent<Constructable>();
                if (constructable != null && hitInfo.distance > constructable.placeMaxDistance)
                    constructable = null;
                if (constructable != null)
                {
                    __instance.OnHover(constructable);
                    if (buttonHeld1)
                    {
                        __instance.Construct(constructable, true);
                    }
                    else
                    {
                        string reason;
                        if (constructable.DeconstructionAllowed(out reason))
                        {
                            if (!buttonHeld2)
                                return false;
                            if (constructable.constructed)
                                constructable.SetState(false, false);
                            else
                                __instance.Construct(constructable, false);
                        }
                        else
                        {
                            if (!buttonDown || string.IsNullOrEmpty(reason))
                                return false;
                            AddMessage(reason);
                        }
                    }
                }
                else
                {
                    BaseDeconstructable deconstructable = hitInfo.collider.GetComponentInParent<BaseDeconstructable>();
                    //BaseDeconstructable deconstructable = result.GetComponentInParent<BaseDeconstructable>();
                    if (deconstructable == null)
                    {
                        BaseExplicitFace componentInParent = hitInfo.collider.GetComponentInParent<BaseExplicitFace>();
                        //BaseExplicitFace componentInParent = result.GetComponentInParent<BaseExplicitFace>();
                        if (componentInParent != null)
                            deconstructable = componentInParent.parent;
                    }
                    if (deconstructable == null)
                        return false;
                    string reason;
                    if (deconstructable.DeconstructionAllowed(out reason))
                    {
                        __instance.OnHover(deconstructable);
                        if (!buttonDown)
                            return false;
                        deconstructable.Deconstruct();
                    }
                    else
                    {
                        if (!buttonDown || string.IsNullOrEmpty(reason))
                            return false;
                        AddMessage(reason);
                    }
                }
                return false;
            }
        }

        //[HarmonyPatch(typeof(BuilderTool), "OnHolster")]
        class BuilderTool_OnHolster_Patch
        {
            public static void Prefix(BuilderTool __instance)
            {
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot && subRoot.isCyclops)
                {
                    //AddDebug("BuilderTool OnHolster ");
                    Transform outerCol = subRoot.transform.Find("CyclopsCollision/zOuterGroup");
                    if (outerCol)
                    {
                        foreach (Transform child in outerCol)
                        {
                            //AddDebug("outerCol child " + child.name);
                            //child.gameObject.layer = LayerID.Default;
                            //child.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(PlayerTool), "OnDraw")]
        class PlayerTool_OnDraw_Patch
        {
            public static void Prefix(PlayerTool __instance)
            {
                SubRoot subRoot = Player.main.currentSub;
                if (subRoot && subRoot.isCyclops && __instance is BuilderTool)
                {
                    //AddDebug("PlayerTool OnDraw ");
                    Transform outerCol = subRoot.transform.Find("CyclopsCollision/zOuterGroup");
                    if (outerCol)
                    {
                        foreach (Transform child in outerCol)
                        {
                            //AddDebug("outerCol child " + child.name);
                            child.gameObject.layer = LayerID.Player;
                            //child.gameObject.SetActive(false);
                        }
                    }
                }
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
