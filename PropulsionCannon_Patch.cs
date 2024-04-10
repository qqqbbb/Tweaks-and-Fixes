using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;
using static HandReticle;
using UnityEngine.AddressableAssets;
using static TechStringCache;
using System.Collections;
using static VFXParticlesPool;

namespace Tweaks_Fixes
{
    internal class PropulsionCannon_Patch
    {
        static bool grabbingResource;
        static string releaseString;
        static string grabbedObjectPickupText;
        static GameObject targetObject;

        [HarmonyPatch(typeof(PropulsionCannonWeapon))]
        class PropulsionCannonWeapon_patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetCustomUseText")]
            public static bool StartPostfix(PropulsionCannonWeapon __instance, ref string __result)
            {
                bool isGrabbingObject = __instance.propulsionCannon.IsGrabbingObject();
                bool hasChargeForShot = __instance.propulsionCannon.HasChargeForShot();
                if (__instance.usingPlayer == null || __instance.usingPlayer.IsInSub() || !(isGrabbingObject | hasChargeForShot))
                {
                    __result = string.Empty;
                    return false;
                }
                string buttonFormat1;
                string buttonFormat2;
                if (isGrabbingObject)
                {
                    buttonFormat1 = grabbedObjectPickupText;
                    buttonFormat2 = releaseString;
                }
                else
                {
                    if (targetObject)
                        buttonFormat1 = LanguageCache.GetButtonFormat("PropulsionCannonToGrab", GameInput.Button.RightHand) + ", ";
                    else
                        buttonFormat1 = "";

                    buttonFormat2 = LanguageCache.GetButtonFormat("PropulsionCannonToLoad", GameInput.Button.AltTool);
                }
                if (buttonFormat1 != __instance.cachedPrimaryUseText || buttonFormat2 != __instance.cachedAltUseText)
                {
                    __instance.cachedCustomUseText = buttonFormat1 + buttonFormat2;
                    __instance.cachedPrimaryUseText = buttonFormat1;
                    __instance.cachedAltUseText = buttonFormat2;
                }
                __result = __instance.cachedCustomUseText;
                return false;
            }

        }

        [HarmonyPatch(typeof(PropulsionCannon))]
        class PropulsionCannon_Patch_
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

            [HarmonyPostfix]
            [HarmonyPatch("TraceForGrabTarget")]
            static void TraceForGrabTargetPostfix(PropulsionCannon __instance, GameObject __result)
            {
                targetObject = __result;
            }

            private static IEnumerator SpawnResource(PropulsionCannon cannon, BreakableResource resource)
            {
                yield return new WaitForSeconds(UnityEngine.Random.value);
                cannon.grabbedObject = null;
                grabbingResource = false;
                resource.BreakIntoResources();
            }

            [HarmonyPrefix]
            [HarmonyPatch("UpdateTargetPosition")]
            static bool UpdateTargetPositionPrefix(PropulsionCannon __instance)
            {
                if (grabbingResource && __instance.grabbedObject)
                {
                    //AddDebug("UpdateTargetPosition grabbingResource");
                    __instance.targetPosition = __instance.grabbedObject.transform.position;
                    UnityEngine.Bounds aabb = __instance.GetAABB(__instance.grabbedObject);
                    __instance.grabbedObjectCenter = aabb.center;
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnShoot")]
            static bool OnShootPrefix(PropulsionCannon __instance)
            {
                if (grabbingResource)
                {
                    //AddDebug("PropulsionCannon OnShoot grabbingResource");
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GrabObject")]
            static bool GrabObjectPrefix(PropulsionCannon __instance, ref GameObject target)
            {
                TechType tt = CraftData.GetTechType(target);
                if (tt == TechType.GenericJeweledDisk)
                {
                    SpawnOnKill spawnOnKill = target.GetComponent<SpawnOnKill>();
                    target = UnityEngine.Object.Instantiate(spawnOnKill.prefabToSpawn, spawnOnKill.transform.position, spawnOnKill.transform.rotation);
                    UnityEngine.Object.Destroy(spawnOnKill.gameObject);
                }
                else if (tt == TechType.LimestoneChunk || tt == TechType.SandstoneChunk || tt == TechType.ShaleChunk)
                {
                    grabbingResource = true;
                    BreakableResource resource = target.GetComponent<BreakableResource>();
                    UWE.CoroutineHost.StartCoroutine(SpawnResource(__instance, resource));
                }
                return true;
            }
          
            [HarmonyPostfix]
            [HarmonyPatch("GrabObject")]
            static void GrabObjectPostfix(PropulsionCannon __instance, GameObject target)
            {
                if (!ConfigToEdit.newUIstrings.Value || __instance.grabbedObject == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.grabbedObject);
                //grabbedObjectName = Language.main.Get(tt.ToString());
                releaseString = Language.main.Get("TF_propulsion_cannon_release") + "(" + UI_Patches.altToolButton + ")";
                Pickupable pickupable = target.GetComponent<Pickupable>();
                if (pickupable != null && Inventory.main._container.HasRoomFor(pickupable))
                {
                    grabbedObjectPickupText = LanguageCache.GetPickupText(tt) + " (" + UI_Patches.leftHandButton + "), ";
                }
                else
                    grabbedObjectPickupText = "";

                grabbedObjectPickupText += LanguageCache.GetButtonFormat("PropulsionCannonToShoot", GameInput.Button.RightHand);
                grabbedObjectPickupText += ", ";
            }

            [HarmonyPrefix]
            [HarmonyPatch("grabbedObject", MethodType.Setter)]
            public static bool Prefix(PropulsionCannon __instance, GameObject value)
            {
                __instance._grabbedObject = value;
                InventoryItem storedItem = __instance.storageSlot.storedItem;
                Pickupable pickupable1 = storedItem == null ? null : storedItem.item;
                Pickupable pickupable2 = __instance._grabbedObject == null ? null : __instance._grabbedObject.GetComponent<Pickupable>();
                if (pickupable1 != null)
                {
                    if (pickupable2 != null)
                    {
                        if (pickupable1 != pickupable2)
                        {
                            __instance.storageSlot.RemoveItem();
                            __instance.storageSlot.AddItem(new InventoryItem(pickupable2));
                        }
                    }
                    else
                        __instance.storageSlot.RemoveItem();
                }
                else if (pickupable2 != null)
                    __instance.storageSlot.AddItem(new InventoryItem(pickupable2));

                if (__instance._grabbedObject != null)
                {
                    __instance.grabbingSound.Play();
                    __instance.fxBeam.SetActive(true);
                    if (ConfigToEdit.propulsionCannonGrabFX.Value && !grabbingResource)
                    {
                        __instance.grabbedEffect.SetActive(true);
                        __instance.grabbedEffect.transform.parent = null;
                        __instance.grabbedEffect.transform.position = __instance._grabbedObject.transform.position;
                    }
                    __instance.timeGrabbed = Time.time;
                    __instance.UpdateTargetPosition();
                }
                else
                {
                    __instance.grabbingSound.Stop();
                    __instance.grabbedEffect.SetActive(false);
                    __instance.fxBeam.SetActive(false);
                    __instance.grabbedEffect.transform.parent = __instance.transform;
                }
                if (MainGameController.Instance == null)
                    return false;

                if (__instance._grabbedObject != null)
                    MainGameController.Instance.RegisterHighFixedTimestepBehavior(__instance);
                else
                    MainGameController.Instance.DeregisterHighFixedTimestepBehavior(__instance);
                
                return false;
            }
        }
    }
}
