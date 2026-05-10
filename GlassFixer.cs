using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class GlassFixer
    {
        [HarmonyPatch(typeof(BaseDeconstructable))]
        class BaseDeconstructable_Patch
        {
            static readonly Dictionary<TechType, List<string>> glassRenderers = new Dictionary<TechType, List<string>> {
                { TechType.BaseWindow, new List<string> { "BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass", "BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass", "BaseRoomGenericInteriorWindowSide01/BaseExteriorRoomGenericWindowSide01Glass_LOD1", "BaseRoomGenericInteriorWindowSide01/BaseInteriorRoomGenericWindowSide01Glass_LOD1", "BaseHatchModel/BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseHatchModel/BaseCorridorInteriorWindowSide_LOD1/BaseCorridorInteriorWindowSide_ext_LOD1", "models/BaseCorridorInteriorWindowTop/BaseCorridorInteriorWindowTop_ext", "models/BaseCorridorInteriorWindowTop/BaseCorridorInteriorWindowTop_ext_LOD1", "models/BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "BaseCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1", "models/BaseCorridorXShapeExteriorWindowTop/BaseCorridorXShapeExteriorWindowTop_ext", "models/BaseCorridorXShapeExteriorWindowTop_LOD1/BaseCorridorXShapeExteriorWindowTop_ext_LOD1", "BaseMapRoomInteriorWindowSide/BaseMapRoomInteriorWindowSideGlass_int", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_01", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_01_LOD1", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_01", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_01_LOD1", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_02", "model/BaseRoomMoonPoolExteriorWindowSide01Glass_02_LOD1", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_02", "model/BaseRoomMoonPoolInteriorWindowSide01Glass_02_LOD1", "model/BaseLargeRoomWindowSide/LargeRoom_lExteriorWindowSide01Glass_01", "model/BaseLargeRoomWindowSide/LargeRoom_InteriorWindowSide01Glass_01", "model/BaseLargeRoomWindowSideShort/LargeRoom_lExteriorWindowSide01Glass_002", "model/BaseLargeRoomWindowSideShort/LargeRoom_InteriorWindowSide01Glass_002" } },
                { TechType.BaseHatch, new List<string> { "BaseCorridorHatch/underWater/model/BaseCorridorExteriorCapHatch/hatch_end_anim/hatch_geo", "underWater/model/BaseCorridorExteriorCapHatch/hatch_end_anim/hatch_geo", "models/BaseExteriorHatchTop/BaseExteriorHatchTop 1/hatch_top_anim/hatch_geo3", "model/hatch_side_anim/sideHatch_geo1", "models/hatch_bottom_anim/hatch_geo1", "BaseCorridorHatch/models/hatch_alienContainment_anims/hatchGlass_geo" } },
                { TechType.BaseGlassDome, new List<string> { "model/BaseRoomGenericExteriorCapTopGlassExterior", "model/BaseRoomGenericExteriorCapTopGlassInterior" } },
                { TechType.BaseLargeGlassDome, new List<string> { "model/LargeRoomExteriorTop_01/LargeRoomExteriorTop_01_glass", "model/LargeRoomExteriorTop_01/LargeRoomInteriorTop_01_glass" } },
                { TechType.BaseWaterPark, new List<string> { "model/Large_Aquarium_02_glass" } },
                { TechType.BaseFiltrationMachine, new List<string> { "model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass" } },
                };
            public static void FixWaterParkGlassRoof(Transform wp)
            {
                string[] wpFoofRenderers = new string[] { "model/BaseLargeWaterParkCeilingGlassDome/BaseWaterParkCeilingGlassDome_glass_ext", "model/BaseLargeWaterParkCeilingGlassDome/BaseWaterParkCeilingGlassDome_glass_int", "model/BaseWaterParkCeilingGlassGlass/BaseWaterParkCeilingGlass_geo" };
                foreach (Transform child in wp.parent)
                {
                    if (child.name.StartsWith("BaseLargeWaterParkCeilingGlass")) // has no BaseDeconstructable
                    {
                        foreach (string rendererName in wpFoofRenderers)
                        {
                            Transform t = child.Find(rendererName);
                            if (t != null)
                                t.DisableShadowCasting();
                        }
                    }
                }
            }

            //[HarmonyPostfix, HarmonyPatch("Awake")]
            static void AwakePostfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Awake " + __instance.recipe);
            }

            [HarmonyPostfix, HarmonyPatch("Init")]
            static void InitPostfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Init " + __instance.recipe);
                if (__instance.recipe == TechType.BaseWaterPark && __instance.name == "BaseLargeWaterParkWalls(Clone)")
                {
                    FixWaterParkGlassRoof(__instance.transform);
                }
                if (glassRenderers.ContainsKey(__instance.recipe))
                {
                    //Main.logger.LogDebug("BaseDeconstructable Init " + __instance.recipe);
                    foreach (string path in glassRenderers[__instance.recipe])
                        __instance.transform.DisableShadowCasting(path);
                }
            }
        }

        [HarmonyPatch(typeof(Leakable), "Start")]
        class Leakable_Start_Patch
        {
            static void Postfix(Leakable __instance)
            {
                //AddDebug("Leakable Start");
                Dictionary<TechType, List<string>> glassRenderers = new Dictionary<TechType, List<string>> { { TechType.BaseCorridorGlassL, new List<string> { "models/BaseCorridorLShapeGlassExterior/BaseCorridorLShapeGlassExteriorGlass", "models/BaseCorridorLShapeGlassExterior_LOD1/BaseCorridorLShapeGlassExteriorGlass_LOD1" } },
                    { TechType.BaseObservatory, new List<string> { "Room_Observatory/BaseRoomObservatory_glass", "Room_Observatory/BaseRoomObservatory_glass_LOD1" } },
                    //{ TechType.BaseConnector, new List<string> { "" } }, same renderer for tube and glass
                    { TechType.BaseCorridorGlassI, new List<string> { "models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass", "models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass_LOD1" } },
                };
                BaseDeconstructable[] bds = __instance.transform.GetComponentsInChildren<BaseDeconstructable>();
                foreach (var bd in bds)
                {
                    //AddDebug("Leakable Start " + bd.recipe);
                    if (glassRenderers.ContainsKey(bd.recipe))
                    {
                        //Main.logger.LogDebug("Leakable Start " + bd.recipe);
                        foreach (string path in glassRenderers[bd.recipe])
                            bd.transform.DisableShadowCasting(path);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Patch
        {
            static Dictionary<TechType, List<string>> glassRenderers = new Dictionary<TechType, List<string>> {
            {TechType.Aquarium, new List < string > { "model/Aquarium_animation2/Aquarium_geo/Aquarium_glass" } },
            {TechType.BarTable, new List < string > { "descent_bar_table_01/descent_bar_table_01_glass" } },
            {TechType.Locker, new List < string > { "model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_R/submarine_Storage_locker_big_01_door_R", "model/submarine_Storage_locker_big_01/submarine_Storage_locker_big_01_hinges_L/submarine_Storage_locker_big_01_door_L" } }};

            [HarmonyPostfix, HarmonyPatch("NotifyConstructedChanged")]
            public static void NotifyConstructedChangedPostfix(Constructable __instance, bool constructed)
            {
                if (!constructed)
                    return;

                //AddDebug("Constructable NotifyConstructedChanged " + __instance.techType);
                if (glassRenderers.ContainsKey(__instance.techType))
                    __instance.transform.DisableShadowCasting(glassRenderers[__instance.techType]);
            }
        }

    }
}
