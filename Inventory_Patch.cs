using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Inventory_Patch
    {
        static InventoryItem selectedItem;
        public static GameInput.Button transferAllItemsButton;
        public static GameInput.Button transferSameItemsButton;

        public static bool MoveAllItems(InventoryItem item)
        {
            //AddDebug("MoveAllItems ");
            if (item == null)
                return false;

            ItemsContainer container = (ItemsContainer)item.container;
            IItemsContainer oppositeContainer = Inventory.main.GetOppositeContainer(item);
            List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
            foreach (TechType itemType in container.GetItemTypes())
                container.GetItems(itemType, itemsToTransfer);

            bool swapped = false;
            foreach (InventoryItem ii in itemsToTransfer)
            {
                //AddDebug("itemsToTransfer " + ii.item.name);
                if (Inventory.AddOrSwap(ii, oppositeContainer))
                    swapped = true;
            }
            return swapped;
        }

        public static bool MoveSameItems(InventoryItem item)
        {
            //AddDebug("MoveSameItems " );
            if (item == null)
                return false;

            ItemsContainer container = (ItemsContainer)item.container;
            IItemsContainer oppositeContainer = Inventory.main.GetOppositeContainer(item);
            List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
            container.GetItems(item.techType, itemsToTransfer);
            bool swapped = false;
            foreach (InventoryItem ii in itemsToTransfer)
            {
                //AddDebug("itemsToTransfer " + ii.item.name);
                if (Inventory.AddOrSwap(ii, oppositeContainer))
                    swapped = true;
            }
            return swapped;
        }

        [HarmonyPatch(typeof(Inventory), "ExecuteItemAction", new Type[] { typeof(ItemAction), typeof(InventoryItem) })]
        class Inventory_ExecuteItemAction_Patch
        {
            public static bool Prefix(Inventory __instance, InventoryItem item, ItemAction action)
            {
                //AddDebug("ExecuteItemAction  " + item.techType);
                //AddDebug("ExecuteItemAction action " + action);
                IItemsContainer oppositeContainer = __instance.GetOppositeContainer(item);
                if (Main.advancedInventoryLoaded || action != ItemAction.Switch || oppositeContainer == null || item.container is Equipment || oppositeContainer is Equipment)
                    return true;

                if (GameInput.lastDevice == GameInput.Device.Keyboard)
                {
                    if (Input.GetKey(ConfigMenu.transferSameItemsButton.Value))
                        return !MoveSameItems(item);

                    if (Input.GetKey(ConfigMenu.transferAllItemsButton.Value))
                        return !MoveAllItems(item);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GamepadInputModule))]
        class GamepadInputModule_Patch
        {          
            [HarmonyPostfix]
            [HarmonyPatch("OnUpdate")]
            public static void OnUpdatePostfix(GamepadInputModule __instance)
            {
                if (Input.GetKeyDown(ConfigMenu.transferAllItemsButton.Value) || GameInput.GetButtonDown(transferAllItemsButton))
                {
                    MoveAllItems(selectedItem);
                }
                else if (Input.GetKeyDown(ConfigMenu.transferSameItemsButton.Value) || GameInput.GetButtonDown(transferSameItemsButton))
                {
                    MoveSameItems(selectedItem);
                }
            }

        }

        [HarmonyPatch(typeof(uGUI_ItemsContainer))]
        class uGUI_ItemsContainer_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("SelectItem")]
            static void Prefix(uGUI_ItemsContainer __instance, object item)
            {
                uGUI_ItemIcon key = item as uGUI_ItemIcon;
                if (key == null || !__instance.icons.TryGetValue(key, out selectedItem))
                    return;

                //AddDebug("uGUI_ItemsContainer SelectItem " + selectedItem.techType);
            }

        }



    }
}
