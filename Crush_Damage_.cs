using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Crush_Damage_
    {
        public static float crushInterval = 3f;
        public static float crushDamageResistance = 0f;
        public static int extraCrushDepth = 0;
        public static Dictionary<TechType, int> crushDepthEquipment = new Dictionary<TechType, int>();
        public static Dictionary<TechType, int> crushDamageEquipment = new Dictionary<TechType, int>();

        public static void CrushDamagePlayer()
        {
            if (!Player.main.gameObject.activeInHierarchy || !Player.main.IsSwimming() || !Player.main.liveMixin)
                return;

            float depth = Ocean.GetDepthOf(Player.mainObject);
            string biome = Player.main.biomeString;
            //AddDebug("CrushDamagePlayer biomeString " + biome);
            if (biome.StartsWith("Prison_Aquarium"))
                depth -= 1450;
            else if (biome.StartsWith("Precursor_Gun"))
                depth -= 95;
            //else if (biome.StartsWith("ILZCastle"))
            //    depth -= 1140;
            float crushDepth = ConfigMenu.crushDepth.Value + extraCrushDepth;
            //AddDebug("CrushDamagePlayer depth " + depth);
            //AddDebug("CrushDamagePlayer extraCrushDepth " + extraCrushDepth);
            //AddDebug("CrushDamagePlayer crushDepth " + crushDepth);
            if (depth < crushDepth)
                return;

            float mult = 1f - crushDamageResistance;
            //AddDebug("crushDamageResistance " + crushDamageResistance);
            float damage = ConfigMenu.crushDamage.Value * mult;
            if (damage <= 0)
                return;
            //AddDebug(" Crush Damage " + damage);
            if (ConfigMenu.crushDamageProgression.Value > 0f)
                damage += (depth - crushDepth) * ConfigMenu.crushDamageProgression.Value;
            //AddDebug(" crush Damage Progression " + damage);
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
                    float res = crushDamageEquipment[tt] * .01f;
                    crushDamageResistance += Mathf.Clamp01(res);
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
                if (crushDamageEquipment.ContainsKey(tt))
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
                if (!Main.gameLoaded)
                    return false;

                if (ConfigMenu.vehicleCrushDamageMult.Value == 1f && ConfigMenu.crushDamageProgression.Value == 0f)
                    return true;

                if (!__instance.gameObject.activeInHierarchy || !__instance.enabled || !__instance.GetCanTakeCrushDamage() || __instance.depthCache == null)
                    return false;

                float depth = __instance.depthCache.Get();
                //AddDebug("CrushDamageUpdate depth " + depth);
                //AddDebug("CrushDamageUpdate crushDepth " + __instance.crushDepth);
                if (depth < __instance.crushDepth)
                    return false;

                float damage = __instance.damagePerCrush * ConfigMenu.vehicleCrushDamageMult.Value;
                if (damage <= 0)
                    return false;

                //AddDebug("damage " + damage);
                if (ConfigMenu.crushDamageProgression.Value > 0f)
                    damage += (depth - __instance.crushDepth) * ConfigMenu.crushDamageProgression.Value;
                //AddDebug("damage Progression " + damage);
                __instance.liveMixin.TakeDamage(damage, __instance.transform.position, DamageType.Pressure);
                if (__instance.soundOnDamage)
                    __instance.soundOnDamage.Play();

                return false;
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("Start")]
            public static bool StartPrefix(CrushDamage __instance)
            {
                //AddDebug("CrushDamage  Start " + __instance.name);

                return true;
            }
        }



    }
}
