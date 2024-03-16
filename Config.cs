using Nautilus.Json;
using Nautilus.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Options;
using System.ComponentModel;

namespace Tweaks_Fixes
{
    [Menu("Tweaks and Fixes")]
    public class Config : ConfigFile
    {
        [Slider("Day/night cycle speed multiplier", 0.1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "The higher the value the shorter days are."), OnChange(nameof(UpdateGameSpeed))]
        public float dayCycleSpeed = 1f;
        [Slider("Player speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Your swimming, walking and running speed will be multiplied by this.")]
        public float playerSpeedMult = 1f;
        [Slider("Player damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage player takes will be multiplied by this.")]
        public float playerDamageMult = 1f;
        [Slider("Vehicle damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage your vehicles take will be multiplied by this.")]
        public float vehicleDamageMult = 1f;
        //[Slider("Damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "When anything but the player or vehicles takes damage, it will be multiplied by this.")]
        //public float damageMult = 1f;
        [Slider("Predator aggression multiplier", 0f, 2f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "The higher it is the more aggressive predators are towards you. When it's 0 you and your vehicles will never be attacked. When it's more than 1 predators attack you from greater distance and more often.")]
        public float aggrMult = 1f;
        [Slider("Oxygen per breath", 0f, 6f, DefaultValue = 3f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Amount of oxygen you consume every breath.")]
        public float oxygenPerBreath = 3f;
        [Slider("Tool power consumption multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of power consumed by your tools will be multiplied by this.")]
        public float toolEnergyConsMult = 1f;
        [Slider("Vehicle power consumption multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of power consumed by your vehicles will be multiplied by this.")]
        public float vehicleEnergyConsMult = 1f;
        [Slider("Base power consumption multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of power consumed by things in your base will be multiplied by this. Leave this at 1 if using EasyCraft mod.")]
        public float baseEnergyConsMult = 1f;
        [Slider("First aid kit HP", 10, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "HP restored by using first aid kit.")]
        public int medKitHP = 50;


        [Slider("Crafting time multiplier", 0.1f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Crafting time will be multiplied by this when crafting things with fabricator or modification station.")]
        public float craftTimeMult = 1f;
        [Slider("Building time multiplier", 0.1f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Building time will be multiplied by this when using builder tool.")]
        public float buildTimeMult = 1f;

        [Toggle("Player movement tweaks", Tooltip = "Player swimming speed is reduced to 70%. Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 5% on land and in water. Fins reduce your speed by 10% on land. Lightweight high capacity tank reduces your speed by 5% on land. Every other tank reduces your speed by 10% on land and by 5% in water. No speed reduction when near wrecks. You can sprint only if moving forward. Seaglide works only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%. Game has to be reloaded after changing this.")]
        public bool playerMoveTweaks = false;
        [Slider("Inventory weight multiplier in water", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are swimming you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultWater = 0f;
        [Slider("Inventory weight multiplier on land", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are on land you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultLand = 0f;
        [Toggle("Seamoth movement tweaks", Tooltip = "Seamoth does not exceed its max speed and does not consume more power when moving diagonally. Its vertical and sideways speed is halved. Its backward speed is reduced to 25%. Game has to be reloaded after changing this.")]
        public bool seamothMoveTweaks = false;
        [Toggle("Prawn suit movement tweaks", Tooltip = "Prawn suit can not move sideways. No time limit when using thrusters, but they consume twice more power than walking. Game has to be reloaded after changing this.")]
        public bool exosuitMoveTweaks = false;
        [Toggle("Cyclops movement tweaks", Tooltip = "Cyclops does not exceed its max speed and does not consume more power when moving diagonally. Its vertical and backward speed is halved.")]
        public bool cyclopsMoveTweaks = false;
        [Slider("Cyclops engine room fire chance percent", 0, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "The game starts checking this after you get your first engine overheat warning. Every 10 seconds chance to catch fire goes up by 10% if you don't slow down.")]
        public int cyclopsFireChance = 50;
        [Slider("Cyclops auto-repair threshold", 0, 100, DefaultValue = 90, Step = 1, Format = "{0:F0}", Tooltip = "Cyclops auto-repairs when it's not on fire and its HP percent is above this.")]
        public int cyclopsAutoHealHealthPercent = 90;
        [Slider("Crush depth", 50, 500, DefaultValue = 200, Step = 10, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if crush damage multiplier is 0.")]
        public int crushDepth = 200;
        [Slider("Crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 every 3 seconds player takes 1 damage multiplied by this for every meter below crush depth.")]
        public float crushDamageMult = 0f;
        [Slider("Vehicle crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 every 3 seconds vehicles take 1 damage multiplied by this for every meter below crush depth.")]
        public float vehicleCrushDamageMult = 0f;
        [Choice("Unmanned vehicles can be attacked", Tooltip = "By default unmanned seamoth or prawn suit can be attacked but cyclops can not.")]
        public EmptyVehiclesCanBeAttacked emptyVehiclesCanBeAttacked;
        //[Choice("Unmanned seamoth or prawn suit can be attacked", Tooltip = "By default unmanned seamoth or prawn suit can be attacked.")]
        //public EmptySeamothCanBeAttacked emptySeamothCanBeAttacked;
        //[Choice("Unmanned cyclops can be attacked", Tooltip = "By default unmanned cyclops can not be attacked.")]
        //public EmptyCyclopsCanBeAttacked emptyCyclopsCanBeAttacked;

        [Slider("Hunger update interval", 1, 100, DefaultValue = 10, Step = 1, Format = "{0:F0}", Tooltip = "Time in seconds it takes your hunger and thirst to update.")]
        public int hungerUpdateInterval = 10;
        [Toggle("New hunger system", Tooltip = "You don't regenerate health when you are full. When you sprint you get hungry and thirsty twice as fast. You don't lose health when your food or water value is 0. Your food and water values can go as low as -100. When your food or water value is below 0 your movement speed will be reduced proportionally to that value. When either your food or water value is -100 your movement speed will be reduced by 50% and you will start taking hunger damage. Your max food and max water value is 200. The higher your food value above 100 is the less food you get when eating: when your food value is 110 you lose 10% of food, when it's 190 you lose 90%.")]
        public bool newHungerSystem = false;

        [Choice("Eating raw fish", Tooltip = "When it's not vanilla, amount of food you get by eating raw fish changes. Harmless: it's a random number between 0 and fish's food value. Risky: it's a random number between fish's negative food value and fish's food value. Harmful: it's a random number between fish's negative food value and 0.")]
        public EatingRawFish eatRawFish;
        [Toggle("Food tweaks", Tooltip = "Raw fish water value is half of its food value. Cooked rotten fish has no food value. Game has to be reloaded after changing this.")]
        public bool foodTweaks = false;
        [Toggle("Thermoblade cooks fish on kill", Tooltip = "")]
        public bool heatBladeCooks = true;
        [Toggle("New poison damage system", Tooltip = "Every 2 seconds poison will deal 1 point of permanent damage and decrease your food and water values by 1. Using first aid kit will remove poison from your system.")]
        public bool newPoisonSystem = false;
        [Slider("Fruit growth time", 0, 30, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a lantern tree fruit, creepvine seeds and blood oil to grow. If it's 0 then vanilla code will run. You have to reload your game after changing this.")]
        public int fruitGrowTime = 0;
        [Toggle("Can't eat underwater", Tooltip = "If enabled you will not be able to eat or drink when swimming underwater.")]
        public bool cantEatUnderwater = false;
        [Toggle("Can't use first aid kit underwater", Tooltip = "If enabled you will not be able to use first aid kit when swimming underwater.")]
        public bool cantUseMedkitUnderwater = false;
        //[Toggle("Eat fish on release", Tooltip = "Eat the fish you are holding in your hand when you press the 'right hand' button.")]
        //public bool eatFishOnRelease = false;
        [Slider("Food decay rate multiplier", 0.1f, 3f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "Food decay rate will be multiplied by this.")]
        public float foodDecayRateMult = 1f;
        [Slider("Catchable fish speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Swimming speed of fish that you can catch will be multiplied by this.")]
        public float fishSpeedMult = 1f;
        [Slider("Other creatures speed multiplier", .1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Swimming speed of creatures that you can't catch will be multiplied by this.")]
        public float creatureSpeedMult = 1f;

        [Slider("Creature flee chance percent", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Creature flee chance percent when it's under attack and its flee damage threshold is reached.")]
        public int CreatureFleeChance = 100;
        [Toggle("Creature flee chance percent depends on its health", Tooltip = "Only creatures's health will be used to decide if it should flee when under attack. Creature with 90% health has 10% chance to flee. Creature with 10% health has 90% chance to flee. This setting overrides both \"Creature flee chance percent\" and CreatureFleeUseDamageThreshold.")]
        public bool CreatureFleeChanceBasedOnHealth = false;
        public bool CreatureFleeUseDamageThreshold = true;

        [Toggle("Creatures im alien comtainment can breed", Tooltip = "")]
        public bool waterparkCreaturesBreed = true;

        [Slider("Knife range multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeRangeMult = 1f;
        [Slider("Knife damage multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeDamageMult = 1f;
        //[Toggle("Stasis rifle tweaks", Tooltip = "Only creatures smaller than the stasis orb will be frozen. Player is not immune to stasis orb.")]
        //public bool stasisRifleTweak = false;
        [Toggle("Can't catch fish with bare hands", Tooltip = "To catch fish you will have to use propulsion cannon or grav trap. Does not apply if you are inside alien containment.")]
        public bool noFishCatching = false;
        [Toggle("Can't break outcrop with bare hands", Tooltip = "You will have to use a knife to break outcrops or collect resources attached to rock or seabed.")]
        public bool noBreakingWithHand = false;
        [Toggle("Player damage impact screen effects", Tooltip = "This toggles cracks on your swimming mask when you take damage.")]
        public bool damageImpactEffect = true;
        [Toggle("Player damage shader screen effects", Tooltip = "This toggles shader screen effects when you take damage.")]
        public bool damageScreenFX = true;
        [Toggle("Drop tool in your hands when taking damage", Tooltip = "Chance to drop your tool is proportional to amount of damage taken. If you take 30 damage, there is 30% chance you will drop your tool.")]
        public bool dropHeldTool = false;
        [Toggle("Stalkers grab tools from player's hands when playing", Tooltip = "Stalkers can grab only things that are in the mod config's 'stalkerPlayThings' list.")]
        public bool stalkersGrabShinyTool = false;
        [Slider("Stalker losing tooth probability percent", 0, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "Probability percent of a stalker losing its tooth when biting something hard.")]
        public int stalkerLoseTooth = 50;
        [Toggle("Disable tutorial messages", Tooltip = "Disable messages that tell you to 'eat something', 'break limestone', etc. Game has to be reloaded after changing this.")]
        public bool disableHints = false;
        [Toggle("Realistic oxygen consumption", Tooltip = "Vanilla oxygen consumption without rebreather has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth between 100 and 0 meters. With this on your oxygen consumption will increase in linear progression using 'Crush depth' setting. When you are at crush depth it will be vanilla max oxygen consumption and will increase as you dive deeper.")]
        public bool realOxygenCons = false;
        //[Slider("brainCoralBubbleInterval", 1, 20, DefaultValue = 3, Step = 1, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if crush damage multiplier is 0.")]
        //public int brainCoralBubbleInterval = 3;
        //[Toggle("Predators less likely to flee", Tooltip = "Predators don't flee when their health is above 50%. When it's not, chance to flee is proportional to their health. The more health they have the less likely they are to flee.")]
        //public bool predatorsDontFlee = false;
        //[Choice("Creatures respawn if killed by player", Tooltip = "By default big creatures and leviathans never respawn if killed by player.")]
        //public CreatureRespawn creatureRespawn;
        //public bool creaturesRespawn = false;
        [Slider("Fish respawn time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes small fish to respawn after it was killed or caught. If it's 0, default value of 6 hours will be used. Game has to be reloaded after changing this.")]
        public int fishRespawnTime = 0;
        [Slider("Big creature respawn time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a creature that you can't catch to respawn after it was killed. If it's 0, default value of 12 hours will be used. Game has to be reloaded after changing this.")]
        public int creatureRespawnTime = 0;
        [Slider("Leviathan respawn time", 0, 50, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Time in days it takes a leviathan to respawn after it was killed. If it's 0, default value of 1 day will be used. Game has to be reloaded after changing this.")]
        public int leviathanRespawnTime = 0;
        [Toggle("Do not spawn fragments for unlocked blueprints", Tooltip = "")]
        public bool dontSpawnKnownFragments = false;
        [Toggle("Unlock prawn suit only by scanning prawn suit", Tooltip = "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arms. Game has to be reloaded after changing this.")]
        public bool cantScanExosuitClawArm = false;
        [Toggle("Camera bobbing", Tooltip = "Camera bobbing when swimming.")]
        public bool cameraBobbing = true;
        //[Toggle("Free camera drones for scanner room", Tooltip = "If disabled scanner room will be built without camera drones.")]
        public bool mapRoomFreeCameras = true;
        //[Slider("flare light intensity", 0.1f, 1f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "You have to reequip your flare after changing this.")]
        //public float flareIntensity = .5f;
        [Slider("Free torpedos", 0, 6, DefaultValue = 2, Step = 1, Format = "{0:F0}", Tooltip = "Number of torpedos you get when installing Torpedo System or Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo System.")]
        public int freeTorpedos = 2;
        //[Toggle("Seamoth and prawn suit can use creature decoy", Tooltip = "After changing this you have to restart the game.")]
        //public bool seamothDecoy = false;
        [Slider("Creature decoy battery life time", 10, 500, DefaultValue = 90, Step = 10, Format = "{0:F0}", Tooltip = "Creature decoy stops working after this number of seconds.")]
        public int decoyLifeTime = 90;
        [Slider("Creature decoy HP", 0, 500, DefaultValue = 0, Step = 10, Format = "{0:F0}", Tooltip = "When it's not 0 creature decoy will be destroyed after taking this amount of damage. By default its HP is infinite.")]
        public int decoyHP = 0;
        [Toggle("Creature decoy does not work when dropped from inventory", Tooltip = "")]
        public bool decoyRequiresSub = false;
        [Slider("Life pod power cell max charge", 10, 100, DefaultValue = 25, Step = 1, Format = "{0:F0}", Tooltip = "Max charge for each of its 3 power cells. Game has to be reloaded after changing this.")]
        public int escapePodMaxPower = 25;
        [Toggle("Life pod power tweaks", Tooltip = "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod's power cells are not charged. Game has to be reloaded after changing this.")]
        public bool escapePodPowerTweak = false;
        [Slider("Battery charge multiplier", 0.5f, 2f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Max charge of batteries and power cells will be multiplied by this. Game has to be reloaded after changing this.")]
        public float batteryChargeMult = 1f;
        [Slider("Crafted battery charge percent", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Charge percent of batteries and power cells you craft will be set to this.")]
        public int craftedBatteryCharge = 100;
        [Slider("Mushroom damage chance percent", 0, 100, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "Chance of a mushroom dealing damage to player when picked up and dealing area damage when destroyed.")]
        public int shroomDamageChance = 0;
        [Choice("Drop items when you die", Tooltip = "")]
        public DropItemsOnDeath dropItemsOnDeath;
        [Toggle("No particles when creature dies", Tooltip = "No particles (yellow cloud) will spawn when a creature dies. Game has to be reloaded after changing this.")]
        public bool noKillParticles = false;
        [Choice("Outcrops from seatreaders", Tooltip = "")]
        public SeaTreaderOutcrop seaTreaderOutcrop;
        //[Toggle("No metal clicking sound when walking", Tooltip = "Removes metal clicking sound when walking on metal surface.")]
        public bool fixFootstepSound = true;
        //[Toggle("Turn off lights in your base"), OnChange(nameof(UpdateBaseLight))]
        //public bool baseLightOff = false;
        //[Toggle("Fix cyclops wall collision", Tooltip = "Wall collision inside cyclops will be much more accurate. Might cause issues when used with other mods. Game has to be reloaded after changing this.")]
        public bool fixCyclopsCollision = false;
        //[Toggle("Sunlight affects lighting in cyclops", Tooltip = "")]
        public bool cyclopsSunlight = false;
        //[Toggle("Instantly open PDA", Tooltip = "Your PDA will open and close instantly. Direction you are looking at will not change when you open it. Game has to be reloaded after changing this. Leave this off if using FCS mods.")]
        //public bool instantPDA = false;
        [Toggle("Always show health and food values in UI", Tooltip = "Health and food values will be always shown not only when PDA is open.")]
        public bool alwaysShowHealthNunbers = false;
        //[Toggle("PDA clock", Tooltip = "Game has to be reloaded after changing this.")]
        public bool pdaClock = true;
        [Keybind("Quickslot cycle key", Tooltip = "Press 'Cycle next' or 'Cycle previous' key while holding down this key to cycle tools in your current quickslot.")]
        public KeyCode quickslotKey = KeyCode.LeftAlt;
        [Keybind("Light intensity key", Tooltip = "When holding a tool in your hand or driving a vehicle press 'Cycle next' or 'Cycle previous' key while holding down this key to change the tool's or vehicle's light intensity.")]
        public KeyCode lightKey = KeyCode.LeftShift;
        //[Keybind("Change torpedo key", Tooltip = "Press 'Cycle next' or 'Cycle previous' key while holding down this key to change torpedo in your current vehicle.")]
        //public KeyCode changeTorpedoKey = KeyCode.LeftAlt;
        [Keybind("Move all items key", Tooltip = "When you have a container open, hold down this key and click an item to move all items.")]
        public KeyCode transferAllItemsKey = KeyCode.LeftControl;
        [Keybind("Move same items key", Tooltip = "When you have a container open, hold down this key and click an item to move all items of the same type.")]
        public KeyCode transferSameItemsKey = KeyCode.LeftShift;

        public bool gameStartWarning = false;
        public bool cyclopsFloodLights = false;
        public bool cyclopsLighting = false;
        public bool exosuitLights = false;
        public bool seaglideLights = false;
        public bool seaglideMap = false;
        public int subThrottleIndex = -1;
        public float knifeRangeDefault = 0f;
        public int activeSlot = -1;
        public Dictionary<string, bool> escapePodSmokeOut = new Dictionary<string, bool>();
        //public Dictionary<string, bool> radioFixed = new Dictionary<string, bool>();
        public HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {{TechType.Salt}, {TechType.Quartz}, {TechType.AluminumOxide}, {TechType.Lithium} , {TechType.Sulphur}, {TechType.Diamond}, {TechType.Kyanite}, {TechType.Magnetite}, {TechType.Nickel}, {TechType.UraniniteCrystal}  };
        public Dictionary<string, Dictionary<int, bool>> openedWreckDoors = new Dictionary<string, Dictionary<int, bool>>();
        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();
        public float medKitHPtoHeal = 0f;
        public Dictionary<string, int> startingLoot = new Dictionary<string, int>
        {
             { "FilteredWater", 2 },
             { "NutrientBlock", 2 },
             { "Flare", 2 },
        };
        public Dictionary<string, int> crushDepthEquipment = new Dictionary<string, int>
        {
             { "ReinforcedDiveSuit", 0 },
        };
        public Dictionary<string, int> crushDamageEquipment = new Dictionary<string, int>
        {
             { "ReinforcedDiveSuit", 0 },
        };
        public Dictionary<string, float> itemMass = new Dictionary<string, float>
        {
            { "PrecursorKey_Blue", 1.6f },
            { "PrecursorKey_Orange", 1.6f },
            { "PrecursorKey_Purple", 1.6f },
            { "PrecursorKey_Red", 1.6f },
            { "PrecursorKey_White", 1.6f },
        };
        public HashSet<string> unmovableItems = new HashSet<string>
        {

        };
        public Dictionary<string, float> bloodColor = new Dictionary<string, float>
        {
            { "Red", 0.784f },
            { "Green", 1f },
            { "Blue", 0.157f },
        };
        //public HashSet<string> nonRechargeable = new HashSet<string>{
        //    { "someBattery" },
        //};

        public HashSet<string> gravTrappable = new HashSet<string>{
            { "seaglide" },
            { "airbladder" },
            { "flare" },
            { "flashlight" },
            { "builder" },
            { "lasercutter" },
            { "ledlight" },
            { "divereel" },
            { "propulsioncannon" },
            { "welder" },
            { "repulsioncannon" },
            { "scanner" },
            { "stasisrifle" },
            { "knife" },
            { "heatblade" },

            { "precursorkey_blue" },
            { "precursorkey_orange" },
            { "precursorkey_purple" },
            { "compass" },
            { "fins" },
            { "fireextinguisher" },
            { "firstaidkit" },
            { "doubletank" },
            { "plasteeltank" },
            { "radiationsuit" },
            { "radiationhelmet" },
            { "radiationgloves" },
            { "rebreather" },
            { "reinforceddivesuit" },
            { "maproomhudchip" },
            { "tank" },
            { "stillsuit" },
            { "swimchargefins" },
            { "ultraglidefins" },
            { "highcapacitytank" },
        };
        public float medKitHPperSecond = 50f;
        public HashSet<TechType> predatorExclusion = new HashSet<TechType> { };

        static void UpdateBaseLight()
        {
            //Base_Light.UpdateBaseLight();
        }
        //public enum CreatureRespawn { Vanilla, Big_creatures_only, Leviathans_only, Big_creatures_and_leviathans }
        public enum DropItemsOnDeath { Vanilla, Drop_everything,Do_not_drop_anything }
        public enum EmptyVehiclesCanBeAttacked { Vanilla, Yes, No, Only_if_lights_on }
        public enum EatingRawFish { Vanilla, Harmless, Risky, Harmful }
        public enum SeaTreaderOutcrop { Vanilla, Only_when_stomping_ground, Never }
        public List<string> silentCreatures = new List<string> { };
        public List<string> removeLight = new List<string> { };
        public List<string> biomesRemoveLight = new List<string> { };
        public List<string> stalkerPlayThings = new List<string> { "ScrapMetal", "MapRoomCamera", "Beacon", "Seaglide", "CyclopsDecoy", "Gravsphere", "SmallStorage", "FireExtinguisher", "DoubleTank", "PlasteelTank", "PrecursorKey_Blue", "PrecursorKey_Orange", "PrecursorKey_Purple", "PrecursorKey_Red", "PrecursorKey_White", "Rebreather", "Tank", "HighCapacityTank", "Flare", "Flashlight", "Builder", "LaserCutter", "LEDLight", "DiveReel", "PropulsionCannon", "Knife", "HeatBlade", "Scanner", "Welder", "RepulsionCannon", "StasisRifle" };
        public Dictionary<TechType, float> lightIntensity = new Dictionary<TechType, float>();
        public Dictionary<string, float> damageMult_ = new Dictionary<string, float> { { "Creepvine", 1f } };
        public bool pickedUpFireExt = false;
        public Dictionary<string, Dictionary<string, bool>> baseLights = new Dictionary<string, Dictionary<string, bool>>();
        //public Dictionary<string, TechType> aliveCreatureLoot = new Dictionary<string, TechType>();
        public Dictionary<string, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<string, Dictionary<TechType, int>> { { "Stalker", new Dictionary<TechType, int> { { TechType.StalkerTooth, 2 } } }, { "Gasopod", new Dictionary<TechType, int> { { TechType.GasPod, 5 } } } };
        public Dictionary<TechType, int> eatableFoodValue = new Dictionary<TechType, int> { };
        public Dictionary<TechType, int> eatableWaterValue = new Dictionary<TechType, int> { };
        //public Dictionary<string, Decoy_Patch.decoyData> decoys = new Dictionary<string, Decoy_Patch.decoyData>();
        //public HashSet<TechType> canAttackPlayer = new HashSet<TechType> { "Shocker, TechType.Biter, TechType.Blighter, TechType.BoneShark, TechType.Crabsnake, TechType.CrabSquid, TechType.Crash, TechType.Mesmer, TechType.SpineEel, TechType.Sandshark, TechType.Stalker, TechType.Warper, TechType.Bleeder, TechType.Shuttlebug, TechType.CaveCrawler, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile, TechType.ReaperLeviathan, TechType.SeaDragon };
        //public HashSet<TechType> canAttackVehicle = new HashSet<TechType> { TechType.Shocker, TechType.BoneShark, TechType.Crabsnake, TechType.CrabSquid, TechType.SpineEel, TechType.Sandshark, TechType.Stalker, TechType.Warper, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile, TechType.ReaperLeviathan, TechType.SeaDragon };
        //public HashSet<TechType> canAttackSub = new HashSet<TechType> { TechType.Shocker, TechType.CrabSquid, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile, TechType.ReaperLeviathan, TechType.SeaDragon };
        //private void EatRawFishChangedEvent(ChoiceChangedEventArgs e)
        //{
        //    AddDebug("EatRawFishChangedEvent " + eatRawFish); 
        //}
        //public HashSet<TechType> ttt = new HashSet<TechType> {TechType.Coffee };
        static void UpdateGameSpeed()
        {
            if (DayNightCycle.main)
                DayNightCycle.main._dayNightSpeed = Main.config.dayCycleSpeed;
        }
        public bool fixMelons = false;
        public bool randomPlantRotation = true;
        public bool silentReactor = false;
        public bool removeFragmentCrate = true;
        public bool creepvineLights = true;
        public bool LEDLightWorksInHand = true;
        //public int detectCollisionsDist = 50;
        public bool newUIstrings = true;
        public bool newStorageUI = true;
        public bool tweaksAffectingGPU = true;
        public bool disableUseText = false;
        public bool craftWithoutBattery = false;
        public bool disableCyclopsProximitySensor = false;
        public float creepVineSeedFood = 10f;
        //[Slider("growingPlantUpdateInterval", 0, 10, DefaultValue = 0, Step = 1, Format = "{0:F0}", Tooltip = "")]
        //public int growingPlantUpdateInterval = 0;
        public bool builderPlacingWhenFinishedBuilding = true;
        public bool crushDamageScreenEffect = true;
        public bool removeCookedFishOnReload = true;
        public bool fishRespawn = true;
        public bool fishRespawnIfKilledByPlayer = true;
        public bool creaturesRespawn = true;
        public bool creaturesRespawnIfKilledByPlayer = false;
        public bool leviathansRespawn = false;
        public bool leviathansRespawnIfKilledByPlayer = false;
        public bool disableGravityForExosuit = false;
        public float cyclopsDealDamageMinSpeed = 2f;
        public float cyclopsTakeDamageMinSpeed = 2f;
        public float cyclopsTakeDamageMinMass = 200f;
        public float exosuitDealDamageMinSpeed = 7f;
        public float exosuitTakeDamageMinSpeed = 7f;
        public float exosuitTakeDamageMinMass = 5f;
        public float seamothDealDamageMinSpeed = 2f;
        public float seamothTakeDamageMinSpeed = 4f;
        public float seamothTakeDamageMinMass = 5f;
        public bool pdaTabSwitchHotkey = true;
        public float lavaGeyserEruptionForce = 20f;
        public float lavaGeyserEruptionInterval = 12f;
        public bool removeLavaGeyserRockParticles = false;
        public bool replaceDealDamageOnImpactScript = true;
        //public bool gelSackDecomposes = false;
        public float solarPanelMaxDepth = 250f;
        public bool stalkerLooseToothSound = true;
        public bool canReplantMelon = true;

        // also edit UI_Patches.GetStrings when editing this
        public List<string> translatableStrings = new List<string> //  translate config enums 
        { "Burnt out ", //  0   flare
            "Lit ",      // 1   flare
        "Toggle lights", // 2   toggle lights
        "Increases your safe diving depth by ", // 3 crushDepthEquipment
        " meters.", // 4    crushDepthEquipment
        "Restores ", // 5    medkit desc
        " health.", // 6    medkit desc
        "mass ",    // 7     invMultWater   invMultLand
        ": min ",     // 8    eatRawFish tooltip 
        ", max ",     // 9    eatRawFish tooltip 
        "Throw",        // 10   flare   
        "Light and throw",  //  11  flare   
        "Light" ,        // 12   flare   
        "Toggle map" ,    // 13   Seaglide map   
         "Push " ,    // 14   push beached seamoth
          "Need a knife to break it" ,    // 15  no breaking with bare hands  BreakableResource
        "Need a knife to break it free" ,    // 16  no breaking with bare hands  Pickupable
        "Toggle lights",            // 17  toggle seamoth lights
        ". Press and hold ",         // 18  seamoth defense module
        " to charge the shot",      // 19  seamoth defense module
        " Change torpedo ",      // 20  vehicle UI
        " Hold ",                // 21  exosuit UI
        " and press ",          // 22  exosuit UI
        " to change torpedo ",      // 23  exosuit UI
        // 24 Bladderfish ttoltip
        "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.",
        // 25 SeamothElectricalDefense tooltip
        "Generates a localized electric field designed to ward off aggressive fauna. Press and hold the button to charge the shot.",
        "Swivel left", // 26 
        "Swivel right", // 27 
        };  // also edit UI_Patches.GetStrings when editing this

    }

}