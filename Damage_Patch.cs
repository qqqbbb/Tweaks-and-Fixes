using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Damage_Patch
    {
        static System.Random rndm = new System.Random();

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                if (__result > 0f)
                {
                    if (target == Player.mainObject)
                    {
                        //ErrorMessage.AddDebug("Player takes damage");
                        __result *= Main.config.playerDamageMult;
                        if (__result == 0f)
                            return;

                        if (Main.config.dropHeldTool)
                        {
                            if (type != DamageType.Cold && type != DamageType.Poison && type != DamageType.Starve && type != DamageType.Radiation && type != DamageType.Pressure)
                            {
                                int rnd = rndm.Next(1, (int)Player.main.liveMixin.maxHealth);
                                if (rnd < damage)
                                {
                                    //ErrorMessage.AddDebug("DropHeldItem");
                                    Inventory.main.DropHeldItem(true);
                                }
                            }
                        }
                        if (Main.config.replacePoisonDamage && type == DamageType.Poison)
                        {
                            //ErrorMessage.AddDebug("Player takes Poison damage " + damage);
                            Survival survival = Player.main.GetComponent<Survival>();
                            int foodMin = Main.config.replaceHungerDamage ? -99 : 1;
                            int damageLeft = 0;
                            for (int i = (int)__result; i > 0; i--)
                            {
                                if (survival.food > foodMin)
                                    survival.food -= 1f;
                                else
                                    damageLeft++;

                                if (survival.water > foodMin)
                                    survival.water -= 1f;
                                else
                                    damageLeft++;
                            }
                            //DamageType.
                            //ErrorMessage.AddDebug("damageLeft " + damageLeft);
                            Player.main.liveMixin.TakeDamage(damageLeft, target.transform.position, DamageType.Starve, dealer);
                            __result = 0f;
                        }
                    }
                    else if (target.GetComponent<Vehicle>() || target.GetComponent<SubControl>())
                    {
                        //ErrorMessage.AddDebug("Vehicle takes damage");
                        __result *= Main.config.vehicleDamageMult;
                    }
                    else
                        __result *= Main.config.damageMult;
                }

            }
        }
    }
}
