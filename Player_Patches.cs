﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;
using HarmonyLib;
using ProtoBuf;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Player_Patches
    {
        public static float exitWaterOffset = 0.8f; // 0.8f
        public static float crushPeriod = 3f;
        public static float healTime = 0f;

        public static void DisableExosuitClawArmScan()
        {
            if (PDAScanner.mapping.ContainsKey(TechType.ExosuitClawArmFragment))
            {
                //AddDebug("DisableExosuitClawArmScan");
                PDAScanner.mapping.Remove(TechType.ExosuitClawArmFragment);
            }
        }


        [HarmonyPatch(typeof(Player))]
        class Player_Patch
        {
            private static float crushTime = 0f;

            static IEnumerator Test()
            {
                AddDebug("Test start ");
                //Main.Log("Test start ");
                while (!uGUI.main.hud.active)
                    yield return null;
                AddDebug("Test end ");
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(Player __instance)
            {
                Main.survival = __instance.GetComponent<Survival>();
                if (Main.config.cantScanExosuitClawArm)
                    DisableExosuitClawArmScan();
            }

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            static void UpdatePostfix(Player __instance)
            {
                if (__instance.currentMountedVehicle)
                {
                    Vehicle_patch.UpdateLights();
                }
                else if (__instance.currentSub && __instance.currentSub.isCyclops && __instance.isPiloting)
                    Vehicle_patch.UpdateLights();
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                if (Main.config.crushDamageMult > 0f && Crush_Damage.crushInterval + crushTime < Time.time)
                {
                    crushTime = Time.time;
                    Crush_Damage.CrushDamage();
                }
                if (Main.config.medKitHPtoHeal > 0 && Time.time > healTime)
                //if (Main.config.medKitHPtoHeal > 0 && DayNightCycle.main.timePassedAsFloat > healTime)
                { // not checking savegame slot
                    healTime = Time.time + 1.0f;
                    //healTime = DayNightCycle.main.timePassedAsFloat + 1f;
                    __instance.liveMixin.AddHealth(Main.config.medKitHPperSecond);
                    //AddDebug("AddHealth " + Main.config.medKitHPperSecond);
                    Main.config.medKitHPtoHeal -= Main.config.medKitHPperSecond;
                    if (Main.config.medKitHPtoHeal < 0)
                        Main.config.medKitHPtoHeal = 0;
                }

                if (!GameModeUtils.RequiresSurvival() || Main.survival.freezeStats || !Main.loadingDone)
                    return;

                if (Food_Patch.hungerUpdateTime > Time.time)
                    return;

                if (Main.config.newHungerSystem)
                {
                    Food_Patch.UpdateStats(Main.survival);
                    //__instance.Invoke("UpdateHunger", updateHungerInterval);
                    //AddDebug("updateHungerInterval " + updateHungerInterval);
                }
                else
                    Main.survival.UpdateHunger();
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetDepthClass")]
            public static bool GetDepthClassPrefix(Player __instance, ref Ocean.DepthClass __result)
            {
                //AddDebug("GetDepthClass");
                Ocean.DepthClass depthClass = Ocean.DepthClass.Surface;
                if (!Main.loadingDone)
                { // avoid null reference exception when loading game inside cyclops
                  //__result = depthClass;
                  //return false;
                }
                CrushDamage crushDamage = null;
                if (__instance.currentSub != null && !__instance.currentSub.isBase || __instance.mode == Player.Mode.LockedPiloting)
                    crushDamage = __instance.currentSub == null ? __instance.gameObject.GetComponentInParent<CrushDamage>() : __instance.currentSub.gameObject.GetComponent<CrushDamage>();
                if (crushDamage != null)
                {
                    depthClass = crushDamage.GetDepthClass();
                    __instance.crushDepth = crushDamage.crushDepth;
                }
                else
                {
                    __instance.crushDepth = Main.config.crushDepth;
                    float depth = Ocean.GetDepthOf(__instance.gameObject);
                    if (depth > __instance.crushDepth)
                        depthClass = Ocean.DepthClass.Crush;
                    else if (depth > __instance.crushDepth * .5f)
                        depthClass = Ocean.DepthClass.Unsafe;
                    else if (depth > __instance.GetSurfaceDepth())
                        depthClass = Ocean.DepthClass.Safe;
                }
                __result = depthClass;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("CanBeAttacked")]
            public static bool Prefix(Player __instance, ref bool __result)
            {
                //AddDebug("AggressiveWhenSeeTarget start " + __instance.myTechType + " " + __instance.maxSearchRings);
                //__result = !__instance.IsInsideWalkable() && !__instance.justSpawned && !GameModeUtils.IsInvisible() && !Player.main.precursorOutOfWater && !PrecursorMoonPoolTrigger.inMoonpool;
                __result = Main.config.aggrMult > 0f && !__instance.justSpawned && !__instance.IsInsideWalkable() && !GameModeUtils.IsInvisible();
                return false;
            }
        }

        //[HarmonyPatch(typeof(CrushDamage), "GetDepth")]
        class CrushDamage_GetDepth_Patch
        {
            public static void Prefix(CrushDamage __instance)
            {
                if (__instance.depthCache == null)
                {
                    AddDebug("__instance.depthCache == null");
                }
                else
                    AddDebug("depthCache" + __instance.depthCache.Get());
            }
        }

        [HarmonyPatch(typeof(Inventory), "LoseItems")]
        class Inventory_LoseItems_Patch
        {
            public static bool Prefix(Inventory __instance)
            {
                //AddDebug("LoseItems");
                if (Main.config.dropItemsOnDeath == Config.DropItemsOnDeath.Drop_everything)
                {
                    List<InventoryItem> inventoryItemList = new List<InventoryItem>();
                    foreach (InventoryItem inventoryItem in Inventory.main.container)
                    {
                        inventoryItemList.Add(inventoryItem);
                    }
                    foreach (InventoryItem inventoryItem in (IItemsContainer)Inventory.main.equipment)
                    {
                        //AddDebug("equipment " + inventoryItem.item.GetTechName());
                        inventoryItemList.Add(inventoryItem);
                    }
                    foreach (InventoryItem item in inventoryItemList)
                    {
                        //AddDebug("DROP " + item.item.GetTechName());
                        __instance.InternalDropItem(item.item, false);
                    }
                    return false;
                }
                else if (Main.config.dropItemsOnDeath == Config.DropItemsOnDeath.Do_not_drop_anything)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FootstepSounds), "OnStep")]
        class FootstepSounds_OnStep_Patch
        {
            static bool Prefix(FootstepSounds __instance, Transform xform)
            {
                if (!Main.config.fixFootstepSound)
                    return true;

                if (!__instance.ShouldPlayStepSounds())
                    return false;

                //FMODAsset test = new FMODAsset() { path = "event:/player/footstep_rocket" };
                FMODAsset asset;
                float volume;
                if (Player.main.precursorOutOfWater)
                {
                    asset = __instance.precursorInteriorSound;
                    volume = __instance.precursorInteriorSoundVolume;
                }
                else if (__instance.groundMoveable.GetGroundSurfaceType() == VFXSurfaceTypes.metal || Player.main.IsInside() || Player.main.GetBiomeString() == FootstepSounds.crashedShip)
                {
                    asset = __instance.metalSound;
                    volume = __instance.metalSoundVolume;
                }
                else
                {
                    asset = __instance.landSound;
                    volume = __instance.landSoundVolume;
                }
                //asset = test;
                EventInstance evt = FMODUWE.GetEvent(asset);
                if (!evt.isValid())
                    return false;

                if (FMODUWE.IsInvalidParameterId(__instance.fmodIndexSpeed))
                    __instance.fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(evt, "speed");
                //AddDebug("Velocity.magnitude " + __instance.groundMoveable.GetVelocity().magnitude);
                ATTRIBUTES_3D attributes = xform.To3DAttributes();
                evt.set3DAttributes(attributes);
                float velMag = __instance.groundMoveable.GetVelocity().magnitude;
                evt.setParameterValueByIndex(__instance.fmodIndexSpeed, velMag);
                evt.setVolume(volume);
                //AddDebug("FootstepSounds onstep " + asset.name);
                if (asset != __instance.landSound)
                {
                    //AddDebug("FIX");
                    evt.setParameterValueByIndex(__instance.fmodIndexSpeed, 7f);
                    if (velMag < 6f)
                        evt.setVolume(.3f);
                }
                evt.start();
                evt.release();
                return false;
            }
        }

        [HarmonyPatch(typeof(VoiceNotification), "Play", new Type[1] { typeof(object[]) })]
        class VoiceNotification_Play_Patch
        {
            public static bool Prefix(VoiceNotification __instance)
            {
                //AddDebug("VoiceNotification Play");
                if (!Main.loadingDone)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(SoundQueue), "PlayQueued", new Type[2] { typeof(string), typeof(string) })]
        class SoundQueue_PlayQueued_Patch
        {
            public static bool Prefix(SoundQueue __instance, string sound)
            {
                //AddDebug(" PlayQueued  " + sound);
                if (!Main.loadingDone)
                    return false;

                return true;
            }
        }

    }
}
