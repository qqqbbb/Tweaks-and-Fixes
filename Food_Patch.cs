﻿using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Food_Patch : MonoBehaviour
    {
        static float foodCons = .5f; // vanilla 0.4
        static float waterCons = .5f; // vanilla 0.55
        //static float updateHungerInterval { get { return Main.config.hungerUpdateInterval / DayNightCycle.main.dayNightSpeed; } }
        public static float hungerUpdateTime = 0f;
        public static HashSet<Pickupable> cookedFish = new HashSet<Pickupable> { };

        public static void UpdateStats(Survival __instance)
        {
            //AddDebug("dayNightSpeed  " + DayNightCycle.main.dayNightSpeed);
            //AddDebug("UpdateStats timeSprinted " + Player_Movement.timeSprinted);
            //AddDebug("UpdateStats updateHungerInterval " + (int)updateHungerInterval);
            float oldFood = __instance.food;
            float oldWater = __instance.water;

            if (Player_Movement.timeSprinted > 0f)
            {
                float sprintFoodCons = foodCons * Player_Movement.timeSprinted * ConfigMenu.hungerUpdateInterval.Value * .01f;
                //AddDebug("UpdateStats sprintFoodCons " + sprintFoodCons);
                __instance.food -= sprintFoodCons;
                __instance.water -= sprintFoodCons;
                Player_Movement.timeSprintStart = 0f;
                Player_Movement.timeSprinted = 0f;
            }
            __instance.food -= foodCons;
            __instance.water -= waterCons;
            float foodDamage = 0f;

            if (__instance.food < -100f)
            {
                foodDamage = Mathf.Abs(__instance.food + 100f);
                __instance.food = -100f;
            }
            if (__instance.water < -100f)
            {
                foodDamage += Mathf.Abs(__instance.water + 100f);
                __instance.water = -100f;
            }
            if (foodDamage > 0)
                Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);

            float threshold1 = ConfigMenu.newHungerSystem.Value ? 0f : 20f;
            float threshold2 = ConfigMenu.newHungerSystem.Value ? -50f : 10f;
            __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, oldFood, threshold1, threshold2);
            __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, oldWater, threshold1, threshold2);
            hungerUpdateTime = Time.time + ConfigMenu.hungerUpdateInterval.Value;
            //AddDebug("Invoke  hungerUpdateInterval " + Main.config.hungerUpdateInterval);
            //AddDebug("Invoke dayNightSpeed " + DayNightCycle.main.dayNightSpeed);
            //__instance.Invoke("UpdateHunger", updateHungerInterval);
        }

        [HarmonyPatch(typeof(Survival))]
        class Survival_Start_patch
        {
            private const float bladderFishOxygen = 15f;

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Survival __instance)
            {
                __instance.CancelInvoke();
            }

            [HarmonyPostfix]
            [HarmonyPatch("UpdateHunger")]
            static void UpdateHungerPostfix(Survival __instance)
            {
                //AddDebug("UpdateHunger ");
                hungerUpdateTime = Time.time + ConfigMenu.hungerUpdateInterval.Value;
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
                if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Vanilla && !ConfigMenu.newHungerSystem.Value)
                    return true;

                Eatable eatable = useObj.GetComponent<Eatable>();
                int food = (int)eatable.foodValue;
                int water = (int)eatable.waterValue;
                int playerMinFood = ConfigMenu.newHungerSystem.Value ? -100 : 0;
                float playerMaxWater = ConfigMenu.newHungerSystem.Value ? 200f : 100f;
                float playerMaxFood = 200f;
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
                int rndFood = Main.random.Next(minFood, maxFood);
                float finalFood = Mathf.Min(food, rndFood);
                if (ConfigMenu.newHungerSystem.Value && __instance.food > 100f && finalFood > 0)
                {
                    float mult = (playerMaxFood - __instance.food) * .01f;
                    finalFood *= mult;
                }
                int rndWater = Main.random.Next(minWater, maxWater);
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
                    Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (finalWater < 0 && __instance.water + finalWater < playerMinFood)
                {
                    int waterDamage = Mathf.Abs((int)(__instance.water + finalWater - playerMinFood));
                    //AddDebug("waterDamage " + waterDamage);
                    Player.main.liveMixin.TakeDamage(waterDamage, Player.main.gameObject.transform.position, DamageType.Starve);
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
                    Player.main.GetComponent<OxygenManager>().AddOxygen(bladderFishOxygen);

                Mathf.Clamp(__instance.water, playerMinFood, playerMaxWater);
                Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);
                int warn = ConfigMenu.newHungerSystem.Value ? 0 : 20;
                if (finalWater > 0 && __instance.water > warn && __instance.water - finalWater < warn)
                    __instance.vitalsOkNotification.Play();
                else if (finalFood > 0 && __instance.food > warn && __instance.food - finalWater < warn)
                    __instance.vitalsOkNotification.Play();

                FMODUWE.PlayOneShot(CraftData.GetUseEatSound(techType), Player.main.transform.position);

                __result = true;
                return false;
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
                        Destroy(__instance);
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

        //[HarmonyPatch(typeof(CrafterLogic), "ConsumeResources")]
        class CrafterLogic_ConsumeResources_patch
        {
            public static void Postfix(CrafterLogic __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
            }
        }

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

        //[HarmonyPatch(typeof(uGUI_CraftingMenu), "Action")]
        class CraftingAnalytics_OnCraft_patch
        {
            public static void Postfix(uGUI_CraftingMenu __instance)
            {
                //if (sender.action == TreeAction.Craft)
                //AddDebug(" uGUI_CraftingMenu Craft " );
            }
        }

    }
}
