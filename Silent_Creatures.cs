
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Silent_Creatures
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };

        [HarmonyPatch(typeof(Creature), "Start")]
        public static class Creature_Start_Patch
        {
            public static void Postfix(Creature __instance)
            {
                //AddDebug(" Creature.Start " + __instance.name);
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    //AddDebug(" silentCreatures.Contains " + tt);
                    foreach (FMOD_StudioEventEmitter ee in __instance.GetComponentsInChildren<FMOD_StudioEventEmitter>())
                        ee.evt.setVolume(0);

                    foreach (FMOD_CustomEmitter ce in __instance.GetComponentsInChildren<FMOD_CustomEmitter>())
                        ce.evt.setVolume(0);
                }
            }
        }

        [HarmonyPatch(typeof(AttackLastTarget), "StartPerform")]
        class AttackLastTarget_StartPerform_Patch
        {
            public static void Prefix(AttackLastTarget __instance)
            {
                if (__instance.attackStartSound == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    //AddDebug("AttackLastTarget silentCreatures.Contains " + tt);
                    __instance.attackStartSound.evt.setVolume(0);
                    //AddDebug(tt + " AttackLastTarget StartPerform");
                }
            }
        }

        [HarmonyPatch(typeof(MeleeAttack), "OnEnable")]
        class MeleeAttack_OnEnable_Patch
        {
            public static void Postfix(MeleeAttack __instance)
            {
                if (__instance.attackSound == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    __instance.attackSound.evt.setVolume(0);
                    //AddDebug(tt + " MeleeAttack OnEnable");
                }
            }
        }

        [HarmonyPatch(typeof(AggressiveWhenSeeTarget), "Start")]
        class AggressiveWhenSeeTarget_Start_Patch
        {
            public static void Postfix(AggressiveWhenSeeTarget __instance)
            {
                if (__instance.sightedSound == null)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    __instance.sightedSound.evt.setVolume(0);
                    //AddDebug(tt + " AggressiveWhenSeeTarget Start");
                }
            }
        }

        [HarmonyPatch(typeof(SandShark), "Update")]
        class SandShark_Update_Patch
        {
            public static void Postfix(SandShark __instance)
            {
                if (silentCreatures.Contains(TechType.Sandshark))
                {
                    __instance.idleSound.evt.setVolume(0);
                    __instance.moveSandSound.evt.setVolume(0);
                    __instance.burrowSound.evt.setVolume(0);
                    __instance.alertSound.evt.setVolume(0);
                    //AddDebug(" SandShark Update");
                }
            }
        }



    }
}
