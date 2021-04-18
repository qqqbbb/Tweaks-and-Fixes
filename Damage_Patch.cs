using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tweaks_Fixes
{
    class Damage_Patch
    {
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
