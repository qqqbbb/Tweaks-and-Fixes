using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Precurcor_Patch
    {
        public static Dictionary<StoryHandTarget, PrecursorComputerTerminal> used = new Dictionary<StoryHandTarget, PrecursorComputerTerminal>();

        [HarmonyPatch(typeof(StoryHandTarget), "OnHandHover")]
        class StoryHandTarget_OnHandHover_Patch
        {
            static bool Prefix(StoryHandTarget __instance)
            {// do not prompt player if used terminal
                if (used.ContainsKey(__instance))
                {
                    PrecursorComputerTerminal pct = used[__instance];
                    if (pct && pct.used)
                        return false;
                }
                else
                {
                    PrecursorComputerTerminal pct = __instance.GetComponent<PrecursorComputerTerminal>();
                    if (pct)
                    {
                        used[__instance] = pct;
                        if (pct.used)
                            return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PrecursorGunStoryEvents), "Update")]
        class PrecursorGunStoryEvents_Update_Patch
        {
            static Transform tr;
            public static void Postfix(PrecursorGunStoryEvents __instance)
            { // fix: player can see one sided outer wall in precursor gun moonpool
                if (tr == null)
                {
                    tr = __instance.transform.Find("precursor_base/Instances/precursor_base_15");
                    return;
                }
                if (Player.main.precursorOutOfWater)
                {
                    if (tr.gameObject.activeSelf)
                    {
                        tr.gameObject.SetActive(false);
                        //AddDebug("disable mesh");
                    }
                }
                else if (tr.gameObject.activeSelf == false)
                {
                    //AddDebug("enable mesh");
                    tr.gameObject.SetActive(true);
                }
            }
        }

        [HarmonyPatch(typeof(DisableEmissiveOnStoryGoal), "Start")]
        class DisableEmissiveOnStoryGoal_Start_Patch
        {
            public static void Postfix(DisableEmissiveOnStoryGoal __instance)
            {// fix: player bumps into collider on roof above entrance to precursor gun
                if (__instance.name == "Precursor_gun(Clone)")
                {
                    Transform parent = __instance.transform.Find("precursor_base/Collisions/precursor_base_06_collisions");
                    Transform tr = parent.Find("Cube (15)");
                    tr.position = new Vector3(tr.position.x, tr.position.y - .3f, tr.position.z);
                    tr = parent.Find("Cube (458)");
                    tr.position = new Vector3(tr.position.x, tr.position.y - .3f, tr.position.z);
                }
            }
        }


        //[HarmonyPatch(typeof(AnteChamber), "Start")]
        class AnteChamber_Start_Patch
        {
            public static bool Prefix(AnteChamber __instance)
            {
                //if (ConfigToEdit.disableIonCubeFabricator.Value)
                {
                    __instance.drillable.deleteWhenDrilled = true;
                    //AddDebug("AnteChamber Start return false");
                    return false;
                }
                //return true;
            }
            public static void Postfix(AnteChamber __instance)
            {
                //return !ConfigToEdit.disableIonCubeFabricator.Value;
                //if (ConfigToEdit.disableIonCubeFabricator.Value)
                {
                    __instance.drillable.deleteWhenDrilled = true;
                }
            }
        }

        //[HarmonyPatch(typeof(AnteChamber), "OnDrilled")]
        class AnteChamber_OnDrilled_Patch
        {
            public static bool Prefix(AnteChamber __instance)
            {
                //UnityEngine.Object.Destroy(__instance.drillable.gameObject);
                //return !ConfigToEdit.disableIonCubeFabricator.Value;
                return false;
            }
        }


    }
}
