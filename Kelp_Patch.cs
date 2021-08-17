using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Kelp_Patch
    { // need to test spawning seeds while they are disabled
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

        //[HarmonyPatch(typeof(FruitPlant))]
        class FruitPlant_Patch
        {
            [HarmonyPatch(nameof(FruitPlant.Start))]
            [HarmonyPostfix]
            static void OnTriggerEnterPostfix(FruitPlant __instance)
            {
                UniqueIdentifier ui = __instance.GetComponentInParent<UniqueIdentifier>();
                if (ui)
                    AddDebug(" FruitPlant.Start " + ui.name);
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
