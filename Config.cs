using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Tweaks_Fixes
{
    [Menu("Tweaks and Fixes")]
    public class Config : ConfigFile
    {
        public float version = 1.04f;
        [Slider("Player damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage player takes will be multiplied by this.")]
        public float playerDamageMult = 1f;
        [Slider("Vehicle damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Amount of damage your vehicles take will be multiplied by this.")]
        public float vehicleDamageMult = 1f;
        [Slider("Damage multiplier", 0f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "When anything but the player or vehicles takes damage, it will be multiplied by this.")]
        public float damageMult = 1f;
        [Slider("Crafting time multiplier", 0.1f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Crafting time will be multiplied by this When crafting things with fabricator or modification station.")]
        public float craftTimeMult = 1f;
        [Slider("Building time multiplier", 0.1f, 3f, DefaultValue = 1f, Step = 0.1f, Format = "{0:R0}", Tooltip = "Building time will be multiplied by this when using builder tool.")]
        public float buildTimeMult = 1f;
        [Toggle("Player movement tweaks", Tooltip = "Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 10% on land and in water. Fins reduce your speed by 10% on land. Lightweight high capacity tank reduces your speed by 5% on land. Every other tank reduces your speed by 10% on land. No speed reduction when near wrecks. You can sprint only if moving forward. Seaglide works only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%.")]
        public bool playerMoveSpeedTweaks = false;
        [Slider("Inventory weight multiplier in water", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are swimming you lose 1% of your max speed for every kilo of weight in your inventory multiplied by this. Works only if 'Player movement tweaks' is enabled.")]
        public float invMultWater = 0f;
        [Slider("Inventory weight multiplier on land", 0f, 1f, DefaultValue = 0f, Step = .001f, Format = "{0:R0}", Tooltip = "When it's not 0 and you are on land you lose 1% of your max speed for every kilo of weight in your inventory multiplied by this. Works only if 'Player movement tweaks' is enabled.")]
        public float invMultLand = 0f;
        [Toggle("Seamoth movement tweaks", Tooltip = "Seamoth does not exceed its max speed and does not consume more power when moving diagonally. Its vertical and sideways speed is halved and  it can not move backward.")]
        public bool seamothMoveTweaks = false;
        [Toggle("Prawn suit movement tweaks", Tooltip = "Prawn suit can not move sideways. No time limit when using thrusters, but they consume twice more power than walking.")]
        public bool exosuitMoveTweaks = false;
        [Toggle("Cyclops movement tweaks", Tooltip = "Cyclops does not exceed its max speed and does not consume more power when moving diagonally. Its vertical and backward speed is halved.")]
        public bool cyclopsMoveTweaks = false;
        [Toggle("Can't break outcrop with bare hands", Tooltip = "You will have to use a knife to break outcrops or collect resources attached to rock.")]
        public bool noBreakingWithHand = false;
        [Slider("Catchable fish speed multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Speed of fish that you can catch will be multiplied by this.")]
        public float fishSpeedMult = 1f;
        [Slider("Big creature speed multiplier", .1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Speed of creatures that you can't catch will be multiplied by this.")]
        public float creatureSpeedMult = 1f;
        [Toggle("Can't catch fish with bare hands", Tooltip = "To catch alive fish you will have to use propulsion cannon or grav trap. Does not apply if you are inside alien containment.")]
        public bool noFishCatching = false;
        [Toggle("Disable health regeneration from food", Tooltip = "You don't regenerate health when full.")]
        public bool noHealthRegenFromFood = false;

        [Toggle("Drop held tool when taking damage", Tooltip = "Chance to drop your tool is proportional to amount of damage taken. If you take 30 damage, there is 30% chance you will drop your tool.")]
        public bool dropHeldTool = false;
        [Toggle("Disable tutorial messages", Tooltip = "Disable messages that tell you to 'eat something', 'break limestone', etc. Game has to be reloaded after changing this.")]
        public bool disableGoals = false;
        [Toggle("Realistic oxygen consumption", Tooltip = "Vanilla oxygen consumption has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth between 100 and 0 meters. With this on it will increase in linear progression using 'Crush depth' setting. Oxygen consumption at crush depth will be equal to vanilla max oxygen consumption and it will increase as you dive deeper.")]
        public bool realOxygenCons = false;
        [Slider("Knife attack range multiplier", 1f, 3f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. Game has to be reloaded after changing this.")]
        public float knifeRangeMult = 1f;
        [Slider("Knife damage multiplier", 1f, 10f, DefaultValue = 1f, Step = .1f, Format = "{0:R0}", Tooltip = "Applies to knife and heatblade. Game has to be reloaded after changing this.")]
        public float knifeDamageMult = 1f;
        [Slider("Crush depth", 100, 500, DefaultValue = 200, Step = 10, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if crush damage multiplier is 0.")]
        public int crushDepth = 200;
        [Slider("Crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 player takes 1 damage multiplied by this for every meter below crush depth.")]
        public float crushDamageMult = 0f;
        [Slider("Vehicle crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 vehicles take 1 damage multiplied by this for every meter below crush depth.")]
        public float vehicleCrushDamageMult = 0f;
        [Slider("Food decay rate multiplier", 0.1f, 2f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "Food decay rate will be multiplied by this.")]
        public float foodDecayRateMult = 1f;
        [Slider("flare light intensity", 0.1f, 1f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "Game has to be reloaded after changing this.")]
        public float flareIntensity = 1f;
        [Toggle("Food tweaks", Tooltip = "Raw fish water value is half of its food value. Cooked rotten fish has no food value.")]
        public bool foodTweaks = false;
        [Toggle("Predators less likely to flee", Tooltip = "Predators don't flee when their health is above 50%. When it's not, chance to flee is proportional to their health. The more health they have the less likely they are to flee.")]
        public bool predatorsDontFlee = false;
        [Toggle("Every creature respawns", Tooltip = "By default big creatures never respawn if killed. Game has to be reloaded after changing this.")]
        public bool creaturesRespawn = false;

        [Toggle("Eat fish on release", Tooltip = "Eat the fish you are holding in your hand when you press the 'right hand' button.")]
        public bool eatFishOnRelease = false;
        [Toggle("Unlock prawn suit only by scanning prawn suit", Tooltip = "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arms. Game has to be reloaded after changing this.")]
        public bool cantScanExosuitClawArm = false;
        [Toggle("Disable reaper's roar")]
        public bool disableReaperRoar = false;
        [Toggle("Disable databox light", Tooltip = "Disable databox light when you open it so it does not draw your attention next time you see it. Game has to be reloaded after changing this.")]
        public bool disableDataboxLight = false;
        [Slider("Life pod power cell max charge", 10, 100, DefaultValue = 25, Step = 1, Format = "{0:F0}", Tooltip = "Max charge for each of its 3 power cells. Game has to be reloaded after changing this.")]
        public int escapePodMaxPower = 25;
        [Toggle("Life pod power tweaks", Tooltip = "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod's power cells are 30% charged. Game has to be reloaded after changing this.")]
        public bool escapePodPowerTweak = false;
        [Toggle("No easy shale outcrops from sea treaders", Tooltip = "Sea treaders unearth shale outcrops only when stomping the ground.")]
        public bool seaTreaderChunks = false;
        [Toggle("No metal clicking sound when walking", Tooltip = "Removes metal clicking sound when walking on metal surface.")]
        public bool fixFootstepSound = false;
        [Toggle("Turn off lights in your base"), OnChange(nameof(UpdateBaseLight))]
        public bool baseLightOff = false;
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
        //public Dictionary<string, Dictionary<int, bool>> test = new Dictionary<string, Dictionary<int, bool>>();

        static void UpdateBaseLight()
        {
            Base_Light.SubRoot_Awake_Patch.UpdateBaseLight(); 
        }
    }
}