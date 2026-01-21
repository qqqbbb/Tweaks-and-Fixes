
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
            VERSION = "4.10.0";

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
        public static Survival survival;

        public static void CleanUp()
        {
            //logger.LogInfo("CleanUp");
            gameLoaded = false;
            QuickSlot_Cycle.invChanged = true;
            Databox_Light.databoxLights.Clear();
            Crush_Damage.extraCrushDepth = 0;
            Crush_Damage.crushDamageResistance = 0;
            Cyclops_Constructable_Collision.CleanUp();
            Geyser_.CleanUp();
            Gravsphere_.gasPods.Clear();
            Gravsphere_.gravSphereFish.Clear();
            Decoy_Patch.decoysToDestroy.Clear();
            Vehicle_patch.currentVehicleTT = TechType.None;
            Exosuit_Patch.exosuitStarted = false;
            Damage_.healTempDamageTime = 0;
            Storage_Patch.savedSigns.Clear();
            Storage_Patch.labelledLockers.Clear();
            PowerConsumption.subPowerRelays.Clear();
            Creatures.pickupShinies.Clear();
            Base_.CleanUp();
            CreatureDeath_.creatureDeathsToDestroy.Clear();
            Drop_items_anywhere.droppedInBase.Clear();
            Drop_items_anywhere.droppedInEscapePod.Clear();
            Player_.healTime = 0;
            Poison_Damage.ResetVars();
            Cyclops_unpowered.poweredSubs.Clear();
            Pickupable_.beacons.Clear();
            Pickupable_.pickupableStorage.Clear();
            Pickupable_.pickupableStorage_.Clear();
            Precurcor_Patch.used.Clear();
            InventoryItemIconColorChanger.CleanUp();
            configMain.Load();
        }

        public static void LoadedGameSetup()
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
            IteratePrefabs();
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
            gameLoaded = true;
        }

        private static void IteratePrefabs()
        {
            if (ConfigToEdit.bloodColor.Value == "0.784 1.0 0.157")
            {
                //logger.LogDebug("bloodColor is default ");
                return;
            }
            foreach (GameObject go in Util.FindAllRootGameObjects())
            {
                if (go.name == "xKnifeHit_Organic" || go.name == "GenericCreatureHit" || go.name == "xExoDrill_Organic")
                {
                    Util.SetBloodColor(go);
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "Start")]
        class uGUI_MainMenu_Start_Patch
        {
            static void Postfix(uGUI_MainMenu __instance)
            {
            }
        }

        //[HarmonyPatch(typeof(GameInput), "Initialize")]
        class GameInput_Initialize_Patch
        {
            static void Postfix()
            { // Gameinput.PrimaryDevice is null when this runs in Start 
                //logger.LogDebug("GameInput Initialize");

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
            InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
            if (heldItem != null)
            {
                PlaceTool pt = heldItem.item.GetComponent<PlaceTool>();
                if (pt)
                {
                    //AddDebug(" heldItem PlaceTool");
                    configMain.activeSlot = -1;
                }
            }
            Decoy_Patch.DestroyDecoys();
            configMain.Save();
            //AddDebug("Save configMain " + Inventory.main.quickSlots.activeSlot);
        }

        public void Setup()
        {
            configMenu = new ConfigFile(configMenuPath, false);
            ConfigMenu.Bind();
            logger = Logger;
            configToEdit = new ConfigFile(configToEditPath, false);
            ConfigToEdit.Bind();
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
            options = new OptionsMenu();
            WaitScreenHandler.RegisterLateLoadTask(MODNAME, task => LoadedGameSetup());
            WaitScreenHandler.RegisterEarlyLoadTask(MODNAME, task => AddTechTypesToClassIDtable());
            //WaitScreenHandler.RegisterEarlyLoadTask(MODNAME, task => StartCoroutine(BeforeLoading(TechType.Seaglide)));

            OptionsPanelHandler.RegisterModOptions(options);
            //SaveUtils.RegisterOnSaveEvent(TestSave);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            CraftDataHandler.SetEatingSound(TechType.Coffee, "event:/player/drink");
            LanguageHandler.RegisterLocalizationFolder();
            GetLoadedMods();
            ConfigToEdit.ParseConfig();
            configMain.Load();
            if (ConfigToEdit.coralShellPlateGivesTableCoral.Value)
            {
                CraftDataHandler.SetHarvestOutput(TechType.CoralShellPlate, TechType.JeweledDiskPiece);
            }
            //CustomSpawns();
            Logger.LogInfo($"Plugin {GUID} {VERSION} is loaded ");
            //SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
            //WaitScreenHandler.RegisterLateLoadTask(MODNAME, task => LateLoad());
            //WaitScreenHandler.RegisterLoadTask(MODNAME, task => Load());
            //WaitScreenHandler.RegisterEarlyLoadTask(MODNAME, task => EarlyLoad());
        }

        private static void CustomSpawns()
        {
            //CoordinatedSpawnsHandler.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-50f, -11f, -430f)));
            //new Spawnables.Stone().Patch();
            //CustomPrefab stone = new CustomPrefab("TF_Stone", "TF_Stone", "");
            //stone.SetSpawns(new SpawnLocation(new Vector3(0.67f, -14.11f, -323.3f), new Vector3(0f, 310f, 329f)));
            //stone.SetGameObject(new CloneTemplate(stone.Info, TechType.SeamothElectricalDefense);
        }

        public static IEnumerator BeforeLoading(TechType techType)
        {
            logger.LogDebug("BeforeLoading " + techType);
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType);
            yield return request;
            logger.LogDebug("BeforeLoading !!! " + techType);
        }

        static void FixCoralShellPlateHarvestType()
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

        public static void GetLoadedMods()
        {
            visibleLockerInteriorLoaded = Chainloader.PluginInfos.ContainsKey("VisibleLockerInterior");
            baseLightSwitchLoaded = Chainloader.PluginInfos.ContainsKey("com.ahk1221.baselightswitch") || Chainloader.PluginInfos.ContainsKey("Cookie_BaseLightSwitch");
            pickupFullCarryallIsLoaded = Chainloader.PluginInfos.ContainsKey("PickupableStorageEnhanced");
            advancedInventoryLoaded = Chainloader.PluginInfos.ContainsKey("sn.advancedinventory.mod");
            flareRepairLoaded = Chainloader.PluginInfos.ContainsKey("com.remodor.rm_flarerepair");
            cyclopsDockingLoaded = Chainloader.PluginInfos.ContainsKey("com.osubmarin.cyclopsdockingmod");
            cyclopsOverheatLoaded = Chainloader.PluginInfos.ContainsKey("CyclopsOverheat");
            torpedoImprovementsLoaded = Chainloader.PluginInfos.ContainsKey("com.TorpedoImprovements.mod");
            aggressiveFaunaLoaded = Chainloader.PluginInfos.ContainsKey("com.lee23.aggressivefauna");
            //com.github.tinyhoot.DeathrunRemade
            //foreach (KeyValuePair<string, PluginInfo> plugin in Chainloader.PluginInfos)
            //    logger.LogInfo(plugin.Key + " loaded Mod " + plugin.Value.Metadata.GUID);
        }

        private static void AddTechTypesToClassIDtable()
        {
            CraftData.PreparePrefabIDCache();
            CraftData.entClassTechTable["769f9f44-30f6-46ed-aaf6-fbba358e1676"] = TechType.BaseBioReactor;
            CraftData.entClassTechTable["864f7780-a4c3-4bf2-b9c7-f4296388b70f"] = TechType.BaseNuclearReactor;
            CraftData.entClassTechTable["4f59199f-7049-4e13-9e57-5ee82c8732c5"] = TechType.Cyclops;


        }


    }
}
