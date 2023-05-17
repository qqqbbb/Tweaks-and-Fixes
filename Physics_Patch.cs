using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace Tweaks_and_Fixes
{
    internal class Physics_Patch
    {
        [HarmonyPatch(typeof(WorldForces), "Start")]
        class WorldForces_Patch
        {
            static void Prefix(WorldForces __instance)
            {
                //Main.logger.LogDebug("WorldForces Start " + __instance.name + " underwaterDrag " + __instance.underwaterDrag);
                //AddDebug("WorldForces Start " + __instance.name + " underwaterDrag " + __instance.underwaterDrag);
                LiveMixin lm = __instance.GetComponent<LiveMixin>();
                if (lm && lm.health > 0)
                    return;

                __instance.underwaterDrag = 1;
            }
        }


            
    }
}
