using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Survival_
    {
        public static HashSet<Pickupable> cookedFish = new HashSet<Pickupable> { };
        static bool updatingStats;
        private static bool usingMedkit;

        public static float UpdateStats(Survival survival, float timePassed)
        {
            float oldFood = survival.food;
            float oldWater = survival.water;
            float foodToLose = timePassed / SurvivalConstants.kFoodTime * SurvivalConstants.kMaxStat;
            float waterToLose = timePassed / SurvivalConstants.kWaterTime * SurvivalConstants.kMaxStat;

            if (Player.main.mode == Player.Mode.Normal && Player.main.IsUnderwaterForSwimming() == false && Player.main.groundMotor.IsGrounded() && Player.main.groundMotor.IsSprinting())
            {
                foodToLose *= 2;
                waterToLose *= 2;
            }
            //AddDebug("UpdateStats foodToLose " + foodToLose);
            survival.food -= foodToLose * ConfigMenu.foodLossMult.Value;
            survival.water -= waterToLose * ConfigMenu.waterLossMult.Value;
            float foodDamage = 0f;

            if (survival.food < -100f)
            {
                foodDamage = Mathf.Abs(survival.food + 100f);
                survival.food = -100f;
            }
            if (survival.water < -100f)
            {
                foodDamage += Mathf.Abs(survival.water + 100f);
                survival.water = -100f;
            }
            //if (foodDamage > 0)
            //    Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);

            float threshold1 = ConfigMenu.newHungerSystem.Value ? 0f : 20f;
            float threshold2 = ConfigMenu.newHungerSystem.Value ? -50f : 10f;
            survival.UpdateWarningSounds(survival.foodWarningSounds, survival.food, oldFood, threshold1, threshold2);
            survival.UpdateWarningSounds(survival.waterWarningSounds, survival.water, oldWater, threshold1, threshold2);
            //hungerUpdateTime = Time.time + ConfigMenu.hungerUpdateInterval.Value;
            //AddDebug("Invoke  hungerUpdateInterval " + Main.config.hungerUpdateInterval);
            //AddDebug("Invoke dayNightSpeed " + DayNightCycle.main.dayNightSpeed);
            //__instance.Invoke("UpdateHunger", updateHungerInterval);
            return foodDamage;
        }

        [HarmonyPatch(typeof(Survival))]
        class Survival_patch
        {
            static float foodBeforeUpdate;
            static float waterBeforeUpdate;

            [HarmonyPostfix, HarmonyPatch("Start")]
            static void StartPostfix(Survival __instance)
            {
                if (ConfigToEdit.consistentHungerUpdateTime.Value)
                {
                    __instance.CancelInvoke();
                    __instance.StartCoroutine(UpdateHunger(__instance));
                }
            }

            static IEnumerator UpdateHunger(Survival survival)
            {
                while (ConfigToEdit.consistentHungerUpdateTime.Value)
                {
                    yield return new WaitForSeconds(GetHungerUpdateTime(survival));
                    //AddDebug("UpdateHunger");
                    survival.UpdateHunger();
                }
            }

            private static float GetHungerUpdateTime(Survival survival)
            {
                return survival.kUpdateHungerInterval / DayNightCycle.main._dayNightSpeed;
            }

            [HarmonyPrefix, HarmonyPatch("UpdateWarningSounds")]
            static bool UpdateWarningSoundsPrefix(Survival __instance)
            {
                //AddDebug("UpdateWarningSounds ");
                if (ConfigMenu.foodLossMult.Value == 1 && ConfigMenu.waterLossMult.Value == 1)
                    return true;

                return !updatingStats;
            }

            public static float GetfoodWaterHealThreshold()
            {
                //Main.logger.LogMessage("GetfoodWaterHealThreshold " + ConfigMenu.foodWaterHealThreshold.Value);
                return ConfigMenu.foodWaterHealThreshold.Value;
            }

            [HarmonyPatch("UpdateHunger"), HarmonyTranspiler]
            static IEnumerable<CodeInstruction> UpdateHungerTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codeMatcher = new CodeMatcher(instructions)
             .MatchForward(false, new CodeMatch(OpCodes.Ldc_R4, SurvivalConstants.kFoodWaterHealThreshold))
             .ThrowIfInvalid("Could not find Ldc_R4 SurvivalConstants.kFoodWaterHealThreshold in UpdateHunger")
             .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<float>>(GetfoodWaterHealThreshold))
             .InstructionEnumeration();
                return codeMatcher;
            }

            [HarmonyPrefix, HarmonyPatch("UpdateStats")]
            static bool UpdateStatsPrefix(Survival __instance, ref float timePassed, ref float __result)
            {
                if (Main.gameLoaded == false)
                    return false;

                //AddDebug("UpdateStats ");
                updatingStats = true;
                foodBeforeUpdate = __instance.food;
                waterBeforeUpdate = __instance.water;
                //if (ConfigToEdit.consistantHungerUpdateTime.Value)
                //    timePassed *= DayNightCycle.main._dayNightSpeed;

                if (ConfigMenu.newHungerSystem.Value)
                {
                    __result = UpdateStats(__instance, timePassed);
                    return false;
                }
                return true;
            }

            [HarmonyPostfix, HarmonyPatch("UpdateStats")]
            static void UpdateStatsPostfix(Survival __instance, float timePassed, ref float __result)
            {
                if (ConfigMenu.foodLossMult.Value == 1 && ConfigMenu.waterLossMult.Value == 1)
                    return;

                float damage = 0;
                if (timePassed > Mathf.Epsilon)
                {
                    //float foodLost = foodBeforeUpdate - __instance.food;
                    //float waterLost = waterBeforeUpdate - __instance.water;
                    float foodToLose = (timePassed / SurvivalConstants.kFoodTime * SurvivalConstants.kMaxStat);
                    foodToLose *= ConfigMenu.foodLossMult.Value;
                    if (foodToLose > foodBeforeUpdate)
                        damage += ((foodToLose - foodBeforeUpdate) * SurvivalConstants.kStarveDamage);

                    __instance.food = Mathf.Clamp(foodBeforeUpdate - foodToLose, 0, SurvivalConstants.kMaxStat * 2f);
                    float waterToLose = (timePassed / SurvivalConstants.kWaterTime * SurvivalConstants.kMaxStat);
                    waterToLose *= ConfigMenu.waterLossMult.Value;
                    //AddDebug("foodToLose " + foodToLose);
                    //AddDebug("waterToLose " + waterToLose);
                    if (waterToLose > waterBeforeUpdate)
                        damage += ((waterToLose - waterBeforeUpdate) * SurvivalConstants.kStarveDamage);

                    __instance.water = Mathf.Clamp(waterBeforeUpdate - waterToLose, 0, SurvivalConstants.kMaxStat);
                    updatingStats = false;
                    __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, foodBeforeUpdate, SurvivalConstants.kLowFoodThreshold, SurvivalConstants.kCriticalFoodThreshold);
                    __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, waterBeforeUpdate, SurvivalConstants.kLowWaterThreshold, SurvivalConstants.kCriticalWaterThreshold);
                }
                //AddDebug("UpdateStats food " + __instance.food);
                //AddDebug("UpdateStats water " + __instance.water);
                __result = damage;
            }
            //!!!            //[HarmonyPrefix] // does not run
            //[HarmonyPatch("GetWeaknessSpeedScalar")]
            public static bool GetWeaknessSpeedScalarPrefix(Survival __instance, ref float __result)
            {
                if (!ConfigMenu.newHungerSystem.Value)
                    return true;

                float foodMult = 1f;
                float waterMult = 1f;
                if (Main.survival.food < 0f)
                {
                    foodMult = Mathf.Abs(Main.survival.food / 100f);
                    foodMult = 1f - foodMult;
                }
                if (Main.survival.water < 0f)
                {
                    waterMult = Mathf.Abs(Main.survival.water / 100f);
                    waterMult = 1f - waterMult;
                }
                __result = (foodMult + waterMult) * .5f;
                //AddDebug("WeaknessSpeedScalar " + __result);
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Eat")]
            public static bool EatPrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla && !ConfigMenu.newHungerSystem.Value && ConfigMenu.maxPlayerWater.Value == 100 && ConfigMenu.maxPlayerFood.Value == 200)
                    return true;

                Eatable eatable = useObj.GetComponent<Eatable>();
                int food = (int)eatable.foodValue;
                int water = (int)eatable.waterValue;
                int playerMinFood = ConfigMenu.newHungerSystem.Value ? -100 : 0;
                float playerMaxWater = ConfigMenu.maxPlayerWater.Value;
                float playerMaxFood = ConfigMenu.maxPlayerFood.Value;
                int minFood = food;
                int maxFood = food;
                int minWater = water;
                int maxWater = water;
                TechType techType = CraftData.GetTechType(useObj);
                if (techType == TechType.None)
                {
                    Pickupable p = useObj.GetComponent<Pickupable>();
                    if (p)
                        techType = p.GetTechType();
                }
                if (Util.IsEatableFish(useObj))
                {
                    if (food > 0)
                    {
                        if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla)
                        {
                            minFood = food;
                            maxFood = food;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                        {
                            minFood = 0;
                            maxFood = food;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                        {
                            minFood = -food;
                            maxFood = food;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                        {
                            minFood = -food;
                            maxFood = 0;
                        }
                    }
                    if (water > 0)
                    {
                        if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla)
                        {
                            minWater = water;
                            maxWater = water;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                        {
                            minWater = 0;
                            maxWater = water;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                        {
                            minWater = -water;
                            maxWater = water;
                        }
                        else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                        {
                            minWater = -water;
                            maxWater = 0;
                        }
                    }
                }
                int rndFood = UnityEngine.Random.Range(minFood, maxFood);
                float finalFood = Mathf.Min(food, rndFood);
                if (ConfigMenu.newHungerSystem.Value && __instance.food > 100f && finalFood > 0)
                {
                    float mult = (playerMaxFood - __instance.food) * .01f;
                    finalFood *= mult;
                }
                int rndWater = UnityEngine.Random.Range(minWater, maxWater);
                float finalWater = Mathf.Min(water, rndWater);
                if (ConfigMenu.newHungerSystem.Value && __instance.water > 100f && finalWater > 0)
                {
                    float mult = (playerMaxWater - __instance.water) * .01f;
                    finalWater *= mult;
                }
                if (finalFood < 0 && __instance.food + finalFood < playerMinFood)
                {
                    int foodDamage = Mathf.Abs((int)(__instance.food + finalFood - playerMinFood));
                    //AddDebug("foodDamage " + foodDamage);
                    Player.main.liveMixin.TakeDamage(foodDamage, Player.main.transform.position, DamageType.Starve);
                }
                if (finalWater < 0 && __instance.water + finalWater < playerMinFood)
                {
                    int waterDamage = Mathf.Abs((int)(__instance.water + finalWater - playerMinFood));
                    //AddDebug("waterDamage " + waterDamage);
                    Player.main.liveMixin.TakeDamage(waterDamage, Player.main.transform.position, DamageType.Starve);
                }
                __instance.onEat.Trigger((float)finalFood);
                __instance.food += finalFood;
                __instance.onDrink.Trigger((float)finalWater);
                __instance.water += finalWater;
                //AddDebug("finalWater " + finalWater);
                //AddDebug("finalFood " + finalFood);

                if (finalFood > 0f)
                    GoalManager.main.OnCustomGoalEvent("Eat_Something");
                if (finalWater > 0f)
                    GoalManager.main.OnCustomGoalEvent("Drink_Something");

                if (techType == TechType.Bladderfish)
                    Player.main.GetComponent<OxygenManager>().AddOxygen(SurvivalConstants.kBladderFishO2OnEat);

                __instance.water = Mathf.Clamp(__instance.water, playerMinFood, playerMaxWater);
                __instance.food = Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);
                int warn = ConfigMenu.newHungerSystem.Value ? 0 : 20;
                if (finalWater > 0 && __instance.water > warn && __instance.water - finalWater < warn)
                    __instance.vitalsOkNotification.Play();
                else if (finalFood > 0 && __instance.food > warn && __instance.food - finalWater < warn)
                    __instance.vitalsOkNotification.Play();

                FMODUWE.PlayOneShot(CraftData.GetUseEatSound(techType), Player.main.transform.position);

                __result = true;
                return false;
            }

            [HarmonyPrefix, HarmonyPatch("Use")]
            public static void UsePrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                TechType techType = CraftData.GetTechType(useObj);
                //Main.logger.LogMessage("Survival Use " + techType);
                if (techType == TechType.FirstAidKit)
                {

                    usingMedkit = true;
                    Poison_Damage.RemovePoison();
                }
            }
        }

        [HarmonyPatch(typeof(LiveMixin), "AddHealth")]
        class LiveMixin_AddHealth_patch
        {
            public static bool Prefix(LiveMixin __instance, ref float healthBack, ref float __result)
            {
                if (usingMedkit == false)
                    return true;

                usingMedkit = false;
                if (ConfigToEdit.medKitHPperSecond.Value >= ConfigMenu.medKitHP.Value)
                    healthBack = ConfigMenu.medKitHP.Value;
                else
                {
                    Main.configMain.SetHPtoHeal(ConfigMenu.medKitHP.Value);
                    Player_.healTime = Time.time;
                    __result = ConfigMenu.medKitHP.Value;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Eatable))]
        class Eatable_patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("Awake")]
            public static void AwakePrefix(Eatable __instance)
            {
                if (ConfigMenu.foodDecayRateMult.Value == 0)
                { // does not work for dead fish
                    __instance.decomposes = false;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            public static void AwakePostfix(Eatable __instance)
            {
                //if (!Main.loadingDone)
                //{
                //    EcoTarget ecoTarget = __instance.GetComponent<EcoTarget>();
                //    if (ecoTarget && ecoTarget.type == EcoTargetType.DeadMeat && ecoTarget.transform.parent.name == "CellRoot(Clone)")
                //    {
                //AddDebug("DeadMeat " + ecoTarget.name + " PARENT " + ecoTarget.transform.parent.name);
                //Destroy(__instance.gameObject);
                if (__instance.decomposes)
                {
                    __instance.kDecayRate *= ConfigMenu.foodDecayRateMult.Value;
                }
                if (ConfigMenu.fishFoodWaterRatio.Value > 0)
                {
                    if (Util.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = __instance.foodValue * ConfigMenu.fishFoodWaterRatio.Value;
                }
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogMessage("Eatable awake " + tt + " eatableFoodValue.ContainsKey " + Main.config.eatableFoodValue.ContainsKey(tt));
                //if (Main.config.eatableFoodValue.ContainsKey(tt))
                //    __instance.foodValue = Main.config.eatableFoodValue[tt];
                //if (Main.config.eatableWaterValue.ContainsKey(tt))
                //    __instance.waterValue = Main.config.eatableWaterValue[tt];
            }
            [HarmonyPrefix]
            [HarmonyPatch("SetDecomposes")]
            public static void SetDecomposesPrefix(Eatable __instance, ref bool value)
            { // SetDecomposes runs when fish killed
                if (value && ConfigMenu.foodDecayRateMult.Value == 0)
                    value = false;
            }
        }

        [HarmonyPatch(typeof(EcoTarget), "OnEnable")]
        class EcoTarget_OnEnable_patch
        {
            public static void Postfix(EcoTarget __instance)
            {
                if (ConfigToEdit.removeCookedFishOnReload.Value && !Main.gameLoaded && __instance.type == EcoTargetType.DeadMeat)
                { // remove cooked fish from lava geysers
                    Pickupable p = __instance.GetComponent<Pickupable>();
                    if (p)// p.inventoryItem is null
                        cookedFish.Add(p);
                }
            }
        }

        [HarmonyPatch(typeof(Plantable), "OnProtoDeserialize")]
        class Inventory_OnProtoDeserialize_patch
        {
            public static void Postfix(Plantable __instance)
            {
                //AddDebug(" OnProtoDeserialize " + __instance.plantTechType);
                if (!ConfigToEdit.canReplantMelon.Value)
                {
                    TechType tt = __instance.plantTechType;
                    if (tt == TechType.Melon || tt == TechType.SmallMelon || tt == TechType.JellyPlant)
                        UnityEngine.Object.Destroy(__instance);
                }
            }
        }

        //[HarmonyPatch(typeof(ItemsContainer), "NotifyRemoveItem")]
        class ItemsContainer_NotifyRemoveItem_patch
        {
            public static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                //if (crafterOpem && Inventory.main._container == __instance)
                //if (Main.config.foodTweaks && Main.crafterOpen)
                { // cooking fish
                  //TechType tt = item.item.GetTechType();

                    if (Util.IsEatableFish(item.item.gameObject))
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        //AddDebug(" NotifyRemoveItem timeDecayStart " + eatable.timeDecayStart);
                        //AddDebug(" NotifyRemoveItem waterValue " + eatable.GetWaterValue() + " " + eatable.GetFoodValue());
                        //waterValueMult = eatable.GetWaterValue() / eatable.waterValue;
                        //foodValueMult = eatable.GetFoodValue() / eatable.foodValue;
                        //timeDecayStart = eatable.timeDecayStart;
                    }
                    //else
                    //    timeDecayStart = 0f;
                    //{
                    //    waterValueMult = 1f;
                    //    foodValueMult = 1f;
                    //}
                }
            }
        }

        //[HarmonyPatch(typeof(Inventory), "ConsumeResourcesForRecipe")]
        class Inventory_ConsumeResourcesForRecipe_patch
        {
            public static void Postfix(Inventory __instance, TechType techType)
            {
                ITechData techData = CraftData.Get(techType);
                if (techData == null)
                    return;
                int index = 0;
                for (int ingredientCount = techData.ingredientCount; index < ingredientCount; ++index)
                {
                    IIngredient ingredient = techData.GetIngredient(index);
                    TechType ingredientTT = ingredient.techType;
                }
            }
        }


        //[HarmonyPatch(typeof(YourTargetClass))] // Replace with the class containing UpdateHunger
        //[HarmonyPatch("UpdateHunger")]
        //public static class UpdateHungerPatch
        //{
        //}

        //[HarmonyPatch(typeof(Crafter), "OnCraftingBegin")]
        class Crafter_OnCraftingBegin_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
            }
        }

        //[HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
            }
        }

        //[HarmonyPatch(typeof(LiveMixin), "TakeDamage")]
        class LiveMixin_TakeDamage_patch
        {
            public static void Prefix(LiveMixin __instance, float originalDamage, Vector3 position = default, DamageType type = DamageType.Normal, GameObject dealer = null)
            {
                if (updatingStats) return;
                AddDebug(" TakeDamage ");
            }
        }

    }
}
