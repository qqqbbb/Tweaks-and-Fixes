
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using Nautilus.Assets;
using Nautilus.Assets.Gadgets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using Nautilus.Options;
using Nautilus.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        public const string
            MODNAME = "Tweaks and Fixes",
            GUID = "qqqbbb.subnautica.tweaksAndFixes",
            VERSION = "4.21.0";

        public static ManualLogSource logger;
        public static bool gameLoaded;  // WaitScreen.IsWaiting
        public static bool advancedInventoryLoaded;
        public static bool flareRepairLoaded;
        public static bool cyclopsDockingLoaded;
        public static bool vehicleLightsImprovedLoaded; // not updated
        public static bool pickupFullCarryallIsLoaded;
        public static bool seaglideMapControlsLoaded;  // not updated
        public static bool baseLightSwitchLoaded;
        public static bool visibleLockerInteriorLoaded;
        public static bool exosuitTorpedoDisplayLoaded; // not updated
        public static bool torpedoImprovementsLoaded;
        public static bool cyclopsOverheatLoaded;
        public static bool aggressiveFaunaLoaded;
        static string configToEditPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MODNAME + Path.DirectorySeparatorChar + "ConfigToEdit.cfg";
        static string configMenuPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MODNAME + Path.DirectorySeparatorChar + "ConfigMenu.cfg";
        public static ConfigMain configMain = new ConfigMain();
        internal static OptionsMenu options;
        public static ConfigFile configMenu;
        public static ConfigFile configToEdit;
        public static WaitForSeconds oneSecond = new WaitForSeconds(1f);
        public static WaitUntil waitUntilGameLoaded = new WaitUntil(() => gameLoaded);

        public void CleanUp()
        {
            //logger.LogInfo("CleanUp");
            gameLoaded = false;
            QuickSlot_Cycle.invChanged = true;
            Crush_Damage.extraCrushDepth = 0;
            Crush_Damage.crushDamageResistance = 0;
            Cyclops_Constructable_Collision.CleanUp();
            Geyser_.CleanUp();
            Gravsphere_.gasPods.Clear();
            Gravsphere_.gravSphereFish.Clear();
            Decoy_Patch.decoysToDestroy.Clear();
            Vehicle_patch.currentVehicleTT = TechType.None;
            Exosuit_Patch.exosuitStarted = false;
            Storage_Patch.savedSigns.Clear();
            Storage_Patch.labelledLockers.Clear();
            PowerConsumption.subPowerRelays.Clear();
            Creatures.pickupShinies.Clear();
            CreatureDeath_.creatureDeathsToDestroy.Clear();
            Drop_items_anywhere.droppedInBase.Clear();
            Drop_items_anywhere.droppedInEscapePod.Clear();
            Player_.healTime = 0;
            Poison_Damage.ResetVars();
            Cyclops_unpowered.poweredSubs.Clear();
            Pickupable_.beacons.Clear();
            Pickupable_.pickupableStorage.Clear();
            Pickupable_.pickupableStorage_.Clear();
            Radiation.auroraRadiation = null;
            Floater_.pickupableFloaters.Clear();
            Medical_Cabinet_.escapePodMedCabinet = null;
            configToEdit.Reload();
            configMain.Load();
        }

        public void LoadedGameSetup()
        {
            //AddDebug("LoadedGameSetup ");
            FixCoralShellPlateHarvestType();
            if (ConfigToEdit.cantScanExosuitClawArm.Value)
                Player_.DisableExosuitClawArmScan();

            if (ConfigToEdit.fixMelons.Value)
            {
                CraftDataHandler.SetItemSize(TechType.MelonPlant, new Vector2int(2, 2));
                //CraftData.itemSizes[TechType.MelonPlant] = ;
            }
            if (PDAScanner.mapping.ContainsKey(TechType.Creepvine))
            { // unlock fibermesh by scanning creepvine
                PDAScanner.mapping[TechType.Creepvine].blueprint = TechType.FiberMesh;
            }
            LoadedGameObjectFixer loadedGameObjectFixer = new LoadedGameObjectFixer();
            loadedGameObjectFixer.IterateRootGameObjects();

            if (configMain.activeSlot != -1 && Player.main.mode == Player.Mode.Normal)
                Inventory.main.quickSlots.SelectImmediate(configMain.activeSlot);

            LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, Language.main.Get("Tooltip_Bladderfish") + Language.main.Get("TF_bladderfish_tooltip"));
            //LanguageHandle r.SetTechTypeTooltip(TechType.SeamothElectricalDefense, Language.main.Get("TF_bladderfish_tooltip"));
            Survival_.RemoveCookedFish();
            Player.main.isUnderwater.changedEvent.AddHandler(Player.main, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(Knife_.OnPlayerUnderwaterChanged));
            Player.main.isUnderwaterForSwimming.changedEvent.AddHandler(Player.main, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(Player_Movement.OnPlayerUnderwaterChanged));
            CreatureDeath_.TryRemoveCorpses();
            Escape_Pod.EscapePodInit();
            Drop_items_anywhere.OnGameLoadingFinished();
            Player.main.groundMotor.forwardMaxSpeed = Player.main.groundMotor.playerController.walkRunForwardMaxSpeed * ConfigMenu.playerGroundSpeedMult.Value;
            Player_Movement.UpdateModifiers();
            MiscSettings.cameraBobbing = ConfigToEdit.cameraBobbing.Value;
            Application.runInBackground = MiscSettings.runInBackground;
            gameLoaded = true;
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "Start")]
        class uGUI_MainMenu_Start_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
            }
        }

        [HarmonyPatch(typeof(MainMenuLoadButton), "Delete")]
        class MainMenuLoadButton_Delete_Patch
        {
            static void Postfix(MainMenuLoadButton __instance)
            {
                //AddDebug("MainMenuLoadButton Delete " + __instance.saveGame);
                configMain.DeleteCurrentSaveSlotData();
            }
        }

        //[HarmonyPatch(typeof(SaveLoadManager))]
        class SaveLoadManager_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("ClearSlotAsync")]
            public static void ClearSlotAsyncPostfix(SaveLoadManager __instance, string slotName)
            { // runs when starting new game
                AddDebug("ClearSlotAsync " + slotName);
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("CreateSlotAsync", new Type[0])]
            public static void CreateSlotAsyncPostfix(SaveLoadManager __instance)
            {
                //AddDebug("SaveLoadManager CreateSlotAsync ");
            }

            //[HarmonyPostfix,HarmonyPatch("SaveToDeepStorageAsync", new Type[0])]
            public static void SaveToDeepStorageAsyncpostfix(SaveLoadManager __instance)
            { // runs after nautilus SaveEvent
                //AddDebug("SaveToDeepStorageAsync");
                //SaveData();
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("LoadSlotsAsync", new Type[0])]
            public static void LoadSlotsAsyncPostfix(SaveLoadManager __instance)
            {

            }
        }

        void SaveData(bool saving)
        {
            if (saving == false)
                return;
            //AddDebug(" SaveData");

            configMain.screenRes = new Screen_Resolution_Fix.ScreenRes(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen);
            configMain.activeSlot = Inventory.main.quickSlots.activeSlot;
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
            if (heldItem != null && heldItem.item.TryGetComponent<PlaceTool>(out _))
            {
                //AddDebug(" heldItem PlaceTool");
                configMain.activeSlot = -1;
            }
            Decoy_Patch.DestroyDecoys();
            configMain.Save();
            //AddDebug("Save configMain " + Inventory.main.quickSlots.activeSlot);
        }

        public void Setup()
        {
            logger = Logger;
            LanguageHandler.RegisterLocalizationFolder();
            configMenu = new ConfigFile(configMenuPath, false);
            ConfigMenu.Bind();
            configMain.Load();
            configToEdit = new ConfigFile(configToEditPath, false);
            Harmony harmony = new Harmony(GUID);
            ConfigToEdit.Bind();
            options = new OptionsMenu();
            ConfigToEdit.ParseConfig();
            harmony.PatchAll();
            WaitScreenHandler.RegisterLateLoadTask(MODNAME, task => LoadedGameSetup());
            WaitScreenHandler.RegisterEarlyLoadTask(MODNAME, task => StartLoadingSetup());
            OptionsPanelHandler.RegisterModOptions(options);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            CraftDataHandler.SetEatingSound(TechType.Coffee, "event:/player/drink");
            GetLoadedMods();
            if (ConfigToEdit.coralShellPlateGivesTableCoral.Value)
            {
                CraftDataHandler.SetHarvestOutput(TechType.CoralShellPlate, TechType.JeweledDiskPiece);
            }
            Application.runInBackground = MiscSettings.runInBackground;
            SaveLoadManager.notificationSaveInProgress += SaveData;
            Logger.LogInfo($"Plugin {MODNAME} {VERSION} is loaded ");
            //SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
        }

        private void CustomSpawns()
        {
            //CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-50f, -11f, -430f)));
            //new Spawnables.Stone().Patch();
            //CustomPrefab stone = new CustomPrefab("TF_Stone", "TF_Stone", "");
            //stone.SetSpawns(new SpawnLocation(new Vector3(0.67f, -14.11f, -323.3f), new Vector3(0f, 310f, 329f)));
            //stone.SetGameObject(new CloneTemplate(stone.Info, TechType.SeamothElectricalDefense);
        }

        void FixCoralShellPlateHarvestType()
        {
            if (TechData.Contains(TechType.CoralShellPlate) == false)
                return;

            JsonValue jv = TechData.entries[TechType.CoralShellPlate];
            jv.SetInt(TechData.PropertyToID("harvestType"), (int)HarvestType.DamageAlive);
            //jv.GetInt(id, out int value);
            //HarvestType harvestType = (HarvestType)value;
            //AddDebug("CoralShellPlate harvestType " + TechData.GetHarvestType(TechType.CoralShellPlate));
        }

        private void Start()
        {
            Setup();
        }

        public void GetLoadedMods()
        {
            visibleLockerInteriorLoaded = Chainloader.PluginInfos.ContainsKey("VisibleLockerInterior");
            baseLightSwitchLoaded = Chainloader.PluginInfos.ContainsKey("com.ahk1221.baselightswitch") || Chainloader.PluginInfos.ContainsKey("Cookie_BaseLightSwitch") || Chainloader.PluginInfos.ContainsKey("RealisticLightSwitch");
            pickupFullCarryallIsLoaded = Chainloader.PluginInfos.ContainsKey("PickupableStorageEnhanced");
            advancedInventoryLoaded = Chainloader.PluginInfos.ContainsKey("sn.advancedinventory.mod");
            flareRepairLoaded = Chainloader.PluginInfos.ContainsKey("com.remodor.rm_flarerepair");
            cyclopsDockingLoaded = Chainloader.PluginInfos.ContainsKey("com.osubmarin.cyclopsdockingmod");
            cyclopsOverheatLoaded = Chainloader.PluginInfos.ContainsKey("CyclopsOverheat");
            torpedoImprovementsLoaded = Chainloader.PluginInfos.ContainsKey("com.TorpedoImprovements.mod");
            aggressiveFaunaLoaded = Chainloader.PluginInfos.ContainsKey("com.lee23.aggressivefauna");
            //devMenuLoaded = Chainloader.PluginInfos.ContainsKey("aedenthorn.DeveloperMenu");
            //com.github.tinyhoot.DeathrunRemade
            //foreach (KeyValuePair<string, PluginInfo> plugin in Chainloader.PluginInfos)
            //    logger.LogInfo(plugin.Key + " loaded Mod " + plugin.Value.Metadata.Name);
        }

        private void StartLoadingSetup()
        {
            FixCraftDataTables();
            UWE.CoroutineHost.StartCoroutine(AuroraFixer.GetMaterialForDecals());
            if (PrefabFixer.prefabsFixed == false)
            {
                PrefabFixer prefabFixer = new PrefabFixer();
                prefabFixer.FixPrefabs();
            }
            if (BasePrefabFixer.basePrefabsFixed == false)
            {
                BasePrefabFixer basePrefabFixer = new BasePrefabFixer();
                UWE.CoroutineHost.StartCoroutine(basePrefabFixer.FixBasePrefabs());
            }
            if (AbandonedBaseFixer.abandonedBasesFixed == false)
            {
                AbandonedBaseFixer abandonedBaseFixer = new AbandonedBaseFixer();
                abandonedBaseFixer.FixAbandonedBases();
            }
            if (VehicleLightFix.fixed_ == false)
            {
                VehicleLightFix vehicleLightFix = new VehicleLightFix();
                UWE.CoroutineHost.StartCoroutine(vehicleLightFix.GetSeaMothVolLight());
                UWE.CoroutineHost.StartCoroutine(vehicleLightFix.FixExosuit());
                UWE.CoroutineHost.StartCoroutine(vehicleLightFix.FixSeamothLights());
                VehicleLightFix.fixed_ = true;
            }
            if (Constructor_.fixed_ == false)
            {
                Constructor_ constructor = new Constructor_();
                UWE.CoroutineHost.StartCoroutine(constructor.FixConstructor());
            }
            if (Util.IsGraphicsPresetHighDetail() && AlienFacilityPrefabFixer.alienFacilityPrefabsFixed == false)
            {
                AlienFacilityPrefabFixer alienFacilityPrefabFixer = new AlienFacilityPrefabFixer();
                alienFacilityPrefabFixer.FixAlienFacilityPrefabs();
            }
            Application.runInBackground = true;
        }

        private void FixCraftDataTables()
        {
            CraftData.PreparePrefabIDCache();
            // Adding to entClassTechTable fixes CraftData.GetTechType
            CraftData.entClassTechTable["853a9c5b-aba3-4d6b-a547-34553aa73fa9"] = TechType.DrillableKyanite;
            CraftData.entClassTechTable["4f441e53-7a9a-44dc-83a4-b1791dc88ffd"] = TechType.DrillableKyanite;
            //CraftData.entClassTechTable["18229b4b-3ed3-4b35-ae30-43b1c31a6d8d"] = TechType.BloodOil;

            // Adding to techMapping fixes CraftData.GetPrefabForTechTypeAsync
            CraftData.techMapping[TechType.DrillableKyanite] = "853a9c5b-aba3-4d6b-a547-34553aa73fa9";
            CraftData.techMapping[TechType.Cyclops] = "4f59199f-7049-4e13-9e57-5ee82c8732c5";
            //CraftData.techMapping[TechType.SpikePlant] = "84794dd0-2c70-4239-9536-230d56811ad4";
        }

        [HarmonyPatch(typeof(ApplicationFocus), "OnRunInBackgroundChanged")]
        class ApplicationFocus_OnRunInBackgroundChanged_Patch
        {
            public static void Postfix(ApplicationFocus __instance)
            {
                //AddDebug("OnRunInBackgroundChanged " + Application.runInBackground);
                Application.runInBackground = MiscSettings.runInBackground;
            }
        }


    }
}
