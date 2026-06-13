using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UWE;
using static ErrorMessage;
using CellLevel = LargeWorldEntity.CellLevel;

namespace Tweaks_Fixes
{
    internal class PrefabFixer
    {
        public static bool prefabsFixed;
        public static readonly int zOffset = Shader.PropertyToID("_ZOffset");

        HashSet<TechType> techTypesToAddWorldForces = new HashSet<TechType> { TechType.CoralChunk, 
        //TechType.CreepvineSeedCluster, TechType.HangingFruit,TechType.KooshChunk
        };

        List<string> prefabsWithoutShadows = new List<string> {
            "6a01a336-fb46-469a-9f7d-1659e07d11d7", // Precursor_Lab_surgical_machine
            "78009225-a9fa-4d21-9580-8719a3368373", // precursor_deco_props_01
            "1673ee4a-6c28-4651-8d5e-929de26dc25f",// Precursor_Prison_EggLab
            "ef375125-885f-4289-8577-c7a4a5f218b3",// Precursor_Prison_DissectionRoom
        };

        readonly Dictionary<string, RendererData> prefabsWithoutShadows_ = new Dictionary<string, RendererData> {
            {"68254d33-2d67-48a8-b485-9929f23a8ba8", new RendererData("", new List<string>{"Pipes", "Eggs", "___New Group" }) }//Precursor_Prison_EggLab_Extras
        };

        readonly Dictionary<string, MaterialZoffsetData> materialZoffsets = new Dictionary<string, MaterialZoffsetData>
        {
            { "66cc5a83-142b-4d8d-8d16-2d6e960f59c3", new MaterialZoffsetData("life_pod_exploded_02/life_pod_pontoons _damaged", 0) },// life_pod_exploded_2
            { "3894aeaf-e1f9-426a-9249-6a4968ac2d8b", new MaterialZoffsetData("life_pod_exploded_02_01/exterior/Life_pod_no_pontoons", 1) },// life_pod_exploded_19
            { "56b5ed17-2bff-4f7e-aba0-275b6a2398f9", new MaterialZoffsetData("life_pod_exploded_02_03/exterior/life_pod_damaged", -1) },// life_pod_exploded_17
            { "00037e80-3037-48cf-b769-dc97c761e5f6", new MaterialZoffsetData("life_pod_exploded_02_02/exterior", -1) },// life_pod_exploded_13
            { "85ae70e0-176c-4de6-8c4d-48c4f504cc79", new MaterialZoffsetData("life_pod_exploded_02_02/exterior", -1) },// life_pod_exploded_6
            { "00891fdf-7264-4c55-b569-732cdcded701", new MaterialZoffsetData("life_pod_exploded_02_03/exterior/life_pod_damaged", -1) },// life_pod_exploded_12
            { "d88147fb-007c-481f-aa75-ebcbab24e4a8", new MaterialZoffsetData("Starship_exploded_debris_19", 1) },
            { "b88a2b71-db4c-47e5-807d-c57fdf90f5ce", new MaterialZoffsetData("coridor_02/corridor_02", 9) },// CrashedShip_locker_room_coridor_02
            { "52175781-b8d5-4956-8d06-650619324934", new MaterialZoffsetData("T_hallway/hallway", 11) },// CrashedShip_T_hallway
            { "49278e68-fe5f-4576-b0f4-d03c2cd834ff", new MaterialZoffsetData("entrance_01_02", 3) },// CrashedShip_entrance_01_02
            { "8f5046b4-b727-4359-9d5a-2640ae6bf5d6", new MaterialZoffsetData("entrance_02_01/entrance_02_01_MeshPart0", new int[]{16, 17}) },// CrashedShip_entrance_02_01
            {"dedee57d-6a84-4bbb-92e3-d9b2249acc15", new MaterialZoffsetData("locker_room/locker_room 2_MeshPart0", 4, 500) },// CrashedShip_locker_room
            {"6f9e2e29-9eba-4261-ba7b-ed5eac120b91", new MaterialZoffsetData("Map_Room_fragment_01", 3, 5000) },// Map_Room_fragment_01

        };

        readonly Dictionary<string, List<MaterialZoffsetData>> materialZoffsets_ = new Dictionary<string, List<MaterialZoffsetData>>
        {
            {"42cac266-525a-4a89-9c11-89fd923faf86",// CrashedShip_elevator_room
            new List<MaterialZoffsetData> {new MaterialZoffsetData("cargo_elevator/cargo_elevator", 2), new MaterialZoffsetData("elevator/big_gate_doors_003", 1)}},
            {"e60dba6a-80d2-4583-a241-058f9ee823ca",// CrashedShip_entrance_03
            new List<MaterialZoffsetData> {new MaterialZoffsetData("entrance_03/entrance_03_MeshPart0", 3), new MaterialZoffsetData("entrance_03/entrance_03_MeshPart1", new int[] {19,20,21,22})}},
        };

        readonly Dictionary<string, string> prefabsToEnableDitherAlpha = new Dictionary<string, string>
        {
            {"d21bca5e-6dd2-48d8-bbf0-2f1d5df7fa9c", "Starship_cargo_02" },// Starship_cargo_02
            {"cc14ee20-80c5-4573-ae1b-68bebc0feadf", "Starship_cargo_02" },// Starship_cargo_large_02
            //{"cc14ee20-80c5-4573-ae1b-68bebc0feadf", "Starship_cargo_02" },// Starship_cargo_large_02
        };

        readonly Dictionary<string, MaterialZoffsetData> materialsToEnableDitherAlpha = new Dictionary<string, MaterialZoffsetData>
        {
            {"75472574-a336-4c64-94af-f2abe1919316", new MaterialZoffsetData("entrance_01_01", 6) },// CrashedShip_entrance_01_01
            {"e60dba6a-80d2-4583-a241-058f9ee823ca",// CrashedShip_entrance_03
            new MaterialZoffsetData("entrance_03/entrance_03_MeshPart1", new int[] {1,3,4,5,6,8})},
        };

        static MaterialZoffsetData crate1ZoffsetDara = new MaterialZoffsetData("Starship_cargo_damaged_01", 1);
        static MaterialZoffsetData crateZoffsetDara = new MaterialZoffsetData("Starship_cargo", 1);

        readonly Dictionary<string, MaterialZoffsetData> materialsToEnableAlphaClip = new Dictionary<string, MaterialZoffsetData>
        {
            {"3038bbc0-7d62-44a5-8cbe-c3fa1cccc55e", new MaterialZoffsetData("explorable_wreckage_modular_room_01/room_01_interior", 2) },// ExplorableWreckRoom01_aurora
            {"ebc835bd-221a-4722-b1d0-becf08bd2f2c", new MaterialZoffsetData("Starship_cargo_damaged_opened_02", 1) },// Starship_cargo_damaged_opened_02
            {"7646d66b-01c0-4110-b6bf-305df024c2b1", new MaterialZoffsetData("Starship_cargo_damaged_02", 1) },// Starship_cargo_damaged_02
            {"65edb6a3-c1e6-4aaf-9747-108bd6a9dcc6", crate1ZoffsetDara },// Starship_cargo_damaged_01
            {"8ba3be30-d89f-474b-87ca-94d3bfff25a4", crate1ZoffsetDara},// Starship_cargo_damaged_large_01
            {"a2104a9e-fe84-4c51-8874-69350507ef98", new MaterialZoffsetData("Starship_cargo_damaged_opened_01", 1)},// Starship_cargo_damaged_opened_large_01
            {"423ab63d-38e0-4dd8-ab8d-fcd6c9ff0759", new MaterialZoffsetData("Starship_cargo_damaged_02", 1)},// Starship_cargo_damaged_large_02
            {"8b43e753-29a6-4365-bc53-822376d1cfa2", crateZoffsetDara},// Starship_cargo_large
            {"af413920-4fe6-4447-9f62-4f04e605d6be", new MaterialZoffsetData("Starship_cargo_opened", 1)},// Starship_cargo_opened_large
            {"354ebf4e-def3-48a6-839d-bf0f478ca915", crateZoffsetDara},// Starship_cargo
            {"75472574-a336-4c64-94af-f2abe1919316", new MaterialZoffsetData("entrance_01_01", 7)},// CrashedShip_entrance_01_01
            {"49278e68-fe5f-4576-b0f4-d03c2cd834ff", new MaterialZoffsetData("entrance_01_02", new int[]{2, 21})},// CrashedShip_entrance_01_02
            {"8c3d54c0-4330-4949-91ad-f046cfd67c7c", new MaterialZoffsetData("Starship_cargo_damaged_opened_01", 1)},// Starship_cargo_damaged_opened_01
            { "8f5046b4-b727-4359-9d5a-2640ae6bf5d6", new MaterialZoffsetData("entrance_02_01/entrance_02_01_MeshPart0", 18) },// CrashedShip_entrance_02_01
            { "3331fe35-7be9-4f59-87ae-9cc54452b136", new MaterialZoffsetData("entrance_02_02", 8) },// CrashedShip_entrance_02_02

        };

        static string barTableGlassPath = "descent_bar_table_01/descent_bar_table_01_glass";
        static string crate2GlassPath = "Starship_cargo_damaged_opened_02/dirt_02";
        static string crate1GlassPath = "Starship_cargo_damaged_opened_01/dirt_01";

