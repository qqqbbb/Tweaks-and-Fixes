using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{   // not tested with more than 1 grav trap
    [HarmonyPatch(typeof(GasPod))]
    class GasPod_Patch
    {
        [HarmonyPatch(nameof(GasPod.OnDrop))]
        public static void Prefix(GasPod __instance)
        {
            if (Gravsphere_Patch.gasPods.Contains(__instance))
            {
                //ErrorMessage.AddDebug("GasPod.OnDrop ");
                Gravsphere_Patch.gasPods.Remove(__instance);
                __instance.grabbedByPropCannon = false;
            }
        }
    }
    [HarmonyPatch(typeof(Gravsphere))]
    public class Gravsphere_Patch
    {
        static public Gravsphere gravSphere;
        static public HashSet<Pickupable> gravSphereFish = new HashSet<Pickupable>();
        static public HashSet<GasPod> gasPods = new HashSet<GasPod>();
        //static public HashSet<Pickupable> gravSphereFish = new HashSet<Pickupable>();
        static public HashSet<TechType> gravTrappable = new HashSet<TechType>();

        [HarmonyPostfix] [HarmonyPatch(nameof(Gravsphere.IsValidTarget))]
        public static void OnPickedUp(Gravsphere __instance, GameObject obj, ref bool __result)
        {
            if (__result)
                return;

            TechType t = CraftData.GetTechType(obj);

            if (t != TechType.None && gravTrappable.Contains(t))
            {
                __result = true;
            }
        }

        [HarmonyPostfix] [HarmonyPatch(nameof(Gravsphere.AddAttractable))]
        public static void AddAttractable(Gravsphere __instance, Rigidbody r)
        {
            gravSphere = __instance;
            GasPod gp = r.gameObject.GetComponent<GasPod>();
            if (gp)
            {
                gp.grabbedByPropCannon = true;
                gasPods.Add(gp);
            }
            else
            {
                Pickupable p = r.GetComponent<Pickupable>();
                //ErrorMessage.AddDebug("AddAttractable ");
                if (p)
                {
                    gravSphereFish.Add(p);
                }

            }
        }

        [HarmonyPostfix] [HarmonyPatch(nameof(Gravsphere.ClearAll))]
        public static void ClearAll(Gravsphere __instance)
        {
            //ErrorMessage.AddDebug("ClearAll ");
            foreach (GasPod gp in gasPods)
            {
                gp.grabbedByPropCannon = false;
            }
            gasPods = new HashSet<GasPod>();
            gravSphereFish = new HashSet<Pickupable>();
        }

        [HarmonyPrefix] [HarmonyPatch(nameof(Gravsphere.OnTriggerEnter))]
        public static bool OnTriggerEnter(Gravsphere __instance, Collider collider)
        { 
            InventoryItem item = Inventory.main.quickSlots.heldItem;
            if (item != null && item.item.transform.root.gameObject == collider.transform.root.gameObject)
            {
                //ErrorMessage.AddDebug("OnTriggerEnter heldItem ");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(Pickupable), "Pickup")]
        internal class Pickupable_Pickup_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (gravSphereFish.Contains(__instance))
                {
                    int num = gravSphere.attractableList.IndexOf(__instance.GetComponent<Rigidbody>());
                    if (num == -1)
                        return;
                    //ErrorMessage.AddDebug("Pick up gravSphere");
                    gravSphere.removeList.Add(num);
                }
            }
        }
    }
}
