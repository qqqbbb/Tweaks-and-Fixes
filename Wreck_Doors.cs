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
        [HarmonyPatch(typeof(BulkheadDoor))]
        class BulkheadDoor_Patch
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

        [HarmonyPatch(typeof(LaserCutObject), "CutOpenDoor")]
        class LaserCutObject_CutOpenDoor_Patch
        {
            private static void Prefix(LaserCutObject __instance)
            {
                if (ConfigToEdit.disableHotMetalGlow.Value)
                {
                    float delay = 20f;
                    if (__instance.isCutOpen)
                        delay = 0;

                    UWE.CoroutineHost.StartCoroutine(DisableGlowShader(__instance.gameObject, delay));
                }
            }
        }

        private static IEnumerator DisableGlowShader(GameObject gameObject, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (gameObject == null)
                yield break;

            foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.materials)
                    m.DisableKeyword("MARMO_EMISSION");
            }
        }
    }
}