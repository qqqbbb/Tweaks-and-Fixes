using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Pickupable_Patch
    {
        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();

        [HarmonyPatch(typeof(Pickupable), "Awake")]
        class Pickupable_Awake_Patch
        {
            static void Postfix(Pickupable __instance)
            {
                TechType tt = __instance.GetTechType();
                if (itemMass.ContainsKey(tt))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.mass = itemMass[tt];
                }
            }
        }

    }
}
