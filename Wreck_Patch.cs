using HarmonyLib;
using System;
using SMLHelper.V2.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    static class BulkheadDoorPatch
    {
        [HarmonyPatch(typeof(BulkheadDoor))]
        class BulkheadDoor_OnHandClick_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("SetState")]
            public static void SetStatePostfix(BulkheadDoor __instance, bool open)
            { // initiallyOpen not saved
                AddDebug("SetState " + open);
                __instance.SetInitialyOpen(open);
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnHandHover")]
            public static void OnHandHoverPostfix(BulkheadDoor __instance)
            {
                AddDebug("opened " + __instance.opened);
                AddDebug("initiallyOpen " + __instance.initiallyOpen);
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnHandClick")]
            public static void OnHandClickPostfix(BulkheadDoor __instance)
            {
                //SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(SaveLoadManager.main.currentSlot);
                //long dictKey = GetKey(SaveLoadManager.main.currentSlot);
                //PrefabIdentifier prefabIdentifier = __instance.GetComponent<PrefabIdentifier>();
                //if (prefabIdentifier)
                //{
                //    Main.config.openedWreckDoors[prefabIdentifier.id] = !__instance.isOpen;
                //}
                int Key = Mathf.RoundToInt(__instance.gameObject.transform.position.x + __instance.gameObject.transform.position.y + __instance.gameObject.transform.position.z);
                string slot = SaveLoadManager.main.currentSlot;
                if (!Main.config.openedWreckDoors.ContainsKey(slot))
                    Main.config.openedWreckDoors[slot] = new Dictionary<int, bool>();

                Main.config.openedWreckDoors[slot][Key] = !__instance.opened;
                //Main.config.Save(); 00058db8c0ac
            }
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static void AwakePrefix(BulkheadDoor __instance)
            {
                //float doorKey = __instance.gameObject.transform.position.x + __instance.gameObject.transform.position.y;
                int Key = Mathf.RoundToInt(__instance.gameObject.transform.position.x + __instance.gameObject.transform.position.y + __instance.gameObject.transform.position.z);

                //Main.Log("doorKey " + doorKey);
                string slot = SaveLoadManager.main.currentSlot;
                if (Main.config.openedWreckDoors.ContainsKey(slot) && Main.config.openedWreckDoors[slot].ContainsKey(Key))
                {
                    //Main.Log("load door " + slot + " " + doorKey + " " + Main.config.openedWreckDoors[slot][doorKey]);
                    //AddDebug("load door " + slot + " " + doorKey + " " + Main.config.openedWreckDoors[slot][doorKey]);
                    __instance.initiallyOpen = Main.config.openedWreckDoors[slot][Key];
                }
            }
        }


        [HarmonyPatch(typeof(StarshipDoor), "OnHandHover")]
        class StarshipDoor_OnHandHover_Patch
        {
            private static bool Prefix(StarshipDoor __instance)
            {
                //AddDebug("doorOpenMethod " + __instance.doorOpenMethod);
                LaserCutObject laserCutObject = __instance.GetComponent<LaserCutObject>();
                if (laserCutObject != null && laserCutObject.isCutOpen)
                {
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