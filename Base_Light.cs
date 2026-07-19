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
        public static Color spotlightColor;
        public static Color spotlightWrongColor = new Color(0.779f, 0.89f, 1f, 1f);
        public static Color spotlightDefaultColor = new Color(0.373f, 0.463f, 0.502f, 1f);


        [HarmonyPatch(typeof(BaseSpotLight), "Start")]
        internal class BaseSpotLight__
        {
            public static void Postfix(BaseSpotLight __instance)
            {
                Light light = __instance.light.GetComponent<Light>();
                //Main.logger.LogError("BaseSpotLight light.intensity " + light.intensity);
                //Main.logger.LogDebug("BaseSpotLight light " + light.color);
                //Main.logger.LogDebug("BaseSpotLight spotlightColor " + spotlightColor);

                if (ConfigToEdit.spotlightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.spotlightIntensityMult.Value;

                //Main.logger.LogError("BaseSpotLight MeshRenderer material " + mr.material.color);
                if (spotlightColor == spotlightWrongColor || spotlightColor == spotlightDefaultColor)
                {
                    //AddDebug("spotlightLightWrongColor == spotlightLightColor)");
                }
                else if (spotlightColor != default)
                { // no VFXVolumetricLight
                    MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                    light.color = spotlightColor;
                    mr.material.color = new Color(spotlightColor.r, spotlightColor.g, spotlightColor.b, mr.material.color.a);
                }
            }
        }

        public static void ToggleBaseLight(SubRoot subRoot)
        {
            subRoot.subLightsOn = !subRoot.subLightsOn;
            if (subRoot.subLightsOn)
                Main.configMain.DeleteBaseLights(subRoot.transform.position);
            else
                Main.configMain.SaveBaseLights(subRoot.transform.position);
        }

    }
}
