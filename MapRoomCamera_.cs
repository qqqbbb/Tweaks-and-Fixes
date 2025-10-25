using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(MapRoomCamera))]
    internal class MapRoomCamera_
    {
        public static Color lightColor;
        static Vector3 cameraVolLightPos = new Vector3(-0.1f, 0.27f, -0.6f);

        [HarmonyPostfix, HarmonyPatch("Start")]
        private static void StartPostfix(MapRoomCamera __instance)
        {
            Light[] lights = __instance.lightsParent.GetComponentsInChildren<Light>();
            foreach (var light in lights)
            {
                if (ConfigToEdit.cameraLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.cameraLightIntensityMult.Value;

                if (lightColor != default)
                    light.color = lightColor;

                //Main.logger.LogMessage("MapRoomCamera light color " + light.color);
            }
            VehicleLightFix.AddVolLight(lights[1].gameObject, cameraVolLightPos);
        }
        [HarmonyPostfix, HarmonyPatch("ControlCamera")]
        private static void ControlCameraPostfix(MapRoomCamera __instance)
        {
            //AddDebug("MapRoomCamera ControlCamera");
            VFXVolumetricLight volLight = __instance.lightsParent.GetComponentInChildren<VFXVolumetricLight>();
            volLight?.DisableVolume();
        }
        [HarmonyPostfix, HarmonyPatch("FreeCamera")]
        private static void FreeCameraPostfix(MapRoomCamera __instance)
        {
            //AddDebug("MapRoomCamera FreeCamera");
            VFXVolumetricLight volLight = __instance.lightsParent.GetComponentInChildren<VFXVolumetricLight>();
            volLight?.RestoreVolume();
        }
    }

}
