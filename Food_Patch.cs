using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Tweaks_Fixes
{
     class Food_Patch : MonoBehaviour
    {
        static bool crafterOpen = false;
        static float waterValueMult = 1f;
        static float foodValueMult = 1f;
        static float foodCons = .5f; // vanilla 0.4
        static float waterCons = .5f; // vanilla 0.55
        static float updateHungerInterval { get { return Main.config.hungerUpdateInterval / DayNightCycle.main.dayNightSpeed; } }

        public static void UpdateStats(Survival __instance)
        {
            //ErrorMessage.AddDebug("UpdateStats  " + updateHungerInterval);
            //ErrorMessage.AddDebug("dayNightSpeed  " + DayNightCycle.main.dayNightSpeed);
            float oldFood = __instance.food;
            float oldWater = __instance.water;

            __instance.food -= foodCons;
            __instance.water -= waterCons;
            if (__instance.food < -100f)
            {
                __instance.food = -100f;
                Player.main.liveMixin.TakeDamage(1f, Player.main.gameObject.transform.position, DamageType.Starve);
            }
            if (__instance.water < -100f)
            {
                __instance.water = -100f;
                Player.main.liveMixin.TakeDamage(1f, Player.main.gameObject.transform.position, DamageType.Starve);
            }
            float threshold1 = Main.config.replaceHungerDamage ? 0f : 20f;
            float threshold2 = Main.config.replaceHungerDamage ? -50f : 10f;
            __instance.UpdateWarningSounds(__instance.foodWarningSounds, __instance.food, oldFood, threshold1, threshold2);
            __instance.UpdateWarningSounds(__instance.waterWarningSounds, __instance.water, oldWater, threshold1, threshold2);
            __instance.Invoke("UpdateHunger", updateHungerInterval);
        }

        [HarmonyPatch(typeof(Survival), "Start")]
        class Survival_Start_patch
        {
            public static void Postfix(Survival __instance)
            {
                if (Main.config.replaceHungerDamage)
                {
                    //ErrorMessage.AddDebug("Survival Start");
                    __instance.CancelInvoke();
                    __instance.Invoke("UpdateHunger", updateHungerInterval);
                }
            }
        }

        [HarmonyPatch(typeof(Survival), "GetWeaknessSpeedScalar")]
        internal class Survival_GetWeaknessSpeedScalar_Patch
        {
            public static bool Prefix(Survival __instance, ref float __result)
            {
                if (!Main.config.replaceHungerDamage)
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
                //ErrorMessage.AddDebug("WeaknessSpeedScalar " + __result);
                return false;
            }
        }

        [HarmonyPatch(typeof(Survival), "UpdateHunger")]
        internal class Survival_UpdateHunger_Patch
        {
            internal static bool Prefix(Survival __instance)
            {
                if (!Main.config.replaceHungerDamage)
                    return true;

                if (!GameModeUtils.RequiresSurvival() || __instance.freezeStats)
                    return false;
                //ErrorMessage.AddDebug("kUpdateHungerInterval " + __instance.kUpdateHungerInterval);
                //__instance.UpdateStats(Main.config.hungerUpdateInterval);
                UpdateStats(__instance);
                return false;
            }
        }

        //[HarmonyPatch(typeof(Survival), "UpdateStats")]
        class Survival_UpdateStats_patch
        {  
            public static bool Prefix(Survival __instance)
            {
                if (!Main.config.replaceHungerDamage)
                    return true;

                //UpdateStats(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(Survival), "Eat")]
        class Survival_Eat_patch
        {
            static System.Random rndm = new System.Random();

            public static bool Prefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla && !Main.config.replaceHungerDamage)
                    return true;

                Eatable eatable = useObj.GetComponent<Eatable>();
                int food = (int)eatable.foodValue;
                int water = (int)eatable.waterValue;
                int minFood = 0;
                int maxFood = 0;
                int minWater = 0;
                int maxWater = 0;

                if (food > 0)
                {
                    if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla || !Main.IsEatableFish(useObj))
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
                    if (Main.config.eatRawFish == Config.EatingRawFish.Vanilla || !Main.IsEatableFish(useObj))
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
                int rndFood = rndm.Next(minFood, maxFood);
                float finalFood = Mathf.Min(food, rndFood);
                if (__instance.food > 100f && finalFood > 0)
                {
                    float mult = (200f - __instance.food) * .01f;
                    finalFood *= mult;
                }
                //ErrorMessage.AddDebug("finalFood " + finalFood);
                int rndWater = rndm.Next(minWater, maxWater);
                float finalWater = Mathf.Min(water, rndWater);
                if (__instance.water > 100f && finalWater > 0)
                {
                    float mult = (200f - __instance.water) * .01f;
                    finalWater *= mult;
                }
                int playerMinFood = Main.config.replaceHungerDamage ? -100 : 0;
                if (finalWater < 0 && __instance.water + finalWater < playerMinFood)
                {
                    int waterDamage = (int)(__instance.water + finalWater - playerMinFood);
                    //ErrorMessage.AddDebug("waterDamage " + waterDamage);
                    Player.main.liveMixin.TakeDamage(Mathf.Abs(waterDamage), Player.main.gameObject.transform.position, DamageType.Starve);
                }
                if (finalFood < 0 && __instance.food + finalFood < playerMinFood)
                {
                    int foodDamage = (int)(__instance.food + finalFood - playerMinFood);
                    //ErrorMessage.AddDebug("foodDamage " + foodDamage);
                    Player.main.liveMixin.TakeDamage(Mathf.Abs(foodDamage), Player.main.gameObject.transform.position, DamageType.Starve);
                }
                __instance.onEat.Trigger((float)finalFood);
                __instance.food += finalFood;
                __instance.onDrink.Trigger((float)finalWater);
                __instance.water += finalWater;
                //ErrorMessage.AddDebug("rndWater " + finalWater);
                Mathf.Clamp(__instance.water, playerMinFood, 200f);
                Mathf.Clamp(__instance.food, playerMinFood, 200f);
                int warn = Main.config.replaceHungerDamage ? 0 : 20;
                if (finalWater > 0 && __instance.water > warn && __instance.water - finalWater < warn)
                    __instance.vitalsOkNotification.Play();
                else if (finalFood > 0 && __instance.food > warn && __instance.food - finalWater < warn)
                    __instance.vitalsOkNotification.Play();

                TechType techType = CraftData.GetTechType(useObj);
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
                //ErrorMessage.AddDebug("Eatable awake " + __instance.gameObject.name);
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
                    if (crafterOpen)
                    { // food values not saved and will reset after reload
                        //ErrorMessage.AddDebug("waterValueMult " + waterValueMult);
                        //ErrorMessage.AddDebug("foodValueMult " + foodValueMult);
                        //if (waterValueMult != 0)
                            __instance.waterValue *= waterValueMult;
                        //if (foodValueMult != 0)
                            __instance.foodValue *= foodValueMult;
                    }
                    else if (Main.IsEatableFishAlive(__instance.gameObject))
                    {
                        __instance.waterValue = Mathf.Abs(__instance.foodValue) * .5f;
                    }
                    else if (__instance.decomposes )
                    {
                        if (Main.IsEatableFish(__instance.gameObject))
                        {
                            //ErrorMessage.AddDebug("dead Fish " + __instance.gameObject.name);
                            __instance.waterValue = Mathf.Abs(__instance.foodValue) * .5f;
                        }
                     
                        //Main.Log(tt + " decomposes " + __instance.kDecayRate);
                        //Main.Log(tt + " decomposes half" + __instance.kDecayRate);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GhostCrafter), "OnOpenedChanged")]
        class GhostCrafter_OnOpenedChanged_patch
        {
            public static void Postfix(GhostCrafter __instance, bool opened)
            {
                //ErrorMessage.AddDebug(" GhostCrafter OnOpenedChanged " + opened);
                //Main.Log(" GhostCrafter OnOpenedChanged " + opened);
                crafterOpen = opened;
                //__instance.PlayerIsInRange
            }
        }

        [HarmonyPatch(typeof(ItemsContainer), "NotifyRemoveItem")]
        class ItemsContainer_NotifyRemoveItem_patch
        {
            public static void Postfix(ItemsContainer __instance, InventoryItem item)
            {
                //if (crafterOpem && Inventory.main._container == __instance)
                if (Main.config.foodTweaks && crafterOpen)
                { // cooking fish
                    //TechType tt = item.item.GetTechType();

                    if (Main.IsEatableFishAlive(item.item.gameObject))
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        //ErrorMessage.AddDebug(" NotifyRemoveItem waterValue " + eatable.GetWaterValue() + " " + eatable.GetFoodValue());
                        waterValueMult = eatable.GetWaterValue() / eatable.waterValue;
                        foodValueMult = eatable.GetFoodValue() / eatable.foodValue;
                    }
                    else
                    {
                        waterValueMult = 1f;
                        foodValueMult = 1f;
                    }
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
                    ErrorMessage.AddDebug(" uGUI_CraftingMenu Craft " + sender.techType0);
                Main.Log(" uGUI_CraftingMenu  action " + sender.action);
                Main.Log(" uGUI_CraftingMenu techType0 " + sender.techType0);
            }
        }
    }
}
