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
    class Tools_Patch
    {
        public static Dictionary<TechType, float> lightIntensityStep = new Dictionary<TechType, float>();
        public static Dictionary<TechType, float> lightOrigIntensity = new Dictionary<TechType, float>();

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

                    lightOrigIntensity[tt] = lights[0].intensity;
                    lightIntensityStep[tt] = lights[0].intensity * .1f;
                    //Main.logger.LogMessage(tt + " lightOrigIntensity " + lights[0].intensity);
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
                if (Main.configMain.lightIntensity.ContainsKey(tt))
                {
                    Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                    //AddDebug(tt + " Lights " + lights.Length);
                    foreach (Light l in lights)
                    {
                        l.intensity = Main.configMain.lightIntensity[tt];
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

                    knife.attackDist = knifeRangeDefault * ConfigMenu.knifeRangeMult.Value;
                    knife.damage = knifeDamageDefault * ConfigMenu.knifeDamageMult.Value;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
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

        public static bool giveResourceOnDamage;
        public static bool spawning;
        static Vector3 targetPos;

        [HarmonyPatch(typeof(Knife))]
        class Knife_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnToolUseAnim")]
            public static bool OnToolUseAnimPrefix(Knife __instance, GUIHand hand)
            {
                Vector3 position = new Vector3();
                GameObject closestObj = null;
                UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref closestObj, ref position);

                //if (closestObj)
                //{
                //AddDebug("OnToolUseAnim closestObj " + closestObj.name);
                //AddDebug("OnToolUseAnim closestObj parent " + closestObj.transform.parent.name);
                //AddDebug("OnToolUseAnim closestObj parent parent " + closestObj.transform.parent.parent.name);
                //}
                //else
                //    AddDebug("OnToolUseAnim closestObj null");

                if (closestObj == null)
                {
                    InteractionVolumeUser ivu = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (ivu != null && ivu.GetMostRecent() != null)
                    {
                        closestObj = ivu.GetMostRecent().gameObject;
                        //AddDebug("OnToolUseAnim GetMostRecent " + closestObj.name);
                    }
                }
                if (closestObj)
                {
                    Utils.PlayFMODAsset(__instance.attackSound, __instance.transform);
                    VFXSurface vfxSurface = closestObj.GetComponentInParent<VFXSurface>();
                    //if (vfxSurface)
                    //    AddDebug("OnToolUseAnim vfxSurface " + vfxSurface.surfaceType);
                    //else
                    //    AddDebug("OnToolUseAnim no vfxSurface ");
                    Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                    ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);

                    LiveMixin liveMixin = closestObj.GetComponentInParent<LiveMixin>();
                    bool validTarget = liveMixin == null || Knife.IsValidTarget(liveMixin);
                    //AddDebug("OnToolUseAnim IsValidTarget " + validTarget);
                    if (validTarget)
                    {
                        if (liveMixin)
                        {
                            bool wasAlive = liveMixin.IsAlive();
                            liveMixin.TakeDamage(__instance.damage, position, __instance.damageType, Player.main.gameObject);
                            __instance.GiveResourceOnDamage(closestObj, liveMixin.IsAlive(), wasAlive);
                        }
                    }
                    else
                        closestObj = null;
                }
                if (closestObj || hand.GetActiveTarget())
                    return false;

                if (Player.main.IsUnderwater())
                    Utils.PlayFMODAsset(__instance.underwaterMissSound, __instance.transform);
                else
                    Utils.PlayFMODAsset(__instance.surfaceMissSound, __instance.transform);

                return false;
            }

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
                    if (Main.configMain.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Player.main.guiHand);
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamagePrefix(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                //AddDebug("GiveResourceOnDamage ");
                giveResourceOnDamage = true;
                targetPos = target.transform.position;
            }

            [HarmonyPostfix]
            [HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamagePostfix(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                giveResourceOnDamage = false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("GiveResourceOnDamage")]
            public static void GiveResourceOnDamageMy(Knife __instance, GameObject target, bool isAlive, bool wasAlive)
            {
                if (isAlive || wasAlive)
                    return;

                //TechType techType = CraftData.GetTechType(target);
                //string name = techType.AsString();
                //if (Main.config.deadCreatureLoot.ContainsKey(name))
                //{
                //    Creature creature = target.GetComponent<Creature>();
                //    if (creature == null)
                //        return;

                //    if (deadCreatureLoot.ContainsKey(creature))
                //    {
                //        foreach (var pair in Main.config.deadCreatureLoot[name])
                //        {
                //            TechType loot = pair.Key;
                //            int max = pair.Value;
                //            if (deadCreatureLoot[creature].ContainsKey(loot) && deadCreatureLoot[creature][loot] < max)
                //            {
                //                CraftData.AddToInventory(loot);
                //                deadCreatureLoot[creature][loot]++;
                //            }
                //        }
                //    }
                //    else
                //    {
                //        foreach (var pair in Main.config.deadCreatureLoot[name])
                //        {
                //            CraftData.AddToInventory(pair.Key);
                //            deadCreatureLoot.Add(creature, new Dictionary<TechType, int> { { pair.Key, 1 } });
                //        }
                //    }
                //}
            }
        }

        public static IEnumerator Spawn(TechType techType, IOut<GameObject> result, int num = 1)
        {
            //AddDebug("AddToInventoryOrSpawn " + techType + " " + num);
            for (int i = 0; i < num; ++i)
            {
                TaskResult<GameObject> currentResult = new TaskResult<GameObject>();
                yield return CraftData.GetPrefabForTechTypeAsync(techType, false, currentResult);
                GameObject prefab = currentResult.Get();
                GameObject target = !(prefab != null) ? Utils.CreateGenericLoot(techType) : Utils.SpawnFromPrefab(prefab, null);
                if (target != null)
                {
                    spawning = true;
                    float x = Mathf.Lerp(targetPos.x, MainCamera.camera.transform.position.x, .5f);
                    float y = MainCamera.camera.transform.position.y + MainCamera.camera.transform.forward.y * 3f; // fix for creepvine 
                    float z = Mathf.Lerp(targetPos.z, MainCamera.camera.transform.position.z, .5f);
                    target.transform.position = new Vector3(x, y, z);
                    CrafterLogic.NotifyCraftEnd(target, techType);
                    result.Set(target);
                }
            }
        }

        [HarmonyPatch(typeof(CraftData), "AddToInventory")]
        class CraftData_AddToInventory_Patch
        {
            static bool Prefix(CraftData __instance, TechType techType, int num = 1, bool noMessage = false, bool spawnIfCantAdd = true)
            {
                if (giveResourceOnDamage && !Inventory.main.HasRoomFor(techType))
                {
                    //AddDebug("AddToInventory Prefix giveResourceOnDamage " + techType + " " + num);
                    CoroutineHost.StartCoroutine(Spawn(techType, DiscardTaskResult<GameObject>.Instance, num));
                    return false;
                }
                return true;
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
                if (Main.configMain.lightIntensity.ContainsKey(TechType.MapRoomCamera))
                {
                    foreach (Light l in Vehicle_patch.currentLights)
                        l.intensity = Main.configMain.lightIntensity[TechType.MapRoomCamera];
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
                if (!Input.GetKey(ConfigMenu.lightButton.Value))
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
                    Main.configMain.lightIntensity[TechType.MapRoomCamera] = l.intensity;
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

                __instance.fieldEnergy -= Time.deltaTime / __instance.time;
                //AddDebug("fieldEnergy " + __instance.fieldEnergy.ToString("0.00"));
                float fieldRadius = __instance.fieldRadius;
                float num1 = fieldRadius * fieldRadius + 4f;
                if (__instance.fieldEnergy <= 0)
                {
                    //AddDebug("CancelAll ");
                    __instance.fieldEnergy = 0f;
                    __instance.CancelAll();
                    FMODUWE.PlayOneShot(__instance.soundDeactivate, __instance.tr.position);
                }
                else
                {
                    Rigidbody target = null;
                    List<Rigidbody> rigidbodyList = new List<Rigidbody>();
                    int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(__instance.tr.position, fieldRadius, (int)__instance.fieldLayerMask);
                    for (int index = 0; index < num2; ++index)
                    {
                        if (__instance.Freeze(UWE.Utils.sharedColliderBuffer[index], ref target))
                            rigidbodyList.Add(target);
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
                            Pickupable componentInParent = target.GetComponentInParent<Pickupable>();
                            if (componentInParent != null && componentInParent.attached)
                                __instance.targets.RemoveAt(index);
                            else if (!rigidbodyList.Contains(target))
                            {
                                Vector3 end = target.ClosestPointOnBounds(__instance.tr.position);
                                Vector3 vector3 = end - __instance.tr.position;
                                //Debug.DrawLine(__instance.tr.position, end, Color.red);
                                if (vector3.sqrMagnitude > num1)
                                {
                                    __instance.Unfreeze(target);
                                    __instance.targets.RemoveAt(index);
                                }
                            }
                        }
                    }
                    __instance.tr.localScale = (2f * fieldRadius + 2f) * Vector3.one;
                    __instance.UpdateMaterials();
                }
                return false;
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

            private static void HighFOVseaglideOnUpdate(MainCameraControl __instance)
            {// fix: can see your neck wnen using seaglide and high FOV
                //AddDebug("MainCameraControl OnUpdate " + Inventory.main.quickSlots.activeToolName);
                float deltaTime = Time.deltaTime;
                __instance.swimCameraAnimation = !__instance.underWaterTracker.isUnderWater ? Mathf.Clamp01(__instance.swimCameraAnimation - deltaTime) : Mathf.Clamp01(__instance.swimCameraAnimation + deltaTime);
                Vector3 velocity = __instance.playerController.velocity;
                bool pdaInUse = false;
                bool flag2 = false;
                bool inVehicle = false;
                bool inExosuit = Player.main.inExosuit;
                bool builderMenuOpen = uGUI_BuilderMenu.IsOpen();
                if (Player.main != null)
                {
                    pdaInUse = Player.main.GetPDA().isInUse;
                    inVehicle = Player.main.motorMode == Player.MotorMode.Vehicle;
                    flag2 = pdaInUse | inVehicle || __instance.cinematicMode;
                    if (XRSettings.enabled && VROptions.gazeBasedCursor)
                        flag2 |= builderMenuOpen;
                }
                if (flag2 != __instance.wasInLockedMode || __instance.lookAroundMode != __instance.wasInLookAroundMode)
                {
                    __instance.camRotationX = 0.0f;
                    __instance.camRotationY = 0.0f;
                    __instance.wasInLockedMode = flag2;
                    __instance.wasInLookAroundMode = __instance.lookAroundMode;
                }
                bool flag5 = (!__instance.cinematicMode || __instance.lookAroundMode && !pdaInUse) && __instance.mouseLookEnabled && (inVehicle || AvatarInputHandler.main == null || AvatarInputHandler.main.IsEnabled() || Builder.isPlacing);
                if (inVehicle && !XRSettings.enabled && !inExosuit)
                    flag5 = false;

                Transform transform = __instance.transform;
                float num1 = pdaInUse || __instance.lookAroundMode || Player.main.GetMode() == Player.Mode.LockedPiloting ? 1f : -1f;
                if (!flag2 || __instance.cinematicMode && !__instance.lookAroundMode)
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
                if (!XRSettings.enabled)
                {
                    Vector3 localPosition = __instance.cameraOffsetTransform.localPosition;
                    localPosition.z = highFOVseaglideCameraOffset;
                    //localPosition.z = Mathf.Clamp(localPosition.z + (PDA.deltaTime * num1 * 0.25f), __instance.camPDAZStart, __instance.camPDAZOffset + __instance.camPDAZStart);
                    //AddDebug("  localPosition.z " + localPosition.z.ToString("0.00"));
                    __instance.cameraOffsetTransform.localPosition = localPosition;
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
                __instance.UpdateCamShake();
                if (__instance.cinematicMode && !__instance.lookAroundMode)
                {
                    __instance.camRotationX = Mathf.LerpAngle(__instance.camRotationX, 0.0f, deltaTime * 2f);
                    __instance.camRotationY = Mathf.LerpAngle(__instance.camRotationY, 0.0f, deltaTime * 2f);
                    __instance.transform.localEulerAngles = new Vector3(-__instance.camRotationY + __instance.camShake, __instance.camRotationX, 0.0f);
                }
                else if (flag2)
                {
                    if (!XRSettings.enabled)
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
                    __instance.cameraUPTransform.localEulerAngles = new Vector3(Mathf.Min(0.0f, -__instance.rotationY + __instance.camShake), 0.0f, 0.0f);
                    transform.localEulerAngles = new Vector3(Mathf.Max(0.0f, -__instance.rotationY + __instance.camShake), __instance.rotationX, 0.0f);
                }
                __instance.UpdateStrafeTilt();
                Vector3 vector3_1 = __instance.transform.localEulerAngles + new Vector3(0.0f, 0.0f, (__instance.cameraAngleMotion.y * __instance.cameraTiltMod + __instance.strafeTilt + __instance.camShake * 0.5f));
                float num3 = 0.0f - __instance.skin;
                if (!flag2 && __instance.GetCameraBob())
                {
                    __instance.smoothedSpeed = UWE.Utils.Slerp(__instance.smoothedSpeed, Mathf.Min(1f, velocity.magnitude / 5f), deltaTime);
                    num3 += ((Mathf.Sin(Time.time * 6f) - 1f) * (0.02f + __instance.smoothedSpeed * 0.15f)) * __instance.swimCameraAnimation;
                }
                if (__instance.impactForce > 0)
                {
                    __instance.impactBob = Mathf.Min(0.9f, __instance.impactBob + __instance.impactForce * deltaTime);
                    __instance.impactForce -= (Mathf.Max(1f, __instance.impactForce) * deltaTime * 5f);
                }
                float y = num3 - __instance.impactBob - __instance.stepAmount;
                if (__instance.impactBob > 0.0)
                    __instance.impactBob = Mathf.Max(0.0f, __instance.impactBob - (Mathf.Pow(__instance.impactBob, 0.5f) * Time.deltaTime * 3f));
                __instance.stepAmount = Mathf.Lerp(__instance.stepAmount, 0f, deltaTime * Mathf.Abs(__instance.stepAmount));
                __instance.transform.localPosition = new Vector3(0f, y, 0f);
                __instance.transform.localEulerAngles = vector3_1;
                if (Player.main.motorMode == Player.MotorMode.Vehicle)
                    __instance.transform.localEulerAngles = Vector3.zero;
                Vector3 vector3_2 = new Vector3(0f, __instance.transform.localEulerAngles.y, 0.0f);
                Vector3 vector3_3 = __instance.transform.localPosition;
                if (XRSettings.enabled)
                {
                    vector3_2.y = !flag2 || inVehicle ? 0f : __instance.viewModelLockedYaw;
                    if (!inVehicle && !__instance.cinematicMode)
                    {
                        if (!flag2)
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

        [HarmonyPatch(typeof(VFXController))]
        class VFXController_SpawnFX_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("Play")]
            static bool PlayPostfix(VFXController __instance, int i)
            {

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("SpawnFX")]
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

        static ParticleSystem[] heatBladeParticles;

        [HarmonyPatch(typeof(VFXLateTimeParticles))]
        public class VFXLateTimeParticles_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Play")]
            public static void PlayPostfix(VFXLateTimeParticles __instance)
            { //  fix heatblade underwater particles play inside
                if (__instance.name != "xHeatBlade_Bubbles(Clone)")
                    return;

                heatBladeParticles = __instance.psChildren;
                FixHeatBlade();
            }
        }

        public static void FixHeatBlade()
        { //  fix heatblade underwater particles 
            if (heatBladeParticles == null || heatBladeParticles.Length != 3 || heatBladeParticles[0] == null || heatBladeParticles[0].gameObject == null || !heatBladeParticles[0].gameObject.activeInHierarchy)
                return;

            //AddDebug("FixHeatBlade");
            bool underwater = Player.main.isUnderwater.value;
            heatBladeParticles[1].EnableEmission(!underwater); // xSmk
            heatBladeParticles[0].EnableEmission(underwater); // xHeatBlade_Bubbles(Clone)
            heatBladeParticles[2].EnableEmission(underwater); // xRefract
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
