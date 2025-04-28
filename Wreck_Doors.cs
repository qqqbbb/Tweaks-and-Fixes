using HarmonyLib;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    static class Wreck_Doors
    {
        static HashSet<StarshipDoor> cutOpenedDoors = new HashSet<StarshipDoor>();

        [HarmonyPatch(typeof(BulkheadDoor))]
        class BulkheadDoor_OnHandClick_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("SetState")]
            public static void SetStatePostfix(BulkheadDoor __instance, bool open)
            { // initiallyOpen not saved
                //AddDebug("SetState " + open);
                __instance.SetInitialyOpen(open);
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnHandHover")]
            public static void OnHandHoverPostfix(BulkheadDoor __instance)
            {
                //AddDebug("opened " + __instance.opened);
                //AddDebug("initiallyOpen " + __instance.initiallyOpen);
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnHandClick")]
            public static void OnHandClickPostfix(BulkheadDoor __instance)
            {
                //AddDebug("BulkheadDoor OnHandClick opened " + __instance.opened);
                if (__instance.opened)
                    Main.configMain.DeleteWreckDoor(__instance.transform.position);
                else
                    Main.configMain.SaveWreckDoor(__instance.transform.position);
            }

            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static void AwakePrefix(BulkheadDoor __instance)
            {
                if (Main.configMain.IsWreckDoorSaved(__instance.transform.position))
                {
                    //Main.Log("load door " + slot + " " + doorKey + " " + Main.config.openedWreckDoors[slot][doorKey]);
                    __instance.initiallyOpen = true;
                }
            }
        }

        [HarmonyPatch(typeof(StarshipDoor), "OnHandHover")]
        class StarshipDoor_OnHandHover_Patch
        {
            private static bool Prefix(StarshipDoor __instance)
            {
                //AddDebug("doorOpenMethod " + __instance.doorOpenMethod);
                if (cutOpenedDoors.Contains(__instance))
                    return false;

                LaserCutObject laserCutObject = __instance.GetComponent<LaserCutObject>();
                if (laserCutObject != null && laserCutObject.isCutOpen)
                {
                    cutOpenedDoors.Add(__instance);
                    //if (Input.GetKey(KeyCode.Z))
                    //{ 
                    //	laserCutObject.cutObject.SetActive(true);
                    //	AddDebug("cutObject.SetActive ");
                    //}
                    return false;
                }
                return true;
            }
        }

    }
}