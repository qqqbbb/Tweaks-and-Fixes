using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Craft_Patch
    {
        static bool crafting = false;
        static List<Battery> batteriesUsedForCrafting = new List<Battery>();
        static float timeDecayStart = 0f;

        [HarmonyPatch(typeof(Crafter), "Craft")]
        class Crafter_Craft_Patch
        {
            static void Prefix(Crafter __instance, TechType techType, ref float duration)
            {
                //AddDebug("CrafterLogic Craft " + techType);
                //crafting = true;
                if (ConfigMenu.craftTimeMult.Value != 1f)
                    duration *= ConfigMenu.craftTimeMult.Value;
            }
        }

        [HarmonyPatch(typeof(CrafterLogic), "NotifyCraftEnd")]
        class CrafterLogic_NotifyCraftEnd_Patch
        {
            static void Postfix(CrafterLogic __instance, GameObject target, TechType techType)
            {
                //AddDebug("CrafterLogic NotifyCraftEnd timeDecayStart " + timeDecayStart);
                if (timeDecayStart > 0)
                {
                    //AddDebug("CrafterLogic NotifyCraftEnd timeDecayStart" + timeDecayStart);
                    Eatable eatable = target.GetComponent<Eatable>();
                    if (eatable)
                        eatable.timeDecayStart = timeDecayStart;
                }
                Battery battery = target.GetComponent<Battery>();
                if (battery)
                {
                    //Main.logger.LogDebug("CrafterLogic NotifyCraftEnd battery capacity " + battery._capacity);
                    //AddDebug("crafterOpen");
                    //if (ConfigMenu.batteryChargeMult.Value != 1f)
                    //    battery._capacity *= ConfigMenu.batteryChargeMult.Value;

                    if (ConfigMenu.craftedBatteryCharge.Value != 1)
                    {
                        float mult = ConfigMenu.craftedBatteryCharge.Value * .01f;
                        battery._charge = battery._capacity * mult;
                    }
                    if (batteriesUsedForCrafting.Count == 2)
                    {
                        //AddDebug("batteries.Count == 2");
                        float averageCharge = Mathf.Lerp(batteriesUsedForCrafting[0].charge, batteriesUsedForCrafting[1].charge, .5f);
                        float newCharge = Util.NormalizeToRange(averageCharge, 0, batteriesUsedForCrafting[0].capacity, 0, battery._capacity);
                        if (newCharge < battery._charge)
                            battery._charge = newCharge;
                    }
                }
                timeDecayStart = 0f;
                batteriesUsedForCrafting.Clear();
                crafting = false;
            }
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ConsumeResourcesForRecipe")]
            static void Prefix(Inventory __instance, TechType techType, uGUI_IconNotifier.AnimationDone endFunc = null)
            {
                crafting = true;
                //AddDebug("ConsumeResourcesForRecipe");
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnRemoveItem " + item.item.GetTechName());
                if (crafting)
                {
                    Battery battery = item.item.GetComponent<Battery>();
                    if (battery)
                        batteriesUsedForCrafting.Add(battery);

                    if (Util.IsEatableFish(item.item.gameObject))
                    {
                        Eatable eatable = item.item.GetComponent<Eatable>();
                        //AddDebug(" OnRemoveItem timeDecayStart " + eatable.timeDecayStart);
                        //AddDebug(" NotifyRemoveItem waterValue " + eatable.GetWaterValue() + " " + eatable.GetFoodValue());
                        //waterValueMult = eatable.GetWaterValue() / eatable.waterValue;
                        //foodValueMult = eatable.GetFoodValue() / eatable.foodValue;
                        timeDecayStart = eatable.timeDecayStart;
                    }
                    //else
                    //    timeDecayStart = 0f;
                }
            }
        }

        [HarmonyPatch(typeof(Constructable), "GetConstructInterval")]
        class Constructable_GetConstructInterval_Patch
        {
            static void Postfix(ref float __result)
            {
                if (NoCostConsoleCommand.main.fastBuildCheat)
                    return;
                //AddDebug("GetConstructInterval " );
                __result *= ConfigMenu.buildTimeMult.Value;
            }
        }

        [HarmonyPatch(typeof(EnergyMixin), "OnCraftEnd")]
        public class NoBattery
        {
            private static void Prefix(EnergyMixin __instance, TechType techType)
            { // applies to tools and vehicles
                //AddDebug("EnergyMixin OnCraftEnd " + techType);
                if (!ConfigToEdit.craftWithoutBattery.Value || techType == TechType.MapRoomCamera)
                    return;

                __instance.defaultBattery = TechType.None;
            }
        }

        /*
      //[HarmonyPatch(typeof(CrafterLogic), "TryPickupSingle")]
      class CrafterLogic_TryPickupSingle_Patch
      {
          static bool Prefix(CrafterLogic __instance, TechType techType, ref bool __result)
          {
              //craftingStart = false;
              AddDebug("CrafterLogic TryPickupSingle");
              Inventory main = Inventory.main;
              bool flag = false;
              GameObject original = CraftData.GetPrefabForTechType(techType);
              if (original == null)
              {
                  original = Utils.genericLootPrefab;
                  flag = true;
              }
              if (original != null)
              {
                  Pickupable component1 = original.GetComponent<Pickupable>();
                  if (component1 != null)
                  {
                      Vector2int itemSize = CraftData.GetItemSize(component1.GetTechType());
                      if (main.HasRoomFor(itemSize.x, itemSize.y))
                      {
                          GameObject gameObject = UnityEngine.Object.Instantiate(original);
                          Pickupable pickupable = gameObject.GetComponent<Pickupable>();
                          if (flag)
                              pickupable.SetTechTypeOverride(techType, true);
                          main.ForcePickup(pickupable);
                          // ForcePickup now before NotifyCraftEnd
                          CrafterLogic.NotifyCraftEnd(gameObject, __instance.craftingTechType);

                          Player.main.PlayGrab();
                          __result = true;
                          return false;
                      }
                      AddMessage(Language.main.Get("InventoryFull"));
                      __result = false;
                      return false;
                  }
                  Debug.LogErrorFormat("Can't find Pickupable component on prefab for TechType.{0}", techType);
                  __result = true;
                  return false;
              }
              Debug.LogErrorFormat("Can't find prefab for TechType.{0}", techType);
              __result = true;
              return false;
          }
      }

      /*/
    }
}
