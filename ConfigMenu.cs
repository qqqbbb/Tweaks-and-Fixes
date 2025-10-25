using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class ConfigMenu
    {
        public static ConfigEntry<float> timeFlowSpeed;
        public static ConfigEntry<float> playerWaterSpeedMult;
        public static ConfigEntry<float> playerGroundSpeedMult;
        public static ConfigEntry<float> seaglideSpeedMult;
        public static ConfigEntry<float> playerDamageMult;
        public static ConfigEntry<float> vehicleDamageMult;
        public static ConfigEntry<float> aggrMult;
        public static ConfigEntry<float> oxygenPerBreath;
        public static ConfigEntry<float> toolEnergyConsMult;
        public static ConfigEntry<float> vehicleEnergyConsMult;
        public static ConfigEntry<float> baseEnergyConsMult;
        public static ConfigEntry<float> knifeRangeMult;
        public static ConfigEntry<float> knifeDamageMult;
        public static ConfigEntry<int> medKitHP;
        public static ConfigEntry<float> craftTimeMult;
        public static ConfigEntry<float> buildTimeMult;
        public static ConfigEntry<int> cyclopsFireChance;
        public static ConfigEntry<int> cyclopsAutoHealHealthPercent;
        public static ConfigEntry<int> crushDepth;
        public static ConfigEntry<float> crushDamage;
        public static ConfigEntry<float> vehicleCrushDamageMult;
        public static ConfigEntry<EmptyVehiclesCanBeAttacked> emptyVehiclesCanBeAttacked;
        public static ConfigEntry<bool> newHungerSystem;
        public static ConfigEntry<int> fishFoodWaterRatio;
        public static ConfigEntry<EatingRawFish> eatRawFish;
        public static ConfigEntry<bool> cantEatUnderwater;
        public static ConfigEntry<bool> cantUseMedkitUnderwater;
        public static ConfigEntry<float> foodDecayRateMult;
        public static ConfigEntry<int> fruitGrowTime;
        public static ConfigEntry<float> fishSpeedMult;
        public static ConfigEntry<float> creatureSpeedMult;
        public static ConfigEntry<int> CreatureFleeChance;
        public static ConfigEntry<bool> creatureFleeUseDamageThreshold;
        public static ConfigEntry<bool> creatureFleeChanceBasedOnHealth;
        public static ConfigEntry<bool> waterparkCreaturesBreed;
        public static ConfigEntry<bool> noFishCatching;
        public static ConfigEntry<bool> noBreakingWithHand;
        public static ConfigEntry<bool> damageImpactEffect;
        public static ConfigEntry<bool> damageScreenFX;
        public static ConfigEntry<int> stalkerLoseToothChance;
        public static ConfigEntry<bool> realOxygenCons;
        public static ConfigEntry<int> decoyLifeTime;
        public static ConfigEntry<int> decoyHP;
        public static ConfigEntry<int> escapePodMaxPower;
        public static ConfigEntry<float> batteryChargeMult;
        public static ConfigEntry<int> craftedBatteryCharge;
        public static ConfigEntry<DropItemsOnDeath> dropItemsOnDeath;
        public static ConfigEntry<float> invMultWater;
        public static ConfigEntry<float> invMultLand;
        public static ConfigEntry<float> baseHullStrengthMult;
        public static ConfigEntry<float> drillDamageMult;
        public static ConfigEntry<float> crushDamageProgression;
        public static ConfigEntry<float> exosuitSpeedMult;
        public static ConfigEntry<float> seamothSpeedMult;
        public static ConfigEntry<float> cyclopsSpeedMult;
        public static ConfigEntry<float> foodLossMult;
        public static ConfigEntry<float> waterLossMult;
        public static ConfigEntry<int> foodWaterHealThreshold;
        public static ConfigEntry<int> maxPlayerFood;
        public static ConfigEntry<int> maxPlayerWater;
        public static ConfigEntry<bool> toolsUItweaks;
        public static ConfigEntry<float> subtitlesDelay;


        public static void Bind()
        {  // “ ” ‛

            timeFlowSpeed = Main.configMenu.Bind("", "Time flow speed multiplier", 1f, "The higher the value the shorter days are. This also affects crafting time, building time, battery charging time.");
            foodLossMult = Main.configMenu.Bind("", "Food loss multiplier", 1f, "Food value you lose when your hunger updates will be multiplied by this.");
            waterLossMult = Main.configMenu.Bind("", "Water loss multiplier", 1f, "Water value you lose when your hunger updates will be multiplied by this.");
            playerWaterSpeedMult = Main.configMenu.Bind("Player movement", "Player speed multiplier in water", 1f);
            playerGroundSpeedMult = Main.configMenu.Bind("Player movement", "Player speed multiplier on ground", 1f);

            exosuitSpeedMult = Main.configMenu.Bind("", "Prawn suit speed multiplier", 1f, "");
            seamothSpeedMult = Main.configMenu.Bind("", "Seamoth speed multiplier", 1f, "");
            cyclopsSpeedMult = Main.configMenu.Bind("", "Cyclops speed multiplier", 1f, "");

            playerDamageMult = Main.configMenu.Bind("", "Player damage multiplier", 1f, "Amount of damage player takes will be multiplied by this.");
            vehicleDamageMult = Main.configMenu.Bind("", "Vehicle damage multiplier", 1f, "Amount of damage your vehicles take will be multiplied by this.");
            aggrMult = Main.configMenu.Bind("", "Predator aggression multiplier", 1f, "The higher it is the more aggressive predators are towards you. When it is 0, you and your vehicles will never be attacked. When it's more than 1 predators attack you from greater distance and more often.");
            oxygenPerBreath = Main.configMenu.Bind("", "Oxygen per breath", 3f, "Amount of oxygen you consume every breath.");
            toolEnergyConsMult = Main.configMenu.Bind("", "Tool power consumption multiplier", 1f, "Amount of power consumed by your tools will be multiplied by this.");
            vehicleEnergyConsMult = Main.configMenu.Bind("", "Vehicle power consumption multiplier", 1f, "Amount of power consumed by your vehicles will be multiplied by this.");
            baseEnergyConsMult = Main.configMenu.Bind("", "Base power consumption multiplier", 1f, "Amount of power consumed by your things in your base will be multiplied by this.");

            baseHullStrengthMult = Main.configMenu.Bind("", "Base hull strength multiplier", 1f, "");
            knifeRangeMult = Main.configMenu.Bind("", "Knife range multiplier", 1f, "Applies to knife and thermoblade. You have to reequip your knife after changing this.");
            knifeDamageMult = Main.configMenu.Bind("", "Knife damage multiplier", 1f, "Applies to knife and thermoblade. You have to reequip your knife after changing this.");
            medKitHP = Main.configMenu.Bind("", "First aid kit health", 50, "Health restored when using first aid kit.");
            craftTimeMult = Main.configMenu.Bind("", "Crafting time multiplier", 1f, "Crafting time will be multiplied by this when crafting things with fabricator or modification station.");
            buildTimeMult = Main.configMenu.Bind("", "Building time multiplier", 1f, "Building time will be multiplied by this when using builder tool.");
            seaglideSpeedMult = Main.configMenu.Bind("Player movement", "Seaglide speed multiplier", 1f, "");

            cyclopsFireChance = Main.configMenu.Bind("", "Cyclops engine room fire chance percent", 50, "The game starts checking this when you get your first engine overheat warning. After that every 10 seconds chance to catch fire goes up by 10% if you don't slow down.");
            cyclopsAutoHealHealthPercent = Main.configMenu.Bind("", "Cyclops auto-repair threshold", 90, "Cyclops auto-repairs when it is not on fire and its HP percent is above this.");
            crushDepth = Main.configMenu.Bind("", "Crush depth", 200, "Depth in meters below which player starts taking crush damage set by 'Crush damage' setting. ");
            crushDamage = Main.configMenu.Bind("", "Crush damage", 0f, "Player takes this damage when below crush depth.");
            crushDamageProgression = Main.configMenu.Bind("", "Crush damage progression", 0f, "If this is more than 0, the crush damage you take will be: 'Crush damage' value + 'Crush damage' value * this * number of meters below crush depth.");
            vehicleCrushDamageMult = Main.configMenu.Bind("", "Vehicle crush damage multiplier", 1f, "Vehicle crush damage will be multiplied by this.");
            emptyVehiclesCanBeAttacked = Main.configMenu.Bind("", "Unmanned vehicles can be attacked", EmptyVehiclesCanBeAttacked.Vanilla, "By default unmanned seamoth or prawn suit can be attacked but cyclops can not.");
            newHungerSystem = Main.configMenu.Bind("", "New hunger system", false, "When you sprint you get hungry and thirsty twice as fast. You don't lose health when your food or water value is 0. Your food and water values can go as low as -100. When your food or water value is below 0 your movement speed will be reduced proportionally to that value. When either your food or water value is -100 your movement speed will be reduced by 50% and you will start taking hunger damage. The higher your food value above 100 is the less food you get when eating: when your food value is 110 you lose 10% of food, when it is 190 you lose 90%.");
            fishFoodWaterRatio = Main.configMenu.Bind("", "Fish water/food value ratio", 0, "Fish's water value will be this percent of its food value if this is more than 0. Game has to be reloaded after changing this.");
            eatRawFish = Main.configMenu.Bind("", "Eating raw fish", EatingRawFish.Vanilla, "This changes amount of food you get by eating raw fish. Harmless: it is a random number between 0 and fish's food value. Risky: it is a random number between fish's negative food value and fish's food value. Harmful: it is a random number between fish's negative food value and 0.");
            cantEatUnderwater = Main.configMenu.Bind("", "Can not eat underwater", false, "You will not be able to eat or drink when swimming underwater if this is on.");
            cantUseMedkitUnderwater = Main.configMenu.Bind("", "Can not use first aid kit underwater", false, "You will not be able to use first aid kit when swimming underwater if this is on.");
            foodDecayRateMult = Main.configMenu.Bind("", "Food decay rate multiplier", 1f, "Food decay rate will be multiplied by this. You have to reload the game after changing this.");
            fruitGrowTime = Main.configMenu.Bind("", "Fruit growth time", 0, "Time in days it takes a lantern tree fruit, creepvine seeds, blood oil to grow. Vanilla values will be used if this is 0: fruits from wild plants will never grow back after ypu pick them, fruits from your plants will grow in less than a day. You have to reload your game after changing this.");
            fishSpeedMult = Main.configMenu.Bind("", "Catchable fish speed multiplier", 1f, "Swimming speed of fish that you can catch will be multiplied by this.");
            creatureSpeedMult = Main.configMenu.Bind("", "Creature speed multiplier", 1f, "Swimming speed of creatures that you can not catch will be multiplied by this.");
            CreatureFleeChance = Main.configMenu.Bind("", "Creature flee chance percent", 100, "Creature's flee chance percent when it is under attack and its flee damage threshold is reached.");
            creatureFleeUseDamageThreshold = Main.configMenu.Bind("", "Damage threshold for fleeing creatures", true, "Most creatures have damage threshold that has to be reached before they start fleeing. When this is off, every creature will flee if it takes any damage.");
            creatureFleeChanceBasedOnHealth = Main.configMenu.Bind("", "Creature flee chance depends on its health", false, "Creatures's health will be used to decide if it should flee when under attack. Creature with 90% health has 10% chance to flee. Creature with 10% health has 90% chance to flee. This setting overrides both 'Creature flee chance percent' and 'Damage threshold for fleeing creatures'.");
            waterparkCreaturesBreed = Main.configMenu.Bind("", "Creatures in alien containment can breed", true);
            noFishCatching = Main.configMenu.Bind("", "Can not catch fish with bare hands", false, "To catch fish you will have to use knife, propulsion cannon, stasis rifle or grav trap. Does not apply if you are inside alien containment.");
            noBreakingWithHand = Main.configMenu.Bind("", "Can not break outcrop with bare hands", false, "You will have to use a knife, repulsion cannon or propulsion cannon to break outcrops or collect resources attached to rock or seabed.");
            damageImpactEffect = Main.configMenu.Bind("", "Player impact damage screen effects", true, "This toggles cracks on your swimming mask when you take damage.");
            damageScreenFX = Main.configMenu.Bind("", "Player damage screen effects", true, "This toggles red screen effect when you take damage.");
            stalkerLoseToothChance = Main.configMenu.Bind("", "Chance percent for stalker to lose its tooth", 50, "Probability percent for stalker to lose its tooth when it bites something hard.");
            realOxygenCons = Main.configMenu.Bind("", "Realistic oxygen consumption", false, "Vanilla oxygen consumption without rebreather has 3 levels: depth below 200 meters, depth between 200 and 100 meters, depth above 100 meters. With this on your oxygen consumption will increase in linear progression using 'Crush depth' setting. When you are at crush depth it will be equal to vanilla max oxygen consumption and will increase as you dive deeper.");
            decoyLifeTime = Main.configMenu.Bind("", "Creature decoy battery life time", 90, "Creature decoy stops working after this number of seconds.");
            decoyHP = Main.configMenu.Bind("", "Creature decoy HP", 0, "Creature decoy will be destroyed after taking this amount of damage if this value is not 0. By default creature decoy HP is infinite.");
            escapePodMaxPower = Main.configMenu.Bind("", "Life pod power cell max charge", 25, "Max charge for each of its 3 power cells. Game has to be reloaded after changing this.");
            batteryChargeMult = Main.configMenu.Bind("", "Battery charge multiplier", 1f, "Max charge of batteries and power cells will be multiplied by this. Game has to be reloaded after changing this.");
            craftedBatteryCharge = Main.configMenu.Bind("", "Crafted battery charge percent", 100, "Charge percent of batteries and power cells you craft will be set to this.");
            dropItemsOnDeath = Main.configMenu.Bind("", "Drop items when you die", DropItemsOnDeath.Vanilla);
            invMultWater = Main.configMenu.Bind("", "Inventory weight multiplier in water", 0f, "When this is not 0 and you are swimming you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.");
            invMultLand = Main.configMenu.Bind("", "Inventory weight multiplier on ground", 0f, "When this is not 0 and you are on land you lose 1% of your max speed for every kilo of mass in your inventory multiplied by this.");
            drillDamageMult = Main.configMenu.Bind("", "Prawn suit drill arm damage multiplier", 1f, "You have to reload the game after changing this");
            foodWaterHealThreshold = Main.configMenu.Bind("", "Food heal threshold", 150, "Your health regenerates when sum of your food and water values is greater than this");
            maxPlayerFood = Main.configMenu.Bind("", "Max player food", (int)SurvivalConstants.kMaxOverfillStat, "Your food meter will be capped at this.");
            maxPlayerWater = Main.configMenu.Bind("", "Max player water", (int)SurvivalConstants.kMaxStat, "Your water meter will be capped at this.");
            subtitlesDelay = Main.configMenu.Bind("", "Subtitles delay", 0f, "Audio clips may be played with a delay when you have mods installed. Use this to set delay in seconds for subtitles so they are not shown too early.");


        }

        public enum DropItemsOnDeath { Vanilla, Drop_everything, Do_not_drop_anything }
        public enum EmptyVehiclesCanBeAttacked { Vanilla, Yes, No, Only_if_lights_on }
        public enum EatingRawFish { Vanilla, Harmless, Risky, Harmful }
    }
}
