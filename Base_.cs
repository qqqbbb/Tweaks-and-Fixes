using FMOD;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    public class Base_
    {
        [HarmonyPatch(typeof(SubRoot))]
        public static class SubRoot_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Awake")]
            static void Postfix(SubRoot __instance)
            {
                //AddDebug(__instance.name + " SubRoot Awake " + __instance.isBase);
                if (__instance.isBase)
                {
                    __instance.subLightsOn = Main.configMain.GetBaseLights(__instance.transform.position);
                    //AddDebug("saved BaseLight " + key + " " + __instance.subLightsOn);
                    __instance.interiorSky.AffectedByDayNightCycle = ConfigToEdit.baseSunlight.Value;
                }
            }
            //[HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(SubRoot __instance)
            {
                //AddDebug("SubRoot Start " + __instance.isBase);
                if (__instance.isBase)
                {
                    __instance.interiorSky.AffectedByDayNightCycle = true;
                }
            }
        }

        [HarmonyPatch(typeof(BaseHullStrength))]
        class BaseHullStrength_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnPostRebuildGeometry")]
            static bool OnPostRebuildGeometryPrefix(BaseHullStrength __instance)
            {
                if (!GameModeUtils.RequiresReinforcements())
                    return false;

                if (ConfigMenu.baseHullStrengthMult.Value == 1)
                    return true;

                float strength = BaseHullStrength.InitialStrength * ConfigMenu.baseHullStrengthMult.Value;
                __instance.victims.Clear();
                foreach (Int3 cell in __instance.baseComp.AllCells)
                {
                    if (__instance.baseComp.GridToWorld(cell).y < 0)
                    {
                        //int cellIndex = __instance.baseComp.GetCellIndex(cell);
                        //AddDebug("OnPostRebuildGeometry cell " + __instance.baseComp.cells[cellIndex]);
                        Transform cellObject = __instance.baseComp.GetCellObject(cell);
                        if (cellObject != null)
                        {
                            //AddDebug("OnPostRebuildGeometry cellObject " + cellObject.name);
                            __instance.victims.Add(cellObject.GetComponent<LiveMixin>());
                            strength += __instance.baseComp.GetHullStrength(cell);
                        }
                    }
                }
                if (!WaitScreen.IsWaiting && !Mathf.Approximately(strength, __instance.totalStrength))
                    AddMessage(Language.main.GetFormat("BaseHullStrChanged", strength - __instance.totalStrength, strength));

                __instance.totalStrength = strength;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("CrushDamageUpdate")]
            static bool CrushDamageUpdatePrefix(BaseHullStrength __instance)
            {
                if (!GameModeUtils.RequiresReinforcements() || __instance.totalStrength >= 0 || __instance.victims.Count <= 0)
                    return false;

                LiveMixin random = __instance.victims.GetRandom();
                random.TakeDamage(BaseHullStrength.damagePerCrush, random.transform.position, DamageType.Pressure);
                int index = 0;
                if (__instance.totalStrength <= -3.0)
                    index = 2;
                else if (__instance.totalStrength <= -2.0)
                    index = 1;

                if (__instance.GetComponent<SubRoot>() == Player.main.currentSub)
                {
                    //AddDebug("Player inside");
                    if (__instance.crushSounds[index] != null)
                        Utils.PlayFMODAsset(__instance.crushSounds[index], random.transform);

                    AddMessage(Language.main.GetFormat("BaseHullStrDamageDetected", __instance.totalStrength));
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(MapRoomCameraDocking), "Start")]
        class MapRoomCameraDocking_Start_Patch
        {
            static bool Prefix(MapRoomCameraDocking __instance)
            {
                return ConfigToEdit.mapRoomFreeCameras.Value;
            }
        }

        [HarmonyPatch(typeof(SolarPanel))]
        class SolarPanel_Patch
        {
            [HarmonyPrefix, HarmonyPatch("Start")]
            static void StartPrefix(SolarPanel __instance)
            {
                __instance.maxDepth = ConfigToEdit.solarPanelMaxDepth.Value;
            }
            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            static bool Prefix(SolarPanel __instance, GUIHand hand)
            {
                Constructable c = __instance.gameObject.GetComponent<Constructable>();
                if (!c || !c.constructed)
                    return false;

                HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int, int>("SolarPanelStatus", Mathf.RoundToInt(__instance.GetRechargeScalar() * 100f), Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
                //HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                //HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
        }


        //[HarmonyPatch(typeof(BaseUpgradeConsoleGeometry), "GetVehicleInfo")] 
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

        //[HarmonyPatch(typeof(FMOD_CustomEmitter), "Awake")]
        class FMOD_CustomEmitter_Awake_Patch
        {
            static bool Prefix(FMOD_CustomEmitter __instance)
            {
                if (ConfigToEdit.silentReactor.Value && __instance.asset && __instance.asset.path == "event:/sub/base/nuke_gen_loop")
                {
                    //AddDebug(__instance.name + " FMOD_CustomEmitter Awake ");
                    __instance.asset = null;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Bench))]
        class Bench_Patch
        {
            private static float chairRotSpeed = 70f;

            [HarmonyPostfix, HarmonyPatch("OnUpdate")]
            static void OnUpdatePostfix(Bench __instance)
            {
                if (__instance.currentPlayer == null || __instance.isSitting == false || __instance.currentPlayer.GetPDA().isInUse)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt == TechType.StarshipChair)
                {
                    HandReticle.main.SetText(HandReticle.TextType.UseSubscript, UI_Patches.swivelText, false);
                    if (GameInput.GetButtonHeld(GameInput.Button.MoveRight))
                        __instance.transform.Rotate(Vector3.up * chairRotSpeed * Time.deltaTime);
                    else if (GameInput.GetButtonHeld(GameInput.Button.MoveLeft))
                        __instance.transform.Rotate(-Vector3.up * chairRotSpeed * Time.deltaTime);
                }
            }
        }


        [HarmonyPatch(typeof(BaseUpgradeConsoleGeometry), "GetVehicleInfo")]
        class BaseUpgradeConsoleGeometry_GetVehicleInfo_patch
        {
            public static void Postfix(BaseUpgradeConsoleGeometry __instance, Vehicle vehicle, ref string __result)
            {
                if (vehicle is Exosuit)
                {// show correct value if left battery is removed
                    float energyScalar = vehicle.GetComponent<EnergyMixin>().GetEnergyScalar();
                    //AddDebug("EnergyMixin energyScalar " + energyScalar);
                    if (energyScalar == 0)
                    {
                        vehicle.GetEnergyValues(out float charge, out float capacity);
                        energyScalar = charge / capacity;
                        int energyPercent = (int)(energyScalar * 100f);
                        //AddDebug(" energyPercent " + energyPercent);
                        if (energyPercent == 100)
                        {
                            __result = $"{__result.Substring(0, __result.Length - 18)}{Language.main.Get("SubmersibleFullyCharged")}";
                            return;
                        }
                        __result = $"{__result.Substring(0, __result.Length - 9)}{energyPercent}%</size>";
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(BaseDeconstructable))]
        class BaseDeconstructable_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("Init")]
            static void InitPostfix(BaseDeconstructable __instance)
            {
                //AddDebug("BaseDeconstructable Init " + __instance.recipe);
                if (__instance.recipe == TechType.BaseHatch && __instance.transform.position.y > Ocean.GetOceanLevel())
                    FixHatchTextureZfighting(__instance.gameObject);
            }

            private static void FixHatchTextureZfighting(GameObject hatch)
            {
                string[] names = new string[] { "BaseCorridorHatch/aboveWater", "aboveWater" };

                foreach (string name in names)
                {
                    Transform child = hatch.transform.Find(name);
                    if (child == null || child.gameObject.activeSelf == false)
                        continue;

                    child = child.transform.Find("model/BaseCorridorBulkhead(Clone) (6)/models/BaseCorridorInteriorWallHatch_Split/BaseCorridorInteriorWallHatch_Front");
                    if (child)
                        child.localPosition = new Vector3(0, -0.002f, 0);
                }
            }
        }
    }



}