using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Trashcan_
    {
        [HarmonyPatch(typeof(Trashcan))]
        class Trashcan_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnEnable")]
            public static void AwakePostfix(Trashcan __instance)
            {
                //AddDebug($"Trashcan OnEnable container {__instance.storageContainer.container.count} wasteList {__instance.wasteList.Count}");
                __instance.startDestroyTimeOut *= ConfigMenu.timeFlowSpeed.Value;
                __instance.destroyInterval *= ConfigMenu.timeFlowSpeed.Value;
                if (__instance.wasteList.Count < __instance.storageContainer.container.count)
                {
                    foreach (var kv in __instance.storageContainer.container._items)
                    {
                        foreach (InventoryItem item in kv.Value.items)
                        {
                            //AddDebug($"Trashcan item " + item.techType);
                            __instance.AddItem(item);
                        }
                    }
                }
            }

        }
    }
}
