
using HarmonyLib;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{ // debris 80 -35 100      200 -70 -680
    class Testing
    {
        //static HashSet<TechType> creatures = new HashSet<TechType>();
        //static Dictionary<TechType, int> creatureHealth = new Dictionary<TechType, int>();
       
        //[HarmonyPatch(typeof(Planter), "IsAllowedToAdd")]
        class Planter_IsAllowedToAdd_Patch
        {
            static void Postfix(Planter __instance, bool __result, Pickupable pickupable)
            {
                if (pickupable == null)
                {
                    AddDebug("Planter IsAllowedToAdd pickupable == null ");
                    return;
                }
                Plantable plantable = pickupable.GetComponent<Plantable>();
                if (plantable == null)
                {
                    return;
                }
                //AddDebug("Planter " + __instance.GetContainerType() + " " );
                AddDebug("plantable size " + plantable.size);
                AddDebug("GetFreeSlotID small " + __instance.GetFreeSlotID());
                AddDebug("GetFreeSlotID big " + __instance.GetFreeSlotID(true));
                //if (__instance.GetFreeSlotID((uint)plantable.size > 0U) < 0)
                //    return ;
                AddDebug("Planter IsAllowedToAdd " + pickupable.GetTechName() + " " + __result);
            }
        }

        //[HarmonyPatch(typeof(ItemsContainer), "IItemsContainer.AllowedToAdd")]
        class ItemsContainer_AllowedToAdd_Patch
        {
            static void Postfix(ItemsContainer __instance, Pickupable pickupable, bool __result)
            {
                AddDebug("ItemsContainer AllowedToAdd " + pickupable.GetTechName() + " " + __result);
            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static void Postfix(Player __instance)
            {
                //AddDebug("WaitScreen.IsWaiting " + WaitScreen.IsWaiting);
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
                    AddDebug("" + Player.main.GetBiomeString());
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    
                    //AddDebug(" WaitScreen.IsWaiting " + WaitScreen.IsWaiting);
                    //AddDebug("  " + Player.main.GetBiomeString());
                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    Main.survival.water++; 
                    //else
                    //    Main.survival.food++;
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
                else if(Input.GetKeyDown(KeyCode.Z))
                {
                    //AddDebug("PDAScanner " + PDAScanner.complete.Contains(TechType.SeaglideFragment));
                    //AddDebug("KnownTech " + KnownTech.Contains(TechType.Seaglide));
                    //AddDebug("Exosuit " + BehaviourData.GetEcoTargetType(TechType.Exosuit));
                    //AddDebug("GetDepth " + Player.main.GetDepth());
                    //Vector3 vel = Player.main.currentMountedVehicle.useRigidbody.velocity;
                    //bool moving = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    //AddDebug("moving " + moving);
                    GameObject target = Player.main.guiHand.activeTarget;
                    //GameObject target = null;
                    if (!target)
                        Targeting.GetTarget(Player.main.gameObject, 11f, out target, out float targetDist);
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
                        //Debug(target);
                        //PrefabIdentifier pi = target.GetComponentInParent<PrefabIdentifier>();
                        //if (pi)
                        //    target = pi.gameObject;
                        //LiveMixin lm = pi.GetComponent<LiveMixin>();
                        //if (lm)
                        //{
                        //    AddDebug("max HP " + lm.data.maxHealth);
                        //    AddDebug(" HP " + lm.health);
                        //}
                        AddDebug(" " + target.gameObject.name);
                        if (target.transform.parent)
                            AddDebug("parent  " + target.transform.parent.name);
                        AddDebug("TechType  " + CraftData.GetTechType(target));
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
