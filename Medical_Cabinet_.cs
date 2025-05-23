﻿using FMOD.Studio;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(MedicalCabinet))]
    internal class Medical_Cabinet_
    {
        public static MedicalCabinet escapePodMedCabinet;

        public static bool IsMedCabinetInEscapePod(MedicalCabinet medicalCabinet)
        {
            if (medicalCabinet.transform.parent == null)
                return false;

            return medicalCabinet.transform.parent.name == "MedCabRoot";
        }

        public static bool CanProduceMedkit()
        {
            if (ConfigToEdit.escapePodMedkitCabinetWorks.Value == ConfigToEdit.EscapePodMedicalCabinetWorks.Never)
                return false;

            //AddDebug("GetHealthFraction " + EscapePod.main.liveMixin.GetHealthFraction());
            if (ConfigToEdit.escapePodMedkitCabinetWorks.Value == ConfigToEdit.EscapePodMedicalCabinetWorks.After_repairing_life_pod)
            {
                if (EscapePod.main.liveMixin.GetHealthFraction() <= 0.99f)
                    return false;
            }
            return true;
        }

        public static void OnEscapePodRepair()
        {
            if (ConfigToEdit.escapePodMedkitCabinetWorks.Value != ConfigToEdit.EscapePodMedicalCabinetWorks.After_repairing_life_pod)
                return;

            if (escapePodMedCabinet == null)
                return;

            if (escapePodMedCabinet.hasMedKit)
                escapePodMedCabinet.InvokeRepeating("BlinkRepeat", 0f, 1f);
            else if (!escapePodMedCabinet.hasMedKit && escapePodMedCabinet.doorOpen)
                escapePodMedCabinet.ToggleDoorState();

            escapePodMedCabinet.timeSpawnMedKit = DayNightCycle.main.timePassedAsFloat + escapePodMedCabinet.medKitSpawnInterval;
        }

        public static void Initialize(MedicalCabinet medicalCabinet)
        {
            escapePodMedCabinet = medicalCabinet;
            //AddDebug("escapePodMedCabinet Init timeSpawnMedKit " + medicalCabinet.timeSpawnMedKit);
            //AddDebug("escapePodMedCabinet Init CanProduceMedkit " + CanProduceMedkit());
            medicalCabinet.doorOpenQuat = medicalCabinet.doorOpenTransform.localRotation;
            medicalCabinet.doorCloseQuat = medicalCabinet.door.transform.localRotation;
            medicalCabinet.doorMat = medicalCabinet.doorRenderer.material;
            medicalCabinet.doorMat.SetFloat(ShaderPropertyID._GlowStrength, 0f);
            medicalCabinet.doorMat.SetFloat(ShaderPropertyID._GlowStrengthNight, 0f);
            if (medicalCabinet.timeSpawnMedKit == -2)
            {
                medicalCabinet.medKitModel.SetActive(false);
                return;
            }
            medicalCabinet.hasMedKit = true;
            medicalCabinet.medKitModel.SetActive(true);
            if (medicalCabinet.hasMedKit && CanProduceMedkit())
                CoroutineHost.StartCoroutine(SetupAlert(medicalCabinet));
        }

        [HarmonyPrefix]
        [HarmonyPatch("Start")]
        public static bool StartPrefix(MedicalCabinet __instance)
        {
            //AddDebug("MedicalCabinet Start CanProduceMedkit " + CanProduceMedkit());
            if (ConfigToEdit.medkitFabAlertSound.Value == false)
                __instance.playSound.evt.setVolume(0);

            if (!IsMedCabinetInEscapePod(__instance))
                return true;

            if (!CanProduceMedkit())
            {
                Initialize(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void StartPostfix(MedicalCabinet __instance)
        {
            //AddDebug(__instance.transform.parent.name + " MedicalCabinet Start hasMedKit " + __instance.hasMedKit + " CanProduceMedkit " + CanProduceMedkit());
            if (!Main.gameLoaded && __instance.hasMedKit)
                if (IsMedCabinetInEscapePod(__instance))
                {
                    if (CanProduceMedkit())
                        CoroutineHost.StartCoroutine(SetupAlert(__instance));
                }
                else
                    CoroutineHost.StartCoroutine(SetupAlert(__instance));
        }

        [HarmonyPrefix]
        [HarmonyPatch("ForceSpawnMedKit")]
        public static bool ForceSpawnMedKitPrefix(MedicalCabinet __instance)
        { // wtf calls this?
            if (!IsMedCabinetInEscapePod(__instance) || CanProduceMedkit())
                return true;

            //AddDebug("ForceSpawnMedKit ");
            return false;
        }

        static IEnumerator SetupAlert(MedicalCabinet medicalCabinet)
        {
            yield return new WaitUntil(() => Main.gameLoaded == true);
            yield return new WaitForSeconds(1);
            //AddDebug(medicalCabinet.transform.parent.name + " MedicalCabinet SetupAlert ");
            medicalCabinet.InvokeRepeating("BlinkRepeat", 0f, 1f);
            medicalCabinet.playSound.Play();
        }

        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static bool UpdatePrefix(MedicalCabinet __instance)
        {
            if (!Main.gameLoaded)
                return false;

            if (!IsMedCabinetInEscapePod(__instance))
                return true;

            //AddDebug("escapePodMedkitCabinet " + ConfigToEdit.escapePodMedkitCabinet.Value);
            if (!CanProduceMedkit())
            {
                //__instance.medKitModel.SetActive(__instance.hasMedKit);
                if (__instance.changeDoorState)
                    __instance.door.transform.localRotation = Quaternion.Slerp(__instance.door.transform.localRotation, __instance.doorOpen ? __instance.doorOpenQuat : __instance.doorCloseQuat, Time.deltaTime * 5f);

                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandClick")]
        public static bool OnHandClickPrefix(MedicalCabinet __instance)
        {
            if (!IsMedCabinetInEscapePod(__instance))
                return true;

            if (!CanProduceMedkit())
            {
                bool invRoom = Player.main.HasInventoryRoom(1, 1);
                //AddDebug("doorOpen " + __instance.doorOpen);
                //AddDebug("hasMedKit " + __instance.hasMedKit);
                //AddDebug("invRoom " + invRoom);
                if (__instance.doorOpen && __instance.hasMedKit && invRoom)
                {
                    CraftData.AddToInventory(TechType.FirstAidKit);
                    __instance.hasMedKit = false;
                    __instance.timeSpawnMedKit = -2;
                    __instance.CancelInvoke("BlinkRepeat");
                    __instance.medKitModel.SetActive(false);
                }
                else
                {
                    __instance.ToggleDoorState();
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandHover")]
        public static bool OnHandHoverPrefix(MedicalCabinet __instance, GUIHand hand)
        {
            if (!IsMedCabinetInEscapePod(__instance))
                return true;
            //AddDebug("CanProduceMedkit " + CanProduceMedkit());
            if (!CanProduceMedkit())
            {
                string text = __instance.doorOpen ? "MedicalCabinet_DoorClose" : "MedicalCabinet_DoorOpen";
                HandReticle.main.SetText(HandReticle.TextType.Hand, nameof(MedicalCabinet), true, GameInput.Button.LeftHand);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text, true);
                if (__instance.hasMedKit)
                    HandReticle.main.SetIcon(HandReticle.IconType.Hand);

                return false;
            }
            return true;
        }

    }


}
