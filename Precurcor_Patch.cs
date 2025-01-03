using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Precurcor_Patch
    {
        [HarmonyPatch(typeof(StoryHandTarget), "OnHandHover")]
        class StoryHandTarget_OnHandHover_Patch
        {
            static bool Prefix(StoryHandTarget __instance)
            {// do not prompt player if used terminal
                PrecursorComputerTerminal pct = __instance.GetComponent<PrecursorComputerTerminal>();
                if (pct && pct.used)
                {
                    return false;
                }
                return true;
            }
        }


        //[HarmonyPatch(typeof(AnteChamber), "Start")]
        class AnteChamber_Start_Patch
        {
            public static bool Prefix(AnteChamber __instance)
            {
                //if (ConfigToEdit.disableIonCubeFabricator.Value)
                {
                    __instance.drillable.deleteWhenDrilled = true;
                    AddDebug("AnteChamber Start return false");
                    return false;
                }
                //return true;
            }
            public static void Postfix(AnteChamber __instance)
            {
                //return !ConfigToEdit.disableIonCubeFabricator.Value;
                //if (ConfigToEdit.disableIonCubeFabricator.Value)
                {
                    __instance.drillable.deleteWhenDrilled = true;
                }
            }
        }

        //[HarmonyPatch(typeof(AnteChamber), "OnDrilled")]
        class AnteChamber_OnDrilled_Patch
        {
            public static bool Prefix(AnteChamber __instance)
            {
                //UnityEngine.Object.Destroy(__instance.drillable.gameObject);
                //return !ConfigToEdit.disableIonCubeFabricator.Value;
                return false;
            }
        }


    }
}
