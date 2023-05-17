using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static ErrorMessage;
using static VFXParticlesPool;

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
                if (!constructed || !Main.loadingDone)
                    return;

                //AddDebug(" NotifyConstructedChanged " + __instance.techType);
                //AddDebug(" NotifyConstructedChanged isPlacing " + Builder.isPlacing);
                if (!Main.config.builderPlacingWhenFinishedBuilding)
                    Player.main.StartCoroutine(BuilderEnd(2));
                
                if (!Main.config.mapRoomFreeCameras && __instance.techType == TechType.BaseMapRoom)
                    camerasToRemove = 2;
                else
                    camerasToRemove = 0;
            }
        }

        static IEnumerator BuilderEnd(int waitFrames)
        {
            //AddDebug("BuilderEnd start ");
            //yield return new WaitForSeconds(waitTime);
            while (waitFrames > 0)
            {
                waitFrames--;
                yield return null;
            }
            Builder.End();
            //AddDebug("BuilderEnd end ");
        }

        [HarmonyPatch(typeof(SolarPanel), "OnHandHover")]
        public static class SolarPanel_OnHandHover_Patch
        {
            static bool Prefix(SolarPanel __instance, GUIHand hand)
            {
                if (!Main.config.newUIstrings)
                    return true;

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
                if (Main.config.silentReactor && __instance.asset && __instance.asset.path == "event:/sub/base/nuke_gen_loop")
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

            [HarmonyPrefix]
            [HarmonyPatch("OnUpdate")]
            static bool Prefix(Bench __instance)
            {
                if (__instance.currentPlayer == null)
                    return false;

                if (__instance.isSitting)
                {
                    if (__instance.currentPlayer.GetPDA().isInUse)
                        return false;

                    if (GameInput.GetButtonDown(GameInput.Button.Exit))
                        __instance.ExitSittingMode(__instance.currentPlayer);

                    HandReticle.main.SetText(HandReticle.TextType.Use, "StandUp", true, GameInput.Button.Exit);
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
                else
                {
                    __instance.Subscribe(__instance.currentPlayer, false);
                    __instance.currentPlayer = null;
                }
                return false;
            }


        }

        [HarmonyPatch(typeof(SolarPanel), "Start")]
        class SolarPanel_Patch
        {
            static void Prefix(SolarPanel __instance)
            {
                __instance.maxDepth = Main.config.solarPanelMaxDepth;
                //Main.logger.LogInfo(" SolarPanel Start " + Ocean.GetDepthOf(__instance.gameObject) + " DepthScalar " + __instance.GetDepthScalar() + " SunScalar " + __instance.GetSunScalar());
                //AddDebug(" SolarPanel Start " + Ocean.GetDepthOf(__instance.gameObject) + " DepthScalar " + __instance.GetDepthScalar() + " SunScalar " + __instance.GetSunScalar());
                //for (int i = 0; i <= 100; i++)
                {
                    //Main.logger.LogInfo(" SolarPanel " + i + " " + __instance.depthCurve.Evaluate(Mathf.Clamp01((100f - (float)i) / 100f)));
                    //Main.logger.LogInfo(" SolarPanel " + 100 + " " + __instance.depthCurve.Evaluate(Mathf.Clamp01((__instance.maxDepth - Ocean.GetDepthOf(__instance.gameObject)) / __instance.maxDepth)));
                }
            }
        }



    }
}