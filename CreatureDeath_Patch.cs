﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(CreatureDeath))]

    internal class CreatureDeath_Patch
    {
        public static HashSet<CreatureDeath> creatureDeathsToDestroy = new HashSet<CreatureDeath>();
        public static HashSet<TechType> notRespawningCreatures;
        public static HashSet<TechType> notRespawningCreaturesIfKilledByPlayer;
        public static Dictionary<TechType, int> respawnTime = new Dictionary<TechType, int>();

        public static void TryRemoveCorpses()
        {
            //AddDebug("TryRemoveCorpses " + creatureDeathsToDestroy.Count);
            foreach (var cd in creatureDeathsToDestroy)
            {
                Pickupable pickupable = cd.GetComponent<Pickupable>();
                if (pickupable && pickupable.inventoryItem != null)
                {
                    //AddDebug("try RemoveCorpse inventoryItem " + cd.name);
                    continue;
                }
                //AddDebug("RemoveCorpse " + cd.name);
                if (ConfigToEdit.removeDeadCreaturesOnLoad.Value)
                    UnityEngine.Object.Destroy(cd.gameObject);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartPostfix(CreatureDeath __instance)
        {
            TechType techType = CraftData.GetTechType(__instance.gameObject);
            //if (!creatureDeaths.Contains(techType))
            //{
            //    creatureDeaths.Add(techType);
            //    Main.logger.LogMessage("CreatureDeath " + techType + " respawns " + __instance.respawn + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature + " respawnInterval " + __instance.respawnInterval);
            //}
            __instance.respawn = !notRespawningCreatures.Contains(techType);
            __instance.respawnOnlyIfKilledByCreature = notRespawningCreaturesIfKilledByPlayer.Contains(techType);
            //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawn " + __instance.respawn);
            //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
            if (respawnTime.ContainsKey(techType))
                __instance.respawnInterval = respawnTime[techType] * Main.dayLengthSeconds;
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnTakeDamage")]
        static void OnTakeDamagePostfix(CreatureDeath __instance, DamageInfo damageInfo)
        {
            //AddDebug(__instance.name + " OnTakeDamage " + damageInfo.dealer.name);
            if (!ConfigToEdit.heatBladeCooks.Value && damageInfo.type == DamageType.Heat && damageInfo.dealer == Player.mainObject)
                __instance.lastDamageWasHeat = false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("RemoveCorpse")]
        static bool RemoveCorpsePrefix(CreatureDeath __instance)
        {
            if (!Main.gameLoaded)
            {
                creatureDeathsToDestroy.Add(__instance);
                return false;
            }
            return true;
        }
        //[HarmonyPostfix]
        //[HarmonyPatch("SpawnRespawner")]
        static void SpawnRespawnerPostfix(CreatureDeath __instance)
        {
            //AddDebug(__instance.name + " SpawnRespawner ");

        }
    }
}
