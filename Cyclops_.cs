using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Cyclops_
    {
        static CyclopsEntryHatch cyclopsEntryHatch;
        public static Color cyclopsLightColor;

        static int GetFireCountInEngineRoom(SubFire subFire)
        {
            int fireCount = 0;
            foreach (KeyValuePair<CyclopsRooms, SubFire.RoomFire> roomFire in subFire.roomFires)
            {
                if (roomFire.Key != CyclopsRooms.EngineRoom)
                    continue;

                foreach (Transform spawnNode in roomFire.Value.spawnNodes)
                {
                    if (spawnNode.childCount != 0)
                        ++fireCount;
                }
            }
            //AddDebug("fire Count " + fireCount);
            return fireCount;
        }

        static int GetEngineOverheatMinValue(SubFire subFire)
        {
            int overheatValue = GetFireCountInEngineRoom(subFire) * 10;

            //if (subFire.cyclopsMotorMode.engineOn && subFire.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
            //    overheatValue += 10;

            //AddDebug("GetEngineOverheatMinValue " + overheatValue);
            return overheatValue;
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        class VehicleDockingBay_Patch
        {
            [HarmonyPostfix, HarmonyPatch("SetVehicleDocked")]
            public static void SetVehicleDockedPostfix(VehicleDockingBay __instance, Vehicle vehicle)
            { // runs when loading saved game too
                //AddDebug("SetVehicleDocked");
                Transform hatchTransform = __instance.transform.parent.parent.parent.transform.Find("CyclopsMeshAnimated/submarine_hatch_03_base 1/submarine_hatch_03 1");
                if (hatchTransform)
                {
                    Openable openable = hatchTransform.GetComponent<Openable>();
                    if (openable)
                        openable.PlayOpenAnimation(true, openable.animTime);
                }
            }
            //[HarmonyPostfix, HarmonyPatch("OnUndockingStart")]
            public static void OnUndockingStartPostfix(VehicleDockingBay __instance)
            {
                //AddDebug("OnUndockingStart");

            }
            //[HarmonyPostfix, HarmonyPatch("DockVehicle")]
            public static void DockVehiclePostfix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //AddDebug("DockVehicle");

            }
        }

        [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
        class CyclopsHelmHUDManager_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(CyclopsHelmHUDManager __instance)
            {
                if (__instance.transform.parent.name == "__LIGHTMAPPED_PREFAB__")
                    return;

                //AddDebug("CyclopsHelmHUDManager Start " + __instance.name);
                //AddDebug("CyclopsHelmHUDManager Start parent " + __instance.transform.parent.name);
                Transform lightsTransform = __instance.transform.parent.Find("Floodlights");
                if (lightsTransform)
                {
                    FixLight(lightsTransform);
                }
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(CyclopsHelmHUDManager __instance)
            {
                if (!__instance.LOD.IsFull())
                    return;
                //if (Player.main.currentSub && Player.main.currentSub == __instance.subRoot)
                {
                    //AddDebug("powerOn " + powerOn);
                    if (ConfigToEdit.cyclopsHUDalwaysOn.Value)
                    {
                        bool powerOn = __instance.subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline;
                        __instance.hudActive = powerOn;
                    }
                    if (__instance.motorMode.engineOn) // hide speed selector when engine off
                        __instance.engineToggleAnimator.SetTrigger("EngineOn");
                    else
                        __instance.engineToggleAnimator.SetTrigger("EngineOff");
                    //AddDebug("hudActive " + __instance.hudActive);
                    //__instance.canvasGroup.alpha = 0f;
                }
            }

            //[HarmonyPostfix, HarmonyPatch("StartPiloting")]
            public static void StartPilotingPostfix(CyclopsHelmHUDManager __instance)
            {
                Vehicle_patch.currentVehicleTT = TechType.Cyclops;
                Transform lightsTransform = __instance.transform.parent.Find("Floodlights");
                if (lightsTransform)
                {
                    //Light_Control.currentLights = lightsTransform.GetComponentsInChildren<Light>(true);
                }
                //AddDebug("StartPiloting  " + rb.mass);
                //AddDebug(" " + __instance.transform.parent.name); 
            }

            private static void FixLight(Transform lightsTransform)
            {
                Transform topLightTransform = lightsTransform.Find("x_FakeVolumletricLight");
                Transform lightTransform = lightsTransform.Find("VolumetricLight_Front");
                topLightTransform.eulerAngles = new Vector3(344f, topLightTransform.eulerAngles.y, topLightTransform.eulerAngles.z);
                Light newLight = topLightTransform.gameObject.AddComponent<Light>();
                Light oldLight = lightTransform.GetComponent<Light>();
                newLight.type = oldLight.type;
                newLight.spotAngle = oldLight.spotAngle;
                newLight.innerSpotAngle = oldLight.innerSpotAngle;
                newLight.intensity = oldLight.intensity;
                newLight.range = oldLight.range;
                newLight.shadows = oldLight.shadows;

                if (ConfigToEdit.cyclopsLightIntensityMult.Value == 1 && cyclopsLightColor == default)
                    return;

                Light[] lights = lightsTransform.GetComponentsInChildren<Light>(true);
                foreach (Light light in lights)
                {
                    //Main.logger.LogInfo("cyclops Light color " + light.color);
                    if (ConfigToEdit.cyclopsLightIntensityMult.Value < 1)
                        light.intensity *= ConfigToEdit.cyclopsLightIntensityMult.Value;

                    if (cyclopsLightColor != default)
                        light.color = cyclopsLightColor;
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("StopPiloting")]
            public static void StopPilotingPostfix(CyclopsHelmHUDManager __instance)
            {
                //Light_Control.currentLights[0] = null;
                //__instance.hudActive = true;
                //__instance.hornObject.SetActive(true);
                //AddDebug("StopPiloting  ");
            }
        }

        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnCollisionEnter")]
            public static bool OnCollisionEnterPrefix(SubRoot __instance, Collision col)
            { // do not play bang sound fx when fish bumps into cyclops
                if (col.gameObject.CompareTag("Player"))
                    return false;

                Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
                if (rb && rb.mass <= 6) // fishschool mass is 6
                {
                    //AddDebug(col.gameObject.name + "OnCollisionEnter Rigidbody mass < 6");
                    return false;
                }
                GameObject root = Util.GetEntityRoot(col.gameObject);
                if (root == null)
                {
                    //AddDebug(col.gameObject.name + " OnCollisionEnter root null");
                    return true;
                }
                rb = root.GetComponent<Rigidbody>();
                if (rb && rb.mass <= 6)
                {
                    //AddDebug(col.gameObject.name + "OnCollisionEnter Rigidbody mass < 6");
                    return false;
                }
                //if (__instance.isCyclops)
                //{
                //    AddDebug(col.gameObject.name + " OnCollisionEnter " + rb.mass + " mag " + col.relativeVelocity.magnitude);
                //}
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("ForceLightingState")]
            public static void ForceLightingStatePostfix(SubRoot __instance, bool lightingOn)
            {
                if (__instance.isCyclops)
                    __instance.interiorSky.affectedByDayNightCycle = ConfigToEdit.cyclopsSunlight.Value && !lightingOn;
                //AddDebug("SubRoot ForceLightingState " + lightingOn);
            }
        }


        [HarmonyPatch(typeof(CyclopsSonarCreatureDetector), "OnEnable")]
        class CyclopsSonarCreatureDetector_OnEnable_Patch
        {
            public static bool Prefix(CyclopsSonarCreatureDetector __instance)
            {
                return ConfigToEdit.cyclopsSonar.Value;
            }
        }

        [HarmonyPatch(typeof(CyclopsSonarDisplay))]
        class CyclopsSonarDisplay_Patch
        {
            [HarmonyPrefix, HarmonyPatch("DistanceCheck")]
            public static void UpdatePrefix(CyclopsSonarDisplay __instance)
            {
                //AddDebug("CyclopsSonarDisplay DistanceCheck ");
                if (Player.main.currentSub && Player.main.currentSub.isCyclops)
                {
                    bool show = ConfigToEdit.cyclopsSonar.Value && Player.main.currentSub.powerRelay.IsPowered();
                    __instance.gameObject.SetActive(show);
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsNoiseManager))]
        class CyclopsNoiseManager_Patch
        {
            static float idleNoise;

            [HarmonyPostfix, HarmonyPatch("RecalculateNoiseValues")]
            public static void RecalculateNoiseValuesPostfix(CyclopsNoiseManager __instance, float __result)
            {
                idleNoise = __result *= .5f;
            }
            [HarmonyPostfix, HarmonyPatch("GetNoisePercent")]
            public static void GetNoisePercentPostfix(CyclopsNoiseManager __instance, ref float __result)
            {
                if (Main.gameLoaded == false || __result == default || idleNoise == 0 || __instance.subControl.appliedThrottle == false)
                    return;

                Vector3 throttle = __instance.subControl.throttle;
                if (throttle == default)
                    return;

                //AddDebug("idleNoise " + idleNoise.ToString("0.0"));
                float max = Mathf.Max(Mathf.Abs(throttle.x), Mathf.Abs(throttle.y), Mathf.Abs(throttle.z));
                float r = __result * max;
                if (r < __result)
                {
                    if (r > idleNoise)
                    {
                        __result = r;
                        //AddDebug("!!!!! " + r.ToString("0.0"));
                    }
                    else if (__result > idleNoise)
                    {
                        __result = idleNoise;
                        //AddDebug("!!!!! " + r.ToString("0.0"));
                    }
                    //else
                    //{
                    //    AddDebug("__result " + __result.ToString("0.0"));
                    //    AddDebug("max " + max.ToString("0.0"));
                    //    AddDebug("r " + r.ToString("0.0"));
                    //}
                }
            }
        }

        [HarmonyPatch(typeof(SubControl))]
        class SubControl_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SubControl __instance)
            {
                if (__instance.name == "__LIGHTMAPPED_PREFAB__")
                    return;

                TechTag techTag = __instance.gameObject.EnsureComponent<TechTag>();
                techTag.type = TechType.Cyclops;
                Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
                Transform tr = __instance.transform.Find("CyclopsCollision/helmGroup");
                if (tr)
                    Util.AddVFXsurfaceComponent(tr.gameObject, VFXSurfaceTypes.glass);

                WorldForces wf = __instance.GetComponent<WorldForces>();
                if (wf) // prevent it from jumping out of water when surfacing
                    wf.aboveWaterGravity = 20f;

                //AddDebug("Start numBallastWeight " + numBallastWeight);
                //tr = __instance.transform.Find("Headlights");
                //if (tr) // not used
                //    UnityEngine.Object.Destroy(tr.gameObject);

                //Light_Control.lightOrigIntensity[TechType.Cyclops] = 2f;
                //Light_Control.lightIntensityStep[TechType.Cyclops] = .2f;

                //if (Light_Control.IsLightSaved(TechType.Cyclops))
                //{
                //    float intensity = Light_Control.GetLightIntensity(TechType.Cyclops);
                //    foreach (Light l in __instance.transform.Find("Floodlights").GetComponentsInChildren<Light>(true))
                //        l.intensity = intensity;
                //}

            }

        }

        [HarmonyPatch(typeof(CyclopsEntryHatch), "OnTriggerEnter")]
        class CyclopsEntryHatch_Start_Patch
        { // fix bug: entrance hatch not close 
            static void Postfix(CyclopsEntryHatch __instance, Collider col)
            {
                //AddDebug("CyclopsEntryHatch OnTriggerEnter " + Player.main.IsInSubmarine());
                if (col.gameObject == Player.main.gameObject)
                {
                    cyclopsEntryHatch = __instance;
                    if (__instance.hatchOpen && Player.main.IsInSubmarine())
                    {
                        //AddDebug("CyclopsEntryHatch OnTriggerEnter ");
                        __instance.hatchOpen = false;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(PlayerCinematicController))]
        class PlayerCinematicController_Patch
        {
            [HarmonyPostfix, HarmonyPatch("StartCinematicMode")]
            public static void StartCinematicModePostfix(PlayerCinematicController __instance, Player setplayer)
            { // exiting cyclops
                if (cyclopsEntryHatch != null && __instance.name == "cyclops_outerhatch")
                {
                    //AddDebug("cyclops_outerhatch StartCinematicMode");
                    cyclopsEntryHatch.hatchOpen = true;
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsSilentRunningAbilityButton))]
        class CyclopsSilentRunningAbilityButton_Patch
        {
            static float silentRunningPowerCost;
            [HarmonyPrefix, HarmonyPatch("SilentRunningIteration")]
            public static void SilentRunningIterationPrefix(CyclopsSilentRunningAbilityButton __instance)
            { // dont consume power when engine is off
                silentRunningPowerCost = __instance.subRoot.silentRunningPowerCost;
                if (Player.main.currentSub && Player.main.currentSub.noiseManager && Player.main.currentSub.noiseManager.noiseScalar == 0f)
                    __instance.subRoot.silentRunningPowerCost = 0;

                //AddDebug("SilentRunningIteration silentRunningPowerCost " + __instance.subRoot.silentRunningPowerCost);
            }
            [HarmonyPostfix, HarmonyPatch("SilentRunningIteration")]
            public static void SilentRunningIterationPostfix(CyclopsSilentRunningAbilityButton __instance)
            {
                __instance.subRoot.silentRunningPowerCost = silentRunningPowerCost;
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        public class VehicleDockingBay_LaunchbayAreaEnter_Patch
        { // dont play sfx if another vehicle docked
            [HarmonyPrefix, HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaEnter))]
            public static bool LaunchbayAreaEnterPrefix(VehicleDockingBay __instance)
            {
                if (__instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;

                return !__instance._dockedVehicle;
            }
            [HarmonyPrefix, HarmonyPatch(nameof(VehicleDockingBay.LaunchbayAreaExit))]
            public static bool LaunchbayAreaExitPrefix(VehicleDockingBay __instance)
            {
                if (__instance.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline)
                    return false;

                return !__instance._dockedVehicle;
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.UpdateDockedPosition))]
            //[HarmonyPrefix]
            public static void UpdateDockedPositionPrefix(VehicleDockingBay __instance, Vehicle vehicle, ref float interpfraction)
            {
                //AddDebug("UpdateDockedPosition " + vehicle.transform.position);
                //if (!properlyDocked)
                //interpfraction = 0;
                //AddDebug("interpfraction " + interpfraction);
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.SetVehicleDocked))]
            //[HarmonyPostfix]
            public static void SetVehicleDockedPrefix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //properlyDocked = false;
                //__instance.exosuitDockPlayerCinematic.StartCinematicMode(Player.main);
                //__instance.DockVehicle(vehicle);
                //__instance.GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);
                //SafeAnimator.SetBool(__instance.animator, "exosuit_docked", true);
                //AddDebug("SetVehicleDocked");
            }
            //[HarmonyPatch(nameof(VehicleDockingBay.DockVehicle))]
            //[HarmonyPrefix]
            public static void DockVehiclePrefix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //AddDebug("DockVehicle");
            }
        }




        [HarmonyPatch(typeof(CyclopsLightingPanel))]
        public class CyclopsLightingPanel_Patch
        {
            static void TurnOnFloodlights(CyclopsLightingPanel clp)
            {
                if (clp.cyclopsRoot.powerRelay.GetPowerStatus() == PowerSystem.Status.Offline || clp.cyclopsRoot.silentRunning)
                    return;

                clp.floodlightsOn = true;
                clp.SetExternalLighting(true);
                //FMODUWE.PlayOneShot(this.floodlightsOn ? this.vn_floodlightsOn : this.vn_floodlightsOff, this.transform.position);
                clp.UpdateLightingButtons();
            }

            static void TurnOffInternalLighting(CyclopsLightingPanel clp)
            {
                clp.lightingOn = false;
                clp.cyclopsRoot.ForceLightingState(false);
                //FMODUWE.PlayOneShot(this.lightingOn ? this.vn_lightsOn : this.vn_lightsOff, this.transform.position);
                clp.UpdateLightingButtons();
            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CyclopsLightingPanel __instance)
            {
                //AddDebug("CyclopsLightingPanel Start " + __instance.transform.parent.name);
                if (__instance.transform.parent.name == "__LIGHTMAPPED_PREFAB__")
                    return;

                GameObject root = __instance.transform.parent.gameObject;
                if (Main.configMain.GetCyclopsFloodLights(root))
                    TurnOnFloodlights(__instance);

                if (Main.configMain.GetCyclopsLighting(root))
                    TurnOffInternalLighting(__instance);
            }
            [HarmonyPostfix]
            [HarmonyPatch("ToggleFloodlights")]
            static void ToggleFloodlightsPostfix(CyclopsLightingPanel __instance)
            {
                //AddDebug("ToggleFloodlights " + __instance.floodlightsOn);
                GameObject root = __instance.transform.parent.gameObject;
                if (__instance.floodlightsOn)
                    Main.configMain.SaveCyclopsFloodLights(root);
                else
                    Main.configMain.DeleteCyclopsFloodLights(root);
            }
            [HarmonyPostfix]
            [HarmonyPatch("ToggleInternalLighting")]
            static void ToggleInternalLightingPostfix(CyclopsLightingPanel __instance)
            {
                //AddDebug("ToggleFloodlights " + __instance.floodlightsOn);
                GameObject root = __instance.transform.parent.gameObject;
                if (__instance.lightingOn)
                    Main.configMain.DeleteCyclopsLighting(root);
                else
                    Main.configMain.SaveCyclopsLighting(root);
            }
            [HarmonyPostfix]
            [HarmonyPatch("SubConstructionComplete")]
            public static void SubConstructionCompletePostfix(CyclopsLightingPanel __instance)
            { // fix: lights are on even if sub has no batteries
                //AddDebug("CyclopsLightingPanel SubConstructionComplete " + __instance.floodlightsOn);
                //AddDebug("CyclopsLightingPanel Powered " + __instance.CheckIsPowered());
                bool powered = __instance.CheckIsPowered();
                __instance.floodlightsOn = powered;
                __instance.SetExternalLighting(powered);
                __instance.UpdateLightingButtons();
            }
            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(CyclopsLightingPanel __instance)
            {
                bool isPowered = __instance.CheckIsPowered();
                if (__instance.prevPowerRelayState && !isPowered)
                {
                    //AddDebug("CyclopsLightingPanel not Powered");
                    __instance.uiPanel.SetBool("PanelActive", false);
                    __instance.Invoke("ButtonsOff", 0f);
                }
            }
            [HarmonyPostfix, HarmonyPatch("OnTriggerEnter")]
            public static void OnTriggerEnterPostfix(CyclopsLightingPanel __instance, Collider col)
            {
                if (col.gameObject != Player.main.gameObject)
                    return;

                bool isPowered = __instance.CheckIsPowered();
                if (!isPowered)
                {
                    __instance.uiPanel.SetBool("PanelActive", false);
                    __instance.ButtonsOff();
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsDestructionEvent))]
        class CyclopsDestructionEvent_SwapToDamagedModels_Patch
        {
            [HarmonyPrefix, HarmonyPatch("DestroyCyclops")]
            static void DestroyCyclopsPrefix(CyclopsDestructionEvent __instance)
            { // fix bug: player respawns in destroyed cyclops
                __instance.subLiveMixin.Kill();
                //AddDebug("CyclopsDestructionEvent DestroyCyclops IsAlive " + __instance.subLiveMixin.IsAlive());
            }
        }

        [HarmonyPatch(typeof(CyclopsProximitySensors), "OnPlayerModeChange")]
        class CyclopsProximitySensors_OnPlayerModeChange_Patch
        {
            public static bool Prefix(CyclopsProximitySensors __instance)
            {
                //Main.config.disableCyclopsProximitySensor = true;
                //AddDebug ("CyclopsProximitySensors OnPlayerModeChange " + ConfigToEdit.disableCyclopsProximitySensor.Value);
                return !ConfigToEdit.disableCyclopsProximitySensor.Value;
            }
        }

        [HarmonyPatch(typeof(SubFire))]
        class SubFire_EngineOverheatSimulation_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static bool StartPrefix(SubFire __instance)
            {
                //AddDebug("SubFire Start  " + __instance.transform.position);
                // prevent invoke in prefab
                if (__instance.transform.position == Vector3.zero)
                    return false;

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool UpdatePrefix(SubFire __instance)
            {
                return Main.gameLoaded;
            }

            [HarmonyPostfix, HarmonyPatch("FireSimulation")]
            static void FireSimulationPostfix(SubFire __instance)
            {
                if (!ConfigToEdit.cyclopsFireMusic.Value && __instance.fireMusic && __instance.fireMusic.playing)
                    __instance.fireMusic.Stop();
            }

            [HarmonyPrefix]
            [HarmonyPatch("EngineOverheatSimulation")]
            static bool EngineOverheatSimulationPrefix(SubFire __instance)
            {
                //AddDebug("SubFire EngineOverheatSimulation activeInHierarchy " + __instance.gameObject.activeInHierarchy);
                //AddDebug("SubFire position " + __instance.transform.position);
                if (Main.cyclopsOverheatLoaded)
                    return true;

                if (!__instance.gameObject.activeInHierarchy || !__instance.LOD.IsFull())
                    return false;

                //AddDebug("engineOverheatValue " + __instance.engineOverheatValue);
                //AddDebug("appliedThrottle " + __instance.subControl.appliedThrottle);
                //AddDebug("engineOn " + __instance.cyclopsMotorMode.engineOn);
                if (!__instance.cyclopsMotorMode.engineOn)
                {
                    //AddDebug("engine off");
                    __instance.engineOverheatValue = Mathf.Max(GetEngineOverheatMinValue(__instance), __instance.engineOverheatValue - 10);
                    return false;
                }
                if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
                {
                    if (__instance.subControl.appliedThrottle)
                        __instance.engineOverheatValue += 10;
                    else
                        __instance.engineOverheatValue -= 4;
                }
                else if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Standard)
                {
                    if (__instance.subControl.appliedThrottle)
                        __instance.engineOverheatValue -= 3;
                    else
                        __instance.engineOverheatValue -= 6;
                }
                else if (__instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Slow)
                {
                    if (__instance.subControl.appliedThrottle)
                        __instance.engineOverheatValue -= 6;
                    else
                        __instance.engineOverheatValue -= 8;
                }
                if (__instance.subControl.appliedThrottle && __instance.cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
                {
                    if (ConfigMenu.cyclopsFireChance.Value > 0)
                    {
                        if (__instance.engineOverheatValue > 75)
                        {
                            __instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatCriticalNotification);
                        }
                        else if (__instance.engineOverheatValue > 50)
                        {
                            __instance.subRoot.voiceNotificationManager.PlayVoiceNotification(__instance.subRoot.engineOverheatNotification);
                        }
                    }
                }
                int overheatMinValue = GetEngineOverheatMinValue(__instance);
                __instance.engineOverheatValue = Mathf.Clamp(__instance.engineOverheatValue, overheatMinValue, 100);

                if (__instance.engineOverheatValue > 50)
                {
                    int fireChance = UnityEngine.Random.Range(0, __instance.engineOverheatValue + 51);
                    fireChance = Mathf.Clamp(fireChance, 0, 100);
                    int fireCheck = 100 - ConfigMenu.cyclopsFireChance.Value;
                    //AddDebug("fireChance " + fireChance);
                    if (fireChance > fireCheck)
                        __instance.CreateFire(__instance.roomFires[CyclopsRooms.EngineRoom]);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CyclopsExternalDamageManager), "UpdateOvershield")]
        class CyclopsExternalDamageManager_UpdateOvershield_Patch
        {
            static void Prefix(CyclopsExternalDamageManager __instance)
            {
                if (Main.gameLoaded == false || ConfigMenu.cyclopsAutoHealHealthPercent.Value == 90)
                    return;

                //AddDebug(__instance.name + " CyclopsExternalDamageManager UpdateOvershield " + __instance.overshieldPercentage);
                __instance.overshieldPercentage = 100 - ConfigMenu.cyclopsAutoHealHealthPercent.Value;
            }
        }

    }
}
