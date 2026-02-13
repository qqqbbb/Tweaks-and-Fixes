using BepInEx;
using BepInEx.Configuration;
using ICSharpCode.SharpZipLib.Zip.Compression;
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
        public static ConfigEntry<int> fishFoodWaterRatio;
        public static ConfigEntry<EatingRawFish> eatRawFish;
        public static ConfigEntry<bool> cantEatUnderwater;
        public static ConfigEntry<bool> cantUseMedkitUnderwater;
        public static ConfigEntry<float> foodDecayRateMult;
        //public static ConfigEntry<int> fruitGrowTime;
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
        public static ConfigEntry<int> foodHealThreshold;

        //public static ConfigEntry<bool> toolsUItweaks;
        public static ConfigEntry<float> subtitlesDelay;


        public static void Bind()
        {

            timeFlowSpeed = Main.configMenu.Bind("", "TF_time_flow", 1f, "TF_time_flow_desc");
            foodLossMult = Main.configMenu.Bind("", "TF_food_loss_mult", 1f, "TF_food_loss_mult_desc");
            waterLossMult = Main.configMenu.Bind("", "TF_water_loss_mult", 1f, "TF_water_loss_mult_desc");
            playerWaterSpeedMult = Main.configMenu.Bind("Player_movement", "TF_player_speed_mult_water", 1f);
            playerGroundSpeedMult = Main.configMenu.Bind("Player_movement", "TF_player_speed_mult_ground", 1f);
            exosuitSpeedMult = Main.configMenu.Bind("", "TF_prawn_suit_speed_mult", 1f, "");
            seamothSpeedMult = Main.configMenu.Bind("", "TF_seamoth_speed_mult", 1f, "");
            cyclopsSpeedMult = Main.configMenu.Bind("", "TF_cyclops_speed_mult", 1f, "");
            playerDamageMult = Main.configMenu.Bind("", "TF_player_damage_mult", 1f, "TF_player_damage_mult_desc");
            vehicleDamageMult = Main.configMenu.Bind("", "TF_vehicle_damage_mult", 1f, "TF_vehicle_damage_mult_desc");
            aggrMult = Main.configMenu.Bind("", "TF_predator_aggr_mult", 1f, "TF_predator_aggr_mult_desc");
            oxygenPerBreath = Main.configMenu.Bind("", "TF_oxygen_per_breath", 3f);
            toolEnergyConsMult = Main.configMenu.Bind("", "TF_tool_power_mult", 1f, "TF_tool_power_mult_desc");
            vehicleEnergyConsMult = Main.configMenu.Bind("", "TF_vehicle_power_mult", 1f, "TF_vehicle_power_mult_desc");
            baseEnergyConsMult = Main.configMenu.Bind("", "TF_base_power_mult", 1f, "TF_base_power_mult_desc");
            baseHullStrengthMult = Main.configMenu.Bind("", "TF_base_hull_strength_mult", 1f, "");
            knifeRangeMult = Main.configMenu.Bind("", "TF_knife_range_mult", 1f, "TF_knife_damage_mult_desc");
            knifeDamageMult = Main.configMenu.Bind("", "TF_knife_damage_mult", 1f, "TF_knife_damage_mult_desc");
            medKitHP = Main.configMenu.Bind("", "TF_med_kit_health", 50, "TF_med_kit_health_desc");
            craftTimeMult = Main.configMenu.Bind("", "TF_crafting_time_mult", 1f, "TF_crafting_time_mult_desc");
            buildTimeMult = Main.configMenu.Bind("", "TF_building_time_mult", 1f, "TF_building_time_mult_desc");
            seaglideSpeedMult = Main.configMenu.Bind("Player movement", "TF_seaglide_speed_mult", 1f, "");

            cyclopsFireChance = Main.configMenu.Bind("", "TF_cyclops_fire_chance", 50, "TF_cyclops_fire_chance_desc");
            cyclopsAutoHealHealthPercent = Main.configMenu.Bind("", "TF_cyclops_auto_repair", 90, "TF_cyclops_auto_repair_desc");
            crushDepth = Main.configMenu.Bind("", "TF_crush_depth", 200, "TF_crush_depth_desc");
            crushDamage = Main.configMenu.Bind("", "TF_crush_damage", 0f, "TF_crush_damage_desc");
            crushDamageProgression = Main.configMenu.Bind("", "TF_crush_damage_progression", 0f, "TF_crush_damage_progression_desc");
            vehicleCrushDamageMult = Main.configMenu.Bind("", "TF_vehicle_crush_damage_mult", 1f);
            emptyVehiclesCanBeAttacked = Main.configMenu.Bind("", "TF_unmanned_vehicles_attack", EmptyVehiclesCanBeAttacked.TF_default_setting, "TF_unmanned_vehicles_attack_desc");

            fishFoodWaterRatio = Main.configMenu.Bind("", "TF_fish_water_food_value_ratio", 0, "TF_fish_water_food_value_ratio_desc");
            eatRawFish = Main.configMenu.Bind("", "TF_eating_raw_fish", EatingRawFish.TF_default_setting, "TF_eating_raw_fish_desc");
            cantEatUnderwater = Main.configMenu.Bind("", "TF_can_not_eat_underwater", false, "TF_can_not_eat_underwater_desc");
            cantUseMedkitUnderwater = Main.configMenu.Bind("", "TF_can_not_use_med_kit_underwater", false, "TF_can_not_use_med_kit_underwater_desc");
            foodDecayRateMult = Main.configMenu.Bind("", "TF_food_decay_rate_mult", 1f, "TF_reload_game");
            fishSpeedMult = Main.configMenu.Bind("", "TF_fish_speed_mult", 1f, "TF_fish_speed_mult_desc");
            creatureSpeedMult = Main.configMenu.Bind("", "TF_creature_speed_mult", 1f, "TF_creature_speed_mult_desc");
            CreatureFleeChance = Main.configMenu.Bind("", "TF_creature_flee_chance_percent", 100, "TF_creature_flee_chance_percent_desc");
            creatureFleeUseDamageThreshold = Main.configMenu.Bind("", "TF_creature_flee_damage_threshold", true, "TF_creature_flee_damage_threshold_desc");
            creatureFleeChanceBasedOnHealth = Main.configMenu.Bind("", "TF_creature_flee_chance_depends_on_its_health", false, "TF_creature_flee_chance_depends_on_its_health_desc");
            waterparkCreaturesBreed = Main.configMenu.Bind("", "TF_creatures_in_alien_containment_can_breed", true);
            noFishCatching = Main.configMenu.Bind("", "TF_can_not_catch_fish_with_bare_hand", false, "TF_can_not_catch_fish_with_bare_hand_desc");
            noBreakingWithHand = Main.configMenu.Bind("", "TF_can_not_break_outcrop_with_bare_hand", false, "TF_can_not_break_outcrop_with_bare_hand_desc");
            damageImpactEffect = Main.configMenu.Bind("", "TF_player_impact_damage_screen_effects", true, "TF_player_impact_damage_screen_effects_desc");
            damageScreenFX = Main.configMenu.Bind("", "TF_player_damage_screen_effects", true, "TF_player_damage_screen_effects_desc");
            stalkerLoseToothChance = Main.configMenu.Bind("", "TF_stalker_lose_tooth_chance_percent", 50, "TF_stalker_lose_tooth_chance_percent_desc");
            realOxygenCons = Main.configMenu.Bind("", "TF_realistic_oxygen_consumption", false, "TF_realistic_oxygen_consumption_desc");
            decoyLifeTime = Main.configMenu.Bind("", "TF_creature_decoy_battery_life_time", 90, "TF_creature_decoy_battery_life_time_desc");
            decoyHP = Main.configMenu.Bind("", "TF_creature_decoy_hp", 0, "TF_creature_decoy_hp_desc");
            escapePodMaxPower = Main.configMenu.Bind("", "TF_life_pod_power_cell_max_charge", 25, "TF_life_pod_power_cell_max_charge_desc");
            batteryChargeMult = Main.configMenu.Bind("", "TF_battery_charge_mult", 1f);
            craftedBatteryCharge = Main.configMenu.Bind("", "TF_crafted_battery_charge_percent", 100, "TF_crafted_battery_charge_percent_desc");
            dropItemsOnDeath = Main.configMenu.Bind("", "TF_drop_items_when_you_die", DropItemsOnDeath.TF_default_setting);
            invMultLand = Main.configMenu.Bind("", "TF_inventory_weight_mult_ground", 0f, "TF_inventory_weight_mult_ground_desc");
            string invWater = "TF_inventory_weight_mult_water";
            if (Language.main.currentLanguage == "English")
                invWater = "Inventory weight multiplier in water";
            invMultWater = Main.configMenu.Bind("", invWater, 0f, "TF_inventory_weight_mult_water_desc");
            drillDamageMult = Main.configMenu.Bind("", "TF_prawn_suit_drill_arm_damage_mult", 1f, "TF_reload_game");
            foodHealThreshold = Main.configMenu.Bind("", "TF_food_heal_threshold", 150, "TF_food_heal_threshold_desc");
            subtitlesDelay = Main.configMenu.Bind("", "TF_subtitles_delay", 0f, "TF_subtitles_delay_desc");


        }

        public enum DropItemsOnDeath { TF_default_setting, TF_drop_items_death_setting_everything, TF_drop_items_death_setting_nothing }
        public enum EmptyVehiclesCanBeAttacked { TF_default_setting, Yes, No, TF_empty_vehicle_can_be_attacked_setting_light }
        public enum EatingRawFish { TF_default_setting, TF_eat_raw_fish_setting_harmless, TF_eat_raw_fish_setting_risky, TF_eat_raw_fish_setting_harmful }

    }
}
