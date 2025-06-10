using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

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
        static HashSet<TechType> bannedTechTypes = new HashSet<TechType> { TechType.HangingStinger };

        private static IEnumerator SpawnResource(PropulsionCannon cannon, BreakableResource resource)
        {
            yield return new WaitForSeconds(.1f + UnityEngine.Random.value);
            cannon.ReleaseGrabbedObject();
            grabbingResource = false;
            resource.BreakIntoResources();
        }

        private static IEnumerator AddToInventoryRoutine(PickPrefab pickPrefab)
        {
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.AddToInventoryAsync(pickPrefab.pickTech, (IOut<GameObject>)result, spawnIfCantAdd: false);
            if (result.Get())
                pickPrefab.SetPickedUp();

            pickPrefab.isAddingToInventory = false;
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
                //AddDebug("SpawnFruitAsync fruit");
                if (pickPrefab.pickTech == TechType.CreepvineSeedCluster)
                { // they spawn close to ground, some below ground
                    fruit.transform.position = new Vector3(pickPrefab.transform.position.x, cannon.transform.position.y, pickPrefab.transform.position.z);
                }
                else
                    fruit.transform.position = pickPrefab.transform.position;

                //AddDebug("spawned fruit " + pickPrefab.pickTech);
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
            //AddDebug("GetFruit " + fruitPlant.fruits.Length);
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
            [HarmonyPrefix, HarmonyPatch("GetCustomUseText")]
            public static bool GetCustomUseTextPostfix(PropulsionCannonWeapon __instance, ref string __result)
            {
                if (ConfigToEdit.propulsionCannonTweaks.Value == false && ConfigToEdit.dropItemsAnywhere.Value == false)
                    return true;

                bool isGrabbingObject = __instance.propulsionCannon.IsGrabbingObject();
                bool hasChargeForShot = __instance.propulsionCannon.HasChargeForShot();
                bool inSub = __instance.usingPlayer.currentSub;
                bool checkInSub = inSub && ConfigToEdit.dropItemsAnywhere.Value && (targetObject || isGrabbingObject);
                //AddDebug("GetCustomUseText isGrabbingObject " + isGrabbingObject);
                //AddDebug("GetCustomUseText releaseString " + releaseString);
                if (inSub || __instance.usingPlayer == null || !(isGrabbingObject | hasChargeForShot))
                {
                    if (!checkInSub)
                    {
                        __result = string.Empty;
                        return false;
                    }
                }
                //AddDebug("GetCustomUseText  ");

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
                    {
                        //AddDebug("GetCustomUseText targetObject " + targetObject.name);
                        sb.Append(LanguageCache.GetButtonFormat("PropulsionCannonToGrab", GameInput.Button.RightHand) + ", ");
                        //AddDebug("GetCustomUseText GetButtonFormat PropulsionCannonToGrab: " + LanguageCache.GetButtonFormat("PropulsionCannonToGrab", GameInput.Button.RightHand));
                    }
                    if (__instance.usingPlayer.currentSub == null)
                        sb.Append(LanguageCache.GetButtonFormat("PropulsionCannonToLoad", GameInput.Button.AltTool));
                }
                string finalText = sb.ToString();
                if (finalText != __instance.cachedPrimaryUseText)
                    __instance.cachedCustomUseText = finalText;

                __result = __instance.cachedCustomUseText;
                //AddDebug("GetCustomUseText 111 ");
                return false;
            }

            [HarmonyPostfix, HarmonyPatch("UpdateEquipped")]
            public static void OnRightHandDownPostfix(PropulsionCannonWeapon __instance, GameObject sender, string slot)
            {
                if (Player.main.currentSub && ConfigToEdit.dropItemsAnywhere.Value)
                {
                    __instance.propulsionCannon.usingCannon = GameInput.GetButtonHeld(GameInput.Button.RightHand);
                    __instance.propulsionCannon.UpdateActive();
                    SafeAnimator.SetBool(Player.main.armsController.GetComponent<Animator>(), "cangrab_propulsioncannon", __instance.propulsionCannon.canGrab || __instance.propulsionCannon.grabbedObject != null);
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnRightHandDown")]
            public static void OnRightHandDownPostfix(PropulsionCannonWeapon __instance, ref bool __result)
            {
                if (Player.main.currentSub && ConfigToEdit.dropItemsAnywhere.Value && !__instance.propulsionCannon.grabbedObject)
                {
                    //AddDebug("PropulsionCannonWeapon OnRightHandDown");
                    if (__instance.firstUseAnimationStarted)
                        __instance.OnFirstUseAnimationStop();

                    __result = __instance.propulsionCannon.OnShoot();
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnAltDown")]
            public static void OnAltDownPostfix(PropulsionCannonWeapon __instance, ref bool __result)
            {
                if (Player.main.currentSub && ConfigToEdit.dropItemsAnywhere.Value)
                {
                    //AddDebug("PropulsionCannonWeapon OnAltDown");
                    if (__instance.firstUseAnimationStarted)
                        __instance.OnFirstUseAnimationStop();

                    if (__instance.propulsionCannon.grabbedObject)
                        __instance.propulsionCannon.ReleaseGrabbedObject();
                    //else if (__instance.propulsionCannon.HasChargeForShot())
                    //{
                    //    if (!__instance.propulsionCannon.OnReload(new List<IItemsContainer>() { Inventory.main.container }))
                    //        AddMessage(Language.main.Get("PropulsionCannonNoItems"));
                    //}
                }
            }


        }

        [HarmonyPatch(typeof(PropulsionCannon))]
        class PropulsionCannon_Patch_
        {
            [HarmonyPostfix, HarmonyPatch("ValidateNewObject")]
            static void ValidateNewObjectPostfix(PropulsionCannon __instance, GameObject go, ref bool __result)
            {
                TechType tt = CraftData.GetTechType(go);
                if (bannedTechTypes.Contains(tt))
                {
                    __result = false;
                }
                //AddDebug($"ValidateNewObject {tt}");
            }

            [HarmonyPrefix, HarmonyPatch("TraceForGrabTarget")]
            static bool TraceForGrabTargetPrefix(PropulsionCannon __instance, ref GameObject __result)
            {
                if (spawningFruit)
                    return false;

                //AddDebug("TraceForGrabTarget Prefix");
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("TraceForGrabTarget")]
            static void TraceForGrabTargetPostfix(PropulsionCannon __instance, ref GameObject __result)
            {
                if (ConfigToEdit.propulsionCannonTweaks.Value == false || spawningFruit)
                    return;

                targetObject = null;
                //if (__result)
                //    AddDebug("TraceForGrabTarget default " + __result.name);

                //if (__result && __result.GetComponent<PlaceTool>())
                //{
                //AddDebug("TraceForGrabTargetPostfix placeTool ");
                //        __result = null;
                //}
                Targeting.GetTarget(Player.main.gameObject, __instance.pickupDistance, out GameObject target, out float targetDist);
                //RaycastHit hitInfo = new RaycastHit();
                //bool gotTarget = Util.GetPlayerTarget(__instance.pickupDistance, out hitInfo);
                //AddDebug("TraceForGrabTarget  " + target.name);
                if (!target)
                    return;
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
                    //AddDebug("TraceForGrabTargetPostfix PickPrefab " + pickPrefab.pickTech);
                    PickPrefab pp = null;
                    if (go.name == "farming_plant_02(Clone)")
                    { // picking PickPrefab on farmibg_plant_02 root GO destroys the plant
                        Transform t = go.transform.Find("farming_plant_02");
                        if (t != null)
                            pp = t.GetComponentInChildren<PickPrefab>();
                    }
                    //AddDebug("TraceForGrabTargetPostfix PickPrefab");
                    if (pp)
                        fruitToPickUp = pp;
                    else
                        fruitToPickUp = pickPrefab;

                    __result = go;
                    //UWE.CoroutineHost.StartCoroutine(SpawnFruitAsync(pickPrefab));
                }
                else if (fruitPlant)
                {
                    //AddDebug("TraceForGrabTargetPostfix fruitPlant " + go.name);
                    fruitToPickUp = GetFruit(fruitPlant);
                    __result = fruitToPickUp ? fruitToPickUp.gameObject : null;
                }
                else
                {
                    fruitToPickUp = null;
                    //AddDebug("TraceForGrabTargetPostfix no fruitPlant no pickPrefab");
                }
                targetObject = __result;
            }

            [HarmonyPrefix, HarmonyPatch("UpdateTargetPosition")]
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

            [HarmonyPrefix, HarmonyPatch("OnShoot")]
            static bool OnShootPrefix(PropulsionCannon __instance)
            {
                if (grabbingResource)
                {
                    //AddDebug("PropulsionCannon OnShoot grabbingResource");
                    return false;
                }
                return true;
            }

            [HarmonyPrefix, HarmonyPatch("GrabObject")]
            static bool GrabObjectPrefix(PropulsionCannon __instance, ref GameObject target)
            {
                if (ConfigToEdit.propulsionCannonTweaks.Value == false)
                    return true;

                if (spawningFruit)
                    return false;

                if (fruitToPickUp)
                {
                    //AddDebug("SpawnFruitAsync position " + (int)fruitToPickUp.transform.position.y);
                    //AddDebug("player pos  " + (int)Player.main.transform.position.y);
                    UWE.CoroutineHost.StartCoroutine(SpawnFruitAsync(fruitToPickUp, __instance));
                    //UWE.CoroutineHost.StartCoroutine(AddToInventoryRoutine(fruitToPickUp));
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

            [HarmonyPostfix, HarmonyPatch("GrabObject")]
            static void GrabObjectPostfix(PropulsionCannon __instance, GameObject target)
            {
                if (ConfigToEdit.propulsionCannonTweaks.Value == false || __instance.grabbedObject == null)
                    return;

                //TechType tt = CraftData.GetTechType(__instance.grabbedObject);
                //grabbedObjectName = Language.main.Get(tt.ToString());
                releaseString = Language.main.Get("TF_propulsion_cannon_release") + "(" + UI_Patches.altToolButton + ")";
                //AddDebug("GrabObject releaseString " + releaseString);

                grabbedEatable = target.GetComponent<Eatable>();
                Pickupable pickupable = target.GetComponent<Pickupable>();
                if (pickupable && target.GetComponent<PlaceTool>())
                {
                    //AddDebug("PropulsionCannon GrabObject placeTool ");
                    pickupable.Unplace();
                }
                if (pickupable != null && Inventory.main._container.HasRoomFor(pickupable))
                {
                    //grabbedObjectPickupText = LanguageCache.GetPickupText(tt) + " (" + UI_Patches.leftHandButton + "), ";
                    grabbedObjectPickupText = UI_Patches.pickupString + " (" + UI_Patches.leftHandButton + "), ";
                }
                else
                    grabbedObjectPickupText = "";

                if (Player.main.currentSub == null)
                    grabbedObjectPickupText += LanguageCache.GetButtonFormat("PropulsionCannonToShoot", GameInput.Button.RightHand);

                grabbedObjectPickupText += ", ";
                //if (ConfigToEdit.dropItemsAnywhere.Value)
                //{ // they drop below floor when released
                //    WorldForces wf = target.GetComponent<WorldForces>();
                //    if (wf)
                //        wf.handleGravity = true;
                //}
            }

            [HarmonyPrefix, HarmonyPatch("grabbedObject", MethodType.Setter)]
            public static bool Prefix(PropulsionCannon __instance, GameObject value)
            {
                if (ConfigToEdit.propulsionCannonTweaks.Value == false)
                    return true;

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