        readonly Dictionary<TechType, string> glassRenderers = new Dictionary<TechType, string>
        {
            { TechType.GhostLeviathanJuvenile, "model/Ghost_Leviathan_anim/Ghost_Leviathan_geo" },
            { TechType.GhostLeviathan, "model/Ghost_Leviathan_anim/Ghost_Leviathan_geo" },
            { TechType.GhostRayBlue, "model/ghost_ray/ghost_ray_geo/GhostRay_outards_quad" },
            { TechType.Jellyray, "jelly_ray_01/Jelly_Ray_01" },
            {TechType.CutefishEgg,"Creatures_eggs_15_anim/Creatures_eggs_15_shell"  },
            {TechType.LabEquipment2, "discovery_lab_props_02/discovery_lab_props_02_glass" },
            {TechType.LabEquipment3, "discovery_lab_props_03/discovery_lab_props_03_glass" },
            {TechType.LabContainer2, "biodome_lab_containers_close_02/biodome_lab_containers_close_02_glass" },
            {TechType.Seamoth, "Model/Submersible_SeaMoth/Submersible_seaMoth_geo/Submersible_SeaMoth_glass_geo" },
            {TechType.Exosuit, "exosuit_01/root/Exosuit_cabin_01_glass" },
            //{TechType.NuclearReactorFragment, "Nuclear_reactor_damaged_02/Nuclear_reactor_damaged_02_glass" },
            {TechType.BaseFiltrationMachine, "model/Water_Filtration_Machine/water_filtration_machine_geo/water_filtration_machine_glass" }, // in wreck
            {TechType.BlueAmoeba, "lost_river_plant_04/lost_river_plant_04_membrane" },
            {TechType.BarTable, barTableGlassPath},
            {TechType.Aquarium, "model/Aquarium_animation2/Aquarium_geo/Aquarium_glass" },
        };

        Dictionary<string, string> glassRenderers_ = new Dictionary<string, string> {{ "a36047b0-1533-4718-8879-d6ba9229c978", "biodome_lab_tube_01/biodome_lab_tube_01_glass" }, // biodome_lab_tube_01
            {"1b0b7f6d-9793-469c-9872-dfe690834fee", "biodome_lab_containers_open_01/biodome_lab_containers_open_01_glass" },// biodome_lab_containers_open_01
            {"d6389e01-f2cd-4f9d-a495-0867753e44f0", "biodome_lab_containers_open_02/biodome_lab_containers_open_02_glass" },// biodome_lab_containers_open_02
            {"1faf2b57-ff4f-4ea5-a715-7cc5ff6aae60", "biodome_lab_containers_open_03/biodome_lab_containers_open_03_glass" },// biodome_lab_containers_open_03
            {"a227d6b6-d64c-4bf0-b919-2db02d67d037", "biodome_lab_containers_tube_02/biodome_lab_containers_tube_02_glass" },// biodome_lab_containers_tube_02
            {"125686ad-18a8-41dd-8d14-ad3118479bd1", null }, // Precursor_LostRiverBase_Lab1Glass -234 -797 254

            {"d3b9095f-fcac-46de-83f7-762e3275e837", "life_pod_exploded_02/submarine_hatch_06" }, // life_pod_exploded_7
            {"f2b9fe45-39d6-4307-b1e0-143eb1937d6e", "life_pod_exploded_01/submarine_hatch_06" }, // life_pod_exploded_4
            {"66cc5a83-142b-4d8d-8d16-2d6e960f59c3", "life_pod_exploded_02/submarine_hatch_06" }, // life_pod_exploded_2
            {"3894aeaf-e1f9-426a-9249-6a4968ac2d8b", "life_pod_exploded_02_01/exterior/submarine_hatch_06/submarine_hatch_06_glass" }, // life_pod_exploded_19
            {"56b5ed17-2bff-4f7e-aba0-275b6a2398f9", "life_pod_exploded_02_03/exterior/submarine_hatch_06" }, // life_pod_exploded_17
            {"00891fdf-7264-4c55-b569-732cdcded701", "life_pod_exploded_02_03/exterior/submarine_hatch_06"}, // life_pod_exploded_12
            {"2aa237f6-2103-4a78-aaa7-104216551f0a", "life_pod_exploded_02_01/exterior/submarine_hatch_06/submarine_hatch_06_glass"}, // life_pod_exploded_3
            {"fb2886c4-7e03-4a47-a122-dc7242e7de5b", crate2GlassPath}, // Starship_cargo_damaged_opened_large_02
            {"ebc835bd-221a-4722-b1d0-becf08bd2f2c", crate2GlassPath}, // Starship_cargo_damaged_opened_02
            {"8c3d54c0-4330-4949-91ad-f046cfd67c7c", crate1GlassPath}, // Starship_cargo_damaged_opened_01
            {"a2104a9e-fe84-4c51-8874-69350507ef98", crate1GlassPath}, // Starship_cargo_damaged_opened_large_01
            {"54a7d6b6-280a-43d5-8bdd-eada3dd5f6c3", "exosuit_damaged_01/Exosuit_01_cabin005/Exosuit_cabin_01_glass005"}, // exosuit_damaged_01
            {"740258f8-bf36-484b-bdb8-d9e5dc3f1e3e", "exosuit_damaged_02/Exosuit_01_cabin004/Exosuit_cabin_01_glass004"}, // exosuit_damaged_02
            {"c3d6cad0-1981-4dfd-9a11-62eb0490b130", "exosuit_damaged_03/Exosuit_01_cabin/Exosuit_cabin_01_glass"}, // exosuit_damaged_03
            {"d70c8458-4b19-4dbc-ba67-afa654af1999", "exosuit_damaged_06/Exosuit_01_cabin008/Exosuit_cabin_01_glass008"}, // exosuit_damaged_06 

            {"90148ef8-fda4-4a95-b2bc-d570543a1ecf", barTableGlassPath},// descent_bar_table_01
            {"33acd899-72fe-4a98-85f9-b6811974fbeb", "biodome_lab_shelf_01/biodome_lab_shelf_01_thing_glass"},// biodome_lab_shelf_01
            {"083e02b8-9ea2-40e5-b8d1-22d236f284b9", "starship_exploded_interior_T_room"},// CrashedShip_interior_T_room
            {"7193a410-ee7b-4bba-85a6-80aa00e2ca68", "entrance_01_03"},// CrashedShip_entrance_01_03
            {"e7f9c5e7-3906-4efd-b239-28783bce17a5", "biodome_lab_containers_close_01/biodome_lab_containers_close_01_glass"},// biodome_lab_containers_close_01

        };

        List<string> glassRendererList = new List<string> {
            "76470f15-8918-4194-8191-4a40f1f3e32c",// starfish_01
            "4605151e-dea4-4ba7-96bf-2f88b3b41bdb",// starfish_02
            "89fa1b49-c1dc-4a87-abba-49689f02a60c",// starfish_03
            "d571d3dc-6229-430e-a513-0dcafc2c41f3",// starfish_04
            "777c5fe6-98d5-4d58-9790-ff57fea62e7c",// starfish_01_bend
            "677db3b4-2e53-40bb-a422-1ea80d61cae7",// starfish_02_bend
            "ff4374cf-2d98-4a0e-b6c8-ded844657323",// starfish_03_bend
            "04542c2f-db0c-4aad-9cc9-b8b5f6a85438",// starfish_04_bend
            "2d422d6b-3c1f-484d-84ee-a07b5b8e32a4",// Coral_reef_sea_crown_Light
            "7935a15e-a9ab-4fc6-90ef-58a65b30a4bd",// coral_reef_Hanging_Stinger_short
            "46d0473e-d366-4644-8c9c-5fdb65cbacb8",// coral_reef_Hanging_Stinger_middle
            "8914acde-168e-438f-9b2b-6b9332d8c1a1",// coral_reef_Hanging_Stinger_long
        };

        Dictionary<TechType, RendererData> glassRenderers__ = new Dictionary<TechType, RendererData> {
        { TechType.CrabSquid, new RendererData("models/Crab_Squid/crab_squid_geo", new List<string> { "crab_squid_head_geo1", "crab_squid_head_geo2", "crab_squid_head_geo1_LOD1", "crab_squid_head_geo1_LOD2", "crab_squid_head_geo1_LOD3", "crab_squid_head_geo2_LOD1", "crab_squid_head_geo2_LOD2", "crab_squid_head_geo2_LOD3" })},
        { TechType.Locker, new RendererData("model/submarine_Storage_locker_big_01", new List<string> { "submarine_Storage_locker_big_01_hinges_R/submarine_Storage_locker_big_01_door_R",
            "submarine_Storage_locker_big_01_hinges_L/submarine_Storage_locker_big_01_door_L" })},
        { TechType.Aquarium, new RendererData("model", new List<string> { "Large_Aquarium_generic_room_glass_01", "Large_Aquarium_02_glass" })},
        { TechType.SpineEel, new RendererData("model/spine_eel_geo" )},
        };

        Dictionary<string, RendererData> glassRenderers___ = new Dictionary<string, RendererData> {
            { "56cdfa77-e7ce-4397-9f8e-fc050e1626d6", new RendererData("pane (1)", new List<string> { "pane", "pane (1)" })}, // Precursor_LostRiverBase_BalconyGlass -252 -803 292
            {"1f5cee66-a02f-4693-a1bd-928c938c7e77", new RendererData("model", new List<string> { "seamoth_fragment_02_glass", "seamoth_fragment_02_interior_glass" })},
            {"2086a6af-c8ba-47f6-8e2a-1a4ac88dcd8b", new RendererData("seamoth_room")}, // CrashedShip_seamoth_room
            {"ba90d6e8-4a8a-4c66-8f1a-2b02f3fc3acd", new RendererData("seamoth_fragment_01", new List<string>{"seamoth_fragment_01_glass", "seamoth_fragment_01_interior_glass" })}, // seamoth_fragment_01_aurora
            {"98ac710d-5390-49fd-a850-dbea7bc07aef", new RendererData("power_room", new List<string>{ "starship_exploded_interior_power_room_02/starship_exploded_interior_power_room_02_MeshPart2", "starship_exploded_interior_power_room_01/starship_exploded_interior_power_room_01_MeshPart1" })}, // CrashedShip_power_room
          
            {"909d56bc-6494-4792-8e11-e2815c59f070", new RendererData("power_corridors/corridors")}, // CrashedShip_power_corridors
            {"a4d261c3-8b08-41d4-9ab4-c647bdbf2bde", new RendererData("exo_room/exo_room")}, // CrashedShip_exo_room

        };

        public static Dictionary<TechType, EatableData> eatables = new Dictionary<TechType, EatableData> { };

        List<TechType> badLODs = new List<TechType> { TechType.LargeFloater, TechType.GarryFish, TechType.SpikePlant, TechType.CrabSquid, };

