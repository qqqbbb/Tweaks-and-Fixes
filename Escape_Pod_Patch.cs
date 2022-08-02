﻿
using UnityEngine;
using HarmonyLib;
//using System;
using System.Collections.Generic;
//using System.Linq;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    //Damaged sky MasterIntensity 2.5
    //Damaged sky DiffIntensity 0.8
    //Damaged sky specIntensity 1
    //Operational sky MasterIntensity 10
    //Operational sky DiffIntensity 2
    //Operational sky specIntensity 1.5
    class Escape_Pod_Patch
    {
        public static void RepairPod(EscapePod escapePod)
        {
            //AddDebug("RepairPod");
            if (escapePod.vfxSpawner.spawnedObj != null)
            {
                ParticleSystem particleSystem = escapePod.vfxSpawner.spawnedObj.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                    particleSystem.Stop();
            }
            for (int index = 0; index < escapePod.lightingController.skies.Length; ++index)
            {
                LightingController.MultiStatesSky sky = escapePod.lightingController.skies[index];
                sky.sky.AffectedByDayNightCycle = false;
            }
            escapePod.animator.SetFloat("lifepod_damage", escapePod.healthScalar);
            escapePod.damagedSound.Stop();
            escapePod.damageEffectsShowing = false;
            escapePod.lightingController.LerpToState(0, 5f);
            uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod4Header"), new Color32((byte)159, (byte)243, (byte)63, byte.MaxValue));
            uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod4Content"), new Color32((byte)159, (byte)243, (byte)63, byte.MaxValue));
            uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod4Power"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
            RegeneratePowerSource[] cells = escapePod.GetAllComponentsInChildren<RegeneratePowerSource>();
            int maxPower = Main.config.escapePodMaxPower;
            foreach (RegeneratePowerSource cell in cells)
            {
                //AddDebug("RepairPod maxPower " + maxPower);
                //AddDebug("RegeneratePowerSource " + cell.name);
                cell.regenerationThreshhold = maxPower;
                cell.powerSource.maxPower = maxPower;
            }
        }

        public static bool IsSmokeOut(string slot)
        {
            return Main.config.escapePodSmokeOut.ContainsKey(slot) && Main.config.escapePodSmokeOut[slot];
        }

        public static void LetSmokeOut(EscapePod escapePod)
        {
            //AddDebug("LetSmokeOut");
            if (escapePod.vfxSpawner.spawnedObj != null)
            {
                ParticleSystem particleSystem = escapePod.vfxSpawner.spawnedObj.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                    particleSystem.Stop();
            }
            for (int index = 0; index < escapePod.lightingController.skies.Length; ++index)
            {
                LightingController.MultiStatesSky sky = escapePod.lightingController.skies[index];
                sky.sky.AffectedByDayNightCycle = true;
            }
            //AddDebug("currentSlot " + SaveLoadManager.main.currentSlot);
            Main.config.escapePodSmokeOut[SaveLoadManager.main.currentSlot] = true;
        }

        [HarmonyPatch(typeof(EscapePod))]
        class EscapePod_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ShowDamagedEffects")]
            public static bool DamagePod(EscapePod __instance)
            {
                //AddDebug("try DamagePod " + __instance.introCinematic.state);
                if (__instance.isNewBorn && __instance.introCinematic.state == PlayerCinematicController.State.None)
                    return false; // dont damage before intro cinematic

                //AddDebug("DamagePod");
                if (!IsSmokeOut(SaveLoadManager.main.currentSlot))
                    __instance.vfxSpawner.SpawnManual();

                __instance.damagedSound.Play();
                __instance.damageEffectsShowing = true;
                __instance.lightingController.SnapToState(2);

                uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod3Header"), (Color)new Color32((byte)243, (byte)201, (byte)63, byte.MaxValue), 2f);
                uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod3Content"), (Color)new Color32((byte)233, (byte)63, (byte)27, byte.MaxValue));
                uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod3Power"), (Color)new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
                RegeneratePowerSource[] cells = __instance.GetAllComponentsInChildren<RegeneratePowerSource>();
                int maxPower = Main.config.escapePodMaxPower;
                //AddDebug("DamagePod maxPower " + maxPower);
                foreach (RegeneratePowerSource cell in cells)
                {
                    //AddDebug("maxPower " + maxPower);
                    cell.regenerationThreshhold = maxPower;
                    if (Main.config.escapePodPowerTweak)
                        cell.powerSource.maxPower = maxPower * .5f;
                    else
                    {
                        cell.powerSource.maxPower = maxPower;
                        cell.powerSource.power = maxPower;
                    }
                }
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("UpdateDamagedEffects")]
            public static bool UpdateDamagedEffectsPrefix(EscapePod __instance)
            {
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("StopIntroCinematic")]
            public static void StopIntroCinematicPostfix(EscapePod __instance)
            {
                RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
                int maxPower = Main.config.escapePodMaxPower;
                if (Main.config.escapePodPowerTweak && cells != null)
                {
                    foreach (RegeneratePowerSource cell in cells)
                    {
                        //float chargeable = cell.powerSource.GetMaxPower() - cell.powerSource.GetPower();
                        cell.powerSource.power = maxPower * .15f;
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRepair")]
            public static void OnRepairPostfix(EscapePod __instance)
            {
                RepairPod(__instance);
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("DamageRadio")]
            public static void DamageRadioPostfix(EscapePod __instance)
            {
                AddDebug("DamageRadio");
            }

        }

        [HarmonyPatch(typeof(EscapePodFirstUseCinematicsController))]
        class EscapePodFirstUseCinematicsController_Patch
        {// exiting escape pod using top hatch
            [HarmonyPatch("OnTopHatchCinematicEnd")]
            [HarmonyPostfix]
            public static void OnTopHatchCinematicEnd(EscapePodFirstUseCinematicsController __instance)
            {
                if (__instance.escapePod.damageEffectsShowing)
                    LetSmokeOut(__instance.escapePod);
            }
        }

        //[HarmonyPatch(typeof(EscapePod), "OnProtoDeserialize")]
        public class EscapePod_OnProtoDeserialize_Patch
        { // power cells not loaded when OnProtoDeserialize runs
            public static void Postfix(EscapePod __instance)
            {
                //AddDebug("EscapePod damageEffectsShowing " + __instance.damageEffectsShowing);
                //AddDebug("health " + __instance.liveMixin.health);
                //AddDebug("OnProtoDeserialize " + __instance.liveMixin.GetHealthFraction()); 
                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;

                if (GameModeUtils.SpawnsInitialItems()) // creative mode
                    return;

                if (__instance.liveMixin.GetHealthFraction() < 0.99f)
                {
                    EscapePod_Patch.DamagePod(__instance);
                    if (IsSmokeOut(SaveLoadManager.main.currentSlot))
                        LetSmokeOut(__instance);
                }
                else
                    RepairPod(__instance);
            }
        }

        [HarmonyPatch(typeof(EnterExitHelper), "Enter")]
        class EnterExitHelper_Enter_Patch
        { // patched method is static
            public static void Postfix( GameObject gameObject)
            { // entering escape pod using top hatch
                //AddDebug("position.y " + gameObject.transform.position.y);
                if (Player.main.currentEscapePod && EscapePod.main.damageEffectsShowing && gameObject.transform.position.y > 2f)
                {
                    LetSmokeOut(EscapePod.main);
                }
            }
        }

        [HarmonyPatch(typeof(IntroFireExtinguisherHandTarget))]
        class IntroFireExtinguisherHandTarget_Patch
        { // not checking save slot
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static bool StartPrefix(IntroFireExtinguisherHandTarget __instance)
            {
                if (Main.config.pickedUpFireExt)
                {
                    __instance.extinguisherModel.SetActive(false);
                    Object.Destroy(__instance.gameObject);
                }
                return false;
            }
            [HarmonyPostfix]
            [HarmonyPatch("UseVolume")]
            static void StartPostfix(IntroFireExtinguisherHandTarget __instance)
            {
                Main.config.pickedUpFireExt = true;
            }
        }

        [HarmonyPatch(typeof(LootSpawner), "Start")]
        class LootSpawner_Start_Patch
        {
            public static void Postfix(LootSpawner __instance)
            {
                __instance.escapePodTechTypes = new List<TechType>();
                foreach (KeyValuePair<string, int> loot in Main.config.startingLoot)
                {
                    TechTypeExtensions.FromString(loot.Key, out TechType tt, true);
                    //Main.Log("Start Loot " + tt);
                    //AddDebug("Start Loot " + tt);
                    if (tt == TechType.None)
                        continue;

                    for (int i = 0; i < loot.Value; i++)
                        __instance.escapePodTechTypes.Add(tt);
                }
            }
        }

        //[HarmonyPatch(typeof(EscapePod), "Start")]
        class EscapePod_Start_Patch
        {
            public static void Postfix(EscapePod __instance)
            {
                RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
                int maxPower = Main.config.escapePodMaxPower;
                if (cells != null)
                {
                    //AddDebug("EscapePod start cells " + cells.Length);
                    foreach (RegeneratePowerSource cell in cells)
                    {
                        //if (Main.config.escapePodPowerTweak && __instance.damageEffectsShowing)
                        //    cell.powerSource.maxPower = maxPower * .5f;
                        //else
                        //    cell.powerSource.maxPower = maxPower;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Radio))]
        class Radio_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(Radio __instance)
            { // fix: radio repairs itself after reload
                bool fixed_ = Main.config.radioFixed.ContainsKey(SaveLoadManager.main.currentSlot) &&  Main.config.radioFixed[SaveLoadManager.main.currentSlot];
                //AddDebug("radio OnEnable " + fixed_);
                //AddDebug("radio OnEnable health " + __instance.liveMixin.health);
                if (!fixed_ && __instance.IsFullHealth())
                {
                    //AddDebug("radio should be damaged ");
                    __instance.liveMixin.TakeDamage(80f);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRepair")]
            public static void OnRepairPostfix(Radio __instance)
            {
                //LiveMixin lm = __instance.GetComponent<LiveMixin>();
                //if (lm)
                //    AddDebug("radio OnRepair " + lm.health);
                Main.config.radioFixed[SaveLoadManager.main.currentSlot] = true;
            }
        }


    }
}
