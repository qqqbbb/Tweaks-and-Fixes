using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaks_Fixes
{
    internal class Precurcor_Terminal_Patch
    {
        [HarmonyPatch(typeof(StoryHandTarget), "OnHandHover")]
        class StoryHandTarget_OnHandHover_Patch
        {
            static bool Prefix(StoryHandTarget __instance)
            {
                PrecursorComputerTerminal pct = __instance.GetComponent<PrecursorComputerTerminal>();
                if (pct && pct.used)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
