using HarmonyLib;
using QModManager.Utility;
using System;
using SMLHelper.V2.Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;

namespace Tweaks_Fixes
{
    internal static class BulkheadDoorPatch
    {
        public static long GetKey(string saveSlot)
        {
            long key = 0;

            if (!string.IsNullOrEmpty(saveSlot))
            {
                SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(saveSlot);
                if (gameInfo != null)
                    key = gameInfo.startTicks;
            }
            return key;
        }



        //[HarmonyPatch(typeof(BulkheadDoor), "OnHandHover")]
        static class BulkheadDoor_OnHandHover_Patch
        {
            public static void Postfix(BulkheadDoor __instance)
            {
                //ErrorMessage.AddDebug("isOpen " + __instance.isOpen);
                //ErrorMessage.AddDebug("initiallyOpen " + __instance.initiallyOpen);
            }
        }

        [HarmonyPatch(typeof(BulkheadDoor), "OnHandClick")]
        internal class BulkheadDoor_OnHandClick_Patch
        {
            public static void Postfix(BulkheadDoor __instance)
            {
                //SaveLoadManager.GameInfo gameInfo = SaveLoadManager.main.GetGameInfo(SaveLoadManager.main.currentSlot);
                //long dictKey = GetKey(SaveLoadManager.main.currentSlot);
                //PrefabIdentifier prefabIdentifier = __instance.GetComponent<PrefabIdentifier>();
                //if (prefabIdentifier)
                //{
                //    Main.config.openedWreckDoors[prefabIdentifier.id] = !__instance.isOpen;
                //}
                int doorKey = Mathf.RoundToInt(__instance.gameObject.transform.position.x + __instance.gameObject.transform.position.y + __instance.gameObject.transform.position.z);
                string slot = SaveLoadManager.main.currentSlot;
                if (!Main.config.openedWreckDoors.ContainsKey(slot))
                    Main.config.openedWreckDoors[slot] = new Dictionary<int, bool>();

                Main.config.openedWreckDoors[slot][doorKey] = !__instance.isOpen;
            }
        }

        [HarmonyPatch(typeof(BulkheadDoor), "Initialize")]
        internal class BulkheadDoor_Initialize_Patch
        {
            public static void Prefix(BulkheadDoor __instance)
            {
                //float doorKey = __instance.gameObject.transform.position.x + __instance.gameObject.transform.position.y;
                int doorKey = Mathf.RoundToInt(__instance.gameObject.transform.position.x + __instance.gameObject.transform.position.y + __instance.gameObject.transform.position.z);

                //Main.Log("doorKey " + doorKey);
                string slot = SaveLoadManager.main.currentSlot;
                if (Main.config.openedWreckDoors.ContainsKey(slot) && Main.config.openedWreckDoors[slot].ContainsKey(doorKey))
                {
                    //Main.Log("load door " + slot + " " + doorKey + " " + Main.config.openedWreckDoors[slot][doorKey]);
                    //ErrorMessage.AddDebug("load door " + slot + " " + doorKey + " " + Main.config.openedWreckDoors[slot][doorKey]);
                    __instance.initiallyOpen = Main.config.openedWreckDoors[slot][doorKey];
                }
            }
        }

    }
}