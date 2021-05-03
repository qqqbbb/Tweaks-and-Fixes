
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
                Player.main.oxygenMgr.AddOxygen(115f);
                //ErrorMessage.AddDebug("health " + (int)Player.main.liveMixin.health);
                //ErrorMessage.AddDebug("timePassedAsFloat " + DayNightCycle.main.timePassedAsFloat);
                //float movementSpeed = (float)System.Math.Round(__instance.movementSpeed * 10f) / 10f;
                //if (uGUI.main.loading.IsLoading)
                //    Main.Message("Loading");
                if (Input.GetKey(KeyCode.B))
                {
                    //ErrorMessage.AddDebug("currentSlot " + Main.config.escapePodSmokeOut[SaveLoadManager.main.currentSlot]);
                    //if (Player.main.IsInBase())
                    //    ErrorMessage.AddDebug("IsInBase");
                    //else if (Player.main.IsInSubmarine())
                    //    ErrorMessage.AddDebug("IsInSubmarine");
                    //else if (Player.main.inExosuit)
                    //    ErrorMessage.AddDebug("GetInMechMode");
                    //else if (Player.main.inSeamoth)
                    //    ErrorMessage.AddDebug("inSeamoth");
                    int x = Mathf.RoundToInt(Player.main.transform.position.x);
                    int y = Mathf.RoundToInt(Player.main.transform.position.y);
                    int z = Mathf.RoundToInt(Player.main.transform.position.z);
                    ErrorMessage.AddDebug(x + " " + y + " " + z);
                    ErrorMessage.AddDebug("" + Player.main.GetBiomeString());
                    //Inventory.main.container.Resize(8,8);   GetPlayerBiome()
                    //HandReticle.main.SetInteractText(nameof(startingFood) + " " + dict[i]);
                }

                if (Input.GetKey(KeyCode.C))
                {
                    Survival survival = Player.main.GetComponent<Survival>();

                    if (Input.GetKey(KeyCode.LeftShift))
                        survival.water++;
                    else
                        survival.food++;
                }

                if (Input.GetKey(KeyCode.X))
                {
                    Survival survival = Player.main.GetComponent<Survival>();
                    if (Input.GetKey(KeyCode.LeftShift))
                        survival.water--;
                    else
                        survival.food--;
                }
                if (Input.GetKey(KeyCode.Z))
                {
                    //ErrorMessage.AddDebug("CanBeAttacked " + Player.main.CanBeAttacked());
                    
                    Targeting.GetTarget(Player.main.gameObject, 5f, out GameObject target, out float targetDist);
                    if (target)
                    {
  

                    }
                    if (Main.guiHand.activeTarget)
                    {
                        ErrorMessage.AddDebug(" " + Main.guiHand.activeTarget.name);
                        ErrorMessage.AddDebug("TechType " + CraftData.GetTechType(Main.guiHand.activeTarget));
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }

                    //else

                    //Inventory.main.DropHeldItem(true);
                    //Player.main.liveMixin.TakeDamage(99);
                    //Pickupable held = Inventory.main.GetHeld();
                    //ErrorMessage.AddDebug("isUnderwaterForSwimming " + Player.main.isUnderwaterForSwimming.value);
                    //ErrorMessage.AddDebug("isUnderwater " + Player.main.isUnderwater.value);
                    //LaserCutObject laserCutObject = 
                    //Inventory.main.quickSlots.Select(1);

                    if (Main.guiHand.activeTarget)
                    {
                        //ErrorMessage.AddDebug("activeTarget " + Main.guiHand.activeTarget.name);
                        //ErrorMessage.AddDebug(" " + CraftData.GetTechType(Main.guiHand.activeTarget));
                        //RadiatePlayerInRange radiatePlayerInRange = Main.guiHand.activeTarget.GetComponent<RadiatePlayerInRange>();
                        //if (radiatePlayerInRange)
                        {

                        }
                        //else
                        //    ErrorMessage.AddDebug("no radiatePlayerInRange " );

                    }
                    //if (target)
                    //    Main.Message(" target " + target.name);
                    //else
                    //{
                    //TechType techType = CraftData.GetTechType(target);
                    //HarvestType harvestTypeFromTech = CraftData.GetHarvestTypeFromTech(techType);
                    //TechType harvest = CraftData.GetHarvestOutputData(techType);
                    //Main.Message("techType " + techType.AsString() );
                    //Main.Message("name " + target.name);
                    //}
                }
            }
        }

        static bool done = false;
        //[HarmonyPatch(typeof(GUIHand), "UpdateActiveTarget")]
        class GUIHand_UpdateActiveTarget_Patch
        {
            public static bool Prefix(GUIHand __instance)
            {
                if (!done && __instance.activeTarget && CraftData.GetTechType(__instance.activeTarget) == TechType.Stalker)
                {
                    ErrorMessage.AddDebug("UpdateActiveTarget 0 " + __instance.activeTarget.name);
                    Main.Log("UpdateActiveTarget 0 " + __instance.activeTarget.name);
                }


                PlayerTool tool = __instance.GetTool();
                if (tool != null && tool.GetComponent<PropulsionCannon>() != null && tool.GetComponent<PropulsionCannon>().IsGrabbingObject())
                {
                    __instance.activeTarget = tool.GetComponent<PropulsionCannon>().GetNearbyGrabbedObject();
                    __instance.suppressTooltip = true;
                }
                else if (tool != null && tool.DoesOverrideHand() || !Targeting.GetTarget(Player.main.gameObject, 2f, out __instance.activeTarget, out __instance.activeHitDistance))
                {
                    __instance.activeTarget = null;
                    __instance.activeHitDistance = 0.0f;
                }
                else if (__instance.activeTarget.layer == LayerID.NotUseable)
                {
                    ErrorMessage.AddDebug("layer NotUseable");
                    __instance.activeTarget = null;
                }
                else
                {
                    if (!done && __instance.activeTarget && CraftData.GetTechType(__instance.activeTarget) == TechType.Stalker)
                        Main.Log("UpdateActiveTarget 4 " + __instance.activeTarget.name);

                    IHandTarget handTarget = null;
                    for (Transform transform = __instance.activeTarget.transform; transform != null; transform = transform.parent)
                    {
                        handTarget = transform.GetComponent<IHandTarget>();
                        if (handTarget != null)
                        {
                            if (!done && __instance.activeTarget && CraftData.GetTechType(__instance.activeTarget) == TechType.Stalker)
                                Main.Log("UpdateActiveTarget 5 " + __instance.activeTarget.name);

                            __instance.activeTarget = transform.gameObject;
                            break;
                        }
                    }
                    if (handTarget == null)
                    {
                        if (!done && __instance.activeTarget && CraftData.GetTechType(__instance.activeTarget) == TechType.Stalker)
                            Main.Log("UpdateActiveTarget 6 " + __instance.activeTarget.name);

                        switch (CraftData.GetHarvestTypeFromTech(CraftData.GetTechType(__instance.activeTarget)))
                        {
                            case HarvestType.None:
                                ErrorMessage.AddDebug("HarvestType.None");
                                __instance.activeTarget = null;
                                break;
                            case HarvestType.Pick:
                                ErrorMessage.AddDebug("HarvestType.Pick");
                                if (Utils.FindAncestorWithComponent<Pickupable>(__instance.activeTarget) == null)
                                {
                                    LargeWorldEntity ancestorWithComponent = Utils.FindAncestorWithComponent<LargeWorldEntity>(__instance.activeTarget);
                                    ancestorWithComponent.gameObject.AddComponent<Pickupable>();
                                    ancestorWithComponent.gameObject.AddComponent<WorldForces>().useRigidbody = ancestorWithComponent.GetComponent<Rigidbody>();
                                    break;
                                }
                                break;
                        }
                    }
                }
                if (!done && __instance.activeTarget && CraftData.GetTechType(__instance.activeTarget) == TechType.Stalker)
                {
                    done = true;
                    Main.Log("UpdateActiveTarget 11 " + __instance.activeTarget.name);
                }
                if (!IntroVignette.isIntroActive)
                    return false;
                __instance.activeTarget = __instance.FilterIntroTarget(__instance.activeTarget);
                return false;
            }
        }

        //[HarmonyPatch(typeof(GUIHand), "OnUpdate")]
        class GUIHand_Update_Patch
        {
            public static bool Prefix(GUIHand __instance)
            {
                __instance.usedToolThisFrame = false;
                __instance.usedAltAttackThisFrame = false;
                __instance.suppressTooltip = false;
                if (__instance.player.IsFreeToInteract() && AvatarInputHandler.main.IsEnabled())
                {
                    string text1 = string.Empty;
                    PlayerTool tool = __instance.GetTool();
                    EnergyMixin energyMixin = null;
                    if (tool != null)
                    {
                        // ProfilingUtils.BeginSample("GUIHandUpdate-GetCustomUseText");
                        text1 = tool.GetCustomUseText();
                        // ProfilingUtils.EndSample();
                        energyMixin = tool.GetComponent<EnergyMixin>();
                    }
                    if (energyMixin != null && energyMixin.allowBatteryReplacement)
                    {
                        // ProfilingUtils.BeginSample("GUIHandUpdate-EnergyMixinAllowBattery");
                        int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
                        if (__instance.cachedTextEnergyScalar != num)
                        {
                            __instance.cachedEnergyHudText = num > 0 ? Language.main.GetFormat<float>("PowerPercent", energyMixin.GetEnergyScalar()) : LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
                            __instance.cachedTextEnergyScalar = num;
                        }
                        HandReticle.main.SetUseTextRaw(text1, __instance.cachedEnergyHudText);
                        // ProfilingUtils.EndSample();
                    }
                    else if (!string.IsNullOrEmpty(text1))
                    {
                        HandReticle.main.SetUseTextRaw(text1, string.Empty);
                        //ErrorMessage.AddDebug("OnUpdate " + text1);
                    }
                    if (__instance.grabMode == GUIHand.GrabMode.None)
                    {
                        __instance.UpdateActiveTarget();
                        //ErrorMessage.AddDebug("OnUpdate " );
                    }
                    if (__instance.activeTarget)
                        //ErrorMessage.AddDebug("OnUpdate 2 " + __instance.activeTarget.name);
                        HandReticle.main.SetTargetDistance(__instance.activeHitDistance);

                    if (__instance.activeTarget != null && !__instance.suppressTooltip)
                    {
                        TechType techType = CraftData.GetTechType(__instance.activeTarget);
                        if (techType != TechType.None)
                        {
                            string name = Language.main.Get(techType);
                            //HandReticle.main.SetInteractInfo(techType.AsString());
                            HandReticle.main.SetInteractText(name, string.Empty);
                            //ErrorMessage.AddDebug("OnUpdate "+ techType.AsString());
                        }


                        // ProfilingUtils.BeginSample("GUIHandUpdate-SendToActiveTarget");
                        GUIHand.Send(__instance.activeTarget, HandTargetEventType.Hover, __instance);
                        // ProfilingUtils.EndSample();
                    }
                    // ProfilingUtils.EndSample();
                    bool flag1 = GameInput.GetButtonDown(GameInput.Button.LeftHand);
                    bool buttonHeld1 = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
                    bool buttonUp1 = GameInput.GetButtonUp(GameInput.Button.LeftHand);
                    bool rightHandDown = GameInput.GetButtonDown(GameInput.Button.RightHand);
                    bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.RightHand);
                    bool buttonUp2 = GameInput.GetButtonUp(GameInput.Button.RightHand);
                    bool buttonDown1 = GameInput.GetButtonDown(GameInput.Button.Reload);
                    bool buttonDown2 = GameInput.GetButtonDown(GameInput.Button.Exit);
                    bool buttonDown3 = GameInput.GetButtonDown(GameInput.Button.AltTool);
                    bool buttonHeld3 = GameInput.GetButtonHeld(GameInput.Button.AltTool);
                    bool buttonUp3 = GameInput.GetButtonUp(GameInput.Button.AltTool);
                    // ProfilingUtils.BeginSample("GUIHandUpdate-PDAScannerGetTarget");
                    PDAScanner.UpdateTarget(8f, buttonDown3 | buttonHeld3);
                    // ProfilingUtils.EndSample();
                    // ProfilingUtils.BeginSample("GUIHandUpdate-PDAScanner");
                    if (PDAScanner.scanTarget.isValid && Inventory.main.container.Contains(TechType.Scanner) && (PDAScanner.CanScan() == PDAScanner.Result.Scan && !PDAScanner.scanTarget.isPlayer))
                        uGUI_ScannerIcon.main.Show();
                    // ProfilingUtils.EndSample();
                    if (tool != null)
                    {
                        //bool flag3;
                        //bool flag4;
                        if (rightHandDown)
                        {
                            if (tool.OnRightHandDown())
                            {
                                __instance.usedToolThisFrame = true;
                                tool.OnToolActionStart();
                                rightHandDown = false;
                                //flag3 = false;
                                //flag4 = false;
                            }
                        }
                        else if (buttonHeld2)
                        {
                            if (tool.OnRightHandHeld())
                            {
                                rightHandDown = false;
                                //flag3 = false;
                            }
                        }
                        //else if (buttonUp2 && tool.OnRightHandUp())
                        //    flag4 = false;
                        //bool flag5;
                        //bool flag6;
                        if (flag1)
                        {
                            if (tool.OnLeftHandDown())
                            {
                                tool.OnToolActionStart();
                                flag1 = false;
                                //flag5 = false;
                                //flag6 = false;
                            }
                        }
                        else if (buttonHeld1)
                        {
                            if (tool.OnLeftHandHeld())
                            {
                                flag1 = false;
                                //flag5 = false;
                            }
                        }
                        //else if (buttonUp1 && tool.OnLeftHandUp())
                        //    flag6 = false;
                        //bool flag7;
                        //bool flag8;
                        //bool flag9;
                        if (buttonDown3)
                        {
                            if (tool.OnAltDown())
                            {
                                __instance.usedAltAttackThisFrame = true;
                                tool.OnToolActionStart();
                                //flag7 = false;
                                //flag8 = false;
                                //flag9 = false;
                            }
                        }
                        else if (buttonHeld3)
                        {
                            if (tool.OnAltHeld())
                            {
                                //flag7 = false;
                                //flag8 = false;
                            }
                        }
                        //else if (buttonUp3 && tool.OnAltUp())
                        //    flag9 = false;
                        //if (buttonDown1 && tool.OnReloadDown())
                        //    ;
                        //if (buttonDown2 && tool.OnExitDown())
                        //    ;
                    }
                    if (tool == null & rightHandDown)
                        Inventory.main.DropHeldItem(true);
                    if (__instance.activeTarget != null && flag1)
                        GUIHand.Send(__instance.activeTarget, HandTargetEventType.Click, __instance);
                }
                // ProfilingUtils.BeginSample("GUIHandUpdate-OpenPDA");
                if (AvatarInputHandler.main.IsEnabled() && GameInput.GetButtonDown(GameInput.Button.PDA) && !IntroVignette.isIntroActive)
                    __instance.player.GetPDA().Open();
                // ProfilingUtils.EndSample();
                if (!__instance.targetDebug || !__instance.activeTarget)
                    return false;
                HandReticle.main.SetInteractTextRaw(string.Format("activeTarget: {0}", __instance.activeTarget.name), string.Empty);
                return false;
            }
        }


    }
}
