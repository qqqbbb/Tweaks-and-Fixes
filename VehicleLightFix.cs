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
    internal class VehicleLightFix
    {
        static VFXVolumetricLight seamothVFXVolumetricLight;
        static GameObject seamothLightCone;
        public static Color seamothLightColor;
        public static Color exosuitLightColor;
        Vector3 exosuitLightBeamPos = new Vector3(0, 0f, -0.6f); // y changes after entering exosuit
        static WaitUntil volLightBeamNotNull = new WaitUntil(() => seamothLightCone != null);
        public static bool fixed_;
        public static FMODAsset lightOnSound;
        public static FMODAsset lightOffSound;

        public void CreateSounds(GameObject exosuit)
        {
            lightOnSound = ScriptableObject.CreateInstance<FMODAsset>();
            lightOnSound.path = "event:/sub/seamoth/seaglide_light_on";
            lightOnSound.id = "{fe76457f-0c94-4245-a080-8a5b2f8853c4}";
            lightOffSound = ScriptableObject.CreateInstance<FMODAsset>();
            lightOffSound.path = "event:/sub/seamoth/seaglide_light_off";
            lightOffSound.id = "{b52592a9-19f5-45d1-ad56-7d355fc3dcc3}";
            CollisionSound collisionSound = exosuit.EnsureComponent<CollisionSound>();
            FMODAsset so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/common/fishsplat";
            so.id = "{0e47f1c6-6178-41bd-93bf-40bfca179cb6}";
            collisionSound.hitSoundSmall = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_hard";
            so.id = "{ed65a390-2e80-4005-b31b-56380500df33}";
            collisionSound.hitSoundFast = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_medium";
            so.id = "{cb2927bf-3f8d-45d8-afe2-c82128f39062}";
            collisionSound.hitSoundMedium = so;
            so = ScriptableObject.CreateInstance<FMODAsset>();
            so.path = "event:/sub/seamoth/impact_solid_soft";
            so.id = "{15dc7344-7b0a-4ffd-9b5c-c40f923e4f4d}";
            collisionSound.hitSoundSlow = so;
        }

        public IEnumerator FixExosuit()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Exosuit);
            yield return request;
            GameObject prefab = request.GetResult();
            Transform lights_parent = prefab.transform.Find("lights_parent");
            Exosuit exosuit = prefab.GetComponent<Exosuit>();
            lights_parent.SetParent(exosuit.leftArmAttach);
            FixExosuitLight(exosuit);
            CreateSounds(prefab);
            EnergyEffect energyEffect = exosuit.GetComponent<EnergyEffect>();
            // it turns off lights when left battery is removed
            UnityEngine.Object.Destroy(energyEffect);
        }

        private void FixExosuitLight(Exosuit exosuit)
        {
            Transform lightTransform = Util.GetExosuitLightsTransform(exosuit);
            Light[] Lights = lightTransform.GetComponentsInChildren<Light>(true);

            foreach (Light light in Lights)
            {
                //Main.logger.LogInfo("Exosuit light color " + light.color);
                if (ConfigToEdit.exosuitLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.exosuitLightIntensityMult.Value;

                //AddDebug("exosuitLightColor " + exosuitLightColor);
                if (exosuitLightColor != default)
                    light.color = exosuitLightColor;

                UWE.CoroutineHost.StartCoroutine(AddVolLight(light.gameObject, exosuitLightBeamPos));
                //AddVolLight(light.gameObject);
            }
            lightTransform.gameObject.SetActive(false);
        }

        public static IEnumerator AddVolLight(GameObject parent, Vector3 pos = default, Vector3 scale = default)
        {
            yield return volLightBeamNotNull;
            GameObject lightCone = UnityEngine.Object.Instantiate(seamothLightCone, Vector3.zero, Quaternion.identity);
            lightCone.transform.parent = parent.transform;
            lightCone.transform.localPosition = pos;
            lightCone.transform.localRotation = Quaternion.identity;
            if (scale != default)
                lightCone.transform.localScale = scale;

            VFXVolumetricLight volLight = parent.gameObject.AddComponent<VFXVolumetricLight>();
            volLight.angle = seamothVFXVolumetricLight.angle;
            volLight.range = seamothVFXVolumetricLight.range;
            volLight.intensity = seamothVFXVolumetricLight.intensity;
            volLight.startOffset = seamothVFXVolumetricLight.startOffset;
            volLight.startFallof = seamothVFXVolumetricLight.startFallof;
            volLight.nearClip = seamothVFXVolumetricLight.nearClip;
            volLight.softEdges = seamothVFXVolumetricLight.softEdges;
            volLight.segments = seamothVFXVolumetricLight.segments;
            volLight.lightType = seamothVFXVolumetricLight.lightType;
            volLight.color = seamothVFXVolumetricLight.color;
            volLight.lightIntensity = seamothVFXVolumetricLight.lightIntensity;
            volLight.coneMat = seamothVFXVolumetricLight.coneMat;
            volLight.sphereMat = seamothVFXVolumetricLight.sphereMat;
            volLight.volumMesh = seamothVFXVolumetricLight.volumMesh;
            volLight.block = seamothVFXVolumetricLight.block;
            volLight.lightSource = parent.GetComponentInChildren<Light>(true);
            //Main.logger.LogMessage("AddVolLight lightSource.color " + volLight.lightSource.color);
            volLight.volumGO = lightCone;
            volLight.volumRenderer = lightCone.GetComponent<MeshRenderer>();
            volLight.volumMeshFilter = lightCone.GetComponent<MeshFilter>();
            volLight.syncMeshWithLight = seamothVFXVolumetricLight.syncMeshWithLight;
            volLight.enabled = false;
            volLight.enabled = true; // need this for exosuit
        }

        private static void ToggleExosuitLights(Exosuit exosuit)
        {
            Transform lightParent = Util.GetExosuitLightsTransform(exosuit);
            if (lightParent == null)
                return;

            //AddDebug("ToggleLights lightsTransform activeSelf " + lightsTransform.gameObject.activeSelf);
            //AddDebug("ToggleLights hasCharge " + exosuit.energyInterface.hasCharge);
            if (!lightParent.gameObject.activeSelf && exosuit.energyInterface.hasCharge)
            {
                lightParent.gameObject.SetActive(true);
                Main.configMain.DeleteExosuitLights(exosuit.gameObject);

                if (lightOnSound)
                    Utils.PlayFMODAsset(lightOnSound, exosuit.transform.position);
            }
            else if (lightParent.gameObject.activeSelf)
            {
                lightParent.gameObject.SetActive(false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                if (lightOffSound)
                    Utils.PlayFMODAsset(lightOffSound, exosuit.transform.position);
            }
        }

        private static void SetExosuitLights(Exosuit exosuit, bool on)
        {
            //AddDebug("SetLights " + on);
            Transform lightParent = Util.GetExosuitLightsTransform(exosuit);
            lightParent.gameObject.SetActive(on);
        }

        [HarmonyPatch(typeof(Vehicle))]
        class Vehicle_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnPoweredChanged")]
            public static void OnPoweredChangedPostfix(Vehicle __instance, bool powered)
            {
                //AddDebug("Vehicle OnPoweredChanged " + powered);
                Exosuit exosuit = __instance as Exosuit;
                if (exosuit && __instance.vfxConstructing.IsConstructed())
                {
                    SetExosuitLights(exosuit, powered);
                }
            }
        }

        [HarmonyPatch(typeof(Exosuit))]
        class Exosuit_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(Exosuit __instance)
            {
                //AddDebug("Exosuit Start ");
                if (Main.gameLoaded == false)
                {
                    bool off = Main.configMain.GetExosuitLights(__instance.gameObject);
                    SetExosuitLights(__instance, !off);
                }
            }

            [HarmonyPostfix, HarmonyPatch("Update")]
            public static void UpdatePostfix(Exosuit __instance)
            {
                if (Main.gameLoaded == false || Main.vehicleLightsImprovedLoaded)
                    return;

                if (!IngameMenu.main.isActiveAndEnabled && !Player.main.pda.isInUse && Player.main.currentMountedVehicle == __instance)
                {
                    if (GameInput.GetButtonDown(GameInput.Button.MoveDown))
                        ToggleExosuitLights(__instance);
                }
            }

            [HarmonyPostfix, HarmonyPatch("EnterVehicle")]
            public static void EnterVehiclePostfix(Exosuit __instance)
            {
                CoroutineHost.StartCoroutine(DisableLightBeam(__instance));
            }

            static IEnumerator DisableLightBeam(Exosuit exosuit)
            {
                yield return Main.waitUntilGameLoaded;
                ToggleLightBeam(exosuit, false);
            }

            static void ToggleLightBeam(Exosuit exosuit, bool on)
            {
                Transform lightT = Util.GetExosuitLightsTransform(exosuit);
                VFXVolumetricLight[] volLights = lightT.GetComponentsInChildren<VFXVolumetricLight>(true);
                foreach (var volL in volLights)
                {
                    if (on)
                        volL.RestoreVolume();
                    else
                        volL.DisableVolume();
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnPilotModeEnd")]
            public static void OnPlayerEnteredPostfix(Exosuit __instance)
            {
                ToggleLightBeam(__instance, true);
            }
        }

        [HarmonyPatch(typeof(VehicleDockingBay))]
        class VehicleDockingBay_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("OnUndockingStart")]
            public static void OnUndockingStartPostfix(VehicleDockingBay __instance)
            {
                Exosuit exosuit = __instance.dockedVehicle as Exosuit;
                if (exosuit)
                {
                    //AddDebug("OnUndockingStart");
                    SetExosuitLights(exosuit, true);
                }
            }

            [HarmonyPostfix, HarmonyPatch("DockVehicle")]
            public static void DockVehiclePostfix(VehicleDockingBay __instance, Vehicle vehicle)
            {
                //AddDebug("DockVehicle");
                Exosuit exosuit = vehicle as Exosuit;
                if (exosuit)
                    CoroutineHost.StartCoroutine(TurnOffLightsDelay(exosuit, 2));
            }

            public static IEnumerator TurnOffLightsDelay(Exosuit exosuit, float delay)
            {
                yield return new WaitForSeconds(delay);
                SetExosuitLights(exosuit, false);
                Main.configMain.SaveExosuitLights(exosuit.gameObject);
                //AddDebug("Set Lights off");
            }
        }

        //[HarmonyPatch(typeof(SeaMoth))]
        class SeaMoth_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("Start")]
            public static void OnUndockingStartPostfix(SeaMoth __instance)
            {
                AddDebug("SeaMoth Start " + __instance.lightsParent.name);
                //__instance.lightsParent.SetActive(false);
            }
            //[HarmonyPostfix, HarmonyPatch("onLightsToggled")]
            public static void onLightsToggledostfix(SeaMoth __instance)
            {
                AddDebug("SeaMoth onLightsToggled");
                //__instance.lightsParent.SetActive(false);
            }
        }

        public IEnumerator GetSeaMothVolLight()
        {
            if (seamothLightCone != null)
                yield break;

            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
            yield return request;
            GameObject prefab = request.GetResult();
            Transform lightTr = prefab.transform.Find("lights_parent/light_left");
            Transform fakeLightTr = lightTr.Find("x_FakeVolumletricLight");
            seamothLightCone = fakeLightTr.gameObject;
            seamothVFXVolumetricLight = lightTr.GetComponent<VFXVolumetricLight>();
        }

        public IEnumerator FixSeamothLights()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
            yield return request;
            GameObject prefab = request.GetResult();
            Transform lightParentTransform = prefab.transform.Find("lights_parent");
            Light[] lights = lightParentTransform.GetComponentsInChildren<Light>(true);
            //AddDebug("FixSeamothLights " + lights.Length);
            for (int i = 0; i < lights.Length; i++)
            {
                Light light = lights[i];
                MeshRenderer mr = light.GetComponentInChildren<MeshRenderer>();
                if (i == 0)
                    mr.transform.localPosition = new Vector3(0.05f, 0.05f, -0.9f);
                else
                    mr.transform.localPosition = new Vector3(-0.05f, 0.05f, -0.9f);

                if (ConfigToEdit.seamothLightIntensityMult.Value < 1)
                    light.intensity *= ConfigToEdit.seamothLightIntensityMult.Value;

                if (seamothLightColor != default)
                    light.color = seamothLightColor;
                //Main.logger.LogInfo("SeaMoth light color " + light.color);
            }
            Transform toggleLights = prefab.transform.Find("ToggleLights");
            toggleLights.gameObject.SetActive(false);
        }


    }
}
