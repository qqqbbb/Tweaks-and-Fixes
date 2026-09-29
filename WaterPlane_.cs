using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class WaterPlane_
    {
        [HarmonyPatch(typeof(WaterPlane), "Start")]
        internal class WaterPlane_Start_Patch
        {
            static void Postfix(WaterPlane __instance)
            { // water surface in Aurora, in prison, in gun base, in cave on gun island
                __instance.transform.DisableShadowCasting();
                float a = 1;
                if (__instance.name == "Precursor_Prison_MoonPoolSurface(Clone)" || __instance.transform.parent.name == "Precursor_Gun_MoonPoolSurface(Clone)")
                    a = .3f;

                Renderer renderer = __instance.GetComponent<Renderer>();
                Color oldColor = renderer.sharedMaterial.color;
                //AddDebug($"WaterPlane {__instance.name} {oldColor}");
                renderer.sharedMaterial.color = new Color(oldColor.r, oldColor.g, oldColor.b, a);
            }
        }
    }
}
