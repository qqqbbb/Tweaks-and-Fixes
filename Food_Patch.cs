using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
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

        public static void UpdateStats(Survival __instance)
        {
            //AddDebug("dayNightSpeed  " + DayNightCycle.main.dayNightSpeed);
            //AddDebug("UpdateStats timeSprinted " + Player_Movement.timeSprinted);
            //AddDebug("UpdateStats updateHungerInterval " + (int)updateHungerInterval);
            float oldFood = __instance.food;
            float oldWater = __instance.water;

            if (Player_Movement.timeSprinted > 0f)
            {
                float sprintFoodCons = foodCons * Player_Movement.timeSprinted * Main.config.hungerUpdateInterval * .01f;
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

            float threshold1 = Main.config.newHungerSystem ? 0f : 20f;
            float threshold2 = Main.config.newHungerSystem ? -50f : 10f;
            __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, oldFood, threshold1, threshold2);
            __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, oldWater, threshold1, threshold2);
            hungerUpdateTime = Time.time + Main.config.hungerUpdateInterval;

            //AddDebug("Invoke  hungerUpdateInterval " + Main.config.hungerUpdateInterval);
            //AddDebug("Invoke dayNightSpeed " + DayNightCycle.main.dayNightSpeed);
            //__instance.Invoke("UpdateHunger", updateHungerInterval);
        }

        [HarmonyPatch(typeof(Survival))]
        class Survival_Start_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Survival __instance)
            {
                __instance.CancelInvoke();
            }

            [HarmonyPostfix]
            [HarmonyPatch("UpdateHunger")]
            internal static void UpdateHungerPostfix(Survival __instance)
            {
                //AddDebug("UpdateHunger " + updateHungerInterval);
                hungerUpdateTime = Time.time + Main.config.hungerUpdateInterval;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetWeaknessSpeedScalar")]
            public static bool GetWeaknessSpeedScalarPrefix(Survival __instance, ref float __result)
            {
                if (!Main.config.newHungerSystem)
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
                if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla && !Main.config.newHungerSystem)
                    return true;

                Eatable eatable = useObj.GetComponent<Eatable>();
                int food = (int)eatable.foodValue;
                int water = (int)eatable.waterValue;
                int playerMinFood = Main.config.newHungerSystem ? -100 : 0;
                float playerMaxWater = Main.config.newHungerSystem ? 200f : 100f;
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
                if (Main.IsEatableFish(useObj))
                {
                    if (food > 0)
                    {
                        if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla)
                        {
                            minFood = food;
                            maxFood = food;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                        {
                            minFood = 0;
                            maxFood = food;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                        {
                            minFood = -food;
                            maxFood = food;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                        {
                            minFood = -food;
                            maxFood = 0;
                        }
                    }
                    if (water > 0)
                    {
                        if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla)
                        {
                            minWater = water;
                            maxWater = water;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                        {
                            minWater = 0;
                            maxWater = water;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                        {
                            minWater = -water;
                            maxWater = water;
                        }
                        else if (Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                        {
                            minWater = -water;
                            maxWater = 0;
                        }
                    }
                }
                int rndFood = Main.rndm.Next(minFood, maxFood);
                float finalFood = Mathf.Min(food, rndFood);
                if (Main.config.newHungerSystem && __instance.food > 100f && finalFood > 0)
                {
                    float mult = (200f - __instance.food) * .01f;
                    finalFood *= mult;
                }
                //AddDebug("finalFood " + finalFood);
                int rndWater = Main.rndm.Next(minWater, maxWater);
                float finalWater = Mathf.Min(water, rndWater);
                if (Main.config.newHungerSystem && __instance.water > 100f && finalWater > 0)
                {
                    float mult = (200f - __instance.water) * .01f;
                    finalWater *= mult;
                }
                if (finalWater < 0 && __instance.water + finalWater < playerMinFood)
                {
                    int waterDamage = Mathf.Abs((int)(__instance.water + finalWater - playerMinFood));
                    //AddDebug("waterDamage " + waterDamage);
                    Player.main.liveMixin.TakeDamage(waterDamage, Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (finalFood < 0 && __instance.food + finalFood < playerMinFood)
                {
                    int foodDamage = Mathf.Abs((int)(__instance.food + finalFood - playerMinFood));
                    //AddDebug("foodDamage " + foodDamage);
                    Player.main.liveMixin.TakeDamage(foodDamage, Player.main.gameObject.transform.position, DamageType.Starve);
                }
                __instance.onEat.Trigger((float)finalFood);
                __instance.food += finalFood;
                __instance.onDrink.Trigger((float)finalWater);
                __instance.water += finalWater;
                //AddDebug("rndWater " + finalWater);
                if (finalFood > 0f)
                    GoalManager.main.OnCustomGoalEvent("Eat_Something");

                if (techType == TechType.Bladderfish)
                    Player.main.GetComponent<OxygenManager>().AddOxygen(15f);
                Mathf.Clamp(__instance.water, playerMinFood, playerMaxWater);
                Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);
                int warn = Main.config.newHungerSystem ? 0 : 20;
                if (finalWater > 0 && __instance.water > warn && __instance.water - finalWater < warn)
                    __instance.vitalsOkNotification.Play();
                else if (finalFood > 0 && __instance.food > warn && __instance.food - finalWater < warn)
                    __instance.vitalsOkNotification.Play();

                FMODUWE.PlayOneShot(CraftData.GetUseEatSound(techType), Player.main.transform.position);

                __result = true;
                return false;
            }


        }

        [HarmonyPatch(typeof(Eatable), "Awake")]
        class Eatable_Awake_patch
        {
            public static void Postfix(Eatable __instance)
            {
                //AddDebug("Eatable awake " + __instance.gameObject.name);
                //Main.Log("Eatable awake " + __instance.gameObject.name + " decomposes "+ __instance.decomposes);
                //__instance.kDecayRate *= .5f;
                //string tt = CraftData.GetTechType(__instance.gameObject).AsString();
                //Main.Log("Eatable awake " + tt );
                //Main.Log("kDecayRate " + __instance.kDecayRate);
                //Main.Log("waterValue " + __instance.waterValue);
                //Creature creature = __instance.GetComponent<Creature>();
                if (__instance.decomposes)
                {
                    __instance.kDecayRate *= Main.config.foodDecayRateMult;
                }
                if (Main.config.foodTweaks) 
                { 
                    if (Main.IsEatableFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = Mathf.Abs(__instance.foodValue) * .5f;

                }
            }
        }

        //[HarmonyPatch(typeof(GhostCrafter), "OnOpenedChanged")]
        class GhostCrafter_OnOpenedChanged_patch
        {
            public static void Postfix(GhostCrafter __instance, bool opened)
            {
                //AddDebug(" GhostCrafter OnOpenedChanged " + opened);
                //Main.Log(" GhostCrafter OnOpenedChanged " + opened);
                //Main.crafterOpen = opened;
                //__instance.PlayerIsInRange
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

                    if (Main.IsEatableFish(item.item.gameObject))
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
                Main.Log("ConsumeResourcesForRecipe " + techType);
                for (int ingredientCount = techData.ingredientCount; index < ingredientCount; ++index)
                {
                    IIngredient ingredient = techData.GetIngredient(index);
                    TechType ingredientTT = ingredient.techType;
                    Main.Log(" TechType " + ingredientTT);
                }
            }
        }

        //[HarmonyPatch(typeof(CrafterLogic), "ConsumeResources")]
        class CrafterLogic_ConsumeResources_patch
        {
            public static void Postfix(CrafterLogic __instance, TechType techType)
            {
                    //TechType tt = item.item.GetTechType();
                    Main.Log("CrafterLogic ConsumeResources " + techType);
            }
        }

        //[HarmonyPatch(typeof(Crafter), "OnCraftingBegin")]
        class Crafter_OnCraftingBegin_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
                Main.Log("Crafter OnCraftingBegin " + techType);
            }
        }

        //[HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_patch
        {
            public static void Prefix(Crafter __instance, TechType techType)
            {
                //TechType tt = item.item.GetTechType();
                Main.Log("Crafter Craft " + techType);
            }
        }

        //[HarmonyPatch(typeof(uGUI_CraftingMenu), "Action")]
        class CraftingAnalytics_OnCraft_patch
        {
            public static void Postfix(uGUI_CraftingMenu __instance, uGUI_CraftNode sender)
            {
                //if (sender.action == TreeAction.Craft)
                    AddDebug(" uGUI_CraftingMenu Craft " + sender.techType0);
                Main.Log(" uGUI_CraftingMenu  action " + sender.action);
                Main.Log(" uGUI_CraftingMenu techType0 " + sender.techType0);
            }
        }
    }
}
