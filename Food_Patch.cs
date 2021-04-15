using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Food_Patch
    {
        static bool crafterOpen = false;
        static float waterValueMult = 1f;
        static float foodValueMult = 1f;
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
