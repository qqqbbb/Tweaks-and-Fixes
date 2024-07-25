using FMOD;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class CyclopsDoorSaver
    {
        static Dictionary<string, float> closedDoorsYangles = new Dictionary<string, float>() { { "submarine_hatch_01", 270f }, { "submarine_hatch_02 1", 180f }, { "submarine_hatch_02", 180f }, { "submarine_hatch_02 7", 0 }, { "submarine_hatch_02 3", 180f }, { "submarine_hatch_02 4", 0 } };

        static bool IsDoorClosed(Openable openable)
        {
            if (!closedDoorsYangles.ContainsKey(openable.name) || !Main.configMain.cyclopsDoors.ContainsKey(SaveLoadManager.main.currentSlot))
                return false;

            //GameObject cyclopsRoot = Util.GetEntityRoot(openable.gameObject);
            //if (cyclopsRoot == null)
            //    return false;

            PrefabIdentifier prefabIdentifier = openable.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null || prefabIdentifier.name == "__LIGHTMAPPED_PREFAB__")
                return false;

            //AddDebug("IsDoorClosed prefabIdentifier " + prefabIdentifier.name);
            //Main.logger.LogMessage("IsDoorClosed prefabIdentifier " + prefabIdentifier.name);
            var saved = Main.configMain.cyclopsDoors[SaveLoadManager.main.currentSlot];
            return saved.ContainsKey(prefabIdentifier.id) && saved[prefabIdentifier.id].Contains(openable.name);
        }

        [HarmonyPatch(typeof(Openable), "Start")]
        class Openable_Start_Patch
        {
            static void Prefix(Openable __instance)
            {
                //Main.logger.LogMessage("Openable Start " + __instance.name);
                if (closedDoorsYangles.ContainsKey(__instance.name) && IsDoorClosed(__instance))
                {
                    CloseDoor(__instance);
                }
            }

            private static void CloseDoor(Openable openable)
            {
                openable.isOpen = false;
                Vector3 rot = openable.transform.localEulerAngles;
                float y = closedDoorsYangles[openable.name];
                openable.transform.localEulerAngles = new Vector3(rot.x, y, rot.z);
            }
        }

        static void SaveClosedDoor(Openable openable)
        {
            PrefabIdentifier prefabIdentifier = openable.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                return;

            //AddDebug(openable.name + " SaveClosedDoor  prefabIdentifier.id " + prefabIdentifier.id);
            string slot = SaveLoadManager.main.currentSlot;
            if (!Main.configMain.cyclopsDoors.ContainsKey(slot))
                Main.configMain.cyclopsDoors[slot] = new Dictionary<string, HashSet<string>>();

            if (Main.configMain.cyclopsDoors[slot].ContainsKey(prefabIdentifier.id))
                Main.configMain.cyclopsDoors[slot][prefabIdentifier.id].Add(openable.name);
            else
                Main.configMain.cyclopsDoors[slot][prefabIdentifier.id] = new HashSet<string>() { openable.name };
        }

        static void DeleteSavedDoor(Openable openable)
        {
            PrefabIdentifier prefabIdentifier = openable.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                return;

            //AddDebug(openable.name + " DeleteSavedDoor  prefabIdentifier.id " + prefabIdentifier.id);
            string slot = SaveLoadManager.main.currentSlot;
            if (!Main.configMain.cyclopsDoors.ContainsKey(slot))
                return;

            if (Main.configMain.cyclopsDoors[slot].ContainsKey(prefabIdentifier.id))
                Main.configMain.cyclopsDoors[slot][prefabIdentifier.id].Remove(openable.name);
        }

        [HarmonyPatch(typeof(Openable), "PlayOpenAnimation")]
        class Openable_PlayOpenAnimation_Patch
        {
            static void Postfix(Openable __instance, bool openState, float duration)
            {
                if (!closedDoorsYangles.ContainsKey(__instance.name))
                    return;

                if (openState == false)
                    SaveClosedDoor(__instance);
                else
                    DeleteSavedDoor(__instance);
            }
        }


    }
}
