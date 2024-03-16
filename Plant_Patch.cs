using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;
using static VehicleUpgradeConsoleInput;

namespace Tweaks_Fixes
{
    class Plants_Patch
    {
        public static float creepVineSeedLightInt = 2f;

        [HarmonyPatch(typeof(GrowingPlant))]
        class GrowingPlant_Patch
        {
            static int growUpdateTime = 0;

            //[HarmonyPostfix]
            //[HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(GrowingPlant __instance)
            {
                TechType tt = __instance.plantTechType;
                //AddDebug(__instance.name + " GrowingPlant OnEnable " + tt);
                string name = __instance.name;
                if (tt == TechType.BulboTree || tt == TechType.PurpleVasePlant || tt == TechType.OrangePetalsPlant || tt == TechType.PinkMushroom || tt == TechType.PurpleRattle)
                {
                    LargeWorldEntity lwe = __instance.GetComponent<LargeWorldEntity>();
                    if (lwe)
                        LargeWorldEntity_Patch.DisableWavingShader(lwe);
                }
                else if (name == "GrowingMembrainTree(Clone)")
                {   //  all LOD meshes look the same, render distance is too small
                    LargeWorldEntity_Patch.AlwaysUseHiPolyMesh(__instance.gameObject);
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("SpawnGrownModel")]
            public static void SpawnGrownModelPostfix(GrowingPlant __instance)
            {
                AddDebug("SpawnGrownModel");
            }

            [HarmonyPrefix]
            [HarmonyPatch("SetScale")]
            static bool SetScalePrefix(GrowingPlant __instance, Transform tr, float progress)
            {
                if (!Main.config.fixMelons)
                    return true;

                TechType tt = __instance.seed.plantTechType;
                if (tt == TechType.MelonPlant)
                {
                    float mult = 1.2f;
                    float x = __instance.isIndoor ? __instance.growthWidthIndoor.Evaluate(progress) : __instance.growthWidth.Evaluate(progress);
                    float y = __instance.isIndoor ? __instance.growthHeightIndoor.Evaluate(progress) : __instance.growthHeight.Evaluate(progress);
                    x *= mult;
                    tr.localScale = new Vector3(x, y * mult, x);
                    if (__instance.passYbounds != null)
                        __instance.passYbounds.UpdateWavingScale(tr.localScale);
                    else if (__instance.wavingScaler != null)
                    {
                        __instance.wavingScaler.UpdateWavingScale(tr.localScale);
                    }
                    //AddDebug("SnowStalkerPlant maxProgress " + __instance.maxProgress);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FruitPlant))]
        class FruitPlant_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            public static void StartPrefix(FruitPlant __instance)
            { // wild lantern tree respawns fruits only in creative mode
                if (Main.config.fruitGrowTime > 0)
                {
                    __instance.fruitSpawnEnabled = true;
                    // fruitSpawnInterval will be mult by 'plants growth' from Day night speed mod 
                    __instance.fruitSpawnInterval = Main.config.fruitGrowTime * 1200f;
                }
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("Initialize")]
            public static void InitializePrefix(FruitPlant __instance)
            {
                if (__instance.inactiveFruits == null)
                    AddDebug("inactiveFruits == null");
                if (__instance.fruits == null)
                    AddDebug("fruits == null");
                if (__instance.fruits.Length > 0 && __instance.fruits[0].pickedEvent == null)
                    AddDebug("pickedEvent == null");
            }

            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            public static void InitializePostfix(FruitPlant __instance)
            {
                if (__instance == null)
                    return;

                if (Main.config.creepvineLights && CraftData.GetTechType(__instance.gameObject) == TechType.Creepvine)
                {
                    Light light = __instance.GetComponentInChildren<Light>();
                    //Light[] lights = __instance.GetComponentsInChildren<Light>();
                    //if (lights.Length > 1)
                    //    AddDebug(__instance.name + " LIGHTS " + lights.Length);
                    if (!light)
                        return;
                    //if (__instance.fruits == null)
                    //    AddDebug(__instance.name + " __instance.fruits == null ");

                    light.intensity = creepVineSeedLightInt - (float)__instance.inactiveFruits.Count / (float)__instance.fruits.Length * creepVineSeedLightInt;
                    //AddDebug(__instance.name + " Initialize intensity " + f);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            static bool UpdatePrefix(FruitPlant __instance)
            {
                if (!__instance.fruitSpawnEnabled)
                    return false;

                if (__instance.inactiveFruits.Count > 0 && DayNightCycle.main.timePassed > __instance.timeNextFruit)
                {
                    PickPrefab random = __instance.inactiveFruits.GetRandom();
                    random.SetPickedState(false);
                    __instance.inactiveFruits.Remove(random);
                    __instance.timeNextFruit += __instance.fruitSpawnInterval;
                    //if (Vector3.Distance(Player.main.transform.position, __instance.transform.position) < 5f)
                    //{
                    //    AddDebug(__instance.name + " Spawn fruit");
                    //    AddDebug("fruitSpawnInterval " + __instance.fruitSpawnInterval);
                    //    AddDebug("timeNextFruit " + (int)__instance.timeNextFruit);
                    //}
                    //if (CraftData.GetTechType(__instance.gameObject) != TechType.Creepvine)
                    //    return false;
                    //Light light = __instance.GetComponentInChildren<Light>();
                    //if (light)
                    //{
                    //    float f = creepVineSeedLightInt - (float)__instance.inactiveFruits.Count / (float)__instance.fruits.Length * creepVineSeedLightInt;
                    //    light.intensity = f;
                    //AddDebug("intensity " + f);
                    //}
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(VFXScaleWaving), "Start")]
        class VFXScaleWaving_Patch
        {
            public static bool Prefix(VFXScaleWaving __instance)
            {
                if (__instance.transform.parent == null)
                    return true;

                GrowingPlant gp = __instance.transform.parent.GetComponent<GrowingPlant>();
                if (gp)
                {
                    //AddDebug("GrowingPlant VFXScaleWaving " + __instance.name);
                    __instance.enabled = false;
                    MeshRenderer[] mrs = gp.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrs)
                    {
                        foreach (Material m in mr.materials)
                            m.DisableKeyword("UWE_WAVING");
                    }
                    return false;
                }
                return true;
            }
        }



        [HarmonyPatch(typeof(PickPrefab))]
        class PickPrefab_Patch
        {
            //[HarmonyPatch(nameof(PickPrefab.SetPickedUp))]
            //[HarmonyPostfix]
            static void SetPickedUpPostfix(PickPrefab __instance)
            {
                if (!Main.config.creepvineLights)
                    return;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt != TechType.CreepvineSeedCluster)
                    return;

                //ChildObjectIdentifier coi = __instance.GetComponent<ChildObjectIdentifier>();
                PrefabIdentifier pi = __instance.GetComponentInParent<PrefabIdentifier>();
                if (!pi)
                    return;
                FruitPlant fp = pi.GetComponent<FruitPlant>();
                if (!fp)
                    return;
                Light light = pi.GetComponentInChildren<Light>();
                if (!light)
                    return;
                //AddDebug(" inactiveFruits " + fp.inactiveFruits.Count);
                light.intensity = creepVineSeedLightInt - (float)fp.inactiveFruits.Count / (float)fp.fruits.Length * creepVineSeedLightInt;
            }

            [HarmonyPostfix]
            [HarmonyPatch("SetPickedState")]
            public static void SetPickedStatePostfix(PickPrefab __instance, bool newPickedState)
            {
                //AddDebug(__instance.pickTech + " SetPickedState " + newPickedState);
                if (!Main.config.creepvineLights || __instance.pickTech != TechType.CreepvineSeedCluster)
                    return;

                FruitPlant fp = __instance.GetComponentInParent<FruitPlant>();
                if (!fp)
                    return;

                Light light = fp.GetComponentInChildren<Light>();
                if (light)
                {
                    float inactiveFruits = fp.inactiveFruits.Count;
                    if (!newPickedState)
                        inactiveFruits -= 1;
                    //AddDebug("inactiveFruits " + inactiveFruits);
            
                    light.intensity = creepVineSeedLightInt - inactiveFruits / (float)fp.fruits.Length * creepVineSeedLightInt;
                    //AddDebug("SetPickedState CreepvineSeed " + newPickedState + " " + light.intensity);
                }
            }
        }

        [HarmonyPatch(typeof(Plantable))]
        class Plantable_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnProtoDeserialize")]
            static void OnProtoDeserializePostfix(Plantable __instance)
            {
                if (!Main.config.fixMelons)
                    return;

                if (__instance.plantTechType == TechType.MelonPlant)
                {
                    //AddDebug("Plantable OnProtoDeserialize " + __instance.plantTechType);
                    //AddDebug("Planter AddItem fix " + p.plantTechType);
                    __instance.size = Plantable.PlantSize.Large;
                }

            }

            [HarmonyPostfix]
            [HarmonyPatch("Spawn")]
            public static void SpawnPostfix(ref GameObject __result)
            {
                //AddDebug("Plantable Spawn " + __result.name);
                if (Main.config.randomPlantRotation)
                {
                    Vector3 rot = __result.transform.eulerAngles;
                    float y = UnityEngine.Random.Range(0, 360);
                    __result.transform.eulerAngles = new Vector3(rot.x, y, rot.z);
                }
            }
        }


    }
}
