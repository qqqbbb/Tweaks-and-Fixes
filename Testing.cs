
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{ // 
    class Testing
    {
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

        void  GetGO()
        {
            int numHits = UWE.Utils.SpherecastIntoSharedBuffer(Player.main.transform.position, 2f, Vector3.forward);
            AddDebug("num Hits " + numHits);
            AddDebug("sharedHitBuffer.Length " + UWE.Utils.sharedHitBuffer.Length);
            for (int index1 = 0; index1<numHits; ++index1)
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

        //[HarmonyPatch(typeof(DamageOnPickup))]
        class ReaperLeviathan_Patch
        {
            //[HarmonyPatch("OnPickedUp")]
            //[HarmonyPostfix]
            static void OnPickedUpPostfix(DamageOnPickup __instance, Pickupable pickupable)
            {
                AddDebug("DamageOnPickup OnPickedUp " + __instance.damageChance + " damageOnPickup " + __instance.damageOnPickup);
            }
            //[HarmonyPatch("OnKill")]
            //[HarmonyPostfix]
            static void OnKillPostfix(DamageOnPickup __instance)
            {
                AddDebug("DamageOnPickup OnKill damageAmount " + __instance.damageAmount);
            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                //Main.Message("Time.time " + Time.time);
                //AddDebug("activeSelf " + IngameMenu.main.gameObject.activeSelf);
                //float movementSpeed = (float)System.Math.Round(__instance.movementSpeed * 10f) / 10f;

                if (Input.GetKeyDown(KeyCode.B))
                {
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
                    AddDebug("" + Player.main.GetBiomeString());
                    //Inventory.main.container.Resize(8,8);   GetPlayerBiome()
                    //HandReticle.main.SetInteractText(nameof(startingFood) + " " + dict[i]);
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    AddDebug(" loadingDone " + Main.loadingDone);
                    AddDebug("  " + Player.main.GetBiomeString());
                    Main.Log(" GetBiomeString " + Player.main.GetBiomeString());
                     //if (Input.GetKey(KeyCode.LeftShift))
                    //    Main.survival.water++; 
                    //else
                    //    Main.survival.food++;
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        //survival.water--;
                        __instance.liveMixin.health--;
                    else
                        //survival.food--;
                        __instance.liveMixin.health++;
                }
                else if(Input.GetKeyDown(KeyCode.Z))
                {
                    //AddDebug("PDAScanner " + PDAScanner.complete.Contains(TechType.SeaglideFragment));
                    //AddDebug("KnownTech " + KnownTech.Contains(TechType.Seaglide));
                    //AddDebug("sub EcoTargetType " + BehaviourData.GetEcoTargetType(Player.main.currentSub.gameObject));
                    //AddDebug("Exosuit " + BehaviourData.GetEcoTargetType(TechType.Exosuit));
                    //AddDebug("GetDepth " + Player.main.GetDepth());
                    //Vector3 vel = Player.main.currentMountedVehicle.useRigidbody.velocity;
                    //bool moving = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    //AddDebug("moving " + moving);
                    GameObject target = Main.guiHand.activeTarget;
                    if (!target)
                        Targeting.GetTarget(Player.main.gameObject, 5f, out target, out float targetDist);
                    //if (!target)
                    //{
                    //    int numHits = Physics.RaycastNonAlloc(new Ray(MainCamera.camera.transform.position, MainCamera.camera.transform.forward), hits, 2.5f);
                    //    for (int index = 0; index < numHits; ++index)
                    //    {
                    //        AddDebug(hits[index].collider.name + " " + hits[index].collider.transform.position);
                    //        Main.Log("player target " + hits[index].collider.name + " " + hits[index].collider.transform.position);
                    //    }
                    //}
                    if (target)
                    {
                        PrefabIdentifier pi = target.GetComponentInParent<PrefabIdentifier>();
                        if (pi)
                            target = pi.gameObject;
                        //LiveMixin lm = pi.GetComponent<LiveMixin>();
                        //if (lm)
                        //{
                        //    AddDebug("max HP " + lm.data.maxHealth);
                        //    AddDebug(" HP " + lm.health);
                        //}
                        AddDebug(" " + target.gameObject.name);
                        if (target.transform.parent)
                            AddDebug("parent  " + target.transform.parent.name);
                        AddDebug("TechType  " + CraftData.GetTechType(target.gameObject));
                        int x = (int)target.transform.position.x;
                        int y = (int)target.transform.position.y;
                        int z = (int)target.transform.position.z;
                        AddDebug(x + " " + y + " " + z);
                        Rigidbody rb = target.GetComponent<Rigidbody>();
                        if (rb)
                            AddDebug("Rigidbody " + rb.mass);
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(GUIHand), "UpdateActiveTarget")]
        class GUIHand_UpdateActiveTarget_Patch
        {
            public static bool Prefix(GUIHand __instance)
            {
                PlayerTool tool = __instance.GetTool();
                if (tool)
                {
                    if (tool.GetComponent<PropulsionCannon>() && tool.GetComponent<PropulsionCannon>().IsGrabbingObject())
                    {
                        __instance.activeTarget = tool.GetComponent<PropulsionCannon>().GetNearbyGrabbedObject();
                        __instance.suppressTooltip = true;
                        return false;
                    }
                    else if (tool.DoesOverrideHand())
                    {
                        __instance.activeTarget = null;
                        __instance.activeHitDistance = 0f;
                        return false;
                    }
                }
                //AddDebug("UpdateActiveTarget " );
                GameObject target = null;
                if (!Targeting.GetTarget(Player.main.gameObject, 2f, out target, out __instance.activeHitDistance))
                {
                    __instance.activeTarget = null;
                    __instance.activeHitDistance = 0f;
                    return false;
                }
                //if (__instance.activeTarget == target)
                //    return false;
                //else
                    __instance.activeTarget = target;

                //AddDebug("UpdateActiveTarget 1");
                if (__instance.activeTarget && __instance.activeTarget.layer == LayerID.NotUseable)
                {
                    __instance.activeTarget = null;
                    return false;
                }
                else
                {
                    TechType tt = CraftData.GetTechType(__instance.activeTarget);
                    AddDebug("TechType " + tt);
                    //if (PDAScanner.complete.Contains(tt))
                    //{
                    //    AddDebug("PDAScanner.complete " + tt);
                    //}
                    IHandTarget handTarget = null;
                    for (Transform transform = __instance.activeTarget.transform; transform != null; transform = transform.parent)
                    {
                        handTarget = transform.GetComponent<IHandTarget>();
                        if (handTarget != null)
                        {
                            AddDebug("handTarget " + tt);
                            __instance.activeTarget = transform.gameObject;
                            return false;
                        }
                    }
                    if (handTarget == null)
                    {
                        switch (CraftData.GetHarvestTypeFromTech(tt))
                        {
                            case HarvestType.None:
                                __instance.activeTarget = null;
                                break;
                            case HarvestType.Pick:
                                if (Utils.FindAncestorWithComponent<Pickupable>(__instance.activeTarget) == null)
                                {
                                    LargeWorldEntity lwe = Utils.FindAncestorWithComponent<LargeWorldEntity>(__instance.activeTarget);
                                    lwe.gameObject.AddComponent<Pickupable>();
                                    lwe.gameObject.AddComponent<WorldForces>().useRigidbody = lwe.GetComponent<Rigidbody>();
                                    break;
                                }
                                break;
                        }
                        if (PDAScanner.complete.Contains(tt))
                        {
                            AddDebug("PDAScanner.complete " + tt);
                            //__instance.activeTarget = target;
                        }
                    }
                }
                if (IntroVignette.isIntroActive)
                    __instance.activeTarget = __instance.FilterIntroTarget(__instance.activeTarget);

                return false;
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


    }
}
