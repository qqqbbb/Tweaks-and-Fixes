using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

            //[HarmonyPostfix, HarmonyPatch("OnHandClick")]
            static void OnHandClickPostfix(Pickupable __instance, GUIHand hand)
            {
                AddDebug("Pickupable OnHandClick");
            }

            [HarmonyPostfix, HarmonyPatch("OnHandHover")]
            public static void OnHandHoverPostfix(Pickupable __instance, GUIHand hand)
            {
                if (!hand.IsFreeToInteract())
                    return;

                //if (!__instance.AllowedToPickUp())
                //    return;
                if (ConfigToEdit.beaconTweaks.Value && beacons.ContainsKey(__instance))
                {
                    Beacon beacon = beacons[__instance];
                    HandReticle.main.SetText(HandReticle.TextType.Hand, UI_Patches.beaconPickString, false);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, beacon.beaconLabel.labelName, false);
                    if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));
                }
            }

            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(Pickupable __instance, GUIHand hand)
            {
                if (!hand.IsFreeToInteract())
                    return false;

                //if (!__instance.AllowedToPickUp())
                //    return false;
                if (Player.main.currentMountedVehicle == null)
                    return true;

                Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                if (exosuit)
                {
                    bool hasClawArm = exosuit.leftArmType == TechType.ExosuitClawArmModule || exosuit.rightArmType == TechType.ExosuitClawArmModule;
                    // fix bug: button prompt shown below crosshair when prop arm equipped
                    if (!hasClawArm)
                        return false;
                }
                return true;
            }
        }



    }
}
