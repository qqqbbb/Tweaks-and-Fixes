using Nautilus.Json;
using Nautilus.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Options;
using System.IO;
using BepInEx;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    public class ConfigMain : JsonFile 
    {
        public ConfigMain()
        {
            this.Load();
        }

        public override string JsonFilePath => Paths.ConfigPath + Path.DirectorySeparatorChar + Main.MODNAME + Path.DirectorySeparatorChar + "config.json";

        public Screen_Resolution_Fix.ScreenRes screenRes;
        public bool cyclopsFloodLights = false;
        public bool cyclopsLighting = false;
        public bool exosuitLights = false;
        public bool seaglideLights = false;
        public bool seaglideMap = false;
        public int subThrottleIndex = -1;
        public int activeSlot = -1;
        public bool escapePodSmokeOut;
        public HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {TechType.Salt, TechType.Quartz, TechType.AluminumOxide, TechType.Lithium , TechType.Sulphur, TechType.Diamond, TechType.Kyanite, TechType.Magnetite, TechType.Nickel, TechType.UraniniteCrystal  };
        public Dictionary<string, Dictionary<int, bool>> openedWreckDoors = new Dictionary<string, Dictionary<int, bool>>();
        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();
        public float medKitHPtoHeal = 0f;

        public enum DropItemsOnDeath { Vanilla, Drop_everything, Do_not_drop_anything }
        public enum EmptyVehiclesCanBeAttacked { Vanilla, Yes, No, Only_if_lights_on }
        public enum EatingRawFish { Vanilla, Harmless, Risky, Harmful }

        //public Dictionary<string, HashSet<string>> objectsSurvivedDespawn = new Dictionary<string, HashSet<string>> { };
        //public HashSet<string> objectsDespawned = new HashSet<string> { };
        //public List<string> removeLight = new List<string> { };
        //public List<string> biomesRemoveLight = new List<string> { };
        public HashSet<string> stalkerPlayThings = new HashSet<string> { "ScrapMetal", "MapRoomCamera", "Beacon", "Seaglide", "CyclopsDecoy", "Gravsphere", "SmallStorage", "FireExtinguisher", "DoubleTank", "PlasteelTank", "PrecursorKey_Blue", "PrecursorKey_Orange", "PrecursorKey_Purple", "PrecursorKey_Red", "PrecursorKey_White", "Rebreather", "Tank", "HighCapacityTank", "Flare", "Flashlight", "Builder", "LaserCutter", "LEDLight", "DiveReel", "PropulsionCannon", "Knife", "HeatBlade", "Scanner", "Welder", "RepulsionCannon", "StasisRifle" };
        public Dictionary<TechType, float> lightIntensity = new Dictionary<TechType, float>();
        public bool pickedUpFireExt = false;
        public Dictionary<string, Dictionary<string, bool>> baseLights = new Dictionary<string, Dictionary<string, bool>>();
        public Dictionary<string, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<string, Dictionary<TechType, int>> { { "Stalker", new Dictionary<TechType, int> { { TechType.StalkerTooth, 2 } } }, { "Gasopod", new Dictionary<TechType, int> { { TechType.GasPod, 5 } } } };

        //public bool LEDLightWorksInHand = true;
        //public int growingPlantUpdateInterval = 0;
        //public bool pdaTabSwitchHotkey = true;


    }

}