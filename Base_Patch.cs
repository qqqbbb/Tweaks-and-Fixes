using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Base_Patch
    {
        static int camerasToRemove = 0;


        public static void ToggleBaseLight(SubRoot subRoot)
        {
            //bool canToggle = subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline;
            //AddDebug(" ToggleBaseLight canToggle " + canToggle);
            //if (!canToggle)
            //    return;

            subRoot.subLightsOn = !subRoot.subLightsOn;
            int x = (int)subRoot.transform.position.x;
            int y = (int)subRoot.transform.position.y;
            int z = (int)subRoot.transform.position.z;
            StringBuilder stringBuilder = new StringBuilder(x.ToString());
            stringBuilder.Append("_");
            stringBuilder.Append(y);
            stringBuilder.Append("_");
            stringBuilder.Append(z);
            //string key = x + "_" + y + "_" + z;
            string key = stringBuilder.ToString();
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (Main.config.baseLights.ContainsKey(currentSlot))
            {
                Main.config.baseLights[currentSlot][key] = subRoot.subLightsOn;
                //AddDebug(" ToggleBaseLight " + key + " " + subRoot.subLightsOn);
            }
            else
            {
                Main.config.baseLights[currentSlot] = new Dictionary<string, bool>();
                Main.config.baseLights[currentSlot][key] = subRoot.subLightsOn;
                //AddDebug(" ToggleBaseLight " + key + " " + subRoot.subLightsOn);
            }
        }

        [HarmonyPatch(typeof(SubRoot), "Awake")]
        public static class SubRoot_Awake_Patch
        {
            static void Postfix(SubRoot __instance)
            {
                //Light[] lights = __instance.GetComponentsInChildren<Light>();
                if (__instance.isBase)
                {
                    //bool canToggle = __instance.powerRelay && __instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Normal;
                    //AddDebug(__instance.name + " canToggle " + canToggle);
                    //if (!canToggle)
                    //    return;

                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    string key = x + "_" + y + "_" + z;
                    string currentSlot = SaveLoadManager.main.currentSlot;
                    //AddDebug("find BaseLight " + currentSlot + " key " + key);
                    if (Main.config.baseLights.ContainsKey(currentSlot) && Main.config.baseLights[currentSlot].ContainsKey(key))
                    {
                        __instance.subLightsOn = Main.config.baseLights[currentSlot][key];
                        //AddDebug("saved BaseLight " + key + " " + __instance.subLightsOn);
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(BaseDeconstructable), "Awake")]
        class BaseDeconstructable_Awake_Patch
        {
            static void Postfix(BaseDeconstructable __instance)
            { // recipe is None
                AddDebug("BaseDeconstructable Awake " + __instance.recipe);
                if (__instance.recipe == TechType.BaseMoonpool)
                {

                }
            }
        }

        [HarmonyPatch(typeof(BaseWaterPlane), "Awake")]
        class BaseWaterPlane_Awake_Patch
        {
            static void Postfix(BaseWaterPlane __instance)
            {
                if (__instance.transform.parent.name == "BaseMoonpool(Clone)")
                {
                    Transform t = __instance.transform.Find("x_BaseWaterPlane");
                    if (t)
                        t.localScale = new Vector3(1f, 1f, .98f);
                }
            }
        }

        [HarmonyPatch(typeof(MapRoomCamera), "Start")]
        class MapRoomCamera_Start_Patch
        {
            static bool Prefix(MapRoomCamera __instance)
            {
                //AddDebug(" MapRoomCamera Start ");
                //if (__instance.dockingPoint)
                //    AddDebug(" dockingPoint ");
                if (camerasToRemove > 0)
                {
                    //AddDebug(" Destroy camera ");
                    UnityEngine.Object.Destroy(__instance.gameObject);
                    camerasToRemove--;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Constructable), "Construct")]
        internal class Constructable_Construct_Patch
        {
            public static void Postfix(Constructable __instance)
            {
                if (__instance.constructedAmount >= 1f)
                {
                    //AddDebug(" constructed " + __instance.techType);
                    if (Main.config.mapRoomFreeCameras == false && __instance.techType == TechType.BaseMapRoom)
                        camerasToRemove = 2;
                    else
                        camerasToRemove = 0;
                }
            }
        }

        [HarmonyPatch(typeof(SolarPanel), "OnHandHover")]
        public static class SolarPanel_OnHandHover_Patch
        {
            static bool Prefix(SolarPanel __instance, GUIHand hand)
            {
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (!c || !c.constructed)
                    return false;
                HandReticle.main.SetInteractText(Language.main.GetFormat<int, int, int>("SolarPanelStatus", Mathf.RoundToInt(__instance.GetRechargeScalar() * 100f), Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
                //HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
        }

        [HarmonyPatch(typeof(BaseUpgradeConsoleGeometry), "GetVehicleInfo")]
        public class BaseUpgradeConsoleGeometry_GetVehicleInfo_Patch
        {
            static bool Prefix(BaseUpgradeConsoleGeometry __instance, Vehicle vehicle, ref string __result)
            {
                if (vehicle == null)
                {
                    __result = "";
                    return false;
                }
                return true;
            }
        }




    }
}