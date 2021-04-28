using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{   // to fix: Boulder with floaters move up
    class Boulder_Fix
    {
        //[HarmonyPatch(typeof(Drillable), "Start")]
        class Drillable_Start_Patch
        {
            static void Postfix(Drillable __instance)
            {
                if (__instance.resources.Count() == 0) 
                {
                    WorldForces wf = __instance.GetComponent<WorldForces>();
                    if (wf)
                    {
                        Floater[] floaters = __instance.GetAllComponentsInChildren<Floater>();
                        if (floaters.Length < 5)
                        {
                            //ErrorMessage.AddDebug("floaters == null");
                            wf.handleGravity = false;
                        }
                        PlayerDistanceTracker pdt = __instance.gameObject.EnsureComponent<PlayerDistanceTracker>();
                        pdt.maxDistance = 50f;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(PlayerDistanceTracker), "ScheduledUpdate")]
        class PlayerDistanceTracker_ScheduledUpdate_Patch
        {
            static void Postfix(PlayerDistanceTracker __instance)
            {
                if (!__instance.GetComponent<Drillable>())
                    return;
                //float magnitude = (__instance.transform.position - Player.main.transform.position).magnitude;
                //ErrorMessage.AddDebug("magnitude " + magnitude);
                WorldForces wf = __instance.GetComponent<WorldForces>();
                if (!wf)
                    return;

                if (!wf.handleGravity && __instance._playerNearby)
                {
                    wf.handleGravity = true;
                    //ErrorMessage.AddDebug("handleGravity  true ");

                }
                else if (wf.handleGravity)
                {
                    Floater[] floaters = __instance.GetAllComponentsInChildren<Floater>();
                    if (floaters.Length < 5)
                    {
                        wf.handleGravity = false;
                    }
                    ErrorMessage.AddDebug("floaters " + floaters.Count());
                }
            }
        }

    }
}
