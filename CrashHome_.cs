using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(CrashHome))]
    internal class CrashHome_
    {
        [HarmonyPrefix, HarmonyPatch("Update")]
        public static void UpdatePrefix(CrashHome __instance)
        {
            if (__instance.crash == null && __instance.spawnTime < 0)
            {
                if (CreatureDeath_.respawnTime != null && CreatureDeath_.respawnTime.ContainsKey(TechType.Crash))
                {
                    //AddDebug("CrashHome set spawn time");
                    __instance.spawnTime = DayNightCycle.main.timePassedAsFloat + CreatureDeath_.respawnTime[TechType.Crash] * DayNightCycle.kDayLengthSeconds;
                }
            }
        }
        [HarmonyPostfix, HarmonyPatch("Spawn")]
        public static void SpawnPostfix(CrashHome __instance)
        {
            if (Main.gameLoaded == false || __instance.prevClosed || ConfigToEdit.respawnCrashPowder.Value == false)
                return;

            int numHits = UWE.Utils.SpherecastIntoSharedBuffer(__instance.transform.position, 1, Vector3.forward);
            for (int i = 0; i < numHits; ++i)
            {
                RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
                if (raycastHit.collider.gameObject.name == "CrashPowder(Clone)")
                {
                    //Main.logger.LogDebug("CrashHome has CrashPowder " + __instance.transform.position);
                    //AddDebug("CrashHome has CrashPowder " + __instance.transform.position);
                    return;
                }
            }
            //Main.logger.LogDebug("CrashHome Spawn no CrashPowder " + __instance.transform.position);
            //AddDebug("CrashHome Spawn no CrashPowder " + __instance.transform.position);
            UWE.CoroutineHost.StartCoroutine(SpawnCrashPowder(__instance.transform));
        }

        public static IEnumerator SpawnCrashPowder(Transform tr)
        {
            //AddDebug("SpawnCrashPowder " + tr.position);
            //Main.logger.LogDebug("SpawnCrashPowder " + tr.position);
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(TechType.CrashPowder, result);
            GameObject powder = result.Get();
            powder.transform.position = tr.position;
            powder.transform.eulerAngles = tr.eulerAngles;
        }
    }

}
