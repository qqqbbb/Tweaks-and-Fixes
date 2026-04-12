using HarmonyLib;
using Nautilus.Options;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Radiation
    {
        internal static LeakingRadiation auroraRadiation;
        internal static float maxDistance;
        internal static float radius;
        internal static float maxRadius;

        internal static void UpdateAuroraRadRadius()
        {
            if (auroraRadiation == null)
                return;

            if (maxDistance > 0)
                auroraRadiation.playerDistanceTracker.maxDistance = maxDistance * ConfigMenu.auroraRadRadiusMult.Value;

            if (radius > 0)
                auroraRadiation.radiatePlayerInRange.radiateRadius = radius * ConfigMenu.auroraRadRadiusMult.Value;

            if (maxRadius > 0)
                auroraRadiation.kMaxRadius = maxRadius * ConfigMenu.auroraRadRadiusMult.Value;
        }

        [HarmonyPatch(typeof(RadiatePlayerInRange))]
        class RadiatePlayerInRange_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(RadiatePlayerInRange __instance)
            {
                if (__instance.name == "RadiationDamage(Clone)")
                {
                    if (radius == 0)
                        radius = __instance.radiateRadius;

                    //AddDebug("RadiatePlayerInRange Start " + (int)__instance.radiateRadius);
                    __instance.radiateRadius = radius * ConfigMenu.auroraRadRadiusMult.Value;
                    if (maxDistance == 0)
                        maxDistance = __instance.tracker.maxDistance;

                    __instance.tracker.maxDistance = maxDistance * ConfigMenu.auroraRadRadiusMult.Value;
                }
            }
            //[HarmonyPostfix, HarmonyPatch("Radiate")]
            public static void UpdatePostfix(RadiatePlayerInRange __instance)
            {
                //AddDebug("RadiatePlayerInRange radius " + (int)__instance.radiateRadius);
            }
        }

        [HarmonyPatch(typeof(LeakingRadiation))]
        class LeakingRadiation_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(LeakingRadiation __instance)
            {
                auroraRadiation = __instance;
                if (maxRadius == 0)
                    maxRadius = __instance.kMaxRadius;

                __instance.kMaxRadius = maxRadius * ConfigMenu.auroraRadRadiusMult.Value;
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(LeakingRadiation __instance)
            {
                if (CrashedShipExploder.main.IsExploded())
                {
                    float radius = __instance.currentRadius * ConfigMenu.auroraRadRadiusMult.Value;
                    __instance.damagePlayerInRadius.damageRadius = radius;
                    __instance.radiatePlayerInRange.radiateRadius = radius;
                    //float d = Vector3.Distance(__instance.transform.position, Player.main.transform.position);
                    //AddDebug("LeakingRadiation radius " + (int)radius);
                }
            }
        }

    }
}
