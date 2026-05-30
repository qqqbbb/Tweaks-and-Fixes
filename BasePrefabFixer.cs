using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class BasePrefabFixer
    {
        public static bool basePrefabsFixed;

        public IEnumerator FixBasePrefabs()
        {
            yield return new WaitUntil(() => Base.initialized);

            RendererData hatchRendererData = new RendererData("BaseCorridorHatch/underWater/model/BaseCorridorExteriorCapHatch/hatch_end_anim/hatch_geo");
            RendererData acHatchRendererData = new RendererData("BaseCorridorHatch/models/hatch_alienContainment_anim/hatchGlass_geo");
            RendererData acLargeHatchRendererData = new RendererData("BaseCorridorHatch/models/hatch_alienContainment_anims/hatchGlass_geo");
            RendererData bottomHatchRendererData = new RendererData("models/hatch_bottom_anim/hatch_geo1");
            RendererData topHatchRendererData = new RendererData("models/BaseExteriorHatchTop/BaseExteriorHatchTop 1/hatch_top_anim/hatch_geo3");
            RendererData filtrationMachineRendererData = new RendererData("model/Water_Filtration_Machine/Water_Filtration_Machine_flat/water_filtration_machine_glass");

            Dictionary<string, RendererData> baseGlassRenderers = new Dictionary<string, RendererData> {
            { "BaseCorridorLShapeGlass", new RendererData("models", new List<string> { "BaseCorridorLShapeGlassExterior/BaseCorridorLShapeGlassExteriorGlass", "BaseCorridorLShapeGlassExterior_LOD1/BaseCorridorLShapeGlassExteriorGlass_LOD1"  })},

            { "BaseObservatory", new RendererData("Room_Observatory",new List<string> { "BaseRoomObservatory_glass", "BaseRoomObservatory_glass_LOD1" })},

            { "BaseCorridorIShapeGlass", new RendererData("models/BaseCorridorhIShapeGlass01Exterior",new List<string> { "BaseCorridorhIShapeGlass01ExteriorGlass", "BaseCorridorhIShapeGlass01ExteriorGlass_LOD1" })},

            { "BaseWaterParkBottom", new RendererData("model/Large_Aquarium_generic_room_glass_01")},

            { "BaseLargeWaterParkWalls", new RendererData("model/Large_Aquarium_02_glass")},

            { "BaseWaterParkCeilingGlass", new RendererData("BaseWaterParkCeilingGlass/model",new List<string> { "Large_Aquarium_generic_room_glass_02 (1)", "Large_Aquarium_generic_room_glass_02"})},

            { "BaseWaterParkCeilingGlassDome", new RendererData("BaseWaterParkCeilingGlass/model/BaseWaterParkCeilingGlassDome",new List<string> { "BaseWaterParkCeilingGlassDome_glass_ext", "BaseWaterParkCeilingGlassDome_glass_int"})},

            { "BaseRoomExteriorTopGlass", new RendererData("model",new List<string> { "BaseRoomGenericExteriorCapTopGlassExterior", "BaseRoomGenericExteriorCapTopGlassInterior"})},

            { "BaseLargeRoomExteriorTopGlass", new RendererData("model/LargeRoomExteriorTop_01", new List<string> { "LargeRoomExteriorTop_01_glass", "LargeRoomInteriorTop_01_glass"})},

            { "BaseLargeRoomFiltrationMachine", filtrationMachineRendererData},
            { "BaseLargeRoomFiltrationMachineShort", filtrationMachineRendererData},

            { "BaseRoomFiltrationMachine", new RendererData("model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass")},

            { "BaseRoomHatch", hatchRendererData},

            { "BaseCorridorHatch", new RendererData("underWater/model/BaseCorridorExteriorCapHatch/hatch_end_anim/hatch_geo")},

            { "BaseCorridorIShapeHatchSide", new RendererData("model/hatch_side_anim/sideHatch_geo1")},

            { "BaseCorridorIShapeHatchTop", topHatchRendererData},
            { "BaseCorridorXShapeHatchTop", topHatchRendererData},

            { "BaseCorridorIShapeHatchBottom", bottomHatchRendererData},
            { "BaseCorridorXShapeHatchBottom", bottomHatchRendererData},

            { "BaseLargeWaterParkCeilingGlass", new RendererData("model/BaseWaterParkCeilingGlassGlass/BaseWaterParkCeilingGlass_geo")},

            { "BaseLargeWaterParkCeilingGlassDome", new RendererData("model/BaseLargeWaterParkCeilingGlassDome", new List<string> { "BaseWaterParkCeilingGlassDome_glass_ext", "BaseWaterParkCeilingGlassDome_glass_int" })},

            { "BaseRoomWindowSide", new RendererData("BaseRoomGenericInteriorWindowSide01", new List<string> { "BaseExteriorRoomGenericWindowSide01Glass", "BaseInteriorRoomGenericWindowSide01Glass" })},

            { "BaseLargeRoomWindowSide", new RendererData("model/BaseLargeRoomWindowSide", new List<string> { "LargeRoom_lExteriorWindowSide01Glass_01", "LargeRoom_InteriorWindowSide01Glass_01" })},

            { "BaseLargeRoomWindowSideShort", new RendererData("model/BaseLargeRoomWindowSideShort", new List<string> { "LargeRoom_InteriorWindowSide01Glass_002", "LargeRoom_lExteriorWindowSide01Glass_002" })},

            { "BaseMoonpoolWindowSide", new RendererData("model", new List<string> { "BaseRoomMoonPoolExteriorWindowSide01Glass_01", "BaseRoomMoonPoolInteriorWindowSide01Glass_01" })},

            { "BaseMoonpoolWindowSideShort", new RendererData("model", new List<string> { "BaseRoomMoonPoolInteriorWindowSide01Glass_02", "BaseRoomMoonPoolExteriorWindowSide01Glass_02" })},

            { "BaseMapRoomWindowSide", new RendererData("BaseMapRoomInteriorWindowSide/BaseMapRoomInteriorWindowSideGlass_int")},

            { "BaseCorridorWindow", new RendererData("models", new List<string> { "BaseCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "BaseCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1" })},

            { "BaseCorridorIShapeWindowSide", new RendererData("BaseHatchModel", new List<string> { "BaseCorridorInteriorWindowSide/BaseCorridorInteriorWindowSide_ext", "BaseCorridorInteriorWindowSide_LOD1/BaseCorridorInteriorWindowSide_ext_LOD1" })},

            { "BaseCorridorIShapeWindowTop", new RendererData("models", new List<string> { "BaseCorridorInteriorWindowTop/BaseCorridorInteriorWindowTop_ext", "BaseCorridorInteriorWindowTop_LOD1/BaseCorridorInteriorWindowTop_ext_LOD1" })},

            { "BaseCorridorXShapeWindowTop", new RendererData("models", new List<string> { "BaseCorridorXShapeExteriorWindowTop/BaseCorridorXShapeExteriorWindowTop_ext", "BaseCorridorXShapeExteriorWindowTop_LOD1/BaseCorridorXShapeExteriorWindowTop_ext_LOD1" })},

            { "BaseLargeRoomWaterParkHatchShort", acLargeHatchRendererData},
            { "BaseLargeRoomWaterParkHatch", acLargeHatchRendererData},
            { "BaseWaterParkHatch", acHatchRendererData},

            { "BaseLargeRoomHatch", hatchRendererData},
            { "BaseLargeRoomHatchShort", hatchRendererData},
            { "BaseMoonpoolHatchShort", hatchRendererData},
            { "BaseMoonpoolHatch", hatchRendererData},
            { "BaseMapRoomHatch", hatchRendererData},
            { "BaseObservatoryHatch", hatchRendererData},

             };

            //AddDebug("pieces " + Base.pieces.Length);
            for (int i = 0; i < Base.pieces.Length; i++)
            {
                Base.PieceDef piece = Base.pieces[i];
                if (piece.prefab == null)
                    continue;

                string name = piece.prefab.name;

                if (baseGlassRenderers.ContainsKey(name))
                    piece.prefab.DisableShadowCasting(baseGlassRenderers[name]);
            }
            FixObservatoryCover();
            FixMapRoomConnector();
            basePrefabsFixed = true;
        }

        void FixMapRoomConnector()
        {
            Base.PieceDef piece = Base.pieces[75];
            Transform roomCorridorConnector = piece.prefab;
            piece = Base.pieces[133];
            Transform mapRoomCorridorConnector = piece.prefab;
            Transform cap_zp_narrower = roomCorridorConnector.Find("cap_zp_narrower 2");
            GameObject newGO = GameObject.Instantiate(cap_zp_narrower.gameObject);
            newGO.transform.SetParent(mapRoomCorridorConnector);
            newGO.transform.localPosition = cap_zp_narrower.localPosition;
            newGO.transform.localEulerAngles = cap_zp_narrower.localEulerAngles;
        }

        void FixObservatoryCover()
        {// fix: cant build hatch into Observatory
            Transform t = Base.pieces[112].prefab;// BaseObservatoryCoverSide
            BoxCollider collider = t.GetComponentInChildren<BoxCollider>();
            collider.size = new Vector3(collider.size.x, collider.size.y, .5f);
            collider.center = new Vector3(collider.center.x, collider.center.y, 2.7f);
        }

    }
}
