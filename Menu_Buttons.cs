using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tweaks_Fixes
{
    internal class Menu_Buttons
    {

        [HarmonyPatch(typeof(uGUI_MainMenu), "Start")]
        class uGUI_MainMenu_Start_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
                if (ConfigToEdit.removeCreditsButton.Value)
                {
                    Transform t = __instance.transform.Find("Panel/MainMenu/PrimaryOptions/MenuButtons/ButtonCredits");
                    if (t)
                        t.gameObject.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(IngameMenu), "OnEnable")]
        class IngameMenu_OnEnable_Patch
        {
            static void Postfix(IngameMenu __instance)
            {
                if (ConfigToEdit.removeUnstuckButton.Value)
                {
                    __instance.unstuckButton.gameObject.SetActive(false);
                }
                if (ConfigToEdit.enableDevButton.Value)
                {
                    __instance.developerMode = true;
                    __instance.developerButton.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_OptionsPanel))]
        class uGUI_OptionsPanel_Patch
        {
            [HarmonyPrefix, HarmonyPatch("AddKeyRedemptionTab")]
            static bool AddKeyRedemptionTabPrefix(uGUI_OptionsPanel __instance)
            {
                return !ConfigToEdit.removeRedeemButton.Value;
            }
            [HarmonyPrefix, HarmonyPatch("AddTroubleshootingTab")]
            static bool AddTroubleshootingTabPrefix(uGUI_OptionsPanel __instance)
            {
                return !ConfigToEdit.removeTroubleshootButton.Value;
            }
        }


        [HarmonyPatch(typeof(uGUI_FeedbackCollector), "IsEnabled")]
        class uGUI_FeedbackCollector_IsEnabled_Patch
        {
            static void Postfix(uGUI_FeedbackCollector __instance, ref bool __result)
            {
                if (__result && ConfigToEdit.removeFeedbackButton.Value)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(UnstuckPlayer))]
        class UnstuckPlayer_Patch
        {
            [HarmonyPrefix, HarmonyPatch("Start")]
            static bool StartPrefix(UnstuckPlayer __instance)
            {
                return !ConfigToEdit.removeUnstuckButton.Value;
            }
            [HarmonyPrefix, HarmonyPatch("OnEnable")]
            static bool AddTroubleshootingTabPrefix(UnstuckPlayer __instance)
            {
                return !ConfigToEdit.removeUnstuckButton.Value;
            }
        }

    }
}
