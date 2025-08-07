using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tweaks_Fixes
{
    internal class Start_Warning
    {
        [HarmonyPatch(typeof(FlashingLightsDisclaimer))]
        class FlashingLightsDisclaimer_CanShow_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("TryToShow")]
            static bool TryToShowPrefix(FlashingLightsDisclaimer __instance)
            {
                bool noText = String.IsNullOrEmpty(ConfigToEdit.gameStartWarningText.Value);
                return !noText;
            }
            [HarmonyPrefix]
            [HarmonyPatch("SetText")]
            static bool SetTextPrefix(FlashingLightsDisclaimer __instance)
            {
                __instance.text.text = Language.main.Get(ConfigToEdit.gameStartWarningText.Value);
                //Main.logger.LogDebug("FlashingLightsDisclaimer SetText ");
                return false;
            }
        }
    }
}
