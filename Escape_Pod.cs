
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    //Damaged sky MasterIntensity 2.5
    //Damaged sky DiffIntensity 0.8
    //Damaged sky specIntensity 1
    //Operational sky MasterIntensity 10
    //Operational sky DiffIntensity 2
    //Operational sky specIntensity 1.5
    class Escape_Pod
    {
        public static Dictionary<TechType, int> newGameLoot = new Dictionary<TechType, int>();

        public static void EscapePodInit()
        {
            SetMaxPower();
            if (ConfigToEdit.sunlightAffectsEscapePodLighting.Value)
                AllowSunlight();

            if (Main.configMain.GetEscapePodSmoke())
                StopSmoke();
        }

        static void StopSmoke()
        {
            //AddDebug("StopSmoke");
            if (EscapePod.main.vfxSpawner.spawnedObj != null)
            {
                ParticleSystem particleSystem = EscapePod.main.vfxSpawner.spawnedObj.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                    particleSystem.Stop();
            }
            //AddDebug("currentSlot " + SaveLoadManager.main.currentSlot);
            Main.configMain.SaveEscapePodSmoke();
            //Main.configMain.Save();
        }

        static void AllowSunlight()
        {
            //AddDebug("AllowSunlight");
            for (int index = 0; index < EscapePod.main.lightingController.skies.Length; ++index)
            {
                LightingController.MultiStatesSky sky = EscapePod.main.lightingController.skies[index];
                sky.sky.AffectedByDayNightCycle = true;
            }
        }

        static void SetMaxPower()
        {
            float maxPower = ConfigMenu.escapePodMaxPower.Value;
            bool damaged = EscapePod.main.damageEffectsShowing || Player.main.isNewBorn;
            foreach (RegeneratePowerSource cell in EscapePod.main.GetAllComponentsInChildren<RegeneratePowerSource>())
            {
                if (ConfigToEdit.escapePodPowerTweak.Value && damaged)
                    cell.powerSource.maxPower = maxPower * .5f;
                else
                    cell.powerSource.maxPower = maxPower;

                //AddDebug("SetMaxPower maxPower " + cell.powerSource.maxPower);
                //AddDebug("SetMaxPower damageEffectsShowing " + escapePod.damageEffectsShowing);
                //AddDebug("SetMaxPower isNewBorn " + Player.main.isNewBorn);
                cell.regenerationThreshhold = maxPower;
                if (cell.powerSource.power > cell.powerSource.maxPower)
                    cell.powerSource.power = cell.powerSource.maxPower;

                if (ConfigToEdit.escapePodPowerTweak.Value && Player.main.isNewBorn)
                    cell.powerSource.power = 0;
            }
        }

        [HarmonyPatch(typeof(EscapePod))]
        class EscapePod_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void StartPostfix(EscapePod __instance)
            {
                //RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
                //if (cells != null)
                {
                    //AddDebug("EscapePod start cells " + cells.Length);
                    //Player.main.StartCoroutine(SetMaxPower(__instance));
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnRepair")]
            public static void OnRepairPostfix(EscapePod __instance)
            {
                //RepairPod(__instance);
                //AddDebug("EscapePod OnRepair");
                int maxPower = ConfigMenu.escapePodMaxPower.Value;
                foreach (RegeneratePowerSource cell in __instance.GetAllComponentsInChildren<RegeneratePowerSource>())
                {
                    //AddDebug("RepairPod maxPower " + maxPower);
                    //AddDebug("RegeneratePowerSource " + cell.name);
                    cell.regenerationThreshhold = maxPower;
                    cell.powerSource.maxPower = maxPower;
                }
                Medical_Cabinet_.OnEscapePodRepair();
            }

        }

        [HarmonyPatch(typeof(EscapePodFirstUseCinematicsController))]
        class EscapePodFirstUseCinematicsController_Patch
        {// exiting escape pod using top hatch
            [HarmonyPostfix, HarmonyPatch("OnTopHatchCinematicEnd")]
            public static void OnTopHatchCinematicEnd(EscapePodFirstUseCinematicsController __instance)
            {
                //AddDebug("OnTopHatchCinematicEnd ");
                if (__instance.escapePod.damageEffectsShowing)
                    StopSmoke();
            }
        }

        [HarmonyPatch(typeof(EnterExitHelper), "Enter")]
        class EnterExitHelper_Enter_Patch
        { // patched method is static
            public static void Postfix(GameObject gameObject)
            { // entering escape pod using top hatch
              //AddDebug("position.y " + gameObject.transform.position.y);
                if (Player.main.currentEscapePod && EscapePod.main.damageEffectsShowing && gameObject.transform.position.y > 2f)
                {
                    StopSmoke();
                }
            }
        }

        [HarmonyPatch(typeof(IntroFireExtinguisherHandTarget))]
        class IntroFireExtinguisherHandTarget_Patch
        {
            [HarmonyPrefix, HarmonyPatch("Start")]
            static bool StartPrefix(IntroFireExtinguisherHandTarget __instance)
            {
                //AddDebug("IntroFireExtinguisherHandTarget Start");
                if (Utils.GetContinueMode() && Main.configMain.pickedUpFireExt.Contains(SaveLoadManager.main.currentSlot))
                {
                    __instance.extinguisherModel.SetActive(false);
                    Util.DestroyEntity(__instance.gameObject);
                }
                return false;
            }
            [HarmonyPostfix, HarmonyPatch("UseVolume")]
            static void StartPostfix(IntroFireExtinguisherHandTarget __instance)
            {
                //AddDebug("IntroFireExtinguisherHandTarget UseVolume");
                Main.configMain.pickedUpFireExt.Add(SaveLoadManager.main.currentSlot);
            }
        }

        [HarmonyPatch(typeof(LootSpawner), "Start")]
        class LootSpawner_Start_Patch
        {
            public static void Postfix(LootSpawner __instance)
            {
                if (newGameLoot == null || newGameLoot.Count == 0)
                    return;

                __instance.escapePodTechTypes.Clear();
                foreach (KeyValuePair<TechType, int> loot in newGameLoot)
                {
                    //Main.Log("Start Loot " + tt);
                    //AddDebug("Start Loot " + tt);
                    for (int i = 0; i < loot.Value; i++)
                        __instance.escapePodTechTypes.Add(loot.Key);
                }
            }
        }

        [HarmonyPatch(typeof(Radio), "OnHandHover")]
        class Radior_OnHandHover_Patch
        {
            public static void Postfix(Radio __instance)
            {
                if (!__instance.IsFullHealth())
                    HandReticle.main.SetText(HandReticle.TextType.Hand, "DamagedRadio", true);
            }
        }

    }
}
