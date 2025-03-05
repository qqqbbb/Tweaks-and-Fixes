using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Tools_
    {
        //public static List<GameObject> repCannonGOs = new List<GameObject>();
        //static ToggleLights seaglideLights;
        //static VehicleInterface_MapController seaglideMap;
        public static Dictionary<Creature, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<Creature, Dictionary<TechType, int>>();
        public static List<Rigidbody> stasisTargets = new List<Rigidbody>();


        [HarmonyPatch(typeof(FlashLight), "Start")]
        public class FlashLight_Start_Patch
        {
            public static void Prefix(FlashLight __instance)
            {
                Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                for (int i = lights.Length - 1; i >= 0; i--)
                {
                    if (lights[i].type == LightType.Point)
                        lights[i].enabled = false;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerTool))]
        public class PlayerTool_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static void AwakePrefix(PlayerTool __instance)
            {
                Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                if (lights.Length > 0)
                { // seaglide uses 2 lights
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    //AddDebug(tt + " PlayerTool.Awake lights " + lights.Length);
                    if (tt == TechType.DiveReel || tt == TechType.LaserCutter)
                        return;

                    Light_Control.lightOrigIntensity[tt] = lights[0].intensity;
                    Light_Control.lightIntensityStep[tt] = lights[0].intensity * .1f;
                    //Main.logger.LogMessage(tt + " lightOrigIntensity " + lights[0].intensity);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt == TechType.Spadefish)
                {
                    //AddDebug("spadefish");
                    Vector3 pos = __instance.transform.localPosition;
                    __instance.transform.localPosition = new Vector3(pos.x -= .07f, pos.y += .03f, pos.z += .07f);
                    return;
                }
                if (Light_Control.IsLightSaved(tt))
                {
                    Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                    //AddDebug(tt + " Lights " + lights.Length);
                    float intensity = Light_Control.GetLightIntensity(tt);
                    foreach (Light l in lights)
                    {
                        l.intensity = intensity;
                        //AddDebug("Light Intensity Down " + l.intensity);
                    }
                }
                //LEDLight ledLight = __instance as LEDLight;
                //if (ledLight)
                //    ledLight.SetLightsActive(Main.config.LEDLightWorksInHand);
            }

            [HarmonyPostfix]
            [HarmonyPatch("GetCustomUseText")]
            public static void GetCustomUseTextPostfix(PlayerTool __instance, ref string __result)
            {
                if (__instance is StasisRifle)
                {
                    __result = UI_Patches.stasisRifleString;
                }
                else if (__instance is ScannerTool)
                {
                    //AddDebug("GetCustomUseText");
                    __result = UI_Patches.scannerString;
                }
            }
        }



        [HarmonyPatch(typeof(MapRoomCamera))]
        class MapRoomCamera_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ControlCamera")]
            private static void ControlCameraPostfix(MapRoomCamera __instance)
            {
                //AddDebug("MapRoomCamera ControlCamera");
                Vehicle_patch.currentVehicleTT = TechType.MapRoomCamera;
                Light_Control.currentLights = __instance.GetComponentsInChildren<Light>(true);
                if (Light_Control.IsLightSaved(TechType.MapRoomCamera))
                {
                    float intensity = Light_Control.GetLightIntensity(TechType.MapRoomCamera);
                    foreach (Light l in Light_Control.currentLights)
                        l.intensity = intensity;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("FreeCamera")]
            private static void FreeCameraPostfix(MapRoomCamera __instance)
            {
                Light_Control.currentLights[0] = null;
                //AddDebug("MapRoomCamera FreeCamera");
            }
        }

        [HarmonyPatch(typeof(Beacon))]
        class Beacon_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(Beacon __instance)
            {
                Transform label = __instance.transform.Find("label");
                if (label)
                {
                    if (__instance.beaconLabel.stringBeaconSubmit.IsNullOrWhiteSpace())
                        __instance.beaconLabel.stringBeaconSubmit = Language.main.Get("BeaconSubmit");

                    BoxCollider boxCollider = label.GetComponent<BoxCollider>();
                    if (boxCollider)
                        //    UnityEngine.Object.Destroy(boxCollider);
                        //AddDebug("Beacon Start  label");
                        label.gameObject.SetActive(false);
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("Throw")]
            static void ThrowPostfix(Beacon __instance)
            {
                // x and z do not matter, it will stabilize itself
                __instance.gameObject.transform.rotation = Camera.main.transform.rotation;
                __instance.transform.Rotate(0f, 180f, 0f);
            }
        }

        [HarmonyPatch(typeof(StasisSphere))]
        class StasisSphere_Patch
        {
            static bool updateFrame;

            private static bool isBigger(StasisSphere stasisSphere, GameObject go)
            {// does not work reliably for boneshark
                float stasisSphereVolume = UWE.Utils.GetAABBVolume(stasisSphere.gameObject);
                float volume = UWE.Utils.GetAABBVolume(go);
                //AddDebug(go.name + " isBigger " + (int)stasisSphereVolume + ' ' + (int)volume);
                return volume > stasisSphereVolume;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("EnableField")]
            private static void EnableFieldPostfix(StasisSphere __instance)
            {

            }

            [HarmonyPrefix]
            [HarmonyPatch("LateUpdate")]
            private static bool LateUpdatePrefix(StasisSphere __instance)
            {
                //if (!Main.config.stasisRifleTweak)
                //    return true;

                if (!__instance.fieldEnabled)
                    return false;

                updateFrame = !updateFrame;
                if (!updateFrame)
                    return false;

                return true;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("LateUpdate")]
            private static bool LateUpdatePrefix_OLD(StasisSphere __instance)
            {
                //if (!Main.config.stasisRifleTweak)
                //    return true;

                if (!__instance.fieldEnabled)
                    return false;

                __instance.fieldEnergy -= Time.deltaTime / __instance.time;
                float fieldRadius = __instance.fieldRadius;
                //AddDebug("fieldRadius " + (fieldRadius.ToString("1.1") ));
                float num1 = (fieldRadius * fieldRadius + 4f);
                if (__instance.fieldEnergy <= 0f)
                {
                    __instance.fieldEnergy = 0f;
                    __instance.CancelAll();
                    FMODUWE.PlayOneShot(__instance.soundDeactivate, __instance.tr.position);
                }
                else
                {
                    Rigidbody target = null;
                    List<Rigidbody> rigidbodyList = new List<Rigidbody>();
                    int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(__instance.tr.position, fieldRadius, (int)__instance.fieldLayerMask);
                    float distToPlayer = Vector3.Distance(__instance.tr.position, Player.main.transform.position);
                    //AddDebug("distance to player " + (int)distToPlayer + ' ' + (int)(fieldRadius ));
                    __instance.tr.localScale = (2f * fieldRadius + 2f) * Vector3.one;
                    if (distToPlayer < fieldRadius + .5f)
                    {
                        //AddDebug("freeze player ");
                        Player.main.rigidBody.isKinematic = true;
                        //bool bigger = isBigger(__instance, Player.mainObject);
                        //AddDebug("Player isBigger " + bigger);
                    }
                    else if (distToPlayer > fieldRadius - .5f)
                    {
                        //AddDebug("unfreeze player ");
                        Player.main.rigidBody.isKinematic = false;
                    }
                    for (int index = 0; index < num2; ++index)
                    {
                        //AddDebug("sharedColliderBuffer " + UWE.Utils.sharedColliderBuffer[index].name);
                        Collider collider = UWE.Utils.sharedColliderBuffer[index];
                        //if (collider.bounds.size.magnitude < fieldRadius && __instance.Freeze(collider, ref target))
                        //    rigidbodyList.Add(target);
                        if (__instance.Freeze(collider, ref target))
                        {
                            //AddDebug("Freeze " + target.name + " size.magnitude " + (int)collider.bounds.size.magnitude);
                            rigidbodyList.Add(target);
                        }
                    }
                    for (int index = __instance.targets.Count - 1; index >= 0; --index)
                    {
                        target = __instance.targets[index];
                        if (target == null || !target.gameObject.activeSelf)
                        {
                            __instance.targets.RemoveAt(index);
                        }
                        else
                        {
                            Pickupable pickupable = target.GetComponentInParent<Pickupable>();
                            if (pickupable != null && pickupable.attached)
                                __instance.targets.RemoveAt(index);
                            else if (!rigidbodyList.Contains(target))
                            {
                                Vector3 end = target.ClosestPointOnBounds(__instance.tr.position);
                                Vector3 vector3 = end - __instance.tr.position;
                                //UnityEngine.Debug.DrawLine(__instance.tr.position, end, Color.red);
                                if (vector3.sqrMagnitude > num1)
                                {
                                    __instance.Unfreeze(target);
                                    __instance.targets.RemoveAt(index);
                                }
                            }
                        }
                    }
                    __instance.UpdateMaterials();
                }
                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Freeze")]
            private static bool FreezePrefix(StasisSphere __instance, Collider other, ref Rigidbody target, ref bool __result)
            {
                //if (!Main.config.stasisRifleTweak)
                //    return true;

                //AddDebug("Freeze Collider " + other.name + " bounds.size " + other.bounds.size);
                target = other.GetComponentInParent<Rigidbody>();

                if (target == null || target.isKinematic)
                {
                    __result = false;
                    return false;
                }
                if (__instance.targets.Contains(target))
                {
                    __result = true;
                    return false;
                }
                bool bigger = isBigger(__instance, target.gameObject);
                //AddDebug("Freeze " + target.name + " isBigger " + bigger);
                if (bigger)
                {
                    __result = false;
                    return false;
                }
                target.isKinematic = true;
                __instance.targets.Add(target);
                Utils.PlayOneShotPS(__instance.vfxFreeze, target.GetComponent<Transform>().position, Quaternion.identity);
                FMODUWE.PlayOneShot(__instance.soundEnter, __instance.tr.position);
                target.SendMessage("OnFreeze", SendMessageOptions.DontRequireReceiver);
                __result = true;
                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Freeze")]
            private static void FreezePrefix(StasisSphere __instance)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    AddDebug("targets " + __instance.targets.Count);
                    foreach (var rb in __instance.targets)
                    {
                        Main.logger.LogMessage("targets " + rb.name);
                    }
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Freeze")]
            private static void FreezePostfix(StasisSphere __instance, Collider other)
            {
                GameObject go = other.transform.gameObject;
                //if (!Creature_Tweaks.objectsInStasis.TryGetValue(go, out string s))
                {
                    TechType tt = CraftData.GetTechType(go);
                    if (tt == TechType.GasPod)
                    {
                        GasPod gasPod = other.GetComponent<GasPod>();
                        if (gasPod && !gasPod.detonated)
                        {
                            //AddDebug("Freeze  GasPod " );
                            //Creature_Tweaks.objectsInStasis.Add(go, null);
                        }
                    }
                    //else if (tt == TechType.Gasopod)
                    //    Creature_Tweaks.objectsInStasis.Add(go, null);
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Unfreeze")]
            private static void UnfreezePostfix(StasisSphere __instance, Rigidbody target)
            {
                if (target == null)
                    return;

                //Creature_Tweaks.objectsInStasis.Remove(target.gameObject);
                //stasisTargets = __instance.targets;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnDestroy")]
            private static void OnDestroyPrefix(StasisSphere __instance)
            {
                AddDebug("StasisSphere OnDestroy");
                //if (Main.loadingDone)
                //    Player.main.rigidBody.isKinematic = false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            private static void AwakePostfix(StasisSphere __instance)
            {
                stasisTargets = __instance.targets;
            }
        }

        public static void SaveSeaglideState(Seaglide seaglide)
        {
            var seaglideMap = seaglide.GetComponent<VehicleInterface_MapController>();
            if (seaglideMap && seaglideMap.miniWorld)
                Main.configMain.seaglideMap = seaglideMap.miniWorld.active;

            if (seaglide.toggleLights)
                Main.configMain.seaglideLights = seaglide.toggleLights.lightsActive;

            //AddDebug("SaveSeaglideState");
            //Main.configMain.Save();
        }

        public static IEnumerator LoadSeaglideState(Seaglide seaglide)
        {
            if (seaglide == null)
                yield break;

            if (seaglide.toggleLights == null)
                yield return null;

            seaglide.toggleLights.SetLightsActive(Main.configMain.seaglideLights);
            var map = seaglide.GetComponent<VehicleInterface_MapController>();
            if (map == null)
                yield break;

            if (map.miniWorld == null)
                yield return null;

            map.miniWorld.active = Main.configMain.seaglideMap;
        }

        [HarmonyPatch(typeof(Seaglide))]
        class Seaglide_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Seaglide __instance)
            {// fires after onLightsToggled
                //Main.logger.LogMessage("Seaglide Start lightsActive " + __instance.toggleLights.lightsActive);
                CoroutineHost.StartCoroutine(LoadSeaglideState(__instance));
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(Seaglide __instance)
            {
                //AddDebug("Seaglide OnDraw " + Main.config.seaglideLights);
                //seaglideLights = __instance.toggleLights;
                //seaglideMap = __instance.GetComponent<VehicleInterface_MapController>();
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnHolster")]
            public static void OnHolsterPostfix(Seaglide __instance)
            { // fires when saving, after nautilus SaveEvent
              //AddDebug("Seaglide OnHolster " + __instance.toggleLights.lightsActive);
                SaveSeaglideState(__instance);
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

        [HarmonyPatch(typeof(BuilderTool), "HasEnergyOrInBase")]
        class BuilderTool_HasEnergyOrInBase_Patch
        {
            static void Postfix(BuilderTool __instance, ref bool __result)
            {
                if (!ConfigToEdit.builderToolBuildsInsideWithoutPower.Value && __instance.energyMixin.charge <= 0)
                {
                    __result = false;
                }
            }
        }

        //[HarmonyPatch(typeof(VFXController))]
        class VFXController_SpawnFX_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("Play")]
            static bool PlayPostfix(VFXController __instance, int i)
            {

                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("SpawnFX")]
            static bool SpawnFXPrefix(VFXController __instance, int i)
            {
                if (__instance.emitters[i].fx == null)
                    return false;

                Transform parent = __instance.emitters[i].parented ? __instance.emitters[i].parentTransform : __instance.transform;
                GameObject gameObject = Utils.SpawnPrefabAt(__instance.emitters[i].fx, parent, parent.position);
                //AddDebug("SpawnFX " + gameObject.name);
                if (__instance.emitters[i].fakeParent && __instance.emitters[i].parented)
                {
                    gameObject.AddComponent<VFXFakeParent>().Parent(__instance.emitters[i].parentTransform, __instance.emitters[i].posOffset, __instance.emitters[i].eulerOffset);
                }
                else
                {
                    gameObject.transform.localEulerAngles = __instance.emitters[i].eulerOffset;
                    gameObject.transform.localPosition = __instance.emitters[i].posOffset;
                }
                if (__instance.emitters[i].lateTime)
                    gameObject.AddComponent<VFXLateTimeParticles>();

                if (!__instance.emitters[i].parented)
                    gameObject.transform.parent = null;

                __instance.emitters[i].instanceGO = gameObject;
                __instance.emitters[i].fxPS = gameObject.GetComponent<ParticleSystem>();
                gameObject.SetActive(true);
                return false;
            }
        }

        [HarmonyPatch(typeof(ScannerTool), "PlayScanFX")]
        class ScannerTool_PlayScanFX_Patch
        {
            static bool Prefix(ScannerTool __instance)
            {
                //AddDebug("ScannerTool PlayScanFX ");
                return ConfigToEdit.scannerFX.Value;
            }
        }



    }
}
