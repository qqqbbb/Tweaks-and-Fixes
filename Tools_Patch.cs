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
        static ToggleLights seaglideToggleLights = null;
        public static Dictionary<Creature, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<Creature, Dictionary<TechType, int>>();

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
                if (tt == TechType.Spadefish)
                {
                    //AddDebug("spadefish");
                    Vector3 pos = __instance.transform.localPosition;
                    __instance.transform.localPosition = new Vector3(pos.x -= .07f, pos.y += .03f, pos.z += .07f); 
                    return;
                }
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
                LEDLight ledLight = __instance as LEDLight;
                if (ledLight)
                    ledLight.SetLightsActive(Main.config.LEDLightWorksInHand);
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
        
        [HarmonyPatch(typeof(Knife))]
        class Knife_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnToolUseAnim")]
            public static void OnToolUseAnimPostfix(Knife __instance)
            {
                if (!Player.main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Player.main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Player.main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (Main.config.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Player.main.guiHand);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamagePostfix(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                if (isAlive || wasAlive)
                    return;

                TechType techType = CraftData.GetTechType(target);
                string name = techType.AsString();
                if (Main.config.deadCreatureLoot.ContainsKey(name))
                {
                    Creature creature = target.GetComponent<Creature>();
                    if (creature == null)
                        return;

                    if (deadCreatureLoot.ContainsKey(creature))
                    {
                        foreach (var pair in Main.config.deadCreatureLoot[name])
                        {
                            TechType loot = pair.Key;
                            int max = pair.Value;
                            if (deadCreatureLoot[creature].ContainsKey(loot) && deadCreatureLoot[creature][loot] < max)
                            {
                                CraftData.AddToInventory(loot);
                                deadCreatureLoot[creature][loot]++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var pair in Main.config.deadCreatureLoot[name])
                        {
                            CraftData.AddToInventory(pair.Key);
                            deadCreatureLoot.Add(creature, new Dictionary<TechType, int> { { pair.Key, 1 } });
                        }
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
                if (itpc)
                    UnityEngine.Object.Destroy(itpc);
            }
        }

        [HarmonyPatch(typeof(ScannerTool))]
        class ScannerTool_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Update")]// Show power when equipped
            private static bool UpdatePrefix(ScannerTool __instance)
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

            static bool scanning = false;
            static bool finishedScan = false;

            [HarmonyPostfix]
            [HarmonyPatch("Scan")]
            private static void ScanPostfix(ScannerTool __instance, PDAScanner.Result __result)
            { // -57 -23 -364
                //if (__result != PDAScanner.Result.None)
                //    AddDebug("Scan " + __result.ToString());
                if (Main.config.removeFragmentCrate && finishedScan && PDAScanner.scanTarget.techType == TechType.StarshipCargoCrate)
                { // destroy crate
                    //AddDebug("finished scan " + __result.ToString() + " " + PDAScanner.scanTarget.gameObject.name);
                    UnityEngine.Object.Destroy(PDAScanner.scanTarget.gameObject);
                    scanning = false;
                    finishedScan = false;
                    return;
                }
                if (scanning && __result == PDAScanner.Result.Done || __result == PDAScanner.Result.Researched)
                    finishedScan = true;
                else
                    finishedScan = false;

                scanning = PDAScanner.IsFragment(PDAScanner.scanTarget.techType);
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
        
        [HarmonyPatch(typeof(Beacon))]
        class Beacon_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Throw")]
            static void ThrowPostfix(Beacon __instance)
            {
                // x and z does not matter, it will stabilize itself
                __instance.gameObject.transform.rotation = Camera.main.transform.rotation;
                __instance.transform.Rotate(0f, 180f, 0f);
            }
        }

        //[HarmonyPatch(typeof(StasisSphere))]
        class StasisSphere_Patch
        {
            private static bool isBigger(StasisSphere stasisSphere, GameObject go)
            {// does not work reliably for boneshark
                float stasisSphereVolume = UWE.Utils.GetAABBVolume(stasisSphere.gameObject);
                float volume = UWE.Utils.GetAABBVolume(go);
                //AddDebug(go.name + " isBigger " + (int)stasisSphereVolume + ' ' + (int)volume);
                return volume > stasisSphereVolume;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("LateUpdate")]
            private static bool LateUpdatePrefix(StasisSphere __instance)
            {
                if (!Main.config.stasisRifleTweak)
                    return true;

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
                        if ( __instance.Freeze(collider, ref target))
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
                if (!Main.config.stasisRifleTweak)
                    return true;

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

            //[HarmonyPostfix]
            //[HarmonyPatch("CancelAll")]
            private static void CancelAllPrefix(StasisSphere __instance)
            {
                //AddDebug("CancelAll");
                if (Main.loadingDone)
                    Player.main.rigidBody.isKinematic = false;
            }
        }



        //[HarmonyPatch(typeof(Seaglide))]
        class Seaglide_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(Seaglide __instance)
            {
                if (!Main.seaglideMapControlsLoaded)
                    seaglideToggleLights = __instance.toggleLights;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnHolster")]
            public static void OnHolsterPostfix(Seaglide __instance)
            {
                seaglideToggleLights = null;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void StartPostfix(Seaglide __instance)
            {
                if (!Main.seaglideMapControlsLoaded)
                    __instance.toggleLights.lightState = 2; // dont show map
                //Main.Message("lightState " + __instance.toggleLights.lightState);
            }
        }

        //[HarmonyPatch(typeof(Inventory), "OnAddItem")]
        class Inventory_OnAddItem_Patch
        {
            public static void Postfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.item && item.item.GetTechType() == TechType.SmallStorage)
                {
                    //AddDebug("Inventory OnAddItem SmallStorage");
                    Transform label = item.item.transform.Find("LidLabel");
                    if (label)
                    {
                        label.localPosition = new Vector3(0.02f, 0.04f, -0.04f);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Bullet), "Deactivate")]
        public class Bullet_Deactivate_Patch
        {
            public static void Prefix(Bullet __instance)
            {
                //AddDebug("Deactivate");
                //Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                //for (int i = lights.Length - 1; i >= 0; i--)
                //{
                //    if (lights[i].type == LightType.Point)
                //        lights[i].enabled = false;
                //}
            }
        }

        //[HarmonyPatch(typeof(ToggleLights))]
        class ToggleLights_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("CheckLightToggle")]
            public static bool CheckLightTogglePrefix(ToggleLights __instance)
            {
                if (!Main.seaglideMapControlsLoaded && __instance == seaglideToggleLights)
                {
                    if (Player.main.pda.isInUse)
                        return false;

                    //HandReticle.main.SetUseTextRaw(UI_Patches.seaglideString, null);
                    if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                        __instance.SetLightsActive(!__instance.lightsActive);
                    else if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                    {
                        if (__instance.lightState == 2)
                            __instance.lightState = 0;
                        else
                            __instance.lightState = 2;
                    }
                    //AddDebug("lightState " + __instance.lightState);
                    return false;
                }
                return true;
            }

        }

        //[HarmonyPatch(typeof(RepulsionCannon), "OnToolUseAnim")]
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

        //[HarmonyPatch(typeof(PropulsionCannon))]
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
                    //AddDebug(" no target");
                    return false;
                }
                UniqueIdentifier ui = target.GetComponentInParent<UniqueIdentifier>();
                if (ui)
                    AddDebug("target " + ui.gameObject.name);
                else
                {
                    //AddDebug(target.name + "has no identifier");
                    return false;
                }
                if (ui.gameObject.GetComponent<FruitPlant>())
                {
                    //AddDebug("FruitPlant");
                    return false;
                }
                if (!__instance.ValidateObject(ui.gameObject))
                {
                    //AddDebug("could not Validate Object");
                    return false;
                }
                if (ui.gameObject.GetComponent<Pickupable>())
                {
                    //AddDebug("Pickupable");
                    __result = target;
                    return false;
                }
                Bounds aabb = __instance.GetAABB(ui.gameObject);
                if (aabb.size.x * aabb.size.y * aabb.size.z <= __instance.maxAABBVolume)
                {
                    __result = target;
                    //AddDebug("small object");
                }
                //if (__result == null)
                //AddDebug("ValidateNewObject null");
                return false;
            }

            //[HarmonyPatch("OnShoot")]
            //[HarmonyPrefix]
            static void OnShootPrefix(PropulsionCannon __instance)
            {
                if (__instance.grabbedObject == null)
                    return;
                //AddDebug("OnShoot " + __instance.grabbedObject.name);
                releasingGrabbedObject = true;
            }

            //[HarmonyPatch("ReleaseGrabbedObject")]
            //[HarmonyPrefix]
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
                //if (rb)
                //    AddDebug("Rigidbody useGravity " + rb.useGravity);
                WorldForces wf = __instance.grabbedObject.GetComponent<WorldForces>();
                if (wf)
                {
                    //AddDebug("WorldForces handleGravity " + wf.handleGravity);
                    //AddDebug("WorldForces aboveWaterGravity " + wf.aboveWaterGravity);
                    //AddDebug("WorldForces underwaterGravity " + wf.underwaterGravity);
                }

                //    rb.useGravity = true;
                //releasingGrabbedObject = true;
            }


        }
   
    }
}
