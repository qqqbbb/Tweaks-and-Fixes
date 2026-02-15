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
                Util.DestroyEntity(go);
            }
            cookedFish.Clear();
        }




    }
}
