using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;
using SMLHelper.V2.Commands;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Interfaces;
using SMLHelper.V2.Options;

namespace Tweaks_Fixes
{
    [Menu("Tweaks and Fixes")]
    public class Config : ConfigFile
    {

        [Slider("Player damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage player takes will be multiplied by this.")]
        public float playerDamageMult = 1f;
        [Slider("Vehicle damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage your vehicles take will be multiplied by this.")]
        public float vehicleDamageMult = 1f;
        [Slider("Damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "When anything but the player or vehicles takes damage, it will be multiplied by this.")]
        public float damageMult = 1f;
        [Slider("Predator aggression multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "The higher it is the more aggressive predators are towards you. When it's 0 you and your vehicles will never be attacked. When it's 3 predators attack you on sight and never flee.")]
        public float aggrMult = 1f;
        [Slider("Oxygen per breath", 0f, 6f, DefaultValue = 3f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Amount of oxygen you consume every breath.")]
        public float oxygenPerBreath = 3f;
        [Slider("Tool power consumption multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amout of energy consumed by your tools will be multiplied by this.")]
        public float toolEnergyConsMult = 1f;
        [Slider("Vehicle power consumption multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amout of energy consumed by your vehicles will be multiplied by this.")]
        public float vehicleEnergyConsMult = 1f;
        [Slider("Base power consumption multiplier", 0f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amout of energy consumed by things in your base will be multiplied by this.")]
        public float baseEnergyConsMult = 1f;
        [Slider("First aid kit HP", 10, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "HP restored by using first aid kit.")]
        public int medKitHP = 50;
        //[Slider("First aid kit HP per second", 1, 100, DefaultValue = 50, Step = 1, Format = "{0:F0}", Tooltip = "HP restored every second after using first aid kit.")]
        [Slider("Crafting time multiplier", 0.1f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Crafting time will be multiplied by this when crafting things with fabricator or modification station.")]
        public float craftTimeMult = 1f;
        [Slider("Building time multiplier", 0.1f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Building time will be multiplied by this when using builder tool.")]
        public float buildTimeMult = 1f;

        [Toggle("Player movement tweaks", Tooltip = "Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 10% on land and in water. Fins reduce your speed by 10% on land. Lightweight high capacity tank reduces your speed by 5% on land. Every other tank reduces your speed by 10% on land. No speed reduction when near wrecks. You can sprint only if moving forward. Seaglide works only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%.")]
        public bool playerMoveTweaks = false;
        [Slider("Inventory weight multiplier in water", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are swimming you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultWater = 0f;
        [Slider("Inventory weight multiplier on land", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are on land you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.")]
        public float invMultLand = 0f;
        [Toggle("Seamoth movement tweaks", Tooltip = "Seamoth does not exceed its max speed and does not consume more power when moving diagonally. Its vertical and sideways speed is halved and  it can not move backward.")]
        public bool seamothMoveTweaks = false;
        [Toggle("Prawn suit movement tweaks", Tooltip = "Prawn suit can not move sideways. No time limit when using thrusters, but they consume twice more power than walking. Game has to be reloaded after changing this.")]
        public bool exosuitMoveTweaks = false;
        [Toggle("Cyclops movement tweaks", Tooltip = "Cyclops does not exceed its max speed and does not consume more power when moving diagonally. Its vertical and backward speed is halved.")]
        public bool cyclopsMoveTweaks = false;
        [Slider("Crush depth", 50, 500, DefaultValue = 200, Step = 10, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if crush damage multiplier is 0.")]
        public int crushDepth = 200;
        [Slider("Crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 every 3 seconds player takes 1 damage multiplied by this for every meter below crush depth.")]
        public float crushDamageMult = 0f;
        [Slider("Vehicle crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 every 3 seconds vehicles take 1 damage multiplied by this for every meter below crush depth.")]
        public float vehicleCrushDamageMult = 0f;
        [Choice("Unmanned vehicles can be attacked", Tooltip = "By default unmanned seamoth or prawn suit can be attacked.")]
        public EmptySeamothCanBeAttacked emptySeamothCanBeAttacked;
        [Choice("Unmanned cyclops can be attacked", Tooltip = "By default unmanned cyclops can not be attacked.")]
        public EmptyCyclopsCanBeAttacked emptyCyclopsCanBeAttacked;
        [Slider("Hunger update interval", 1, 100, DefaultValue = 10, Step = 1, Format = "{0:F0}", Tooltip = "Time interval in game seconds after which your hunger and thirst update. This is not the same as the 'hunger/thirst' setting from the 'Day/Night Speed' mod. That setting changes amount of food you lose, not the update interval.")]
        public int hungerUpdateInterval = 10;
        [Toggle("New hunger system", Tooltip = "You don't regenerate health when you are full. You don't lose health when your food or water value is 0. Your food and water values can go as low as -100. When your food or water value is below 0 your movement speed will be reduced proportionally to that value. When either your food or water value is -100 your movement speed will be reduced by 50% and you will start taking hunger damage. Your max food and max water value is 200. The higher your food value above 100 is the less food you get when eating: when your food value is 110 you lose 10% of food, when it's 190 you lose 90%.")]
        public bool newHungerSystem = false;

        [Choice("Eating raw fish", Tooltip = "When it's not vanilla, amount of food you get by eating raw fish changes. Harmless: it's a random number between 0 and fish's food value. Risky: it's a random number between fish's food negative value and fish's food value. Harmful: it's a random number between fish's food negative value and 0.")]
        public EatingRawFish eatRawFish;
        [Toggle("Food tweaks", Tooltip = "Raw fish water value is half of its food value. Cooked rotten fish has no food value. Game has to be reloaded after changing this.")]
        public bool foodTweaks = false;
        [Toggle("Can't eat underwater", Tooltip = "You can't eat or drink when swimming underwater.")]
        public bool cantEatUnderwater = false;
        [Toggle("Can't use first aid kit underwater", Tooltip = "You won't be able to use first aid kit when swimming underwater.")]
        public bool cantUseMedkitUnderwater = false;
        //[Toggle("Eat fish on release", Tooltip = "Eat the fish you are holding in your hand when you press the 'right hand' button.")]
        //public bool eatFishOnRelease = false;
        [Slider("Food decay rate multiplier", 0.1f, 3f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "Food decay rate will be multiplied by this.")]
        public float foodDecayRateMult = 1f;
        [Slider("Catchable fish speed multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Swimming speed of fish that you can catch will be multiplied by this.")]
        public float fishSpeedMult = 1f;
        [Slider("Other creatures speed multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Swimming speed of creatures that you can't catch will be multiplied by this.")]
        public float creatureSpeedMult = 1f;
        [Slider("Knife attack range multiplier", 1f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeRangeMult = 1f;
        [Slider("Knife damage multiplier", 1f, 5f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. You have to reequip your knife after changing this.")]
        public float knifeDamageMult = 1f;

        [Toggle("Can't catch fish with bare hands", Tooltip = "To catch fish you will have to use propulsion cannon or grav trap. Does not apply if you are inside alien containment.")]
        public bool noFishCatching = false;
        [Toggle("Can't break outcrop with bare hands", Tooltip = "You will have to use a knife to break outcrops or collect resources attached to rock or seabed.")]
        public bool noBreakingWithHand = false;
        [Toggle("Disable tutorial messages", Tooltip = "Disable messages that tell you to 'eat something', 'break limestone', etc. Game has to be reloaded after changing this.")]
        public bool disableHints = false;
        [Toggle("Realistic oxygen consumption", Tooltip = "Vanilla oxygen consumption has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth between 100 and 0 meters. With this on your oxygen consumption will increase in linear progression using 'Crush depth' setting. When you are at crush depth it will be vanilla max oxygen consumption and will increase as you dive deeper.")]
        public bool realOxygenCons = false;
        //[Slider("brainCoralBubbleInterval", 1, 20, DefaultValue = 3, Step = 1, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if crush damage multiplier is 0.")]
        //public int brainCoralBubbleInterval = 3;
        [Toggle("Poison increases hunger", Tooltip = "Poison damage you take will increase your hunger and thirst instead of reducing your HP. Only when your food or water value is 0 you will start losing health.")]
        public bool replacePoisonDamage = false;
  
        [Toggle("Drop held tool when taking damage", Tooltip = "Chance to drop your tool is proportional to amount of damage taken. If you take 30 damage, there is 30% chance you will drop your tool.")]
        public bool dropHeldTool = false;

        //[Toggle("Predators less likely to flee", Tooltip = "Predators don't flee when their health is above 50%. When it's not, chance to flee is proportional to their health. The more health they have the less likely they are to flee.")]
        //public bool predatorsDontFlee = false;
        [Toggle("Every creature respawns", Tooltip = "By default big creatures never respawn if killed by player.")]
        public bool creaturesRespawn = false;

        [Toggle("Camera bobbing", Tooltip = "Camera bobbing when swimming.")]
        public bool cameraBobbing = true;
        [Toggle("Free camera drones for scanner room", Tooltip = "If disabled, scanner room will be built without camera drones.")]
        public bool mapRoomFreeCameras = true;
        [Slider("flare light intensity", 0.1f, 1f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "You have to reequip your flare after changing this.")]
        public float flareIntensity = 1f;
        [Toggle("Unlock prawn suit only by scanning prawn suit", Tooltip = "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arms. Game has to be reloaded after changing this.")]
        public bool cantScanExosuitClawArm = false;

        [Slider("Free torpedos", 0, 6, DefaultValue = 2, Step = 1, Format = "{0:F0}", Tooltip = "Number of torpedos you get when installing Torpedo System or Prawn Suit Torpedo Arm. After changing this you have to craft a new Torpedo System.")]
        public int freeTorpedos = 2;
        //[Toggle("Remove light from open databox", Tooltip = "Disable databox light when you open it so it does not draw your attention next time you see it. Game has to be reloaded after changing this.")]
        //public bool disableDataboxLight = false;
        [Slider("Life pod power cell max charge", 10, 100, DefaultValue = 25, Step = 1, Format = "{0:F0}", Tooltip = "Max charge for each of its 3 power cells. Game has to be reloaded after changing this.")]
        public int escapePodMaxPower = 25;
        [Toggle("Life pod power tweaks", Tooltip = "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod's power cells are 30% charged. Game has to be reloaded after changing this.")]
        public bool escapePodPowerTweak = false;
        [Slider("Crafted battery charge percent", 0, 100, DefaultValue = 100, Step = 1, Format = "{0:F0}", Tooltip = "Charge percent of batteries and power cells you craft will be set to this.")]
        public int craftedBatteryCharge = 100;
        [Choice("Drop items when you die", Tooltip = "")]
        public DropItemsOnDeath dropItemsOnDeath;
        [Toggle("No particles when creature dies", Tooltip = "No particles (yellow cloud) will spawn when a creature dies. Game has to be reloaded after changing this.")]
        public bool noKillParticles = false;
        [Choice("Outcrops from seatreaders", Tooltip = "")]
        public SeaTreaderOutcrop seaTreaderOutcrop;
        //[Toggle("No easy shale outcrops from sea treaders", Tooltip = "Sea treaders unearth shale outcrops only when stomping the ground.")]
        //public bool seaTreaderChunks = false;
        //[Toggle("Disable reaper's roar", Tooltip = "Game has to be reloaded after changing this.")]
        //public bool disableReaperRoar = false;
        [Toggle("No metal clicking sound when walking", Tooltip = "Removes metal clicking sound when walking on metal surface.")]
        public bool fixFootstepSound = false;
        [Toggle("Turn off lights in your base"), OnChange(nameof(UpdateBaseLight))]
        public bool baseLightOff = false;
        [Toggle("Sunlight affects lighting in cyclops", Tooltip = "")]
        public bool cyclopsSunlight = false;
        [Keybind("Quickslot cycle key", Tooltip = "Press 'Cycle next' or 'Cycle previous' key while holding down this key to cycle tools in your current quickslot.")]
        public KeyCode quickslotKey = KeyCode.LeftAlt;

        public int subThrottleIndex = -1;
        public float knifeRangeDefault = 0f;
        public float playerCamRot = -1f;
        public int activeSlot = -1;
        public Dictionary<string, bool> escapePodSmokeOut = new Dictionary<string, bool>();
        public HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {{TechType.Salt}, {TechType.Quartz}, {TechType.AluminumOxide}, {TechType.Lithium} , {TechType.Sulphur}, {TechType.Diamond}, {TechType.Kyanite}, {TechType.Magnetite}, {TechType.Nickel}, {TechType.UraniniteCrystal}  };
        public Dictionary<string, Dictionary<int, bool>> openedWreckDoors = new Dictionary<string, Dictionary<int, bool>>();
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
        public Dictionary<string, float> itemMass = new Dictionary<string, float>
        {
            { "ScrapMetal", 120 },
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
        public HashSet<TechType> predatorExclusion = new HashSet<TechType> {TechType.Crash, TechType.Mesmer };

        static void UpdateBaseLight()
        {
            Base_Light.SubRoot_Awake_Patch.UpdateBaseLight();
        }
        public enum DropItemsOnDeath { Vanilla, Drop_everything, Do_not_drop_anything }
        public enum EmptyCyclopsCanBeAttacked { No, Yes, Only_if_its_lights_on }
        public enum EmptySeamothCanBeAttacked { Yes, No, Only_if_its_lights_on }
        public enum EatingRawFish { Vanilla, Harmless, Risky, Harmful }
        public enum SeaTreaderOutcrop { Vanilla, Only_when_stomping_ground, Never }
        public HashSet<TechType> silentCreatures = new HashSet<TechType> { };
        public HashSet<TechType> canAttackPlayer = new HashSet<TechType> { TechType.Shocker, TechType.Biter, TechType.Blighter, TechType.BoneShark, TechType.Crabsnake, TechType.CrabSquid, TechType.Crash, TechType.Mesmer, TechType.SpineEel, TechType.Sandshark, TechType.Stalker, TechType.Warper, TechType.Bleeder, TechType.Shuttlebug, TechType.CaveCrawler, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile, TechType.ReaperLeviathan, TechType.SeaDragon };
        public HashSet<TechType> canAttackVehicle = new HashSet<TechType> { TechType.Shocker, TechType.BoneShark, TechType.Crabsnake, TechType.CrabSquid, TechType.SpineEel, TechType.Sandshark, TechType.Stalker, TechType.Warper, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile, TechType.ReaperLeviathan, TechType.SeaDragon };
        //public HashSet<TechType> canAttackSub = new HashSet<TechType> { TechType.Shocker, TechType.CrabSquid, TechType.GhostLeviathan, TechType.GhostLeviathanJuvenile, TechType.ReaperLeviathan, TechType.SeaDragon };
        //private void EatRawFishChangedEvent(ChoiceChangedEventArgs e)
        //{
        //    AddDebug("EatRawFishChangedEvent " + eatRawFish); 
        //}

    }
}