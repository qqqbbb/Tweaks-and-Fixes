using Nautilus.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class OptionsMenu : ModOptions
    {
        public OptionsMenu() : base("Tweaks and Fixes")
        {
            ModSliderOption timeFlowSpeedSlider = ConfigMenu.timeFlowSpeed.ToModSliderOption(.1f, 10f, .1f, "{0:0.#}");
            ModSliderOption playerSpeedSlider = ConfigMenu.playerSpeedMult.ToModSliderOption(.5f, 5f, .1f, "{0:0.#}");
            timeFlowSpeedSlider.OnChanged += UpdateTimeSpeed;
            ModSliderOption playerDamageSlider = ConfigMenu.playerDamageMult.ToModSliderOption(0f, 2f, .1f, "{0:0.#}");
            ModSliderOption vehicleDamageSlider = ConfigMenu.vehicleDamageMult.ToModSliderOption(0f, 2f, .1f, "{0:0.#}");
            ModSliderOption aggrSlider = ConfigMenu.aggrMult.ToModSliderOption(0f, 2f, .1f, "{0:0.#}");
            ModSliderOption oxygenSlider = ConfigMenu.oxygenPerBreath.ToModSliderOption(0f, 6f, .1f, "{0:0.#}");
            ModSliderOption toolEnergySlider = ConfigMenu.toolEnergyConsMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption vehicleEnergySlider = ConfigMenu.vehicleEnergyConsMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption baseEnergySlider = ConfigMenu.baseEnergyConsMult.ToModSliderOption(0f, 3f, .1f, "{0:0.#}");
            ModSliderOption knifeRangeSlider = ConfigMenu.knifeRangeMult.ToModSliderOption(1f, 5f, .1f, "{0:0.#}");
            ModSliderOption knifeDamageSlider = ConfigMenu.knifeDamageMult.ToModSliderOption(1f, 5f, .1f, "{0:0.#}");
            ModSliderOption medKitHPslider = ConfigMenu.medKitHP.ToModSliderOption(10, 100, 1);
            ModSliderOption craftTimeSlider = ConfigMenu.craftTimeMult.ToModSliderOption(0.01f, 3f, .01f, "{0:0.0#}");
            ModSliderOption buildTimeSlider = ConfigMenu.buildTimeMult.ToModSliderOption(0.01f, 3f, .01f, "{0:0.0#}");
            ModSliderOption cyclopsFireChanceSlider = ConfigMenu.cyclopsFireChance.ToModSliderOption(0, 100, 1);
            ModSliderOption cyclopsAutoHealSlider = ConfigMenu.cyclopsAutoHealHealthPercent.ToModSliderOption(0, 100, 1);
            ModSliderOption crushDepthSlider = ConfigMenu.crushDepth.ToModSliderOption(50, 500, 10);
            ModSliderOption crushDamageSlider = ConfigMenu.crushDamage.ToModSliderOption(0f, 10f, .1f, "{0:0.0#}");
            ModSliderOption vehicleCrushDamageSlider = ConfigMenu.vehicleCrushDamageMult.ToModSliderOption(0f, 10f, .1f, "{0:0.0#}");
            ModSliderOption crushDamageProgressionSlider = ConfigMenu.crushDamageProgression.ToModSliderOption(0f, 1f, .01f, "{0:0.0#}");
            ModSliderOption hungerUpdateIntervalSlider = ConfigMenu.hungerUpdateInterval.ToModSliderOption(1, 100, 1);
            ModSliderOption fishFoodWaterRatioSlider = ConfigMenu.fishFoodWaterRatio.ToModSliderOption(0f, 1f, .01f, "{0:0.0#}");
            ModSliderOption foodDecayRateSlider = ConfigMenu.foodDecayRateMult.ToModSliderOption(0f, 3f, .01f, "{0:0.0#}");
            ModSliderOption fruitGrowTimeSlider = ConfigMenu.fruitGrowTime.ToModSliderOption(0, 30, 1);
            ModSliderOption fishSpeedSlider = ConfigMenu.fishSpeedMult.ToModSliderOption(0.1f, 5f, .1f, "{0:0.#}");
            ModSliderOption creatureSpeedSlider = ConfigMenu.creatureSpeedMult.ToModSliderOption(0.1f, 5f, .1f, "{0:0.#}");
            ModSliderOption CreatureFleeChanceSlider = ConfigMenu.CreatureFleeChance.ToModSliderOption(0, 100, 1);
            ModSliderOption stalkerLoseToothChanceSlider = ConfigMenu.stalkerLoseToothChance.ToModSliderOption(0, 100, 1);
            ModSliderOption decoyLifeTimeSlider = ConfigMenu.decoyLifeTime.ToModSliderOption(10, 500, 10);
            ModSliderOption decoyHPslider = ConfigMenu.decoyHP.ToModSliderOption(0, 500, 10);
            ModSliderOption escapePodMaxPowerSlider = ConfigMenu.escapePodMaxPower.ToModSliderOption(10, 100, 1);
            ModSliderOption batteryChargeSlider = ConfigMenu.batteryChargeMult.ToModSliderOption(0.5f, 3f, .1f, "{0:0.#}");
            ModSliderOption craftedBatteryChargeSlider = ConfigMenu.craftedBatteryCharge.ToModSliderOption(0, 100, 1);
            ModSliderOption invMultWaterSlider = ConfigMenu.invMultWater.ToModSliderOption(0f, 1f, .01f, "{0:0.0#}");
            ModSliderOption invMultLandSlider = ConfigMenu.invMultLand.ToModSliderOption(0f, 1f, .01f, "{0:0.0#}");
            ModSliderOption baseHullStrengthSlider = ConfigMenu.baseHullStrengthMult.ToModSliderOption(1f, 10f, .1f, "{0:0.#}");
            ModSliderOption drillDamageMultSlider = ConfigMenu.drillDamageMult.ToModSliderOption(1f, 10f, .1f, "{0:0.#}");

            AddItem(timeFlowSpeedSlider);
            AddItem(playerSpeedSlider);
            AddItem(playerDamageSlider);
            AddItem(vehicleDamageSlider);
            AddItem(aggrSlider);
            AddItem(oxygenSlider);
            AddItem(toolEnergySlider);
            AddItem(vehicleEnergySlider);
            AddItem(baseEnergySlider);
            AddItem(knifeRangeSlider);
            AddItem(knifeDamageSlider);
            AddItem(medKitHPslider);
            AddItem(craftTimeSlider);
            AddItem(buildTimeSlider);
            AddItem(ConfigMenu.playerMoveTweaks.ToModToggleOption());
            AddItem(ConfigMenu.seamothMoveTweaks.ToModToggleOption());
            AddItem(ConfigMenu.exosuitMoveTweaks.ToModToggleOption());
            AddItem(ConfigMenu.cyclopsMoveTweaks.ToModToggleOption());
            AddItem(drillDamageMultSlider);
            AddItem(cyclopsFireChanceSlider);
            AddItem(cyclopsAutoHealSlider);
            AddItem(crushDepthSlider);
            AddItem(crushDamageSlider);
            AddItem(vehicleCrushDamageSlider);
            AddItem(crushDamageProgressionSlider);

            AddItem(baseHullStrengthSlider);
            AddItem(ConfigMenu.emptyVehiclesCanBeAttacked.ToModChoiceOption());
            AddItem(hungerUpdateIntervalSlider);
            AddItem(ConfigMenu.newHungerSystem.ToModToggleOption());
            AddItem(fishFoodWaterRatioSlider);
            AddItem(ConfigMenu.eatRawFish.ToModChoiceOption());
            AddItem(ConfigMenu.cantEatUnderwater.ToModToggleOption());
            AddItem(ConfigMenu.cantUseMedkitUnderwater.ToModToggleOption());
            AddItem(foodDecayRateSlider);
            AddItem(fruitGrowTimeSlider);
            AddItem(fishSpeedSlider);
            AddItem(creatureSpeedSlider);
            AddItem(CreatureFleeChanceSlider);
            AddItem(ConfigMenu.creatureFleeUseDamageThreshold.ToModToggleOption());
            AddItem(ConfigMenu.creatureFleeChanceBasedOnHealth.ToModToggleOption());
            AddItem(ConfigMenu.waterparkCreaturesBreed.ToModToggleOption());
            AddItem(ConfigMenu.noFishCatching.ToModToggleOption());
            AddItem(ConfigMenu.noBreakingWithHand.ToModToggleOption());
            AddItem(ConfigMenu.damageImpactEffect.ToModToggleOption());
            AddItem(ConfigMenu.damageScreenFX.ToModToggleOption());
            AddItem(stalkerLoseToothChanceSlider);
            //AddItem(ConfigMenu.disableHints.ToModToggleOption());
            AddItem(ConfigMenu.realOxygenCons.ToModToggleOption());
            //AddItem(ConfigMenu.cameraBobbing.ToModToggleOption());
            AddItem(decoyLifeTimeSlider);
            AddItem(decoyHPslider);
            AddItem(escapePodMaxPowerSlider);
            AddItem(batteryChargeSlider);
            AddItem(craftedBatteryChargeSlider);
            AddItem(ConfigMenu.dropItemsOnDeath.ToModChoiceOption());
            AddItem(invMultWaterSlider);
            AddItem(invMultLandSlider);
            AddItem(ConfigMenu.transferAllItemsButton.ToModKeybindOption());
            AddItem(ConfigMenu.transferSameItemsButton.ToModKeybindOption());
            AddItem(ConfigMenu.lightButton.ToModKeybindOption());
            AddItem(ConfigMenu.quickslotButton.ToModKeybindOption());
            AddItem(ConfigMenu.previousPDATabKey.ToModKeybindOption());
            AddItem(ConfigMenu.nextPDATabKey.ToModKeybindOption());

        }

        void UpdateTimeSpeed(object sender, SliderChangedEventArgs e)
        {
            //AddDebug("UpdateTimeSpeed");
            if (DayNightCycle.main)
                DayNightCycle.main._dayNightSpeed = ConfigMenu.timeFlowSpeed.Value;
        }

    }
}
