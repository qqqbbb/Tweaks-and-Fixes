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
        static Material materialForDecals;

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
                switch (__instance.name)
                {
                    case "DeepGrandReefAbandonedBase(Clone)":
                        FixDeepGrandReefBase(__instance.gameObject);
                        break;
                    case "AbandonedBaseJellyShroom1(Clone)":
                        FixJellyShroomBase1(__instance.gameObject);
                        break;
                    case "AbandonedBaseJellyShroom3(Clone)":
                        FixJellyShroomBase3(__instance.gameObject);
                        break;
                    case "AbandonedBaseJellyShroom4(Clone)":
                        FixJellyShroomBase4(__instance.gameObject);
                        break;
                    case "AbandonedBaseJellyShroom6(Clone)":
                        FixJellyShroomBase6(__instance.gameObject);
                        break;
                    case "AbandonedBaseFloatingIsland1(Clone)":
                        FixFloatingIslandBase1(__instance.gameObject);
                        break;
                    case "AbandonedBaseFloatingIsland3(Clone)":
                        FixFloatingIslandBase3(__instance.gameObject);
                        break;
                    case "AbandonedBaseFloatingIsland2(Clone)":
                        FixFloatingIslandBase2(__instance.gameObject);
                        break;
                }
            }
        }

        private static void FixJellyShroomBase1(GameObject base_)
        {// AbandonedBaseJellyShroom1  111 -264 -372
            FixJellyShroomBaseCollision(base_);
            Transform culling = base_.transform.GetChild(1);
            FixAbandonedBaseGlass(culling.gameObject);
            UWE.CoroutineHost.StartCoroutine(FixDecals(culling));

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
                //FixJellyShroomBase1Loot(base_.transform);
            }
        }

        private static void FixJellyShroomBase1Loot(Transform base_)
        {
            Transform slots = base_.GetChild(0);
            Transform t = slots.GetChild(slots.childCount - 3);
            t.localPosition = new Vector3(t.localPosition.x, 0, t.localPosition.z);
        }

        private static void FixJellyShroomBase6(GameObject base_)
        {// -393 -230 -110
            FixBaseLODs(base_);
            FixAbandonedBaseGlass(base_);
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
        }

        private static void FixJellyShroomBase4(GameObject base_)
        {//  -540 -250 -86
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
            FixJellyShroomBase4Decals(base_);
        }

        private static void FixJellyShroomBase3(GameObject base_)
        { // -265 -240 -231
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
        }

        private static void FixDeepGrandReefBase(GameObject base_)
        {// DeepGrandReefAbandonedBase -642 -509 -943
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
            FixDeepGrandReefBaseDecals(base_);
            FixDeepGrandReefBasePillars(base_);
            FixDeepGrandReefBaseColliders(base_);
            FixAbandonedBaseGlass(base_);

            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(base_);
        }

        private static void FixFloatingIslandBase1(GameObject base_)
        {// AbandonedBaseFloatingIsland1 -754 16 -1118
            FixAbandonedBaseGlass(base_);
            FixFloatingIslandBase1Decals(base_);
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
            FixFloatingIslandBase1Collision(base_);
            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(base_);
        }

        private static void FixFloatingIslandBase1Decals(GameObject base_)
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
            //t.SetPositionAndRotation(new Vector3(t.position.x, t.position.y, -6.4f), Quaternion.Euler(0, 40.13f, 350));
            //t.eulerAngles = new Vector3(t.eulerAngles.x, 40.13f, 335f);

        }

        private static void FixFloatingIslandBase3(GameObject base_)
        {// AbandonedBaseFloatingIsland3  -705 76 -1163
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
            FixAbandonedBaseGlass(base_);
            FixFloatingIslandBase3Collision(base_);
            FixFloatingIslandBase3Decals(base_);
            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(base_);
        }

        private static void FixFloatingIslandBase2(GameObject base_)
        {// AbandonedBaseFloatingIsland2  -800 78 -1055
            FixAbandonedBaseGlass(base_);
            FixFloatingIslandBase2Collision(base_);
            UWE.CoroutineHost.StartCoroutine(FixDecals(base_.transform));
            FixFloatingIslandBase2Decals(base_);

            if (Util.IsGraphicsPresetHighDetail())
                FixBaseLODs(base_);
        }

        private static void FixFloatingIslandBase2Decals(GameObject base_)
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

        private static void FixFloatingIslandBase3Decals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(1);
            Transform decals = baseCell.GetChild(0);
            Transform t = decals.GetChild(8); // texture z fighting
            t.position = new Vector3(-714.21f, t.position.y, t.position.z);
        }

        public static IEnumerator FixDecals(Transform base_)
        {
            if (materialForDecals == null)
            {
                GameObject starfishPrefab = null;
                IPrefabRequest prefabTask = PrefabDatabase.GetPrefabAsync("4605151e-dea4-4ba7-96bf-2f88b3b41bdb"); // starfish_02
                yield return prefabTask;
                prefabTask.TryGetPrefab(out starfishPrefab);
                materialForDecals = starfishPrefab.GetComponentInChildren<Renderer>().material;
            }
            List<Transform> baseCells;
            Transform decals = base_.Find("Decals");
            if (decals == null)
                baseCells = base_.transform.FindAllChildren("BaseCell");
            else // AbandonedBaseJellyShroom1
                baseCells = new List<Transform> { base_ };

            foreach (Transform baseCell in baseCells)
            {
                decals = baseCell.Find("Decals");
                if (decals == null)
                    continue;

                foreach (Renderer renderer in decals.GetComponentsInChildren<Renderer>())
                {
                    Texture decalTexture = renderer.material.mainTexture;
                    renderer.material = materialForDecals;
                    renderer.material.mainTexture = decalTexture;
                    renderer.material.DisableKeyword("MARMO_EMISSION");
                    renderer.material.DisableKeyword("MARMO_SPECMAP");
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }
        }

        private static void FixJellyShroomBase4Decals(GameObject base_)
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

        private static void FixDeepGrandReefBaseColliders(GameObject base_)
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

        private static void FixBaseLODs(GameObject base_)
        {
            //AddDebug("FixBaseLODs " + base_.name);
            HashSet<string> badLODs = new HashSet<string> { "BaseAbandonedRoomCorridorConnector", "BaseAbandonedRoom", "BaseAbandonedFoundationPiece", "BaseAbandonedCorridorIShape", "BaseAbandonedCorridorTShape", "BaseAbandonedRoomInteriorTop", "BaseAbandonedRoomHatch", "BaseRoomCoverTop", "BaseAbandonedRoomExteriorTop", "BaseAbandonedCorridorXShape", "BaseAbandonedCorridorCoverXShapeTopExtClosed", "BaseAbandonedCorridorIShapeGlass", "BaseRoomCoverBottom", "BaseAbandonedRoomReinforcementSide" };
            List<Transform> baseCells = base_.transform.FindAllChildren("BaseCell");
            foreach (Transform baseCell in baseCells)
            {
                LODGroup[] lODGroups = baseCell.GetComponentsInChildren<LODGroup>();
                foreach (LODGroup lODGroup in lODGroups)
                {
                    if (badLODs.Contains(lODGroup.name))
                    {
                        //Main.logger.LogDebug("DisableDeepGrandReefBaseLODs " + lODGroup.name);
                        lODGroup.ForceLOD(0);
                    }
                }
            }
        }

        private static void FixFloatingIslandBase2Collision(GameObject base_)
        {
            Transform baseCEll = base_.transform.GetChild(1);
            Transform col = baseCEll.Find("BaseAbandonedCorridorIShape/collisions/Cube");
            BoxCollider[] bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
            baseCEll = base_.transform.GetChild(2);
            col = baseCEll.Find("BaseAbandonedCorridorTShape/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
        }

        private static void FixFloatingIslandBase3Collision(GameObject base_)
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

        private static void FixCorridorColliders(BoxCollider[] bcs)
        {
            foreach (BoxCollider c in bcs)
            {
                if (c.center.y < -1)
                    c.center = new Vector3(c.center.x, -1.31f, c.center.z);
                else if (c.center.y > 1)
                    c.center = new Vector3(c.center.x, 1.5f, c.center.z);
            }
        }

        private static void FixFloatingIslandBase1Collision(GameObject base_)
        {
            Transform baseCEll = base_.transform.GetChild(3);
            Transform col = baseCEll.Find("BaseAbandonedCorridorIShape/BaseAbandonedCorridorCoverIShapeTopExtOpened/collisions/Cube");
            BoxCollider[] bcs = col.GetComponents<BoxCollider>();
            foreach (BoxCollider c in bcs)
            { // colliders next to ladder you get stuck in
                c.center = new Vector3(c.center.x, -1f, c.center.z);
            }
            col = baseCEll.Find("BaseAbandonedCorridorIShape/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            FixCorridorColliders(bcs);
            baseCEll = base_.transform.GetChild(2);
            //col = baseCEll.Find("BaseAbandonedCorridorXShape/BaseCorridorCoverXShapeBottomIntClosed/collisions");
            //col.gameObject.SetActive(false);

            col = baseCEll.Find("BaseAbandonedCorridorBulkhead/collisions/Cube");
            bcs = col.GetComponents<BoxCollider>();
            BoxCollider bc = bcs[bcs.Length - 1];
            UnityEngine.Object.Destroy(bc); // make it easy to exit thru door

            col = baseCEll.Find("BaseAbandonedCorridorXShape/collisions/Cube");
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
            //baseCEll = base_.transform.GetChild(5);
            //col = baseCEll.Find("BaseAbandonedCorridorIShape/BaseAbandonedCorridorCap/collisions/Cube");
        }

        private static void FixJellyShroomBaseCollision(GameObject base_)
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

        private static void FixAbandonedBaseGlass(GameObject base_)
        {
            Dictionary<string, string[]> glassRenderers = new Dictionary<string, string[]>{
                {"BaseAbandonedRoomWindowSide", new[] { "BaseRoomGenericInteriorWindowSide01Broken01/BaseInteriorRoomGenericWindowSide01Glass", "BaseRoomGenericInteriorWindowSide01Broken01/BaseExteriorRoomGenericWindowSide01Glass", "LODs/BaseRoomGenericInteriorWindowSide01_LOD3/BaseInteriorRoomGenericWindowSide01Glass_LOD3", "LODs/BaseRoomGenericInteriorWindowSide01_LOD2/BaseInteriorRoomGenericWindowSide01Glass_LOD2", "LODs/BaseRoomGenericInteriorWindowSide01_LOD1/BaseInteriorRoomGenericWindowSide01Glass_LOD1" } },
                {"BaseAbandonedRoomHatch", new[] { "BaseCorridorHatch/models/BaseCorridorExteriorCapHatch/BaseCorridorExteriorCapHatchMovable", "LODs/BaseCorridorExteriorCapHatch_LOD1/BaseCorridorExteriorCapHatchMovable_LOD1", "LODs/BaseCorridorExteriorCapHatch_LOD2/BaseCorridorExteriorCapHatchMovable_LOD2", "LODs/BaseCorridorExteriorCapHatch_LOD3/BaseCorridorExteriorCapHatchMovable_LOD3" } },
                {"BaseAbandonedCorridorIShapeGlass", new[] { "models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass", "LODs/BaseCorridorhIShapeGlass01Exterior_LOD1/BaseCorridorhIShapeGlass01ExteriorGlass_LOD1", "LODs/BaseCorridorhIShapeGlass01Exterior_LOD2/BaseCorridorhIShapeGlass01ExteriorGlass_LOD2", "LODs/BaseCorridorhIShapeGlass01Exterior_LOD3/BaseCorridorhIShapeGlass01ExteriorGlass_LOD3" } },
                {"BaseRoomWaterParkBottom", new[] { "model/Large_Aquarium_generic_room_glass_01" } },
                {"BaseAbandonedRoomWindowSideBroken", new[] { "BaseRoomGenericInteriorWindowSide01Broken02/BaseInteriorRoomGenericWindowSide01GlassBroken", "BaseRoomGenericInteriorWindowSide01Broken02/BaseExteriorRoomGenericWindowSide01GlassBroken" } },
                {"BaseAbandonedRoomFiltrationMachine", new[] { "model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass" } },
                {"BaseAbandonedObservatory", new[] { "BaseAbandonedRoomObservatory/BaseRoomObservatory_glass", "LODs/BaseAbandonedRoomObservatory_LOD3/BaseRoomObservatory_glass_LOD3", "LODs/BaseAbandonedRoomObservatory_LOD2/BaseRoomObservatory_glass_LOD2", "LODs/BaseAbandonedRoomObservatory_LOD1/BaseRoomObservatory_glass_LOD1" } },
                {"BaseAbandonedCorridorWindow", new[] {"models/BaseAbandonedCorridorExteriorCap_01/BaseCorridorExteriorCap_01_int", "models/BaseAbandonedCorridorExteriorCap_01/BaseCorridorExteriorCap_01_ext", "models/BaseAbandonedCorridorExteriorCap_01/BaseCorridorExteriorCapWindow_01_int", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_int_LOD1", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD2/BaseCorridorExteriorCap_01_int_LOD2", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD3/BaseCorridorExteriorCap_01_int_LOD3", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCap_01_ext_LOD1", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD2/BaseCorridorExteriorCap_01_ext_LOD2", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD3/BaseCorridorExteriorCap_01_ext_LOD3", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD1/BaseCorridorExteriorCapGlass_01_int_LOD1", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD2/BaseCorridorExteriorCapGlass_01_int_LOD2", "LODs/BaseAbandonedCorridorExteriorCap_01_LOD3/BaseCorridorExteriorCapWindow_01_int_LOD3" } },
                {"BaseAbandonedCorridorIShapeWindowSide", new[] { "BaseAbandonedCorridorInteriorWindowSide/BaseAbandonedCorridorInteriorWindowSide_ext", "BaseAbandonedCorridorInteriorWindowSide/BaseAbandonedCorridorInteriorWindowSideGlass_int", "LODs/BaseAbandonedCorridorInteriorWindowSide_LOD1/BaseAbandonedCorridorInteriorWindowSide_ext_LOD1", "LODs/BaseAbandonedCorridorInteriorWindowSide_LOD2/BaseAbandonedCorridorInteriorWindowSide_ext_LOD2", "LODs/BaseAbandonedCorridorInteriorWindowSide_LOD3/BaseAbandonedCorridorInteriorWindowSide_ext_LOD3", "LODs/BaseAbandonedCorridorInteriorWindowSide_LOD1/BaseAbandonedCorridorInteriorWindowSideGlass_int_LOD1", "LODs/BaseAbandonedCorridorInteriorWindowSide_LOD2/BaseAbandonedCorridorInteriorWindowSideGlass_int_LOD2", "LODs/BaseAbandonedCorridorInteriorWindowSide_LOD3/BaseAbandonedCorridorInteriorWindowSideGlass_int_LOD3" } },
                {"BaseAbandonedCorridorTShape", new[] { "BaseAbandonedCorridorTShapeWindowTop/models/BaseCorridorTShapeExteriorWindowTop/BaseCorridorTShapeExteriorWindowTop_glass", "BaseAbandonedCorridorTShapeWindowTop/LODs/BaseCorridorTShapeExteriorWindowTop_LOD1/BaseCorridorTShapeExteriorWindowTop_glass_LOD1", "BaseAbandonedCorridorTShapeWindowTop/LODs/BaseCorridorTShapeExteriorWindowTop_LOD2/BaseCorridorTShapeExteriorWindowTop_glass_LOD2", "BaseAbandonedCorridorTShapeWindowTop/LODs/BaseCorridorTShapeExteriorWindowTop_LOD3/BaseCorridorTShapeExteriorWindowTop_glass_LOD3" } },
                {"BaseAbandonedCorridorHatch", new[] { "models/BaseCorridorExteriorCapHatch/BaseCorridorExteriorCapHatchMovable", "LODs/BaseCorridorExteriorCapHatch_LOD1/BaseCorridorExteriorCapHatchMovable_LOD1", "LODs/BaseCorridorExteriorCapHatch_LOD2/BaseCorridorExteriorCapHatchMovable_LOD2", "LODs/BaseCorridorExteriorCapHatch_LOD3/BaseCorridorExteriorCapHatchMovable_LOD3" } } };
            List<Transform> baseCells = base_.transform.FindAllChildren("BaseCell");
            foreach (Transform baseCell in baseCells)
            {
                foreach (var windowName in glassRenderers.Keys)
                {
                    List<Transform> windows = baseCell.FindAllChildren(windowName);
                    foreach (Transform window in windows)
                    {
                        string[] rendererNames = glassRenderers[window.name];
                        if (rendererNames == null)
                        {
                            //AddDebug($"FixGlass no renderer names for window {window.name}");
                            //Main.logger.LogError($"FixGlass no renderer names for window {window.name}");
                        }
                        foreach (string rendererName in rendererNames)
                        {
                            Transform renderer = window.Find(rendererName);
                            if (renderer == null)
                            {
                                //AddDebug($"FixGlass window {window.name} has no renderer {rendererName}");
                                //Main.logger.LogError($"FixGlass window {window.name} has no renderer {rendererName}");
                                continue;
                            }
                            renderer.DisableShadowCasting();
                        }
                    }
                }
            }
        }

        private static void FixDeepGrandReefBasePillars(GameObject base_)
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

        private static void FixDeepGrandReefBaseDecals(GameObject base_)
        {
            Transform baseCell = base_.transform.GetChild(3);
            Transform coral = baseCell.GetChild(0);
            Transform t = coral.GetChild(21); // starfish on AC wall. Only 1 side is visible. Place it in AC
            t.SetPositionAndRotation(new Vector3(-641.3f, -503.25f, -945.5f), Quaternion.Euler(0, 120f, 0));
            t = coral.GetChild(22); // starfish on AC wall
            t.SetPositionAndRotation(new Vector3(-642f, -506.95f, -943f), Quaternion.Euler(0, 13, 0));
            t = coral.GetChild(23); // starfish on AC wall
            t.SetPositionAndRotation(new Vector3(-642.6f, -502.4f, -944f), Quaternion.Euler(0, 0, 0));
            t = coral.GetChild(6); // starfish on roof
            t.position = new Vector3(t.position.x, -497.86f, t.position.z);

            Transform decals = baseCell.GetChild(2);
            t = decals.GetChild(30); // texture z fighting
            t.position = new Vector3(-644.8f, t.position.y, t.position.z);

            baseCell = base_.transform.GetChild(5);
            decals = baseCell.GetChild(0);
            foreach (int i in new[] { 10, 23, 24 }) // stray decals
                decals.GetChild(i).gameObject.SetActive(false);

            coral = baseCell.GetChild(2);
            t = coral.GetChild(9);// stray starfish near big crate -633 -510 -938
            t.SetPositionAndRotation(new Vector3(-633f, -511.1f, -938.1f), Quaternion.Euler(0, 309f, 0));
            t = coral.GetChild(0);// stray starfish in big crate -634 -509 -938
            t.SetPositionAndRotation(new Vector3(-649f, -511.2f, -946f), Quaternion.Euler(0, 182f, 0));
        }

        private static void FixDecals_(GameObject base_)
        {
            for (int i = 1; i < 13; i++)
            {
                Transform baseCell_ = base_.transform.GetChild(i);
                Transform decals_ = baseCell_.Find("Decals");
                if (decals_ == null)
                    continue;

                foreach (Renderer r in decals_.GetComponentsInChildren<MeshRenderer>())
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            Transform baseCell = base_.transform.GetChild(3);
            Transform coral = baseCell.GetChild(0);
            Transform t = coral.GetChild(21); // starfish on AC wall. Only 1 side is visible. Place it in AC
            bool wasFixedBefore = t.eulerAngles.x == 0;
            //AddDebug($"FixDecals wasFixedBefore  {wasFixedBefore}");
            if (wasFixedBefore == false)
            {
                t.SetPositionAndRotation(new Vector3(-641.3f, -503.25f, -945.5f), Quaternion.Euler(0, 120f, 0));
                t = coral.GetChild(22); // starfish on AC wall
                t.SetPositionAndRotation(new Vector3(-642f, -506.95f, -943f), Quaternion.Euler(0, 13, 0));
                t = coral.GetChild(23); // starfish on AC wall
                t.SetPositionAndRotation(new Vector3(-642.6f, -502.4f, -944f), Quaternion.Euler(0, 0, 0));
                t = coral.GetChild(6); // starfish on roof
                t.position = new Vector3(-642.2f, -497.86f, -943.1f);
            }

            baseCell = base_.transform.GetChild(5);
            Transform decals = baseCell.GetChild(0);
            List<Transform> toDestroy = new List<Transform>();
            foreach (int i in new[] { 10, 23, 24 }) // stray decals
                toDestroy.Add(decals.GetChild(i));

            foreach (var tt in toDestroy)
                UnityEngine.Object.Destroy(tt.gameObject);

            //List<Transform> onBigCrate = new List<Transform>();
            //List<Transform> onCrate = new List<Transform>();
            //List<Transform> onCrate1 = new List<Transform>();
            //List<Transform> onCrate2 = new List<Transform>();
            //List<Transform> onCrate3 = new List<Transform>();
            if (wasFixedBefore)
                return;

            //Transform deco = baseCell.GetChild(1);
            coral = baseCell.GetChild(2);
            //AddDebug($"deco.childCount {deco.childCount}");

            for (int i = 0; i < coral.childCount; i++)
            {
                Transform child = coral.GetChild(i);
                Main.logger.LogDebug($"coral child {i} {child.name} {child.position}");
            }
            t = coral.GetChild(9);// stray starfish near big crate -633.074 -510.363 -938.11
            t.SetPositionAndRotation(new Vector3(-633f, -511.1f, -938.1f), Quaternion.Euler(0, 309f, 0));
            t = coral.GetChild(0);// stray starfish in big crate -634.105 -509.685 -938.331
            t.SetPositionAndRotation(new Vector3(-649f, -511.2f, -946f), Quaternion.Euler(0, 182f, 0));
            return;

            //t = coral.GetChild(15);
            //onBigCrate.Add(t);
            //t = coral.GetChild(coral.childCount - 1);
            //onBigCrate.Add(t);

            //foreach (int i in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 28, 29, 30 })
            //    onBigCrate.Add(decals.GetChild(i));

            //t = coral.GetChild(coral.childCount - 2);
            //onCrate.Add(t);
            //t = coral.GetChild(coral.childCount - 3);
            //onCrate.Add(t);
            //t = coral.GetChild(coral.childCount - 4);
            //onCrate.Add(t);

            //foreach (int i in new[] { 9, 20, 21 })
            //    onCrate.Add(decals.GetChild(i));

            //foreach (int i in new[] { 17, 18, 19, 22 })
            //    onCrate1.Add(decals.GetChild(i));

            //foreach (int i in new[] { 11, 12, 13, 25, 20 })
            //    onCrate2.Add(decals.GetChild(i));

            //foreach (int i in new[] { 14, 15, 16, 22, 26, 27 })
            //    onCrate3.Add(decals.GetChild(i));

            //for (int i = onBigCrate.Count; i-- > 0;)
            //    onBigCrate[i].SetParent(bigCrate);
            //for (int i = onCrate.Count; i-- > 0;)
            //    onCrate[i].SetParent(crate);
            //for (int i = onCrate1.Count; i-- > 0;)
            //    onCrate1[i].SetParent(crate1);
            //for (int i = onCrate2.Count; i-- > 0;)
            //    onCrate2[i].SetParent(crate2);
            //for (int i = onCrate3.Count; i-- > 0;)
            //    onCrate3[i].SetParent(crate3);
        }
    }
}
