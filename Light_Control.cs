using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Light_Control
    {
        public static Dictionary<TechType, float> lightIntensityStep = new Dictionary<TechType, float>();
        public static Dictionary<TechType, float> lightOrigIntensity = new Dictionary<TechType, float>();
        public static GameInput.Button lightButton;
        public static Light[] currentLights = new Light[2];

        public static void SaveLightIntensity(TechType tt, float f)
        {
            Main.configMain.lightIntensity[tt.ToString()] = f;
        }

        public static bool IsLightSaved(TechType tt)
        {
            return Main.configMain.lightIntensity.ContainsKey(tt.ToString());
        }

        public static float GetLightIntensity(TechType tt)
        {
            return Main.configMain.lightIntensity[tt.ToString()];
        }

        public static void UpdateLights()
        {
            //AddDebug("UpdateLights " + currentLights.Length);
            if (currentLights == null || currentLights.Length == 0 || currentLights[0] == null || currentLights[0].gameObject == null || !currentLights[0].gameObject.activeInHierarchy)
                return;

            if (!Input.GetKey(ConfigMenu.lightButton.Value))
                return;

            TechType vehTT = Vehicle_patch.currentVehicleTT;
            //Light[] lights = __instance.GetComponentsInChildren<Light>();
            //AddDebug("lights.Length  " + currentLights[0].gameObject.activeInHierarchy);
            if (!lightIntensityStep.ContainsKey(vehTT))
            {
                AddDebug("lightIntensityStep missing " + vehTT);
                return;
            }
            if (!lightOrigIntensity.ContainsKey(vehTT))
            {
                AddDebug("lightOrigIntensity missing " + vehTT);
                return;
            }
            float step = 0f;
            //AddDebug("UpdateLights currentVehicleTT " + currentVehicleTT);
            if (GameInput.GetButtonDown(GameInput.Button.CycleNext))
                step = lightIntensityStep[vehTT];
            else if (GameInput.GetButtonDown(GameInput.Button.CyclePrev))
                step = -lightIntensityStep[vehTT];

            if (step == 0f)
                return;

            foreach (Light l in currentLights)
            {
                if (step > 0 && l.intensity > lightOrigIntensity[vehTT])
                    return;

                l.intensity += step;
                //AddDebug("Light Intensity " + l.intensity);
                SaveLightIntensity(vehTT, l.intensity);
            }
        }

        [HarmonyPatch(typeof(Vehicle))]
        public class Vehicle_patch_
        {
            [HarmonyPostfix]
            [HarmonyPatch("EnterVehicle")]
            public static void EnterVehiclePostfix(Vehicle __instance)
            {
                if (__instance is Exosuit) { }
                else
                {
                    Transform lightT = __instance.transform.Find("lights_parent");
                    if (lightT)
                        currentLights = lightT.GetComponentsInChildren<Light>(true);
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnPilotModeEnd")]
            public static void OnPilotModeEndPostfix(Vehicle __instance)
            {
                currentLights[0] = null;
                //AddDebug("Vehicle OnPilotModeEnd " + currentLights.Length);
            }

        }
        [HarmonyPatch(typeof(QuickSlots))]
        class QuickSlots_Bind_Patch
        {
            [HarmonyPrefix, HarmonyPatch("SlotNext")]
            public static bool SlotNextPrefix(QuickSlots __instance)
            {
                //AddDebug("SlotNext");
                if (Input.GetKey(ConfigMenu.lightButton.Value) || GameInput.GetButtonHeld(lightButton))
                {
                    HandleLight(false);
                    return false;
                }
                return true;
            }

            private static void HandleLight(bool reduce)
            {
                Pickupable p = Inventory.main.GetHeld();
                if (p == null)
                    return;

                TechType tt = CraftData.GetTechType(p.gameObject);
                //AddDebug("lights TechType " + tt);
                if (tt == TechType.DiveReel || tt == TechType.LaserCutter)
                    return;

                Light[] lights = p.GetComponentsInChildren<Light>();
                //AddDebug("lights.Length  " + lights.Length);
                if (lights.Length == 0 || !lights[0].gameObject.activeInHierarchy)
                    return;

                if (!lightIntensityStep.ContainsKey(tt))
                {
                    AddDebug("lightIntensityStep missing " + tt);
                    return;
                }
                if (!lightOrigIntensity.ContainsKey(tt))
                {
                    AddDebug("lightOrigIntensity missing " + tt);
                    return;
                }
                float origIntensity = lightOrigIntensity[tt];
                Flare flare = p.GetComponent<Flare>();
                if (flare && flare.flareActivateTime == 0)
                    return;

                float step = lightIntensityStep[tt];
                foreach (Light l in lights)
                {
                    if (!reduce)
                    {
                        if (l.intensity + step > origIntensity)
                            l.intensity = origIntensity;
                        else
                            l.intensity += step;
                    }
                    else // light intensity will not go below 0
                        l.intensity -= step;

                    if (flare)
                        flare.originalIntensity = l.intensity;

                    //AddDebug("Light Intensity " + l.intensity.ToString("0.0"));
                    SaveLightIntensity(tt, l.intensity);
                }
            }

            [HarmonyPrefix, HarmonyPatch("SlotPrevious")]
            public static bool SlotPreviousPrefix(QuickSlots __instance)
            {
                if (Input.GetKey(ConfigMenu.lightButton.Value) || GameInput.GetButtonHeld(lightButton))
                {
                    HandleLight(true);
                    return false;
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(MapRoomScreen), "CycleCamera")]
        class MapRoomScreen_CycleCamera_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CycleCamera")]
            static bool CycleCameraPrefix(MapRoomScreen __instance, int direction)
            {
                if (!Input.GetKey(ConfigMenu.lightButton.Value))
                    return true;

                if (currentLights.Length == 0)
                {
                    //AddDebug("lights.Length == 0 ");
                    return true;
                }
                if (!lightIntensityStep.ContainsKey(TechType.MapRoomCamera))
                {
                    AddDebug("lightIntensityStep missing " + TechType.MapRoomCamera);
                    return false;
                }
                if (!lightOrigIntensity.ContainsKey(TechType.MapRoomCamera))
                {
                    AddDebug("lightOrigIntensity missing " + TechType.MapRoomCamera);
                    return false;
                }
                float step = lightIntensityStep[TechType.MapRoomCamera];
                if (direction < 0)
                    step = -step;

                foreach (Light l in currentLights)
                {
                    if (step > 0 && l.intensity > lightOrigIntensity[TechType.MapRoomCamera])
                        return false;

                    l.intensity += step;
                    //AddDebug("Light Intensity " + l.intensity);
                    SaveLightIntensity(TechType.MapRoomCamera, l.intensity);
                }
                return false;
            }
        }



    }
}
