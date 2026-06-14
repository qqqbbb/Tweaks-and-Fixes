using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Fragment
    {
        //SeamothFragment ExosuitFragment WorkbenchFragment BaseFiltrationMachineFragment CyclopsHullFragment CyclopsBridgeFragment CyclopsEngineFragment ConstructorFragment BaseUpgradeConsoleFragment BaseWaterParkFragment BaseBulkheadFragment BatteryChargerFragment PictureFrameFragment BeaconFragment GravsphereFragment LaserCutterFragment ConstructorFragment PrecursorDroid

        static bool IsFragmentCrate(Transform transform)
        {
            return transform.name.EndsWith("InCrate(Clone)") || transform.name.EndsWith("Fragment(Clone)");
        }

        private static void TestDecals(GameObject go)
        {
            foreach (Renderer renderer in go.GetAllComponentsInChildren<Renderer>())
            {
                foreach (var m in renderer.materials)
                {
                    if (m.GetFloat(PrefabFixer.zOffset) < 0)
                    {
                        TechType tt = CraftData.GetTechType(go);
                        AddDebug("TestDecals " + tt);
                        Main.logger.LogDebug($"TestDecals {tt} {go.name} {renderer.name} {m.name}");
                        //m.EnableKeyword("MARMO_ALPHA_CLIP");
                        //m.SetFloat(zOffset, 0);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ResourceTracker), "Start")]
        class ResourceTracker_Start_Patch
        {
            static void Postfix(ResourceTracker __instance)
            {
                if (__instance.techType != TechType.Fragment)
                    return;

                if (ConfigToEdit.dontSpawnKnownFragments.Value)
                {
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    if (PDAScanner.complete.Contains(tt))
                    {
                        //AddDebug("ResourceTracker start " + tt);
                        __instance.Unregister();
                        if (IsFragmentCrate(__instance.transform.parent))
                        { // destroy fragment and crate
                            Util.DestroyEntity(__instance.transform.parent.gameObject);
                        }
                        else
                            Util.DestroyEntity(__instance.gameObject);
                    }
                }
            }
        }

    }
}
