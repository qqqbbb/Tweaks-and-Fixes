﻿using FMOD;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Player_Patches
    {
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

            //[HarmonyPrefix]
            //[HarmonyPatch("MovePlayerToRespawnPoint")]
            static void MovePlayerToRespawnPointPostfix(Player __instance)
            {
                AddDebug("MovePlayerToRespawnPoint lastValidSub " + __instance.lastValidSub);
                if (__instance.lastValidSub)
                {
                    AddDebug("MovePlayerToRespawnPoint CheckSubValid " + __instance.CheckSubValid(__instance.lastValidSub));
                }
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("CheckSubValid")]
            static bool CheckSubValidPrefix(Player __instance, SubRoot sub, ref bool __result)
            {
                //AddDebug("CheckSubValid lastValidSub " + __instance.lastValidSub);
                bool valid = false;
                if (sub != null)
                {
                    bool isAlive = true;
                    LiveMixin liveMixin = sub.GetComponent<LiveMixin>();
                    if (liveMixin != null)
                        isAlive = liveMixin.IsAlive();
                    AddDebug("CheckSubValid IsAlive " + isAlive);
                    AddDebug("CheckSubValid GetLeakAmount " + sub.GetLeakAmount());
                    valid = isAlive && sub.GetLeakAmount() <= 0.2;
                }
                __result = valid;
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void Finalizer(Player __instance)
            { // using finalizer bc some mod throws exception in prefix
                //__instance.StartCoroutine(Test());
                crushTime = 0;
                Main.survival = __instance.GetComponent<Survival>();
            }

            [HarmonyPostfix]
            [HarmonyPatch("Update")]
            static void UpdatePostfix(Player __instance)
            {
                if (!Main.gameLoaded)
                    return;

                if (__instance.currentMountedVehicle)
                    Light_Control.UpdateLights();
                else if (__instance.currentSub && __instance.currentSub.isCyclops && __instance.isPiloting)
                    Light_Control.UpdateLights();
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                if (ConfigMenu.crushDamage.Value > 0f && Crush_Damage_.crushInterval + crushTime < Time.time)
                {
                    crushTime = Time.time;
                    Crush_Damage_.CrushDamagePlayer();
                }
                if (Main.configMain.medKitHPtoHeal > 0 && Time.time > healTime)
                { // not checking savegame slot
                    healTime = Time.time + 1.0f;
                    //healTime = DayNightCycle.main.timePassedAsFloat + 1f;
                    __instance.liveMixin.AddHealth(ConfigToEdit.medKitHPperSecond.Value);
                    //AddDebug("AddHealth " + Main.config.medKitHPperSecond);
                    Main.configMain.medKitHPtoHeal -= ConfigToEdit.medKitHPperSecond.Value;
                    if (Main.configMain.medKitHPtoHeal < 0)
                        Main.configMain.medKitHPtoHeal = 0;
                }
                //if (!GameModeUtils.RequiresSurvival() || Main.survival.freezeStats)
                //    return;

                //if (Survival_.hungerUpdateTime > Time.time)
                //    return;


                //__instance.Invoke("UpdateHunger", updateHungerInterval);
                //AddDebug("updateHungerInterval " + updateHungerInterval);
                //}
                //else
                //    Main.survival.UpdateHunger();
            }


            [HarmonyPrefix]
            [HarmonyPatch("GetDepthClass")]
            public static bool GetDepthClassPrefix(Player __instance, ref Ocean.DepthClass __result)
            {
                //AddDebug("GetDepthClass");
                Ocean.DepthClass depthClass = Ocean.DepthClass.Surface;
                if (!Main.gameLoaded)
                { // avoid null reference exception when loading game inside cyclops
                  //__result = depthClass;
                  //  return false;
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
                    __instance.crushDepth = ConfigMenu.crushDepth.Value;
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

            [HarmonyPostfix, HarmonyPatch("CanBeAttacked")]
            public static void CanBeAttackedPostfix(Player __instance, ref bool __result)
            {
                if (ConfigMenu.aggrMult.Value == 0)
                    __result = false;
            }

        }

        [HarmonyPatch(typeof(Inventory), "LoseItems")]
        class Inventory_LoseItems_Patch
        {
            public static bool Prefix(Inventory __instance)
            {
                //AddDebug("LoseItems");
                if (ConfigMenu.dropItemsOnDeath.Value == ConfigMenu.DropItemsOnDeath.Drop_everything)
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
                else if (ConfigMenu.dropItemsOnDeath.Value == ConfigMenu.DropItemsOnDeath.Do_not_drop_anything)
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
                if (!ConfigToEdit.disableFootstepClickSound.Value)
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
                return Main.gameLoaded;
            }
        }

        [HarmonyPatch(typeof(SoundQueue), "PlayQueued", new Type[2] { typeof(string), typeof(string) })]
        class SoundQueue_PlayQueued_Patch
        {
            public static bool Prefix(SoundQueue __instance, string sound)
            {
                //AddDebug(" PlayQueued  " + sound);
                return Main.gameLoaded;
            }
        }

        [HarmonyPatch(typeof(PlayerBreathBubbles), "MakeBubbles")]
        class PlayerBreathBubbles_MakeBubbles_Patch
        {
            public static bool Prefix(PlayerBreathBubbles __instance)
            {
                if (ConfigToEdit.playerBreathBubbles.Value && ConfigToEdit.playerBreathBubblesSoundFX.Value)
                    return true;

                if (!__instance.enabled)
                    return false;

                if (ConfigToEdit.playerBreathBubblesSoundFX.Value)
                    __instance.bubbleSound.Play();

                if (ConfigToEdit.playerBreathBubbles.Value == false)
                    return false;

                __instance.bubbles = UnityEngine.Object.Instantiate(__instance.bubblesPrefab.gameObject);
                if (!__instance.bubbles)
                    return false;

                Transform transform = __instance.bubbles.transform;
                transform.SetParent(__instance.anchor, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                ParticleSystem component = __instance.bubbles.GetComponent<ParticleSystem>();
                component.Play();
                UnityEngine.Object.Destroy(__instance.bubbles, component.duration + component.startLifetime);
                return false;
            }
        }


    }
}
