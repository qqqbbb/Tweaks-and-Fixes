using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Floater_
    {
        static Dictionary<Pickupable, Floater> pickupableFloaters = new Dictionary<Pickupable, Floater>();

        [HarmonyPatch(typeof(Floater), "Start")]
        class Floater_Start_Patch
        {
            public static void Postfix(Floater __instance)
            {
                Pickupable pickupable = __instance.GetComponent<Pickupable>();
                if (pickupable)
                    pickupableFloaters.Add(pickupable, __instance);
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


    }
}
