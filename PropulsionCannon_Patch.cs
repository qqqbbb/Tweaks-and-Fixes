using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;
using static HandReticle;
using UnityEngine.AddressableAssets;
using System.Collections;

namespace Tweaks_Fixes
{
    internal class PropulsionCannon_Patch
    {
        static bool grabbingResource;
        static string releaseString;
        static string grabbedObjectPickupText;
        static GameObject targetObject;
        static Eatable grabbedEatable;
        static bool spawningFruit;
        static PickPrefab fruitToPickUp;

        private static IEnumerator SpawnResource(PropulsionCannon cannon, BreakableResource resource)
        {
            yield return new WaitForSeconds(.1f + UnityEngine.Random.value);
            cannon.ReleaseGrabbedObject();
            grabbingResource = false;
            resource.BreakIntoResources();
        }
      
        private static IEnumerator SpawnFruitAsync(PickPrefab pickPrefab, PropulsionCannon cannon)
        {
            if (!pickPrefab.gameObject.activeInHierarchy || pickPrefab.isAddingToInventory)
                yield break;

            spawningFruit = true;
            pickPrefab.isAddingToInventory = true;
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(pickPrefab.pickTech, result);
            GameObject fruit = result.Get();
            if (fruit)
            {
                fruit.transform.position = pickPrefab.transform.position;
                //AddDebug("spawned fruit from pickPrefab " + fruit.transform.position);
                pickPrefab.SetPickedUp();
                spawningFruit = false;
                cannon.GrabObject(fruit);
            }
            pickPrefab.isAddingToInventory = false;
        }

        private static IEnumerator SpawnCoralDiskAsync(PropulsionCannon cannon, Vector3 position)
        {
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(TechType.JeweledDiskPiece, result);
            GameObject disk = result.Get();
            if (disk)
            {
                disk.transform.position = position;
                //AddDebug("spawned Coral Disk ");
                cannon.GrabObject(disk);
            }
        }

        private static PickPrefab GetFruit(FruitPlant fruitPlant)
        {
            foreach (PickPrefab pickPrefab in fruitPlant.fruits)
            {
                if (!pickPrefab.pickedState)
                    return pickPrefab;
            }
            return null;
            //int randomFruitIndex = UnityEngine.Random.Range(0, fruitPlant.fruits.Length);
            //fruitToPickUp = fruitPlant.fruits[randomFruitIndex];
        }

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
                StringBuilder sb = new StringBuilder();
                if (isGrabbingObject)
                {
                    if (Util.CanPlayerEat() && grabbedEatable)
                    {
                        //AddDebug("GetCustomUseText grabbedEatable");
                        sb.Append(UI_Patches.propCannonEatString + ", ");
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        {
                            __instance.propulsionCannon.ReleaseGrabbedObject();
                            Main.survival.Eat(grabbedEatable.gameObject);
                            UnityEngine.Object.Destroy(grabbedEatable.gameObject);
                        }
                    }
                    sb.Append(grabbedObjectPickupText);
                    sb.Append(releaseString);
                }
                else
                {
                    if (targetObject)
                        sb.Append(LanguageCache.GetButtonFormat("PropulsionCannonToGrab", GameInput.Button.RightHand) + ", ");

                    sb.Append(LanguageCache.GetButtonFormat("PropulsionCannonToLoad", GameInput.Button.AltTool));
                }
                string finalText = sb.ToString();
                if (finalText != __instance.cachedPrimaryUseText)
                    __instance.cachedCustomUseText = finalText;
                
