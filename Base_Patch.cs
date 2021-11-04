using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Base_Patch
    {
        //[HarmonyPatch(typeof(BaseDeconstructable), "Awake")]
        class BaseDeconstructable_Awake_Patch
        {
            static void Postfix(BaseDeconstructable __instance)
            { // recipe is None
                AddDebug("BaseDeconstructable Awake " + __instance.recipe);
                if (__instance.recipe == TechType.BaseMoonpool)
                {

                }
            }
        }

        [HarmonyPatch(typeof(BaseWaterPlane), "Awake")]
        class BaseWaterPlane_Awake_Patch
        {
            static void Postfix(BaseWaterPlane __instance)
            {
                if (__instance.transform.parent.name == "BaseMoonpool(Clone)")
                {
                    Transform t = __instance.transform.Find("x_BaseWaterPlane");
                    if (t)
                        t.localScale = new Vector3(1f, 1f, .98f);
                }
            }
        }

    }
}