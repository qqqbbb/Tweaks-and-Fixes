using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Oxygen_
    {

        [HarmonyPatch(typeof(IntermittentInstantiate), "OnEnable")]
        class IntermittentInstantiate_OnEnable_Patch
        {
            static float baseIntervalTime;
            static float randomIntervalTime;

            public static void Postfix(IntermittentInstantiate __instance)
            {
                if (__instance.GetComponent<BrainCoral>())
                {
                    //AddDebug("baseIntervalTime " + __instance.baseIntervalTime);
                    //AddDebug("randomIntervalTime " + __instance.randomIntervalTime);
                    //AddDebug("numToInstantiate " + __instance.numToInstantiate);
                    if (baseIntervalTime == 0)
                    {
                        __instance.baseIntervalTime /= __instance.numToInstantiate;
                        baseIntervalTime = __instance.baseIntervalTime;
                    }
                    else
                        __instance.baseIntervalTime = baseIntervalTime;

                    if (randomIntervalTime == 0)
                    {
                        __instance.randomIntervalTime /= __instance.numToInstantiate;
                        randomIntervalTime = __instance.randomIntervalTime;
                    }
                    else
                        __instance.randomIntervalTime = randomIntervalTime;

                    __instance.numToInstantiate = 1;
                    //extraBreathPeriod = __instance.baseIntervalTime + __instance.randomIntervalTime;

                }
            }
        }

        [HarmonyPatch(typeof(Player))]
        class Player_Patch
        {
            private const float breathPeriodMax = 3f;

            [HarmonyPostfix, HarmonyPatch("GetOxygenPerBreath")]
            static void GetOxygenPerBreathPostfix(Player __instance, ref float __result, float breathingInterval, int depthClass)
            {// vanilla script returns wrong value at depth 200-100
                if (GameModeUtils.RequiresOxygen())
                    __result = ConfigMenu.oxygenPerBreath.Value;
                else
                    __result = 0f;
                //AddDebug("GetOxygenPerBreath breathingInterval " + breathingInterval);
                //AddDebug("GetOxygenPerBreath  " + __result);
                //return false;
            }
            [HarmonyPrefix, HarmonyPatch("GetBreathPeriod")]
            static bool GetBreathPeriodPrefix(Player __instance, ref float __result)
            {
                //AddDebug("depthLevel " + (int)__instance.depthLevel);
                //AddDebug("depthOf " + (int)Ocean.main.GetDepthOf(__instance.gameObject);
                if (!ConfigMenu.realOxygenCons.Value)
                    return true;

                if (__instance.currentSub || __instance.mode == Player.Mode.Piloting || __instance.mode == Player.Mode.LockedPiloting || __instance.currentWaterPark || Inventory.main.equipment.GetCount(TechType.Rebreather) > 0)
                {
                    //AddDebug("safe ox consump " );
                    __result = breathPeriodMax;
                    return false;
                }
                float depth = Mathf.Abs(__instance.depthLevel);
                float mult = 1.5f / ConfigMenu.crushDepth.Value;
                __result = breathPeriodMax - depth * mult;
                // fix negative __result when depth is 2x deeper than crushDepth
                __result = Mathf.Clamp(__result, 0.1f, breathPeriodMax);
                return false;
            }
        }


        //[HarmonyPatch(typeof(OxygenManager), "RemoveOxygen")]
        class OxygenManager_RemoveOxygen_Patch
        {
            public static void Prefix(OxygenManager __instance, ref float amountToRemove)
            {
                //amountToRemove *= Main.config.oxygenMult;
                //if (extraBreathPeriod > 0)
                //{
                //extraBreathPeriod -= extraBreathPeriod;
                //extraBreathPeriod -= extraBreathPeriod;
                //Mathf.Clamp(extraBreathPeriod, 0f, extraBreathPeriod);
                //AddDebug("RemoveOxygen extra " );
                //}
                //AddDebug("RemoveOxygen " + amountToRemove);
            }
        }
        //[HarmonyPatch(typeof(OxygenManager), "Update")]
        class OxygenManager_Update_Patch
        {
            public static void Prefix(OxygenManager __instance)
            {
                //amountToRemove *= Main.config.oxygenMult;
                if (__instance.GetComponent<Player>())
                {
                    //AddDebug("Player OxygenManager ");
                }
                else
                {
                    //AddDebug("Oxygen Available " + __instance.GetOxygenAvailable());
                    //AddDebug("Oxygen Capacity " + __instance.GetOxygenCapacity());
                }

            }
        }

        /*
        public class OxygenArea_Mono : MonoBehaviour
        {
            void OnTriggerExit(Collider other)
            {
                if (Main.refillOxygenTankLoaded && other.gameObject.FindAncestor<Player>() == Player.main)
                {
                    Main.canBreathe = false;
                    //AddDebug("OnTriggerExit ");
                }
            }
        }

        static void SpawnBubble(Vector3 position)
        {
            //UWE.PrefabDatabase.TryGetPrefabForFilename("WorldEntities/VFX/x_BubblesPlane_01", out bubble);
            //bubble = Resources.Load<GameObject>("PrefabInstance/Bubble");
            if (bubble)
            {
                //AddDebug("bubble ");
                //GameObject ent = UWE.Utils.InstantiateWrap(bubble, position, Quaternion.identity);
                UnityEngine.Object.Instantiate<GameObject>(bubble, position, Quaternion.identity);
            }
        }

        [HarmonyPatch(typeof(OxygenPipe), "UpdatePipe")]
        class OxygenPipe_UpdatePipe_Patch
        {
            public static void Postfix(OxygenPipe __instance)
            {
                //AddDebug("UpdatePipe " );
                if (Main.refillOxygenTankLoaded && __instance.oxygenProvider.activeInHierarchy)
                {
                    __instance.oxygenProvider.EnsureComponent<OxygenArea_Mono>();
                }
            }
        }

        [HarmonyPatch(typeof(Bubble), "Pop")]
        class Bubble_Pop_Patch
        {
            public static void Postfix(Bubble __instance, bool hitPlayer)
            {
                if (Main.refillOxygenTankLoaded && hitPlayer)
                {
                    //extraBreathPeriod += extraBreathPeriodDefault;
                    bubbleEndTime = Time.time + extraBreathPeriod;
                    //bubbleEndTime = __instance.oxygenSeconds + extraBreathPeriod;
                    Main.canBreathe = true;
                    //AddDebug("Bubble Pop extraBreathPeriod " + extraBreathPeriod);
                    //AddDebug("Bubble Pop bubbleEndTime " + bubbleEndTime);
                }
            }
        }

        [HarmonyPatch(typeof(OxygenArea), "OnTriggerStay")]
        class OxygenArea_OnTriggerStay_Patch
        { // OnTriggerExit does not fire when you pick up pipe
            public static void Postfix(OxygenArea __instance, Collider other)
            {
                if (Main.refillOxygenTankLoaded && other.gameObject.FindAncestor<Player>() == Player.main)
                {
                    Main.canBreathe = true;
                    //AddDebug("OnTriggerStay ");
                }
            }
        }

        [HarmonyPatch(typeof(OxygenPipe), "OnPickedUp")]
        class OxygenPipe_OnPickedUp_Patch
        { 
            public static void Postfix(OxygenPipe __instance)
            {
                    Main.canBreathe = false;
                    //AddDebug("OnPickedUp ");
            }
        }

        [HarmonyPatch(typeof(Player), "CanBreathe")]
        class Player_CanBreathe_Patch
        {
            public static void Postfix(Player __instance, ref bool __result)
            {

                if (Main.refillOxygenTankLoaded && Main.canBreathe)
                {
                    if (bubbleEndTime > 0 && Time.time > bubbleEndTime)
                    {
                        bubbleEndTime = 0f;
                        Main.canBreathe = false;
                        //AddDebug("CanBreathe bubbleEndTime 0");
                    }
                    __result = Main.canBreathe;
                }
            }
        }

        [HarmonyPatch(typeof(LiveMixin), "TakeDamage")]
        class LiveMixin_TakeDamage_Patch
        {
            public static void Postfix(LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (originalDamage > 1f && __instance.GetComponent<BrainCoral>())
                {
                    //AddDebug("BrainCoral " );
                    IntermittentInstantiate ii = __instance.GetComponent<IntermittentInstantiate>();
                    //GameObject ent = UWE.Utils.InstantiateWrap(ii.prefab, position, Quaternion.identity);
                    UnityEngine.Object.Instantiate<GameObject>(ii.prefab, position, Quaternion.identity);
                    //if (ii.registerToWorldStreamer && LargeWorldStreamer.main)
                    //    LargeWorldStreamer.main.cellManager.RegisterEntity(ent);
                    //if (ii.onCreateSound)
                    //    Utils.PlayEnvSound(ii.onCreateSound, ent.transform.position, 1f);
                }
            }
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {// cant use Resources.Load , bubble prefab is not in resources folder
            public static void Postfix()
            {
                GameObject brainCoral = CraftData.GetPrefabForTechType(TechType.PurpleBrainCoral);
                brainCoral = Utils.CreatePrefab(brainCoral);
                IntermittentInstantiate ii = brainCoral.GetComponent<IntermittentInstantiate>();
                bubble = ii.prefab;
                UnityEngine.Object.Destroy(brainCoral);
                //LargeWorldEntity.Register(prefab);

            }
        }

        [HarmonyPatch(typeof(LiveMixin), "Kill")]
        class LiveMixin_Kill_Patch
        {
            public static void Postfix(LiveMixin __instance)
            {
                if (__instance.GetComponent<Bladderfish>() || __instance.GetComponent<Floater>())
                {
                    //AddDebug("Kill ");
                    SpawnBubble(__instance.gameObject.transform.position);
                }

            }
        }
        /*/
    }
}
