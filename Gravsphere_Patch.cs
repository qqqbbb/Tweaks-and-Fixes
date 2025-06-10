using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{   // not tested with more than 1 grav trap
    [HarmonyPatch(typeof(Gravsphere))]
    public class Gravsphere_Patch
    {
        static public HashSet<Gravsphere> gravSpheres = new HashSet<Gravsphere>();
        static public HashSet<Pickupable> gravSphereFish = new HashSet<Pickupable>();
        static public HashSet<GasPod> gasPods = new HashSet<GasPod>();
        static public HashSet<TechType> gravTrappable = new HashSet<TechType>();

        [HarmonyPrefix, HarmonyPatch("OnDropped")]
        public static void OnDropped(Gravsphere __instance)
        {
            AddDebug("OnDropped  ");
            gravSpheres.Add(__instance);
        }

        [HarmonyPostfix, HarmonyPatch("OnPickedUp")]
        public static void OnPickedUp(Gravsphere __instance)
        {
            AddDebug("OnPickedUp  ");
            gravSpheres.Remove(__instance);
        }

        [HarmonyPostfix, HarmonyPatch(nameof(Gravsphere.IsValidTarget))]
        public static void IsValidTarget(Gravsphere __instance, GameObject obj, ref bool __result)
        {
            if (__result)
                return;

            TechType t = CraftData.GetTechType(obj);
            if (t != TechType.None && gravTrappable.Contains(t))
                __result = true;
        }

        [HarmonyPostfix, HarmonyPatch(nameof(Gravsphere.AddAttractable))]
        public static void AddAttractable(Gravsphere __instance, Rigidbody r)
        {
            GasPod gp = r.gameObject.GetComponent<GasPod>();
            if (gp)
            {
                gp.grabbedByPropCannon = true;
                gasPods.Add(gp);
            }
            else
            {
                Pickupable p = r.GetComponent<Pickupable>();
                //AddDebug("AddAttractable ");
                if (p)
                {
                    gravSphereFish.Add(p);
                }

            }
        }

        [HarmonyPostfix, HarmonyPatch(nameof(Gravsphere.ClearAll))]
        public static void ClearAll(Gravsphere __instance)
        {
            //AddDebug("ClearAll ");
            foreach (GasPod gp in gasPods)
            {
                gp.grabbedByPropCannon = false;
            }
            gasPods.Clear();
            gravSphereFish.Clear();
        }

        [HarmonyPrefix, HarmonyPatch("OnTriggerEnter")]
        public static bool OnTriggerEnter(Gravsphere __instance, Collider collider)
        {
            InventoryItem item = Inventory.main.quickSlots.heldItem;
            if (item != null && item.item.transform.root.gameObject == collider.transform.root.gameObject)
            {
                //AddDebug("OnTriggerEnter heldItem ");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Pickupable), "Pickup")]
        class Pickupable_Pickup_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (gravSphereFish.Contains(__instance))
                {
                    foreach (var gravSphere in gravSpheres)
                    {
                        int num = gravSphere.attractableList.IndexOf(__instance.GetComponent<Rigidbody>());
                        if (num == -1)
                            return;
                        //AddDebug("Pick up gravSphere");
                        gravSphere.removeList.Add(num);
                    }
                }
            }
        }

    }
}
