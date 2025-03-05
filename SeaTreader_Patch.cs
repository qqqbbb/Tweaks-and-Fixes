using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class SeaTreader_Patch
    {
        static bool seaTreaderSoundsOnStep;
        static bool seaTreaderSoundsOnStomp;

        [HarmonyPatch(typeof(SeaTreaderMeleeAttack), "GetCanAttack")]
        class SeaTreaderMeleeAttack_GetCanAttack_Patch
        { // fix bug: they never attack player
            static void Postfix(SeaTreaderMeleeAttack __instance, GameObject otherGameObject, ref bool __result)
            {
                __result = !__instance.frozen && !__instance.treader.cinematicMode && (Time.time > __instance.lastAttackTime + __instance.attackInterval) && __instance.GetCanHit(otherGameObject);
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
                    seaTreaderSoundsOnStep = true;
            }
            [HarmonyPrefix, HarmonyPatch("OnStomp")]
            public static void OnStompPrefix(SeaTreaderSounds __instance)
            {
                if (ConfigToEdit.seaTreaderAttackOutcropMult.Value < 100)
                    seaTreaderSoundsOnStomp = true;
            }
            [HarmonyPrefix, HarmonyPatch("SpawnChunks")]
            public static bool SpawnChunksPrefix(SeaTreaderSounds __instance)
            {
                if (seaTreaderSoundsOnStomp)
                {
                    seaTreaderSoundsOnStomp = false;
                    int rnd = Main.random.Next(1, 101);
                    if (ConfigToEdit.seaTreaderAttackOutcropMult.Value < rnd)
                    {
                        //AddDebug("SpawnChunks seaTreaderSoundsOnStomp ");
                        return false;
                    }
                }
                else if (seaTreaderSoundsOnStep)
                {
                    seaTreaderSoundsOnStep = false;
                    int rnd = Main.random.Next(1, 101);
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
