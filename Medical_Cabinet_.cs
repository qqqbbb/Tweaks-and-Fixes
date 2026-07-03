using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(MedicalCabinet))]
    internal class Medical_Cabinet_
    {
        public static MedicalCabinet escapePodMedCabinet;
        static Vector3 posFix = new Vector3(0, 0, .02f);

        public static bool IsMedCabinetInEscapePod(MedicalCabinet medicalCabinet)
        {
            if (medicalCabinet.transform.parent == null)
                return false;

            return medicalCabinet.transform.parent.name == "MedCabRoot";
        }

        public static bool CanProduceMedkit(MedicalCabinet medicalCabinet)
        {
            if (IsMedCabinetInEscapePod(medicalCabinet) == false)
                return true;

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
            if (medicalCabinet.hasMedKit && CanProduceMedkit(medicalCabinet))
                CoroutineHost.StartCoroutine(SetupAlert(medicalCabinet));
        }

        [HarmonyPrefix, HarmonyPatch("Start")]
        public static bool StartPrefix(MedicalCabinet __instance)
        {
            //AddDebug($"MedicalCabinet Start hasMedKit {__instance.hasMedKit} timeSpawnMedKit {__instance.timeSpawnMedKit}");
            if (__instance.timeSpawnMedKit == -1)
            {
                __instance.transform.Translate(posFix);
                //__instance.ToggleDoorState();
            }
            //if (ConfigToEdit.medkitFabAlertSound.Value == false)
            //    __instance.playSound.evt.setVolume(0);

            if (!CanProduceMedkit(__instance))
            {
                Initialize(__instance);
                return false;
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(MedicalCabinet __instance)
        {
            if (!Main.gameLoaded && __instance.hasMedKit)
            {
                if (CanProduceMedkit(__instance))
                    CoroutineHost.StartCoroutine(SetupAlert(__instance));
                else
                    CoroutineHost.StartCoroutine(SetupAlert(__instance));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ForceSpawnMedKit")]
        public static bool ForceSpawnMedKitPrefix(MedicalCabinet __instance)
        { // wtf calls this?
            //AddDebug("ForceSpawnMedKit ");
            return CanProduceMedkit(__instance);
        }

        static IEnumerator SetupAlert(MedicalCabinet medicalCabinet)
        {
            yield return Main.waitUntilGameLoaded;
            yield return Main.oneSecond;
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

            //AddDebug("escapePodMedkitCabinet " + ConfigToEdit.escapePodMedkitCabinet.Value);
            if (CanProduceMedkit(__instance) == false)
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
            if (CanProduceMedkit(__instance) == false)
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
            //AddDebug("CanProduceMedkit " + CanProduceMedkit());
            if (CanProduceMedkit(__instance) == false)
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
