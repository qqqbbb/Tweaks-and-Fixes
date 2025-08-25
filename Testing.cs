
using BepInEx;
using FMOD.Studio;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TMPro;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Testing
    {
        public static GameObject storedGO;
        public static PrefabIdentifier prefabIdentifier;



        static bool GetScanTarget(float distance, out GameObject result)
        {
            bool flag = false;
            Transform transform = MainCamera.camera.transform;
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            Ray ray = new Ray(position, forward);
            int layerMask = ~(1 << LayerID.OnlyVehicle);
            int numHits = UWE.Utils.RaycastIntoSharedBuffer(ray, distance, layerMask, QueryTriggerInteraction.Collide);
            //DebugTargetConsoleCommand.radius = -1f;
            RaycastHit resultHit = new RaycastHit();
            AddDebug("GetScanTarget numHits1 " + numHits);

            //if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, out resultHit))
            //    flag = true;
            for (int index1 = 0; index1 < numHits; ++index1)
            {
                RaycastHit hit = UWE.Utils.sharedHitBuffer[index1];
                Collider collider = hit.collider;
                if (collider == null)
                    continue;

                GameObject gameObject = collider.gameObject;
                Transform transform1 = collider.transform;
                if (gameObject == null || transform1 == null)
                    continue;

                int layer = gameObject.layer;
                //Transform transform2 = null;
                bool next = false;
                for (int index2 = 0; index2 < Targeting.ignoreList.Count; ++index2)
                {
                    Transform ignore = Targeting.ignoreList[index2];
                    if (transform1.IsAncestorOf(ignore))
                    {
                        //transform2 = ignore;
                        next = true;
                        break;
                    }
                }
                if (next)
                    continue;

                //if (transform2 == null)
                if (resultHit.collider == null || hit.distance < resultHit.distance)
                    resultHit = hit;

            }
            if (resultHit.collider != null)
            {
                GameObject go = Util.GetEntityRoot(resultHit.collider.gameObject);
                if (go)
                    AddDebug("GetScanTarget resultHit " + go.name);
            }


            //if (!flag)
            //{
            //    foreach (float radius in GameInput.IsPrimaryDeviceGamepad() ? Targeting.gamepadRadiuses : Targeting.standardRadiuses)
            //    {
            //        DebugTargetConsoleCommand.radius = radius;
            //        ray.origin = position + forward * radius;
            //        int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radius, distance, layerMask, queryTriggerInteraction);
            //        if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, out resultHit))
            //        {
            //            flag = true;
            //            break;
            //        }
            //    }
            //}
            Targeting.Reset();
            DebugTargetConsoleCommand.Stop();
            result = resultHit.collider != null ? resultHit.collider.gameObject : null;
            distance = resultHit.distance;
            return flag;
        }

        public static bool GetTarget(float maxDistance, out GameObject result, out float distance)
        {
            bool flag = false;
            Transform transform = MainCamera.camera.transform;
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            Ray ray = new Ray(position, forward);
            int layerMask = ~(1 << LayerID.Trigger | 1 << LayerID.OnlyVehicle);
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
            int numHits1 = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, layerMask, queryTriggerInteraction);
            DebugTargetConsoleCommand.radius = -1f;
            RaycastHit resultHit;
            if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, out resultHit))
                flag = true;
            if (!flag)
            {
                foreach (float radius in GameInput.IsPrimaryDeviceGamepad() ? Targeting.gamepadRadiuses : Targeting.standardRadiuses)
                {
                    DebugTargetConsoleCommand.radius = radius;
                    ray.origin = position + forward * radius;
                    int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
                    if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, out resultHit))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            Targeting.Reset();
            DebugTargetConsoleCommand.Stop();
            result = resultHit.collider != null ? resultHit.collider.gameObject : null;
            distance = resultHit.distance;
            return flag;
        }

        static void PrintRawBiomeNames()
        {

            AddDebug("RawBiomeName " + Util.GetRawBiomeName());
            AddDebug("Player biomeString " + Player.main.biomeString);
            //AddDebug("LargeWorld GetBiome " + LargeWorld.main.GetBiome(Player.main.transform.position));
            //AddDebug("GetRichPresence " + PlatformUtils.main.GetServices().GetRichPresence());
        }


        //[HarmonyPatch(typeof(RadiatePlayerInRange), "Radiate")]
        class RadiatePlayerInRange_Radiate_patch
        {
            public static void Postfix(RadiatePlayerInRange __instance)
            {
                AddDebug("RadiatePlayerInRange Radiate " + __instance.name);

            }
        }


        //[HarmonyPatch(typeof(Survival), "UpdateStats")]
        public static class UpdateStatsPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                var kFoodTimeField = AccessTools.Field(typeof(SurvivalConstants), nameof(SurvivalConstants.kFoodTime));

                foreach (var instruction in instructions)
                {
                    if (instruction.operand != null && instruction.opcode != null)
                        Main.logger.LogDebug("UpdateStats instruction opcode " + instruction.opcode + " operand " + instruction.operand + " " + instruction.operand.GetType());

                    if (instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == SurvivalConstants.kFoodTime)
                    {
                        //instruction.opcode = OpCodes.Ldc_R4;
                        Main.logger.LogDebug("UpdateStats Transpiler !!!");
                        instruction.operand = SurvivalConstants.kFoodTime * ConfigMenu.foodLossMult.Value;
                        Main.logger.LogDebug("UpdateStats Transpiler !!!!!!");
                    }
                }
                return codes.AsEnumerable();
            }
        }

        //[HarmonyPatch(typeof(GroundMotor), "GetMaxAcceleration")]
        class GroundMotor_GetMaxAcceleration_Patch
        {
            public static bool Prefix(GroundMotor __instance, ref float __result)
            {
                //if (!Util.IsGameLoadedAndRunning())
                //    return;

                //AddDebug("GroundMotor ApplyInputVelocityChange forwardMaxSpeed " + __instance.forwardMaxSpeed);
                AddDebug("Tf GroundMotor GetMaxAcceleration Prefix ");
                return false;
            }
            public static void Postfix(GroundMotor __instance, ref float __result)
            {
                if (!Main.gameLoaded)
                    return;

                //AddDebug("GroundMotor ApplyInputVelocityChange forwardMaxSpeed " + __instance.forwardMaxSpeed);
                AddDebug("TF GroundMotor GetMaxAcceleration Postfix ");

                //throw new Exception();
            }
        }



        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                if (!Main.gameLoaded)
                    return;

                //PrintRawBiomeNames();
                //AddDebug("Grounded " + __instance.groundMotor.IsGrounded());
                //AddDebug("mode " + __instance.mode);
                if (Input.GetKeyDown(KeyCode.B))
                {
                    //if (Player.main.IsInBase())
                    //    AddDebug("IsInBase");
                    //else if (Player.main.IsInSubmarine())
                    //    AddDebug("IsInSubmarine");
                    //else if (Player.main.inExosuit)
                    //    AddDebug("GetInMechMode");
                    //else if (Player.main.inSeamoth)
                    //    AddDebug("inSeamoth");
                    int x = Mathf.RoundToInt(Player.main.transform.position.x);
                    int y = Mathf.RoundToInt(Player.main.transform.position.y);
                    int z = Mathf.RoundToInt(Player.main.transform.position.z);
                    //AddDebug(x + " " + y + " " + z);

                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    //AddDebug("GetContinueMode " + Utils.GetContinueMode());
                    //PrintTerrainSurfaceType();
                    //FindObjectClosestToPlayer(3);
                    //AddDebug("activeTarget  " + Player.main.guiHand.activeTarget);
                    //                bool hit = Physics.Linecast(Player.main.transform.position, Vector3.zero
                    //                    , Voxeland.GetTerrainLayerMask());
                    //                AddDebug(" hit from player " + hit);
                    //                hit = Physics.Linecast(Vector3.zero, Player.main.transform.position
                    //, Voxeland.GetTerrainLayerMask());
                    //                AddDebug(" hit to player " + hit);

                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    Main.survival.water++; 
                    //else
                    //    Main.survival.food++;
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    printTarget();
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        //    //survival.water--;
                        __instance.liveMixin.health--;
                    else
                        //    //survival.food--;
                        __instance.liveMixin.health++;
                }
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    //GameObject goToTest = Player.main.guiHand.activeTarget;
                    //AddDebug("PDAScanner " + PDAScanner.complete.Contains(TechType.SeaglideFragment));
                    //AddDebug("KnownTech " + KnownTech.Contains(TechType.Seaglide));
                    //AddDebug("Exosuit " + BehaviourData.GetEcoTargetType(TechType.Exosuit));
                    //AddDebug("GetDepth " + Player.main.GetDepth());
                    //Vector3 vel = Player.main.currentMountedVehicle.useRigidbody.velocity;
                    //bool moving = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    //AddDebug("moving " + moving);

                    //AddDebug(" " + target.gameObject.name );
                    //if (target.transform.parent)
                    //    AddDebug("parent  " + target.transform.parent.name);

                    //if (target.GetComponent<InfectedMixin>())
                    //{
                    //    AddDebug("infectedAmount  " + target.GetComponent<InfectedMixin>().infectedAmount);
                    //}
                    //int x = (int)target.transform.position.x;
                    //int y = (int)target.transform.position.y;
                    //int z = (int)target.transform.position.z;
                    //AddDebug(x + " " + y + " " + z);
                    //Rigidbody rb = target.GetComponent<Rigidbody>();
                    //if (rb)
                    //    AddDebug("Rigidbody " + rb.mass);
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }
                }
            }


        }

        static void PrintTerrainSurfaceType()
        {
            VFXSurfaceTypes vfxSurfaceTypes = VFXSurfaceTypes.none;
            int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
            RaycastHit hitInfo;
            if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 111f, layerMask) && hitInfo.collider.gameObject.layer == LayerID.TerrainCollider)
            {
                vfxSurfaceTypes = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                AddDebug("vfxSurfaceTypes " + vfxSurfaceTypes);
            }
            else
                AddDebug("no terrain ");
        }

        public static void printTarget()
        {
            GameObject target = Player.main.guiHand.activeTarget;
            RaycastHit hitInfo = new RaycastHit();
            if (!target)
                //Util.GetTarget(Player.mainObject.transform.position, MainCamera.camera.transform.forward, 11f, out hitInfo);
                Targeting.GetTarget(Player.main.gameObject, 11f, out target, out float targetDist);
            //if (hitInfo.collider)
            //    target = hitInfo.collider.gameObject;

            if (!target)
                return;

            //AddDebug("target  " + target.name);
            GameObject root = Util.GetEntityRoot(target);
            if (root)
                target = root;

            //WorldForces worldForces = target.GetComponent<WorldForces>();
            //if (worldForces != null)
            //{
            //    AddDebug("WorldForces IsAboveWater " + worldForces.IsAboveWater());
            //}
            VFXSurfaceTypes vfxSurfaceType = VFXSurfaceTypes.none;
            TerrainChunkPieceCollider tcpc = target.GetComponent<TerrainChunkPieceCollider>();
            if (tcpc)
            {
                vfxSurfaceType = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                AddDebug("Terrain vfxSurfaceType  " + vfxSurfaceType);
                return;
            }
            if (target)
                vfxSurfaceType = Util.GetObjectSurfaceType(target);

            AddDebug("vfxSurfaceType  " + vfxSurfaceType);
            LargeWorldEntity lwe = target.GetComponentInParent<LargeWorldEntity>();
            if (lwe)
            {
                //goToTest = lwe.gameObject;
                //target = lwe.gameObject;
                //Rigidbody rb = lwe.GetComponent<Rigidbody>();
                //if (rb)
                //    AddDebug("Rigidbody mass " + rb.mass + " drag " + rb.drag + " ang useGravity " + rb.useGravity);

                //WorldForces wf = lwe.GetComponent<WorldForces>();
                //if (wf)
                //    AddDebug("WorldForces handleGravity " + wf.handleGravity + " underwaterGravity " + wf.underwaterGravity + " underwaterDrag " + wf.underwaterDrag);
                //AddDebug("PDAScanner isValid " + PDAScanner.scanTarget.isValid);
                //AddDebug("PDAScanner CanScan " + PDAScanner.CanScan());
                //AddDebug("PDAScanner scanTarget " + PDAScanner.scanTarget.techType);
                //AddDebug(" cellLevel " + lwe.cellLevel);
                //AddDebug("vfxSurfaceType  " + vfxSurfaceType);
                //LiveMixin lm = lwe.GetComponent<LiveMixin>();
                //if (lm)
                //    AddDebug("max HP " + lm.data.maxHealth + " HP " + lm.health);
            }
            //AddDebug(target.name);
            //AddDebug(target.name + " IsDecoPlant " + Util.IsDecoPlant(target));
            //if (target.transform.parent)
            //    AddDebug(target.transform.parent.name);

            //AddDebug("parent " + target.transform.parent.gameObject.name);
            //if (target.transform.parent.parent)
            //    AddDebug("parent parent " + target.transform.parent.parent.gameObject.name);
            MedicalCabinet medicalCabinet = target.GetComponent<MedicalCabinet>();
            if (medicalCabinet)
            {
                medicalCabinet.playSound.evt.getPlaybackState(out PLAYBACK_STATE state);
                AddDebug(" state " + state + " hasMedKit " + medicalCabinet.hasMedKit);
            }
            TechType techType = CraftData.GetTechType(target);
            FruitPlant fruitPlant = target.GetComponent<FruitPlant>();
            if (fruitPlant != null)
            {
                if (!fruitPlant.fruitSpawnEnabled)
                    AddDebug("fruitPlant fruit Spawn disabled ");

                AddDebug("fruitPlant SpawnInterval " + fruitPlant.fruitSpawnInterval);
                PickPrefab[] pickPrefabs = target.GetComponentsInChildren<PickPrefab>(true);
                AddDebug("fruitPlant pickPrefabs " + pickPrefabs.Length);

            }
            if (techType != TechType.None)
                AddDebug("TechType  " + techType);

            int x = (int)target.transform.position.x;
            int y = (int)target.transform.position.y;
            int z = (int)target.transform.position.z;
            //AddDebug(x + " " + y + " " + z);
        }

        private Vector3 ClipWithTerrain(GameObject go)
        {
            Vector3 origin = go.transform.position;
            //origin.y = go.transform.position.y + 5f;
            //RaycastHit hitInfo;
            //if (!Physics.Raycast(new Ray(origin, Vector3.down), out hitInfo, 10f, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
            //    return;
            //go.transform.position.y = Mathf.Max(go.transform.position.y, hitInfo.point.y + 0.3f);
            return origin;
        }

        void GetGO()
        {
            int numHits = UWE.Utils.SpherecastIntoSharedBuffer(Player.main.transform.position, 2f, Vector3.forward);
            AddDebug("num Hits " + numHits);
            AddDebug("sharedHitBuffer.Length " + UWE.Utils.sharedHitBuffer.Length);
            for (int index1 = 0; index1 < numHits; ++index1)
            {
                RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[index1];
                Vector3 point = raycastHit.point;
                AddDebug("raycastHit " + raycastHit.collider.gameObject.name);
                GameObject go = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
                if (go == null)
                    AddDebug("go == null " + raycastHit.collider.gameObject.name);
                else
                    AddDebug(go.name);
            }
        }

        //[HarmonyPatch(typeof(VoxelandGrassBuilder), "CreateUnityMeshes")]
        class VoxelandGrassBuilder_CreateUnityMeshes_Patch
        {
            static bool Prefix(VoxelandGrassBuilder __instance, IVoxelandChunk2 chunk, TerrainPoolManager terrainPoolManager)
            {
                for (int index = 0; index < __instance.builtMeshes.Count; ++index)
                {
                    TerrainChunkPiece grassObj = __instance.GetGrassObj(chunk, terrainPoolManager);
                    chunk.grassFilters.Add(grassObj.meshFilter);
                    chunk.grassRenders.Add(grassObj.meshRenderer);
                    chunk.chunkPieces.Add(grassObj);
                    MeshFilter grassFilter = chunk.grassFilters[index];
                    grassFilter.gameObject.SetActive(true);
                    MeshRenderer grassRender = chunk.grassRenders[index];
                    VoxelandBlockType type = __instance.types[index];
                    grassFilter.sharedMesh = terrainPoolManager.GetMeshForPiece(grassObj);
                    Material grassMaterial = type.grassMaterial;
                    grassRender.sharedMaterial = grassMaterial;
                    //Main.logger.LogDebug("material  " + grassRender.material.name + " VoxelandBlockType " + type.name + " grassMeshName " + type.grassMeshName + " layer " + type.layer + " filled " + type.filled);
                    //AddDebug("grassRender.material  " + grassRender.material.name);
                    //Main.logger.LogDebug("grassRender " + grassRender.material.name);
                    //coral_reef_grass_10_gr    coral_reef_grass_11_02_gr   coral_reef_grass_07_gr
                    //if (grassRender.material.name == "Coral_reef_red_seaweed_03 (Instance)" || grassRender.material.name == "Coral_reef_red_seaweed_01 (Instance)")
                    {
                        //AddDebug("!!!");
                        UWE.MeshBuffer builtMesh = __instance.builtMeshes[index];
                        builtMesh.Upload(grassFilter.sharedMesh);
                        builtMesh.Return();
                    }
                }
                __instance.state = VoxelandGrassBuilder.State.Init;
                return false;
            }
        }

        //[HarmonyPatch(typeof(FreezeRigidbodyWhenFar), "FixedUpdate")]
        class SubControl_Update_Patch
        {
            public static void Prefix(FreezeRigidbodyWhenFar __instance)
            {
                if (__instance.transform.position.y > __instance.freezeDist / 2.0)
                    return;
                if ((MainCamera.camera.transform.position - __instance.transform.position).sqrMagnitude > __instance.freezeDist * __instance.freezeDist)
                    AddDebug("FreezeRigidbodyWhenFar FixedUpdate ");
                else
                    AddDebug("FreezeRigidbodyWhenFar FixedUpdate !!!!!!!!!");

            }
        }

        //[HarmonyPatch(typeof(Story.StoryGoal), "Execute")]
        class StoryGoal_Execute_Patch
        {
            public static void Postfix(Story.StoryGoal __instance, string key, Story.GoalType goalType)
            {
                AddDebug("StoryGoal " + key);
                AddDebug("goalType " + goalType);
                //return false;
            }
        }

        //[HarmonyPatch(typeof(PDAEncyclopedia), "AddAndPlaySound")]
        class PDAEncyclopedia_AddAndPlaySound_Patch
        {
            public static void Postfix(string key, PDAEncyclopedia.EntryData __result)
            {
                AddDebug("AddAndPlaySound " + key);
                AddDebug("EntryData " + __result.key);
                //return false;
            }
        }

        //[HarmonyPatch(typeof(LargeWorldEntity), "Awake")]
        class LargeWorldEntity_Awake_patch
        {
            public static void Postfix(LargeWorldEntity __instance)
            {
                if (__instance.name == "AbandonedBaseJellyShroom1(Clone)")
                {
                    Material dirtyGlassMat = null;
                    //Transform dirtyGlassTr = __instance.transform.Find("Culling/BaseCell/BaseAbandonedObservatory/BaseAbandonedRoomObservatory/BaseRoomObservatory_glass");
                    Transform dirtyGlassTr = __instance.transform.Find("Culling/BaseCell/BaseAbandonedRoomWindowSide/BaseRoomGenericInteriorWindowSide01Broken01/BaseExteriorRoomGenericWindowSide01Glass");
                    if (dirtyGlassTr)
                    {
                        MeshRenderer mr = dirtyGlassTr.GetComponent<MeshRenderer>();
                        if (mr)
                            dirtyGlassMat = mr.material;
                    }
                    if (dirtyGlassMat == null)
                        return;

                    Transform glassTr = __instance.transform.Find("Culling/BaseCell/BaseAbandonedCorridorIShapeGlass/models/BaseCorridorhIShapeGlass01Exterior/BaseCorridorhIShapeGlass01ExteriorGlass");
                    if (glassTr)
                    {
                        MeshRenderer mr = glassTr.GetComponent<MeshRenderer>();
                        mr.material = dirtyGlassMat;
                        //mr.sharedMaterial = dirtyGlassMat;
                        //mr.material.mainTextureOffset = new Vector2(.5f, 1.5f);
                        //AddDebug("color " + mr.material.color);
                    }

                }

            }
        }

        static void Debug(GameObject go)
        {
            if (!go || go.name == "Debug")
                return;

            if (!go.transform.Find("Debug"))
            {
                GameObject debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debug.name = "Debug";
                debug.GetComponent<SphereCollider>().enabled = false;
                debug.transform.SetParent(go.transform, false);
                debug.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
                AddDebug("Creating debug sphere for: " + go.name);
                //debug.transform.localScale = Vector3.one * 0.1f;
            }
            //for (var i = 0; i < go.transform.childCount; ++i)
            //    Debug(go.transform.GetChild(i).gameObject);
        }

        //[HarmonyPatch(typeof(Targeting), "GetTarget", new Type[] { typeof(float), typeof(GameObject), typeof(float), typeof(Targeting.FilterRaycast) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal })]
        class Targeting_GetTarget_PostfixPatch
        {
            public static void Postfix(ref GameObject result)
            {
                //AddDebug(" Targeting GetTarget  " + result.name);
            }
        }

        //[HarmonyPatch(typeof(CrafterLogic), "progress", MethodType.Getter)]
        class CrafterLogic_progress_Patch
        { // patch property getter
            public static bool Prefix(CrafterLogic __instance, ref float __result)
            {
                double timePassed = DayNightCycle.main.timePassed;
                double num1 = __instance.timeCraftingEnd - __instance.timeCraftingBegin;
                double timeCraftingBegin = __instance.timeCraftingBegin;
                double num2 = ((timePassed - timeCraftingBegin) / num1);
                __result = __instance.timeCraftingEnd <= __instance.timeCraftingBegin ? -1f : Mathf.Clamp01((float)num2);
                return false;
            }
        }

        //[HarmonyPatch(typeof(ExampleClass), MethodType.Constructor)]
        //[HarmonyPatch(new Type[] { typeof(int) })]
        //class ExampleClassConstructorPatch
        //{
        //    static void Postfix(int value)
        //    {
        //        Console.WriteLine($"Postfix: {value}");
        //    }
        //}
    }
}
