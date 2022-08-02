using System;
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
                //Main.Message("DisableExosuitClawArmScan");
                PDAScanner.mapping.Remove(TechType.ExosuitClawArmFragment);
            }
        }

        //[HarmonyPatch(typeof(Survival), nameof(Survival.Reset))]
        internal class Survival_Reset_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(Survival __instance)
            {
                //survival = Player.main.GetComponent<Survival>();
                //liveMixin = Player.main.GetComponent<LiveMixin>();
                //Main.Log("1.40129846432482E-45  " + (int)1.40129846432482E-45);
                //Main.Message("Survival_Reset_Patch "); 
                //__instance.food = 11f;
                //__instance.water = 11f;
                //Player.main.liveMixin.health -= 40f;
            }
        }

        [HarmonyPatch(typeof(Player))]
        class Player_Patch
        {
            private static float crushTime = 0f;

            static IEnumerator Test()
            {
                //AddDebug("Test start ");
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
                //IngameMenuHandler.RegisterOnSaveEvent(config.Save);
                Main.guiHand = __instance.GetComponent<GUIHand>();
                Main.pda = __instance.GetPDA();
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

                if (!GameModeUtils.RequiresSurvival() || Main.survival.freezeStats || !Main.loadingDone)
                    return;

                if (Main.config.medKitHPtoHeal > 0 && Time.time > healTime)
                //if (Main.config.medKitHPtoHeal > 0 && DayNightCycle.main.timePassedAsFloat > healTime)
                { // not checking savegame slot
                    healTime = Time.time + 1.0f;
                    //healTime = DayNightCycle.main.timePassedAsFloat + 1f;
                    __instance.liveMixin.AddHealth(Main.config.medKitHPperSecond);
                    Main.config.medKitHPtoHeal -= Main.config.medKitHPperSecond;
                    if (Main.config.medKitHPtoHeal < 0)
                        Main.config.medKitHPtoHeal = 0;
                }

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
                    __result = depthClass;
                    return false;
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
                    float depth = Ocean.main.GetDepthOf(__instance.gameObject);
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
                __result = !__instance.IsInsideWalkable() && !__instance.justSpawned && !GameModeUtils.IsInvisible() && Main.config.aggrMult > 0f;
                return false;
            }
        }

        //[HarmonyPatch(typeof(CrushDamage), "GetDepth")]
        internal class CrushDamage_GetDepth_Patch
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

        [HarmonyPatch(typeof(MainCameraControl), "Awake")]
        internal class MainCameraControl_Awake_Patch
        {
            public static void Postfix(MainCameraControl __instance)
            {
                if (Main.config.playerCamRot != -1f)
                    __instance.rotationX = Main.config.playerCamRot;
            }
        }

        [HarmonyPatch(typeof(Inventory), "OnProtoDeserialize")]
        internal class Inventory_OnProtoDeserialize_Patch
        {
            public static void Postfix(Inventory __instance)
            {
                if (Main.config.activeSlot == -1)
                    __instance.quickSlots.Deselect();
                else
                    __instance.quickSlots.SelectImmediate(Main.config.activeSlot);
            }
        }

        [HarmonyPatch(typeof(Inventory), "LoseItems")]
        internal class Inventory_LoseItems_Patch
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
                if (__instance.groundMoveable.GetGroundSurfaceType() == VFXSurfaceTypes.metal || Player.main.IsInside() || Player.main.GetBiomeString() == FootstepSounds.crashedShip)
                    asset = __instance.metalSound;
                else if (Player.main.precursorOutOfWater)
                    asset = __instance.precursorInteriorSound;
                else
                    asset = __instance.landSound;

                //asset = test;
                EventInstance evt = FMODUWE.GetEvent(asset);

                if (!evt.isValid())
                    return false;
                if (__instance.fmodIndexSpeed < 0)
                    __instance.fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(evt, "speed");
                //AddDebug("Velocity.magnitude " + __instance.groundMoveable.GetVelocity().magnitude);
                ATTRIBUTES_3D attributes = xform.To3DAttributes();
                evt.set3DAttributes(attributes);
                float velMag = __instance.groundMoveable.GetVelocity().magnitude;

                evt.setParameterValueByIndex(__instance.fmodIndexSpeed, velMag);
                evt.setVolume(1f);
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
