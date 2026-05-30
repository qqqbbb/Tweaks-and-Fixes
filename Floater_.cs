using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Floater_
    {
        public static Dictionary<Pickupable, Floater> pickupableFloaters = new Dictionary<Pickupable, Floater>();

        [HarmonyPatch(typeof(Floater))]
        class Floater_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(Floater __instance)
            {
                Pickupable pickupable = __instance.GetComponent<Pickupable>();
                if (pickupable)
                    pickupableFloaters.Add(pickupable, __instance);
            }
            [HarmonyPostfix, HarmonyPatch("GetCanConnectTo")]
            public static void GetCanConnectToPostfix(Floater __instance, Rigidbody rb, ref bool __result)
            {
                //AddDebug($"Floater GetCanConnectTo {rb.name} {__result}");
                if (__result == true)
                {
                    TechType tt = CraftData.GetTechType(rb.gameObject);
                    if (tt == TechType.HoopfishSchool)
                        __result = false;
                }
            }
            //[HarmonyPostfix, HarmonyPatch("BuoyancyEnabled")]
            public static void BuoyancyEnabledPostfix(Floater __instance, ref bool __result)
            {
                //AddDebug($"Floater GetCanConnectTo {rb.name} {__result}");
                __result = false;
            }
        }

        [HarmonyPatch(typeof(Pickupable), "Pickup")]
        class Pickupable_Pickup_Patch
        {
            public static void Postfix(Pickupable __instance)
            {// fix bug: Floater.Disconnect does not run when picked up
                if (pickupableFloaters.ContainsKey(__instance))
                {
                    //AddDebug("Pickupable Pickup floater");
                    pickupableFloaters[__instance].Disconnect();
                }
            }
        }

        [HarmonyPatch(typeof(Drillable), "SpawnFX")]
        class Drillable_SpawnFX_Patch
        {
            public static void Postfix(Drillable __instance)
            {
                //AddDebug("Drillable SpawnFX");
                CheckFloatersOnDrillable(__instance.gameObject);
            }

            private static void CheckFloatersOnDrillable(GameObject go)
            {
                FloatersTarget floatersTarget = go.GetComponent<FloatersTarget>();
                if (floatersTarget == null)
                    return;

                for (int i = floatersTarget.attachedFloaters.Count - 1; i >= 0; i--)
                {
                    Floater floater = floatersTarget.attachedFloaters[i];
                    if (floater == null)
                        continue;

                    Vector3 dir = -floater.transform.up;
                    if (Physics.Raycast(floater.transform.position, dir, out RaycastHit hitInfo, 0.5f) == false)
                    {
                        //AddDebug("Disconnect floater");
                        floater.Disconnect();
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(FloatersTarget))]
        class FloatersTarget_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("OnFloaterDetached")]
            public static void OnFloaterDetachedPostfix(FloatersTarget __instance)
            {
                CheckStone(__instance);
            }

            private static void CheckStone(FloatersTarget __instance)
            {
                if (__instance.attachedFloaters.Count == 0)
                {
                    Drillable drillable = __instance.GetComponent<Drillable>();
                    if (drillable && drillable.resources.Length == 0)
                    {
                        AddDebug("Floater Stone");
                        LargeWorldEntity entity = __instance.GetComponent<LargeWorldEntity>();
                        if (entity)
                            entity.cellLevel = LargeWorldEntity.CellLevel.Near;
                    }
                }
            }

            //[HarmonyPostfix, HarmonyPatch("DetachAll")]
            public static void DetachAllPostfix(FloatersTarget __instance)
            {
                CheckStone(__instance);
            }
        }


    }
}