                __result = __instance.cachedCustomUseText;
                return false;
            }

        }

        [HarmonyPatch(typeof(PropulsionCannon))]
        class PropulsionCannon_Patch_
        {
            [HarmonyPrefix]
            [HarmonyPatch("TraceForGrabTarget")]
            static bool TraceForGrabTargetPrefix(PropulsionCannon __instance, ref GameObject __result)
            {
                if (spawningFruit)
                    return false;

                return true;
            }


            [HarmonyPostfix]
            [HarmonyPatch("TraceForGrabTarget")]
            static void TraceForGrabTargetPostfix(PropulsionCannon __instance, ref GameObject __result)
            {
                if (spawningFruit)
                    return;

                //if (__result)
                //    AddDebug("TraceForGrabTarget default " + __result.name);

                Targeting.GetTarget(Player.main.gameObject, __instance.pickupDistance, out GameObject target, out float targetDist);
                //RaycastHit hitInfo = new RaycastHit();
                //bool gotTarget = Util.GetPlayerTarget(__instance.pickupDistance, out hitInfo);
                //AddDebug("TraceForGrabTarget gotTarget " + gotTarget);
                if (!target)
                {
                    targetObject = null;
                    return;
                }
                //Transform parent = target.transform.parent;
                //AddDebug("TraceForGrabTarget GetTarget " + target.name);
                Transform parent = target.transform.parent;
                if (parent && parent.parent && parent.parent.name == "TreaderShale(Clone)")
                { // has no UniqueIdentifier
                    //AddDebug("TraceForGrabTarget TreaderShale ");
                    __result = parent.parent.gameObject;
                    return;
                }
                GameObject go = Util.GetEntityRoot(target);
                if (go == null)
                {
                    //AddDebug("no UniqueIdentifier ");
                    return;
                }
                //AddDebug("TraceForGrabTargetPostfix target " + go.name);
                FruitPlant fruitPlant = go.GetComponent<FruitPlant>();
                //if (fruitPlant != null)
                //    AddDebug("TraceForGrabTargetPostfix fruitPlant ");

                PickPrefab pickPrefab = go.GetComponent<PickPrefab>();
                if (pickPrefab)
                {
                    //AddDebug("TraceForGrabTargetPostfix PickPrefab");
                    fruitToPickUp = pickPrefab;
                    __result = go;
                    //UWE.CoroutineHost.StartCoroutine(SpawnFruitAsync(pickPrefab));
                }
                else if (fruitPlant)
                {
                    //AddDebug("TraceForGrabTargetPostfix fruitPlant");
                    fruitToPickUp = GetFruit(fruitPlant);
                    __result = fruitToPickUp ? fruitToPickUp.gameObject : null;
                }
                else
                    fruitToPickUp = null;

                //if (fruitToPickUp)
                //    AddDebug("TraceForGrabTarget pickPrefab ");

                targetObject = __result;
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
                if (spawningFruit)
                    return false;

                if (fruitToPickUp)
                {
                    //AddDebug("StartCoroutine SpawnFruitAsync ");
                    UWE.CoroutineHost.StartCoroutine(SpawnFruitAsync(fruitToPickUp, __instance));
                    fruitToPickUp = null;
                    return false;
                }
                TechType tt = CraftData.GetTechType(target);
                if (tt == TechType.GenericJeweledDisk)
                {
                    SpawnOnKill spawnOnKill = target.GetComponent<SpawnOnKill>();
                    target = UnityEngine.Object.Instantiate(spawnOnKill.prefabToSpawn, spawnOnKill.transform.position, spawnOnKill.transform.rotation);
                    UnityEngine.Object.Destroy(spawnOnKill.gameObject);
                }
                else if (tt == TechType.CoralShellPlate)
                {
                    UWE.CoroutineHost.StartCoroutine(SpawnCoralDiskAsync(__instance, target.transform.position));
                    UnityEngine.Object.Destroy(target);
                    return false;
                }
                else if (tt == TechType.LimestoneChunk || tt == TechType.SandstoneChunk || tt == TechType.ShaleChunk || target.name == "TreaderShale(Clone)")
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

                //TechType tt = CraftData.GetTechType(__instance.grabbedObject);
                //grabbedObjectName = Language.main.Get(tt.ToString());
                releaseString = Language.main.Get("TF_propulsion_cannon_release") + "(" + UI_Patches.altToolButton + ")";
                grabbedEatable = target.GetComponent<Eatable>();
                Pickupable pickupable = target.GetComponent<Pickupable>();
                if (pickupable != null && Inventory.main._container.HasRoomFor(pickupable))
                {
                    //grabbedObjectPickupText = LanguageCache.GetPickupText(tt) + " (" + UI_Patches.leftHandButton + "), ";
                    grabbedObjectPickupText = UI_Patches.pickupString + " (" + UI_Patches.leftHandButton + "), ";
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