        List<string> badLODs_ = new List<string> {
        "ce650c66-355c-4b77-ad4e-a2bea7e36c95",// land_plant_middle_04_02
        "1d5877a7-bc56-46c8-a27c-f9d0ab99cc80",// land_plant_middle_04_01
        "35056c71-5da7-4e73-be60-3c22c5c9e75c",// land_plant_middle_05_01
        "061af756-643c-42ad-9645-a522f1338084",// Coral_reef_slanted_coral_plates_01_01
        "60fdf752-bc74-4f85-8a9c-72f86031a52f",// coral_reef_blood_mushrooms_01_01 WhiteMushroom
        "29ab9e04-a045-413b-886b-e03fa6b86aee",// coral_reef_blood_mushrooms_01_02 WhiteMushroom
        "e4ea0e38-7baa-49ce-b85c-89a22935574f",// coral_reef_blood_mushrooms_01_03 WhiteMushroom
        "a6dac068-6f8d-4e32-b5e7-2e34a9f97d11",// coral_reef_blood_mushrooms_01_04 WhiteMushroom
        "a0c5b949-22a4-4899-9c51-64ccce6956bc",// Coral_reef_cave_root_small_02_blood BloodRoot
        "a0cbac2e-f86d-4ab0-a090-8115f5196f7c",// Coral_reef_cave_root_small_03_blood BloodRoot

         //No prefab"3d841353-7f68-42f3-b2ce-29655d5f7b06",// cave_root_02_blood_pod_05

        "92b48933-f89e-4d9d-a432-323785d7cdd2",// coral_reef_Stalactites_cluster_02_01
        "9d7aac2f-7ae7-4b55-ae83-f2ff0627fa2b",// coral_reef_Stalactites_cluster_02_02
        "c081e286-bb51-4cc6-b48c-ea03d817ccf9",// coral_reef_Stalactites_cluster_03_01
        "409f6f77-4fcd-4c4c-b6e6-cf4ca1ca4e9d",// coral_reef_Stalactites_cluster_03_02
        "20b16320-ed22-4d60-8890-93eded9e8e16",// coral_reef_Stalactites_cluster_04_01

        "0515430d-18cb-4ac7-bf6d-22bc877a16aa",// coral_reef_Stalactites_cluster_01
        "a3d11348-e589-4867-ac60-1fa122145615",// Crab_snake_mushrooms_04
        "8d0b24b7-c71f-42ab-8df9-7bfe05616ab4",// Crab_snake_mushrooms_03
        "50ebde28-dcd9-46be-bafd-9e2b483a1d22",// coral_reef_plant_small_01_02 BluePalm
        "e8047056-e202-49b3-829f-7458615103ac",// Coral_reef_purple_tentacle_plant_01_01

        "c72724f3-125d-4e87-b82f-a91b5892c936",// Coral_reef_floating_stones_big_02
        "228e5af5-a579-4c99-9fb0-04b653f73cd3",// Coral_reef_floating_stones_small_01
        "1645f35d-af23-4b98-b1e4-44d430421721",// Coral_reef_floating_stones_small_02
        "1cafd118-47e6-48c4-bfd7-718df9984685",// Coral_reef_floating_stones_mid_01
        "7444baa0-1416-4cb6-aa9a-162ccd4b98c7",// Coral_reef_floating_stones_mid_02
        "93a9886d-f2d3-4b6c-8e5f-216f569f82b2",// Coral_reef_slanted_coral_plates_01_02
        "fcf04278-bfbb-409d-bada-a6f22564efde",// Coral_reef_koosh_bush_large
        "210fdf87-54e0-4c83-9bf3-31bbc06f38a6",// coral_reef_plant_small_01_03 BluePalm
        //"20ad299d-ca52-48ef-ac29-c5ec5479e070",// Precursor_Prison_Outpost4
        //"",// Coral_reef_floating_stones_big_02
        };

        List<TechType> terribleLODs = new List<TechType> { TechType.Eyeye, TechType.Boomerang, TechType.RedBush, TechType.BulboTree };

        List<string> terribleLODs_ = new List<string> {
            "1cc51be0-8ea9-4730-936f-23b562a9256f",// Land_tree_01
            "3dbab1b9-cc52-4da4-8633-89b33add18f4",// Coral_reef_purple_tentacle_plant_01_02 
            "7bfe0629-a008-43b8-bd16-d69ad056769f",// Coral_reef_Kelp_blood_small_plants_01
            "e291d076-bf95-4cdd-9dd9-6acd37566cf6",// Coral_reef_Kelp_blood_small_plants_02
            "2bfcbaf4-1ae6-4628-9816-28a6a26ff340",// Coral_reef_Kelp_blood_small_plants_03
            "2ab96dc4-5201-4a41-aa5c-908f0a9a0da8",// Coral_reef_Kelp_blood_small_plants_04
            "f90ba94f-326b-4cbd-bc95-4dc39addbf33",// Coral_reef_koosh_bush_small
            "a5076433-b586-4c4f-adff-b002028e8014",// Coral_reef_koosh_bush_medium
            "1d6d89dd-3e49-48b7-90e4-b521fbc3d36f",// land_plant_middle_03_02 FernPalm 
            "523879d5-3241-4a94-8588-cb3b38945119",// land_plant_middle_03_01 FernPalm
        //"",// 
        };

        List<TechType> techTypesToMakeUnmovable = new List<TechType> { TechType.BulboTree, TechType.PurpleBrainCoral, TechType.HangingFruitTree, TechType.SpikePlant };

        List<string> classIDToMakeUnmovable = new List<string> { "c9d84dfd-6802-41bd-a7b1-34d9b3a31531",// Base_exterior_Planter_Tray_01_abandoned
            "5cd34124-935f-4628-b694-a266bc2f5517",// Starship_exploded_debris_01
            "df36cdfb-abee-41f1-bdc6-fec6566d3557",// Starship_exploded_debris_06 
            "d88147fb-007c-481f-aa75-ebcbab24e4a8",// Starship_exploded_debris_19
            "0c65ee6e-a84a-4989-a846-19eb53c13071",// Starship_exploded_debris_20 
            "dee7831a-96d6-4452-be36-d259cf96cf1a",// Starship_exploded_debris_21 
            "72437ebc-7d61-49b8-bac4-cb7f3af3af8e",// Starship_exploded_debris_22 
            "5a6279e2-fab9-48c9-bcb3-fdeb02fd4ce2",// Starship_exploded_debris_38 
            "b24e3e84-4b33-4202-9c1e-1032ef3a8d72",// Starship_exploded_debris_01_Far 
            "190f8620-b5d2-4799-9b3c-84b7f93fe594",// Starship_exploded_debris_06_Far 
            "7238ff24-5e21-44ea-86ab-603d378ba4bd",// Wrecks_VentCover_Aurora 
            "7bfe0629-a008-43b8-bd16-d69ad056769f",// Coral_reef_Kelp_blood_small_plants_01
            "e291d076-bf95-4cdd-9dd9-6acd37566cf6",// Coral_reef_Kelp_blood_small_plants_02
            "2bfcbaf4-1ae6-4628-9816-28a6a26ff340",// Coral_reef_Kelp_blood_small_plants_03
            "2ab96dc4-5201-4a41-aa5c-908f0a9a0da8",// Coral_reef_Kelp_blood_small_plants_04
            "4bfe1877-1b83-4d5d-9470-3bb2d5f389cc",// Coral_reef_cave_root_small_01_blood
            "da7341c3-e6a3-4cd3-ad57-49a4dc732ac9",// Coral_reef_cave_root_01_blood_Light
            "b0cae640-b155-4bac-9ed5-29ba64a1ee9f",// Coral_reef_cave_root_02_blood_Light
            "5beba896-bccf-4993-8bcb-1cdabb68e706",// Coral_reef_cave_root_03_blood_Light
            "db79ee0b-65e9-4ea1-8b8b-948bbae128f7",// Coral_reef_cave_root_04_blood_Light

            "1c28891f-df08-4eee-a081-118955b0d303",// Coral_reef_Kelp_blood_01_Light
            "a4912ba2-5643-46ee-bd69-6be53dd55d45",// Coral_reef_Kelp_blood_02_Light
            "e0ae8532-a6d5-436f-bdc0-846061d91686",// Coral_reef_Kelp_blood_03_Light
            "66f2188b-b537-49ac-b6e7-08f446eca9e8",// Coral_reef_Kelp_blood_04_Light
        };

        Dictionary<string, RendererData> classIDtoRemoveGlow = new Dictionary<string, RendererData> {
         { "da7341c3-e6a3-4cd3-ad57-49a4dc732ac9", new RendererData("models")},// Coral_reef_cave_root_01_blood_Light
         { "b0cae640-b155-4bac-9ed5-29ba64a1ee9f", new RendererData("models")},// Coral_reef_cave_root_02_blood_Light
         { "5beba896-bccf-4993-8bcb-1cdabb68e706", new RendererData("models")},// Coral_reef_cave_root_03_blood_Light
         { "db79ee0b-65e9-4ea1-8b8b-948bbae128f7", new RendererData("models")},// Coral_reef_cave_root_04_blood_Light
         { "1cc51be0-8ea9-4730-936f-23b562a9256f", null},// Land_tree_01
        };

        Dictionary<TechType, VFXSurfaceTypes> surfaceTypes_ = new Dictionary<TechType, VFXSurfaceTypes>
        {
            {TechType.BulboTree, VFXSurfaceTypes.wood },
            {TechType.PurpleBranches, VFXSurfaceTypes.vegetation },
            {TechType.PurpleFan, VFXSurfaceTypes.vegetation },
            {TechType.HangingFruitTree, VFXSurfaceTypes.wood },
            {TechType.DrillableCopper, VFXSurfaceTypes.rock },
            {TechType.DrillableGold, VFXSurfaceTypes.rock },
            {TechType.DrillableLithium, VFXSurfaceTypes.metal },
            {TechType.DrillableTitanium, VFXSurfaceTypes.metal },
            {TechType.DrillableSilver, VFXSurfaceTypes.rock },
            {TechType.DrillableSalt, VFXSurfaceTypes.rock },
            {TechType.DrillableQuartz, VFXSurfaceTypes.glass },
            {TechType.DrillableMagnetite, VFXSurfaceTypes.metal },
        };

