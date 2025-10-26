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
    internal class VehicleLightFix
    {
        static VFXVolumetricLight seamothVFXVolumetricLight;
        static GameObject seamothLightCone;
        public static Color seamothLightColor;
        public static Color exosuitLightColor;
        static Vector3 exosuitLightBeamPos = new Vector3(0, 0.4f, -0.8f); // y changes after entering exosuit

        public static void AddVolLight(GameObject parent, Vector3 pos = default)
        {
            if (seamothLightCone == null)
            {
                Main.logger.LogWarning("can not add vol Light to " + parent.name);
                return;
            }
            GameObject lightCone = UnityEngine.Object.Instantiate(seamothLightCone, Vector3.zero, Quaternion.identity);
            lightCone.transform.parent = parent.transform;
            lightCone.transform.localPosition = pos;
            lightCone.transform.localRotation = Quaternion.identity;
            VFXVolumetricLight volLight = parent.gameObject.AddComponent<VFXVolumetricLight>();
            volLight.syncMeshWithLight = seamothVFXVolumetricLight.syncMeshWithLight;
            volLight.angle = seamothVFXVolumetricLight.angle;
            volLight.range = seamothVFXVolumetricLight.range;
            volLight.intensity = seamothVFXVolumetricLight.intensity;
            volLight.startOffset = seamothVFXVolumetricLight.startOffset;
            volLight.startFallof = seamothVFXVolumetricLight.startFallof;
            volLight.nearClip = seamothVFXVolumetricLight.nearClip;
            volLight.softEdges = seamothVFXVolumetricLight.softEdges;
            volLight.segments = seamothVFXVolumetricLight.segments;
            volLight.lightType = seamothVFXVolumetricLight.lightType;
            volLight.color = seamothVFXVolumetricLight.color;
            volLight.lightIntensity = seamothVFXVolumetricLight.lightIntensity;
            volLight.coneMat = seamothVFXVolumetricLight.coneMat;
            volLight.sphereMat = seamothVFXVolumetricLight.sphereMat;
            volLight.volumMesh = seamothVFXVolumetricLight.volumMesh;
            volLight.block = seamothVFXVolumetricLight.block;
            volLight.lightSource = parent.GetComponentInChildren<Light>();
            volLight.volumGO = lightCone;
            volLight.volumRenderer = lightCone.GetComponent<MeshRenderer>();
            volLight.volumMeshFilter = lightCone.GetComponent<MeshFilter>();
        }

        private static void ToggleLights(Exosuit exosuit)
        {
            Transform lightsTransform = Util.GetExosuitLightsTransform(exosuit);
            if (lightsTransform == null)
                return;

            //AddDebug("ToggleLights lightsTransform activeSelf " + lightsTransform.gameObject.activeSelf);
            //AddDebug("ToggleLights hasCharge " + exosuit.energyInterface.hasCharge);
            if (!lightsTransform.gameObject.activeSelf && exosuit.energyInterface.hasCharge)
            {
                lightsTransform.gameObject.SetActive(true);
                Main.configMain.DeleteExosuitLights(exosuit.gameObject);
                if (Exosuit_Sounds.lightOnSound)
                    Utils.PlayFMODAsset(Exosuit_Sounds.lightOnSound, exosuit.transform.position);
            }
            else if (lightsTransform.gameObject.activeSelf)
            {
                lightsTransform.gameObject.SetActive(false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                if (Exosuit_Sounds.lightOffSound)
                    Utils.PlayFMODAsset(Exosuit_Sounds.lightOffSound, exosuit.transform.position);
            }
            //AddDebug("lights " + lightsT.gameObject.activeSelf);
        }

        private static void SetLights(Exosuit exosuit, bool on)
        {
            if (on && !exosuit.energyInterface.hasCharge)
                return;

            Util.GetExosuitLightsTransform(exosuit).gameObject.SetActive(on);
            //AddDebug("SetLights " + active);
        }

        [HarmonyPatch(typeof(Exosuit))]
        class Exosuit_Patch
        {
            private static void FixExosuitLight(Exosuit exosuit)
            {
                Transform lightTransform = Util.GetExosuitLightsTransform(exosuit);
                Light[] Lights = lightTransform.GetComponentsInChildren<Light>();

                foreach (var light in Lights)
                {
                    //Main.logger.LogInfo("Exosuit light color " + light.color);
                    if (ConfigToEdit.exosuitLightIntensityMult.Value < 1)
                        light.intensity *= ConfigToEdit.exosuitLightIntensityMult.Value;

                    if (exosuitLightColor != default)
                        light.color = exosuitLightColor;

                    AddVolLight(light.gameObject, exosuitLightBeamPos);
                }
            }

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void AwakePostfix(Exosuit __instance)
            {
                Util.GetExosuitLightsTransform(__instance).SetParent(__instance.leftArmAttach);
                FixExosuitLight(__instance);
                if (Main.configMain.GetExosuitLights(__instance.gameObject))
                    SetLights(__instance, false);
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(Exosuit __instance)
            {
                if (Main.gameLoaded == false || Main.vehicleLightsImprovedLoaded)
                    return;

                if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.currentMountedVehicle == __instance)
                {
                    if (GameInput.GetButtonDown(GameInput.Button.MoveDown))
                        ToggleLights(__instance);
                }
            }

            [HarmonyPostfix, HarmonyPatch("EnterVehicle")]
            public static void EnterVehiclePostfix(Exosuit __instance)
            {
                CoroutineHost.StartCoroutine(DisableLightBeam(__instance));
            }

            static IEnumerator DisableLightBeam(Exosuit exosuit)
            {
                yield return new WaitUntil(() => Main.gameLoaded);
                ToggleLightBeam(exosuit, false);
            }

            static void ToggleLightBeam(Exosuit exosuit, bool on)
            {
                Transform lightT = Util.GetExosuitLightsTransform(exosuit);
                VFXVolumetricLight[] volLights = lightT.GetComponentsInChildren<VFXVolumetricLight>();
                foreach (var volL in volLights)
                {
                    if (on)
                        volL.RestoreVolume();
                    else
                        volL.DisableVolume();
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnPilotModeEnd")]
            public static void OnPlayerEnteredPostfix(Exosuit __instance)
            {
                ToggleLightBeam(__instance, true);
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        class VehicleDockingBay_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("OnUndockingStart")]
            public static void OnUndockingStartPostfix(VehicleDockingBay __instance)
            {
                Exosuit exosuit = __instance.dockedVehicle as Exosuit;
                if (exosuit)
                {
                    //AddDebug("OnUndockingStart");
                    SetLights(exosuit, true);
                }
            }

            [HarmonyPostfix, HarmonyPatch("DockVehicle")]
            public static void DockVehiclePostfix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //AddDebug("DockVehicle");
                Exosuit exosuit = vehicle as Exosuit;
                if (exosuit)
                    CoroutineHost.StartCoroutine(TurnOffLightsDelay(exosuit, 2));
            }

            public static IEnumerator TurnOffLightsDelay(Exosuit exosuit, float delay)
            {
                yield return new WaitForSeconds(delay);
                SetLights(exosuit, false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                //AddDebug("Set Lights off");
            }
        }

        [HarmonyPatch(typeof(SeaMoth))]
        class SeaMoth_patch
        {
            [HarmonyPrefix, HarmonyPatch("Awake")]
            public static void AwakePrefix(SeaMoth __instance)
            {
                GetSeaMothVolLight(__instance);
            }

            [HarmonyPrefix, HarmonyPatch("Start")]
            public static void StartPrefix(SeaMoth __instance)
            {
                FixSeamothLights(__instance);
            }

            private static void FixSeamothLights(SeaMoth __instance)
            {
                Transform lightParentTransform = __instance.transform.Find("lights_parent");
                Light[] lights = lightParentTransform.GetComponentsInChildren<Light>();

                for (int i = 0; i < lights.Length; i++)
                {
                    Light light = lights[i];
                    MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                    if (i == 0)
                        mr.transform.localPosition = new Vector3(0.05f, 0.05f, -0.9f);
                    else
                        mr.transform.localPosition = new Vector3(-0.05f, 0.05f, -0.9f);

                    if (ConfigToEdit.seamothLightIntensityMult.Value < 1)
                        light.intensity *= ConfigToEdit.seamothLightIntensityMult.Value;

                    if (seamothLightColor != default)
                        light.color = seamothLightColor;
                    //Main.logger.LogInfo("SeaMoth light color " + light.color);
                }
            }

            private static void GetSeaMothVolLight(SeaMoth seaMoth)
            {
                if (seamothLightCone != null)
                    return;

                Transform lightParentTransform = seaMoth.transform.Find("lights_parent");
                Light light = lightParentTransform.GetComponentInChildren<Light>();
                Transform fakeLightTransform = light.transform.Find("x_FakeVolumletricLight");
                seamothLightCone = fakeLightTransform.gameObject;
                seamothVFXVolumetricLight = light.GetComponent<VFXVolumetricLight>();
            }

        }
    }
}
