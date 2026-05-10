using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UWE;
using static ErrorMessage;



namespace Tweaks_Fixes
{ // bad terrain 331 -40 -225
    class LargeWorldEntity_
    {
        class PosRotData
        {
            public string name;
            public Vector3 newPos;
            public Vector3 newRot;

            public PosRotData(string name, Vector3 newPos, Vector3 newRot)
            {
                this.name = name;
                this.newPos = newPos;
                this.newRot = newRot;
            }
        }

        public static Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public static Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };
        static HashSet<TechType> drillables = new HashSet<TechType> { TechType.DrillableAluminiumOxide, TechType.DrillableCopper, TechType.DrillableDiamond, TechType.DrillableGold, TechType.DrillableKyanite, TechType.DrillableLead, TechType.DrillableLithium, TechType.DrillableMagnetite, TechType.DrillableMercury, TechType.DrillableNickel, TechType.DrillableQuartz, TechType.DrillableSalt, TechType.DrillableSilver, TechType.DrillableSulphur, TechType.DrillableTitanium, TechType.DrillableUranium };

        static HashSet<TechType> plantSurfaces = new HashSet<TechType> {TechType.BloodRoot, TechType.BloodOil, TechType.BloodVine, TechType.BluePalm, TechType.KooshChunk, TechType.HugeKoosh, TechType.LargeKoosh, TechType.MediumKoosh, TechType.SmallKoosh, TechType.BulboTreePiece, TechType.BulboTree, TechType.PurpleBranches, TechType.PurpleVegetablePlant, TechType.Creepvine, TechType.AcidMushroom, TechType.WhiteMushroom, TechType.EyesPlant, TechType.FernPalm, TechType.RedRollPlant, TechType.GabeSFeather, TechType.RedGreenTentacle, TechType.JellyPlant, TechType.OrangeMushroom, TechType.SnakeMushroom, TechType.OrangePetalsPlant, TechType.SpikePlant, TechType.MembrainTree, TechType.Melon, TechType.SmallMelon, TechType.MelonPlant, TechType
        .HangingFruitTree, TechType.PurpleVasePlant, TechType.PinkMushroom, TechType.TreeMushroom, TechType.BallClusters, TechType.SmallFanCluster, TechType.SmallFan, TechType.RedConePlant, TechType.RedBush, TechType.SeaCrown, TechType.PurpleRattle, TechType.RedBasketPlant, TechType.ShellGrass, TechType.SpikePlant, TechType.CrashHome, TechType.CrashPowder, TechType.SpottedLeavesPlant, TechType.PurpleFan, TechType.PinkFlower, TechType.PurpleTentacle, TechType.PurpleStalk, TechType.FloatingStone, TechType.BlueLostRiverLilly, TechType.BlueTipLostRiverPlant, TechType.HangingStinger, TechType.CoveTree, TechType.BarnacleSuckers, TechType.BlueCluster};
        static HashSet<TechType> coralSurfaces = new HashSet<TechType> { TechType.BigCoralTubes, TechType.CoralShellPlate, TechType.GenericJeweledDisk, TechType.JeweledDiskPiece, TechType.CoralChunk };
        static HashSet<string> plantsWithNoTechtype = new HashSet<string> { "Coral_reef_small_deco_03(Clone)", "Coral_reef_small_deco_05(Clone)", "Coral_reef_small_deco_08(Clone)" };
        static HashSet<TechType> techTypesToMakeUnmovable = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleBrainCoral, TechType.HangingFruitTree, TechType.SpikePlant };
        static HashSet<string> objectsToMakeUnmovable = new HashSet<string> { "Wrecks_VentCover_Aurora(Clone)", "Starship_exploded_debris_01(Clone)", "Starship_exploded_debris_06(Clone)", "Starship_exploded_debris_19(Clone)", "Starship_exploded_debris_20(Clone)", "Starship_exploded_debris_22(Clone)", "Starship_exploded_debris_38(Clone)" };
        static HashSet<TechType> techTypesToRemoveWavingShader = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleVasePlant, TechType.OrangePetalsPlant, TechType.PinkMushroom, TechType.PurpleRattle, TechType.PinkFlower };
        static HashSet<TechType> fruitTechTypes = new HashSet<TechType> { TechType.BloodRoot, TechType.BloodVine, TechType.Creepvine };
        static HashSet<TechType> techTypesToAddWorldForces = new HashSet<TechType> { TechType.CoralChunk };
        public static HashSet<TechType> fragments;
        public static Dictionary<string, List<string>> hotMetalDebris = new Dictionary<string, List<string>> {
            { "Starship_exploded_debris_02(Clone)", null },
            { "Starship_exploded_debris_13(Clone)", null },
            { "Starship_exploded_debris_14(Clone)",null },
            {"Starship_exploded_debris_15(Clone)", null },
            {"Starship_exploded_debris_16(Clone)", null },
            {"Starship_exploded_debris_22(Clone)", null },
            {"Starship_exploded_debris_30(Clone)", null },
            {"Starship_exploded_debris_31(Clone)", null },
            {"Starship_exploded_debris_32(Clone)", null },
            {"Starship_exploded_debris_33(Clone)", null },
            {"Starship_exploded_debris_34(Clone)", null },
            {"Starship_exploded_debris_35(Clone)", null },
            {"Starship_exploded_debris_36(Clone)", null },
            {"Starship_exploded_debris_37(Clone)", null },
            {"explorable_wreckage_modular_wall_details_01(Clone)",null },
            {"explorable_wreckage_modular_room_details_03(Clone)",null },
            {"explorable_wreckage_modular_room_details_06(Clone)",null },
            {"explorable_wreckage_modular_room_details_07(Clone)",null },
            {"explorable_wreckage_modular_room_details_08(Clone)",null },
            {"explorable_wreckage_modular_room_details_10(Clone)",null },
            {"explorable_wreckage_modular_room_details_11(Clone)",null },
            {"explorable_wreckage_modular_room_details_14(Clone)",null },
            {"explorable_wreckage_modular_room_details_23(Clone)",null },
            {"life_pod_exploded_2(Clone)", null },
            {"life_pod_exploded_3(Clone)", null },
            {"life_pod_exploded_4(Clone)", null },
            {"life_pod_exploded_6(Clone)", null },
            {"life_pod_exploded_7(Clone)", null },
            {"life_pod_exploded_12(Clone)", null },
            {"life_pod_exploded_13(Clone)",null },
            {"life_pod_exploded_17(Clone)",null },
            {"life_pod_exploded_19(Clone)",null },
            {"CrashedShip_pipes_room(Clone)",null },
            {"Wreck(Clone)",null },
            {"base_hull_crack_02(Clone)",null },
            {"base_hull_crack_03(Clone)",null },

            {"ExplorableWreck_Grassy_1(Clone)", new List<string>{ "explorable_wreckage_02/exterior_03_hull/exterior_03_hull", "explorable_wreckage_02/hull/hull", "explorable_wreckage_02/exterior_01", "explorable_wreckage_02/exterior_03_hull", "explorable_wreckage_02/exterior_02_hull", "explorable_wreckage_02/room_03/room_03" } },
            {"ExplorableWreck_Grassy_2(Clone)", new List<string>{"ExplorableWreck2_clean/explorable_wreckage_03/room_04/room_04/exterior_04", "ExplorableWreck2_clean/explorable_wreckage_03/room_07/room_07/room_07_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_08/room_08/room_08_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_11/room_11/room_11_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_06/room_06/room_06_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_05/room_05/room_05_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_10/room_10/room_10_exterior",  "ExplorableWreck2_clean/explorable_wreckage_03/exterior_01/exterior_01", "ExplorableWreck2_clean/explorable_wreckage_03/hull_03/hull_03", "ExplorableWreck2_clean/explorable_wreckage_03/hull_01/hull_01", "ExplorableWreck2_clean/explorable_wreckage_03/room_04/room_04/room_04_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_01/room_01/room_01_exterior" }},
            {"ExplorableWreck_Dunes_6(Clone)", new List<string>{"ExplorableWreck2_clean/explorable_wreckage_03/room_04/room_04/room_04_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_04/room_04/exterior_04", "ExplorableWreck2_clean/explorable_wreckage_03/exterior_01/exterior_01", "ExplorableWreck2_clean/explorable_wreckage_03/hull_01/hull_01", "ExplorableWreck2_clean/explorable_wreckage_03/room_07/room_07/room_07_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_06/room_06/room_06_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_01/room_01/room_01_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/exterior_02/exterior_02", "ExplorableWreck2_clean/explorable_wreckage_03/room_11/room_11/room_11_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_05/room_05/room_05_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_08/room_08/room_08_exterior"}},
            {"ExplorableWreck_BloodKelp_8(Clone)", new List<string>{"ExplorableWreck1_clean/explorable_wreckage_02/hull/hull",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_01/exterior_01_LODs",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_01/exterior_01",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_03_hull/exterior_03_hull_LODs",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_03_hull/exterior_03_hull",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_02_hull/exterior_02_LODs",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_02_hull/exterior_02_hull",
                "ExplorableWreck1_clean/explorable_wreckage_02/room_03/room_03" }},
            {"ExplorableWreck_GrandReef_10(Clone)", new List<string>{"ExplorableWreck1_clean/explorable_wreckage_02/exterior_02_hull/exterior_02_hull", "ExplorableWreck1_clean/explorable_wreckage_02/exterior_01/exterior_01", "ExplorableWreck1_clean/explorable_wreckage_02/exterior_03_hull/exterior_03_hull", "ExplorableWreck1_clean/explorable_wreckage_02/hull/hull", "ExplorableWreck1_clean/explorable_wreckage_02/room_06/room_06", "ExplorableWreck1_clean/explorable_wreckage_02/room_03/room_03", "ExplorableWreck1_clean/explorable_wreckage_02/room_02/room_02"}},
            {"ExplorableWreck_Grassy_12(Clone)", new List<string>{"ExplorableWreck1_clean/explorable_wreckage_02/exterior_02_hull/exterior_02_hull", "ExteriorProps"}},
            {"ExplorableWreck_KooshZone_3(Clone)", new List<string>{ "ExplorableWreck1_clean/explorable_wreckage_02/exterior_03_hull",
                "ExplorableWreck1_clean/explorable_wreckage_02/exterior_02_hull", "ExplorableWreck1_clean/explorable_wreckage_02/exterior_01", "ExplorableWreck1_clean/explorable_wreckage_02/room_02/room_02", "ExplorableWreck1_clean/explorable_wreckage_02/room_03/room_03" }},
             {"ExplorableWreck_Grassy_14(Clone)", new List<string>{ "ExteriorProps"}},
             {"ExplorableWreck_SafeShallows_15(Clone)", new List<string>{ "ExteriorProps"}},
             {"ExplorableWreck_SafeShallows_16(Clone)", new List<string>{ "ExteriorProps"}},
             {"ExplorableWreck_KelpForest_17(Clone)", new List<string>{ "Vent1/ExteriorProps", "Room1/ExteriorProps"}},
             {"ExplorableWreck_KelpForest_18(Clone)", new List<string>{ "Room1/ExteriorProps", "Room2/ExteriorProps"}},
             {"ExplorableWreck_Mountains_19(Clone)", new List<string>{ "ExteriorProps", "Room3/ExteriorProps", "Room1/ExteriorProps", "Room2/ExteriorProps" }},
             {"ExplorableWreck_SparseReef_20(Clone)", new List<string>{ "Room1/ExteriorProps", "Room2/ExteriorProps",  }},
             {"ExplorableWreck_Mountains_5(Clone)", new List<string>{ "ExplorableWreck2_clean/explorable_wreckage_03/exterior_01/exterior_01", "ExplorableWreck2_clean/explorable_wreckage_03/exterior_02/exterior_02", "ExplorableWreck2_clean/explorable_wreckage_03/room_09/room_09/room_09_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_09/room_09/room_09_exterior_2", "ExplorableWreck2_clean/explorable_wreckage_03/room_08/room_08/room_08_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_07/room_07/room_07_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/hull_01/hull_01", "ExplorableWreck2_clean/explorable_wreckage_03/room_06/room_06/room_06_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_04/room_04/room_04_exterior", "ExplorableWreck2_clean/explorable_wreckage_03/room_04/room_04/exterior_04", "ExplorableWreck2_clean/explorable_wreckage_03/room_05/room_05/room_05_exterior"}},
             {"ExplorableWreck_GrandReef_11(Clone)", new List<string>{ "ExteriorProps" }},
             {"ExplorableWreck_UnderwaterIslands_4(Clone)", new List<string>{ "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_04/room_04/exterior_04", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_04/room_04/room_04_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/hull_01/hull_01", "/ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_06/room_06/room_06_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_07/room_07/room_07_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/hull_03/hull_03", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_06/room_06/room_06_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_11/room_11/room_11_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_01/room_01/room_01_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/exterior_01/exterior_01", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_08/room_08/room_08_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_09/room_09/room_09_exterior_2", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/exterior_02/exterior_02", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_09/room_09/room_09_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_08/room_08", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_05/room_05/room_05_exterior" }},
             {"ExplorableWreck_TreaderPath_7(Clone)", new List<string>{ "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_10/room_10/room_10_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/exterior_02/exterior_02", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/exterior_02/exterior_02", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_09/room_09/room_09_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_04/room_04/exterior_04", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_05/room_05/room_05_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_11/room_11/room_11_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_04/room_04/room_04_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_06/room_06/room_06_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_07/room_07/room_07_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_01/room_01/room_01_exterior" , "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/hull_03/hull_03", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/hull_01/hull_01", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_08/room_08/room_08_exterior", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/room_09/room_09/room_09_exterior_2", "ExplorableWreck2_clean(Clone)/explorable_wreckage_03/exterior_01/exterior_01" }},
             {"ExplorableWreck_MushroomForest_9(Clone)", new List<string>{ "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/hull/hull", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/exterior_02_hull/exterior_02_hull", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/exterior_02_hull/exterior_02_LODs", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/exterior_01/exterior_01_LODs", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/exterior_01/exterior_01", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/exterior_03_hull/exterior_03_hull_LODs", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/exterior_03_hull/exterior_03_hull", "ExplorableWreck1_clean(Clone)/explorable_wreckage_02/room_06/room_06" }},
             {"ExplorableWreck_Grassy_13(Clone)", new List<string>{ "ExteriorProps", "Vent/explorable_wreckage_modular_room_details_15(Clone)/explorable_wreckage_modular_room_details_15", "Vent/explorable_wreckage_modular_room_details_19(Clone)/explorable_wreckage_modular_room_details_19" }},
        };



        public static GameObject droppedObject;
        public static bool spawning;

        static Dictionary<Vector3Int, List<PosRotData>> newPosRots = new Dictionary<Vector3Int, List<PosRotData>>  {
            {new Vector3Int(101, -266, -368), new List<PosRotData>{
            new PosRotData("Moon_Pool_fragment_02(Clone)", default, new Vector3(0, 300, 0)) }},

            {new Vector3Int(280, -40, -195), new List<PosRotData>{
                new PosRotData("CrashHome(Clone)", new Vector3(float.NaN, -40, float.NaN), new Vector3(70, 0, 0)),
                new PosRotData("CrashPowder(Clone)", new Vector3(float.NaN, -40, float.NaN), new Vector3(70, 0, 0)),
                }},

            {new Vector3Int(272, -41, -199),new List<PosRotData>{
                new PosRotData("CrashHome(Clone)", new Vector3(float.NaN, -41.3f, float.NaN), new Vector3(50, 320, 0)),
                new PosRotData("CrashPowder(Clone)", new Vector3(float.NaN, -41.3f, float.NaN), new Vector3(50, 320, 0)) }},
            {new Vector3Int(935, 12, 9),new List<PosRotData>{
                new PosRotData("cyclopsenginefragment3(Clone)", new Vector3(935.85f, 12.5f, 9.75f), default) }},
            {new Vector3Int(966, 10, 15),new List<PosRotData>{
                new PosRotData("cyclopsenginefragment3(Clone)", new Vector3(966.4f, 10f, 15.4f), default) }},
            {new Vector3Int(955, 11, 7),new List<PosRotData>{
                new PosRotData("cyclopsenginefragment2(Clone)", default, new Vector3(float.NaN, 35, float.NaN)) }},
            {new Vector3Int(29, -51, -472),new List<PosRotData>{
                new PosRotData("Coral_reef_kelp_young_01(Clone)", new Vector3(float.NaN, -55, float.NaN), default) }},
            {new Vector3Int(47, -34, -6),new List<PosRotData>{
                new PosRotData("Coral_reef_shell_tunnel_02(Clone)", new Vector3(float.NaN, float.NaN, -6.815f), default) }},
            {new Vector3Int(-20, -28, -381),new List<PosRotData>{
                new PosRotData("Coral_reef_shell_tunnel_02(Clone)", new Vector3(float.NaN, -28.62f, float.NaN), default) }},
            {new Vector3Int(86, -33, -334),new List<PosRotData>{
                new PosRotData("Coral_reef_shell_tunnel_01(Clone)", new Vector3(float.NaN, -33.4f, float.NaN), new Vector3(350, 100, 90)) }},
            {new Vector3Int(448, -77, -8),new List<PosRotData>{
                new PosRotData("Coral_reef_shell_tunnel_02(Clone)", new Vector3(float.NaN, float.NaN, -6.5f), default) }},

         };

        static Dictionary<TechType, VFXSurfaceTypes> surfaceTypes_ = new Dictionary<TechType, VFXSurfaceTypes> { {TechType.SeamothFragment, VFXSurfaceTypes.metal },
            {TechType.Trashcans, VFXSurfaceTypes.metal},
            {TechType.StarshipDesk, VFXSurfaceTypes.metal},
            {TechType.SingleWallShelf, VFXSurfaceTypes.metal},
            {TechType.ExosuitFragment, VFXSurfaceTypes.metal},
            {TechType.LabCounter, VFXSurfaceTypes.metal},
            {TechType.BarTable, VFXSurfaceTypes.glass},
            {TechType.VendingMachine, VFXSurfaceTypes.glass},
            {TechType.PictureFrame, VFXSurfaceTypes.glass},
            {TechType.Floater, VFXSurfaceTypes.organic},

        };
        static Dictionary<string, VFXSurfaceTypes> surfaceTypes = new Dictionary<string, VFXSurfaceTypes> { {"CrashedShip_interior_T_room(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_T_hallway(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_cargo_room(Clone)", VFXSurfaceTypes.metal},
             {"generic_forklift(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_elevator_room(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_seamoth_room(Clone)", VFXSurfaceTypes.metal},
             {"submarine_locker_04_open(Clone)", VFXSurfaceTypes.metal},
             {"submarine_locker_04(Clone)", VFXSurfaceTypes.metal},
             {"submarine_locker_05(Clone)", VFXSurfaceTypes.metal},
             {"discovery_lab_cart_01(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_power_corridors(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_power_room(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_locker_room(Clone)", VFXSurfaceTypes.metal},
             {"base_hull_crack_01(Clone)", VFXSurfaceTypes.metal},
             {"base_hull_crack_02(Clone)", VFXSurfaceTypes.metal},
             {"base_hull_crack_03(Clone)", VFXSurfaceTypes.metal},
             {"Wrecks_Starship_doors_sealed(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_locker_room_coridor_02(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_exo_room(Clone)", VFXSurfaceTypes.metal},
             {"Wrecks_Starship_doors_locked_nokey_Aurora(Clone)", VFXSurfaceTypes.metal},
             {"ExplorableWreckRoom01_aurora(Clone)", VFXSurfaceTypes.metal},
             {"ExplorableWreckRoom03_aurora(Clone)", VFXSurfaceTypes.metal},
             {"ExplorableWreckRoom04_aurora(Clone)", VFXSurfaceTypes.metal},
             {"ExplorableWreckRoom05_aurora(Clone)", VFXSurfaceTypes.metal},
             {"ExplorableWreckRoom06_aurora(Clone)", VFXSurfaceTypes.metal},

             {"Wrecks_VentCover_Aurora(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_entrance_01_01(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_entrance_01_02(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_entrance_01_03(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_entrance_02_01(Clone)", VFXSurfaceTypes.metal},
             {"biodome_Robot_Arm(Clone)", VFXSurfaceTypes.metal},
             {"biodome_Robot_Arm_wall_tile(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_pipes_room(Clone)", VFXSurfaceTypes.metal},
             {"CrashedShip_entrance_03(Clone)", VFXSurfaceTypes.metal},
             {"Wrecks_Starship_doors_manual_Aurora(Clone)", VFXSurfaceTypes.metal},
             {"biodome_lab_shelf_01(Clone)", VFXSurfaceTypes.metal},
             {"vent_constructor_cover_01(Clone)", VFXSurfaceTypes.metal},
             {"docking_bar_bottle_02(Clone)", VFXSurfaceTypes.glass},
             {"docking_bar_bottle_03(Clone)", VFXSurfaceTypes.glass},
             {"docking_bar_bottle_05(Clone)", VFXSurfaceTypes.glass},


        };

        static Dictionary<string, LargeWorldEntity.CellLevel> cellLevels_ = new Dictionary<string, LargeWorldEntity.CellLevel> {
            { "Crab_snake_Lower_plate_01_01(Clone)", LargeWorldEntity.CellLevel.Far },
            { "Crab_snake_Lower_plate_01_02(Clone)", LargeWorldEntity.CellLevel.Far },
            { "Crab_snake_Lower_plate_01_03(Clone)", LargeWorldEntity.CellLevel.Far },
            { "coral_reef_Stalactites_cluster_01(Clone)", LargeWorldEntity.CellLevel.Far },
            { "coral_reef_Stalactite_02_01(Clone)", LargeWorldEntity.CellLevel.Far },
        };

        static Dictionary<TechType, LargeWorldEntity.CellLevel> cellLevels = new Dictionary<TechType, LargeWorldEntity.CellLevel>
        {
            { TechType.PurpleBranches, LargeWorldEntity.CellLevel.Far },
            { TechType.SnakeMushroom, LargeWorldEntity.CellLevel.Far },
            { TechType.PurpleStalk, LargeWorldEntity.CellLevel.Far },
            { TechType.LargeFloater, LargeWorldEntity.CellLevel.Far },
            { TechType.FloatingStone, LargeWorldEntity.CellLevel.Far },
        };

        static Dictionary<string, string> glassRenderers = new Dictionary<string, string> {{ "biodome_lab_tube_01(Clone)", "biodome_lab_tube_01/biodome_lab_tube_01_glass" },
            {"biodome_lab_containers_open_01(Clone)", "biodome_lab_containers_open_01/biodome_lab_containers_open_01_glass" },
            {"biodome_lab_containers_open_02(Clone)", "biodome_lab_containers_open_02/biodome_lab_containers_open_02_glass" },
            {"biodome_lab_containers_open_03(Clone)", "biodome_lab_containers_open_03/biodome_lab_containers_open_03_glass" },
            {"biodome_lab_containers_tube_02(Clone)", "biodome_lab_containers_tube_02/biodome_lab_containers_tube_02_glass" },
            {"Precursor_LostRiverBase_Lab1Glass(Clone)", null },
        };
        static Dictionary<TechType, string> glassRenderers_ = new Dictionary<TechType, string> {
            {TechType.CutefishEgg,"Creatures_eggs_15_anim/Creatures_eggs_15_shell"  },
            {TechType.LabEquipment2, "discovery_lab_props_02/discovery_lab_props_02_glass" },
            {TechType.LabEquipment3, "discovery_lab_props_03/discovery_lab_props_03_glass" },
            {TechType.LabContainer2, "biodome_lab_containers_close_02/biodome_lab_containers_close_02_glass" },
            {TechType.Seamoth, "Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_geo" },
            {TechType.Exosuit, "exosuit_01/root/Exosuit_cabin_01_glass" },
            {TechType.BaseNuclearReactorFragment, "Nuclear_reactor_damaged_02/Nuclear_reactor_damaged_02_glass" },
        };
        static HashSet<TechType> badLODs = new HashSet<TechType> { TechType.FernPalm, TechType.BulboTree, TechType.PurpleVasePlant, TechType.OrangeMushroom, TechType.PinkFlower, TechType.LargeFloater, TechType.BluePalm, TechType.RedRollPlant, TechType.CoralShellPlate, TechType.PurpleTentacle, TechType.Boomerang, TechType.SnakeMushroom, TechType.Eyeye, TechType.RedBush };
        static HashSet<string> badLODs_ = new HashSet<string> { "coral_reef_Stalactites_cluster_01(Clone)", "coral_reef_Stalactites_cluster_02_01(Clone)", "coral_reef_Stalactites_cluster_02_03(Clone)", "coral_reef_Stalactites_cluster_02_02(Clone)" };

        public static void DisableLODs(GameObject go, TechType techType = TechType.None)
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

        public static void DisableWavingShader(GameObject go)
        {
            foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.materials)
                {
                    //AddDebug(m.shader.name + " DisableKeyword UWE_WAVING");
                    m.DisableKeyword("UWE_WAVING");
                }
            }
        }

        private static IEnumerator SetNewPosRot(GameObject go, PosRotData data)
        {
            if (go.name != data.name)
                yield break;

            yield return new WaitForFrames(1);

            Vector3 currentPos = go.transform.position;
            float x = float.IsNaN(data.newPos.x) ? currentPos.x : data.newPos.x;
            float y = float.IsNaN(data.newPos.y) ? currentPos.y : data.newPos.y;
            float z = float.IsNaN(data.newPos.z) ? currentPos.z : data.newPos.z;
            Vector3 newPos = new Vector3(x, y, z);

            Vector3 currentRot = go.transform.eulerAngles;
            float xRot = float.IsNaN(data.newRot.x) ? currentRot.x : data.newRot.x;
            float yRot = float.IsNaN(data.newRot.y) ? currentRot.y : data.newRot.y;
            float zRot = float.IsNaN(data.newRot.z) ? currentRot.z : data.newRot.z;
            Vector3 newRot = new Vector3(xRot, yRot, zRot);

            AddDebug($"SetNewPosRot {go.name} currentPos {currentPos} currentRot {currentRot} newRot {newRot} newPos {newPos}");
            Main.logger.LogDebug($"SetNewPosRot {go.name} currentPos {currentPos} currentRot {currentRot} newRot {newRot} newPos {newPos}");

            if (data.newPos != default && data.newRot != default && currentPos != newPos && currentRot != data.newRot)
            {
                //Main.logger.LogDebug($"SetNewPosRot {go.name} SetPositionAndRotation");
                go.transform.SetPositionAndRotation(newPos, Quaternion.Euler(newRot));
            }
            else if (data.newPos != default && currentPos != newPos)
            {
                //Main.logger.LogDebug($"SetNewPosRot {go.name} newPos {newPos}");
                go.transform.position = newPos;
            }
            else if (newRot != default && currentRot != newRot)
            {
                //Main.logger.LogDebug($"SetNewPosRot {go.name} newRot {data.newRot}");
                go.transform.eulerAngles = newRot;
            }
            if (go.TryGetComponent(out CrashHome crashHome) && crashHome.crash)
            {
                crashHome.crash.transform.forward = go.transform.forward;
                crashHome.crash.transform.Rotate(-90, 0, 0);
                Vector3 posDif = currentPos - newPos;
                crashHome.crash.transform.position -= posDif;
                //AddDebug("move crash " + posDif);
            }
        }


        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            { // runs for every LWE only if loading saved game first time 
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogDebug("LargeWorldEntity Start " + __instance.name + " " + techType);
                Vector3 posV3 = __instance.transform.position;
                Vector3Int posV3int = new Vector3Int((int)posV3.x, (int)posV3.y, (int)posV3.z);

                if (newPosRots.ContainsKey(posV3int))
                {
                    foreach (var newPosRot in newPosRots[posV3int])
                        UWE.CoroutineHost.StartCoroutine(SetNewPosRot(__instance.gameObject, newPosRot));
                }
                if (techType == TechType.OrangeMushroom)
                {
                    FixOrangeMushroomCollider(__instance);
                }
                if (glassRenderers_.ContainsKey(techType))
                {
                    __instance.transform.DisableShadowCasting(glassRenderers_[techType]);
                }
                if (eatableFoodValue.ContainsKey(techType))
                {
                    Util.MakeEatable(__instance.gameObject, eatableFoodValue[techType]);
                }
                if (eatableWaterValue.ContainsKey(techType))
                {
                    Util.MakeDrinkable(__instance.gameObject, eatableWaterValue[techType]);
                }
                if (surfaceTypes_.ContainsKey(techType))
                {
                    __instance.gameObject.AddVFXsurfaceComponent(surfaceTypes_[techType]);
                }
                if (ConfigToEdit.propCannonGrabsAnyPlant.Value == false)
                {
                    if (techType != TechType.Creepvine && techType != TechType.Cyclops && techType != TechType.BigCoralTubes && techType != TechType.None && techType != TechType.BloodVine && techType != TechType.Seamoth)
                    {
                        if (Util.IsDecoPlant(__instance.gameObject))
                            Util.MakeUnmovable(__instance.gameObject);
                    }
                }
                if (ConfigToEdit.fruitGrowTime.Value > 0 && fruitTechTypes.Contains(techType))
                {
                    Util.EnsureFruits(__instance.gameObject);
                }
                if (fragments.Contains(techType))
                {
                    if (__instance.TryGetComponent(out ResourceTracker rt))
                    { // fix: some fragments could not be found by map room scanning
                        rt.overrideTechType = TechType.Fragment;
                        rt.techType = TechType.Fragment;
                    }
                    if (__instance.TryGetComponent(out Pickupable p))
                        UnityEngine.Object.Destroy(p);
                }
                if (techTypesToAddWorldForces.Contains(techType))
                {
                    AddWorldForces(__instance.gameObject);
                }
                if (techTypesToMakeUnmovable.Contains(techType))
                {
                    //AddDebug("techTypesToMakeUnmovable MakeUnmovable " + __instance.name);
                    Util.MakeUnmovable(__instance.gameObject);
                }
                if (ConfigToEdit.disableWeirdPlantAnimation.Value && techTypesToRemoveWavingShader.Contains(techType))
                {
                    DisableWavingShader(__instance.gameObject);
                }
                if (plantSurfaces.Contains(techType))
                {
                    __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.vegetation);
                }
                else if (coralSurfaces.Contains(techType))
                {
                    __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.coral);
                }

                if (techType == TechType.NarrowBed || techType == TechType.Bed1)
                { // beds in Aurora
                    if (__instance.TryGetComponent<Bed>(out _))
                        return;

                    CoroutineHost.StartCoroutine(MakeSleepable(__instance.gameObject, techType));
                }
                else if (techType == TechType.BloodVine)
                {
                    LiveMixin lm = __instance.GetComponent<LiveMixin>();
                    if (lm)
                        UnityEngine.Object.Destroy(lm);
                }
                else if (techType == TechType.PurpleFan) // veined nettle
                { // disable collision, allow scanning
                    DisableCollision(__instance);
                }
                else if (techType == TechType.PurpleTentacle) // writhing weed
                { // disable collision, allow scanning
                    DisableCollision(__instance, new Vector3(1, 1, 3));
                }
                else if (techType == TechType.FarmingTray && __instance.name == "Base_exterior_Planter_Tray_01_abandoned(Clone)")
                {
                    Util.MakeUnmovable(__instance.gameObject);
                }
                else if (techType != TechType.None && Util.IsGraphicsPresetHighDetail())
                {
                    if (techType == TechType.FloatingStone)
                    { // Anchor Pod
                        DisableFloatingStoneLODs(__instance);
                        //ForceLODs(__instance.gameObject);
                    }
                    else if (badLODs.Contains(techType))
                    {
                        //DisableLODs(__instance.gameObject);
                        __instance.gameObject.ForceLODs();
                    }
                    if (cellLevels.ContainsKey(techType))
                    {
                        __instance.cellLevel = cellLevels[techType];
                    }
                    if (__instance.cellLevel == LargeWorldEntity.CellLevel.Near)
                    {
                        __instance.cellLevel = LargeWorldEntity.CellLevel.Medium;
                    }
                }

                else if (techType == TechType.None)
                {
                    //RemoveLight(__instance);
                    if (Util.IsGraphicsPresetHighDetail())
                    {
                        if (__instance.cellLevel == LargeWorldEntity.CellLevel.Near)
                        {
                            __instance.cellLevel = LargeWorldEntity.CellLevel.Medium;
                        }
                        if (badLODs_.Contains(__instance.name))
                        {
                            DisableLODs(__instance.gameObject);
                        }
                        if (cellLevels_.ContainsKey(__instance.name))
                        {
                            __instance.cellLevel = cellLevels_[__instance.name];
                        }
                    }

                    if (glassRenderers.ContainsKey(__instance.name))
                    {
                        string path = glassRenderers[__instance.name];
                        if (path != null)
                            __instance.transform.DisableShadowCasting(path);
                        else
                            __instance.transform.DisableShadowCastingInChildren();
                    }
                    if (ConfigToEdit.disableHotMetalGlow.Value && Main.gameLoaded == false)
                    {
                        if (__instance.name == "Wrecks_Starship_doors_sealed(Clone)")
                        { // door you cut open with laser cutter
                            __instance.gameObject.DisableGlowShader();
                            return;
                        }
                    }

                    if (surfaceTypes.ContainsKey(__instance.name))
                    {
                        __instance.gameObject.AddVFXsurfaceComponent(surfaceTypes[__instance.name]);
                    }
                    if (objectsToMakeUnmovable.Contains(__instance.name))
                    {
                        Util.MakeUnmovable(__instance.gameObject);
                    }
                    if (IsRockInCaveEntrance(__instance)) // -6 -13
                    { // fix bug: rocks that block cave entrance fall down bc they load before terrain 
                        __instance.cellLevel = LargeWorldEntity.CellLevel.Near;
                    }
                    if (ConfigToEdit.disableHotMetalGlow.Value && hotMetalDebris.ContainsKey(__instance.name) && __instance.transform.position.y < Ocean.GetOceanLevel() + .5)
                    {
                        //AddDebug($"hotMetalDebris {__instance.name} {__instance.transform.position.y}");
                        var list = hotMetalDebris[__instance.name];
                        if (list == null)
                            __instance.gameObject.DisableGlowShader();
                        else
                        {
                            foreach (var s in list)
                            {
                                Transform t = __instance.transform.Find(s);
                                if (t)
                                    t.gameObject.DisableGlowShader();
                            }
                        }
                    }
                    if (__instance.name == "Land_tree_01(Clone)")
                    {
                        __instance.gameObject.DisableGlowShader();
                        if (Util.IsGraphicsPresetHighDetail())
                            __instance.gameObject.ForceLODs();
                    }
                    //else if (__instance.name == "ExplorableWreck_KooshZone_3(Clone)")
                    //{ // 910, -199, 612
                    //    Transform interactable = __instance.transform.GetChild(1);
                    //    Transform t = interactable.Find("CyclopsSonarModuleDataboxSpawner(Placeholder)");
                    //    t.position = new Vector3(t.position.x, -202.4f, t.position.z);
                    //}
                    else if (__instance.name.StartsWith("ExplorableWreck"))
                    {
                        __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.metal);
                    }
                    else if (plantsWithNoTechtype.Contains(__instance.name))
                    {
                        DisableCollision(__instance);
                        __instance.gameObject.AddVFXsurfaceComponent(VFXSurfaceTypes.vegetation);
                    }
                    return;
                }
            }

            private static void FixOrangeMushroomCollider(LargeWorldEntity __instance)
            { // land_plant_middle_05_01    center y 0.54 rad 0.7864308  h 2
                CapsuleCollider cc = __instance.GetComponentInChildren<CapsuleCollider>();
                cc.height = 0;
                cc.center = new Vector3(0, .3f, 0);
            }

            private static void DisableFloatingStoneLODs(LargeWorldEntity __instance)
            {
                Transform model = __instance.transform.Find("model");
                if (model.childCount == 1)
                    model = model.GetChild(0);

                foreach (Transform child in model)
                {
                    LODGroup lODGroup = child.GetComponent<LODGroup>();
                    lODGroup.enabled = false;
                    Transform lod = child.GetChild(1);
                    lod.gameObject.SetActive(false);
                }
            }

            private static bool IsRockInCaveEntrance(LargeWorldEntity entity)
            {
                return entity.name.StartsWith("FloatingStone") && !entity.name.EndsWith("Floaters(Clone)") && entity.transform.position.y < Ocean.GetOceanLevel() + 1f;
            }

            private static void RemoveLight(LargeWorldEntity entity)
            {
                //if (entity.GetComponent<StoreInformationIdentifier>() && Main.config.biomesRemoveLight.Contains(Player.main.GetBiomeString()))
                //{
                //    Light light = entity.GetComponent<Light>();
                //    if (light && light.enabled && entity.transform.childCount == 0)
                //        light.enabled = false;
                //}
            }

            public static IEnumerator MakeSleepable(GameObject go, TechType techType)
            {
                //AddDebug("MakeSleepable " + go.name);
                TaskResult<GameObject> result = new TaskResult<GameObject>();
                yield return CraftData.InstantiateFromPrefabAsync(techType, result);
                GameObject newBed = result.Get();
                Rigidbody rb = go.GetComponent<Rigidbody>();
                UnityEngine.Object.Destroy(rb);
                newBed.transform.position = go.transform.position;
                newBed.transform.rotation = go.transform.rotation;
                newBed.transform.localScale = go.transform.localScale;
                Constructable c = newBed.GetComponent<Constructable>();
                UnityEngine.Object.Destroy(c);
                ConstructableBounds[] cbs = newBed.GetComponents<ConstructableBounds>();
                foreach (var cb in cbs)
                    UnityEngine.Object.Destroy(cb);

                foreach (Transform child in go.transform)
                    child.gameObject.SetActive(false);
            }

            private static void TestHarvest(TechType techType)
            {
                HarvestType harvestType = TechData.GetHarvestType(techType);
                if (harvestType != HarvestType.None)
                {
                    TechType harvestOutput = TechData.GetHarvestOutput(techType);
                    //harvest.Add($"{techType} harvest {harvestType} {harvestOutput}");
                }
                //AddDebug("AddHarvestResourceToExosuit harvestType " + harvestType);
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


            //[HarmonyPostfix, HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(LargeWorldEntity __instance)
            {
            }

            [HarmonyPrefix, HarmonyPatch("StartFading")]
            public static bool StartFadingPrefix(LargeWorldEntity __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                //AddDebug("StartFading " + __instance.name);
                if (spawning)
                {
                    //AddDebug("StartFading spawning " + __instance.name);
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
