using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Eatable))]
    internal class Eatable_
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
        { //  runs when fish killed
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
                    if (eatable == __instance)
                        return false;
                }
            }
            return true;
        }

    }
}
