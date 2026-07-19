using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class AbandonedBaseFixer
    {
        public static bool abandonedBasesFixed;
        HashSet<string> badLODs = new HashSet<string> { "BaseAbandonedRoomCorridorConnector", "BaseAbandonedRoom", "BaseAbandonedFoundationPiece", "BaseAbandonedCorridorIShape", "BaseAbandonedCorridorTShape", "BaseAbandonedRoomInteriorTop", "BaseAbandonedRoomHatch", "BaseRoomCoverTop", "BaseAbandonedRoomExteriorTop", "BaseAbandonedCorridorXShape", "BaseAbandonedCorridorCoverXShapeTopExtClosed", "BaseAbandonedCorridorIShapeGlass", "BaseRoomCoverBottom", "BaseAbandonedRoomReinforcementSide" };

        Dictionary<string, List<RendererData>> glassRenderers = new Dictionary<string, List<RendererData>>{
            {"BaseRoomWaterParkBottom", new List<RendererData>{ new RendererData("model/Large_Aquarium_generic_room_glass_01" )
            }},
            {"BaseAbandonedRoomFiltrationMachine",new List<RendererData>{  new RendererData("model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass" )
            }},
            { "BaseAbandonedRoomWindowSideBroken", new List<RendererData>{ new RendererData("BaseRoomGenericInteriorWindowSide01Broken02", new List<string> { "BaseInteriorRoomGenericWindowSide01GlassBroken", "BaseExteriorRoomGenericWindowSide01GlassBroken" })
            }},
            {"BaseAbandonedRoomWindowSide", new List<RendererData>{
                new RendererData( "BaseRoomGenericInteriorWindowSide01Broken01", new List<string>{ "BaseInteriorRoomGenericWindowSide01Glass", "BaseExteriorRoomGenericWindowSide01Glass" }),
                new RendererData("LODs", new List<string>{"BaseRoomGenericInteriorWindowSide01_LOD3/BaseInteriorRoomGenericWindowSide01Glass_LOD3",  "BaseRoomGenericInteriorWindowSide01_LOD2/BaseInteriorRoomGenericWindowSide01Glass_LOD2", "BaseRoomGenericInteriorWindowSide01_LOD1/BaseInteriorRoomGenericWindowSide01Glass_LOD1" })
            } },
            {"BaseAbandonedRoomHatch", new List<RendererData>{
                new RendererData( "BaseCorridorHatch/models/BaseCorridorExteriorCapHatch/BaseCorridorExteriorCapHatchMovable"),
                new RendererData( "LODs", new List<string>{ "BaseCorridorExteriorCapHatch_LOD1/BaseCorridorExteriorCapHatchMovable_LOD1", "BaseCorridorExteriorCapHatch_LOD2/BaseCorridorExteriorCapHatchMovable_LOD2", "BaseCorridorExteriorCapHatch_LOD3/BaseCorridorExteriorCapHatchMovable_LOD3" }),
            } },
            {"BaseAbandonedCorridorIShapeGlass", new List<RendererData>{
                new RendererData("models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass"),
                new RendererData("LODs", new List<string>{"BaseCorridorhIShapeGlass01Exterior_LOD1/BaseCorridorhIShapeGlass01ExteriorGlass_LOD1", "BaseCorridorhIShapeGlass01Exterior_LOD2/BaseCorridorhIShapeGlass01ExteriorGlass_LOD2", "BaseCorridorhIShapeGlass01Exterior_LOD3/BaseCorridorhIShapeGlass01ExteriorGlass_LOD3" }),
            } },
            {"BaseAbandonedObservatory", new List<RendererData>{
                new RendererData("BaseAbandonedRoomObservatory/BaseRoomObservatory_glass"),
                new RendererData("LODs", new List<string>{ "BaseAbandonedRoomObservatory_LOD3/BaseRoomObservatory_glass_LOD3", "BaseAbandonedRoomObservatory_LOD2/BaseRoomObservatory_glass_LOD2", "BaseAbandonedRoomObservatory_LOD1/BaseRoomObservatory_glass_LOD1" }),
            } },
            {"BaseAbandonedCorridorWindow", new List<RendererData>{
                new RendererData("models", new List<string>{ "BaseAbandonedCorridorExteriorCap_01/BaseCorridorExteriorCap_01_int", "BaseAbandonedCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "BaseAbandonedCorridorExteriorCap_01/BaseCorridorExteriorCapWindow_01_int" }),
                new RendererData("LODs", new List<string>{ "BaseAbandonedCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_int_LOD1", "BaseAbandonedCorridorExteriorCap_01_LOD2/BaseCorridorExteriorCap_01_int_LOD2", "BaseAbandonedCorridorExteriorCap_01_LOD3/BaseCorridorExteriorCap_01_int_LOD3", "BaseAbandonedCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1", "BaseAbandonedCorridorExteriorCap_01_LOD2/BaseCorridorExteriorCap_01_ext_LOD2", "BaseAbandonedCorridorExteriorCap_01_LOD3/BaseCorridorExteriorCap_01_ext_LOD3", "BaseAbandonedCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCapGlass_01_int_LOD1", "BaseAbandonedCorridorExteriorCap_01_LOD2/BaseCorridorExteriorCapGlass_01_int_LOD2", "BaseAbandonedCorridorExteriorCap_01_LOD3/BaseCorridorExteriorCapWindow_01_int_LOD3" })
            } },
            {"BaseAbandonedCorridorIShapeWindowSide", new List<RendererData>{
                new RendererData("BaseAbandonedCorridorInteriorWindowSide", new List<string>{ "BaseAbandonedCorridorInteriorWindowSide_ext", "BaseAbandonedCorridorInteriorWindowSideGlass_int" }),
                new RendererData("LODs", new List<string>{ "BaseAbandonedCorridorInteriorWindowSide_LOD1/BaseAbandonedCorridorInteriorWindowSide_ext_LOD1", "BaseAbandonedCorridorInteriorWindowSide_LOD2/BaseAbandonedCorridorInteriorWindowSide_ext_LOD2", "BaseAbandonedCorridorInteriorWindowSide_LOD3/BaseAbandonedCorridorInteriorWindowSide_ext_LOD3", "BaseAbandonedCorridorInteriorWindowSide_LOD1/BaseAbandonedCorridorInteriorWindowSideGlass_int_LOD1", "BaseAbandonedCorridorInteriorWindowSide_LOD2/BaseAbandonedCorridorInteriorWindowSideGlass_int_LOD2", "BaseAbandonedCorridorInteriorWindowSide_LOD3/BaseAbandonedCorridorInteriorWindowSideGlass_int_LOD3"}),
            } },
            {"BaseAbandonedCorridorTShape", new List<RendererData>{
                new RendererData("BaseAbandonedCorridorTShapeWindowTop/models/BaseCorridorTShapeExteriorWindowTop/BaseCorridorTShapeExteriorWindowTop_glass"),
                new RendererData("BaseAbandonedCorridorTShapeWindowTop/LODs", new List<string>{ "BaseCorridorTShapeExteriorWindowTop_LOD1/BaseCorridorTShapeExteriorWindowTop_glass_LOD1", "BaseCorridorTShapeExteriorWindowTop_LOD2/BaseCorridorTShapeExteriorWindowTop_glass_LOD2", "BaseCorridorTShapeExteriorWindowTop_LOD3/BaseCorridorTShapeExteriorWindowTop_glass_LOD3" }),
            } },
                {"BaseAbandonedCorridorHatch", new List<RendererData>{
                new RendererData("models/BaseCorridorExteriorCapHatch/BaseCorridorExteriorCapHatchMovable"),
                new RendererData("LODs", new List<string>{ "BaseCorridorExteriorCapHatch_LOD1/BaseCorridorExteriorCapHatchMovable_LOD1", "BaseCorridorExteriorCapHatch_LOD2/BaseCorridorExteriorCapHatchMovable_LOD2", "BaseCorridorExteriorCapHatch_LOD3/BaseCorridorExteriorCapHatchMovable_LOD3" }),
            } },
        };

        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            {
                if (__instance.transform.parent && __instance.transform.parent.parent && __instance.transform.parent.name == "Decoration" && __instance.transform.parent.parent.name == "BaseCell" && __instance.name.StartsWith("Starship_cargo"))
                { // decals on ctares not their children, they stay when crates move
                    //AddDebug("base crate " + __instance.name);
                    __instance.gameObject.MakeUnmovable();
                }
            }
        }

        public void FixAbandonedBases()
        {
            UWE.CoroutineHost.StartCoroutine(FixDeepGrandReefBase());
            UWE.CoroutineHost.StartCoroutine(FixJellyShroomBase1());
            UWE.CoroutineHost.StartCoroutine(FixJellyShroomBase3());
            UWE.CoroutineHost.StartCoroutine(FixJellyShroomBase4());
            UWE.CoroutineHost.StartCoroutine(FixJellyShroomBase6());
            UWE.CoroutineHost.StartCoroutine(FixFloatingIslandBase1());
            UWE.CoroutineHost.StartCoroutine(FixFloatingIslandBase2());
            UWE.CoroutineHost.StartCoroutine(FixFloatingIslandBase3());
            abandonedBasesFixed = true;
        }

        private IEnumerator FixJellyShroomBase1()
        {// AbandonedBaseJellyShroom1  111 -264 -372
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("2921736c-c898-4213-9615-ea1a72e28178");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixJellyShroomBase1 No prefab");
                yield break;
            }
            FixJellyShroomBaseCollision(prefab);
            Transform culling = prefab.transform.GetChild(1);
            FixAbandonedBaseGlass(culling.gameObject);
            FixBlackTextureDecals(culling, true);

            if (ConfigToEdit.disableHotMetalGlow.Value)
            {
                Transform damage = culling.Find("Damage");
                damage.gameObject.DisableGlowShader();
            }
            if (Util.IsGraphicsPresetHighDetail())
            {
                DistanceCull distanceCull = culling.GetComponent<DistanceCull>();
                distanceCull.distanceSqr = 50000;
                FixBaseLODs(culling.gameObject);
            }
        }

        private IEnumerator FixJellyShroomBase6()
        {// -393 -230 -110
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("c1139534-b3b9-4750-b60b-a77ca054b3dd");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixJellyShroomBase6 No prefab");
                yield break;
            }
            FixAbandonedBaseGlass(prefab);
            FixBlackTextureDecals(prefab.transform);
            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(prefab);
        }

        private IEnumerator FixJellyShroomBase4()
        {//  -540 -250 -86
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("256a06d3-b861-487a-b8ac-050daa0d683d");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixJellyShroomBase6 No prefab");
                yield break;
            }
            FixBlackTextureDecals(prefab.transform);
            FixJellyShroomBase4Decals(prefab);
        }

        private IEnumerator FixJellyShroomBase3()
        { // -265 -240 -231
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("026c39c1-d0cc-442c-aa42-e574c9c281b2");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixJellyShroomBase3 No prefab");
                yield break;
            }
            FixBlackTextureDecals(prefab.transform);
        }

        private IEnumerator FixDeepGrandReefBase()
        {// DeepGrandReefAbandonedBase -642 -509 -943
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("42a80cbc-d9fd-49d2-94b3-b5178024b3cb");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixDeepGrandReefBase No prefab");
                yield break;
            }
            FixBlackTextureDecals(prefab.transform);
            FixDeepGrandReefBaseDecals(prefab);
            FixDeepGrandReefBasePillars(prefab);
            FixDeepGrandReefBaseColliders(prefab);
            FixAbandonedBaseGlass(prefab);

            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(prefab);
        }

        private IEnumerator FixFloatingIslandBase1()
        {// AbandonedBaseFloatingIsland1 -754 16 -1118
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("99b164ac-dfb4-4a14-b305-8666fa227717");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixFloatingIslandBase1 No prefab");
                yield break;
            }
            FixAbandonedBaseGlass(prefab);
            //FixFloatingIslandBase1Decals(prefab);
            FixBlackTextureDecals(prefab.transform);
            FixFloatingIslandBase1Collision(prefab);
            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(prefab);
        }

        private void FixFloatingIslandBase1Decals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(4);
            Transform decals = baseCell.GetChild(0);
            Transform t = decals.GetChild(63); // texture z fighting
            t.position = new Vector3(-764.62f, t.position.y, t.position.z);
            t = decals.GetChild(52); // decal behind surface texture
            t.position = new Vector3(-763.92f, t.position.y, t.position.z);

            t = decals.GetChild(65); // decal behind surface texture
            t.position = new Vector3(t.position.x, 19.945f, t.position.z);
            t = decals.GetChild(66); // decal behind surface texture
            t.position = new Vector3(t.position.x, 19.945f, t.position.z);
            t = decals.GetChild(67); // decal behind surface texture
            t.position = new Vector3(t.position.x, 19.944f, t.position.z);

            baseCell = base_.transform.GetChild(5);
            decals = baseCell.GetChild(0);
            t = decals.GetChild(22); // texture z fighting
            t.position = new Vector3(-757.39f, t.position.y, t.position.z);
            t = decals.GetChild(34);
        }

        private IEnumerator FixFloatingIslandBase3()
        {// AbandonedBaseFloatingIsland3  -705 76 -1163
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("0e394d55-da8c-4b3e-b038-979477ce77c1");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixFloatingIslandBase3 No prefab");
                yield break;
            }
            FixBlackTextureDecals(prefab.transform);
            FixAbandonedBaseGlass(prefab);
            FixFloatingIslandBase3Collision(prefab);
            //FixFloatingIslandBase3Decals(base_);
            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(prefab);
        }

        private IEnumerator FixFloatingIslandBase2()
        {// AbandonedBaseFloatingIsland2  -800 78 -1055
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("569f22e0-274d-49b0-ae5e-21ef0ce907ca");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixFloatingIslandBase2 No prefab");
                yield break;
            }
            FixAbandonedBaseGlass(prefab);
            FixFloatingIslandBase2Collision(prefab);
            FixBlackTextureDecals(prefab.transform);
            //FixFloatingIslandBase2Decals(prefab);
            MoveFloatingIslandBase2LootCrate(prefab);
            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(prefab);
        }

        private void MoveFloatingIslandBase2LootCrate(GameObject base_)
        {
            Transform slots = base_.transform.GetChild(3);
            Transform cratePlaceholder = slots.GetChild(10);
            cratePlaceholder.position = new Vector3(-804.8f, 76.3f, -1057.4f);
        }

        private void FixFloatingIslandBase2Decals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(2);
            Transform decals = baseCell.GetChild(0);
            Transform t = decals.GetChild(61); // decal behind surface texture
            t.position = new Vector3(t.position.x, 79.235f, t.position.z);
            t = decals.GetChild(62); // decal behind surface texture
            t.position = new Vector3(t.position.x, 79.235f, t.position.z);
            t = decals.GetChild(63); // decal behind surface texture
            t.position = new Vector3(t.position.x, 79.234f, t.position.z);

            t = decals.GetChild(22); // decal behind surface texture
            t.position = new Vector3(t.position.x, 79.239f, t.position.z);
            t = decals.GetChild(23); // decal behind surface texture
            t.position = new Vector3(t.position.x, 79.239f, t.position.z);

            t = decals.GetChild(7); // texture z fighting
            t.position = new Vector3(-805.792f, 79.239f, t.position.z);
            t = decals.GetChild(44);
            t.position = new Vector3(-803.8f, t.position.y, -1052f);

            baseCell = base_.transform.GetChild(1);
            decals = baseCell.GetChild(0);
            t = decals.GetChild(decals.childCount - 1);
            t.position = new Vector3(-803.5f, t.position.y, -1051.05f);
        }

        private void FixFloatingIslandBase3Decals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(1);
            Transform decals = baseCell.GetChild(0);
            Transform t = decals.GetChild(8); // texture z fighting
            t.position = new Vector3(-714.21f, t.position.y, t.position.z);
        }

        public void FixBlackTextureDecals(Transform base_, bool decalsInRoot = false)
        {
            //Main.logger.LogDebug("FixBlackTextureDecals " + base_.name);
            List<Transform> baseCells;
            if (decalsInRoot)
                baseCells = new List<Transform> { base_ };
            else
                baseCells = base_.FindAllChildren("BaseCell");

            foreach (Transform baseCell in baseCells)
            {
                Transform decals = baseCell.Find("Decals");
                if (decals == null)
                    continue;

                foreach (Renderer renderer in decals.GetComponentsInChildren<Renderer>())
                {
                    Texture decalTexture = renderer.material.mainTexture;
                    renderer.material = AuroraFixer.materialForDecals;
                    renderer.material.mainTexture = decalTexture;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        private void FixJellyShroomBase4Decals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(0);
            Transform decals = baseCell.transform.GetChild(0);
            Transform decal = decals.transform.GetChild(5);
            decal.localPosition = new Vector3(-2.8f, 1.864f, -0.7f);
            decal = decals.transform.GetChild(decals.childCount - 1);
            decal.localPosition = new Vector3(-3.5f, 2f, 4f);
            decal = decals.transform.GetChild(1);
            decal.localPosition = new Vector3(2.45f, decal.localPosition.y, decal.localPosition.z);
        }

        private void FixDeepGrandReefBaseColliders(GameObject base_)
        {
            for (int i = 1; i < 4; i += 2)
            {
                Transform baseCell = base_.transform.GetChild(i);
                List<Transform> connectors = baseCell.FindAllChildren("BaseAbandonedRoomCorridorConnector");
                foreach (Transform connector in connectors)
                {
                    Transform col = connector.Find("collision/floor");
                    BoxCollider[] colliders = col.GetComponents<BoxCollider>();
                    BoxCollider collider = colliders[5];
                    collider.center = new Vector3(collider.center.x, collider.center.y, 1);
                    collider = colliders[6];
                    collider.center = new Vector3(collider.center.x, 1, collider.center.z);
                    collider = colliders[7];
                    collider.center = new Vector3(collider.center.x, collider.center.y, -1);
                }
            }
        }

        private void FixBaseLODs(GameObject base_)
        {
            //Main.logger.LogDebug("FixBaseLODs " + base_.name);
            List<Transform> baseCells = base_.transform.FindAllChildren("BaseCell");
            foreach (Transform baseCell in baseCells)
            {
                LODGroup[] lODGroups = baseCell.GetComponentsInChildren<LODGroup>();
                foreach (LODGroup lODGroup in lODGroups)
                {
                    if (badLODs.Contains(lODGroup.name))
                        baseCell.gameObject.IncreaseLODdistane();
                }
            }
        }

        private void FixFloatingIslandBase2Collision(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(1);
            Transform col = baseCell.Find("BaseAbandonedCorridorIShape/collisions/Cube");
            BoxCollider[] bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
            baseCell = base_.transform.GetChild(2);
            col = baseCell.Find("BaseAbandonedCorridorTShape/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
        }

        private void FixFloatingIslandBase3Collision(GameObject base_)
        {
            Transform baseCEll = base_.transform.GetChild(1);
            Transform col = baseCEll.Find("BaseAbandonedCorridorIShape/collisions/Cube");
            BoxCollider[] bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
            baseCEll = base_.transform.GetChild(2);
            Transform corridor = baseCEll.Find("BaseAbandonedCorridorXShape");
            col = corridor.Find("collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
            BoxCollider bc = bcs[bcs.Length - 5];
            bc.center = new Vector3(bc.center.x, bc.center.y, 0);
            bc.size = new Vector3(bc.size.x, bc.size.y, 5.1f);
            bc = bcs[bcs.Length - 4];
            UnityEngine.Object.Destroy(bc);
            col = corridor.Find("BaseAbandonedCorridorCoverXShapeBottomExtClosed/collisions/Cube");
            col.gameObject.SetActive(false);
            col = baseCEll.Find("BaseAbandonedCorridorXShape/BaseAbandonedCorridorXShapeWindowTop/collisions");
            col.localPosition = new Vector3(0, 0.4f, 0);
        }

        private void FixCorridorColliders(BoxCollider[] bcs)
        {
            foreach (BoxCollider c in bcs)
            {
                if (c.center.y < -1)
                    c.center = new Vector3(c.center.x, -1.31f, c.center.z);
                else if (c.center.y > 1)
                    c.center = new Vector3(c.center.x, 1.5f, c.center.z);
            }
        }

        private void FixFloatingIslandBase1Collision(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(3);
            Transform col = baseCell.Find("BaseAbandonedCorridorIShape/BaseAbandonedCorridorCoverIShapeTopExtOpened/collisions/Cube");
            BoxCollider[] bcs = col.GetComponents<BoxCollider>();
            foreach (BoxCollider c in bcs)
            { // colliders next to ladder you get stuck in
                c.center = new Vector3(c.center.x, -1f, c.center.z);
            }
            col = baseCell.Find("BaseAbandonedCorridorIShape/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
            baseCell = base_.transform.GetChild(2);
            //col = baseCEll.Find("BaseAbandonedCorridorXShape/BaseCorridorCoverXShapeBottomIntClosed/collisions");
            //col.gameObject.SetActive(false);

            col = baseCell.Find("BaseAbandonedCorridorBulkhead/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            BoxCollider bc = bcs[bcs.Length - 1];
            UnityEngine.Object.Destroy(bc); // make it easy to exit thru door

            col = baseCell.Find("BaseAbandonedCorridorXShape/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            foreach (BoxCollider c in bcs)
            {
                if (c.center.y < -1)
                    c.center = new Vector3(c.center.x, -1.31f, c.center.z);
                else if (c.center.y > 1)
                {
                    c.center = new Vector3(c.center.x, 1.4f, c.center.z);
                    c.size = new Vector3(c.size.x, .3f, c.size.z);
                }
            }
            bc = bcs[bcs.Length - 4];
            bc.center = new Vector3(0, bc.center.y, bc.center.z);
            bc.size = new Vector3(5.1f, bc.size.y, bc.size.z);
            bc = bcs[bcs.Length - 3];
            UnityEngine.Object.Destroy(bc);
        }

        private void FixJellyShroomBaseCollision(GameObject base_)
        {
            Transform culling = base_.transform.GetChild(1);
            Transform baseCell = culling.transform.GetChild(8);
            Transform collision = baseCell.Find("BaseAbandonedCorridorIShapeReinforcementSide/collisions/Cube");
            collision.localPosition = new Vector3(0.15f, 0, 0); // allow to swim between ladder and wall
            int[] baseCellIndexes = new int[] { 1, 5, 16 };

            foreach (int i in baseCellIndexes)
            {
                baseCell = culling.transform.GetChild(i);
                collision = baseCell.Find("BaseAbandonedRoomCorridorConnector/collision");
                Transform railings = collision.GetChild(1);
                railings.localPosition = new Vector3(0.15f, 0, 0);
                Transform floor = collision.GetChild(2);
                BoxCollider[] colliders_ = floor.GetComponents<BoxCollider>();
                BoxCollider collider = colliders_[2];
                collider.center = new Vector3(collider.center.x, 1.2f, collider.center.z);
                for (int j = 5; j < 8; j++)
                    UnityEngine.Object.Destroy(colliders_[j]);
            }
            baseCellIndexes = new int[] { 3, 8 };
            foreach (int i in baseCellIndexes)
            {
                baseCell = culling.transform.GetChild(i);
                collision = baseCell.Find("BaseCorridorLadderTop/logic");
                BoxCollider collider = collision.GetComponent<BoxCollider>();
                collider.center = new Vector3(collider.center.x, 0.36f, collider.center.z);
                collider.size = new Vector3(collider.size.x, 0.1f, collider.size.z);
            }
            Transform damage = culling.Find("Damage");
            collision = damage.Find("Collision");
            Transform t = collision.GetChild(1);
            BoxCollider[] colliders = t.GetComponents<BoxCollider>();

            for (int i = 0; i < colliders.Length; i++)
            {
                BoxCollider c = colliders[i];
                c.size = new Vector3(c.size.x, 0.2f, c.size.z);
                if (i == 0)
                    c.center = new Vector3(c.center.x, 31.25f, c.center.z);
                else
                    c.center = new Vector3(c.center.x, 28.8f, c.center.z);
            }
            t = collision.GetChild(5);
            colliders = t.GetComponents<BoxCollider>();
            BoxCollider bc = colliders[1];
            bc.size = new Vector3(bc.size.x * .5f, bc.size.y * .5f, bc.size.z);

            t = collision.GetChild(6);
            bc = t.GetComponent<BoxCollider>();
            bc.size = new Vector3(bc.size.x * .5f, bc.size.y * .5f, bc.size.z);

            t = collision.GetChild(4);
            bc = t.GetComponent<BoxCollider>();
            bc.size = new Vector3(1, 0.2f, bc.size.z);
        }

        private void FixAbandonedBaseGlass(GameObject base_)
        {
            List<Transform> baseCells = base_.transform.FindAllChildren("BaseCell");

            foreach (Transform baseCell in baseCells)
            {
                foreach (var windowName in glassRenderers.Keys)
                {
                    List<Transform> windows = baseCell.FindAllChildren(windowName);

                    foreach (Transform window in windows)
                    {
                        foreach (RendererData data in glassRenderers[windowName])
                            window.DisableShadowCasting(data);
                    }
                }
            }
        }

        private void FixDeepGrandReefBasePillars(GameObject base_)
        {
            Transform t = base_.transform.GetChild(12);
            t = t.GetChild(1);
            Transform models = t.GetChild(3);
            t = models.GetChild(5); // pillar not touching ground
            t.localScale = new Vector3(1, 1, 5.2f);
            t = models.GetChild(6); // pillar not touching ground
            t.localScale = new Vector3(1, 1, 5.6f);
            t = models.GetChild(7); // pillar not touching ground
            t.localScale = new Vector3(1, 1, 5.7f);

            t = base_.transform.GetChild(5);
            t = t.GetChild(3);
            models = t.GetChild(3);
            t = models.GetChild(9); // pillar not touching ground
            t.localScale = new Vector3(1, 1, 4.7f);

            t = base_.transform.GetChild(4);
            t = t.GetChild(1);
            models = t.GetChild(3);
            t = models.GetChild(6); // pillar not touching ground
            t.localScale = new Vector3(1, 1, 1.5f);
        }

        private void FixDeepGrandReefBaseDecals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(3);
            Transform coral = baseCell.GetChild(0);
            Transform t = coral.GetChild(21); // starfish on AC wall. Only 1 side is visible. Place it in AC
            t.eulerAngles = new Vector3(0, 120f, 0); // move to -641.3 -503.25 -945.5
            t.localPosition = new Vector3(.64f, -1.21f, -1.78f);

            t = coral.GetChild(22); // starfish on AC wall
            t.eulerAngles = new Vector3(0, 13, 0); // move to -642 -506.95 -943
            t.localPosition = new Vector3(1.216f, -4.95f, 0.13f);
            t = coral.GetChild(23); // starfish on AC wall
            t.eulerAngles = default; // move to -642.6 -502.4 -944
            t.localPosition = new Vector3(0.1f, -0.35f, 0.7f);
            t = coral.GetChild(6); // starfish on roof
            t.position = new Vector3(t.position.x, 5.784f, t.position.z);

            baseCell = base_.transform.GetChild(5);
            Transform decals = baseCell.GetChild(0);

            foreach (int i in new[] { 10, 23, 24 }) // stray decals
                decals.GetChild(i).gameObject.SetActive(false);

            coral = baseCell.GetChild(2);
            t = coral.GetChild(9);// stray starfish near big crate -633 -510 -938
            t.SetPositionAndRotation(new Vector3(-139.9f, -7.46f, 140.7f), Quaternion.Euler(0, 309f, 0));
            t = coral.GetChild(0);// stray starfish in big crate -634 -509 -938
            t.SetPositionAndRotation(new Vector3(-155.8f, -7.5f, 132.8f), Quaternion.Euler(0, 182f, 0)); // move to -649 -511 -946
        }

    }
}
