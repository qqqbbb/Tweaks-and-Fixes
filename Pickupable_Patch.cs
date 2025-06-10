using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_Patch
    {
        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();
        public static HashSet<TechType> shinies = new HashSet<TechType>();
        public static HashSet<TechType> unmovableItems = new HashSet<TechType>();
        static FMODAsset eatSound;

        [HarmonyPatch(typeof(Pickupable))]
        public class Pickupable_Patch_
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
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

                TechType tt = __instance.GetTechType();
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
                if (shinies.Contains(tt))
                {
                    HardnessMixin hm = __instance.gameObject.EnsureComponent<HardnessMixin>();
                    hm.hardness = 1f;
                    EcoTarget[] ets = __instance.gameObject.GetComponents<EcoTarget>();
                    foreach (EcoTarget et in ets)
                    {
                        if (et.type == EcoTargetType.Shiny)
                            return;
                    }
                    EcoTarget ecoTarget1 = __instance.gameObject.AddComponent<EcoTarget>();
                    ecoTarget1.type = EcoTargetType.Shiny;
                }

            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnHandClick")]
            static void OnHandClickPostfix(Pickupable __instance, GUIHand hand)
            {
                AddDebug("Pickupable OnHandClick");
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnHandHover")]
            public static void OnHandHoverPostfix(Pickupable __instance, GUIHand hand)
            {
                if (!hand.IsFreeToInteract())
                    return;

                //if (!__instance.AllowedToPickUp())
                //    return;

                TechType techType = __instance.GetTechType();
                if (techType == TechType.Beacon)
                {
                    //AddDebug("Beacon ");
                    Beacon beacon = __instance.GetComponent<Beacon>();
                    if (beacon)
                    {
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                            uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));

                        HandReticle.main.SetText(HandReticle.TextType.Hand, UI_Patches.beaconPickString, false);
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, beacon.beaconLabel.labelName, false);
                    }
                }
            }

            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(Pickupable __instance, GUIHand hand)
            {
                if (!hand.IsFreeToInteract())
                    return false;

                //if (!__instance.AllowedToPickUp())
                //    return false;

                Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                if (exosuit)
                {
                    bool hasClawArm = exosuit.leftArmType == TechType.ExosuitClawArmModule || exosuit.rightArmType == TechType.ExosuitClawArmModule;

                    if (!hasClawArm)
                        return false;
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(BeaconLabel))]
        class BeaconLabel_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            static void StartPostfix(BeaconLabel __instance)
            {
                Collider collider = __instance.GetComponent<Collider>();
                if (collider)
                    UnityEngine.Object.Destroy(collider);
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnPickedUp")]
            static bool OnPickedUpPrefix(BeaconLabel __instance)
            {
                return false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnDropped")]
            static bool OnDroppedPrefix(BeaconLabel __instance)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetItemAction")]
        class Inventory_GetUseItemAction_Patch
        {
            static void Postfix(Inventory __instance, ref ItemAction __result, InventoryItem item)
            {
                if (__result == ItemAction.Eat)
                {
                    if (ConfigMenu.cantEatUnderwater.Value && Player.main.IsUnderwater())
                        __result = ItemAction.None;
                }
                else if (__result == ItemAction.Use)
                {
                    TechType tt = item.item.GetTechType();
                    if (tt == TechType.FirstAidKit && ConfigMenu.cantUseMedkitUnderwater.Value && Player.main.IsUnderwater())
                    {
                        __result = ItemAction.None;
                        return;
                    }
                }
            }
        }


    }
}
