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
    class Tools
    {
        public static Dictionary<Creature, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<Creature, Dictionary<TechType, int>>();
        public static List<Rigidbody> stasisTargets = new List<Rigidbody>();
        public static Color flashLightLightColor;


        [HarmonyPatch(typeof(FlashLight), "Start")]
        public class FlashLight_Start_Patch
        {
            public static void Prefix(FlashLight __instance)
            {
                Transform lightParentTransform = __instance.transform.Find("lights_parent");
                Light[] lights = lightParentTransform.GetComponentsInChildren<Light>(true);
                for (int i = lights.Length - 1; i >= 0; i--)
                {
                    Light light = lights[i];
                    if (light.type == LightType.Point)
                        light.enabled = false;
                    else
                    {
                        if (flashLightLightColor != default)
                            light.color = flashLightLightColor;

                        if (ConfigToEdit.flashlightLightIntensityMult.Value < 1)
                            light.intensity *= ConfigToEdit.flashlightLightIntensityMult.Value;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerTool))]
        public class PlayerTool_Patch
        {
            //[HarmonyPrefix, HarmonyPatch("Awake")]
            public static void AwakePrefix(PlayerTool __instance)
            {
                Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                if (lights.Length > 0)
                { // seaglide uses 2 lights
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    //AddDebug(tt + " PlayerTool.Awake lights " + lights.Length);
                    if (tt == TechType.DiveReel || tt == TechType.LaserCutter)
                        return;

                    //Light_Control.lightOrigIntensity[tt] = lights[0].intensity;
                    //Light_Control.lightIntensityStep[tt] = lights[0].intensity * .1f;
                    //Main.logger.LogMessage(tt + " lightOrigIntensity " + lights[0].intensity);
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnDraw")]
            public static void OnDrawPostfix(PlayerTool __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt == TechType.Spadefish && Util.IsDead(__instance.gameObject))
                {
                    //AddDebug("dead spadefish");
                    Vector3 pos = __instance.transform.localPosition;
                    __instance.transform.localPosition = new Vector3(pos.x -= .07f, pos.y += .03f, pos.z += .07f);
                    return;
                }
                //if (Light_Control.IsLightSaved(tt))
                {
                    //Light[] lights = __instance.GetComponentsInChildren<Light>(true);
                    //AddDebug(tt + " Lights " + lights.Length);
                    //float intensity = Light_Control.GetLightIntensity(tt);
                    //foreach (Light l in lights)
                    //    l.intensity = intensity;

                    //if (__instance is Flare)
                    {
                        //Flare flare = __instance as Flare;
                        //flare.originalIntensity = intensity;
                        //AddDebug($"OnDraw Flare Intensity {intensity} originalIntensity {flare.originalIntensity}");
                    }
                }
                //LEDLight ledLight = __instance as LEDLight;
                //if (ledLight)
                //    ledLight.SetLightsActive(Main.config.LEDLightWorksInHand);
            }

            [HarmonyPostfix, HarmonyPatch("GetCustomUseText")]
            public static void GetCustomUseTextPostfix(PlayerTool __instance, ref string __result)
            {
                if (ConfigToEdit.stasisRifleTweaks.Value && __instance is StasisRifle)
                {
                    __result = UI_Patches.stasisRifleString;
                }
                else if (__instance is ScannerTool)
                {
                    //AddDebug("GetCustomUseText");
                    __result = UI_Patches.scannerString;
                }
            }
        }

        [HarmonyPatch(typeof(Beacon))]
        class Beacon_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            static void StartPostfix(Beacon __instance)
            {
                Transform label = __instance.transform.Find("label");
                if (label)
                {
                    if (__instance.beaconLabel.stringBeaconSubmit.IsNullOrWhiteSpace())
                        __instance.beaconLabel.stringBeaconSubmit = Language.main.Get("BeaconSubmit");

                    if (ConfigToEdit.beaconTweaks.Value)
                    {
                        BoxCollider boxCollider = label.GetComponent<BoxCollider>();
                        if (boxCollider)
                            //    UnityEngine.Object.Destroy(boxCollider);
                            //AddDebug("Beacon Start  label");
                            label.gameObject.SetActive(false);
                    }
                }
            }
            [HarmonyPostfix, HarmonyPatch("Throw")]
            static void ThrowPostfix(Beacon __instance)
            {
                if (ConfigToEdit.beaconTweaks.Value)
                {
                    __instance.gameObject.transform.rotation = Camera.main.transform.rotation;
                    __instance.transform.Rotate(0f, 180f, 0f); // x and z do not matter, it will stabilize itself
                }
            }

        }
        [HarmonyPatch(typeof(StasisSphere))]
        class StasisSphere_Patch
        {
            static bool updateFrame;

            private static bool isBigger(StasisSphere stasisSphere, GameObject go)
            {// does not work reliably for boneshark
                float stasisSphereVolume = UWE.Utils.GetAABBVolume(stasisSphere.gameObject);
                float volume = UWE.Utils.GetAABBVolume(go);
                //AddDebug(go.name + " isBigger " + (int)stasisSphereVolume + ' ' + (int)volume);
                return volume > stasisSphereVolume;
            }

            //[HarmonyPostfix,HarmonyPatch("EnableField")]
            private static void EnableFieldPostfix(StasisSphere __instance)
            {

            }

            [HarmonyPrefix, HarmonyPatch("LateUpdate")]
            private static bool LateUpdatePrefix(StasisSphere __instance)
            {
                //if (!Main.config.stasisRifleTweak)
                //    return true;

                if (!__instance.fieldEnabled)
                    return false;

                updateFrame = !updateFrame;
                return updateFrame;
            }

            //[HarmonyPrefix,HarmonyPatch("LateUpdate")]
            private static bool LateUpdatePrefix_OLD(StasisSphere __instance)
            {
                //if (!Main.config.stasisRifleTweak)
                //    return true;

                if (!__instance.fieldEnabled)
                    return false;

                __instance.fieldEnergy -= Time.deltaTime / __instance.time;
                float fieldRadius = __instance.fieldRadius;
                //AddDebug("fieldRadius " + (fieldRadius.ToString("1.1") ));
                float num1 = (fieldRadius * fieldRadius + 4f);
                if (__instance.fieldEnergy <= 0f)
                {
                    __instance.fieldEnergy = 0f;
                    __instance.CancelAll();
                    FMODUWE.PlayOneShot(__instance.soundDeactivate, __instance.tr.position);
                }
                else
                {
                    Rigidbody target = null;
                    List<Rigidbody> rigidbodyList = new List<Rigidbody>();
                    int num2 = UWE.Utils.OverlapSphereIntoSharedBuffer(__instance.tr.position, fieldRadius, (int)__instance.fieldLayerMask);
                    float distToPlayer = Vector3.Distance(__instance.tr.position, Player.main.transform.position);
                    //AddDebug("distance to player " + (int)distToPlayer + ' ' + (int)(fieldRadius ));
                    __instance.tr.localScale = (2f * fieldRadius + 2f) * Vector3.one;
                    if (distToPlayer < fieldRadius + .5f)
                    {
                        //AddDebug("freeze player ");
                        Player.main.rigidBody.isKinematic = true;
                        //bool bigger = isBigger(__instance, Player.mainObject);
                        //AddDebug("Player isBigger " + bigger);
                    }
                    else if (distToPlayer > fieldRadius - .5f)
                    {
                        //AddDebug("unfreeze player ");
                        Player.main.rigidBody.isKinematic = false;
                    }
                    for (int index = 0; index < num2; ++index)
                    {
                        //AddDebug("sharedColliderBuffer " + UWE.Utils.sharedColliderBuffer[index].name);
                        Collider collider = UWE.Utils.sharedColliderBuffer[index];
                        //if (collider.bounds.size.magnitude < fieldRadius && __instance.Freeze(collider, ref target))
                        //    rigidbodyList.Add(target);
                        if (__instance.Freeze(collider, ref target))
                        {
                            //AddDebug("Freeze " + target.name + " size.magnitude " + (int)collider.bounds.size.magnitude);
                            rigidbodyList.Add(target);
                        }
                    }
                    for (int index = __instance.targets.Count - 1; index >= 0; --index)
                    {
                        target = __instance.targets[index];
                        if (target == null || !target.gameObject.activeSelf)
                        {
                            __instance.targets.RemoveAt(index);
                        }
                        else
                        {
                            Pickupable pickupable = target.GetComponentInParent<Pickupable>();
                            if (pickupable != null && pickupable.attached)
                                __instance.targets.RemoveAt(index);
                            else if (!rigidbodyList.Contains(target))
                            {
                                Vector3 end = target.ClosestPointOnBounds(__instance.tr.position);
                                Vector3 vector3 = end - __instance.tr.position;
                                //UnityEngine.Debug.DrawLine(__instance.tr.position, end, Color.red);
                                if (vector3.sqrMagnitude > num1)
                                {
                                    __instance.Unfreeze(target);
                                    __instance.targets.RemoveAt(index);
                                }
                            }
                        }
                    }
                    __instance.UpdateMaterials();
                }
                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Freeze")]
            private static bool FreezePrefix(StasisSphere __instance, Collider other, ref Rigidbody target, ref bool __result)
            {
                //if (!Main.config.stasisRifleTweak)
                //    return true;

                //AddDebug("Freeze Collider " + other.name + " bounds.size " + other.bounds.size);
                target = other.GetComponentInParent<Rigidbody>();

                if (target == null || target.isKinematic)
                {
                    __result = false;
                    return false;
                }
                if (__instance.targets.Contains(target))
                {
                    __result = true;
                    return false;
                }
                bool bigger = isBigger(__instance, target.gameObject);
                //AddDebug("Freeze " + target.name + " isBigger " + bigger);
                if (bigger)
                {
                    __result = false;
                    return false;
                }
                target.isKinematic = true;
                __instance.targets.Add(target);
                Utils.PlayOneShotPS(__instance.vfxFreeze, target.GetComponent<Transform>().position, Quaternion.identity);
                FMODUWE.PlayOneShot(__instance.soundEnter, __instance.tr.position);
                target.SendMessage("OnFreeze", SendMessageOptions.DontRequireReceiver);
                __result = true;
                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Freeze")]
            private static void FreezePrefix(StasisSphere __instance)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    AddDebug("targets " + __instance.targets.Count);
                    foreach (var rb in __instance.targets)
                    {
                        Main.logger.LogMessage("targets " + rb.name);
                    }
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Freeze")]
            private static void FreezePostfix(StasisSphere __instance, Collider other)
            {
                GameObject go = other.transform.gameObject;
                //if (!Creature_Tweaks.objectsInStasis.TryGetValue(go, out string s))
                {
                    TechType tt = CraftData.GetTechType(go);
                    if (tt == TechType.GasPod)
                    {
                        GasPod gasPod = other.GetComponent<GasPod>();
                        if (gasPod && !gasPod.detonated)
                        {
                            //AddDebug("Freeze  GasPod " );
                            //Creature_Tweaks.objectsInStasis.Add(go, null);
                        }
                    }
                    //else if (tt == TechType.Gasopod)
                    //    Creature_Tweaks.objectsInStasis.Add(go, null);
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Unfreeze")]
            private static void UnfreezePostfix(StasisSphere __instance, Rigidbody target)
            {
                if (target == null)
                    return;

                //Creature_Tweaks.objectsInStasis.Remove(target.gameObject);
                //stasisTargets = __instance.targets;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnDestroy")]
            private static void OnDestroyPrefix(StasisSphere __instance)
            {
                AddDebug("StasisSphere OnDestroy");
                //if (Main.loadingDone)
                //    Player.main.rigidBody.isKinematic = false;
            }

            [HarmonyPostfix, HarmonyPatch("Awake")]
            private static void AwakePostfix(StasisSphere __instance)
            {
                stasisTargets = __instance.targets;
            }
        }

        [HarmonyPatch(typeof(BuilderTool), "HasEnergyOrInBase")]
        class BuilderTool_HasEnergyOrInBase_Patch
        {
            static void Postfix(BuilderTool __instance, ref bool __result)
            {
                if (!ConfigToEdit.builderToolBuildsInsideWithoutPower.Value && __instance.energyMixin.charge <= 0)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(ScannerTool), "PlayScanFX")]
        class ScannerTool_PlayScanFX_Patch
        {
            static bool Prefix(ScannerTool __instance)
            {
                //AddDebug("ScannerTool PlayScanFX ");
                return ConfigToEdit.scannerFX.Value;
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
                //AddDebug(" NotifyConstructedChanged isPlacing " + Builder.isPlacing);
                if (!ConfigToEdit.builderPlacingWhenFinishedBuilding.Value)
                    Player.main.StartCoroutine(BuilderEnd(2));
            }
        }

        [HarmonyPatch(typeof(BuilderTool), "HandleInput")]
        class BuilderTool_HandleInput_patch
        {
            public static void Postfix(BuilderTool __instance)
            {
                if (Builder.isPlacing && GameInput.GetButtonDown(GameInput.Button.Exit))
                {
                    //AddDebug("BuilderTool HandleInput Exit");
                    //__instance.OnHolster();
                    Inventory.main.quickSlots.Deselect();
                }
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

        [HarmonyPatch(typeof(LaserCutter))]
        internal class LaserCutter_
        {
            [HarmonyPostfix, HarmonyPatch("RandomizeIntensity")]
            static void RandomizeIntensityPostfix(LaserCutter __instance)
            {
                if (ConfigToEdit.laserCutterLightIntensityMult.Value == 1)
                    return;

                __instance.lightIntensity *= ConfigToEdit.laserCutterLightIntensityMult.Value;
                //AddDebug("Randomize Intensity " + __instance.lightIntensity.ToString("0.0"));
            }
        }

    }
}
