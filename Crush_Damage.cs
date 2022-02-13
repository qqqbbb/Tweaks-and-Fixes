using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Crush_Damage
    {
        public static float crushInterval = 3f;
        public static float crushDamageResistance = 0f;
        public static int extraCrushDepth = 0;
        public static Dictionary<TechType, int> crushDepthEquipment = new Dictionary<TechType, int>();
        public static Dictionary<TechType, float> crushDamageEquipment = new Dictionary<TechType, float>();
        
        public static void CrushDamage()
        {
            if (!Player.main.gameObject.activeInHierarchy)
                return;

            float depth = Ocean.main.GetDepthOf(Player.main.gameObject);
            float crushDepth = Main.config.crushDepth + extraCrushDepth;
            if (depth < crushDepth || !Player.main.IsSwimming() || !Player.main.liveMixin)
                return;

            float mult = 1f - crushDamageResistance;
            float damage = (depth - crushDepth) * Main.config.crushDamageMult * mult;
            if (damage < 0f)
                damage = 0f;
            //AddDebug(" CrushDamageUpdate " + damage);
            Player.main.liveMixin.TakeDamage(damage, Utils.GetRandomPosInView(), DamageType.Pressure);
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnEquip")]
            static void OnEquipPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("crushDepthEquipment.Count " + crushDepthEquipment.Count);
                TechType tt = item.item.GetTechType();
                
                if (crushDepthEquipment.ContainsKey(tt))
                {
                    //Main.config.crushDepth += crushDepthEquipment[tt];
                    extraCrushDepth += crushDepthEquipment[tt];
                    //AddDebug("crushDepth " + Main.config.crushDepth);
                }
                if (crushDamageEquipment.ContainsKey(tt))
                {
                    //AddDebug("crushDamageEquipment " + crushDamageEquipment[tt]);
                    crushDamageResistance += crushDamageEquipment[tt];
                    if (crushDamageResistance > 1f)
                        crushDamageResistance = 1f;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnUnequip")]
            static void OnUnequipPostfix(Inventory __instance, InventoryItem item)
            {
                //Main.Message("Depth Class " + __instance.GetDepthClass());
                //TechTypeExtensions.FromString(loot.Key, out TechType tt, false);
                TechType tt = item.item.GetTechType();

                if (crushDepthEquipment.ContainsKey(tt))
                {
                    //Main.config.crushDepth -= crushDepthEquipment[tt];
                    extraCrushDepth -= crushDepthEquipment[tt];
                    //AddDebug("crushDepth " + Main.config.crushDepth);
                }
                if(crushDamageEquipment.ContainsKey(tt))
                {
                    //AddDebug("crushDamageEquipment " + crushDamageEquipment[tt]);
                    crushDamageResistance -= crushDamageEquipment[tt];
                    if (crushDamageResistance < 0f)
                        crushDamageResistance = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(CrushDamage))]
        class DamageSystem_Patch
        { // player does not have this
            [HarmonyPrefix]
            [HarmonyPatch("CrushDamageUpdate")]
            public static bool CrushDamageUpdatePrefix(CrushDamage __instance)
            {
                if (Main.config.vehicleCrushDamageMult == 0f)
                    return true;

                if (!__instance.gameObject.activeInHierarchy || !__instance.enabled || !__instance.GetCanTakeCrushDamage())
                    return false;

                float depth = __instance.depthCache.Get();
                if (depth < __instance.crushDepth)
                    return false;

                float damage = (depth - __instance.crushDepth) * Main.config.vehicleCrushDamageMult;
                //AddDebug("damage " + damage);
                __instance.liveMixin.TakeDamage(damage, __instance.transform.position, DamageType.Pressure);
                if (__instance.soundOnDamage)
                    __instance.soundOnDamage.Play();

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("UpdateDepthClassification")]
            public static bool UpdateDepthClassificationPrefix(CrushDamage __instance)
            {
                if (!__instance.gameObject.activeInHierarchy)
                    return false;
                float crushDepth1 = __instance.crushDepth;
                __instance.crushDepth = __instance.kBaseCrushDepth + __instance.extraCrushDepth;
                float crushDepth2 = __instance.crushDepth;
                if (!Main.loadingDone || Mathf.Approximately(crushDepth1, crushDepth2))
                    return false;
                ErrorMessage.AddMessage(Language.main.GetFormat<float>("CrushDepthNow", __instance.crushDepth));

                return false;
            }
        }



    }
}