        Dictionary<string, VFXSurfaceTypes> surfaceTypes = new Dictionary<string, VFXSurfaceTypes>
        {
            {"8ba14c3e-2264-47b8-8484-042b813ec484", VFXSurfaceTypes.metal},// ExplorableWreck_KooshZone_3
            {"c0684ae2-d4fa-4161-bad9-c7b52ddee33c", VFXSurfaceTypes.metal},// ExplorableWreck_Grassy_2
            {"7e5d948c-9bf5-4b3d-8f71-9d7cbcf84991", VFXSurfaceTypes.metal},// ExplorableWreck_Grassy_1
            {"38f4a1d4-7cbc-4a21-a953-02b3f667975f", VFXSurfaceTypes.metal},// ExplorableWreck_Dunes_6
            {"3d4c323b-e664-42d2-9c02-0b55481760dd", VFXSurfaceTypes.metal},// ExplorableWreck_BloodKelp_8
            {"55a41eb8-8c37-4d09-a78b-5be1080fb224", VFXSurfaceTypes.metal},// ExplorableWreck_GrandReef_10
            {"ad1e0255-d577-43ac-afa6-4cf17e08a067", VFXSurfaceTypes.metal},// ExplorableWreck_Grassy_12
            {"90677349-315b-4221-a7af-7e2ffa72c226", VFXSurfaceTypes.metal},// ExplorableWreck_Grassy_14
            {"427c6033-a3b2-4c0f-bbca-a7c26a909849", VFXSurfaceTypes.metal},// ExplorableWreck_SafeShallows_15
            {"9c4d8ef4-1948-4ed9-aab3-e9eb52ba666b", VFXSurfaceTypes.metal},// ExplorableWreck_SafeShallows_16
            {"34d3f7a1-3516-43c6-99f9-bf4cccc3a30b", VFXSurfaceTypes.metal},// ExplorableWreck_KelpForest_17
            {"238f887e-5cfa-4d66-b652-4be27583a4cb", VFXSurfaceTypes.metal},// ExplorableWreck_KelpForest_18
            {"1ac450d0-bfa3-42f4-b367-debb2981298a", VFXSurfaceTypes.metal},// ExplorableWreck_Mountains_19
            {"176d929f-3d86-4627-93ad-b656d4111337", VFXSurfaceTypes.metal},// ExplorableWreck_SparseReef_20
            {"fd3f6266-079a-4aa8-8f26-ef21045724c9", VFXSurfaceTypes.metal},// ExplorableWreck_Mountains_5
            {"1618a787-67b7-4e35-9869-3ec558ed2835", VFXSurfaceTypes.metal},// ExplorableWreck_GrandReef_11
            {"b6e4b065-88f4-442c-92a3-1b92dbdc6ae3", VFXSurfaceTypes.metal},// ExplorableWreck_UnderwaterIslands_4
            {"91e5a9b7-04d0-429a-95a5-478a494f5557", VFXSurfaceTypes.metal},// ExplorableWreck_TreaderPath_7
            {"249edffd-6516-4347-93ab-edb295b5bab4", VFXSurfaceTypes.metal},// ExplorableWreck_MushroomForest_9
            {"237347f9-36ce-4dd4-8a34-f75ccc00fc09", VFXSurfaceTypes.metal},// ExplorableWreck_Grassy_13
            {"eca96e8f-0097-4627-b906-f454c329d9e5", VFXSurfaceTypes.metal},// base_hull_crack_02
            {"33acd899-72fe-4a98-85f9-b6811974fbeb", VFXSurfaceTypes.metal},// biodome_lab_shelf_01
            {"75472574-a336-4c64-94af-f2abe1919316", VFXSurfaceTypes.metal},// CrashedShip_entrance_01_01
            {"e60dba6a-80d2-4583-a241-058f9ee823ca", VFXSurfaceTypes.metal},// CrashedShip_entrance_03
            {"49278e68-fe5f-4576-b0f4-d03c2cd834ff", VFXSurfaceTypes.metal},// CrashedShip_entrance_01_02
            {"8f5046b4-b727-4359-9d5a-2640ae6bf5d6", VFXSurfaceTypes.metal},// CrashedShip_entrance_02_01
            {"af165b07-a2a3-4d85-8ad7-0c801334c115", VFXSurfaceTypes.metal},// discovery_lab_cart_01
            {"7193a410-ee7b-4bba-85a6-80aa00e2ca68", VFXSurfaceTypes.metal},// CrashedShip_entrance_01_03
            {"737e0cdd-5333-4e1a-9b5d-f808340e71ec", VFXSurfaceTypes.metal},// CrashedShip_pipes_room
            {"38bfd21a-924d-4c08-87e4-49d88f3b626e", VFXSurfaceTypes.metal},// ExplorableWreckRoom04_aurora
            {"2018e281-5012-4d04-8b81-cc3c7f4706ed", VFXSurfaceTypes.metal},// ExplorableWreckRoom06_aurora
            {"3038bbc0-7d62-44a5-8cbe-c3fa1cccc55e", VFXSurfaceTypes.metal},// ExplorableWreckRoom01_aurora
            {"9c609b7f-e87e-46a0-a807-60777234d20d", VFXSurfaceTypes.metal},// ExplorableWreckRoom03_aurora
            {"7d11df51-b70f-431a-a68f-495e0cae2459", VFXSurfaceTypes.metal},// ExplorableWreckRoom05_aurora
            {"3c12b4b9-018f-45a3-95bf-7b4770b744a1", VFXSurfaceTypes.metal},// biodome_Robot_Arm_wall_tile
            {"68e7dcd8-fe09-4dac-b966-85463c3c58af", VFXSurfaceTypes.metal},// biodome_Robot_Arm
            {"13d0fb01-2957-49e0-b153-6dc88332694c", VFXSurfaceTypes.metal},// generic_forklift
            {"8ca26b21-f70b-4de9-9892-f4a382e1a20a", VFXSurfaceTypes.metal},// base_hull_crack_01
            {"52175781-b8d5-4956-8d06-650619324934", VFXSurfaceTypes.metal},// CrashedShip_T_hallway
            {"083e02b8-9ea2-40e5-b8d1-22d236f284b9", VFXSurfaceTypes.metal},// CrashedShip_interior_T_room
            {"a4d261c3-8b08-41d4-9ab4-c647bdbf2bde", VFXSurfaceTypes.metal},// CrashedShip_exo_room
            {"b88a2b71-db4c-47e5-807d-c57fdf90f5ce", VFXSurfaceTypes.metal},// CrashedShip_locker_room_coridor_02
            {"235f771a-bb5a-4f58-8484-4ad9a6f4e95c", VFXSurfaceTypes.metal},// vent_constructor_cover_01
            {"7238ff24-5e21-44ea-86ab-603d378ba4bd", VFXSurfaceTypes.metal},// Wrecks_VentCover_Aurora
            {"d3ec61fc-a3ac-494c-bf24-6ab6968c5179", VFXSurfaceTypes.metal},// Wrecks_Starship_doors_locked_nokey_Aurora
            {"bca9b19c-616d-4948-8742-9bb6f4296dc3", VFXSurfaceTypes.metal},// submarine_locker_04_open
            {"078b41f8-968e-4ca3-8a7e-4e3d7d98422c", VFXSurfaceTypes.metal},// submarine_locker_05
            {"29680106-d337-46ea-a55b-5eb5fd8445f3", VFXSurfaceTypes.metal},// base_hull_crack_03
            {"32fb101b-b834-4982-a6eb-338ff2f98ea4", VFXSurfaceTypes.metal},// Wrecks_Starship_doors_manual_Aurora
            {"c80288ce-9522-45f5-b3c2-01fe459ae5fe", VFXSurfaceTypes.metal},// CrashedShip_cargo_room
            {"6f01d2df-03b8-411f-808f-b3f0f37b0d5c", VFXSurfaceTypes.metal},// Wrecks_Starship_doors_sealed
            {"dedee57d-6a84-4bbb-92e3-d9b2249acc15", VFXSurfaceTypes.metal},// CrashedShip_locker_room
            {"42cac266-525a-4a89-9c11-89fd923faf86", VFXSurfaceTypes.metal},// CrashedShip_elevator_room
            {"909d56bc-6494-4792-8e11-e2815c59f070", VFXSurfaceTypes.metal},// CrashedShip_power_corridors
            {"98ac710d-5390-49fd-a850-dbea7bc07aef", VFXSurfaceTypes.metal},// CrashedShip_power_room
            {"cd34fecd-794c-4a0c-8012-dd81b77f2840", VFXSurfaceTypes.metal},// submarine_locker_04
            {"2086a6af-c8ba-47f6-8e2a-1a4ac88dcd8b", VFXSurfaceTypes.metal},// CrashedShip_seamoth_room
            {"41399588-124d-4e01-92b7-f5b10c882ac8", VFXSurfaceTypes.glass},// docking_bar_bottle_05
            {"d53cfbf1-f14d-4e9b-b8bb-cc65e734a9c5", VFXSurfaceTypes.glass},// docking_bar_bottle_03
            {"ff9b4394-e7d3-42e6-b924-585af2d0e03f", VFXSurfaceTypes.glass},// docking_bar_bottle_02
            {"da7341c3-e6a3-4cd3-ad57-49a4dc732ac9", VFXSurfaceTypes.vegetation},// Coral_reef_cave_root_01_blood_Light
            {"b0cae640-b155-4bac-9ed5-29ba64a1ee9f", VFXSurfaceTypes.vegetation},// Coral_reef_cave_root_02_blood_Light
            {"5beba896-bccf-4993-8bcb-1cdabb68e706", VFXSurfaceTypes.vegetation}, // Coral_reef_cave_root_03_blood_Light
            {"db79ee0b-65e9-4ea1-8b8b-948bbae128f7", VFXSurfaceTypes.vegetation},// Coral_reef_cave_root_04_blood_Light
            {"1c28891f-df08-4eee-a081-118955b0d303", VFXSurfaceTypes.vegetation},// Coral_reef_Kelp_blood_01_Light
            {"a4912ba2-5643-46ee-bd69-6be53dd55d45", VFXSurfaceTypes.vegetation},// Coral_reef_Kelp_blood_02_Light
            {"e0ae8532-a6d5-436f-bdc0-846061d91686", VFXSurfaceTypes.vegetation},// Coral_reef_Kelp_blood_03_Light
            {"66f2188b-b537-49ac-b6e7-08f446eca9e8", VFXSurfaceTypes.vegetation},// Coral_reef_Kelp_blood_04_Light
            {"57a31bf5-5b86-4bf6-9a14-9291c6e8a79c", VFXSurfaceTypes.vegetation},// coral_reef_plant_small_01_01 BluePalm
            {"50ebde28-dcd9-46be-bafd-9e2b483a1d22", VFXSurfaceTypes.vegetation},// coral_reef_plant_small_01_02 BluePalm
            {"210fdf87-54e0-4c83-9bf3-31bbc06f38a6", VFXSurfaceTypes.vegetation},// coral_reef_plant_small_01_03 BluePalm
            {"56b5ed17-2bff-4f7e-aba0-275b6a2398f9", VFXSurfaceTypes.metal},// life_pod_exploded_17
            {"d3b9095f-fcac-46de-83f7-762e3275e837", VFXSurfaceTypes.metal},// life_pod_exploded_7
            {"00037e80-3037-48cf-b769-dc97c761e5f6", VFXSurfaceTypes.metal},// life_pod_exploded_13
            {"85ae70e0-176c-4de6-8c4d-48c4f504cc79", VFXSurfaceTypes.metal},// life_pod_exploded_6
            {"00891fdf-7264-4c55-b569-732cdcded701", VFXSurfaceTypes.metal},// life_pod_exploded_12
            {"f2b9fe45-39d6-4307-b1e0-143eb1937d6e", VFXSurfaceTypes.metal},// life_pod_exploded_4
            {"2aa237f6-2103-4a78-aaa7-104216551f0a", VFXSurfaceTypes.metal},// life_pod_exploded_3
            {"66cc5a83-142b-4d8d-8d16-2d6e960f59c3", VFXSurfaceTypes.metal},// life_pod_exploded_2
            {"3894aeaf-e1f9-426a-9249-6a4968ac2d8b", VFXSurfaceTypes.metal},// life_pod_exploded_19
            {"ef1370e3-832f-4008-ac39-99ad24f43f76", VFXSurfaceTypes.metal},// Starship_doors_door
            {"533d54b0-e54a-4aec-8dd0-a9eb89868c59", VFXSurfaceTypes.vegetation},// farming_plant_02 PurpleVegetablePlant
            {"a966a14f-d188-4de4-a488-f2c0302ca250", VFXSurfaceTypes.vegetation},// farming_plant_01_01 MelonPlant
            {"e9445fdf-fbae-49dc-a005-48c05bf9f401", VFXSurfaceTypes.vegetation},// farming_plant_01_02 MelonPlant
            {"9bfe02bd-60a3-401b-b7a0-627c3bdc4451", VFXSurfaceTypes.vegetation},// creepvine
            {"1fd4d86f-3b06-4369-945c-ca65f50b4800", VFXSurfaceTypes.vegetation},// creepvine
            {"de0e28a2-7a17-4254-b520-5f0e28355059", VFXSurfaceTypes.vegetation},// creepvine
            {"ee1baf03-0560-4f4d-ad29-13a337bef0d7", VFXSurfaceTypes.vegetation},// creepvine
            {"de972f1f-daab-41d6-b274-5173b0dd23d8", VFXSurfaceTypes.vegetation},// creepvine
            {"7329db6b-7385-4e77-8afa-71830ead9350", VFXSurfaceTypes.vegetation},// creepvine
            {"a17ef178-6952-4a91-8f66-44e1d8ca0575", VFXSurfaceTypes.vegetation},// creepvine
            {"e8047056-e202-49b3-829f-7458615103ac", VFXSurfaceTypes.vegetation},// Coral_reef_purple_tentacle_plant_01_01 
            {"3dbab1b9-cc52-4da4-8633-89b33add18f4", VFXSurfaceTypes.vegetation},// Coral_reef_purple_tentacle_plant_01_02 
            //{"e80b22ff-064d-46ca-b71e-456d6b3426ab", VFXSurfaceTypes.vegetation},// Coral_reef_purple_fan 
            //{"8fa4a413-57fa-47a3-828d-de2255dbce4f", VFXSurfaceTypes.wood},// farming_plant_03 HangingFruitTree  
            //{"4626f3eb-23c3-4e04-b9df-829cb051758a", VFXSurfaceTypes.wood},// land_plant_middle_01 BulboTree  
            //{"", VFXSurfaceTypes.vegetation},// Coral_reef_purple_fan 
        };

