
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
{
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

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                
                //AddDebug("activeSelf " + IngameMenu.main.gameObject.activeSelf);
                //float movementSpeed = (float)System.Math.Round(__instance.movementSpeed * 10f) / 10f;

                if (Input.GetKey(KeyCode.B))
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
                else if (Input.GetKey(KeyCode.C))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        Main.survival.water++;
                    else
                        Main.survival.food++;
                }
                else if (Input.GetKey(KeyCode.X))
                {
                    if (Input.GetKey(KeyCode.LeftShift))
                        //survival.water--;
                        __instance.liveMixin.health--;
                    else
                        //survival.food--;
                        __instance.liveMixin.health++;
                }
                else if(Input.GetKey(KeyCode.Z))
                {
                    //AddDebug("PDAScanner " + PDAScanner.complete.Contains(TechType.SeaglideFragment));
                    //AddDebug("KnownTech " + KnownTech.Contains(TechType.Seaglide));
                    //AddDebug("sub EcoTargetType " + BehaviourData.GetEcoTargetType(Player.main.currentSub.gameObject));
                    //AddDebug("Exosuit " + BehaviourData.GetEcoTargetType(TechType.Exosuit));
                    //AddDebug("GetDepth " + Player.main.GetDepth());
                    //Vector3 vel = Player.main.currentMountedVehicle.useRigidbody.velocity;
                    //bool moving = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    //AddDebug("moving " + moving);
                    Targeting.GetTarget(Player.main.gameObject, 5f, out GameObject target, out float targetDist);
                    if (target)
                    {
                        UniqueIdentifier ui = target.GetComponentInParent<UniqueIdentifier>();
                        if (ui)
                        {
                            AddDebug("target " + ui.gameObject.name);
                            AddDebug("target TechType " + CraftData.GetTechType( ui.gameObject));
                        }
                    }
                    if (Main.guiHand.activeTarget)
                    {
                        //VFXSurface[] vFXSurfaces = __instance.GetAllComponentsInChildren<VFXSurface>();
                        //if (vFXSurfaces.Length == 0)
                        //    AddDebug(" " + Main.guiHand.activeTarget.name + " no VFXSurface");
                        //else
                        ChildObjectIdentifier coi = Main.guiHand.activeTarget.GetComponentInParent<ChildObjectIdentifier>();
                        PrefabIdentifier pi = Main.guiHand.activeTarget.GetComponentInParent<PrefabIdentifier>();
                        if (coi)
                            AddDebug("activeTarget child " + coi.gameObject.name);
                        if (pi)
                            AddDebug("activeTarget  " + pi.gameObject.name);
                        LiveMixin lm = pi.GetComponent<LiveMixin>();
                        if (lm)
                        {
                            AddDebug("max HP " + lm.data.maxHealth);
                            AddDebug(" HP " + lm.health);
                        }
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
   
             //[HarmonyPatch(typeof(LiveMixin), "Start")]
            class Vehicle_LiveMixin_patch
        {
            public static void Postfix(LiveMixin __instance)
            {
                TechTag techTag = __instance.GetComponent<TechTag>();
                if (techTag)
                {
                    AddDebug(" techTag " + techTag.type);
                    Main.Log(" techTag " + techTag.type);
                }
            }
        }


    }
}
