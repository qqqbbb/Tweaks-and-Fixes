using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;
// need to test spawning seeds while they are disabled
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

        [HarmonyPatch(typeof(GrowingPlant), "OnEnable")]
        class GrowingPlant_OnEnable_Patch
        {
            public static void Postfix(GrowingPlant __instance)
            {
                if(__instance.name == "GrowingBulboTreePiece(Clone)")
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
            }
        }
               
        [HarmonyPatch(typeof(FruitPlant))]
        class FruitPlant_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(nameof(FruitPlant.Start))]
            public static void StartPrefix(FruitPlant __instance)
            { // lantern tree respawns fruits only in creative mode
                __instance.fruitSpawnEnabled = true;
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(FruitPlant.Initialize))]
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
            [HarmonyPatch(nameof(FruitPlant.Update))]
            [HarmonyPrefix]
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
                    if (CraftData.GetTechType(__instance.gameObject) != TechType.Creepvine)
                        return false;
                    Light light = __instance.GetComponentInChildren<Light>();
                    if (light)
                    {
                        float f = creepVineSeedLightInt - (float)__instance.inactiveFruits.Count / (float)__instance.fruits.Length * creepVineSeedLightInt;
                        light.intensity = f;
                        //AddDebug("intensity " + f);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(VFXScaleWaving))]
        class VFXScaleWaving_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            public static bool GetGrowthDurationPrefix(VFXScaleWaving __instance)
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

        [HarmonyPatch(typeof(PickPrefab))]
        class PickPrefab_Patch
        {
            [HarmonyPatch(nameof(PickPrefab.SetPickedUp))]
            [HarmonyPostfix]
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
