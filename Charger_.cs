using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Charger))]
    internal class Charger_
    {
        public static HashSet<TechType> notRechargableBatteries = new HashSet<TechType>();

        public static IEnumerator CloseUIafterAnimationFinished(Charger charger)
        {
            //AddDebug("WaitForAnimationToFinish " + charger.animTimeOpen);
            yield return new WaitForSeconds(charger.animTimeOpen);
            //AddDebug("WaitForAnimationToFinish !");
            charger.ui.SetActive(false);
        }

        [HarmonyPrefix, HarmonyPatch("ToggleUI")]
        public static bool ToggleUIPrefix(Charger __instance, bool active)
        {
            //AddDebug($"ToggleUI {active}");
            if (active == false)
            {
                CoroutineHost.StartCoroutine(CloseUIafterAnimationFinished(__instance));
                return false;
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix(Charger __instance)
        {
            bool animPlaying = Util.IsAnimationPlaying(__instance.animator);
            //AddDebug($"OnHandClick {animPlaying}");
            return animPlaying == false;
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(Charger __instance)
        {
            //AddDebug(__instance.name + " Charger Start");
            if (__instance.allowedTech == null)
                return;

            foreach (TechType tt in notRechargableBatteries)
            {
                if (__instance.allowedTech.Contains(tt))
                {
                    __instance.allowedTech.Remove(tt);
                    //AddDebug("remove " + tt + " from " + __instance.name);
                }
            }
            //Main.logger.LogMessage(__instance.name + " Charger Start");
            //foreach (var tt in __instance.allowedTech)
            //    Main.logger.LogMessage(__instance.name + " allowedTech " + tt);
        }


    }
}
