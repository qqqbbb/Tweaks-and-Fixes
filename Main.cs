
using HarmonyLib;
using System.Reflection;
using System;
using Nautilus.Handlers;
using Nautilus.Assets;
using Nautilus.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Bootstrap;
using static ErrorMessage;
using Nautilus.Assets.PrefabTemplates;
using static Tweaks_Fixes.Spawnables;
using Nautilus.Assets.Gadgets;

namespace Tweaks_Fixes
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {
        private const string
            MODNAME = "Tweaks and Fixes",
            GUID = "qqqbbb.subnautica.tweaksAndFixes",
            VERSION = "3.0.0";

        public static ManualLogSource logger;
        //public static GUIHand guiHand;
        public static Survival survival;
        public static bool canBreathe = false;
        public static bool loadingDone = false;  // WaitScreen.IsWaiting
        public static System.Random rndm = new System.Random();
        public static bool advancedInventoryLoaded = false;
        public static bool flareRepairLoaded = false; // not updated
        public static bool cyclopsDockingLoaded = false;
        public static bool vehicleLightsImprovedLoaded = false; // not updated
        public static bool languageCheck = false;
        public static bool pickupFullCarryallsLoaded = false;
        public static bool seaglideMapControlsLoaded = false;  // not updated
        public static bool baseLightSwitchLoaded = false;
        public static bool visibleLockerInteriorModLoaded;
        public static bool prawnSuitTorpedoDisplayLoaded = false; // not updated
        public static bool torpedoImprovementsLoaded = false; // not updated
        //public static bool refillOxygenTankLoaded = false;
        

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        public static void CleanUp()
        {
            loadingDone = false;
            canBreathe = false;
            QuickSlots_Patch.invChanged = true;
            Databox_Light_Patch.databoxLights = new List<GameObject>();
            Crush_Damage.extraCrushDepth = 0;
            Cyclops_Patch.ceh = null;
            Cyclops_Patch.collidersInSub = new HashSet<Collider>();
            Gravsphere_Patch.gasPods = new HashSet<GasPod>();
            Gravsphere_Patch.gravSphereFish = new HashSet<Pickupable>();
            Decoy_Patch.decoysToDestroy = new List<GameObject>();
            Vehicle_patch.currentLights = new Light[2];
            Vehicle_patch.dockedVehicles = new Dictionary<Vehicle, Vehicle.DockType>();
            Exosuit_Patch.exosuitStarted = false;
            Damage_Patch.healTempDamageTime = 0;
            Damage_Patch.tempDamageLMs = new List<LiveMixin>();
            Storage_Patch.savedSigns = new List<Sign>();
            Storage_Patch.labelledLockers = new List<StorageContainer>();
            Battery_Patch.subPowerRelays = new List<PowerRelay>();
            config.Load();
        }

        [HarmonyPatch(typeof(WaitScreen), "Remove")]
        class WaitScreen_Remove_Patch
        { 
            public static void Postfix(WaitScreen.IWaitItem item)
            { // __instance is null !
                if (WaitScreen.main.items.Count == 0)
                {
                    LoadedGameSetup();
                    logger.LogInfo("loaded game world");
                }
            }
        }

        public static void  LoadedGameSetup()
        {
            loadingDone = true;
            config.predatorExclusion.Remove(TechType.Crash);
            config.predatorExclusion.Remove(TechType.Mesmer);

            foreach (LiveMixin lm in Damage_Patch.tempDamageLMs)
            {
                //AddDebug("uGUI_SceneLoading End " + lm.tempDamage);
                lm.SyncUpdatingState();
            }
            //AddDebug("LoadedGameSetup activeSlot " + config.activeSlot);
            if (config.activeSlot != -1 && Player.main.mode == Player.Mode.Normal )
                Inventory.main.quickSlots.SelectImmediate(config.activeSlot);

            //if (EscapePod.main)
            //    Escape_Pod_Patch.EscapePod_OnProtoDeserialize_Patch.Postfix(EscapePod.main);

            languageCheck = Language.main.GetCurrentLanguage() == "English" || !config.translatableStrings[0].Equals("Burnt out ");
            if (languageCheck)
            {
                //LanguageHandler.SetLanguageLine("Tooltip_Bladderfish", "Unique outer membrane has potential as a natural water filter. Can also be used as a source of oxygen.");
                LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, config.translatableStrings[24]);
                LanguageHandler.SetTechTypeTooltip(TechType.SeamothElectricalDefense, config.translatableStrings[25]);
            }
            foreach (Pickupable p in Food_Patch.cookedFish)
            {
                if (p != null && p.inventoryItem == null)
                    Destroy(p.gameObject);
            }
            Food_Patch.cookedFish = new HashSet<Pickupable>();
            //Storage_Patch.savedSigns = new List<Sign>();
        }
        
        [HarmonyPatch(typeof(SaveLoadManager))]
        class SaveLoadManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch( "ClearSlotAsync")]
            public static void ClearSlotAsyncPostfix(SaveLoadManager __instance, string slotName)
            {
                //AddDebug("ClearSlotAsync " + slotName);
                config.escapePodSmokeOut.Remove(slotName);
                config.openedWreckDoors.Remove(slotName);
                config.lockerNames.Remove(slotName);
                config.baseLights.Remove(slotName);
                config.Save();
            }

            [HarmonyPostfix]
            [HarmonyPatch("CreateSlotAsync", new Type[0])]
            public static void CreateSlotAsyncPostfix(SaveLoadManager __instance)
            {
                //AddDebug("SaveLoadManager CreateSlotAsync ");
                config.pickedUpFireExt = false;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("LoadSlotsAsync", new Type[0])]
            public static void LoadSlotsAsyncPostfix(SaveLoadManager __instance)
            {

            }
        }

        [HarmonyPatch(typeof(IngameMenu), "SaveGame")]
        class IngameMenu_SaveGame_Patch
        {
            public static void Prefix(IngameMenu __instance)
            {
                //AddDebug("SaveGame ");
                config.Save();
                for (int i = Decoy_Patch.decoysToDestroy.Count - 1; i >= 0; i--)
                    Destroy(Decoy_Patch.decoysToDestroy[i]);
                //AddDebug("decoysToDestroy.Count " + Decoy_Patch.decoysToDestroy.Count);
            }
        }

        static void SaveData()
        {
            //AddDebug("SaveData " + Inventory.main.quickSlots.activeSlot);
            //Main.config.activeSlot = Inventory.main.quickSlots.activeSlot;
            config.activeSlot = Inventory.main.quickSlots.activeSlot;
            //config.crushDepth -= Crush_Damage.extraCrushDepth;
            //config.crushDepth += Crush_Damage.extraCrushDepth;
            for (int i = Decoy_Patch.decoysToDestroy.Count - 1; i >= 0; i--)
                Destroy(Decoy_Patch.decoysToDestroy[i]);

            config.Save();
        }

        public static void Setup()
        {
            SaveUtils.RegisterOnSaveEvent(SaveData);
            SaveUtils.RegisterOnQuitEvent(CleanUp);
            CraftDataHandler.SetEatingSound(TechType.Coffee, "event:/player/drink");
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

        private void Awake()
        {
            logger = Logger;
        }

        private void Start()
        {
            //BepInEx.Logging.Logger.CreateLogSource("Tweaks and fixes: ").Log(LogLevel.Error, " Awake ");
            //AddDebug("Mono Start ");
            //Logger.LogInfo("Mono Start");
            //config.Load();
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll();
            Setup();
            ParseFromConfig();
            GetLoadedMods();
        }

        public static void GetLoadedMods()
        {
            //logger.LogInfo("Chainloader.PluginInfos Count " + Chainloader.PluginInfos.Count);
            //AddDebug("Chainloader.PluginInfos Count " + Chainloader.PluginInfos.Count);
            foreach (var plugin in Chainloader.PluginInfos)
            {
                var metadata = plugin.Value.Metadata;
                //logger.LogInfo("loaded Mod " + metadata.GUID);
                if (metadata.GUID.Equals("VisibleLockerInterior"))
                    visibleLockerInteriorModLoaded = true;
                else if (metadata.GUID.Equals("PrawnSuitTorpedoDisplay"))
                    prawnSuitTorpedoDisplayLoaded = true;
                else if (metadata.GUID.Equals("com.ahk1221.baselightswitch"))
                    baseLightSwitchLoaded = true;
                else if (metadata.GUID.Equals("SeaglideMapControls"))
                    seaglideMapControlsLoaded = true;
                else if (metadata.GUID.Equals("PickupFullCarryalls"))
                    pickupFullCarryallsLoaded = true;
                else if (metadata.GUID.Equals("sn.advancedinventory.mod"))
                    advancedInventoryLoaded = true;
                else if (metadata.GUID.Equals("Rm_FlareRepair"))
                    flareRepairLoaded = true;
                else if (metadata.GUID.Equals("com.osubmarin.cyclopsdockingmod"))
                    cyclopsDockingLoaded = true;
                else if (metadata.GUID.Equals("Rm_VehicleLightsImproved"))
                    vehicleLightsImprovedLoaded = true;
                else if (metadata.GUID.Equals("TorpedoImprovements"))
                    torpedoImprovementsLoaded = true;
                //else if (metadata.GUID.Equals("sn.oxygentank.mod"))
                //    refillOxygenTankLoaded = true;
            }
        }
         
        public static void ParseFromConfig()
        {
            foreach (var item in config.crushDepthEquipment)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                //Log("crushDepthEquipment str " + item.Key);
                //Log("crushDepthEquipment TechType " + tt);
                if (tt != TechType.None)
                    Crush_Damage.crushDepthEquipment[tt] = item.Value;
            }
            foreach (var item in config.crushDamageEquipment)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                if (tt != TechType.None)
                    Crush_Damage.crushDamageEquipment[tt] = item.Value * .01f;
            }
            foreach (var item in config.itemMass)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                //Log("crushDepthEquipment str " + item.Key);
                //Log("crushDepthEquipment TechType " + tt);
                if (tt != TechType.None)
                    Pickupable_Patch.itemMass[tt] = item.Value;
            }
            foreach (var name in config.unmovableItems)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                if (tt != TechType.None)
                {
                    Pickupable_Patch.unmovableItems.Add(tt);
                    //Log("unmovableItems " + tt);
                }
            }
            foreach (string name in config.gravTrappable)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                if (tt != TechType.None)
                    Gravsphere_Patch.gravTrappable.Add(tt);
            }
            foreach (string name in config.silentCreatures)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                if (tt != TechType.None)
                    Creature_Tweaks.silentCreatures.Add(tt);
            }
            foreach (string name in config.stalkerPlayThings)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                if (tt != TechType.None)
                    Pickupable_Patch.shinies.Add(tt);
            }
            foreach (string name in config.removeLight)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                //Log("config.removeLight " + tt);
                if (tt != TechType.None)
                    LargeWorldEntity_Patch.removeLight.Add(tt);
            }
            foreach (var kv in config.damageMult_)
            {
                TechTypeExtensions.FromString(kv.Key, out TechType tt, true);
                if (tt != TechType.None)
                   Damage_Patch.damageMult.Add(tt, kv.Value);
            }
        }
    }
}
