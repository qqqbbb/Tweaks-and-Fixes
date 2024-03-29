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
    {
        public static HashSet<TechType> removeLight = new HashSet<TechType> { };
        public static Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };

        public static void AlwaysUseHiPolyMesh(GameObject go, TechType techType = TechType.None)
        {
            if (!ConfigToEdit.tweaksAffectingGPU.Value)
                return;
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

        public static void SetCellLevel(LargeWorldEntity lwe, LargeWorldEntity.CellLevel cellLevel)
        {
            if (!ConfigToEdit.tweaksAffectingGPU.Value && cellLevel > lwe.cellLevel)
                return;

            lwe.cellLevel = cellLevel;
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

        public static void AddVFXsurfaceComponent(GameObject go, VFXSurfaceTypes type)
        {
            VFXSurface vFXSurface = go.EnsureComponent<VFXSurface>();
            vFXSurface.surfaceType = type;
        }
    
        private static void RemoveLivemixin(GameObject go)
        {
            LiveMixin lm = go.GetComponent<LiveMixin>();
            if (lm)
                UnityEngine.Object.Destroy(lm);
        }

        public static void DisableWavingShader(LargeWorldEntity __instance)
        {
            foreach (MeshRenderer mr in __instance.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.materials)
                {
                    //AddDebug(m.shader.name + " DisableKeyword UWE_WAVING");
                    m.DisableKeyword("UWE_WAVING");
                }
            }
        }

        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Awake_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(LargeWorldEntity __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogMessage("LargeWorldEntity Awake " + __instance.name + " " + tt);
                //if (Vector3.Distance(__instance.transform.position, Player.main.transform.position) < 3f)
                //    Main.logger.LogMessage("Closest LargeWorldEntity " + __instance.name + " " + tt);

                if(tt == TechType.BigCoralTubes)
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
                    else if (x == 86 && y == -33 && z == -334)
                    { // let player swim thru it
                        __instance.transform.position = new Vector3(__instance.transform.position.x, -33.3f, __instance.transform.position.z);
                    }
                    //86.651 -33.781 -334.973
                }
                //else if (tt == TechType.MembrainTree)
                //{
                    //if (__instance.transform.parent == null)
                    //    return; // has just grown in planter  
                    //else if (__instance.GetComponent<GrownPlant>())
                    //    return; // spawned in planter
                                //AddDebug(" fix  MembrainTree " + __instance.name);

                    //AlwaysUseHiPolyMesh(__instance.gameObject);
                    //model / Coral_reef_membrain_tree_01_25
                //}
                //else if (tt == TechType.PurpleTentacle && __instance.name == "Coral_reef_purple_tentacle_plant_01_02(Clone)")
                //    AlwaysUseHiPolyMesh(__instance.gameObject);
                //else if (tt == TechType.BluePalm && __instance.name == "coral_reef_plant_small_01_03(Clone)")
                //    AlwaysUseHiPolyMesh(__instance.gameObject);
                //else if (tt == TechType.Boomerang)
                //    AlwaysUseHiPolyMesh(__instance.gameObject, TechType.Boomerang);
                //else if (tt == TechType.LargeFloater)
                //    AlwaysUseHiPolyMesh(__instance.gameObject, TechType.LargeFloater);
                else if (tt == TechType.BulboTree || tt == TechType.PurpleVasePlant || tt == TechType.OrangePetalsPlant || tt == TechType.PinkMushroom || tt == TechType.PurpleRattle)
                {
                    DisableWavingShader(__instance);
                    if (tt == TechType.BulboTree)
                    { 
                        //AlwaysUseHiPolyMesh(__instance.gameObject);
                        MakeImmuneToCannon(__instance.gameObject);
                    }
                    //else if (tt == TechType.PurpleVasePlant)
                    //    AlwaysUseHiPolyMesh(__instance.gameObject);
                }
                else if (tt == TechType.PurpleBrainCoral || tt == TechType.HangingFruitTree)
                {
                    MakeImmuneToCannon(__instance.gameObject);
                }
                //else if (tt == TechType.WhiteMushroom)
                //    AlwaysUseHiPolyMesh(__instance.gameObject);
                else if (tt == TechType.BloodRoot || tt == TechType.BloodVine)
                {
                    if (Main.config.fruitGrowTime > 0)
                        Util.EnsureFruits(__instance.gameObject);
                }
                else if (tt == TechType.Creepvine)
                {
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                    if (Main.config.fruitGrowTime > 0)
                        Util.EnsureFruits(__instance.gameObject);
                }
                else if (tt == TechType.CrashHome || tt == TechType.CrashPowder)
                {
                    if (tt == TechType.CrashHome)
                        MakeImmuneToCannon(__instance.gameObject);

                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if ((x == 280 && y == -40 && z == -195) || (x == 272 && y == -41 && z == -199))
                        __instance.transform.Rotate(90, 0, 0);
                }
                else if (tt == TechType.PurpleBranches)
                { // tall purple plant in shroom cave
                    SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                }
                else if (tt == TechType.SnakeMushroom)
                {
                    SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                }
                else if (tt == TechType.FloatingStone) // ?
                {
                    //SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                }
                //else if (tt == TechType.CreepvineSeedCluster)
                //{
                        //Util.MakeEatable(__instance.gameObject, creepVineSeedFood * .5f, creepVineSeedFood, false);
                //}
                else if (tt == TechType.CoralShellPlate)
                {
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.coral);
                    MakeImmuneToCannon(__instance.gameObject);
                }
                else if (tt == TechType.Floater)
                {
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.organic);
                }
                else if (tt == TechType.SpikePlant)
                {
                    MakeImmuneToCannon(__instance.gameObject);
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                }
                else if (tt == TechType.None)
                {
                    if (__instance.GetComponent<StoreInformationIdentifier>() && Main.config.biomesRemoveLight.Contains(Player.main.GetBiomeString()))
                    {
                        Light light = __instance.GetComponent<Light>();
                        if (light && light.enabled && __instance.transform.childCount == 0)
                            light.enabled = false;
                    }
                    //else if (__instance.name == "coral_reef_small_deco_12(Clone)")
                    //    AlwaysUseHiPolyMesh(__instance.gameObject);
                    //else if (__instance.name == "Coral_reef_ball_clusters_01_Light(Clone)")
                    //    AlwaysUseHiPolyMesh(__instance.gameObject);
                    else if (__instance.name == "Land_tree_01(Clone)")
                    {
                        AlwaysUseHiPolyMesh(__instance.gameObject);
                        foreach (MeshRenderer mr in __instance.GetComponentsInChildren<MeshRenderer>())
                        {
                            foreach (Material m in mr.materials)
                                m.DisableKeyword("MARMO_EMISSION");
                        }
                    }
                    else if (__instance.name.StartsWith("Crab_snake_mushrooms"))
                    { // small shrooms have no techtype
                        SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                    }
                    else if (__instance.name.StartsWith("coral_reef_Stalactite"))
                    { // Stalactites in shroom cave
                        //AddDebug(__instance.name + " cellLevel " + __instance.cellLevel);
                        SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                    }
                    else if (__instance.name.StartsWith("ExplorableWreck_"))
                    {
                        AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
                    }
                    //else if (__instance.name == "Coral_reef_small_deco_03(Clone)")
                    //{ // short purple plant
                    //    BoxCollider bc = __instance.GetComponentInChildren<BoxCollider>();
                    //    if (bc)
                    //        UnityEngine.Object.Destroy(bc);
                    //}
                }
                //else if (tt.ToString() == "TF_Stone")
                //{
                //    int x = (int)__instance.transform.position.x;
                //    int y = (int)__instance.transform.position.y;
                //    int z = (int)__instance.transform.position.z;
                //    if (x == -63 && y == -16 && z == -223)
                //        __instance.gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                //}
                if (eatableFoodValue.ContainsKey(tt))
                {
                    Util.MakeEatable(__instance.gameObject, eatableFoodValue[tt]);
                }
                if (eatableWaterValue.ContainsKey(tt))
                {
                    Util.MakeDrinkable(__instance.gameObject, eatableWaterValue[tt]);
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

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            {
                if (__instance.transform.position.y < 1f && __instance.name.StartsWith("FloatingStone") && !__instance.name.EndsWith("Floaters(Clone)"))
                //if (__instance.name.StartsWith("FloatingStone"))
                {
                  //Floater[] floaters = __instance.GetAllComponentsInChildren<Floater>();
                  //if (floaters.Length == 0)
                  //if (__instance.transform.position.y < 1f && __instance.GetComponent<FloatersTarget>() == null)
                    {
                        //Rigidbody rb = __instance.GetComponent<Rigidbody>();
                        //if (rb)
                        {
                            //AddDebug("LWE Start " + __instance.name + " CellLevel " + __instance.cellLevel);
                            //rb.isKinematic = true;
                        }
                        //else
                        //    AddDebug(__instance.name + " CellLevel " + __instance.cellLevel + " " + x + " " + y + " " + z + " ");
                        //Main.Log(__instance.name + "  " + __instance.transform.position);
                        //Main.Log(__instance.name + " classId " + __instance.GetComponent<PrefabIdentifier>().classId);
                        SetCellLevel(__instance, LargeWorldEntity.CellLevel.Near);
                    }
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("UpdateCell")]
            static void UpdateCellPostfix(LargeWorldEntity __instance, LargeWorldStreamer streamer)
            { // make boulders that block cave entrances not fall down when world chunk unloads
                if (!__instance.name.StartsWith("FloatingStone"))
                    return;

                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                if (rb == null)
                    return;

                float dist = Vector3.Distance(__instance.transform.position, MainCamera.camera.transform.position);
                //if (rb && Testing.rbToTest == rb)
                //    AddDebug("FloatingStone UpdateCell isKinematic " + rb.isKinematic + " " + dist);

                //rb.isKinematic = dist > Main.config.detectCollisionsDist;
            }

            [HarmonyPrefix]
            [HarmonyPatch("StartFading")]
            public static bool StartFadingPrefix(LargeWorldEntity __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt == TechType.Titanium || tt == TechType.Copper || tt == TechType.Silver || tt == TechType.Gold || tt == TechType.Lead || tt == TechType.Diamond || tt == TechType.Lithium)
                {
                    //AddDebug("StartFading " + __instance.name);
                    return false;
                }
                if (Creature_Tweaks.pickupShinies.TryGetValue(__instance.gameObject, out string s))
                {
                    //AddDebug("StartFading pickupShinies " + __instance.name);
                    return false;
                }
                //    AddDebug("StartFading " + __instance.name);
                //else if (Tools_Patch.releasingGrabbedObject)
                {
                    //Tools_Patch.releasingGrabbedObject = false;
                    //AddDebug("StartFading releasingGrabbedObject " + __instance.name);
                    //return false;
                }
                //else if (Tools_Patch.repCannonGOs.Contains(__instance.gameObject))
                {
                    //AddDebug("StartFading rep Cannon go " + __instance.name);
                    //    Tools_Patch.repCannonGOs.Remove(__instance.gameObject);
                    //    return false;
                }
                return true;
            }
        }

        
          
    }
}