        Dictionary<TechType, CellLevel> cellLevels_ = new Dictionary<TechType, CellLevel> {
            {TechType.PurpleBranches, CellLevel.Medium },
            {TechType.RedConePlant, CellLevel.Medium },
            //{TechType.DrillableAluminiumOxide, CellLevel.Medium }, not placed
            {TechType.DrillableCopper, CellLevel.Medium },
            //{TechType.DrillableDiamond, CellLevel.Medium }, not placed
            {TechType.DrillableGold, CellLevel.Medium },
            {TechType.DrillableKyanite, CellLevel.Far },// near lava, has to be visible
            {TechType.DrillableLead, CellLevel.Medium },
            {TechType.DrillableLithium, CellLevel.Medium },
            {TechType.DrillableMagnetite, CellLevel.Medium },
            //{TechType.DrillableMercury, CellLevel.Medium }, not placed
            {TechType.DrillableNickel, CellLevel.Medium },
            {TechType.DrillableQuartz, CellLevel.Medium },
            {TechType.DrillableSalt, CellLevel.Medium },
            {TechType.DrillableSilver, CellLevel.Medium },
            //{TechType.DrillableSulphur, CellLevel.Medium }, not placed
            {TechType.DrillableTitanium, CellLevel.Medium },
            {TechType.DrillableUranium, CellLevel.Medium },
            {TechType.ShellGrass, CellLevel.Medium },
            {TechType.BrainCoral, CellLevel.Medium },
            {TechType.LargeFloater, CellLevel.Far },
        };

        Dictionary<string, CellLevel> cellLevels = new Dictionary<string, CellLevel> {
        {"0642b532-9433-4f65-aa39-7757d954b7d2",  CellLevel.Medium},// Crab_snake_mushrooms_06_01
        {"159a22bd-8ab9-479b-95c0-35b09ecdd8b7",  CellLevel.Medium},// Crab_snake_mushrooms_06_02
        {"234a33e5-693f-4458-a916-5b1108c33fc2",  CellLevel.Medium},// Crab_snake_mushrooms_06_03
        {"d00efe9c-3412-4592-9c85-866be52d34cf",  CellLevel.Medium},// Crab_snake_mushrooms_06_04
        {"8ab168d7-dce9-4a2f-bbbc-79c3b632776f",  CellLevel.Medium},// Crab_snake_mushrooms_06_06
        {"4856ff40-43d2-4b15-acdc-d6a45f85c157",  CellLevel.Medium},// coral_reef_plant_middle_12
        {"c853e507-6ad9-4ff2-95c1-6044f024a19e",  CellLevel.Far},// coral_reef_Stalactite_02_01
        {"0515430d-18cb-4ac7-bf6d-22bc877a16aa",  CellLevel.Far},// coral_reef_Stalactites_cluster_01
        {"1cafd118-47e6-48c4-bfd7-718df9984685",  CellLevel.Far},// Coral_reef_floating_stones_mid_01
        {"228e5af5-a579-4c99-9fb0-04b653f73cd3",  CellLevel.Far},// Coral_reef_floating_stones_small_01
        {"1645f35d-af23-4b98-b1e4-44d430421721",  CellLevel.Far},// Coral_reef_floating_stones_small_02
        {"71498905-2ce2-4622-8d6f-40212f6202df",  CellLevel.Medium},// Spiral_blue_thing_cluster_01
        {"6f5c4850-b8bd-461a-999d-1c49d69ffe3a",  CellLevel.Medium},// Spiral_blue_thing_cluster_02
        {"94d7ed83-abb8-49af-9f27-10771dcd1485",  CellLevel.Medium},// Spiral_blue_thing_cluster_05
        {"82287160-87eb-4fdd-ae33-945ba666ae60",  CellLevel.Medium},// Spiral_blue_thing_cluster_07
        //{"",  CellLevel.Medium},// coral_reef_plant_middle_12
        };

        List<string> decoPlants = new List<string> {
            "71498905-2ce2-4622-8d6f-40212f6202df",// Spiral_blue_thing_cluster_01
            "6f5c4850-b8bd-461a-999d-1c49d69ffe3a",// Spiral_blue_thing_cluster_02
            "94d7ed83-abb8-49af-9f27-10771dcd1485",// Spiral_blue_thing_cluster_05
            "82287160-87eb-4fdd-ae33-945ba666ae60",// Spiral_blue_thing_cluster_07
            "6d9e37de-f808-4621-a762-e0d6340b30dc",// Coral_reef_small_deco_03
        };

        List<TechType> techTypeToRemoveCollision = new List<TechType> {
            TechType.PurpleFan, TechType.RedGreenTentacle
        };

        List<string> classIDToRemoveCollision = new List<string> {
            "e8047056-e202-49b3-829f-7458615103ac",// Coral_reef_purple_tentacle_plant_01_01
            "3dbab1b9-cc52-4da4-8633-89b33add18f4",// Coral_reef_purple_tentacle_plant_01_02
            "8409a079-a96c-43d3-a891-af500b04e0af",// Coral_reef_Gabe's_Feather
            "6d9e37de-f808-4621-a762-e0d6340b30dc",// Coral_reef_small_deco_03
            //"",// Coral_reef_purple_fan
        };

