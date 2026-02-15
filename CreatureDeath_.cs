using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(CreatureDeath))]

    internal class CreatureDeath_
    {
        public static HashSet<CreatureDeath> creatureDeathsToDestroy = new HashSet<CreatureDeath>();
        public static HashSet<TechType> notRespawningCreatures;
        public static HashSet<TechType> notRespawningCreaturesIfKilledByPlayer;
        public static Dictionary<TechType, int> respawnTime;

        public static void TryRemoveCorpses()
        {
            //AddDebug("TryRemoveCorpses " + creatureDeathsToDestroy.Count);
            if (ConfigToEdit.removeDeadCreaturesOnLoad.Value == false)
                return;

            foreach (var cd in creatureDeathsToDestroy)
            {
                Pickupable pickupable = cd.GetComponent<Pickupable>();
                if (pickupable)
                {
                    if (pickupable._isInSub || pickupable.inventoryItem != null)
                    { // dont remove dead fish from containers
                      //AddDebug("try RemoveCorpse inventoryItem " + cd.name);
                        continue;
                    }
                }
                //AddDebug("RemoveCorpse " + cd.name);
                Util.DestroyEntity(cd.gameObject);
            }
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(CreatureDeath __instance)
        {
            TechType techType = CraftData.GetTechType(__instance.gameObject);
            //if (!creatureDeaths.Contains(techType))
            //{
            //    creatureDeaths.Add(techType);
            //    Main.logger.LogMessage("CreatureDeath " + techType + " respawns " + __instance.respawn + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature + " respawnInterval " + __instance.respawnInterval);
            //}
            if (notRespawningCreatures != null)
                __instance.respawn = !notRespawningCreatures.Contains(techType);

            if (notRespawningCreaturesIfKilledByPlayer != null)
                __instance.respawnOnlyIfKilledByCreature = notRespawningCreaturesIfKilledByPlayer.Contains(techType);

            //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawn " + __instance.respawn);
            //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
            if (respawnTime != null && respawnTime.ContainsKey(techType))
                __instance.respawnInterval = respawnTime[techType] * DayNightCycle.kDayLengthSeconds;
        }
        [HarmonyPostfix, HarmonyPatch("OnTakeDamage")]
        static void OnTakeDamagePostfix(CreatureDeath __instance, DamageInfo damageInfo)
        {
            //AddDebug(__instance.name + " OnTakeDamage " + damageInfo.dealer.name);
            if (!ConfigToEdit.heatBladeCooks.Value && damageInfo.type == DamageType.Heat && damageInfo.dealer == Player.mainObject)
                __instance.lastDamageWasHeat = false;
        }
        [HarmonyPrefix, HarmonyPatch("RemoveCorpse")]
        static bool RemoveCorpsePrefix(CreatureDeath __instance)
        {
            if (Main.gameLoaded)
                return true;

            creatureDeathsToDestroy.Add(__instance);
            return false;
        }
        //[HarmonyPostfix]
        //[HarmonyPatch("SpawnRespawner")]
        static void SpawnRespawnerPostfix(CreatureDeath __instance)
        {
            //AddDebug(__instance.name + " SpawnRespawner ");

        }
    }
}
