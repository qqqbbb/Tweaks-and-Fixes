using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Cyclops_unpowered
    {
        static HashSet<SubRoot> poweredSubs = new HashSet<SubRoot>();

        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            [HarmonyPostfix, HarmonyPatch("UpdateSubModules")]
            public static void UpdateSubModulesPostix(SubRoot __instance)
            {
                if (!Main.gameLoaded || !__instance.isCyclops)
                    return;

                bool isPowered = __instance.powerRelay.IsPowered();
                if (!isPowered && poweredSubs.Contains(__instance))
                {
                    ToggleStuff(__instance, false);
                    poweredSubs.Remove(__instance);
                }
                else if (isPowered && !poweredSubs.Contains(__instance))
                {
                    ToggleStuff(__instance, true);
                    poweredSubs.Add(__instance);
                }
            }

            private static void ToggleStuff(SubRoot subRoot, bool state)
            {
                //AddDebug($"ToggleStuff  {state} ");
                Transform holographicDisplayTr = subRoot.transform.Find("HolographicDisplay");
                holographicDisplayTr?.gameObject.SetActive(state);
                Transform CompassTr = subRoot.transform.Find("Compass");
                CompassTr?.gameObject.SetActive(state);
                Transform DecoyScreenTr = subRoot.transform.Find("DecoyScreenHUD");
                DecoyScreenTr?.gameObject.SetActive(state);
                Transform cvstmScreen = subRoot.transform.Find("CyclopsVehicleStorageTerminal/GUIScreen");
                cvstmScreen?.gameObject.SetActive(state);
                Transform UpgradeConsoleTr = subRoot.transform.Find("UpgradeConsoleHUD");
                UpgradeConsoleTr?.gameObject.SetActive(state);
                Transform renameConsole = subRoot.transform.Find("SubName");
                renameConsole?.gameObject.SetActive(state);
            }


            [HarmonyPatch(typeof(PilotingChair))]
            public class PilotingChair_Patch
            {
                [HarmonyPostfix, HarmonyPatch("IsValidHandTarget")]
                public static void IsValidHandTargetPostfix(PilotingChair __instance, GUIHand hand, ref bool __result)
                {
                    if (!__instance.subRoot.powerRelay.IsPowered())
                        __result = false;
                }
            }



        }
    }
}
