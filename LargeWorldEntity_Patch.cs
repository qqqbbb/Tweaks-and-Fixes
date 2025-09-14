using BepInEx;
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
        public static HashSet<string> harvest = new HashSet<string>();

        public static Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> techTypesToDespawn = new Dictionary<TechType, int> { };
        static HashSet<TechType> drillables = new HashSet<TechType> { TechType.DrillableAluminiumOxide, TechType.DrillableCopper, TechType.DrillableDiamond, TechType.DrillableGold, TechType.DrillableKyanite, TechType.DrillableLead, TechType.DrillableLithium, TechType.DrillableMagnetite, TechType.DrillableMercury, TechType.DrillableNickel, TechType.DrillableQuartz, TechType.DrillableSalt, TechType.DrillableSilver, TechType.DrillableSulphur, TechType.DrillableTitanium, TechType.DrillableUranium };



        static HashSet<TechType> plantSurfaces = new HashSet<TechType> {TechType.BloodRoot, TechType.BloodOil, TechType.BloodVine, TechType.BluePalm, TechType.KooshChunk, TechType.HugeKoosh, TechType.LargeKoosh, TechType.MediumKoosh, TechType.SmallKoosh, TechType.BulboTreePiece, TechType.BulboTree, TechType.PurpleBranches, TechType.PurpleVegetablePlant, TechType.Creepvine, TechType.AcidMushroom, TechType.WhiteMushroom, TechType.EyesPlant, TechType.FernPalm, TechType.RedRollPlant, TechType.GabeSFeather, TechType.RedGreenTentacle, TechType.JellyPlant, TechType.OrangeMushroom, TechType.SnakeMushroom, TechType.OrangePetalsPlant, TechType.SpikePlant, TechType.MembrainTree, TechType.Melon, TechType.SmallMelon, TechType.MelonPlant, TechType
        .HangingFruitTree, TechType.PurpleVasePlant, TechType.PinkMushroom, TechType.TreeMushroom, TechType.BallClusters, TechType.SmallFanCluster, TechType.SmallFan, TechType.RedConePlant, TechType.RedBush, TechType.SeaCrown, TechType.PurpleRattle, TechType.RedBasketPlant, TechType.ShellGrass, TechType.SpikePlant, TechType.CrashHome, TechType.CrashPowder, TechType.SpottedLeavesPlant, TechType.PurpleFan, TechType.PinkFlower, TechType.PurpleTentacle, TechType.PurpleStalk, TechType.FloatingStone, TechType.BlueLostRiverLilly, TechType.BlueTipLostRiverPlant, TechType.HangingStinger, TechType.CoveTree, TechType.BarnacleSuckers, TechType.BlueCluster};
        static HashSet<TechType> coralSurfaces = new HashSet<TechType> { TechType.BigCoralTubes, TechType.CoralShellPlate, TechType.GenericJeweledDisk, TechType.JeweledDiskPiece, TechType.CoralChunk };
        static HashSet<string> plantsWithNoTechtype = new HashSet<string> { "Coral_reef_small_deco_03(Clone)", "Coral_reef_small_deco_05(Clone)", "Coral_reef_small_deco_08(Clone)" };
        static HashSet<TechType> techTypesToMakeUnmovable = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleBrainCoral, TechType.HangingFruitTree, TechType.SpikePlant };
        static HashSet<TechType> techTypesToRemoveWavingShader = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleVasePlant, TechType.OrangePetalsPlant, TechType.PinkMushroom, TechType.PurpleRattle, TechType.PinkFlower };
        static HashSet<TechType> fruitTechTypes = new HashSet<TechType> { TechType.BloodRoot, TechType.BloodVine, TechType.Creepvine };
        static HashSet<TechType> techTypesToAddWorldForces = new HashSet<TechType> { TechType.CoralChunk };
        public static GameObject droppedObject;
        public static bool spawning;

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
            //Main.logger.LogMessage("MakeUnmovable " + go.name);
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb)
                UnityEngine.Object.Destroy(rb);

            WorldForces wf = go.GetComponent<WorldForces>();
            if (wf)
                UnityEngine.Object.Destroy(wf);
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

        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Awake_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(LargeWorldEntity __instance)
            {
                HandleLWE(__instance);
            }

            private static void HandleLWE(LargeWorldEntity __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogMessage("LargeWorldEntity Awake " + __instance.name + " " + tt);
                //if (Vector3.Distance(__instance.transform.position, Player.main.transform.position) < 3f)
                //    Main.logger.LogMessage("Closest LargeWorldEntity " + __instance.name + " " + tt);
                if (!ConfigToEdit.propCannonGrabsAnyPlant.Value)
                {
                    if (tt != TechType.Creepvine && tt != TechType.Cyclops && tt != TechType.BigCoralTubes && tt != TechType.None && tt != TechType.BloodVine && tt != TechType.Seamoth)
                    {
                        if (Util.IsDecoPlant(__instance.gameObject))
                            MakeUnmovable(__instance.gameObject);
                    }
                }
                if (ConfigMenu.fruitGrowTime.Value > 0 && fruitTechTypes.Contains(tt))
                {
                    Util.EnsureFruits(__instance.gameObject);
                }
                if (drillables.Contains(tt))
                {
                    if (Util.IsGraphicsPresetHighDetail())
                        SetCellLevel(__instance, LargeWorldEntity.CellLevel.Medium);
                }
                if (techTypesToAddWorldForces.Contains(tt))
                {
                    AddWorldForces(__instance.gameObject);
                }
                if (techTypesToMakeUnmovable.Contains(tt))
                {
                    //AddDebug("techTypesToMakeUnmovable MakeUnmovable " + __instance.name);
                    MakeUnmovable(__instance.gameObject);
                }
                if (ConfigToEdit.disableWeirdPlantAnimation.Value && techTypesToRemoveWavingShader.Contains(tt))
                {
                    DisableWavingShader(__instance);
                }
                if (plantSurfaces.Contains(tt))
                {
                    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                }
                else if (coralSurfaces.Contains(tt))
                {
                    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.coral);
                }
                if (tt == TechType.BigCoralTubes)
                {
                    FixCoralTubesPositions(__instance);
                }
                else if (tt == TechType.Creepvine)
                {
                    Vector3 pos = __instance.transform.position;
                    int x = (int)pos.x;
                    int y = (int)pos.y;
                    int z = (int)pos.z;
                    if (x == 29 && y == -51 && z == -472) // not attached to ground
                        __instance.transform.position = new Vector3(pos.x, -55f, pos.z);
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
                else if (tt == TechType.Floater)
                {
                    Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.organic);
                }
                else if (tt == TechType.PurpleFan) // veined nettle
                { // disable collision, allow scanning
                    DisableCollision(__instance);
                }
                else if (tt == TechType.PurpleTentacle) // writhing weed
                { // disable collision, allow scanning
                    DisableCollision(__instance, new Vector3(1, 1, 3));
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
                        Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.metal);
                    }
                    else if (plantsWithNoTechtype.Contains(__instance.name))
                    {
                        DisableCollision(__instance);
                        Util.AddVFXsurfaceComponent(__instance.gameObject, VFXSurfaceTypes.vegetation);
                    }
                    return;
                }
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

            private static void TestHarvest(TechType techType)
            {
                HarvestType harvestType = TechData.GetHarvestType(techType);
                if (harvestType != HarvestType.None)
                {
                    TechType harvestOutput = TechData.GetHarvestOutput(techType);
                    harvest.Add($"{techType} harvest {harvestType} {harvestOutput}");
                }
                //AddDebug("AddHarvestResourceToExosuit harvestType " + harvestType);
            }

            private static void FixCoralTubesPositions(Component component)
            {
                int x = (int)component.transform.position.x;
                int y = (int)component.transform.position.y;
                int z = (int)component.transform.position.z;
                if (x == 47 && y == -34 && z == -6)
                {
                    component.transform.position = new Vector3(component.transform.position.x, component.transform.position.y, -6.815f);
                }
                else if (x == -20 && y == -28 && z == -381)
                {
                    component.transform.position = new Vector3(component.transform.position.x, -28.62f, component.transform.position.z);
                }
                else if (x == 86 && y == -33 && z == -334)
                { // let player swim thru it
                    component.transform.position = new Vector3(component.transform.position.x, -33.3f, component.transform.position.z);
                }
                else if (x == 448 && y == -77 && z == -7)
                { // let player swim thru it
                    component.transform.position = new Vector3(component.transform.position.x, component.transform.position.y, -6.5f);
                }
                //86.651 -33.781 -334.973
            }

            private static void DisableCollision(Component component, Vector3 sizeMult = default)
            {
                BoxCollider bc = component.GetComponentInChildren<BoxCollider>();
                if (bc)
                {
                    bc.gameObject.layer = LayerID.Useable;
                    bc.isTrigger = true;
                    if (sizeMult != default)
                        bc.size = new Vector3(bc.size.x * sizeMult.x, bc.size.y * sizeMult.y, bc.size.z * sizeMult.z);
                }
            }

            private static void AddWorldForces(GameObject gameObject, float underWaterGravity = float.MaxValue)
            {
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if (rb == null)
                    return;

                WorldForces wf = gameObject.EnsureComponent<WorldForces>();
                if (underWaterGravity != float.MaxValue)
                    wf.underwaterGravity = underWaterGravity;

                //worldForces.underwaterDrag = 11;
                wf.useRigidbody = rb;
                rb.isKinematic = false;
                rb.useGravity = false;
                //if (WorldForcesManager.instance.m_AllForces.Contains(worldForces) == false)
                WorldForcesManager.Instance.AddWorldForces(wf);
            }

            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            {
                if (__instance.transform.position.y < 1f && __instance.name.StartsWith("FloatingStone") && !__instance.name.EndsWith("Floaters(Clone)")) // -6 -13
                { // fix bug: rocks that block cave entrance fall down bc they load before terrain 
                    SetCellLevel(__instance, LargeWorldEntity.CellLevel.Near);
                }
            }

            //[HarmonyPostfix, HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(LargeWorldEntity __instance)
            {
            }

            [HarmonyPrefix]
            [HarmonyPatch("StartFading")]
            public static bool StartFadingPrefix(LargeWorldEntity __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                //AddDebug("StartFading " + __instance.name);
                if (spawning)
                {
                    //AddDebug("spawning " + __instance.name);
                    spawning = false;
                    return false;
                }
                if (Creatures.pickupShinies.Contains(__instance.gameObject))
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
