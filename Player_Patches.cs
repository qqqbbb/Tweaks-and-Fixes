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
        static float updateHungerInterval = 10f;
        static Survival survival;
        static LiveMixin liveMixin;
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

        [HarmonyPatch(typeof(Survival), "UpdateHunger")]
        internal class Survival_UpdateHunger_Patch
        {// remove health regen from food
            internal static bool Prefix(Survival __instance)
            {
                if (!Main.config.noHealthRegenFromFood)
                    return true;

                if (!GameModeUtils.RequiresSurvival() || __instance.freezeStats)
                    return false;
                //ErrorMessage.AddDebug("kUpdateHungerInterval " + __instance.kUpdateHungerInterval);
                float originalDamage = __instance.UpdateStats(updateHungerInterval);
                if (liveMixin && originalDamage > 1.4f)
                    liveMixin.TakeDamage(originalDamage, Player.main.transform.position, DamageType.Starve);
                return false;
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

        [HarmonyPatch(typeof(Survival), nameof(Survival.Reset))]
        internal class Survival_Reset_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(Survival __instance)
            {
                survival = Player.main.GetComponent<Survival>();
                liveMixin = Player.main.GetComponent<LiveMixin>();
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
                //Eatable ds = Player.main.gameObject.GetComponent<Eatable>();
                //if (ds == null)
                //{
                //    ErrorMessage.AddDebug("AddComponent DoorSave");
                //    ds = Player.main.gameObject.AddComponent<Eatable>();
                //    ds.timeDecayStart = -111f;
                //}
                //else
                //    ErrorMessage.AddDebug(" DoorSave " + ds.timeDecayStart);
            }
        }

        [HarmonyPatch(typeof(IngameMenu), "SaveGame")]
        internal class SaveGamePatch
        {
            public static void Postfix()
            {
                //Main.config.activeSlot = Inventory.main.quickSlots.activeSlot;
                if (Player.main.mode == Player.Mode.Normal)
                    Main.config.playerCamRot = MainCameraControl.main.viewModel.localRotation.eulerAngles.y;
                else
                    Main.config.playerCamRot = -1f;

                Main.config.activeSlot = Inventory.main.quickSlots.activeSlot;

                Main.config.Save();
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
                    //Inventory.main.quickSlots.DeselectImmediate();
                    Inventory.main.quickSlots.Deselect();
                else
                    Inventory.main.quickSlots.SelectImmediate(Main.config.activeSlot);
            }
        }

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            static System.Random rndm = new System.Random();

            public static void Postfix(float damage, ref float __result, GameObject target, DamageType type, GameObject dealer)
            {
                //if (dealer == null) 
                //Main.Message("CalculateDamage no dealer");
                //else
                //    Main.Message("dealer " + dealer.name);
                //if (target == Player.mainObject)
                //{
                    //if (Main.config.disableDamage)
                    //    __result = .0f;
                //}
                if (Main.config.dropHeldTool && target == Player.mainObject)
                {
                    if (type != DamageType.Cold && type != DamageType.Poison && type != DamageType.Starve && type != DamageType.Radiation && type != DamageType.Pressure)
                    {
                        int rnd = rndm.Next(1, (int)Player.main.liveMixin.maxHealth);
                        if (rnd < damage)
                        {
                            //ErrorMessage.AddDebug("DropHeldItem");
                            Inventory.main.DropHeldItem(true);
                        }
                    }
                }
                //else 
                //__result *= 2222f;

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

        //[HarmonyPatch(typeof(PlayerController), "SetMotorMode")]
        internal class Player_Movement_Speed_Patch
        {
            public static void Postfix(PlayerController __instance)
            {
                ErrorMessage.AddDebug("PlayerController SetMotorMode");
                __instance.underWaterController.backwardMaxSpeed = __instance.underWaterController.forwardMaxSpeed * .5f;
                __instance.underWaterController.strafeMaxSpeed = __instance.underWaterController.backwardMaxSpeed;

                __instance.groundController.backwardMaxSpeed = __instance.groundController.forwardMaxSpeed * .5f;
                __instance.groundController.strafeMaxSpeed = __instance.groundController.backwardMaxSpeed;
            }
        }



    }
}
