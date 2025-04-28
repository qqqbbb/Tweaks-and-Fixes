using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;
     
namespace Tweaks_Fixes
{
    class Databox_Light
    {
        public static HashSet<GameObject> databoxLights = new HashSet<GameObject>();
        
        public static GameObject GetClosestToPlayer()
        {
            GameObject closest = null;
            //float shortestDist = float.PositiveInfinity;
            //Main.Log("GetClosetToPlayer " + databoxLights.Count);
            foreach (GameObject go in databoxLights)
            {
                if (go && go.activeSelf)
                {
                    float distance = (go.transform.position - Player.main.transform.position).magnitude;
                    //Main.Log("distance " + distance);
                    if (distance < 5f)
                    {
                        closest = go;
                        break;
                    }
                }
            }
            return closest;
        }

        [HarmonyPatch(typeof(GenericHandTarget), "OnHandClick")]
        class GenericHandTarget_OnHandClick_Patch
        {
            public static void Postfix(GenericHandTarget __instance)
            {
                if (__instance.GetComponent<BlueprintHandTarget>())
                {
                    GameObject closestLight = GetClosestToPlayer();
                    if (closestLight != null)
                    {
                        //AddDebug("remove light");
                        closestLight.SetActive(false);
                        databoxLights.Remove(closestLight);
                    }

                }
            }
        }

        [HarmonyPatch(typeof(VFXVolumetricLight), "Awake")]
        class VFXVolumetricLight_Awake_Patch
        {
            public static void Postfix(VFXVolumetricLight __instance)
            {
                if (__instance.transform.parent.name.StartsWith("DataboxLight"))
                {
                    //Main.Log("VFXVolumetricLight Awake parent " + __instance.transform.parent.name);
                    databoxLights.Add(__instance.transform.parent.gameObject);
                }
            }
        }

        
    }
}
