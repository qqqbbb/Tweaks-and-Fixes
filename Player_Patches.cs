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

namespace Tweaks_Fixes
{
    class Player_Patches
    {
        //static Survival survival;
        //static LiveMixin liveMixin;
        public static GUIHand gUIHand;
        public static float exitWaterOffset = 0.8f; // 0.8f
        public static float crushPeriod = 3f;

        public static void DisableExosuitClawArmScan()
        {
            if (PDAScanner.mapping.ContainsKey(TechType.ExosuitClawArmFragment))
            {
                //Main.Message("DisableExosuitClawArmScan");
                PDAScanner.mapping.Remove(TechType.ExosuitClawArmFragment);
            }
        }

        [HarmonyPatch(typeof(Player), "GetBreathPeriod")]
        internal class Player_GetBreathPeriod_Patch
        {
            internal static bool Prefix(Player __instance, ref float __result)
            {
                //ErrorMessage.AddDebug("depthLevel " + (int)__instance.depthLevel);
                //ErrorMessage.AddDebug("depthOf " + (int)Ocean.main.GetDepthOf(__instance.gameObject);
                if (!Main.config.realOxygenCons)
                    return true;

                if (__instance.mode == Player.Mode.Piloting || __instance.mode == Player.Mode.LockedPiloting)
                {
                    __result = 3f;
                    return false;
                }
                if (Inventory.main.equipment.GetCount(TechType.Rebreather) > 0)
                {
                    __result = 3f;
                    return false;
                }
                float depth = Mathf.Abs(__instance.depthLevel);
                float mult = 1.5f / Main.config.crushDepth;
                __result = 3f - depth * mult;
                return false;
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

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static IEnumerator Test()
            {
                //ErrorMessage.AddDebug("Test start ");
                //Main.Log("Test start ");
                while (!uGUI.main.hud.active)
                    yield return null;
                ErrorMessage.AddDebug("Test end ");
            }

            static void Postfix(Player __instance)
            {
                gUIHand = Player.main.GetComponent<GUIHand>();
                if (Main.config.cantScanExosuitClawArm)
                    DisableExosuitClawArmScan();

                //__instance.StartCoroutine(Test());

            }
        }

        [HarmonyPatch(typeof(Player), "GetDepthClass")]
        internal class Player_GetDepthClass_Patch
        {
            public static bool Prefix(Player __instance, ref Ocean.DepthClass __result)
            {
                //ErrorMessage.AddDebug("GetDepthClass");
                Ocean.DepthClass depthClass = Ocean.DepthClass.Surface;
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
                    Inventory.main.quickSlots.Deselect();
                else
                    Inventory.main.quickSlots.SelectImmediate(Main.config.activeSlot);
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetUseItemAction")]
        internal class Inventory_GetUseItemAction_Patch
        {
            internal static void Postfix(Inventory __instance, ref ItemAction __result, InventoryItem item)
            {
                if (Main.config.cantEatUnderwater && Player.main.IsUnderwater())
                {
                    Pickupable pickupable = item.item;
                    if (pickupable.gameObject.GetComponent<Eatable>())
                    {
                        __result = ItemAction.None;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_SceneLoading), "End")]
        internal class uGUI_SceneLoading_End_Patch
        {
            public static void Postfix(uGUI_SceneLoading __instance)
            {
                //ErrorMessage.AddDebug("uGUI_SceneLoading End");
                //Main.config.activeSlot = -1;
                //Main.config.openedWreckDoors = new Dictionary<int, bool>();
                //Main.config.Save();
            }
        }

        [HarmonyPatch(typeof(Inventory), "LoseItems")]
        internal class Inventory_LoseItems_Patch
        {
            public static void Postfix(Inventory __instance)
            {
                //ErrorMessage.AddDebug("LoseItems");
                if (Main.config.dropAllitemsOndeath)
                {
                    List<InventoryItem> inventoryItemList = new List<InventoryItem>();
                    foreach (InventoryItem inventoryItem in Inventory.main.container)
                    {
                        inventoryItemList.Add(inventoryItem);
                    }
                    foreach (InventoryItem inventoryItem in (IItemsContainer)Inventory.main.equipment)
                    {
                        //ErrorMessage.AddDebug("equipment " + inventoryItem.item.GetTechName());
                        inventoryItemList.Add(inventoryItem);
                    }
                    foreach (InventoryItem item in inventoryItemList)
                    {
                        //ErrorMessage.AddDebug("DROP " + item.item.GetTechName());
                        __instance.InternalDropItem(item.item, false);
                    }
                }
            }
        }



    }
}
