using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class Poison_Damage
    {
        static float poisonDamageTotal;
        static float foodDamageTotal;

        public static void ResetVars()
        {
            poisonDamageTotal = 0;
            foodDamageTotal = 0;
        }

        public static void RemovePoison()
        {
            if (ConfigToEdit.permPoisonDamage.Value == 0 && ConfigToEdit.poisonFoodDamage.Value == 0)
            {
                LiveMixin lm = Player.main.GetComponent<LiveMixin>();
                lm.HealTempDamage(1111);
            }
            poisonDamageTotal = 0;
            foodDamageTotal = 0;
        }

        public static IEnumerator DealPoisonDamage(LiveMixin liveMixin, float damage)
        {
            damage *= ConfigToEdit.permPoisonDamage.Value * .01f;
            poisonDamageTotal += damage;
            //AddDebug($"DealPoisonDamage {poisonDamageTotal}");
            while (poisonDamageTotal > 0)
            {
                float damageToDeal = poisonDamageTotal < 1 ? poisonDamageTotal : 1;
                liveMixin.TakeDamage(damageToDeal, liveMixin.transform.position, DamageType.Poison, Player.mainObject);
                poisonDamageTotal--;
                yield return new WaitForSeconds(1f);
            }
            poisonDamageTotal = 0;
        }

        public static IEnumerator DealFoodDamage(float damage, Survival survival, LiveMixin liveMixin)
        {
            damage *= ConfigToEdit.poisonFoodDamage.Value * .01f;
            foodDamageTotal += damage;
            //AddDebug($"foodDamageToDeal {foodDamageTotal}");
            while (foodDamageTotal > 0)
            {
                float damageToDeal = foodDamageTotal < 1 ? foodDamageTotal : 1;
                int foodMin = ConfigMenu.newHungerSystem.Value ? -100 : 0;
                if (UnityEngine.Random.value > .5f)
                    survival.food = Mathf.Max(foodMin, survival.food - damageToDeal);
                else
                    survival.water = Mathf.Max(foodMin, survival.water - damageToDeal);

                foodDamageTotal--;
                DamageInfo damageInfo = liveMixin.damageInfo;
                if (ConfigToEdit.permPoisonDamage.Value == 0 && damageInfo != null)
                {
                    damageInfo.Clear();
                    damageInfo.originalDamage = damageToDeal;
                    damageInfo.damage = damageToDeal;
                    damageInfo.position = liveMixin.transform.position;
                    damageInfo.type = DamageType.Poison;
                    damageInfo.dealer = Player.mainObject;
                    liveMixin.NotifyAllAttachedDamageReceivers(damageInfo);
                }
                yield return new WaitForSeconds(1f);
            }
            foodDamageTotal = 0;
        }

        [HarmonyPatch(typeof(LiveMixin))]
        class LiveMixin_Patch
        {
            [HarmonyPrefix, HarmonyPatch("TakeDamage")]
            static bool TakeDamagePrefix(LiveMixin __instance, ref bool __result, ref float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (ConfigToEdit.permPoisonDamage.Value == 0 && ConfigToEdit.poisonFoodDamage.Value == 0)
                    return true;

                if (originalDamage == 0 || __instance.health <= 0f || type != DamageType.Poison || __instance.gameObject != Player.mainObject)
                    return true;

                if (NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat())
                    return true;

                bool isBase = __instance.GetComponent<BaseCell>() != null;
                bool invincible = GameModeUtils.IsInvisible() && __instance.invincibleInCreative | isBase;
                if (__instance.shielded || __instance.invincible || invincible)
                    return true;

                bool startedCoroutine = false;
                if (ConfigToEdit.permPoisonDamage.Value > 0)
                {
                    if (dealer == Player.mainObject)
                        __instance.health = Mathf.Max(0, __instance.health - originalDamage);
                    else
                    {
                        CoroutineHost.StartCoroutine(DealPoisonDamage(__instance, originalDamage));
                        startedCoroutine = true;
                    }
                }
                if (ConfigToEdit.poisonFoodDamage.Value > 0)
                {
                    if (dealer != Player.mainObject)
                    {
                        CoroutineHost.StartCoroutine(DealFoodDamage(originalDamage, Main.survival, __instance));
                        startedCoroutine = true;
                    }
                }
                if (startedCoroutine)
                    return false;

                if (__instance.damageInfo != null)
                {
                    __instance.damageInfo.Clear();
                    __instance.damageInfo.originalDamage = originalDamage;
                    __instance.damageInfo.damage = originalDamage;
                    __instance.damageInfo.position = position == default ? __instance.transform.position : position;
                    __instance.damageInfo.type = type;
                    __instance.damageInfo.dealer = dealer;
                    //AddDebug("NotifyAllAttachedDamageReceivers");
                    __instance.NotifyAllAttachedDamageReceivers(__instance.damageInfo);
                }
                if (__instance.damageClip && __instance.damageInfo != null && originalDamage >= __instance.minDamageForSound)
                    Utils.PlayEnvSound(__instance.damageClip, __instance.damageInfo.position);

                if (__instance.loopingDamageEffect && !__instance.loopingDamageEffectObj && __instance.GetHealthFraction() < __instance.loopEffectBelowPercent)
                {
                    __instance.loopingDamageEffectObj = UWE.Utils.InstantiateWrap(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                    __instance.loopingDamageEffectObj.transform.parent = __instance.transform;
                }
                if (__instance.health <= 0)
                {
                    __result = true;
                    if (!__instance.IsCinematicActive() || __instance.ShouldKillInCinematic())
                        __instance.Kill(type);
                    else
                    {
                        __instance.cinematicModeActive = true;
                        __instance.SyncUpdatingState();
                    }
                }
                return false;
            }


        }
    }
}
