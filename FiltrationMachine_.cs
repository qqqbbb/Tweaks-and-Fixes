using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(FiltrationMachine))]
    internal class FiltrationMachine_
    {
        [HarmonyPostfix, HarmonyPatch("TryFilterWater")]
        static void TryFilterWaterPostfix(FiltrationMachine __instance)
        {
            if (__instance.timeRemainingWater == FiltrationMachine.spawnWaterInterval)
            {
                __instance.timeRemainingWater *= ConfigToEdit.filtrationMachineWaterTimeMult.Value;
                //AddDebug($"TryFilterWaterPostfix {FiltrationMachine.spawnWaterInterval} after {__instance.timeRemainingWater}");
            }
        }

        [HarmonyPostfix, HarmonyPatch("TryFilterSalt")]
        static void TryFilterSaltPostfix(FiltrationMachine __instance)
        {
            if (__instance.timeRemainingSalt == FiltrationMachine.spawnSaltInterval)
            {
                __instance.timeRemainingSalt *= ConfigToEdit.filtrationMachineSaltTimeMult.Value;
                //AddDebug($"TryFilterSaltostfix {FiltrationMachine.spawnSaltInterval} after {__instance.timeRemainingSalt}");
            }
        }


    }
}
