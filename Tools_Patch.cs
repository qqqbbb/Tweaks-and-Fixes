using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Tools_Patch
    {
        public static Dictionary<TechType, float> lightIntensityStep = new Dictionary<TechType, float>();
        public static Dictionary<TechType, float> lightOrigIntensity = new Dictionary<TechType, float>();
        public static bool releasingGrabbedObject = false;
        public static bool shootingObject = false;
        public static List<GameObject> repCannonGOs = new List<GameObject>();

        [HarmonyPatch(typeof(FlashLight), nameof(FlashLight.Start))]
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

        //[HarmonyPatch(typeof(Inventory), "OnAddItem")]
        internal class Inventory_OnAddItem_Patch
        { 
            public static void Postfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.item && item.item.GetTechType() == TechType.SmallStorage)
                {
                    AddDebug("Inventory OnAddItem SmallStorage");
                    Transform label = item.item.transform.Find("LidLabel");
                    if (label)
                    {
                        label.localPosition = new Vector3(0.02f, 0.04f, -0.04f);
                    }
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
                    if (tt == TechType.DiveReel || tt == TechType.LaserCutter)
                        return;
                    //AddDebug(tt + " PlayerTool.Awake lights " + lights.Length);
                    lightOrigIntensity[tt] = lights[0].intensity;
                    lightIntensityStep[tt] = lights[0].intensity * .1f;
                    //Main.Log(tt + " lightOrigIntensity " + lights[0].intensity);
                }

            }
            static float knifeRangeDefault = 0f;
            static float knifeDamageDefault = 0f;
            [HarmonyPostfix]
            [HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (Main.config.lightIntensity.ContainsKey(tt))
                {
                    Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                    //AddDebug(tt + " Lights " + lights.Length);
                    foreach (Light l in lights)
                    {
                        l.intensity = Main.config.lightIntensity[tt];
                        //AddDebug("Light Intensity Down " + l.intensity);
                    }
                }
                Knife knife = __instance as Knife;
                if (knife)
                {
                    if (knifeRangeDefault == 0f)
                        knifeRangeDefault = knife.attackDist;
                    if (knifeDamageDefault == 0f)
                        knifeDamageDefault = knife.damage;

                    knife.attackDist = knifeRangeDefault * Main.config.knifeRangeMult;
                    knife.damage = knifeDamageDefault * Main.config.knifeDamageMult;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
                }
                else if (__instance is LEDLight)
                {
                    __instance.transform.localPosition = new Vector3(0.075f, -0.08f, -0.09f);
                    __instance.transform.localEulerAngles = new Vector3(33f, 77f, 45f);
                    __instance.transform.localScale = new Vector3(.9f, .9f, .9f);
                    //AddDebug(" LEDLight OnDraw " + __instance.transform.localPosition);
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnHolster")]
            public static void OnHolsterPostfix(PlayerTool __instance)
            {
                if (__instance is LEDLight)
                    __instance.transform.localScale = Vector3.one;
            }
        }
         
        [HarmonyPatch(typeof(Knife), nameof(Knife.OnToolUseAnim))]
        class Knife_OnToolUseAnim_Postfix_Patch
        {
            public static void Postfix(Knife __instance)
            {
                if (!Main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (Main.config.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Main.guiHand);
                    }
                }
            }
        } 

        [HarmonyPatch(typeof(Constructor), "OnEnable")]
        class Constructor_OnEnable_Patch
        {
            static void Postfix(Constructor __instance)
            {
                //AddDebug("OnEnable Constructor ");
                ImmuneToPropulsioncannon itpc = __instance.GetComponent<ImmuneToPropulsioncannon>();
                //itpc.enabled = false;
                UnityEngine.Object.Destroy(itpc);
            }
        }

        [HarmonyPatch(typeof(ScannerTool), "Update")]
        class ScannerTool_Update_Patch
        {// SHOW power when equipped
            private static bool Prefix(ScannerTool __instance)
            {
                //PlayerTool playerTool = 
                //bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
                if (__instance.isDrawn)
                {
                    //float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
                    //AddDebug("useText1 " + HandReticle.main.useText1);
                    //AddDebug("useText2 " + HandReticle.main.useText2);
                    if (__instance.idleTimer > 0f)
                    {
                        __instance.idleTimer = Mathf.Max(0f, __instance.idleTimer - Time.deltaTime);
                        //string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
                        //               HandReticle.main.SetUseTextRaw(buttonFormat, null);
                    }
                }
                return false;
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
                Vehicle_patch.currentLights = __instance.GetComponentsInChildren<Light>(true);
                if (Main.config.lightIntensity.ContainsKey(TechType.MapRoomCamera))
                {
                    foreach (Light l in Vehicle_patch.currentLights)
                        l.intensity = Main.config.lightIntensity[TechType.MapRoomCamera];
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("FreeCamera")]
            private static void FreeCameraPostfix(MapRoomCamera __instance)
            {
                Vehicle_patch.currentLights[0] = null;
                //AddDebug("MapRoomCamera FreeCamera");
            }
        }

        [HarmonyPatch(typeof(MapRoomScreen), "CycleCamera")]
        class MapRoomScreen_CycleCamera_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CycleCamera")]
            static bool CycleCameraPrefix(MapRoomScreen __instance, int direction)
            {
                if (!Input.GetKey(Main.config.lightKey))
                    return true;

                if (Vehicle_patch.currentLights.Length == 0)
                {
                    //AddDebug("lights.Length == 0 ");
                    return true;
                }
                if (!lightIntensityStep.ContainsKey(TechType.MapRoomCamera))
                {
                    AddDebug("lightIntensityStep missing " + TechType.MapRoomCamera);
                    return false;
                }
                if (!lightOrigIntensity.ContainsKey(TechType.MapRoomCamera))
                {
                    AddDebug("lightOrigIntensity missing " + TechType.MapRoomCamera);
                    return false;
                }
                float step = lightIntensityStep[TechType.MapRoomCamera];
                if (direction < 0)
                    step = -step;
                foreach (Light l in Vehicle_patch.currentLights)
                {
                    if (step > 0 && l.intensity > lightOrigIntensity[TechType.MapRoomCamera])
                        return false;
                    l.intensity += step; 
                    //AddDebug("Light Intensity " + l.intensity);
                    Main.config.lightIntensity[TechType.MapRoomCamera] = l.intensity;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(RepulsionCannon), "OnToolUseAnim")]
        class RepulsionCannon_OnToolUseAnim_Patch
        {
            static bool Prefix(RepulsionCannon __instance, GUIHand guiHand)
            {
                //AddDebug("ShootObject " + rb.name);
                if (__instance.energyMixin.charge <= 0f)
                    return false;
                float num1 = Mathf.Clamp01(__instance.energyMixin.charge * .25f);
                Vector3 forward = MainCamera.camera.transform.forward;
                Vector3 position = MainCamera.camera.transform.position;
                int num2 = UWE.Utils.SpherecastIntoSharedBuffer(position, 1f, forward, 35f, ~(1 << LayerMask.NameToLayer("Player")));
                float num3 = 0f;
                for (int index1 = 0; index1 < num2; ++index1)
                {
                    RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[index1];
                    Vector3 point = raycastHit.point;
                    float num4 = 1f - Mathf.Clamp01(((position - point).magnitude - 1f) * .02857f);
                    GameObject go = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
                    if (go == null)
                        go = raycastHit.collider.gameObject;
                    Rigidbody rb = go.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        num3 += rb.mass;
                        bool flag = true;
                        go.GetComponents<IPropulsionCannonAmmo>(__instance.iammo);
                        for (int index2 = 0; index2 < __instance.iammo.Count; ++index2)
                        {
                            if (!__instance.iammo[index2].GetAllowedToShoot())
                            {
                                flag = false;
                                break;
                            }
                        }
                        __instance.iammo.Clear();
                        if (flag && !(raycastHit.collider is MeshCollider) && (go.GetComponent<Pickupable>() != null || go.GetComponent<Living>() != null || rb.mass <= 1300f && UWE.Utils.GetAABBVolume(go) <= 400f))
                        {
                            float num5 = (1f + rb.mass * 0.005f);
                            Vector3 velocity = forward * num4 * num1 * 70f / num5;
                            repCannonGOs.Add(go);
                            __instance.ShootObject(rb, velocity);
                        }
                    }
                }
                __instance.energyMixin.ConsumeEnergy(4f);
                __instance.fxControl.Play();
                __instance.callBubblesFX = true;
                Utils.PlayFMODAsset(__instance.shootSound, __instance.transform);
                float num6 = Mathf.Clamp(num3 / 100f, 0f, 15f);
                Player.main.GetComponent<Rigidbody>().AddForce(-forward * num6, ForceMode.VelocityChange);
                return false;
            }
        }

        [HarmonyPatch(typeof(PropulsionCannon))]
        class PropulsionCannon_Patch
        {
            //[HarmonyPatch(nameof(PropulsionCannon.TraceForGrabTarget))]
            //[HarmonyPrefix]
            static bool TraceForGrabTargetPrefix(PropulsionCannon __instance, ref GameObject __result)
            {
                __result = null;
                Targeting.GetTarget(Player.main.gameObject, __instance.pickupDistance, out GameObject target, out float targetDist);
                if (target == null)
                {
                    AddDebug(" no target");
                    return false;
                }
                UniqueIdentifier ui = target.GetComponentInParent<UniqueIdentifier>();
                if (ui)
                    AddDebug("target " + ui.gameObject.name);
                else
                {
                    AddDebug(target.name + "has no identifier");
                    return false;
                }
                if (ui.gameObject.GetComponent<FruitPlant>())
                {
                    AddDebug("FruitPlant");
                    return false;
                }
                if (!__instance.ValidateObject(ui.gameObject))
                {
                    AddDebug("could not Validate Object");
                    return false;
                }
                if (ui.gameObject.GetComponent<Pickupable>())
                {
                    AddDebug("Pickupable");
                    __result = target;
                    return false;
                }
                Bounds aabb = __instance.GetAABB(ui.gameObject);
                if (aabb.size.x * aabb.size.y * aabb.size.z <= __instance.maxAABBVolume)
                {
                    __result = target;
                    AddDebug("small object");
                }
                if (__result == null)
                    AddDebug("ValidateNewObject null");
                return false;
            }

            [HarmonyPatch("OnShoot")]
            [HarmonyPrefix]
            static void OnShootPrefix(PropulsionCannon __instance)
            {
                if (__instance.grabbedObject == null)
                    return;
                //AddDebug("OnShoot " + __instance.grabbedObject.name);
                releasingGrabbedObject = true;
            }

            [HarmonyPatch("ReleaseGrabbedObject")]
            [HarmonyPrefix]
            static void ReleaseGrabbedObjectPrefix(PropulsionCannon __instance)
            {
                if (__instance.grabbedObject == null)
                    return;
                //AddDebug("ReleaseGrabbedObject " + __instance.grabbedObject.name);
                releasingGrabbedObject = true;
            }

            //[HarmonyPatch("GrabObject")]
            //[HarmonyPostfix]
            static void GrabObjectPostfix(PropulsionCannon __instance, GameObject target)
            {
                //if (__instance.grabbedObject == null)
                //    return;
                Rigidbody rb = __instance.grabbedObject.GetComponent<Rigidbody>();
                //AddDebug("ReleaseGrabbedObject " + __instance.grabbedObject.name);
                if (rb)
                    AddDebug("Rigidbody useGravity " + rb.useGravity);
                WorldForces wf = __instance.grabbedObject.GetComponent<WorldForces>();
                if (wf)
                {
                    AddDebug("WorldForces handleGravity " + wf.handleGravity);
                    AddDebug("WorldForces aboveWaterGravity " + wf.aboveWaterGravity);
                    AddDebug("WorldForces underwaterGravity " + wf.underwaterGravity);
                }

                //    rb.useGravity = true;
                //releasingGrabbedObject = true;
            }

        }

        [HarmonyPatch(typeof(Beacon))]
        class Beacon_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            static void StartPostfix(Beacon __instance)
            {
                if (string.IsNullOrEmpty(UI_Patches.beaconToolString))
                {
                    //UI_Patches.beaconToolString = TooltipFactory.stringDrop + " (" + TooltipFactory.stringRightHand + ")  " + Language.main.Get("BeaconLabelEdit") + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
                    ////AddDebug("beaconToolString " + UI_Patches.beaconToolString);
                    ////Main.Log("beaconToolString " + UI_Patches.beaconToolString);
                    //UI_Patches.beaconPickString = "(" + TooltipFactory.stringLeftHand + ")\n" + Language.main.Get("BeaconLabelEdit") + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("Throw")]
            static void ThrowPostfix(Beacon __instance)
            {
                // x and z does not matter, it will stabilize itself
                __instance.gameObject.transform.rotation = Camera.main.transform.rotation;
                __instance.transform.Rotate(0f, 180f, 0f);
            }
        }

    }
}
