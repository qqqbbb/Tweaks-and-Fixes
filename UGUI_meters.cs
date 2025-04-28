using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    internal class UGUI_meters
    {
        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static void Postfix(uGUI_HealthBar __instance)
            {
                if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value && __instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        [HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
        class uGUI_FoodBar_LateUpdate_Patch
        {
            public static void Postfix(uGUI_FoodBar __instance)
            {
                if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value && __instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        [HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
        class uGUI_WaterBar_LateUpdate_Patch
        {
            public static void Postfix(uGUI_WaterBar __instance)
            {
                if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value && __instance.icon.localRotation.y != 180f)
                    __instance.icon.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }


    }
}
