using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class AuroraDecalFix
    {
        internal class DecalFixData
        {
            public readonly string rendererName;
            public readonly int[] materialIndices;
            public Dictionary<int, int> zOffsetvalues;

            public DecalFixData(string rendererName, int[] materialIndices)
            {
                this.rendererName = rendererName;
                this.materialIndices = materialIndices;
            }

            public DecalFixData(string rendererName, int[] materialIndices, Dictionary<int, int> zOffsetvalues)
            {
                this.rendererName = rendererName;
                this.materialIndices = materialIndices;
                this.zOffsetvalues = zOffsetvalues;
            }
        }

        static readonly int zOffset = Shader.PropertyToID("_ZOffset");
        static Dictionary<string, string> renderersToDisableShadowCasting = new Dictionary<string, string>
        {
            {"Starship_cargo_damaged_opened_large_01(Clone)", "Starship_cargo_damaged_opened_01/dirt_01" }, // 1016 1 3
            {"Starship_cargo_damaged_opened_large_02(Clone)", "Starship_cargo_damaged_opened_02/dirt_02" },
            {"Starship_cargo_damaged_opened_01(Clone)", "Starship_cargo_damaged_opened_01/dirt_01" },
            {"Starship_cargo_damaged_opened_02(Clone)", "Starship_cargo_damaged_opened_02/dirt_02" },
            { "CrashedShip_seamoth_room(Clone)", "seamoth_room/seamoth_room 1" },
            { "CrashedShip_exo_room(Clone)", "exo_room/exo_room/exo_room_MeshPart1" },
        };
        static Dictionary<string, string> renderersToEnableDitherAlpha = new Dictionary<string, string> {
            {"Starship_cargo_large_02(Clone)" , "Starship_cargo_02" },
            {"Starship_cargo_02(Clone)" , "Starship_cargo_02" },
        };

        static Dictionary<string, DecalFixData> materialsToEnableDitherAlpha = new Dictionary<string, DecalFixData> {
            {"CrashedShip_entrance_01_02(Clone)" , new DecalFixData("entrance_01_02", new int[]{ 3 })},
        };

        static Dictionary<string, List<DecalFixData>> materialsToEnableAlphaClip = new Dictionary<string, List<DecalFixData>> {
             {"CrashedShip_elevator_room(Clone)" ,new List<DecalFixData>{ new DecalFixData("cargo_elevator/cargo_elevator", new int[]{ 2 }) } },
             {"CrashedShip_entrance_01_01(Clone)" ,new List<DecalFixData>{ new DecalFixData("entrance_01_01", new int[]{ 7 }) } },
             {"CrashedShip_entrance_01_02(Clone)" ,new List<DecalFixData>{ new DecalFixData("entrance_01_02", new int[]{ 2, 21 }) } },
             {"CrashedShip_entrance_01_03(Clone)" ,new List<DecalFixData>{ new DecalFixData("entrance_01_03", new int[]{ 1 }) } },
             {"CrashedShip_power_corridors(Clone)" ,new List<DecalFixData>{ new DecalFixData("power_corridors/corridors/corridors_MeshPart1", new int[]{ 9 }),
              new DecalFixData("power_corridors/corridors/corridors_MeshPart0", new int[]{ 1})
             } },

             {"CrashedShip_entrance_02_01(Clone)" ,new List<DecalFixData>{ new DecalFixData("entrance_02_01/entrance_02_01_MeshPart0", new int[]{16, 17, 18}),
                 new DecalFixData("entrance_02_01/entrance_02_01_MeshPart1", new int[]{11})
             }},
             {"CrashedShip_entrance_02_02(Clone)", new List<DecalFixData>{ new DecalFixData("entrance_02_02", new int[]{8})}},
             {"CrashedShip_locker_room(Clone)" ,new List<DecalFixData>{ new DecalFixData("locker_room/locker_room 2_MeshPart0", new int[]{ 4 }, new Dictionary<int, int> { {4, 500 } }) } },
             {"CrashedShip_interior_T_room(Clone)" ,new List<DecalFixData>{new DecalFixData("starship_exploded_interior_T_room", new int[]{ 0, 12,15,17,18,21, 22, 23, 35,36 }, new Dictionary<int, int> { {35, 2000 },{ 36, 3000 } }) } },
             { "Starship_exploded_debris_19(Clone)" ,new List<DecalFixData>{new DecalFixData("Starship_exploded_debris_19", new int[]{ 1 })} },
             {"Aurora" ,new List<DecalFixData>{new DecalFixData("starship_expoded/model_LODs/starship_exploded_02/starship_exploded_interior_03", new int[]{ 4 }) } },

             {"CrashedShip_power_room(Clone)" ,new List<DecalFixData>{new DecalFixData("power_room/starship_exploded_interior_power_room_01/starship_exploded_interior_power_room_01_MeshPart1", new int[]{ 16,17 }),
                 new DecalFixData("power_room/starship_exploded_interior_power_room_02/starship_exploded_interior_power_room_02_MeshPart2", new int[] { 3, 4 }) } },

             {"CrashedShip_exo_room(Clone)" ,new List<DecalFixData>{new DecalFixData("exo_room/exo_room/exo_room_MeshPart0", new int[]{ 7 }),
                 new DecalFixData("exo_room/exo_room/exo_room_MeshPart3", new int[]{ 5 })
             } },
             {"CrashedShip_entrance_03(Clone)" ,new List<DecalFixData>{new DecalFixData("entrance_03/entrance_03_MeshPart1", new int[]{ 1, 3, 4, 5, 6, 8, 10, 19, 20, 21, 22}),
                 new DecalFixData("entrance_03/entrance_03_MeshPart0", new int[] {3, 22 }) } },
        };

        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (materialsToEnableAlphaClip.ContainsKey(__instance.name))
                    EnableAlphaClip(__instance.gameObject);

                if (renderersToDisableShadowCasting.ContainsKey(__instance.name))
                    DisableShadowCasting(__instance.gameObject, renderersToDisableShadowCasting[__instance.name]);
                else if (renderersToEnableDitherAlpha.ContainsKey(__instance.name))
                    EnableDitherAlpha(__instance.gameObject, renderersToEnableDitherAlpha[__instance.name]);

                if (materialsToEnableDitherAlpha.ContainsKey(__instance.name))
                    EnableDitherAlpha(__instance.gameObject);
            }

            private static void EnableDitherAlpha(GameObject go)
            {
                if (materialsToEnableDitherAlpha.ContainsKey(go.name) == false)
                {
                    AddDebug($"EnableDitherAlpha no DecalFixData for {go.name} ");
                    Main.logger.LogMessage($"EnableDitherAlpha no DecalFixData for {go.name} ");
                    return;
                }
                DecalFixData data = materialsToEnableDitherAlpha[go.name];
                Transform t = go.transform.Find(data.rendererName);
                if (t != null)
                {
                    MeshRenderer mr = t.GetComponent<MeshRenderer>();
                    foreach (int index in data.materialIndices)
                    {
                        Material material = mr.materials[index];
                        material.EnableKeyword("UWE_DITHERALPHA");
                        material.SetFloat(zOffset, 0);
                    }
                }
            }

            private static void EnableDitherAlpha(GameObject go, string child)
            {
                Transform t = go.transform.Find(child);
                if (t != null)
                { // fix: word 'cargo' covered on crate
                    MeshRenderer mr = t.GetComponent<MeshRenderer>();
                    mr.material.EnableKeyword("UWE_DITHERALPHA");
                }
            }
        }

        private static void EnableAlphaClip(GameObject go)
        {
            if (materialsToEnableAlphaClip[go.name] == null)
            {
                AddDebug($"EnableAlphaClip no DecalFixData for {go.name} ");
                Main.logger.LogMessage($"EnableAlphaClip no DecalFixData for {go.name} ");
                return;
            }
            foreach (DecalFixData data in materialsToEnableAlphaClip[go.name])
            {
                string name = data.rendererName;
                Transform t = go.transform.Find(name);
                if (t == null)
                {
                    AddDebug($"EnableAlphaClip  {name}  was not found on {go.name}");
                    Main.logger.LogMessage($"EnableAlphaClip  {name}  was not found on {go.name}");
                    return;
                }
                MeshRenderer mr = t.GetComponent<MeshRenderer>();
                foreach (int index in data.materialIndices)
                {
                    int value = 0;
                    if (data.zOffsetvalues != null && data.zOffsetvalues.ContainsKey(index))
                        value = data.zOffsetvalues[index];

                    Material material = mr.materials[index];
                    material.EnableKeyword("MARMO_ALPHA_CLIP");
                    material.SetFloat(zOffset, value);
                }
            }
        }

        private static void DisableShadowCastingInChildren(GameObject go, string child)
        {
            Transform t = go.transform.Find(child);
            if (t == null)
            {
                AddDebug($"DisableChildrenShadowCasting  {child}  was not found on {go}");
                Main.logger.LogMessage($"DisableChildrenShadowCasting  {child}  was not found on {go}");
                return;
            }
            MeshRenderer[] mrs = t.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in mrs)
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static void DisableShadowCasting(GameObject go, string[] children)
        {
            foreach (string name in children)
            {
                Transform t = go.transform.Find(name);
                if (t == null)
                {
                    AddDebug($"DisableShadowCasting  {name}  was not found on {go}");
                    Main.logger.LogMessage($"DisableShadowCasting  {name}  was not found on {go}");
                    return;
                }
                MeshRenderer mr = t.GetComponent<MeshRenderer>();
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        private static void DisableShadowCasting(GameObject go, string child)
        {
            Transform t = go.transform.Find(child);
            if (t == null)
            {
                AddDebug($"DisableShadowCasting  {child}  was not found on {go}");
                Main.logger.LogMessage($"DisableShadowCasting  {child}  was not found on {go}");
                return;
            }
            MeshRenderer mr = t.GetComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        [HarmonyPatch(typeof(CrashedShipExploder), "Start")]
        class CrashedShipExploder_Start_Patch
        {
            static void Postfix(CrashedShipExploder __instance)
            {
                DisableShadowCasting(__instance.gameObject, "starship_expoded/model_LODs/starship_exploded_02/starship_exploded_interior_decals");
                DisableShadowCastingInChildren(__instance.gameObject, "explodedFX/Ship_Interior_PowerRoomFX(Clone)");
                __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.metal);
            }
        }

    }
}
