using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Starting_Loot_Patch
    {

        [HarmonyPatch(typeof(LootSpawner), "Start")]
        class LootSpawner_Start_Patch
        {
            public static void Postfix(LootSpawner __instance)
            {
                __instance.escapePodTechTypes = new List<TechType>();
                foreach (KeyValuePair<string, int> loot in Main.config.startingLoot)
                {
                    TechTypeExtensions.FromString(loot.Key, out TechType tt, false);
                    //Main.Log("Start Loot " + tt);
                    //ErrorMessage.AddDebug("Start Loot " + tt);
                    if (tt == TechType.None)
                        continue;

                    for (int i = 0; i < loot.Value; i++)
                        __instance.escapePodTechTypes.Add(tt);
                }
            }
        }


    }
}
