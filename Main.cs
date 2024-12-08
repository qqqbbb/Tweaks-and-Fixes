
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
using System.Text;
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
            VERSION = "3.13.1";

        public static ManualLogSource logger;
        public static Survival survival;
        public static bool gameLoaded = false;  // WaitScreen.IsWaiting
        public static System.Random rndm = new System.Random();
        public static bool advancedInventoryLoaded = false;
        public static bool flareRepairLoaded = false; // not updated
        public static bool cyclopsDockingLoaded = false;
        public static bool vehicleLightsImprovedLoaded = false; // not updated
        public static bool pickupFullCarryallsLoaded = false;  // not updated
        public static bool seaglideMapControlsLoaded = false;  // not updated
        public static bool baseLightSwitchLoaded = false;
        public static bool visibleLockerInteriorLoaded;
        public static bool exosuitTorpedoDisplayLoaded = false; // not updated
        public static bool torpedoImprovementsLoaded = false;
        static string configToEditPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MODNAME + Path.DirectorySeparatorChar + "ConfigToEdit.cfg";
        static string configMenuPath = Paths.ConfigPath + Path.DirectorySeparatorChar + MODNAME + Path.DirectorySeparatorChar + "ConfigMenu.cfg";
        public const float dayLengthSeconds = 1200f;
        public static ConfigMain configMain = new ConfigMain();
        internal static OptionsMenu options;
        public static ConfigFile configMenu;
        public static ConfigFile configToEdit;

        public static void CleanUp()
        {
            //logger.LogInfo("CleanUp");
            gameLoaded = false;
            QuickSlots_Patch.invChanged = true;
            Databox_Light_Patch.databoxLights.Clear();
            Crush_Damage.extraCrushDepth = 0;
            Crush_Damage.crushDamageResistance = 0;
            Cyclops_Patch.ceh = null;
            Cyclops_Patch.collidersInSub.Clear();
            Geyser_Patch.eruptionForce.Clear();
            Geyser_Patch.rotationForce.Clear();
            Gravsphere_Patch.gasPods.Clear();
            Gravsphere_Patch.gravSphereFish.Clear();
            Decoy_Patch.decoysToDestroy.Clear();
            Vehicle_patch.currentLights = new Light[2];
            Vehicle_patch.dockedVehicles.Clear();
            Exosuit_Patch.exosuitStarted = false;
            Damage_Patch.healTempDamageTime = 0;
            Damage_Patch.tempDamageLMs.Clear();
            Storage_Patch.savedSigns.Clear();
            Storage_Patch.labelledLockers.Clear();
            Battery_Patch.subPowerRelays.Clear();
            Coffee_Patch.spawnedCoffeeTime.Clear();
            UI_Patches.planters.Clear();
            Creature_Tweaks.pickupShinies.Clear();
            Base_Patch.baseHullStrengths.Clear();
            CreatureDeath_Patch.creatureDeathsToDestroy.Clear();
            Drop_items_anywhere.droppedInBase.Clear();
            Drop_items_anywhere.droppedInEscapePod.Clear();

            configMain.Load();
        }

        public static void LoadedGameSetup()
        {
            if (ConfigToEdit.cantScanExosuitClawArm.Value)
                Player_Patches.DisableExosuitClawArmScan();

            if (ConfigToEdit.fixMelons.Value)
                CraftData.itemSizes[TechType.MelonPlant] = new Vector2int(2, 2);

            if (PDAScanner.mapping.ContainsKey(TechType.Creepvine))
            { // unlock fibermesh by scanning creepvine
                PDAScanner.mapping[TechType.Creepvine].blueprint = TechType.FiberMesh;
            }
            if (ConfigToEdit.bloodColor.Value.x != 0.784f || ConfigToEdit.bloodColor.Value.y != 1f || ConfigToEdit.bloodColor.Value.z != 0.157f)
            {
                Damage_Patch.SetBloodColor();
            }
            foreach (LiveMixin lm in Damage_Patch.tempDamageLMs)
            {
                //AddDebug("uGUI_SceneLoading End " + lm.tempDamage);
                lm.SyncUpdatingState();
            }
            //AddDebug("LoadedGameSetup activeSlot " + config.activeSlot);
            if (configMain.activeSlot != -1 && Player.main.mode == Player.Mode.Normal)
                Inventory.main.quickSlots.SelectImmediate(configMain.activeSlot);

            LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, Language.main.Get("Tooltip_Bladderfish") + Language.main.Get("TF_bladderfish_tooltip"));
            //LanguageHandler.SetTechTypeTooltip(TechType.SeamothElectricalDefense, Language.main.Get("TF_bladderfish_tooltip")); 

            foreach (Pickupable p in Food_Patch.cookedFish)
            { // remove dead fish from geysers
                if (p != null && p.inventoryItem == null)
                    Destroy(p.gameObject);
            }
            Food_Patch.cookedFish.Clear();
            Player.main.isUnderwater.changedEvent.AddHandler(Player.main, new UWE.Event<Utils.MonitoredValue<bool>>.HandleFunction(Knife_Patch.OnPlayerUnderwaterChanged));
            //AddDebug("LoadedGameSetup ");
            //logger.LogMessage("LoadedGameSetup ");
            CreatureDeath_Patch.TryRemoveCorpses();
            Escape_Pod_Patch.EscapePodInit();
            Drop_items_anywhere.OnGameLoadingFinished();
            gameLoaded = true;
        }

        [HarmonyPatch(typeof(MainMenuLoadButton), "Delete")]
        class MainMenuLoadButton_Delete_Patch
        {
            static void Postfix(MainMenuLoadButton __instance)
            {
                //AddDebug("MainMenuLoadButton Delete " + __instance.saveGame);
                DeleteSaveSlotData(__instance.saveGame);
            }
        }

        public static void DeleteSaveSlotData(string slotName)
        {
            //AddDebug("DeleteSaveSlotData " + slotName);
            configMain.openedWreckDoors.Remove(slotName);
            configMain.lockerNames.Remove(slotName);
            configMain.baseLights.Remove(slotName);
            configMain.cyclopsDoors.Remove(slotName);
            //config.objectsSurvidedDespawn.Remove(slotName);
            configMain.escapePodSmokeOut.Remove(slotName);
            configMain.pickedUpFireExt.Remove(slotName);
            configMain.Save();
        }

        [HarmonyPatch(typeof(SaveLoadManager))]
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
            //[HarmonyPrefix]
            //[HarmonyPatch("SaveToDeepStorageAsync", new Type[0])]
            public static void SaveToDeepStorageAsyncprefix(SaveLoadManager __instance)
            { // runs after nautilus SaveEvent
                //AddDebug("SaveToDeepStorageAsync");
                //Drop_items_anywhere.SavedroppedInBase();
            }
            [HarmonyPostfix]
            [HarmonyPatch("SaveToDeepStorageAsync", new Type[0])]
            public static void SaveToDeepStorageAsyncpostfix(SaveLoadManager __instance)
            { // runs after nautilus SaveEvent
                //AddDebug("SaveToDeepStorageAsync");
                SaveData();
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("LoadSlotsAsync", new Type[0])]
            public static void LoadSlotsAsyncPostfix(SaveLoadManager __instance)
            {

            }
        }



        static void SaveData()
        {
            configMain.screenRes = new Screen_Resolution_Fix.ScreenRes(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen);
            configMain.activeSlot = Inventory.main.quickSlots.activeSlot;
            Decoy_Patch.DestroyDecoys();
            configMain.Save();
            //AddDebug("Save configMain " + Inventory.main.quickSlots.activeSlot);
        }

        public void Setup()
        {
            //Logger.LogDebug("configOld activeSlot " + config.activeSlot);
            //configMenu = this.Config;
            configMenu = new ConfigFile(configMenuPath, false);
            ConfigMenu.Bind();
            logger = Logger;
            configToEdit = new ConfigFile(configToEditPath, false);
            ConfigToEdit.Bind();
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
            CraftData.harvestOutputList[TechType.CoralShellPlate] = TechType.JeweledDiskPiece;
            SaveUtils.RegisterOnFinishLoadingEvent(LoadedGameSetup);
            //SaveUtils.RegisterOnSaveEvent(TestSave);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            CraftDataHandler.SetEatingSound(TechType.Coffee, "event:/player/drink");
            LanguageHandler.RegisterLocalizationFolder();
            GetLoadedMods();
            ConfigToEdit.ParseFromConfig();
            options = new OptionsMenu();
            OptionsPanelHandler.RegisterModOptions(options);
            AddTechTypesToClassIDtable();
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-50f, -11f, -430f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(348.3f, -25.3f, -205.1f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-637f, -110.5f, -49.2f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-185f, -42f, 138.5f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-63.85f, -16f, -223f)));
            //new Spawnables.Stone().Patch();
            //CustomPrefab stone = new CustomPrefab( "TF_Stone", "TF_Stone", "");
            //stone.SetSpawns(new SpawnLocation(new Vector3(0.67f, -14.11f, -323.3f), new Vector3(0f, 310f, 329f)));
            //stone.SetGameObject(new CloneTemplate(stone.Info, TechType.SeamothElectricalDefense);
        }

        private void Start()
        {
            //BepInEx.Logging.Logger.CreateLogSource("Tweaks and fixes: ").Log(LogLevel.Error, " Awake ");
            //AddDebug("Mono Start ");
            //Logger.LogInfo("Mono Start");
            Setup();
        }

        public static void GetLoadedMods()
        {
            //logger.LogInfo("Chainloader.PluginInfos Count " + Chainloader.PluginInfos.Count);
            //AddDebug("Chainloader.PluginInfos Count " + Chainloader.PluginInfos.Count);
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                //logger.LogInfo("loaded Mod " + metadata.GUID);
                if (metadata.GUID == "VisibleLockerInterior")
                    visibleLockerInteriorLoaded = true;
                //else if (metadata.GUID.Equals("PrawnSuitTorpedoDisplay"))
                //    exosuitTorpedoDisplayLoaded = true;
                else if (metadata.GUID == "com.ahk1221.baselightswitch")
                    baseLightSwitchLoaded = true;
                //else if (metadata.GUID.Equals("SeaglideMapControls"))
                //    seaglideMapControlsLoaded = true;
                else if (metadata.GUID == "PickupableStorageEnhanced")
                    pickupFullCarryallsLoaded = true;
                else if (metadata.GUID == "sn.advancedinventory.mod")
                    advancedInventoryLoaded = true;
                //else if (metadata.GUID.Equals("Rm_FlareRepair"))
                //    flareRepairLoaded = true;
                else if (metadata.GUID == "com.osubmarin.cyclopsdockingmod")
                    cyclopsDockingLoaded = true;
                //else if (metadata.GUID.Equals("Rm_VehicleLightsImproved"))
                //    vehicleLightsImprovedLoaded = true;
                else if (metadata.GUID == "com.TorpedoImprovements.mod")
                    torpedoImprovementsLoaded = true;
                //else if (metadata.GUID.Equals("sn.oxygentank.mod"))
                //    refillOxygenTankLoaded = true;
            }
        }

        private static void AddTechTypesToClassIDtable()
        {
            CraftData.entClassTechTable["769f9f44-30f6-46ed-aaf6-fbba358e1676"] = TechType.BaseBioReactor;
            CraftData.entClassTechTable["864f7780-a4c3-4bf2-b9c7-f4296388b70f"] = TechType.BaseNuclearReactor;
            CraftData.entClassTechTable["4f59199f-7049-4e13-9e57-5ee82c8732c5"] = TechType.Cyclops;


        }

        public static void ParseFromConfig()
        {
            //foreach (string name in config.removeLight)
            //{
            //    TechTypeExtensions.FromString(name, out TechType tt, true);
            //    //Log("config.removeLight " + tt);
            //    if (tt != TechType.None)
            //        LargeWorldEntity_Patch.removeLight.Add(tt);
            //}
            //foreach (var kv in config.damageMult_)
            //{
            //    TechTypeExtensions.FromString(kv.Key, out TechType tt, true);
            //    if (tt != TechType.None)
            //       Damage_Patch.damageMult.Add(tt, kv.Value);
            //}
        }
    }
}
