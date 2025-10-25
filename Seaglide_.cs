using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Seaglide))]
    internal class Seaglide_
    {
        public static Color lightColor;
        public static Vector3 lightPosLeft = new Vector3(-.1f, 0, 0);
        public static Vector3 lightPosRight = new Vector3(.1f, 0, 0);

        public static void SaveSeaglideState(Seaglide seaglide)
        {
            var seaglideMap = seaglide.GetComponent<VehicleInterface_MapController>();
            if (seaglideMap && seaglideMap.miniWorld)
            {
                if (seaglideMap.miniWorld.active)
                    Main.configMain.DeleteSeaglideMap(seaglide.gameObject);
                else
                    Main.configMain.SaveSeaglideMap(seaglide.gameObject);
            }
            if (seaglide.toggleLights)
            {
                if (seaglide.toggleLights.lightsActive)
                    Main.configMain.SaveSeaglideLights(seaglide.gameObject);
                else
                    Main.configMain.DeleteSeaglideLights(seaglide.gameObject);
            }
        }

        public static IEnumerator LoadSeaglideState(Seaglide seaglide)
        {
            if (seaglide == null)
                yield break;

            if (seaglide.toggleLights == null)
                yield return null;

            seaglide.toggleLights.SetLightsActive(Main.configMain.GetSeaglideLights(seaglide.gameObject));
            var map = seaglide.GetComponent<VehicleInterface_MapController>();
            if (map == null)
                yield break;

            if (map.miniWorld == null)
                yield return null;

            map.miniWorld.active = Main.configMain.GetSeaglideMap(seaglide.gameObject);
        }

        [HarmonyPostfix, HarmonyPatch("Start")]
        public static void StartPostfix(Seaglide __instance)
        {// fires after onLightsToggled
            //AddDebug("Seaglide Start");
            CoroutineHost.StartCoroutine(LoadSeaglideState(__instance));

            if (lightColor == default && ConfigToEdit.seaglideLightIntensityMult.Value == 1)
                return;

            Transform t = __instance.transform.Find("lights_parent");
            Light[] lights = t.GetComponentsInChildren<Light>();
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                if (i == 0)
                    VehicleLightFix.AddVolLight(light.gameObject, lightPosLeft);
                else
                    VehicleLightFix.AddVolLight(light.gameObject, lightPosRight);

                if (lightColor != default)
                    light.color = lightColor;

                //Main.logger.LogInfo("Seaglide light color " + light.color);
                if (ConfigToEdit.seaglideLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.seaglideLightIntensityMult.Value;
            }
        }

        [HarmonyPostfix, HarmonyPatch("OnDraw")]
        public static void OnDrawPostfix(Seaglide __instance)
        { // fires before Start
            CoroutineHost.StartCoroutine(DisableVolLight(__instance));
        }

        public static IEnumerator DisableVolLight(Seaglide seaglide)
        {
            bool thisFrame = true;
            if (thisFrame)
            {
                thisFrame = false;
                yield return null;
            }
            ToggleVolLight(seaglide, false);
        }

        private static void ToggleVolLight(Seaglide seaglide, bool enable)
        {
            Transform t = seaglide.transform.Find("lights_parent");
            Light[] lights = t.GetComponentsInChildren<Light>();
            foreach (var light in lights)
            {
                VFXVolumetricLight volLight = light.GetComponentInChildren<VFXVolumetricLight>();
                if (volLight)
                {
                    //AddDebug("Seaglide DisableVolume");
                    if (enable)
                        volLight.RestoreVolume();
                    else
                        volLight.DisableVolume();
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch("OnHolster")]
        public static void OnHolsterPostfix(Seaglide __instance)
        { // fires when saving, after nautilus SaveEvent
            ToggleVolLight(__instance, true);
            SaveSeaglideState(__instance);
        }
    }

}
