using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tweaks_Fixes;
using UnityEngine;

namespace Tweaks_and_Fixes
{
    internal class Scanner_Patch
    {

        [HarmonyPatch(typeof(ScannerTool))]
        class ScannerTool_Patch
        {
            static bool scanning = false;
            static bool finishedScan = false;

            [HarmonyPrefix]
            [HarmonyPatch("Update")]// Show power when equipped
            private static bool UpdatePrefix(ScannerTool __instance)
            {
                //PlayerTool playerTool = 
                //bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
                if (__instance.isDrawn)
                {
                    //float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
                    //AddDebug("useText1 " + HandReticle.main.useText1);
                    //AddDebug("useText2 " + HandReticle.main.useText2);
                    if (__instance.idleTimer > 0f)
                    {
                        __instance.idleTimer = Mathf.Max(0f, __instance.idleTimer - Time.deltaTime);
                        //string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
                        //               HandReticle.main.SetUseTextRaw(buttonFormat, null);
                    }
                }
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Scan")]
            private static void ScanPostfix(ScannerTool __instance, PDAScanner.Result __result)
            { // -57 -23 -364
                //if (__result != PDAScanner.Result.None)
                //    AddDebug("Scan " + __result.ToString());
                if (Main.config.removeFragmentCrate && finishedScan && PDAScanner.scanTarget.techType == TechType.StarshipCargoCrate)
                { // destroy crate
                    //AddDebug("finished scan " + __result.ToString() + " " + PDAScanner.scanTarget.gameObject.name);
                    UnityEngine.Object.Destroy(PDAScanner.scanTarget.gameObject);
                    scanning = false;
                    finishedScan = false;
                    return;
                }
                if (scanning && __result == PDAScanner.Result.Done || __result == PDAScanner.Result.Researched)
                    finishedScan = true;
                else
                    finishedScan = false;

                scanning = PDAScanner.IsFragment(PDAScanner.scanTarget.techType);
            }
        }

        [HarmonyPatch(typeof(PDAScanner))]
        class PDAScanner_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            static void InitializePostfix()
            {
                if (PDAScanner.mapping.ContainsKey(TechType.MediumKoosh))
                {
                    PDAScanner.EntryData entryData = PDAScanner.mapping[TechType.MediumKoosh];
                    //Main.logger.LogDebug("PDAScanner Initialize " + entryData);
                    if (!PDAScanner.mapping.ContainsKey(TechType.LargeKoosh))
                        PDAScanner.mapping.Add(TechType.LargeKoosh, entryData);

                    if (!PDAScanner.mapping.ContainsKey(TechType.SmallKoosh))
                        PDAScanner.mapping.Add(TechType.SmallKoosh, entryData);
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("Unlock")]
            static void UnlockPostfix(PDAScanner.EntryData entryData, bool unlockBlueprint, bool unlockEncyclopedia, bool verbose)
            {
                //AddDebug("Unlock " + entryData.key);
                if (entryData.key == TechType.MediumKoosh || entryData.key == TechType.SmallKoosh || entryData.key == TechType.LargeKoosh)
                {
                    PDAScanner.complete.Add(TechType.LargeKoosh);
                    PDAScanner.complete.Add(TechType.SmallKoosh);
                    PDAScanner.complete.Add(TechType.MediumKoosh);
                }
            }
        }


    }
}
