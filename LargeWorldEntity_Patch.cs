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
        public static Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> techTypesToDespawn = new Dictionary<TechType, int> { };
        static HashSet<TechType> drillables = new HashSet<TechType> { TechType.DrillableAluminiumOxide, TechType.DrillableCopper, TechType.DrillableDiamond, TechType.DrillableGold, TechType.DrillableKyanite, TechType.DrillableLead, TechType.DrillableLithium, TechType.DrillableMagnetite, TechType.DrillableMercury, TechType.DrillableNickel, TechType.DrillableQuartz, TechType.DrillableSalt, TechType.DrillableSilver, TechType.DrillableSulphur, TechType.DrillableTitanium, TechType.DrillableUranium };



        static HashSet<TechType> plantSurfaces = new HashSet<TechType> {TechType.BloodRoot, TechType.BloodOil, TechType.BloodVine, TechType.BluePalm, TechType.KooshChunk, TechType.HugeKoosh, TechType.LargeKoosh, TechType.MediumKoosh, TechType.SmallKoosh, TechType.BulboTreePiece, TechType.BulboTree, TechType.PurpleBranches, TechType.PurpleVegetablePlant, TechType.Creepvine, TechType.AcidMushroom, TechType.WhiteMushroom, TechType.EyesPlant, TechType.FernPalm, TechType.RedRollPlant, TechType.GabeSFeather, TechType.RedGreenTentacle, TechType.JellyPlant, TechType.OrangeMushroom, TechType.SnakeMushroom, TechType.OrangePetalsPlant, TechType.SpikePlant, TechType.MembrainTree, TechType.Melon, TechType.SmallMelon, TechType.MelonPlant, TechType
        .HangingFruitTree, TechType.PurpleVasePlant, TechType.PinkMushroom, TechType.TreeMushroom, TechType.BallClusters, TechType.SmallFanCluster, TechType.SmallFan, TechType.RedConePlant, TechType.RedBush, TechType.SeaCrown, TechType.PurpleRattle, TechType.RedBasketPlant, TechType.ShellGrass, TechType.SpikePlant, TechType.CrashHome, TechType.CrashPowder, TechType.SpottedLeavesPlant, TechType.PurpleFan, TechType.PinkFlower, TechType.PurpleTentacle, TechType.PurpleStalk, TechType.FloatingStone, TechType.BlueLostRiverLilly, TechType.BlueTipLostRiverPlant, TechType.HangingStinger, TechType.CoveTree, TechType.BarnacleSuckers, TechType.BlueCluster};
        static HashSet<TechType> coralSurfaces = new HashSet<TechType> { TechType.BigCoralTubes, TechType.CoralShellPlate, TechType.GenericJeweledDisk, TechType.JeweledDiskPiece };
        static HashSet<string> plantsWithNoTechtype = new HashSet<string> { "Coral_reef_small_deco_03(Clone)", "Coral_reef_small_deco_05(Clone)", "Coral_reef_small_deco_08(Clone)" };
        static HashSet<TechType> techTypesToMakeUnmovable = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleBrainCoral, TechType.HangingFruitTree, TechType.CrashHome, TechType.SpikePlant, TechType.HangingStinger };
        static HashSet<TechType> techTypesToRemoveWavingShader = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleVasePlant, TechType.OrangePetalsPlant, TechType.PinkMushroom, TechType.PurpleRattle, TechType.PinkFlower };
        static HashSet<TechType> techTypesToAddFruits = new HashSet<TechType> { TechType.BloodRoot, TechType.BloodVine, TechType.Creepvine };
        public static GameObject droppedObject;

        public static void ForceBestLODmesh(GameObject go, TechType techType = TechType.None)
        {
            //if (!ConfigToEdit.tweaksAffectingGPU.Value)
            //    return;
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
            //if (!ConfigToEdit.tweaksAffectingGPU.Value)
            lwe.cellLevel = cellLevel;
        }

        static void MakeUnmovable(GameObject go)
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


        public static void DisableWavingShader(Component component)
        {
            foreach (MeshRenderer mr in component.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.materials)
                {
                    //AddDebug(m.shader.name + " DisableKeyword UWE_WAVING");
                    m.DisableKeyword("UWE_WAVING");
                }
            }
        }



        //[HarmonyPatch(typeof(UniqueIdentifier), "Awake")]
        class UniqueIdentifier_Awake_patch
        {
            public static bool Prefix(UniqueIdentifier __instance)
            {
                //if (!Main.gameLoaded)
                //    return true;

                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (!techTypesToDespawn.ContainsKey(tt))
                    return true;

                //Main.logger.LogMessage("UniqueIdentifier Awake " + tt + " pi.id " + __instance.id);
                string currentSlot = SaveLoadManager.main.currentSlot;

                //if (Main.config.objectsSurvivedDespawn.ContainsKey(currentSlot) && Main.config.objectsSurvivedDespawn[currentSlot].Contains(__instance.id))
                //{
                //    Main.logger.LogMessage("UniqueIdentifier objectsSurvidedDespawn " + tt + " pi.id " + __instance.id);
                //    return true;
                //}
                //if (Main.config.objectsDespawned.Contains(__instance.id))
                //{
                //    Main.logger.LogMessage("UniqueIdentifier objectsDespawned " + tt + " pi.id " + __instance.id);
                //    return true;
                //}
                //int rnd = Main.rndm.Next(0, 101);
                //if (techTypesToDespawn[tt] >= rnd)
                //{
                //Main.logger.LogMessage("Destroy UniqueIdentifier " + tt);
                //Main.config.objectsDespawned.Add(__instance.id);
                //UnityEngine.Object.Destroy(__instance.gameObject);
                //return false;
                //}
                //else
                //{
                //    if (String.IsNullOrEmpty(__instance.id))
                //    {
                //        int x = (int)__instance.transform.position.x;
                //        int y = (int)__instance.transform.position.y;
                //        int z = (int)__instance.transform.position.z;
                //        Main.logger.LogMessage("UniqueIdentifier Awake null pi.id " + tt + " " + x + " " + y + " " + z);
                //        return true;
                //    }
                //    Main.logger.LogMessage("save UniqueIdentifier " + tt + " PrefabIdentifier " + __instance.id);
                //    if (Main.config.objectsSurvivedDespawn.ContainsKey(currentSlot))
                //        Main.config.objectsSurvivedDespawn[currentSlot].Add(__instance.id);
                //    else
                //        Main.config.objectsSurvivedDespawn[currentSlot] = new HashSet<string> { __instance.id };
                //}
                return true;
            }
        }

        static IEnumerator Despawn(PrefabIdentifier prefabIdentifier)
        {
            //AddDebug("Test start ");
            //Main.Log("Test start ");
            while (string.IsNullOrEmpty(prefabIdentifier.id))
                yield return null;

            AddDebug("Test end !!! ");
            Main.logger.LogMessage("Test end !!! ");
        }

        static public HashSet<TechType> decoPlants = new HashSet<TechType>();
        static public Dictionary<string, TechType> decoPlantsDic = new Dictionary<string, TechType>();
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
                if (!ConfigToEdit.propCannonGrabsAnyPlant.Value)
                {
                    if (tt != TechType.Creepvine && tt != TechType.Cyclops && tt != TechType.BigCoralTubes && tt != TechType.None && tt != TechType.BloodVine)
                    {
                        if (Util.IsDecoPlant(__instance.gameObject))
                        {
                            //decoPlantsDic[__instance.name] = tt;
                            MakeUnmovable(__instance.gameObject);
                        }
                    }
                }
                if (ConfigMenu.fruitGrowTime.Value > 0 && techTypesToAddFruits.Contains(tt))
                {
                    Util.EnsureFruits(__instance.gameObject);
                    //fff.Add(__instance.gameObject);
                }
                if (drillables.Contains(tt))
                {
                    if (Util.IsGraphicsPresetHighDetail())
                        SetCellLevel(__instance, LargeWorldEntity.CellLevel.Medium);
                }
                if (techTypesToMakeUnmovable.Contains(tt))
                {
                    MakeUnmovable(__instance.gameObject);
                }
                if (techTypesToRemoveWavingShader.Contains(tt))
                {
                    DisableWavingShader(__instance);
                }
                if (plantSurfaces.Contains(tt))
                {
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                }
                else if (coralSurfaces.Contains(tt))
                {
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.coral);
                }
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
                    else if (x == 86 && y == -33 && z == -334)
                    { // let player swim thru it
                        __instance.transform.position = new Vector3(__instance.transform.position.x, -33.3f, __instance.transform.position.z);
                    }
                    else if (x == 448 && y == -77 && z == -7)
                    { // let player swim thru it
                        __instance.transform.position = new Vector3(__instance.transform.position.x, __instance.transform.position.y, -6.5f);
                    }
                    //86.651 -33.781 -334.973
                }
                //else if (tt == TechType.PurpleTentacle && __instance.name == "Coral_reef_purple_tentacle_plant_01_02(Clone)")
                //    AlwaysUseHiPolyMesh(__instance.gameObject);
                //else if (tt == TechType.BluePalm && __instance.name == "coral_reef_plant_small_01_03(Clone)")
                //    AlwaysUseHiPolyMesh(__instance.gameObject);
                //else if (tt == TechType.Boomerang)
                //    AlwaysUseHiPolyMesh(__instance.gameObject, TechType.Boomerang);
                //else if (tt == TechType.LargeFloater)
                //    AlwaysUseHiPolyMesh(__instance.gameObject, TechType.LargeFloater);
                //else if (tt == TechType.WhiteMushroom)
                //    AlwaysUseHiPolyMesh(__instance.gameObject);
                else if (tt == TechType.BloodVine)
                {
                    LiveMixin lm = __instance.GetComponent<LiveMixin>();
                    if (lm)
                        UnityEngine.Object.Destroy(lm);
                }
                else if (tt == TechType.CrashHome || tt == TechType.CrashPowder)
                {
                    int x = (int)__instance.transform.position.x;
                    int y = (int)__instance.transform.position.y;
                    int z = (int)__instance.transform.position.z;
                    if ((x == 280 && y == -40 && z == -195) || (x == 272 && y == -41 && z == -199))
                        __instance.transform.Rotate(90, 0, 0);
                }
                else if (tt == TechType.PurpleBranches || tt == TechType.SnakeMushroom || tt == TechType.PurpleStalk)
                { // things in shroom cave
                    if (Util.IsGraphicsPresetHighDetail())
                        SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                }
                else if (tt == TechType.FloatingStone) // ?
                {
                    //SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                }
                //else if (tt == TechType.HangingStinger)
                //{
                //    EnableCreepvineShader(__instance);
                //}
                else if (tt == TechType.Floater)
                {
                    AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.organic);
                }
                else if (tt == TechType.PurpleFan) // veined nettle
                { // disable collision, allow scanning
                    BoxCollider bc = __instance.GetComponentInChildren<BoxCollider>();
                    bc.gameObject.layer = LayerID.Useable;
                    bc.isTrigger = true;
                }
                else if (tt == TechType.PurpleTentacle) // writhing weed
                { // disable collision, allow scanning
                    BoxCollider bc = __instance.GetComponentInChildren<BoxCollider>();
                    bc.gameObject.layer = LayerID.Useable;
                    bc.isTrigger = true;
                    bc.size = new Vector3(bc.size.x, bc.size.y, bc.size.z * 3);
                }
                else if (tt == TechType.FarmingTray && __instance.name == "Base_exterior_Planter_Tray_01_abandoned(Clone)")
                {
                    MakeUnmovable(__instance.gameObject);
                }
                else if (tt == TechType.None)
                {
                    //if (__instance.GetComponent<StoreInformationIdentifier>() && Main.config.biomesRemoveLight.Contains(Player.main.GetBiomeString()))
                    //{
                    //    Light light = __instance.GetComponent<Light>();
                    //    if (light && light.enabled && __instance.transform.childCount == 0)
                    //        light.enabled = false;
                    //}
                    if (__instance.name == "Land_tree_01(Clone)")
                    { // remove stupid light
                        //ForceBestLODmesh(__instance.gameObject);
                        foreach (MeshRenderer mr in __instance.GetComponentsInChildren<MeshRenderer>())
                        {
                            foreach (Material m in mr.materials)
                                m.DisableKeyword("MARMO_EMISSION");
                        }
                    }
                    else if (__instance.name.StartsWith("Crab_snake_mushrooms"))
                    { // small shrooms have no techtype
                        if (Util.IsGraphicsPresetHighDetail())
                            SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                    }
                    else if (__instance.name.StartsWith("coral_reef_Stalactite"))
                    { // Stalactites in shroom cave
                      //AddDebug(__instance.name + " cellLevel " + __instance.cellLevel);
                        if (Util.IsGraphicsPresetHighDetail())
                            SetCellLevel(__instance, LargeWorldEntity.CellLevel.Far);
                    }
                    else if (__instance.name.StartsWith("ExplorableWreck_"))
                    {
                        AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
                    }
                    else if (plantsWithNoTechtype.Contains(__instance.name))
                    {
                        BoxCollider bc = __instance.GetComponentInChildren<BoxCollider>();
                        if (bc)
                        {
                            bc.isTrigger = true;
                            bc.gameObject.layer = LayerID.Useable;
                        }
                        AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                    }
                    return;
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
                //if (removeLight.Contains(tt))
                //{
                //    MeshRenderer[] mrs = __instance.GetComponentsInChildren<MeshRenderer>();
                //    foreach (MeshRenderer mr in mrs)
                //    {
                //        if (mr.GetComponentInParent<ChildObjectIdentifier>())
                //            continue; // skip fruits

                //        foreach (Material m in mr.materials)
                //        {
                //            m.DisableKeyword("MARMO_EMISSION");
                //            //m.DisableKeyword("MARMO_SPECMAP"); 
                //        }
                //    }
                //    //AddDebug(__instance.name + " removeLight ");
                //    Light[] lights = __instance.GetComponentsInChildren<Light>();
                //    foreach (Light l in lights)
                //        l.enabled = false;
                //}

            }

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            {
                //TechType tt = CraftData.GetTechType(__instance.gameObject);

                //if (techTypesToDespawn.ContainsKey(tt))
                //{
                //PrefabIdentifier pi = __instance.GetComponent<PrefabIdentifier>();
                //if (Main.gameLoaded && pi && string.IsNullOrEmpty(pi.id))
                //{
                //    Main.logger.LogMessage("LargeWorldEntity Start null pi.id " + __instance.name);
                //    if (Testing.prefabIdentifier == null)
                //    {
                //        AddDebug("LargeWorldEntity Start null pi.id " + __instance.name);
                //        Testing.prefabIdentifier = pi;
                //        UWE.CoroutineHost.StartCoroutine(Test(pi));
                //    }
                //    return;
                //}
                //}
                if (__instance.transform.position.y < 1f && __instance.name.StartsWith("FloatingStone") && !__instance.name.EndsWith("Floaters(Clone)")) // -6 -13
                //if (__instance.name.StartsWith("FloatingStone"))
                {// make boulders that block cave entrances not fall down when world chunk unloads
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

            [HarmonyPrefix]
            [HarmonyPatch("StartFading")]
            public static bool StartFadingPrefix(LargeWorldEntity __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                //AddDebug("StartFading " + __instance.name);
                if (Util.spawning)
                {
                    //AddDebug("spawning " + __instance.name);
                    Util.spawning = false;
                    return false;
                }
                if (Creature_Tweaks.pickupShinies.Contains(__instance.gameObject))
                {
                    //AddDebug("StartFading pickupShinies " + __instance.name);
                    return false;
                }
                if (__instance.gameObject == droppedObject)
                {
                    //AddDebug("StartFading droppedObject " + __instance.name);
                    droppedObject = null;
                    return false;
                }
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                switch (tt)
                {
                    case TechType.Titanium:
                    case TechType.Copper:
                    case TechType.Silver:
                    case TechType.Gold:
                    case TechType.Lead:
                    case TechType.Diamond:
                    case TechType.Lithium:
                    case TechType.JeweledDiskPiece:
                        return false;
                        //default:
                        //    break;
                }
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
