
using UnityEngine;
using HarmonyLib;

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
            //ErrorMessage.AddDebug("RepairPod");
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
            uGUI_EscapePod.main.SetHeader(Language.main.Get("IntroEscapePod4Header"), (Color)new Color32((byte)159, (byte)243, (byte)63, byte.MaxValue));
            uGUI_EscapePod.main.SetContent(Language.main.Get("IntroEscapePod4Content"), (Color)new Color32((byte)159, (byte)243, (byte)63, byte.MaxValue));
            uGUI_EscapePod.main.SetPower(Language.main.Get("IntroEscapePod4Power"), (Color)new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
            RegeneratePowerSource[] cells = escapePod.GetAllComponentsInChildren<RegeneratePowerSource>();
            int maxPower = Main.config.escapePodMaxPower;
            if (cells != null)
            {
                foreach (RegeneratePowerSource cell in cells)
                {
                    cell.powerSource.maxPower = maxPower;
                }
            }
        }

        public static bool IsSmokeOut(string slot)
        {
            if (Main.config.escapePodSmokeOut.ContainsKey(slot) && Main.config.escapePodSmokeOut[slot])
                return true;

            return false;
        }

        public static void LetSmokeOut(EscapePod escapePod)
        {
            //ErrorMessage.AddDebug("LetSmokeOut");
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
            //ErrorMessage.AddDebug("currentSlot " + SaveLoadManager.main.currentSlot);
            Main.config.escapePodSmokeOut[SaveLoadManager.main.currentSlot] = true;
        }

        [HarmonyPatch(typeof(EscapePod), "ShowDamagedEffects")]
        class ShowDamagedEffects_Patch
        {
            [HarmonyPrefix]
            public static bool DamagePod(EscapePod __instance)
            {
                //if (__instance.damageEffectsShowing)
                //    return false;
                //ErrorMessage.AddDebug("DamagePod");
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
                if (Main.config.escapePodPowerTweak && cells != null)
                {
                    foreach (RegeneratePowerSource cell in cells)
                    {
                        cell.powerSource.maxPower = maxPower * .5f;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(EscapePod), "UpdateDamagedEffects")]
        class EscapePod_UpdateDamagedEffects_Patch
        {
            public static bool Prefix(EscapePod __instance)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(EscapePod), "StopIntroCinematic")]
        class EscapePod_StopIntroCinematic_Patch
        {
            public static void Postfix(EscapePod __instance)
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
        }

        [HarmonyPatch(typeof(EscapePod), "OnRepair")]
        class EscapePod_OnRepair_Patch
        {
            public static void Postfix(EscapePod __instance)
            {
                RepairPod(__instance);
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

        [HarmonyPatch(typeof(EscapePod), "OnProtoDeserialize")]
        class EscapePod_OnProtoDeserialize_Patch
        {
            public static void Postfix(EscapePod __instance)
            {
                //ErrorMessage.AddDebug("EscapePod damageEffectsShowing " + __instance.damageEffectsShowing);
                //ErrorMessage.AddDebug("health " + __instance.liveMixin.health);
                //ErrorMessage.AddDebug("OnProtoDeserialize " + __instance.liveMixin.GetHealthFraction()); 
                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                rb.constraints = RigidbodyConstraints.None;

                if (GameModeUtils.SpawnsInitialItems()) // creative mode
                    return;

                if (__instance.liveMixin.GetHealthFraction() < 0.99f)
                {
                    ShowDamagedEffects_Patch.DamagePod(__instance);
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
                //ErrorMessage.AddDebug("position.y " + gameObject.transform.position.y);
                if (Player.main.currentEscapePod && EscapePod.main.damageEffectsShowing && gameObject.transform.position.y > 2f)
                {
                    LetSmokeOut(EscapePod.main);
                }
            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {

            static void Postfix(Player __instance)
            {
                if (Input.GetKey(KeyCode.Z))
                {
                    //ErrorMessage.AddDebug("bottomHatchUsed " + EscapePod.main.bottomHatchUsed);
                    //ErrorMessage.AddDebug("topHatchUsed " + EscapePod.main.topHatchUsed);
                    ErrorMessage.AddDebug(" escapePod " + Player.main.escapePod.value);
                    if (Player.main.currentEscapePod)
                    {
                        ErrorMessage.AddDebug(" currentEscapePod " + Player.main.currentEscapePod);
                    }

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
                    foreach (RegeneratePowerSource cell in cells)
                    {
                        if (Main.config.escapePodPowerTweak && __instance.damageEffectsShowing)
                            cell.powerSource.maxPower = maxPower * .5f;
                        else
                            cell.powerSource.maxPower = maxPower;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Radio), "OnHandHover")]
        //class Radio_OnHandHover_Patch
        //{
        //    public static void Postfix(Radio __instance)
        //    {
        //        LiveMixin radioLM = __instance.GetComponent<LiveMixin>();
        //        if (radioLM)
        //            Main.Message("radio health " + radioLM.health);
        //        else
        //            Main.Message("no radioLM  ");
        //    }
        //}
    }
}
