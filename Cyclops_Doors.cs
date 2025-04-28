using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Cyclops_Doors
    {
        static Dictionary<string, float> closedDoorsAngles = new Dictionary<string, float>() { { "submarine_hatch_01", 270f }, { "submarine_hatch_02 1", 180f }, { "submarine_hatch_02", 180f }, { "submarine_hatch_02 7", 0 }, { "submarine_hatch_02 3", 180f }, { "submarine_hatch_02 4", 0 } };

        static bool IsDoorSaved(Openable openable)
        {
            if (!closedDoorsAngles.ContainsKey(openable.name))
                return false;

            PrefabIdentifier prefabIdentifier = openable.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null || prefabIdentifier.name == "__LIGHTMAPPED_PREFAB__")
                return false;

            //AddDebug("IsDoorClosed prefabIdentifier " + prefabIdentifier.name);
            //Main.logger.LogMessage("IsDoorClosed prefabIdentifier " + prefabIdentifier.name);
            return Main.configMain.GetCyclopsDoor(prefabIdentifier.id, openable.name);
        }

        [HarmonyPatch(typeof(Openable), "Start")]
        class Openable_Start_Patch
        {
            static void Prefix(Openable __instance)
            {
                //Main.logger.LogMessage("Openable Start " + __instance.name);
                if (IsDoorSaved(__instance))
                {
                    CloseDoor(__instance);
                }
            }

            private static void CloseDoor(Openable openable)
            {
                openable.isOpen = false;
                Vector3 rot = openable.transform.localEulerAngles;
                float y = closedDoorsAngles[openable.name];
                openable.transform.localEulerAngles = new Vector3(rot.x, y, rot.z);
            }
        }

        static void SaveClosedDoor(Openable openable)
        {
            PrefabIdentifier prefabIdentifier = openable.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                return;

            //AddDebug(openable.name + " SaveClosedDoor  prefabIdentifier.id " + prefabIdentifier.id);
            Main.configMain.SaveCyclopsDoor(prefabIdentifier.id, openable.name);
        }

        static void DeleteSavedDoor(Openable openable)
        {
            PrefabIdentifier prefabIdentifier = openable.GetComponentInParent<PrefabIdentifier>();
            if (prefabIdentifier == null)
                return;

            //AddDebug(openable.name + " DeleteSavedDoor  prefabIdentifier.id " + prefabIdentifier.id);
            Main.configMain.DeleteCyclopsDoor(prefabIdentifier.id, openable.name);
        }

        [HarmonyPatch(typeof(Openable), "PlayOpenAnimation")]
        class Openable_PlayOpenAnimation_Patch
        {
            static void Postfix(Openable __instance, bool openState, float duration)
            {
                if (!closedDoorsAngles.ContainsKey(__instance.name))
                    return;

                if (openState)
                    DeleteSavedDoor(__instance);
                else
                    SaveClosedDoor(__instance);
            }
        }


    }
}
