using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Base_Light
    {
        public static Color spotlightLightColor;
        public static Color vehicleDockingBayLightColor;
        static Vector3[] vehicleDockingBayLightBeamPos = new Vector3[] { new Vector3(0, 0, -0.95f), new Vector3(0, 0, -1.25f), new Vector3(0, 0, -1.17f), new Vector3(0, 0, -0.93f) };
        static Vector3 vehicleDockingBayLightScale = new Vector3(40f, 40f, 40f);

        [HarmonyPatch(typeof(BaseSpotLight), "Start")]
        internal class BaseSpotLight__
        {
            public static void Postfix(BaseSpotLight __instance)
            {
                Light light = __instance.light.GetComponent<Light>();
                //Main.logger.LogError("BaseSpotLight light.intensity " + light.intensity);
                //Main.logger.LogError("BaseSpotLight light.a " + light.color.a);

                if (ConfigToEdit.spotlightLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.spotlightLightIntensityMult.Value;

                if (spotlightLightColor != default)
                { // no VFXVolumetricLight
                    light.color = spotlightLightColor;
                    MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                    mr.material.color = new Color(spotlightLightColor.r, spotlightLightColor.g, spotlightLightColor.b, mr.material.color.a);
                }
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        public class VehicleDockingBay_Patch
        {
            public static Dictionary<VehicleDockingBay, PowerSystem.Status> savedPowerStatus = new Dictionary<VehicleDockingBay, PowerSystem.Status>();

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(VehicleDockingBay __instance)
            {
                //if (__instance.transform.parent.parent.name != "BaseMoonpool(Clone)")
                //    return;
                if (__instance.subRoot == null || __instance.subRoot.isCyclops)
                    return;

                //AddDebug("VehicleDockingBay Start " + __instance.transform.parent.parent.name);
                CoroutineHost.StartCoroutine(FixVehicleDockingBayLights(__instance));
            }

            static IEnumerator FixVehicleDockingBayLights(VehicleDockingBay vehicleDockingBay)
            {
                yield return new WaitUntil(() => Main.gameLoaded);
                //Main.logger.LogMessage("FixVehicleDockingBayLights " + vehicleDockingBay.name);
                //AddDebug("FixVehicleDockingBayLights " + vehicleDockingBay.name);
                List<Light> lights = GetPillarLights(vehicleDockingBay);
                if (lights == null || lights.Count == 0)
                    yield break;

                //AddDebug("FixVehicleDockingBayLights  " + lights.Count);
                for (int i = 0; i < lights.Count; i++)
                {// no VFXVolumetricLight
                    Light light = lights[i];
                    Vector3 lightBeamPos = vehicleDockingBayLightBeamPos[i];
                    VehicleLightFix.AddVolLight(light.gameObject, lightBeamPos, vehicleDockingBayLightScale);
                    //Main.logger.LogInfo("VehicleDockingBay lightColor " + light.color);
                    light.range = 20;
                    if (vehicleDockingBayLightColor != default) // 0.361, 1.000, 1.000
                        light.color = vehicleDockingBayLightColor;

                    //Main.logger.LogInfo("VehicleDockingBay light intensity " + light.intensity);
                    if (ConfigToEdit.vehicleDockingBayLightIntensityMult.Value < 1) // 1.73
                        light.intensity *= ConfigToEdit.vehicleDockingBayLightIntensityMult.Value;
                }
            }

            private static List<Light> GetPillarLights(VehicleDockingBay vehicleDockingBay)
            {
                //AddDebug("GetPillarLights " + vehicleDockingBay.transform.parent.parent.name);
                Transform pillars = vehicleDockingBay.transform.parent.parent.Find("pillars");
                if (pillars == null)
                    return null;

                List<Light> lights = new List<Light>();
                foreach (Transform pillar in pillars.transform)
                {
                    //AddDebug("GetPillarLights pillar " + pillar.name + " activeSelf " + pillar.gameObject.activeSelf);
                    Transform lightT = pillar.Find("light");
                    if (lightT != null)
                        lights.Add(lightT.GetComponent<Light>());
                }
                return lights;
            }

            [HarmonyPostfix, HarmonyPatch("LateUpdate")]
            public static void LateUpdatePostfix(VehicleDockingBay __instance)
            {
                if (Main.gameLoaded == false || __instance.subRoot == null || __instance.subRoot.isCyclops)
                    return;

                //AddDebug("VehicleDockingBay LateUpdate  " + __instance.name);
                PowerSystem.Status currentStatus = __instance.powerRelay.powerStatus;
                if (!savedPowerStatus.ContainsKey(__instance) || currentStatus != savedPowerStatus[__instance])
                {
                    //AddDebug("VehicleDockingBay Update lights ");
                    savedPowerStatus[__instance] = currentStatus;
                    List<Light> lights = GetPillarLights(__instance);
                    if (lights == null || lights.Count == 0)
                        return;

                    bool on = currentStatus != PowerSystem.Status.Offline;
                    foreach (Light light in lights)
                    {
                        if (on == light.gameObject.activeSelf)
                            continue;

                        light.gameObject.SetActive(on);
                    }
                }
            }
        }


    }
}
