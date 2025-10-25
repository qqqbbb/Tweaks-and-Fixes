using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
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

        [HarmonyPostfix, HarmonyPatch("ToggleUIPowered")]
        public static void ToggleUIPoweredPostfix(Charger __instance, bool powered)
        {
            if (powered && __instance.ui.activeSelf)
                return;
            else if (powered == false && __instance.ui.activeSelf == false)
                return;

            PowerRelay powerRelay = PowerSource.FindRelay(__instance.transform);
            __instance.ui.SetActive(powerRelay.IsPowered());
        }

        [HarmonyPrefix, HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix(Charger __instance)
        {
            if (__instance.enabled && __instance.opened == false)
            {
                PowerRelay powerRelay = PowerSource.FindRelay(__instance.transform);
                //AddDebug($"OnHandClick IsPowered {powerRelay.IsPowered()}");
                if (powerRelay.IsPowered() == false)
                    return false;
            }
            bool animPlaying = Util.IsAnimationPlaying(__instance.animator);
            //AddDebug($"OnHandClick {animPlaying}");
            return animPlaying == false;
        }

        [HarmonyPrefix, HarmonyPatch("OnCloseCallback")]
        public static bool OnCloseCallbackPreix(Charger __instance)
        {
            if (__instance.enabled && __instance.opened)
            { // dont play animation when unpowered
                PowerRelay powerRelay = PowerSource.FindRelay(__instance.transform);
                //AddDebug($"OnHandClick IsPowered {powerRelay.IsPowered()}");
                if (powerRelay.IsPowered() == false)
                    return false;
            }
            return true;
        }

        //[HarmonyPostfix, HarmonyPatch("OnHandHover")]
        public static void UpdatePostfix(Charger __instance)
        {
            //AddDebug("nextChargeAttemptTimer " + __instance.nextChargeAttemptTimer.ToString("0.0"));
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(Charger __instance)
        {
            //AddDebug(__instance.name + " Charger Start");
            //__instance.uiUnpoweredText.color = Color.white;
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