        List<string> fruitPlants = new List<string> {
            "da7341c3-e6a3-4cd3-ad57-49a4dc732ac9",// Coral_reef_cave_root_01_blood_Light
            "b0cae640-b155-4bac-9ed5-29ba64a1ee9f",// Coral_reef_cave_root_02_blood_Light
            "5beba896-bccf-4993-8bcb-1cdabb68e706",// Coral_reef_cave_root_03_blood_Light
            "db79ee0b-65e9-4ea1-8b8b-948bbae128f7",// Coral_reef_cave_root_04_blood_Light
            "1c28891f-df08-4eee-a081-118955b0d303",// Coral_reef_Kelp_blood_01_Light
            "a4912ba2-5643-46ee-bd69-6be53dd55d45",// Coral_reef_Kelp_blood_02_Light
            "e0ae8532-a6d5-436f-bdc0-846061d91686",// Coral_reef_Kelp_blood_03_Light
            "66f2188b-b537-49ac-b6e7-08f446eca9e8",// Coral_reef_Kelp_blood_04_Light
            "8fa4a413-57fa-47a3-828d-de2255dbce4f",// farming_plant_03 HangingFruitTree
            "a17ef178-6952-4a91-8f66-44e1d8ca0575",// Coral_reef_kelp_02_short
            "7329db6b-7385-4e77-8afa-71830ead9350",// Coral_reef_kelp_01_mid
            "de972f1f-daab-41d6-b274-5173b0dd23d8",// Coral_reef_kelp_01_long
        };

        List<string> fragments = new List<string> {
            "6f9e2e29-9eba-4261-ba7b-ed5eac120b91",// Map_Room_fragment_01 
            "a72c61ae-1ab7-4ee8-bced-b76505d3f1e2",// Map_Room_fragment_02
            "f350b8ae-9ee4-4349-a6de-d031b11c82b1",// Map_Room_fragment_03
            "cf4ca320-bb13-45b6-b4c9-2a079023e787",// Map_Room_fragment_04
            "30189aca-d5b5-4363-8398-11d6a109addb",// Moon_Pool_fragment_01
            "85259b00-2672-497e-bec9-b200a1ab012f",// Moon_Pool_fragment_02
            "f744e6d9-f719-4653-906b-34ed5dbdb230",// Moon_Pool_fragment_03
            "498a843d-efed-4fc0-8243-13453aee2559",// Moon_Pool_fragment_04
            "72a8c169-ca00-48aa-94f9-d92d932548e0",// Moon_Pool_fragment_05
            "bd3c0070-3af4-4e44-b50d-506c438829ec",// Moon_Pool_fragment_06
            "33d63e93-e5fd-4911-b7ce-63bf43cc6c95",// battery_charging_station_damaged_base
            "18521f9a-4b46-4994-9475-984d64993d9c",// battery_charging_station_damaged_cover
            "c395b5a9-9e44-4c2b-b030-1e987009f5b7",// Bio_reactor_damaged_01
            "db2df7f8-db1a-4210-8ca0-73531b93b889",// Bio_reactor_damaged_02
            "ffef3320-9d36-4a0f-8b2b-6ab1247426cb",// Bio_reactor_damaged_03
            "088bda17-d77b-4c64-9f2a-42c8bcf9f7a5",// Bio_reactor_damaged_04
            "10a176a9-8762-492f-b1b6-0b32e737b1bc",// constructorfragment1
            "e411825d-cc5e-4717-a1c1-a533c9d40939",// constructorfragment2
            "f60b5fb5-9430-4a1d-9978-390cd4685132",// constructorfragment3
            "871b7a1f-1b43-487f-87af-877fb6260613",// constructorfragment4
            "d0115374-d251-4e52-8404-af15cc6244c3",// cyclopsbridgefragment1
            "72d0460c-1b50-416b-8a9d-58e415132d3d",// cyclopsbridgefragment2
            "0e54e72a-3da8-4f5d-8440-f51033fcad8c",// cyclopsbridgefragment3
            "3c076458-505e-4683-90c1-34c1f7939a0f",// cyclopsenginefragment1
            "ceaa255c-e1e7-4cbc-938f-fcf735bca757",// cyclopsenginefragment2
            "52568520-541c-4a5a-a4fa-b5dbac219915",// cyclopsenginefragment3
            "656f6191-214e-4b26-8833-fa47b297219e",// cyclopshullfragment1
            "5643d0f8-c305-4bdc-b80d-012d8cbfb6e5",// cyclopshullfragment2
            "0ba2de19-0f6e-4469-bf77-8c0f9db95875",// cyclopshullfragment3
            "7f673d9f-0d08-4c3b-a229-d3124c0ac197",// cyclopshullfragment4
            "bc62e06d-0ccc-47b4-90c5-62f6422d4af7",// cyclopshullfragment5
            "d5f3a601-729e-407a-b229-fd3daa601dd3",// cyclopshullfragment6
            "bc7d4038-d681-41dd-b7ae-e134048f421b",// cyclopshullfragment7
            "f4b3942e-02d8-4526-b384-677a2ad9ce58",// cyclopshullfragment8
            "b19be61c-b011-400f-9cfe-4ad9c70adf6d",// cyclopshullfragment9
            "2d1951c4-49ec-4298-bc1c-b3af75092832",// exosuitfragment1
            "80bf1cc6-d627-47a1-b4b4-33f47e59231c",// exosuitfragment2
            "44d49e2d-37ab-47b1-9f1d-bb63d16ccfbb",// exosuitfragment3
            "2a70438f-ecbb-4c2c-9512-848c46b43316",// exosuitfragment4
            "16d326b7-0cbe-4df9-bc58-3cd26b5458af",// exosuitfragment5
            "ad23314d-256d-4b8a-ab4e-e49502f62723",// ExosuitDrillArmfragment !!! not fragment techtype
            "4904e113-8765-4d27-a750-33d89d50a8ae",// ExosuitGrapplingArmFragment
            "9abc15fc-433c-4fbd-b3e6-d1b2cc73abb2",// ExosuitPropulsionArmFragment
            "1c953310-8436-4012-8e0d-3b4634f07e57",// ExosuitTorpedoArmFragment
            "54a7d6b6-280a-43d5-8bdd-eada3dd5f6c3",// exosuit_damaged_01 !!! not fragment techtype
            "740258f8-bf36-484b-bdb8-d9e5dc3f1e3e",// exosuit_damaged_02 !!! not fragment techtype
            "c3d6cad0-1981-4dfd-9a11-62eb0490b130",// exosuit_damaged_03 !!! not fragment techtype
            "4c924ad2-ab9a-4ff8-b2bd-3541b1b9d043",// exosuit_damaged_04 !!! not fragment techtype
            "1b8df552-1b3e-4e96-ba1a-3d35afcb2c18",// exosuit_damaged_05 !!! not fragment techtype
            "d70c8458-4b19-4dbc-ba67-afa654af1999",// exosuit_damaged_06 !!! not fragment techtype
            "6e4f85c2-ad1d-4d0a-b20c-1158204ee424",// GravSphere_Fragment
            "aeff4dad-8256-475b-a764-d5e7028220ce",// LaserCutterFragment
            "c1f8aa68-0ac0-419e-81ec-b7a388027c24",// LEDLightFragment
            "7436aeba-f8df-4887-b369-e630fa01f716",// PowerTransmitterFragment
            "21e4c817-e3a7-4a0d-a931-0bc68243cb1e",// PropulsionCannonFragment
            "47ca9b30-9bf2-4956-8f30-2407567496ac",// reinforcehullfragment
            "127f22a3-44cd-4341-adb8-8937317f53de",// SeaglideFragment
            "292ba610-ed40-461f-826b-7b2645b37b5f",// seamoth_fragment_01
            "ba90d6e8-4a8a-4c66-8f1a-2b02f3fc3acd",// seamoth_fragment_01_aurora !!! not fragment techtype
            "1f5cee66-a02f-4693-a1bd-928c938c7e77",// seamoth_fragment_02
            "284573d8-9a80-4867-a09a-85df573c29ef",// seamoth_fragment_03
            "1abc3b6f-0b8e-4066-8288-48e5d06ac8c9",// seamoth_fragment_03_aurora !!! not fragment techtype
            "b9764db6-1f2a-4cfc-bda0-8a179cb7e155",// seamoth_fragment_04
            "a73218d6-b307-450a-890e-ec2e2c206324",// seamoth_fragment_05
            "57c48cfa-867d-4722-8e51-5bf4fee0d9e3",// StasisRifleFragment
            "403b8d2f-b009-483d-8358-bfcde62daa42",// Nuclear_reactor_damaged_01
            "6c58dc6b-2ae2-41ca-8c43-f953b919f7ab",// Nuclear_reactor_damaged_02
            "e35fb5aa-19ba-4736-8f8c-6db679b5766c",// Nuclear_reactor_damaged_03
            "872b7c65-7597-4ca2-9c96-03b2405b8784",// Nuclear_reactor_damaged_04
            "f41a1855-1dc1-495a-adf2-c4495fd39936",// Power_Cell_Charging_Station_damaged_base
            "9569f745-4853-47cf-aaf5-b849c91651f4",// Power_Cell_Charging_Station_damaged_cover
            "8029a9ce-ab75-46d0-a8ab-63138f6f83e4",// submarine_Workbench_damaged_01
            "4cc70e47-a05f-4e27-9920-9a6d0e90083d",// submarine_Workbench_damaged_02
            "d420cd62-2983-44a9-886a-8c7d214a2db9",// submarine_Workbench_damaged_03
            "88c4c1fa-0b52-44cb-9db5-2ef18447ae5c",// Thermal_reactor_damaged_01
            "06cc39eb-af4c-4573-866a-d92e5d4c2bf1",// Thermal_reactor_damaged_02
            "47c32ae8-b168-4ddf-bbae-7467038e3457",// Thermal_reactor_damaged_03
            "a50c91eb-f7cf-4fbf-8157-0aa8d444820c",// Beacon_Fragment
        };


