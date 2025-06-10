using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Fragment_
    {
        static bool IsFragmentCrate(Transform transform)
        {
            return transform.name.EndsWith("InCrate(Clone)") || transform.name.EndsWith("Fragment(Clone)");
        }

        [HarmonyPatch(typeof(ResourceTracker), "Start")]
        class ResourceTracker_Start_Patch
        {
            static void Postfix(ResourceTracker __instance)
            {
                if (ConfigToEdit.dontSpawnKnownFragments.Value && __instance.techType == TechType.Fragment)
                {
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    if (PDAScanner.complete.Contains(tt))
                    {
                        //AddDebug("ResourceTracker start " + tt);
                        __instance.Unregister();
                        if (IsFragmentCrate(__instance.transform.parent))
                        { // destroy fragment and crate
                            UnityEngine.Object.Destroy(__instance.transform.parent.gameObject);
                        }
                        else
                            UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                }
            }
        }

    }
}
