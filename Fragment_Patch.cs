using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Fragment_Patch
    {

        //[HarmonyPatch(typeof(CSVEntitySpawner), nameof(CSVEntitySpawner.GetPrefabForSlot))]
        internal class CSVEntitySpawner_GetPrefabForSlot_Patch
        {
            public static bool Prefix(CSVEntitySpawner __instance, IEntitySlot slot, ref EntitySlot.Filler __result, bool filterKnown)
            {
                EntitySlot.Filler filler = new EntitySlot.Filler();
                filler.classId = null;
                filler.count = 0;
                LootDistributionData.DstData data1;
                if (__instance.lootDistribution.GetBiomeLoot(slot.GetBiomeType(), out data1))
                {
                    if (CSVEntitySpawner.sData.Count > 0)
                        CSVEntitySpawner.sData.Clear();
                    float num1 = 0.0f;
                    float num2 = 0.0f;
                    Dictionary<string, LootDistributionData.SrcData> srcDistribution = __instance.lootDistribution.srcDistribution;
                    int index1 = 0;
                    for (int count = data1.prefabs.Count; index1 < count; ++index1)
                    {
                        LootDistributionData.PrefabData prefab = data1.prefabs[index1];
                        if (!string.Equals(prefab.classId, "None") && srcDistribution.TryGetValue(prefab.classId, out LootDistributionData.SrcData _))
                        {
                            UWE.WorldEntityInfo info;
                            if (!UWE.WorldEntityDatabase.TryGetInfo(prefab.classId, out info))
                                UnityEngine.Debug.LogErrorFormat(__instance, "Missing world entity info for prefab '{0}'", prefab.classId);
                            else if (slot.IsTypeAllowed(info.slotType))
                            {
                                float num3 = prefab.probability / slot.GetDensity();
                                if (num3 > 0f)
                                {
                                    TechType techType = info.techType;
                                    bool flag = false;
                                    if (filterKnown)
                                    {
                                        flag = PDAScanner.IsFragment(techType);
                                        if (flag && PDAScanner.ContainsCompleteEntry(techType))
                                        {
                                            if (Main.config.dontSpawnKnownFragments)
                                            {
                                                AddDebug("DONT LOAD " + techType);
                                                __result = filler;
                                                return false;
                                            }
                                            num2 += num3;
                                            continue;
                                        }
                                    }
                                    CSVEntitySpawner.sData.Add(new CSVEntitySpawner.Data()
                                    {
                                        classId = prefab.classId,
                                        count = prefab.count,
                                        probability = num3,
                                        isFragment = flag
                                    });
                                    if (flag)
                                        num1 += num3;
                                }
                            }
                        }
                    }
                    bool flag1 = num2 > 0f && num1 > 0f;
                    float num4 = flag1 ? (num2 + num1) / num1 : 1f;
                    float num5 = 0f;
                    for (int index2 = 0; index2 < CSVEntitySpawner.sData.Count; ++index2)
                    {
                        CSVEntitySpawner.Data data2 = CSVEntitySpawner.sData[index2];
                        if (flag1 && data2.isFragment)
                        {
                            data2.probability *= num4;
                            CSVEntitySpawner.sData[index2] = data2;
                        }
                        num5 += data2.probability;
                    }
                    CSVEntitySpawner.Data data3;
                    data3.count = 0;
                    data3.classId = null;
                    if (num5 > 0f)
                    {
                        float num3 = UnityEngine.Random.value;
                        if (num5 > 1f)
                            num3 *= num5;
                        float num6 = 0f;
                        for (int index2 = 0; index2 < CSVEntitySpawner.sData.Count; ++index2)
                        {
                            CSVEntitySpawner.Data data2 = CSVEntitySpawner.sData[index2];
                            num6 += data2.probability;
                            if (num6 >= num3)
                            {
                                data3 = data2;
                                break;
                            }
                        }
                    }
                    CSVEntitySpawner.sData.Clear();
                    if (data3.count > 0)
                    {
                        filler.classId = data3.classId;
                        filler.count = data3.count;
                    }
                }
                __result = filler;
                return false;
            }

        }

        [HarmonyPatch(typeof(ResourceTracker), "Start")]
        class ResourceTracker_Start_Patch
        {
            static void Postfix(ResourceTracker __instance)
            {
                if (Main.config.dontSpawnKnownFragments && __instance.techType == TechType.Fragment)
                {
                    TechType tt = CraftData.GetTechType(__instance.gameObject);

                    if (PDAScanner.complete.Contains(tt))
                    {
                        //AddDebug("ResourceTracker start " + __instance.techType + " " + CraftData.GetTechType(__instance.gameObject));
                        __instance.Unregister();
                        //AddDebug("Destroy " + tt);
                        if (__instance.transform.parent.name == "CellRoot(Clone)")
                            UnityEngine.Object.Destroy(__instance.gameObject);
                        else // destroy fragment and crate
                            UnityEngine.Object.Destroy(__instance.transform.parent.gameObject);
                    }
                }
            }
        }
    }
}
