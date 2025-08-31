using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    // only IsLeaking works to check if base is flooded
    public class Base_
    {
        static int camerasToRemove = 0;
        static Dictionary<BaseHullStrength, SubRoot> baseHullStrengths = new Dictionary<BaseHullStrength, SubRoot>();
        static HashSet<ModularStairs> fixedStairs = new HashSet<ModularStairs>();

        public static void CleanUp()
        {
            fixedStairs.Clear();
            baseHullStrengths.Clear();

        }

        public static void ToggleBaseLight(SubRoot subRoot)
        {
            subRoot.subLightsOn = !subRoot.subLightsOn;
            if (subRoot.subLightsOn)
                Main.configMain.DeleteBaseLights(subRoot.transform.position);
            else
                Main.configMain.SaveBaseLights(subRoot.transform.position);
        }

        [HarmonyPatch(typeof(SubRoot), "Awake")]
        public static class SubRoot_Awake_Patch
        {
            static void Postfix(SubRoot __instance)
            {
                //AddDebug(__instance.name + " SubRoot Awake " + __instance.isBase);
                //Light[] lights = __instance.GetComponentsInChildren<Light>();
                if (__instance.isBase)
                {
                    __instance.subLightsOn = Main.configMain.GetBaseLights(__instance.transform.position);
                    //AddDebug("saved BaseLight " + key + " " + __instance.subLightsOn);
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

                if (!baseHullStrengths.ContainsKey(__instance))
                {
                    baseHullStrengths[__instance] = __instance.GetComponent<SubRoot>();
                }
                else if (baseHullStrengths[__instance] == Player.main.currentSub)
                {
                    //AddDebug("Player inside");
                    if (__instance.crushSounds[index] != null)
                        Utils.PlayFMODAsset(__instance.crushSounds[index], random.transform);

                    AddMessage(Language.main.GetFormat("BaseHullStrDamageDetected", __instance.totalStrength));
                }
                return false;
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

        [HarmonyPatch(typeof(Constructable))]
        class Constructable_Construct_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("NotifyConstructedChanged")]
            public static void Postfix(Constructable __instance, bool constructed)
            {
                if (!constructed || !Main.gameLoaded)
                    return;

                //AddDebug(" NotifyConstructedChanged " + __instance.techType);
                if (!ConfigToEdit.mapRoomFreeCameras.Value && __instance.techType == TechType.BaseMapRoom)
                    camerasToRemove = 2;
                else
                    camerasToRemove = 0;
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

        [HarmonyPatch(typeof(FMOD_CustomEmitter), "Awake")]
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

        [HarmonyPatch(typeof(SolarPanel), "Start")]
        class SolarPanel_Patch
        {
            static void Prefix(SolarPanel __instance)
            {
                __instance.maxDepth = ConfigToEdit.solarPanelMaxDepth.Value;
            }
        }


    }



}