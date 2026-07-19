using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
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

        public static IEnumerator DisableAnimatorWhenOpened(Charger charger)
        {
            yield return Main.waitUntilGameLoaded;
            yield return new WaitForSeconds(charger.animTimeOpen);
            charger.animator.enabled = false;
        }

        public static void TrimUnpoweredNotifyStrings(Charger charger)
        {
            string s = Language.main.Get("ChargerInsufficientPower");
            s = RemoveAfterNewLine(s);
            //AddDebug(s);
            for (int i = 0; i <= Charger.chargeAttemptInterval; i++)
                charger.unpoweredNotifyStrings[i] = s;
        }

        public static string RemoveAfterNewLine(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            int newLineIndex = input.IndexOfAny(new char[] { '\n', '\r' });

            if (newLineIndex == -1)
                return input;

            return input.Substring(0, newLineIndex);
        }

        //[HarmonyPrefix, HarmonyPatch("ToggleUI")]
        public static bool ToggleUIPrefix(Charger __instance, bool active)
        {
            AddDebug($"ToggleUI {active}");
            if (active == false)
            {
                CoroutineHost.StartCoroutine(CloseUIafterAnimationFinished(__instance));
                return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch("OnOpen")]
        public static void OnOpenPostfix(Charger __instance)
        {
            //AddDebug($"OnOpen ");
            if (__instance is PowerCellCharger)
                CoroutineHost.StartCoroutine(DisableAnimatorWhenOpened(__instance));
        }

        [HarmonyPrefix, HarmonyPatch("OnCloseCallback")]
        public static void OnCloseCallbackPrefix(Charger __instance)
        {
            //AddDebug($"OnCloseCallback HasChargables " + __instance.HasChargables());
            if (__instance is PowerCellCharger && __instance.HasChargables() == false)
                __instance.animator.enabled = true;
        }

        //[HarmonyPostfix, HarmonyPatch("ToggleUIPowered")]
        public static void ToggleUIPoweredPostfix(Charger __instance, bool powered)
        {
            if (powered && __instance.ui.activeSelf)
                return;
            else if (powered == false && __instance.ui.activeSelf == false)
                return;

            PowerRelay powerRelay = PowerSource.FindRelay(__instance.transform);
            __instance.ui.SetActive(powerRelay.IsPowered());
        }

        //[HarmonyPrefix, HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix(Charger __instance)
        {
            bool animPlaying = Util.IsAnimationPlaying(__instance.animator);
            //AddDebug($"OnHandClick {animPlaying}");
            return animPlaying == false;
        }

        //[HarmonyPrefix, HarmonyPatch("Update")]
        public static bool UpdatePostix(Charger __instance)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                AddDebug("unpoweredNotifyStrings " + __instance.unpoweredNotifyStrings.Count);
                foreach (var kv in __instance.unpoweredNotifyStrings)
                    Main.logger.LogDebug($"unpoweredNotifyStrings {kv.Key} {kv.Value}");

                //__instance.animator.enabled = false;
                //__instance.sequence.ForceState(false);
                return false;
            }
            return true;
        }

        //[HarmonyPostfix, HarmonyPatch("OnHandHover")]
        public static void UpdatePostfix(Charger __instance)
        {
            //AddDebug("nextChargeAttemptTimer " + __instance.nextChargeAttemptTimer.ToString("0.0"));
            //AddDebug("ChargerInsufficientPower " + Language.main.GetFormat("ChargerInsufficientPower", 11));
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(Charger __instance)
        {
            //AddDebug("Start unpoweredNotifyStrings " + __instance.unpoweredNotifyStrings.Count);
            if (__instance is PowerCellCharger && __instance.opened)
                CoroutineHost.StartCoroutine(DisableAnimatorWhenOpened(__instance));

            //__instance.uiUnpoweredText.color = Color.white;
            __instance.chargeSpeed *= ConfigToEdit.batteryChargeSpeedMult.Value;
            //AddDebug($"{__instance.name}  Charger Start {s} mod {__instance.chargeSpeed}");

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
            if (ConfigToEdit.hints.Value == false)
                TrimUnpoweredNotifyStrings(__instance);
            //Main.logger.LogMessage(__instance.name + " Charger Start");
            //foreach (var tt in __instance.allowedTech)
            //    Main.logger.LogMessage(__instance.name + " allowedTech " + tt);
        }

    }
}
