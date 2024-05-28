using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Camera_Patch
    {

        [HarmonyPatch(typeof(MainCameraControl), "ShakeCamera")]
        class MainCameraControl_ShakeCamera_Patch
        {
            static bool Prefix(MainCameraControl __instance)
            {
                //AddDebug("MainCameraControl ShakeCamera");
                return ConfigToEdit.cameraShake.Value;
            }
        }

        [HarmonyPatch(typeof(DamageFX), "AddHudDamage")]
        class DamageFX_AddHudDamage_Patch
        {
            public static bool Prefix(DamageFX __instance, float damageScalar, Vector3 damageSource, DamageInfo damageInfo)
            {
                //Main.config.crushDamageScreenEffect = false;
                //AddDebug("AddHudDamage " + damageInfo.type);
                if (!ConfigToEdit.crushDamageScreenEffect.Value && damageInfo.type == DamageType.Pressure)
                    return false;

                if (ConfigMenu.damageImpactEffect.Value)
                    __instance.CreateImpactEffect(damageScalar, damageSource, damageInfo.type);

                if (ConfigMenu.damageScreenFX.Value)
                    __instance.PlayScreenFX(damageInfo);

                return false;
            }
        }


    }

}
