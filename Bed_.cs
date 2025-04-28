using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using static Bed;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Bed))]
    internal class Bed_
    {
        static bool space;
        static Vector3 playerPos;

        //[HarmonyPostfix, HarmonyPatch("GetCanSleep")]
        public static void GetCanSleepPostfix(Bed __instance, Player player, bool notify, ref bool __result)
        {
            __result = true;
        }
        [HarmonyPostfix, HarmonyPatch("CheckForSpace")]
        public static void CheckForSpacePostfix(Bed __instance, ref bool __result)
        {
            if (__result == true || __instance.name == "NeedForSleepBed")
                space = true;
            //AddDebug("CheckForSpace " + __instance.name);
            __result = true;
        }

        [HarmonyPrefix, HarmonyPatch("OnHandClick")]
        public static void OnHandClickPrefix(Bed __instance)
        {
            space = false;
            if (__instance.name == "NeedForSleepBed")
                space = true;

            playerPos = Player.main.transform.position;
        }

        [HarmonyPostfix, HarmonyPatch("ExitInUseMode")]
        public static void ExitInUseModePostfix(Bed __instance, Player player)
        {
            //AddDebug($"ExitInUseMode space {space}");
            if (space == false)
                __instance.StartCoroutine(RestorePlayerPos(player));
        }

        public static IEnumerator RestorePlayerPos(Player player)
        {
            yield return new WaitUntil(() => player.cinematicModeActive == false);
            //AddDebug("RestorePlayerPos ");
            if (playerPos != default)
                player.transform.position = playerPos;
        }



    }
}
