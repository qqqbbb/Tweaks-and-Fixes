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
        public static HashSet<GameObject> cookedFish = new HashSet<GameObject> { };
        private static bool usingMedkit;
        static float foodLowScalar = SurvivalConstants.kLowFoodThreshold / 100f;
        static float waterLowScalar = SurvivalConstants.kLowWaterThreshold / 100f;
        static float foodCriticalScalar = SurvivalConstants.kCriticalFoodThreshold / 100f;
        static float waterCriticalScalar = SurvivalConstants.kCriticalWaterThreshold / 100f;

        [HarmonyPatch(typeof(Survival))]
        class Survival_patch
        {
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
                    yield return new WaitForSeconds(GetHungerUpdateInterval(survival));
                    //AddDebug("UpdateHunger");
                    survival.UpdateHunger();
                }
            }

            private static float GetHungerUpdateInterval(Survival survival)
            {
                return survival.kUpdateHungerInterval / DayNightCycle.main._dayNightSpeed;
            }

            public static float GetfoodWaterHealThreshold()
            {
                //Main.logger.LogMessage("GetfoodWaterHealThreshold " + ConfigMenu.foodWaterHealThreshold.Value);
                return ConfigMenu.foodHealThreshold.Value;
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
                //if (ConfigMenu.foodLossMult.Value == 1 && ConfigMenu.waterLossMult.Value == 1 && ConfigMenu.playerMaxFood.Value == (int)SurvivalConstants.kMaxOverfillStat && ConfigMenu.PlayerMaxWater.Value == (int)SurvivalConstants.kMaxStat && ConfigToEdit.playerFoodDamageThreshold.Value == 0 && ConfigToEdit.playerWaterDamageThreshold.Value == 0 && ConfigToEdit.foodLossMultSprint.Value == 1f && ConfigToEdit.starveDamage.Value == SurvivalConstants.kStarveDamage)
                //    return true;

                __result = UpdateStats(__instance, timePassed);
                return false;
            }

            public static float UpdateStats(Survival survival, float timePassed)
            {
                if (timePassed < Mathf.Epsilon)
                    return 0;

                float oldFood = survival.food;
                float oldWater = survival.water;
                float foodToLose = timePassed / SurvivalConstants.kFoodTime * SurvivalConstants.kMaxStat;
                float waterToLose = timePassed / SurvivalConstants.kWaterTime * SurvivalConstants.kMaxStat;
                float minFood = ConfigToEdit.starvationThreshold.Value;
                float minWater = ConfigToEdit.dehydrationThreshold.Value;
                if (ConfigToEdit.foodLossMultSprint.Value > 1 && Player.main.mode == Player.Mode.Normal && Player.main.IsUnderwaterForSwimming() == false && Player.main.groundMotor.IsGrounded() && Player.main.groundMotor.IsSprinting())
                {
                    foodToLose *= ConfigToEdit.foodLossMultSprint.Value;
                    waterToLose *= ConfigToEdit.foodLossMultSprint.Value;
                }
                //AddDebug("UpdateStats foodToLose " + foodToLose);
                survival.food -= foodToLose * ConfigMenu.foodLossMult.Value;
                survival.water -= waterToLose * ConfigMenu.waterLossMult.Value;
                float starveDamage = 0;

                if (survival.food < minFood)
                {
                    starveDamage = ConfigToEdit.starveDamage.Value;
                    survival.food = minFood;
                }
                else if (survival.water < minWater)
                {
                    starveDamage = ConfigToEdit.starveDamage.Value;
                    survival.water = minWater;
                }
                float foodLowThreshold = Mathf.Lerp(minFood, ConfigToEdit.playerFullFood.Value, foodLowScalar);
                float waterLowThreshold = Mathf.Lerp(minWater, ConfigToEdit.playerFullWater.Value, waterLowScalar);
                float foodCriticalThreshold = Mathf.Lerp(minFood, ConfigToEdit.playerFullFood.Value, foodCriticalScalar);
                float waterCriticalThreshold = Mathf.Lerp(minWater, ConfigToEdit.playerFullWater.Value, foodCriticalScalar);
                survival.UpdateWarningSounds(survival.foodWarningSounds, survival.food, oldFood, foodLowThreshold, foodCriticalThreshold);
                survival.UpdateWarningSounds(survival.waterWarningSounds, survival.water, oldWater, waterLowThreshold, waterCriticalThreshold);
                return starveDamage;
            }


            [HarmonyPrefix, HarmonyPatch("Eat")]
            public static bool EatPrefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                if (useObj == null)
                    return false;

                Eatable eatable = useObj.GetComponent<Eatable>();
                if (eatable == null)
                    return false;

                //AddDebug("Eat " + eatable.name);
                float food = eatable.foodValue;
                float water = eatable.waterValue;
                float playerMinFood = ConfigToEdit.starvationThreshold.Value;
                float playerMinWater = ConfigToEdit.dehydrationThreshold.Value;
                float playerMaxWater = ConfigToEdit.PlayerMaxWater.Value;
                float playerFullWater = ConfigToEdit.playerFullWater.Value;
                float playerMaxFood = ConfigToEdit.playerMaxFood.Value;
                float playerFullFood = ConfigToEdit.playerFullFood.Value;
                //AddDebug($"playerMinFood {playerMinFood} playerMaxFood {playerMaxFood}");

                TechType techType = CraftData.GetTechType(useObj);
                if (techType == TechType.None)
                {
                    if (useObj.TryGetComponent(out Pickupable p))
                        techType = p.GetTechType();
                }
                if (Util.IsRawFish(useObj))
                {
                    food = Util.GetFishFoodValue(food);
                    water = Util.GetFishFoodValue(water);
                }
                if (food > 0 && __instance.food > playerFullFood && playerFullFood < playerMaxFood)
                {
                    float mult = (playerMaxFood - __instance.food) * .01f;
                    food *= mult;
                }
                if (water > 0 && __instance.water > playerFullWater && playerFullWater < playerMaxWater)
                {
                    float mult = (playerMaxWater - __instance.water) * .01f;
                    water *= mult;
                }
                __instance.onEat.Trigger(food);
                __instance.food += food;
                __instance.onDrink.Trigger(water);
                __instance.water += water;
                //AddDebug($"food {food} water {water} ");
                __instance.water = Mathf.Clamp(__instance.water, playerMinWater, playerMaxWater);
                __instance.food = Mathf.Clamp(__instance.food, playerMinFood, playerMaxFood);

                if (food > 0)
                    GoalManager.main.OnCustomGoalEvent("Eat_Something");
                if (water > 0)
                    GoalManager.main.OnCustomGoalEvent("Drink_Something");

                if (techType == TechType.Bladderfish)
                    Player.main.GetComponent<OxygenManager>().AddOxygen(SurvivalConstants.kBladderFishO2OnEat);

                float foodOkThreshold = Mathf.Lerp(playerMinFood, playerMaxFood, foodLowScalar);
                float waterOkThreshold = Mathf.Lerp(playerMinWater, playerMaxWater, waterLowScalar);
                if (water > 0 && __instance.water > waterOkThreshold && __instance.water - water < waterOkThreshold)
                    __instance.vitalsOkNotification.Play();
                else if (food > 0 && __instance.food > foodOkThreshold && __instance.food - food < foodOkThreshold)
                    __instance.vitalsOkNotification.Play();

                if (food > 0 || water > 0)
                    FMODUWE.PlayOneShot(TechData.GetSoundUse(techType), Player.main.transform.position);

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
            [HarmonyPrefix, HarmonyPatch("Awake")]
            public static void AwakePrefix(Eatable __instance)
            {
                if (ConfigMenu.foodDecayRateMult.Value == 0)
                { // does not work for dead fish
                    __instance.decomposes = false;
                }
            }
            [HarmonyPostfix, HarmonyPatch("Awake")]
            public static void AwakePostfix(Eatable __instance)
            {
                //if (!Main.loadingDone)
                //{
                //    EcoTarget ecoTarget = __instance.GetComponent<EcoTarget>();
                //    if (ecoTarget && ecoTarget.type == EcoTargetType.DeadMeat && ecoTarget.transform.parent.name == "CellRoot(Clone)")
                //    {
                //AddDebug("DeadMeat " + ecoTarget.name + " PARENT " + ecoTarget.transform.parent.name);
                //Destroy(__instance.gameObject);
                if (__instance.decomposes && ConfigMenu.foodDecayRateMult.Value != 1)
                {
                    __instance.kDecayRate *= ConfigMenu.foodDecayRateMult.Value;
                }
                if (ConfigMenu.fishFoodWaterRatio.Value > 0)
                {
                    if (Util.IsRawFish(__instance.gameObject) && __instance.foodValue > 0)
                        __instance.waterValue = __instance.foodValue * ConfigMenu.fishFoodWaterRatio.Value * .01f;
                }
                //TechType tt = CraftData.GetTechType(__instance.gameObject);
                //Main.logger.LogMessage("Eatable awake " + tt + " eatableFoodValue.ContainsKey " + Main.config.eatableFoodValue.ContainsKey(tt));
                //if (Main.config.eatableFoodValue.ContainsKey(tt))
                //    __instance.foodValue = Main.config.eatableFoodValue[tt];
                //if (Main.config.eatableWaterValue.ContainsKey(tt))
                //    __instance.waterValue = Main.config.eatableWaterValue[tt];
            }
            [HarmonyPrefix, HarmonyPatch("SetDecomposes")]
            public static void SetDecomposesPrefix(Eatable __instance, ref bool value)
            { // SetDecomposes runs when fish killed
                if (value && ConfigMenu.foodDecayRateMult.Value == 0)
                    value = false;
            }
            [HarmonyPrefix, HarmonyPatch("IterateDespawn")]
            public static bool IterateDespawnPrefix(Eatable __instance)
            {// fix bug: dead fish in player hand despawns
                if (__instance.gameObject.activeSelf && __instance.IsRotten() && DayNightCycle.main.timePassedAsFloat - __instance.timeDespawnStart > __instance.despawnDelay)
                {
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    if (tool)
                    {
                        Eatable eatable = tool.GetComponent<Eatable>();
                        if (eatable && eatable == __instance)
                            return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(EcoTarget), "OnEnable")]
        class EcoTarget_OnEnable_patch
        {
            public static void Postfix(EcoTarget __instance)
            {
                if (ConfigToEdit.removeCookedFishOnReload.Value && !Main.gameLoaded && Util.IsFishCooked(__instance) && Util.IsPickupableInContainer(__instance) == false)
                {
                    cookedFish.Add(__instance.gameObject);
                }
            }
        }

        public static void RemoveCookedFish()
        {
            foreach (GameObject go in cookedFish)
            {
                UnityEngine.Object.Destroy(go);
            }
            cookedFish.Clear();
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

                    if (Util.IsRawFish(item.item.gameObject))
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



    }
}
