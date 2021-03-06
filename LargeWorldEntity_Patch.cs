using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class LargeWorldEntity_Patch
    { // biomes to remove light BloodKelp_Trench
        public static HashSet<TechType> removeLight = new HashSet<TechType> { };

        public static void AlwaysUseHiPolyMesh(GameObject go, TechType techType = TechType.None)
        {
            //AddDebug("AlwaysUseHiPolyMesh " + go.name);
            if (techType == TechType.Boomerang)// dont disable FP model
                go = go.transform.Find("model").gameObject;
            LODGroup lod = go.GetComponentInChildren<LODGroup>();
            if (lod == null)
                return;

            lod.enabled = false;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            //AddDebug(go.name + " AlwaysUseHiPolyMesh " + renderers.Length);
            for (int i = 1; i < renderers.Length; i++)
                renderers[i].enabled = false;
        }

        static void MakeImmuneToCannon(GameObject go)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb)
                UnityEngine.Object.Destroy(rb);
            WorldForces wf = go.GetComponent<WorldForces>();
            if (wf)
                UnityEngine.Object.Destroy(wf);
        }

        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Awake_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(LargeWorldEntity __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //if (Vector3.Distance(__instance.transform.position, Player.main.transform.position) < 3f)
                //    Main.Log("Closest LargeWorldEntity " + __instance.name + " " + tt);
                if (tt == TechType.BigCoralTubes)
                {// fix  clipping with terrain 
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == 47 && y == -34 && z == -6)
                    { 
                        __instance.transform.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y, -6.815f);
                    }
                    else if (x == -20 && y == -28 && z == -381)
                    { 
                        __instance.transform.position = new Vector3(__instance.transform.position.x, -28.62f, __instance.transform.position.z);
                    }
                }
                else if (tt == TechType.MembrainTree)
                {
                    if (__instance.transform.parent == null)
                        return; // has just grown in planter  
                    else if (__instance.GetComponent<GrownPlant>())
                        return; // spawned in planter
                    //AddDebug(" fix  MembrainTree " + __instance.name);
                    SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
                    renderers[0].transform.Rotate(90f, 0f, 0f);
                }
                else if (tt == TechType.PurpleTentacle && __instance.name == "Coral_reef_purple_tentacle_plant_01_02(Clone)")
                    AlwaysUseHiPolyMesh(__instance.gameObject);
                else if (tt == TechType.BluePalm && __instance.name == "coral_reef_plant_small_01_03(Clone)")
                    AlwaysUseHiPolyMesh(__instance.gameObject);
                else if (tt == TechType.Boomerang)
                    AlwaysUseHiPolyMesh(__instance.gameObject, TechType.Boomerang);
                else if (tt == TechType.LargeFloater)
                    AlwaysUseHiPolyMesh(__instance.gameObject, TechType.LargeFloater);
                else if (tt == TechType.BulboTree || tt == TechType.PurpleVasePlant || tt == TechType.OrangePetalsPlant || tt == TechType.PinkMushroom || tt == TechType.PurpleRattle)
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
                    if (tt == TechType.BulboTree)
                        MakeImmuneToCannon(__instance.gameObject);
                }
                else if (tt == TechType.FarmingTray || tt == TechType.PurpleBrainCoral || tt == TechType.HangingFruitTree)
                {
                    MakeImmuneToCannon(__instance.gameObject);
                }
                else if (tt == TechType.WhiteMushroom)
                    AlwaysUseHiPolyMesh(__instance.gameObject);
                else if (tt == TechType.BloodRoot || tt == TechType.BloodVine || tt == TechType.Creepvine)
                {
                    PickPrefab[] pickPrefabs = __instance.gameObject.GetComponentsInChildren<PickPrefab>(true);
                    if (pickPrefabs.Length > 0)
                    {
                        FruitPlant fp = __instance.gameObject.EnsureComponent<FruitPlant>();
                        fp.fruitSpawnEnabled = true;
                        //AddDebug(__instance.name + " fruitSpawnInterval orig " + fp.fruitSpawnInterval);
                        // fruitSpawnInterval will be mult by 'plants growth' from Day night speed mod 
                        fp.fruitSpawnInterval = Main.config.fruitGrowTime * 1200f;
                        //AddDebug(__instance.name + " fruitSpawnInterval " + fp.fruitSpawnInterval);
                        if (fp.fruitSpawnInterval == 0f)
                            fp.fruitSpawnInterval = 1;
                        //AddDebug(__instance.name + " fruitSpawnInterval after " + fp.fruitSpawnInterval);
                        fp.fruits = pickPrefabs;
                        if (tt == TechType.Creepvine)
                        {
                            TechTag techTag = __instance.gameObject.EnsureComponent<TechTag>();
                            techTag.type = TechType.Creepvine;
                        }
                    }
                }

                else if (tt == TechType.CrashHome || tt == TechType.CrashPowder)
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if ((x == 280 && y == -40 && z == -195) || (x == 272 && y == -41 && z == -199))
                        __instance.transform.Rotate(90, 0, 0);
                }
                else if (tt == TechType.None)
                {
                    if (__instance.GetComponent<StoreInformationIdentifier>() && Main.config.biomesRemoveLight.Contains(Player.main.GetBiomeString()))
                    {
                        Light light = __instance.GetComponent<Light>();
                        if (light && light.enabled && __instance.transform.childCount == 0)
                            light.enabled = false;
                    }
                    else if (__instance.name == "coral_reef_small_deco_12(Clone)")
                        AlwaysUseHiPolyMesh(__instance.gameObject);
                    //else if (__instance.name == "Coral_reef_ball_clusters_01_Light(Clone)")
                    //    AlwaysUseHiPolyMesh(__instance.gameObject);
                    else if (__instance.name == "Land_tree_01(Clone)")
                    {
                        MeshRenderer[] mrs = __instance.GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer mr in mrs)
                        {
                            foreach (Material m in mr.materials)
                                m.DisableKeyword("MARMO_EMISSION");
                        }
                    }
                    else if (__instance.transform.position.y < 1f && __instance.name.StartsWith("FloatingStone") && !__instance.name.EndsWith("Floaters(Clone)"))
                    { // make boulders that block cave entrances not fall down when world chunk unloads
                        //Floater[] floaters = __instance.GetAllComponentsInChildren<Floater>();
                        //if (floaters.Length == 0)
                        //if (__instance.transform.position.y < 1f && __instance.GetComponent<FloatersTarget>() == null)
                        {
                            //AddDebug(__instance.name + " CellLevel " + __instance.cellLevel);
                            //Main.Log(__instance.name + "  " + __instance.transform.position);
                            //Main.Log(__instance.name + " classId " + __instance.GetComponent<PrefabIdentifier>().classId);
                            __instance.cellLevel = LargeWorldEntity.CellLevel.Near;
                        }
                    }
                }
                else if (tt.ToString() == "TF_Stone")
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if (x == -63 && y == -16 && z == -223)
                        __instance.gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }

                if (removeLight.Contains(tt))
                {
                    MeshRenderer[] mrs = __instance.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer mr in mrs)
                    {
                        if (mr.GetComponentInParent<ChildObjectIdentifier>())
                            continue; // skip fruits
                        foreach (Material m in mr.materials)
                        {
                            m.DisableKeyword("MARMO_EMISSION");
                            //m.DisableKeyword("MARMO_SPECMAP"); 
                        }
                    }
                    //AddDebug(__instance.name + " removeLight ");
                    Light[] lights = __instance.GetComponentsInChildren<Light>();
                    foreach (Light l in lights)
                        l.enabled = false;
                }

            }
            [HarmonyPrefix]
            [HarmonyPatch("StartFading")]
            public static bool StartFadingPrefix(LargeWorldEntity __instance)
            {
                if (!Main.loadingDone)
                    return false;
                else if (Tools_Patch.releasingGrabbedObject)
                {
                    Tools_Patch.releasingGrabbedObject = false;
                    //AddDebug("StartFading releasingGrabbedObject " + __instance.name);
                    return false;
                }
                else if (Tools_Patch.repCannonGOs.Contains(__instance.gameObject))
                {
                    //AddDebug("StartFading rep Cannon go " + __instance.name);
                    Tools_Patch.repCannonGOs.Remove(__instance.gameObject);
                    return false;
                }
                return true;
            }
        }


    }
}
