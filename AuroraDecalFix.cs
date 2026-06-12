using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class AuroraDecalFix
    {
        public static Material materialForDecals;

        [HarmonyPatch(typeof(WaterPlane), "Start")]
        internal class WaterPlane_Start_Patch
        {
            static void Postfix(WaterPlane __instance)
            { // water surface in Aurora
                //GameObject root = Util.GetEntityRoot(__instance.gameObject);
                //AddDebug("WaterPlane Start " + root.name);
                __instance.transform.DisableShadowCasting();
            }
        }

        [HarmonyPatch(typeof(CrashedShipExploder), "Start")]
        class CrashedShipExploder_Start_Patch
        {
            static void Postfix(CrashedShipExploder __instance)
            {
                __instance.transform.DisableShadowCasting("starship_expoded/model_LODs/starship_exploded_02/starship_exploded_interior_decals");
                Transform t = __instance.transform.Find("explodedFX/Ship_Interior_PowerRoomFX(Clone)");
                FixDecals(t);
                __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.metal);
                t = __instance.transform.Find("starship_expoded/model_LODs/starship_exploded_02/starship_exploded_interior_03");
                Renderer renderer = t.GetComponent<Renderer>();
                Material material = renderer.materials[4];
                material.SetFloat(PrefabFixer.zOffset, 0);
            }

            public static void FixDecals(Transform t)
            {
                Renderer[] renderers = t.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    Texture decalTexture = renderer.material.mainTexture;
                    renderer.material = materialForDecals;
                    renderer.material.mainTexture = decalTexture;
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

        }

        public static IEnumerator GetMaterialForDecals()
        {
            GameObject prefab = null;
            IPrefabRequest prefabTask = PrefabDatabase.GetPrefabAsync("4605151e-dea4-4ba7-96bf-2f88b3b41bdb"); // starfish_02
            yield return prefabTask;
            prefabTask.TryGetPrefab(out prefab);
            GameObject obj = UnityEngine.Object.Instantiate(prefab);
            materialForDecals = obj.GetComponentInChildren<Renderer>().material;
            materialForDecals.DisableKeyword("MARMO_EMISSION");
            materialForDecals.DisableKeyword("MARMO_SPECMAP");
        }
    }
}
