
using UnityEngine;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using static ErrorMessage;
using static VFXParticlesPool;

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

        static IEnumerator SetMaxPower(EscapePod escapePod)
        {
            //yield return new WaitForSeconds(waitTime);
            while (!Main.loadingDone)
                yield return null;
            
            float maxPower = Main.config.escapePodMaxPower;
            bool damaged = escapePod.damageEffectsShowing || Player.main.isNewBorn;
            foreach (RegeneratePowerSource cell in escapePod.GetAllComponentsInChildren<RegeneratePowerSource>())
            {
                if (Main.config.escapePodPowerTweak && damaged)
                    cell.powerSource.maxPower = maxPower * .5f;
                else
                    cell.powerSource.maxPower = maxPower;

                //AddDebug("SetMaxPower maxPower " + cell.powerSource.maxPower);
                //AddDebug("SetMaxPower damageEffectsShowing " + escapePod.damageEffectsShowing);
                //AddDebug("SetMaxPower isNewBorn " + Player.main.isNewBorn);
                cell.regenerationThreshhold = maxPower;
                if (cell.powerSource.power > cell.powerSource.maxPower)
                    cell.powerSource.power = cell.powerSource.maxPower;

                if (Main.config.escapePodPowerTweak && Player.main.isNewBorn)
                    cell.powerSource.power = 0;
            }
            if (damaged && IsSmokeOut(SaveLoadManager.main.currentSlot))
                LetSmokeOut(escapePod);
        }

        [HarmonyPatch(typeof(EscapePod))]
        class EscapePod_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(EscapePod __instance)
            {
                //RegeneratePowerSource[] cells = EscapePod.main.gameObject.GetAllComponentsInChildren<RegeneratePowerSource>();
                //if (cells != null)
                {
                    //AddDebug("EscapePod start cells " + cells.Length);
                    Player.main.StartCoroutine(SetMaxPower(__instance));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRepair")]
            public static void OnRepairPostfix(EscapePod __instance)
            {
                //RepairPod(__instance);
                //AddDebug("EscapePod OnRepair");
                int maxPower = Main.config.escapePodMaxPower;
                foreach (RegeneratePowerSource cell in __instance.GetAllComponentsInChildren<RegeneratePowerSource>())
                {
                    //AddDebug("RepairPod maxPower " + maxPower);
                    //AddDebug("RegeneratePowerSource " + cell.name);
                    cell.regenerationThreshhold = maxPower;
                    cell.powerSource.maxPower = maxPower;
                }
            }

        }

        [HarmonyPatch(typeof(EscapePodFirstUseCinematicsController))]
        class EscapePodFirstUseCinematicsController_Patch
        {// exiting escape pod using top hatch
            [HarmonyPostfix]
            [HarmonyPatch("OnTopHatchCinematicEnd")]
            public static void OnTopHatchCinematicEnd(EscapePodFirstUseCinematicsController __instance)
            {
                //AddDebug("OnTopHatchCinematicEnd ");
                if (__instance.escapePod.damageEffectsShowing)
                    LetSmokeOut(__instance.escapePod);
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
                //AddDebug("IntroFireExtinguisherHandTarget Start");
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
                //AddDebug("IntroFireExtinguisherHandTarget UseVolume");
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