        public void FixPrefabs()
        {
            if (prefabsFixed)
                return;

            if (Util.IsGraphicsPresetHighDetail())
            {
                foreach (TechType tt in badLODs)
                {
                    UWE.CoroutineHost.StartCoroutine(IncreaseLODdistane(tt));
                }
                foreach (string classID in badLODs_)
                {
                    UWE.CoroutineHost.StartCoroutine(IncreaseLODdistane(classID));
                }
                foreach (string classID in terribleLODs_)
                {
                    UWE.CoroutineHost.StartCoroutine(DisableLODs(classID));
                }
                foreach (TechType tt in terribleLODs)
                {
                    UWE.CoroutineHost.StartCoroutine(DisableLODs(tt));
                }
                foreach (string classID in prefabsWithoutShadows)
                {
                    UWE.CoroutineHost.StartCoroutine(EnableShadowCasting(classID));
                }
                foreach (var kv in prefabsWithoutShadows_)
                {
                    UWE.CoroutineHost.StartCoroutine(EnableShadowCasting(kv.Key, kv.Value));
                }
                UWE.CoroutineHost.StartCoroutine(FixPrecursorLabContainerShadow());
                UWE.CoroutineHost.StartCoroutine(FixDissectionRoomEmperorTankShadow());
            }
            foreach (var kv in materialZoffsets)
            {
                UWE.CoroutineHost.StartCoroutine(SetMaterialZoffset(kv.Key, kv.Value));
            }
            foreach (var kv in materialZoffsets_)
            {
                UWE.CoroutineHost.StartCoroutine(SetMaterialZoffset(kv.Key, kv.Value));
            }
            foreach (string classID in fragments)
            {
                UWE.CoroutineHost.StartCoroutine(FixFragment(classID));
            }
            if (ConfigToEdit.fruitGrowTime.Value > 0)
            {
                foreach (string classID in fruitPlants)
                {
                    UWE.CoroutineHost.StartCoroutine(EnsureFruits(classID));
                }
            }
            foreach (TechType techType in techTypesToAddWorldForces)
            {
                UWE.CoroutineHost.StartCoroutine(AddWorldForces(techType));
            }
            foreach (string classID in classIDToRemoveCollision)
            {
                UWE.CoroutineHost.StartCoroutine(DisableCollision(classID));
            }
            foreach (TechType techType in techTypeToRemoveCollision)
            {
                UWE.CoroutineHost.StartCoroutine(DisableCollision(techType));
            }
            foreach (var kv in cellLevels)
            {
                UWE.CoroutineHost.StartCoroutine(SetCellLevel(kv.Key, kv.Value));
            }
            foreach (var kv in cellLevels_)
            {
                UWE.CoroutineHost.StartCoroutine(SetCellLevel(kv.Key, kv.Value));
            }
            foreach (var kv in surfaceTypes)
            {
                UWE.CoroutineHost.StartCoroutine(AddVFXsurfaceComponent(kv.Key, kv.Value));
            }
            foreach (var kv in surfaceTypes_)
            {
                UWE.CoroutineHost.StartCoroutine(AddVFXsurfaceComponent(kv.Key, kv.Value));
            }
            foreach (var kv in classIDtoRemoveGlow)
            {
                UWE.CoroutineHost.StartCoroutine(RemoveGlow(kv.Key));
            }
            foreach (TechType tt in techTypesToMakeUnmovable)
            {
                UWE.CoroutineHost.StartCoroutine(MakeUnmovable(tt));
            }
            foreach (string classID in classIDToMakeUnmovable)
            {
                UWE.CoroutineHost.StartCoroutine(MakeUnmovable(classID));
            }
            foreach (var kv in eatables)
            {
                UWE.CoroutineHost.StartCoroutine(MakeEatable(kv.Key));
            }
            foreach (string classID in glassRendererList)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(classID));
            }
            foreach (var kv in glassRenderers)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassRenderers_)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassRenderers__)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in glassRenderers___)
            {
                UWE.CoroutineHost.StartCoroutine(DisableShadowCasting(kv.Key, kv.Value));
            }
            foreach (var kv in materialsToEnableAlphaClip)
            {
                UWE.CoroutineHost.StartCoroutine(EnableAlphaClip(kv.Key, kv.Value));
            }
            foreach (var kv in prefabsToEnableDitherAlpha)
            {
                UWE.CoroutineHost.StartCoroutine(EnableDitherAlpha(kv.Key, kv.Value));
            }
            foreach (var kv in materialsToEnableDitherAlpha)
            {
                UWE.CoroutineHost.StartCoroutine(EnableDitherAlpha(kv.Key, kv.Value));
            }
            UWE.CoroutineHost.StartCoroutine(FixPrisonTeleporterRoom03Shadows());
            UWE.CoroutineHost.StartCoroutine(FixOrangeMushroomCollider());
            UWE.CoroutineHost.StartCoroutine(FixBiohazardTrashCanDesc());
            UWE.CoroutineHost.StartCoroutine(FixPrisonTankGlass());
            UWE.CoroutineHost.StartCoroutine(FixStones());
            prefabsFixed = true;
        }

        private static IEnumerator EnableDitherAlpha(string classID, string path)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableDitherAlpha No prefab for " + classID);
                yield break;
            }
            EnableDitherAlpha(prefab, path);
        }

        private static IEnumerator EnableDitherAlpha(string classID, MaterialZoffsetData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableDitherAlpha No prefab for " + classID);
                yield break;
            }
            EnableDitherAlpha(prefab, data);
        }

        private static void EnableDitherAlpha(GameObject prefab, string path)
        {
            Transform t = prefab.transform.Find(path);
            if (t == null)
            {
                Main.logger.LogError($"EnableDitherAlpha {prefab.name} has no child {path}");
                return;
            }
            Renderer renderer = t.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Main.logger.LogError($"EnableDitherAlpha {prefab.name} has no renderer {path}");
                return;
            }
            renderer.material.SetFloat(zOffset, 0);
            renderer.material.EnableKeyword("UWE_DITHERALPHA");
        }

        private static void EnableDitherAlpha(GameObject prefab, MaterialZoffsetData data)
        {
            Transform t = prefab.transform.Find(data.rendererPath);
            if (t == null)
            {
                Main.logger.LogError($"EnableDitherAlpha {prefab.name} has no child {data.rendererPath}");
                return;
            }
            Renderer renderer = t.GetComponentInChildren<Renderer>();
            if (renderer == null)
            {
                Main.logger.LogError($"EnableDitherAlpha {prefab.name} has no renderer {data.rendererPath}");
                return;
            }
            Material material = renderer.materials[data.materialIndex];
            material.SetFloat(zOffset, data.offsetValue);
            material.EnableKeyword("UWE_DITHERALPHA");
        }

        private static IEnumerator EnableAlphaClip(string classID, MaterialZoffsetData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableAlphaClip No prefab for " + classID);
                yield break;
            }
            //Main.logger.LogDebug($"EnableAlphaClip classID {classID}");
            EnableAlphaClip(prefab, data);
        }

        private static void EnableAlphaClip(GameObject prefab, MaterialZoffsetData data)
        {
            //Main.logger.LogDebug($"EnableAlphaClip prefab {prefab.name}");
            Transform t = prefab.transform.Find(data.rendererPath);
            if (t == null)
            {
                Main.logger.LogError($"EnableAlphaClip {prefab.name} has no child {data.rendererPath}");
                return;
            }
            if (data.materialIndexes != null)
            {
                foreach (int i in data.materialIndexes)
                    EnableAlphaClip(i, t);

                return;
            }
            EnableAlphaClip(data.materialIndex, t);
        }

        private static void EnableAlphaClip(int matIndex, Transform transform)
        {
            //Main.logger.LogDebug($"EnableAlphaClip transform {transform.name} matIndex {matIndex}");
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer == null)
            {
                Main.logger.LogError($"EnableAlphaClip {renderer.name} has no renderer");
                return;
            }
            //Main.logger.LogDebug($"EnableAlphaClip renderer {renderer.name} materials  {renderer.materials.Length}matIndex {matIndex}");
            Material material = renderer.materials[matIndex];
            if (material == null)
            {
                Main.logger.LogError($"EnableAlphaClip renderer {renderer.name} has no material at index {matIndex}");
                return;
            }
            material.SetFloat(zOffset, 0);
            material.EnableKeyword("MARMO_ALPHA_CLIP");
        }

        private IEnumerator AddVFXsurfaceComponent(TechType techType, VFXSurfaceTypes surfaceType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("AddVFXsurfaceComponent No prefab for " + techType);
                yield break;
            }
            prefab.AddVFXsurfaceComponent(surfaceType);
        }

        IEnumerator SetMaterialZoffset(string classID, MaterialZoffsetData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixMaterialZoffset No prefab for " + classID);
                yield break;
            }
            SetMaterialZoffset(prefab, data);
        }

        IEnumerator SetMaterialZoffset(string classID, List<MaterialZoffsetData> dataList)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixMaterialZoffset No prefab for " + classID);
                yield break;
            }
            foreach (MaterialZoffsetData data in dataList)
                SetMaterialZoffset(prefab, data);
        }

        private static void SetMaterialZoffset(GameObject prefab, MaterialZoffsetData data)
        {
            Transform t = prefab.transform.Find(data.rendererPath);
            if (t == null)
            {
                Main.logger.LogError($"SetMaterialZoffset {prefab.name} has no child {data.rendererPath}");
                return;
            }
            Renderer renderer = t.GetComponentInChildren<Renderer>();
            if (data.materialIndexes != null)
            {
                foreach (int i in data.materialIndexes)
                {
                    if (i >= renderer.materials.Length)
                    {
                        Main.logger.LogError($"SetMaterialZoffset renderer {renderer.name} has no material at index {i}");
                        continue;
                    }
                    renderer.materials[i].SetFloat(zOffset, data.offsetValue);
                }
                return;
            }
            if (data.materialIndex < 0)
            {
                foreach (Material m in renderer.materials)
                    m.SetFloat(zOffset, data.offsetValue);

                return;
            }
            Material material = renderer.materials[data.materialIndex];
            if (material == null)
            {
                Main.logger.LogError($"SetMaterialZoffset renderer {renderer.name} has no material at index {data.materialIndex}");
                return;
            }
            material.SetFloat(zOffset, data.offsetValue);
        }

        IEnumerator FixFragment(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixFragment No prefab for " + classID);
                yield break;
            }
            ResourceTracker resourceTracker = prefab.GetComponent<ResourceTracker>();
            if (resourceTracker == null)
            {
                Main.logger.LogError("FixFragment No ResourceTracker for " + prefab.name);
                yield break;
            }
            //Main.logger.LogError($"FixFragment {prefab.name} RT TT {resourceTracker.techType} RT OTT {resourceTracker.overrideTechType}");
            resourceTracker.overrideTechType = TechType.Fragment;
            Pickupable pickupable = prefab.GetComponent<Pickupable>();
            if (pickupable)
                UnityEngine.Object.Destroy(pickupable);
        }

        IEnumerator EnsureFruits(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnsureFruits No prefab for " + classID);
                yield break;
            }
            Util.EnsureFruits(prefab);
        }

        IEnumerator AddWorldForces(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("AddWorldForces No prefab for " + techType);
                yield break;
            }
            Rigidbody rb = prefab.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Main.logger.LogError("AddWorldForces No Rigidbody on " + prefab.name);
                yield break;
            }
            WorldForces wf = prefab.EnsureComponent<WorldForces>();
            //worldForces.underwaterDrag = 11;
            wf.useRigidbody = rb;
            rb.isKinematic = false;
            rb.useGravity = false;
            //WorldForcesManager.Instance.AddWorldForces(wf);
        }

        IEnumerator DisableCollision(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableCollision No prefab for " + classID);
                yield break;
            }
            Util.DisableCollision(prefab);
        }

        IEnumerator DisableCollision(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("DisableCollision No prefab for " + techType);
                yield break;
            }
            Util.DisableCollision(prefab);
        }


        IEnumerator SetCellLevel(string classID, CellLevel newLevel)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("SetCellLevel No prefab for " + classID);
                yield break;
            }
            Util.SetEntityCellLevel(prefab, newLevel);
        }

        IEnumerator SetCellLevel(TechType techType, CellLevel newLevel)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("SetCellLevel No prefab for " + techType);
                yield break;
            }
            Util.SetEntityCellLevel(prefab, newLevel);
        }

        IEnumerator FixStones()
        {// fix bug: rocks that block cave entrance fall down bc they load before terrain 
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("a1f3da68-d810-44ff-a0a2-6cf3c6a3eff5");// FloatingStone5
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixStones No prefab for FloatingStone5");
                yield break;
            }
            LargeWorldEntity lwe = prefab.GetComponent<LargeWorldEntity>();
            lwe.cellLevel = CellLevel.Near;
        }

        IEnumerator AddVFXsurfaceComponent(string classID, VFXSurfaceTypes surfaceType)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("AddVFXsurfaceComponent No prefab for " + classID);
                yield break;
            }
            prefab.AddVFXsurfaceComponent(surfaceType);
        }

        IEnumerator RemoveGlow(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("RemoveGlow No prefab for " + classID);
                yield break;
            }
            RendererData data = classIDtoRemoveGlow[classID];
            prefab.transform.DisableGlowShader(data);
        }

        IEnumerator MakeUnmovable(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("MakeUnmovable No prefab for" + techType);
                yield break;
            }
            Util.MakeUnmovable(prefab);
        }

        IEnumerator MakeUnmovable(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("MakeUnmovable No prefab for " + classID);
                yield break;
            }
            Util.MakeUnmovable(prefab);
        }

        IEnumerator DisableLODs(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("DisableLODs No prefab for" + techType);
                yield break;
            }
            prefab.DisableLODs();
        }

        IEnumerator DisableLODs(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableLODs No prefab for " + classID);
                yield break;
            }
            prefab.DisableLODs();
        }

        IEnumerator FixBulboTree()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.BulboTree);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("FixBulboTree No prefab for BulboTree");
                yield break;
            }
            prefab.DisableLODs();
            //prefab.DisableGlowShader();
        }

        IEnumerator IncreaseLODdistane(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("ForceLOD No prefab for " + classID);
                yield break;
            }
            Util.IncreaseLODdistane(prefab);
        }

        IEnumerator IncreaseLODdistane(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("ForceLOD No prefab for " + techType);
                yield break;
            }
            Util.IncreaseLODdistane(prefab);
        }

        IEnumerator FixBiohazardTrashCanDesc()
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(TechType.LabTrashcan);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("MakeEatable No prefab for LabTrashcan");
                yield break;
            }
            Trashcan trashcan = prefab.GetComponent<Trashcan>();
            trashcan.storageContainer.hoverText = Language.main.Get("LabTrashcan");
        }

        IEnumerator MakeEatable(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("MakeEatable No prefab for " + techType);
                yield break;
            }
            Util.MakeEatable(prefab, eatables[techType]);
        }

        IEnumerator DisableShadowCasting(string classID)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + classID);
                yield break;
            }
            prefab.transform.DisableShadowCastingInChildren();
        }

        IEnumerator DisableShadowCasting(string classID, string path)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + classID);
                yield break;
            }
            prefab.transform.DisableShadowCasting(path);
        }

        IEnumerator DisableShadowCasting(string classID, RendererData data)
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + classID);
                yield break;
            }
            prefab.transform.DisableShadowCasting(data);
        }

        IEnumerator DisableShadowCasting(TechType techType, string path)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + techType);
                yield break;
            }
            prefab.transform.DisableShadowCasting(path);
        }

        IEnumerator DisableShadowCasting(TechType techType, RendererData data)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            GameObject prefab = request.GetResult();
            if (prefab == null)
            {
                Main.logger.LogError("DisableShadowCasting No prefab for " + techType);
                yield break;
            }
            prefab.transform.DisableShadowCasting(data);
        }

        IEnumerator EnableShadowCasting(string classID)
        {
            //Main.logger.LogDebug("EnableShadowCasting " + classID);
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableShadowCasting No prefab for " + classID);
                yield break;
            }
            prefab.transform.EnableShadowCastingInChildren();
        }

        IEnumerator EnableShadowCasting(string classID, RendererData value)
        {
            //Main.logger.LogDebug("EnableShadowCasting RendererData " + classID);
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classID);
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("EnableShadowCasting No prefab for " + classID);
                yield break;
            }
            RendererData data = prefabsWithoutShadows_[classID];
            if (data == null)
            {
                Main.logger.LogError("EnableShadowCasting No RendererData for " + classID);
                yield break;
            }
            prefab.transform.EnableShadowCasting(data);
        }

        IEnumerator FixPrisonTankGlass()
        {
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("ac6dd6fe-5835-41b9-96e8-2ec4120699ff");
            yield return request; // Precursor_Prison_TankGlassSmall  188 -1440 -420
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixPrisonTankGlass No prefab for PrisonTankGlass");
                yield break;
            }
            prefab.AddComponent<PrisonTankGlassFixer>();
        }

        IEnumerator FixPrisonTeleporterRoom03Shadows()
        { //  334 -1430 -278
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("8c3cc489-cb05-42a0-830d-b9dc73a841c0"); // Precursor_Prison_TeleporterRoom_03
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("No prefab for Precursor_Prison_TeleporterRoom_03");
                yield break;
            }
            Transform meshes = prefab.transform.GetChild(1);
            foreach (int i in new int[] { 19, 20, 40, 41 })
            {
                Transform t = meshes.GetChild(i);
                Renderer renderer = t.GetComponentInChildren<Renderer>();
                //Main.logger.LogDebug("FixPrisonTeleporterRoom03Shadows Renderer " + renderer.name);
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        IEnumerator FixPrecursorLabContainerShadow()
        { //  183 -1440 -423
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("d0fea4da-39f2-47b4-aece-bb12fe7f9410"); // Precursor_lab_container_01
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("No prefab for Precursor_lab_container_01");
                yield break;
            }
            Renderer[] renderers = prefab.transform.GetComponentsInChildren<Renderer>();
            renderers[0].receiveShadows = true;
            renderers[0].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderers[2].receiveShadows = true;
            renderers[2].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        IEnumerator FixDissectionRoomEmperorTankShadow()
        { //  225 -1428 -282
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("44974fcd-c47a-41aa-a279-43eaf234bfa6"); // Precursor_Prison_DissectionRoom_EmperorTank
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("No prefab for Precursor_Prison_DissectionRoom_EmperorTank");
                yield break;
            }
            for (int i = 2; i < 5; i++)
            {
                Transform child = prefab.transform.GetChild(i);
                child.EnableShadowCastingInChildren();
            }
        }

        IEnumerator FixOrangeMushroomCollider()
        { // land_plant_middle_05_01    center y 0.54 rad 0.7864308  h 2
            IPrefabRequest request = PrefabDatabase.GetPrefabAsync("35056c71-5da7-4e73-be60-3c22c5c9e75c");
            yield return request;
            GameObject prefab;
            if (request.TryGetPrefab(out prefab) == false)
            {
                Main.logger.LogError("FixOrangeMushroomCollider No prefab ");
                yield break;
            }
            CapsuleCollider cc = prefab.GetComponentInChildren<CapsuleCollider>();
            cc.height = 0;
            cc.center = new Vector3(0, .3f, 0);
        }


    }

    class MaterialZoffsetData
    {
        public string rendererPath;
        public int[] materialIndexes;
        public int materialIndex;
        public int offsetValue;

        public MaterialZoffsetData(string rendererPath, int materialIndex, int offsetValue)
        {
            this.rendererPath = rendererPath;
            this.materialIndex = materialIndex;
            this.offsetValue = offsetValue;
        }

        public MaterialZoffsetData(string rendererPath, int[] materialIndexes)
        {
            this.rendererPath = rendererPath;
            this.materialIndexes = materialIndexes;
        }
        public MaterialZoffsetData(string rendererPath, int materialIndex)
        {
            this.rendererPath = rendererPath;
            this.materialIndex = materialIndex;
        }
    }

    public struct EatableData
    {
        public int food;
        public int water;
        public int health;

        public EatableData(int food, int water, int health)
        {
            this.food = food;
            this.water = water;
            this.health = health;
        }
    }

    public class RendererData
    {
        public string parentPath;
        public List<string> renderers;

        public RendererData(string parentPath, List<string> renderers)
        {
            this.parentPath = parentPath;
            this.renderers = renderers;
        }

        public RendererData(string parentPath)
        {
            this.parentPath = parentPath;
        }
    }

    public class PrisonTankGlassFixer : MonoBehaviour
    {
        public void Start()
        {
            //Main.logger.LogDebug("Prison Tank glass Start " + transform.parent.name);
            if (transform.parent.name == "CellRoot(Clone)")
            {
                //AddDebug("Destroy Prison Tank glass");
                Destroy(transform.gameObject);
            }
            else
                Destroy(this);
        }
    }


}


