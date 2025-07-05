using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class SeaTreader_
    {
        static bool seaTreaderStep;
        static bool seaTreaderStomp;

        [HarmonyPatch(typeof(SeaTreaderMeleeAttack), "GetCanAttack")]
        class SeaTreaderMeleeAttack_GetCanAttack_Patch
        { // fix bug: they never attack player
            static void Postfix(SeaTreaderMeleeAttack __instance, GameObject otherGameObject, ref bool __result)
            {
                __result = !__instance.frozen && !__instance.treader.cinematicMode && Time.time > __instance.lastAttackTime + __instance.attackInterval && __instance.GetCanHit(otherGameObject);
                //AddDebug("GetCanAttack " + __result);
            }
        }

        [HarmonyPatch(typeof(SeaTreaderSounds))]
        class SeaTreaderSounds_patch
        {
            [HarmonyPrefix, HarmonyPatch("OnStep")]
            public static void OnStepPrefix(SeaTreaderSounds __instance)
            {
                if (ConfigToEdit.seaTreaderOutcropMult.Value < 100)
                    seaTreaderStep = true;
            }
            [HarmonyPrefix, HarmonyPatch("OnStomp")]
            public static void OnStompPrefix(SeaTreaderSounds __instance)
            {
                if (ConfigToEdit.seaTreaderAttackOutcropMult.Value < 100)
                    seaTreaderStomp = true;
            }
            [HarmonyPrefix, HarmonyPatch("SpawnChunks")]
            public static bool SpawnChunksPrefix(SeaTreaderSounds __instance)
            {
                if (seaTreaderStomp)
                {
                    seaTreaderStomp = false;
                    int rnd = UnityEngine.Random.Range(1, 101);
                    if (ConfigToEdit.seaTreaderAttackOutcropMult.Value < rnd)
                    {
                        //AddDebug("SpawnChunks seaTreaderSoundsOnStomp ");
                        return false;
                    }
                }
                else if (seaTreaderStep)
                {
                    seaTreaderStep = false;
                    int rnd = UnityEngine.Random.Range(1, 101);
                    if (ConfigToEdit.seaTreaderOutcropMult.Value < rnd)
                    {
                        //AddDebug("SpawnChunks seaTreaderSoundsOnStep ");
                        return false;
                    }
                }
                return true;
            }
        }



    }
}
