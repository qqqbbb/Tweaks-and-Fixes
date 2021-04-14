using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Collections.Generic;
using UnityEngine;

namespace Tweaks_Fixes
{
    [Menu("Tweaks and Fixes")]
    public class Config : ConfigFile
    {
        //[Toggle("disable damage")]
        //public bool disableDamage = false;

        [Toggle("Player movement speed tweaks", Tooltip = "Player vertical, backward, sideways movement speed is halved. Any diving suit reduces your speed by 10% both in water and on land. Fins reduce your speed by 10% when on land. No speed reduction when near wrecks. You can sprint only if moving forward. When swimming while your PDA is open your movement speed is halved. When swimming while holding a tool in your hand your movement speed is reduced to 70%.")]
        public bool playerMoveSpeedTweaks = false;
        [Toggle("Inventory affects your movement speed", Tooltip = "The more items you have in inventory the slower you move. When your inventory is full your movement speed is reduced to 50%. Works only if 'Player movement speed tweaks' is enabled.")]
        public bool InvAffectSpeed = false;
        [Toggle("Vehicle movement tweaks", Tooltip = "Vehicles don't exceed their max speed and don't consume more power when moving diagonally. Prawn suit can't move sideways. Seamoth can't move backward. Seamoth's vertical and sideways speed is halved. Cyclop's vertical and backward speed is halved. No time limit when using prawn suit thrusters, but they consume twice more power than walking.")]
        public bool vehicleMoveTweaks = false;
        [Toggle("Can't break outcrop with bare hands", Tooltip = "You will have to use a knife to break outcrops or collect resources attached to rock.")]
        public bool noBreakingWithHand = false;
        [Toggle("Disable health regeneration from food", Tooltip = "You don't regenerate health when full.")]
        public bool noHealthRegenFromFood = false;

        [Toggle("Drop held tool when taking damage", Tooltip = "Chance to drop your tool is proportional to amount of damage taken. If you take 30 damage, there is 30% chance you will drop your tool.")]
        public bool dropHeldTool = false;
        [Toggle("Disable tutorial messages", Tooltip = "Disable messages that tell you to 'eat something', 'break limestone', etc.")]
        public bool disableGoals;
        [Slider("Knife attack range percent", 100, 300, DefaultValue = 100, Step = 1, Format = "{0:F0}")]
        public int KnifeRangeMult = 100;
        [Toggle("Realistic oxygen consumption", Tooltip = "Vanilla oxygen consumption has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth between 100 and 0 meters. With this on it will increase in linear progression using 'Crush depth' setting.")]
        public bool realOxygenCons = false;
        [Slider("Crush depth", 100, 500, DefaultValue = 200, Step = 10, Format = "{0:F0}", Tooltip = "Depth below which player starts taking damage. Does not work if 'Crush damage multiplier' is 0.")]
        public int crushDepth = 200;
        [Slider("Crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 player takes 1 damage multiplied by this for every meter below crush depth.")]
        public float crushDamageMult = 0f;
        [Slider("Vehicle crush damage multiplier", 0f, 1f, DefaultValue = 0f, Step = .01f, Format = "{0:R0}", Tooltip = "When it's not 0 vehicles take 1 damage multiplied by this for every meter below crush depth.")]
        public float vehicleCrushDamageMult = 0f;
        [Slider("Food decay rate multiplier", 0.1f, 2f, DefaultValue = 1f, Step = .01f, Format = "{0:R0}", Tooltip = "Food decay rate will be multiplied by this.")]
        public float foodDecayRateMult = 1f;
        [Toggle("Food tweaks", Tooltip = "Fish water value is half of its food value. Cooked rotten fish has no food value.")]
        public bool foodTweaks = false;
        [Toggle("Predators less likely to flee", Tooltip = "Predators don't flee when their health is above 50%. When it's not, chance to flee is proportional to their health. The more health they have the less likely they are to flee.")]
        public bool predatorsDontFlee = false;
        [Toggle("Every creature respawns", Tooltip = "By default big creatures never respawn.")]
        public bool creaturesRespawn = false;

        [Toggle("Eat fish on release", Tooltip = "Eat the fish you are holding in your hand when you press the 'right hand' button.")]
        public bool eatFishOnRelease = false;
        [Toggle("Unlock prawn suit only by scanning prawn suit", Tooltip = "In vanilla game prawn suit can be unlocked by scanning 20 prawn suit arms.")]
        public bool cantScanExosuitClawArm = false;
        [Toggle("Disable reaper's roar")]
        public bool disableReaperRoar = false;
        [Toggle("Disable databox light", Tooltip = "Disable databox light when you open it so you don't have to check it next time you see it.")]
        public bool disableDataboxLight = false;
        [Slider("life pod power cell max charge", 10, 100, DefaultValue = 25, Step = 1, Format = "{0:F0}", Tooltip = "Max charge for each of its 3 power cells.")]
        public int escapePodMaxPower = 25;
        [Toggle("life pod power tweaks", Tooltip = "When your life pod is damaged its max power is reduced to 50%. When you crashland your life pod's power cells are 30% charged.")]
        public bool escapePodPowerTweak = false;
        [Toggle("No easy shale outcrops from sea treaders", Tooltip = "Sea treaders unearth shale outcrops only when stomping the ground.")]
        public bool seaTreaderChunks = false;
        [Keybind("Quickslot cycle key", Tooltip = "Press 'Cycle next' or 'Cycle previous' key while holding down this key to cycle tools in your current quickslot.")]
        public KeyCode quickslotKey = KeyCode.LeftAlt;

        public int subThrottleIndex = -1;
        public float KnifeRangeDefault = 0f;
        public float playerCamRot = -1f;
        public int activeSlot = -1;
        public Dictionary<string, bool> escapePodSmokeOut = new Dictionary<string, bool>();
        public HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {{TechType.Salt}, {TechType.Quartz}, {TechType.AluminumOxide}, {TechType.Lithium} };
        public Dictionary<string, Dictionary<int, bool>> openedWreckDoors = new Dictionary<string, Dictionary<int, bool>>();
        //public Dictionary<string, Dictionary<int, bool>> test = new Dictionary<string, Dictionary<int, bool>>();
    }
}