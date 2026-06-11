using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Escape_pod_wrecks
    {
        [HarmonyPatch(typeof(ResourceTracker))]
        class ResourceTracker_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(ResourceTracker __instance)
            {
                if (__instance.techType == TechType.Wreck && __instance.name == "life_pod_exploded_3(Clone)")
                    UWE.CoroutineHost.StartCoroutine(FixEscapePod3(__instance.gameObject));
            }

            private static IEnumerator FixEscapePod3(GameObject go)
            { // cant replace texture in prefab
                yield return new WaitForFrames(1);
                Transform exterior = go.transform.Find("life_pod_exploded_02_01/exterior");
                Transform life_pod_damaged = exterior.Find("life_pod_damaged");
                Renderer renderer = life_pod_damaged.GetComponent<Renderer>();
                Material[] sharedMats = renderer.sharedMaterials;
                sharedMats[1].mainTexture = sharedMats[2].mainTexture;
                sharedMats[1].SetFloat(PrefabFixer.zOffset, 0);
                sharedMats[4].SetFloat(PrefabFixer.zOffset, 0);
                sharedMats[7].SetFloat(PrefabFixer.zOffset, 0);
                Transform life_pod_pontoons_damaged_01 = exterior.Find("life_pod_pontoons_damaged_01");
                renderer = life_pod_pontoons_damaged_01.GetComponent<Renderer>();
                renderer.material.SetFloat(PrefabFixer.zOffset, 0);
                Transform Life_pod_no_pontoons = exterior.Find("Life_pod_no_pontoons");
                renderer = Life_pod_no_pontoons.GetComponent<Renderer>();
                renderer.materials[1].SetFloat(PrefabFixer.zOffset, 0);
                //renderer.material.EnableKeyword("UWE_DITHERALPHA");
            }

        }

    }
}