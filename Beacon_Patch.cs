using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(Beacon), "Throw")]
    class Beacon_Patch
    {
        static void Postfix(Beacon __instance)
        {
            // x and z does not matter, it will stabilize itself
            __instance.gameObject.transform.rotation = Camera.main.transform.rotation;
            __instance.transform.Rotate(0.0f, 180f, 0.0f);
        }
    }
}
