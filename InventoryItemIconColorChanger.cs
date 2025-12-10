using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class InventoryItemIconColorChanger
    {
        static bool chargerOpen = false;
        public static Dictionary<ItemsContainer, Planter> planters = new Dictionary<ItemsContainer, Planter>();
        public static HashSet<ItemsContainer> aquariumContainers = new HashSet<ItemsContainer>();

        public static void CleanUp()
        {
            aquariumContainers.Clear();
            planters.Clear();
        }

        [HarmonyPatch(typeof(Aquarium), "Start")]
        class Aquarium_Start_Patch
        {
            static void Postfix(Aquarium __instance)
            {
                aquariumContainers.Add(__instance.storageContainer.container);
            }
        }

        [HarmonyPatch(typeof(Planter), "Start")]
        class Planter_Start_Patch
        {
            static void Postfix(Planter __instance)
            {
                planters[__instance.storageContainer.container] = __instance;
            }
        }

        [HarmonyPatch(typeof(Trashcan), "OnEnable")]
        class Trashcan_OnEnable_Patch
        {
            static void Postfix(Trashcan __instance)
            {
                //AddDebug("Trashcan " + __instance.biohazard + " " + __instance.storageContainer.hoverText);
                if (__instance.biohazard)
                {
                    if (__instance.storageContainer.container.allowedTech == null)
                    {
                        //AddDebug("LabTrashcan allowedTech == null ");
                        __instance.storageContainer.container.allowedTech = new HashSet<TechType> { TechType.ReactorRod, TechType.DepletedReactorRod };
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_ItemsContainer), "OnAddItem")]
        class uGUI_ItemsContainer_OnAddItem_Patch
        {
            static void Postfix(uGUI_ItemsContainer __instance, InventoryItem item)
            {
                //AddDebug("uGUI_ItemsContainer OnAddItem " + item.item.GetTechName());
                if (chargerOpen)
                {
                    Battery battery = item.item.GetComponent<Battery>();
                    if (battery && battery.charge == battery.capacity)
                    {
                        //AddDebug(pair.Key.item.GetTechType() + " charge == capacity ");
                        __instance.items[item].SetChroma(0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab))]
        class uGUI_InventoryTab_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnOpenPDA")]
            static void OnOpenPDAPostfix(uGUI_InventoryTab __instance)
            {
                IItemsContainer itemsContainer = null;
                //AddDebug("UsedStorageCount " + Inventory.main.usedStorage.Count);
                for (int i = 0; i < Inventory.main.usedStorage.Count; i++)
                {
                    itemsContainer = Inventory.main.GetUsedStorage(i);
                    if (itemsContainer != null)
                    {
                        //AddDebug("UsedStorage " + i);
                        break;
                    }
                }
                Equipment equipment = itemsContainer as Equipment;
                ItemsContainer container = itemsContainer as ItemsContainer;
                if (container != null)
                {
                    //AddDebug(" container ");
                    if (planters.ContainsKey(container))
                    {
                        Planter planter = planters[container];
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!planter.IsAllowedToAdd(pair.Key.item, false))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    else if (aquariumContainers.Contains(container))
                    {
                        //AddDebug("aquarium Container");
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (pair.Key.item.GetComponent<AquariumFish>() == false)
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    foreach (var pair in __instance.inventory.items)
                    {
                        TechType tt = pair.Key.item.GetTechType();
                        //AddDebug(tt + " Allowed " + container.IsTechTypeAllowed(tt));
                        if (!container.IsTechTypeAllowed(tt))
                            pair.Value.SetChroma(0f);
                    }
                }
                if (equipment != null)
                {
                    chargerOpen = equipment.GetCompatibleSlot(EquipmentType.BatteryCharger, out string s) || equipment.GetCompatibleSlot(EquipmentType.PowerCellCharger, out string ss);
                    //AddDebug("charger " + charger);
                    foreach (var pair in __instance.inventory.items)
                    {
                        TechType tt = pair.Key.item.GetTechType();
                        EquipmentType itemType = TechData.GetEquipmentType(tt);
                        //AddDebug(pair.Key.item.GetTechType() + " " + itemType);
                        string slot = string.Empty;
                        if (equipment.GetCompatibleSlot(itemType, out slot))
                        {
                            //Main.logger.LogMessage(__instance.name + " GetCompatibleSlot " + tt);
                            //EquipmentType chargerType = Equipment.GetSlotType(slot);
                            //AddDebug(__instance.name + " " + chargerType);
                            //if (chargerType == EquipmentType.BatteryCharger || chargerType ==  EquipmentType.PowerCellCharger)
                            if (chargerOpen)
                            {
                                //chargerOpen = true;
                                if (Charger_.notRechargableBatteries.Contains(tt))
                                {
                                    pair.Value.SetChroma(0f);
                                    continue;
                                }
                                Battery battery = pair.Key.item.GetComponent<Battery>();
                                if (battery && battery.charge == battery.capacity)
                                    pair.Value.SetChroma(0f);
                            }
                        }
                        else
                            pair.Value.SetChroma(0f);
                    }
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnClosePDA")]
            static void OnClosePDAPostfix(uGUI_InventoryTab __instance)
            {
                chargerOpen = false;
                foreach (var pair in __instance.inventory.items)
                    pair.Value.SetChroma(1f);
            }
        }

        [HarmonyPatch(typeof(BaseBioReactor), "Start")]
        class BaseBioReactor_Start_Patch
        {
            static void Postfix(BaseBioReactor __instance)
            {
                if (__instance.container.allowedTech == null)
                {
                    //AddDebug("BaseBioReactor container.allowedTech == null ");
                    __instance.container.allowedTech = new HashSet<TechType>();
                    foreach (var pair in BaseBioReactor.charge)
                        __instance.container.allowedTech.Add(pair.Key);
                }
            }
        }




    }
}
