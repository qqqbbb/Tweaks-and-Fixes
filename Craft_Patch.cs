using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Craft_Patch
    {

        [HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_Patch
        {
            static void Prefix(Crafter __instance, TechType techType, ref float duration)
            {
                //ErrorMessage.AddDebug("Craft " + techType);
                duration *= Main.config.craftTimeMult;
                //return true;
            }
        }

        [HarmonyPatch(typeof(Constructable), "GetConstructInterval")]
        class Constructable_GetConstructInterval_Patch
        {
            static void Postfix(ref float __result)
            {
                if (NoCostConsoleCommand.main.fastBuildCheat)
                    return;
                //ErrorMessage.AddDebug("GetConstructInterval " );
                __result *= Main.config.buildTimeMult;
            }
        }
    }
}
