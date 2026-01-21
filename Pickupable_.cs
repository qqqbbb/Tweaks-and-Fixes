using HarmonyLib;
using Nautilus.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_
    {
        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();
        public static HashSet<TechType> shinies = new HashSet<TechType>();
        public static HashSet<TechType> unmovableItems = new HashSet<TechType>();
        public static Dictionary<Pickupable, Beacon> beacons = new Dictionary<Pickupable, Beacon>();
        public static Dictionary<Pickupable, StorageContainer> pickupableStorage = new Dictionary<Pickupable, StorageContainer>();
        public static Dictionary<Pickupable, PickupableStorage> pickupableStorage_ = new Dictionary<Pickupable, PickupableStorage>();


        [HarmonyPatch(typeof(Pickupable))]
        public class Pickupable_Patch_
        {
            [HarmonyPostfix, HarmonyPatch("Awake")]
            static void AwakePostfix(Pickupable __instance)
            {
                //AddDebug(" Pickupable Awake " + __instance.name);
                //Rigidbody rb = __instance.GetComponent<Rigidbody>();
                //if (rb)
                //{
                //    rb.useGravity = true;
                //    rb.drag = 1;
                //    rb.angularDrag = 1;
                //}
                if (ConfigToEdit.beaconTweaks.Value)
                {
                    Beacon beacon = __instance.GetComponent<Beacon>();
                    if (beacon)
                        beacons.Add(__instance, beacon);
                }
                TechType tt = __instance.GetTechType();
                if (tt == TechType.SmallStorage || tt == TechType.LuggageBag)
                {
                    if (Main.pickupFullCarryallIsLoaded == false)
                    {
                        PickupableStorage ps = __instance.GetComponentInChildren<PickupableStorage>();
                        if (ps)
                            pickupableStorage_.Add(__instance, ps);

                        StorageContainer sc = __instance.GetComponentInChildren<StorageContainer>();
                        if (sc)
                            pickupableStorage.Add(__instance, sc);
                    }
                }
                if (unmovableItems.Contains(tt))
                { // isKinematic gets saved
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                if (itemMass.ContainsKey(tt))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        //Main.logger.LogMessage(__instance.name + " itemMass " + rb.mass);
                        rb.mass = itemMass[tt];
                    }
                }
                CheckShinyEcoTarget(__instance.gameObject, shinies.Contains(tt));
            }

            private static void CheckShinyEcoTarget(GameObject go, bool addShinyEcoTarget)
            {
                EcoTarget ecoTarget = null;
                foreach (EcoTarget et in go.GetComponents<EcoTarget>())
                {
                    if (et.type == EcoTargetType.Shiny)
                    {
                        //AddDebug(go.name + " is Shiny");
                        ecoTarget = et;
                        break;
                    }
                }
                if (addShinyEcoTarget)
                {
                    HardnessMixin hm = go.EnsureComponent<HardnessMixin>();
                    hm.hardness = 1f;
                    if (ecoTarget == null)
                    {
                        //AddDebug(go.name + " add Shiny EcoTarget");
                        EcoTarget ecoTarget1 = go.AddComponent<EcoTarget>();
                        ecoTarget1.type = EcoTargetType.Shiny;
                    }
                }
                else if (ecoTarget)
                {
                    //AddDebug(go.name + " Destroy Shiny ecotarget");
                    UnityEngine.Object.Destroy(ecoTarget);
                    HardnessMixin hm = go.GetComponent<HardnessMixin>();
                    if (hm)
                        UnityEngine.Object.Destroy(hm);
                }
            }

            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(Pickupable __instance, GUIHand hand)
            {
                //AddDebug(__instance.name + " Pickupable OnHandHover AllowedToPickUp " + __instance.AllowedToPickUp());

                if (!hand.IsFreeToInteract())
                    return false;

                if (!__instance.AllowedToPickUp())
                    return false;

                Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
                if (exosuit)
                {
                    bool hasClawArm = exosuit.leftArmType == TechType.ExosuitClawArmModule || exosuit.rightArmType == TechType.ExosuitClawArmModule;
                    // fix bug: button prompt shown below crosshair when prop arm equipped
                    if (!hasClawArm)
                    {
                        return false;
                    }
                }
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("OnHandHover")]
            public static void OnHandHoverPostfix(Pickupable __instance, GUIHand hand)
            {
                if (!hand.IsFreeToInteract())
                    return;

                if (ConfigToEdit.beaconTweaks.Value && beacons.ContainsKey(__instance))
                {
                    Beacon beacon = beacons[__instance];
                    HandReticle.main.SetText(HandReticle.TextType.Hand, UI_Patches.beaconPickString, false);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, beacon.beaconLabel.labelName, false);
                    if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));
                }
                if (Main.pickupFullCarryallIsLoaded == false && ConfigToEdit.canPickUpContainerWithItems.Value == false && pickupableStorage_.ContainsKey(__instance) && Player.main.currentMountedVehicle is Exosuit)
                {
                    //AddDebug(__instance.name + " Pickupable OnHandHover AllowedToPickUp " + __instance.AllowedToPickUp());
                    if (__instance.AllowedToPickUp() == false)
                    {
                        PickupableStorage ps = pickupableStorage_[__instance];
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, Language.main.Get(ps.cantPickupClickText), true);
                    }
                }
            }

            //[HarmonyPrefix, HarmonyPatch("OnHandClick")]
            public static bool OnHandClickPrefix(Pickupable __instance, GUIHand hand)
            {
                //AddDebug(__instance.name + " Pickupable OnHandClick AllowedToPickUp " + __instance.AllowedToPickUp());
                return false;
            }
            [HarmonyPostfix, HarmonyPatch("AllowedToPickUp")]
            public static void AllowedToPickUpPostfix(Pickupable __instance, ref bool __result)
            {
                if (Main.pickupFullCarryallIsLoaded == false && pickupableStorage.ContainsKey(__instance))
                { // fix bug: exosuit can pick up containers with items
                    if (ConfigToEdit.canPickUpContainerWithItems.Value)
                        __result = true;
                    else
                        __result = pickupableStorage[__instance].container.IsEmpty();
                    //AddDebug(__instance.name + " Pickupable AllowedToPickUp " + __result);
                    return;
                }
                if (ConfigMenu.noFishCatching.Value && Player.main._currentWaterPark == null && Util.IsEatableFish(__instance.gameObject) && Util.IsDead(__instance.gameObject) == false)
                {
                    __result = false;
                }
                PropulsionCannonWeapon pc = Inventory.main.GetHeldTool() as PropulsionCannonWeapon;
                if (pc && pc.propulsionCannon.grabbedObject == __instance.gameObject)
                {
                    //AddDebug("PropulsionCannonWeapon ");
                    __result = true;
                    return;
                }
                foreach (Pickupable p in Gravsphere_.gravSphereFish)
                {
                    if (p == __instance)
                    {
                        //AddDebug("Gravsphere ");
                        __result = true;
                        return;
                    }
                }
                Rigidbody rigidbody = __instance.GetComponent<Rigidbody>();
                if (rigidbody == null)
                    return;

                foreach (Rigidbody rb in Tools.stasisTargets)
                {
                    if (rigidbody == rb)
                    {
                        __result = true;
                    }
                }
            }

            [HarmonyPostfix, HarmonyPatch("Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
            static void DropPostfix(Pickupable __instance)
            { // collider that is not trigger gets destroyed in Storage_Patch.StorageContainer_Patch.CreateContainerPostfix
                if (__instance.GetTechType() == TechType.LuggageBag && Main.pickupFullCarryallIsLoaded == false)
                {
                    //AddDebug(" Pickupable Drop " + __instance.name);
                    BoxCollider boxCollider = __instance.GetComponentInChildren<BoxCollider>();
                    boxCollider.isTrigger = false;
                }
            }
            [HarmonyPostfix, HarmonyPatch("Pickup")]
            static void PickupPostfix(Pickupable __instance)
            {// collider that is not trigger gets destroyed in Storage_Patch.StorageContainer_Patch.CreateContainerPostfix
                if (__instance.GetTechType() == TechType.LuggageBag && Main.pickupFullCarryallIsLoaded == false)
                {
                    //AddDebug(" Pickupable Pickup " + __instance.name);
                    BoxCollider boxCollider = __instance.GetComponentInChildren<BoxCollider>();
                    boxCollider.isTrigger = true;
                }
            }
        }

        [HarmonyPatch(typeof(ExosuitClawArm))]
        public static class ExosuitClawArm_Patch
        {
            [HarmonyPrefix, HarmonyPatch("OnPickup")]
            public static bool OnPickupPrefix(ExosuitClawArm __instance)
            {
                //AddDebug("ExosuitClawArm OnPickup");
                if (ConfigToEdit.canPickUpContainerWithItems.Value || Main.pickupFullCarryallIsLoaded)
                    return true;

                GameObject target = __instance.exosuit.GetActiveTarget();
                if (target)
                {
                    Pickupable p = target.GetComponent<Pickupable>();
                    if (p && pickupableStorage.ContainsKey(p))
                    {
                        bool empty = pickupableStorage[p].IsEmpty();
                        //AddDebug("ExosuitClawArm OnPickup pickupableStorage " + empty);
                        return empty;
                    }
                }
                return true;
            }
        }




    }
}
