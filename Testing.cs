
using HarmonyLib;
using System.Reflection;
using System;
using Nautilus.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static ErrorMessage;
using static VFXParticlesPool;
using static HandReticle;
using static UWE.CubeFace;
using System.Linq;
using UWE;
using static TechStringCache;

namespace Tweaks_Fixes
{
    class Testing
    {


        //[HarmonyPatch(typeof(Utils), nameof(Utils.PlayFMODAsset), new Type[] {typeof(FMODAsset), typeof(Transform), typeof(float) })]
        class MeshRenderer_Start_patch
        {
            public static bool Prefix(FMODAsset asset, Transform t)
            {
                if (asset == null)
                    AddDebug("Utils PlayFMODAsset null ");
                else
                    AddDebug("Utils PlayFMODAsset " + asset.name);
                //return false;
                //AddDebug("Knife OnToolUseAnim 1");
                return true;
            }
        }

        //[HarmonyPatch(typeof(DisplayManager))]
        class DisplayManager_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Update")]
            static void UpdatePostfix(DisplayManager __instance)
            {
                //AddDebug("Screen.currentResolution.width " + Screen.currentResolution.width);
                //AddDebug("DisplayManager.resolution.width " + __instance.resolution.width);
                if (Screen.currentResolution.width != 1280)
                {
                    Screen.SetResolution(1280, 720, true);
                    //__instance.resolution = Screen.currentResolution;
                }
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("Initialize")]
            static void InitializePrefix(DisplayManager __instance)
            {
                if (Screen.currentResolution.width != __instance.resolution.width)
                    AddDebug("Resolution !!! ");

                AddDebug("Initialize Screen.currentResolution " + Screen.currentResolution.width);
                AddDebug("Initialize DisplayManager.resolution " + __instance.resolution.width);
                Main.logger.LogMessage("Resolution should be fixed");
                if (Screen.currentResolution.width != 1280)
                {
                    Main.logger.LogMessage("Resolution should be fixed");
                    AddDebug("Resolution should be fixed");
                    //Screen.SetResolution(1280, 720, true);
                    //__instance.resolution = Screen.currentResolution;
                }
            }
        }


        //[HarmonyPatch(typeof(AggressiveWhenSeePlayer), "GetAggressionTarget")]
        class CreatureDeath_OnKillAsync_Patch
        {
            static void Postfix(AggressiveWhenSeePlayer __instance)
            {
                AddDebug(__instance.name + " AggressiveWhenSeePlayer GetAggressionTarget ");

            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                //AddDebug("activeTarget " + Player.main.guiHand.activeTarget);
                //AddDebug("stalkerLoseTooth " + Main.config.stalkerLoseTooth * .01f);
                //AddDebug("Time.time " + (int)Time.time);
                //AddDebug("isUnderwaterForSwimming " + __instance.isUnderwaterForSwimming.value);
                //float movementSpeed = (float)System.Math.Round(__instance.movementSpeed * 10f) / 10f;
                if (Input.GetKeyDown(KeyCode.B))
                {
                    //AddDebug("activeSlot " + Inventory.main.quickSlots.activeSlot);
                    //AddDebug("currentSlot " + Main.config.escapePodSmokeOut[SaveLoadManager.main.currentSlot]);
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
                    AddDebug(x + " " + y + " " + z);
                    AddDebug("GetBiomeString " + Player.main.GetBiomeString());
                    AddDebug("LargeWorld GetBiome " + LargeWorld.main.GetBiome(__instance.transform.position));
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    PrintTerrainSurfaceType();
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

                    //goToTest = Player.main.guiHand.activeTarget;
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
                AddDebug("no terrain " );
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

            LargeWorldEntity lwe = target.GetComponentInParent<LargeWorldEntity>();
            if (lwe)
            {
                //goToTest = lwe.gameObject;
                target = lwe.gameObject;
                Rigidbody rb = lwe.GetComponent<Rigidbody>();
                if (rb)
                {
                    AddDebug(" mass " + rb.mass + " drag " + rb.drag + " ang drag " + rb.angularDrag);
                }
                //AddDebug("PDAScanner isValid " + PDAScanner.scanTarget.isValid);
                //AddDebug("PDAScanner CanScan " + PDAScanner.CanScan());
                //AddDebug("PDAScanner scanTarget " + PDAScanner.scanTarget.techType);

                
                //AddDebug(" cellLevel " + lwe.cellLevel);
                //AddDebug("vfxSurfaceType  " + vfxSurfaceType);
                //LiveMixin lm = lwe.GetComponent<LiveMixin>();
                //if (lm)
                //    AddDebug("max HP " + lm.data.maxHealth + " HP " + lm.health);
            }
            AddDebug(target.gameObject.name);
            //AddDebug("parent " + target.transform.parent.gameObject.name);
            //if (target.transform.parent.parent)
            //    AddDebug("parent parent " + target.transform.parent.parent.gameObject.name);
            TechType techType = CraftData.GetTechType(target);
            if (techType != TechType.None)
                AddDebug("TechType  " + techType);
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

        //[HarmonyPatch(typeof(SubRoot), "Update")]
        class SubControl_Update_Patch
        {
            public static void Prefix(SubRoot __instance)
            {
                AddDebug("SubRoot Update LOD " + __instance.LOD.IsFull());
                //AddDebug("CyclopsHelmHUDManager Update IsAlive " + __instance.subLiveMixin.IsAlive());
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
                debug.GetComponent<MeshRenderer>().material.color = new Color(1f,0f,0f);
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
    }
}
