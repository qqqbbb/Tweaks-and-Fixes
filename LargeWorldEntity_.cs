using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        static HashSet<TechType> plantSurfaces = new HashSet<TechType> {  TechType.RedRollPlant, TechType.GabeSFeather, TechType.RedGreenTentacle, TechType.JellyPlant, TechType.OrangeMushroom, TechType.SnakeMushroom, TechType.OrangePetalsPlant, TechType.SpikePlant, TechType.MembrainTree, TechType.Melon, TechType.SmallMelon, TechType.MelonPlant, TechType
        .HangingFruitTree, TechType.PurpleVasePlant, TechType.PinkMushroom, TechType.TreeMushroom, TechType.BallClusters, TechType.SmallFanCluster, TechType.SmallFan, TechType.RedConePlant, TechType.RedBush, TechType.SeaCrown, TechType.PurpleRattle, TechType.RedBasketPlant, TechType.ShellGrass, TechType.SpikePlant, TechType.CrashHome, TechType.CrashPowder, TechType.SpottedLeavesPlant, TechType.PurpleFan, TechType.PinkFlower, TechType.PurpleTentacle, TechType.PurpleStalk, TechType.FloatingStone, TechType.BlueLostRiverLilly, TechType.BlueTipLostRiverPlant, TechType.HangingStinger, TechType.CoveTree, TechType.BarnacleSuckers, TechType.BlueCluster};

        //static HashSet<TechType> coralSurfaces = new HashSet<TechType> { TechType.BigCoralTubes, TechType.CoralShellPlate, TechType.GenericJeweledDisk, TechType.JeweledDiskPiece, TechType.CoralChunk };
        static HashSet<string> plantsWithNoTechtype = new HashSet<string> { "Coral_reef_small_deco_03(Clone)", "Coral_reef_small_deco_05(Clone)", "Coral_reef_small_deco_08(Clone)" };

        static HashSet<TechType> techTypesToRemoveWavingShader = new HashSet<TechType> { TechType.BulboTree, TechType.PurpleVasePlant, TechType.OrangePetalsPlant, TechType.PinkMushroom, TechType.PurpleRattle, TechType.PinkFlower };

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
            {"explorable_wreckage_modular_room_details_09(Clone)",null },
            {"explorable_wreckage_modular_room_details_10(Clone)",null },
            {"explorable_wreckage_modular_room_details_11(Clone)",null },
            {"explorable_wreckage_modular_room_details_14(Clone)",null },
            {"explorable_wreckage_modular_room_details_19(Clone)",null },
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

            {"ExplorableWreck_KooshZone_1(Clone)", new List<string>{ "ExteriorProps" } },
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


        public static bool spawningNearPlayer;

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
            {new Vector3Int(448, -77, -8), new List<PosRotData>{
                new PosRotData("Coral_reef_shell_tunnel_02(Clone)", new Vector3(float.NaN, float.NaN, -6.5f), default) }},
            {new Vector3Int(1256, -220, 634), new List<PosRotData>{
                new PosRotData("Coral_reef_koosh_bush_small(Clone)", new Vector3(float.NaN, -220.5f,  float.NaN), default) }},
            {new Vector3Int(1257, -220, 635), new List<PosRotData>{
                new PosRotData("Coral_reef_koosh_bush_medium(Clone)", new Vector3(float.NaN, -220.1f,  float.NaN), default) }},
            {new Vector3Int(1249, -219, 622), new List<PosRotData>{
                new PosRotData("Coral_reef_koosh_bush_small(Clone)", new Vector3(float.NaN, -220.1f,  float.NaN), default) }},
            {new Vector3Int(1237, -239, 463), new List<PosRotData>{
                new PosRotData("SupplyCrate_FirstAidKit(Clone)", new Vector3(float.NaN, -240f,  float.NaN), default) }},

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

            //AddDebug($"SetNewPosRot {go.name} currentPos {currentPos} currentRot {currentRot} newRot {newRot} newPos {newPos}");
            //Main.logger.LogDebug($"SetNewPosRot {go.name} currentPos {currentPos} currentRot {currentRot} newRot {newRot} newPos {newPos}");

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

        public static HashSet<string> fruitPlants = new HashSet<string>();
        public static HashSet<string> creepvines = new HashSet<string>();

        [HarmonyPatch(typeof(LargeWorldEntity))]
        class LargeWorldEntity_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void StartPostfix(LargeWorldEntity __instance)
            { // runs for every LWE only if loading saved game first time 
                //TechType techType = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogDebug("LargeWorldEntity Start " + __instance.name + " " + techType);
                Vector3 posV3 = __instance.transform.position;
                Vector3Int posV3int = new Vector3Int((int)posV3.x, (int)posV3.y, (int)posV3.z);

                //if (techType == TechType.Creepvine)
                {
                    //PrefabIdentifier identifier = __instance.GetComponent<PrefabIdentifier>();
                    //if (identifier == null)
                    //{
                    //    AddDebug($"{__instance.name} has no PrefabIdentifier");
                    //    return;
                    //}
                    //creepvines.Add(identifier.classId);
                }
                if (newPosRots.ContainsKey(posV3int))
                {
                    foreach (var newPosRot in newPosRots[posV3int])
                        UWE.CoroutineHost.StartCoroutine(SetNewPosRot(__instance.gameObject, newPosRot));
                }
                if (ConfigToEdit.propCannonGrabsAnyPlant.Value == false)
                {
                    //if (techType != TechType.Creepvine && techType != TechType.Cyclops && techType != TechType.BigCoralTubes && techType != TechType.None && techType != TechType.BloodVine && techType != TechType.Seamoth)
                    {
                        //bool decoPlant = Util.IsDecoPlant(__instance.gameObject);
                        //AddDebug($"{__instance.name} decoPlant {decoPlant}");
                        //Main.logger.LogDebug($"{__instance.name} decoPlant {decoPlant}");
                        //if (decoPlant)
                        //    Util.MakeUnmovable(__instance.gameObject);
                    }
                }
                //if (techType == TechType.NarrowBed || techType == TechType.Bed1)
                //{ // beds in Aurora
                //    if (__instance.TryGetComponent<Bed>(out _))
                //        return;

                //    CoroutineHost.StartCoroutine(MakeSleepable(__instance.gameObject, techType));
                //}
                //else if (techType == TechType.BloodVine)
                //{
                //    LiveMixin lm = __instance.GetComponent<LiveMixin>();
                //    if (lm)
                //        UnityEngine.Object.Destroy(lm);
                //}
                //else if (techType == TechType.None)
                {
                    //RemoveLight(__instance);
                    if (ConfigToEdit.disableHotMetalGlow.Value)
                    {
                        if (Main.gameLoaded == false)
                        {
                            if (__instance.name == "Wrecks_Starship_doors_sealed(Clone)")
                            { // door you cut open with laser cutter
                                __instance.gameObject.DisableGlowShader();
                                return;
                            }
                        }
                        if (hotMetalDebris.ContainsKey(__instance.name) && __instance.transform.position.y < Ocean.GetOceanLevel() + .5)
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
                    }
                    //else if (__instance.name == "ExplorableWreck_KooshZone_3(Clone)")
                    //{ // 910, -199, 612
                    //    Transform interactable = __instance.transform.GetChild(1);
                    //    Transform t = interactable.Find("CyclopsSonarModuleDataboxSpawner(Placeholder)");
                    //    t.position = new Vector3(t.position.x, -202.4f, t.position.z);
                    //}
                }
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

            [HarmonyPrefix, HarmonyPatch("StartFading")]
            public static bool StartFadingPrefix(LargeWorldEntity __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                //AddDebug("StartFading " + __instance.name);
                if (spawningNearPlayer)
                {
                    //AddDebug("StartFading spawning " + __instance.name);
                    spawningNearPlayer = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(BreakableResource), "SpawnResourceFromPrefab", new Type[] { typeof(AssetReferenceGameObject) })]
        class BreakableResource_SpawnResourceFromPrefab_Patch
        {
            public static void Prefix(BreakableResource __instance)
            {
                spawningNearPlayer = true;
            }
        }

        [HarmonyPatch(typeof(SpawnOnKill), "OnKill")]
        class SpawnOnKill_OnKill_Patch
        {
            public static void Prefix(SpawnOnKill __instance)
            {
                spawningNearPlayer = true;
            }
        }

        [HarmonyPatch(typeof(CollectShiny), "DropShinyTarget", new Type[] { typeof(GameObject) })]
        class CollectShiny_DropShinyTarget_Patch
        {
            public static void Prefix(CollectShiny __instance)
            {
                spawningNearPlayer = true;
            }
        }

    }
}
