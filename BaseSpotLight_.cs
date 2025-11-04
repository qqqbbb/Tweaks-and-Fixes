using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(BaseSpotLight))]
    internal class BaseSpotLight_
    {
        public static Color lightColor;

        [HarmonyPostfix, HarmonyPatch("Start")]
        static void StartPostfix(BaseSpotLight __instance)
        {
            Light light = __instance.light.GetComponent<Light>();
            if (ConfigToEdit.spotlightLightIntensityMult.Value < 1)
                light.intensity *= ConfigToEdit.spotlightLightIntensityMult.Value;

            if (lightColor != default)
            { // no VFXVolumetricLight
                light.color = lightColor;
                MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                //0.373 0.463 0.502 0.502
                mr.material.color = new Color(lightColor.r * .5f, lightColor.g * .5f, lightColor.b * .5f, lightColor.a * .5f);
                //Main.logger.LogInfo("BaseSpotLight beam Color " + mr.material.color);
            }
        }

    }
}
