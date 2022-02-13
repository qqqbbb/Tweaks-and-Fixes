using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Plants_Patch
    {
        public static float creepVineSeedLightInt = 2f;
        public static Dictionary<GameObject, int> enteredColliders = new Dictionary<GameObject, int> ();
        static Dictionary<GameObject, HashSet<GameObject>> disabledChildren = new Dictionary<GameObject, HashSet<GameObject>>();

        static void ToggleKelp(GameObject go, bool enable)
        {
            //if (enable)
            //    AddDebug("enable " + go.name);
            //else
            //    AddDebug("disable " + go.name);

            //AddDebug("enteredColliders " + enteredColliders.Count);
            MeshRenderer[] meshRenderers = go.GetAllComponentsInChildren<MeshRenderer>();
            disabledChildren[go] = new HashSet<GameObject>();
            foreach (MeshRenderer mr in meshRenderers)
            {
                //Main.Log("disabledChildren " + mr.name);
                //mr.enabled = enable;
                if (mr.gameObject.activeSelf)
                {
                    mr.gameObject.SetActive(enable);
                    disabledChildren[go].Add(mr.gameObject);
                }
            }
            //AddDebug("meshRenderers " + meshRenderers.Length);
            Transform lightTr = go.transform.Find("light");
            if (lightTr)
            {
                lightTr.gameObject.SetActive(enable);
                disabledChildren[go].Add(lightTr.gameObject);
            }
            Transform zoneTr = go.transform.Find("zone");
            if (zoneTr)
            {
                zoneTr.gameObject.SetActive(enable);
                disabledChildren[go].Add(zoneTr.gameObject);
            }
        }

        [HarmonyPatch(typeof(GrowingPlant))]
        class GrowingPlant_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(GrowingPlant __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug(__instance.name + " GrowingPlant OnEnable " + tt);
                string name = __instance.name;
                if (name == "GrowingBulboTreePiece(Clone)")
                {
                    MeshRenderer[] mrs = __instance.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrs)
                    {
                        foreach (Material m in mr.materials)
                        {
                            //AddDebug(m.shader.name + " DisableKeyword UWE_WAVING");
                            m.DisableKeyword("UWE_WAVING");
                        }
                    }
                }
                else if (name == "GrowingMembrainTree(Clone)")
                {   //  all LOD meshes look the same, render distance is too small
                    LargeWorldEntity_Patch.AlwaysUseHiPolyMesh(__instance.gameObject);
                }
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("GetGrowthDuration")]
            public static bool GetGrowthDurationPrefix(GrowingPlant __instance, ref float __result)
            {
                //__result = __instance.growthDuration * Main.config.plantGrowthTimeMult * (NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f);
                return false;
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
                    float num = __instance.isIndoor ? __instance.growthWidthIndoor.Evaluate(progress) : __instance.growthWidth.Evaluate(progress);
                    float y = __instance.isIndoor ? __instance.growthHeightIndoor.Evaluate(progress) : __instance.growthHeight.Evaluate(progress);
                    num *= mult;
                    tr.localScale = new Vector3(num, y * mult, num);
                    if (__instance.passYbounds != null)
                        __instance.passYbounds.UpdateWavingScale(tr.localScale);
                    else
                    {
                        if (__instance.wavingScaler != null)
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
            { // lantern tree respawns fruits only in creative mode
                __instance.fruitSpawnEnabled = true;
                // fruitSpawnInterval will be mult by 'plants growth' from Day night speed mod 
                __instance.fruitSpawnInterval = Main.config.fruitGrowTime * 1200f;
            }
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            public static void InitializePostfix(FruitPlant __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == TechType.Creepvine)
                {
                    Light light = __instance.GetComponentInChildren<Light>();
                    //Light[] lights = __instance.GetComponentsInChildren<Light>();
                    //if (lights.Length > 1)
                    //    AddDebug(__instance.name + " LIGHTS " + lights.Length);
                    if (!light)
                        return;

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
                    //AddDebug(__instance.name + " Spawn fruit");
                    PickPrefab random = __instance.inactiveFruits.GetRandom();
                    random.SetPickedState(false);
                    __instance.inactiveFruits.Remove(random);
                    __instance.timeNextFruit += __instance.fruitSpawnInterval;
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
                if (__instance.transform.parent)
                {
                    GrowingPlant gp = __instance.transform.parent.GetComponent<GrowingPlant>();
                    if (gp)
                    {
                        //AddDebug("GrowingPlant VFXScaleWaving");
                        __instance.enabled = false;
                        MeshRenderer[] mrs = gp.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in mrs)
                        {
                            foreach (Material m in mr.materials)
                                m.DisableKeyword("UWE_WAVING");
                        }
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(VFXPassYboundsToMat))]
        class VFXPassYboundsToMat_Patch
        {
            [HarmonyPatch(nameof(VFXPassYboundsToMat.GetMinAndMaxYpos))]
            [HarmonyPrefix]
            static bool GetMinAndMaxYposPostfix(VFXPassYboundsToMat __instance)
            { // fix seeds from grown creepvine not moving with it
                __instance.minYpos = float.PositiveInfinity;
                __instance.maxYpos = -__instance.minYpos;
                __instance.renderers = __instance.gameObject.GetComponentsInChildren<Renderer>(true);
                for (int index = 0; index < __instance.renderers.Length; ++index)
                {
                    Bounds bounds = __instance.renderers[index].bounds;
                    if (bounds.max.y > __instance.maxYpos)
                    {
                        bounds = __instance.renderers[index].bounds;
                        __instance.maxYpos = bounds.max.y;
                    }
                    bounds = __instance.renderers[index].bounds;
                    if (bounds.min.y < __instance.minYpos)
                    {
                        bounds = __instance.renderers[index].bounds;
                        __instance.minYpos = bounds.min.y;
                    }
                }
                float num1 = __instance.minYpos + __instance.minYposScalar * (__instance.maxYpos - __instance.minYpos);
                float num2 = __instance.maxYpos + ((__instance.maxYposScalar - 1f) * (__instance.maxYpos - __instance.minYpos));
                __instance.block = new MaterialPropertyBlock();
                for (int index = 0; index < __instance.renderers.Length; ++index)
                {
                    __instance.renderers[index].GetPropertyBlock(__instance.block);
                    __instance.block.SetFloat(ShaderPropertyID._minYpos, num1);
                    __instance.block.SetFloat(ShaderPropertyID._maxYpos, num2);
                    if (__instance.scaleWaving && __instance.transform.localScale != Vector3.one)
                        __instance.block.SetVector(ShaderPropertyID._ScaleModifier,__instance.transform.localScale - Vector3.one);
                    __instance.renderers[index].SetPropertyBlock(__instance.block);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Planter), "AddItem", new Type[1] { typeof(InventoryItem) })]
        class Planter_AddItem_Patch
        {
            static void Prefix(Planter __instance, InventoryItem item)
            {
                if (!Main.config.fixMelons)
                    return;

                Plantable p = item.item.GetComponent<Plantable>();
                if (p && p.plantTechType == TechType.MelonPlant)
                {
                    //AddDebug("Planter AddItem fix " + p.plantTechType);
                    p.size = Plantable.PlantSize.Large;
                }
            }
        }

        [HarmonyPatch(typeof(PickPrefab))]
        class PickPrefab_Patch
        {
            //[HarmonyPatch(nameof(PickPrefab.SetPickedUp))]
            //[HarmonyPostfix]
            static void SetPickedUpPostfix(PickPrefab __instance)
            {
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
            [HarmonyPatch("SetPickedState")]
            [HarmonyPostfix]
            public static void SetPickedStatePostfix(PickPrefab __instance, bool newPickedState)
            {
                //AddDebug(__instance.pickTech + " SetPickedState " + newPickedState);
                //if (newPickedState)
                //    return;
                if (__instance.pickTech == TechType.CreepvineSeedCluster)
                {
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
                Vector3 rot = __result.transform.eulerAngles;
                float y = UnityEngine.Random.Range(0, 360);
                __result.transform.eulerAngles = new Vector3(rot.x, y, rot.z);
            }
        }


        //[HarmonyPatch(typeof(InteractionVolumeCollider))]
        class InteractionVolumeUser_Patch
        {
            [HarmonyPatch(nameof(InteractionVolumeCollider.OnTriggerEnter))]
            [HarmonyPostfix]
            static void OnTriggerEnterPostfix(InteractionVolumeCollider __instance, Collider other)
            {
                //if (enteredColliders.ContainsKey(__instance))
                //    return;
                if (CraftData.GetTechType(__instance.gameObject) != TechType.Creepvine)
                    return;
                if (CraftData.GetTechType(other.gameObject) != TechType.Seamoth)
                    return;
                // they have multiple InteractionVolumeCollider
                if (__instance.transform.parent.name == "physics")
                {
                    UniqueIdentifier ui = __instance.GetComponentInParent<UniqueIdentifier>();
                    if (ui)
                    { // seeds may be already disabled 
                        AddDebug("OnTriggerEnter " + ui.name);
                        if (enteredColliders.ContainsKey(ui.gameObject))
                            enteredColliders[ui.gameObject]++;
                        else
                            enteredColliders[ui.gameObject] = 1;

                        //if (enteredColliders[ui.gameObject] == 1)
                        if (!disabledChildren.ContainsKey(ui.gameObject))
                            ToggleKelp(ui.gameObject, false);
                    }
                }
                else if (__instance.GetComponent<UniqueIdentifier>() && __instance.transform.Find("physics") == null)
                {
                    AddDebug("OnTriggerEnter " + __instance.name);
                    //enteredColliders[__instance] = __instance.transform.name;
                    ToggleKelp(__instance.gameObject, false);
                }
            }
         
            [HarmonyPatch(nameof(InteractionVolumeCollider.OnTriggerExit))]
            [HarmonyPostfix]
            static void OnTriggerExitPostfix(InteractionVolumeCollider __instance, Collider other)
            {

                if (CraftData.GetTechType(__instance.gameObject) != TechType.Creepvine)
                    return;
                if (CraftData.GetTechType(other.gameObject) != TechType.Seamoth)
                    return;

                if (__instance.transform.parent.name == "physics")
                {
                    UniqueIdentifier ui = __instance.GetComponentInParent<UniqueIdentifier>();
                    if (ui)
                    {
                        if (enteredColliders.ContainsKey(ui.gameObject))
                        {
                            enteredColliders[ui.gameObject]--;
                            AddDebug("OnTriggerExit " + ui.name);
                            AddDebug("enteredColliders " + enteredColliders[ui.gameObject]);
                            if (enteredColliders[ui.gameObject] > 0)
                                return;
                        }
                        //if (enteredColliders[ui.gameObject] < 0)
                        //    AddDebug("enteredColliders < 0 " + ui.name);
                        //ToggleKelp(ui.gameObject, true);

                        foreach (GameObject mr in disabledChildren[ui.gameObject])
                        {
                            mr.gameObject.SetActive(true);
                            //disabledChildren[go].Add(mr.gameObject);
                        }
                        //disabledChildren[ui.gameObject] = new HashSet<GameObject>();
                        disabledChildren.Remove(ui.gameObject);
                    }
                }
                else if (__instance.GetComponent<UniqueIdentifier>() && __instance.transform.Find("physics") == null)
                {
                    AddDebug("OnTriggerExit " + __instance.name);
                    AddDebug("disabledChildren " + disabledChildren[__instance.gameObject].Count);
                    foreach (GameObject mr in disabledChildren[__instance.gameObject])
                    {
                        mr.gameObject.SetActive(true);
                        //disabledChildren[go].Add(mr.gameObject);
                    }
                    //disabledChildren[__instance.gameObject] = new HashSet<GameObject>();
                    disabledChildren.Remove(__instance.gameObject);
                }
            }
        }


    }
}
