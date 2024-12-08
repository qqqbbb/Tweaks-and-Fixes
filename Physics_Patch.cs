using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaks_Fixes
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

                if (__instance.GetComponent<Projectile>())
                    return;

                if (__instance.underwaterDrag == 0)
                    __instance.underwaterDrag = 1;
            }
        }



    }
}
