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
        public static GameInput.Button transferAllItemsButton;
        public static GameInput.Button transferSameItemsButton;
        static bool transferAllItems;
        static bool transferSameItems;

        public static bool MoveAllItems(InventoryItem item)
        {
            transferAllItems = false;
            ItemsContainer container = (ItemsContainer)item.container;
            IItemsContainer oppositeContainer = Inventory.main.GetOppositeContainer(item);
            List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
            foreach (TechType itemType in container.GetItemTypes())
                container.GetItems(itemType, itemsToTransfer);

            foreach (InventoryItem ii in itemsToTransfer)
            {
                //AddDebug("itemsToTransfer " + ii.item.name);
                Inventory.AddOrSwap(ii, oppositeContainer);
            }
            if (itemsToTransfer.Count > 0)
                return true;
            else
                return false;
        }

        public static bool MoveSameItems(InventoryItem item)
        {
            //AddDebug("MoveSameItems " );
            transferSameItems = false;
            ItemsContainer container = (ItemsContainer)item.container;
            IItemsContainer oppositeContainer = Inventory.main.GetOppositeContainer(item);
            List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
            container.GetItems(item.item.GetTechType(), itemsToTransfer);

            foreach (InventoryItem ii in itemsToTransfer)
            {
                //AddDebug("itemsToTransfer " + ii.item.name);
                Inventory.AddOrSwap(ii, oppositeContainer);
            }
            if (itemsToTransfer.Count > 0)
                return true;
            else
                return false;
        }

        [HarmonyPatch(typeof(Inventory), "ExecuteItemAction", new Type[] { typeof(ItemAction), typeof(InventoryItem) })]
        class Inventory_ExecuteItemAction_Patch
        {
            public static bool Prefix(Inventory __instance, InventoryItem item, ItemAction action)
            {
                //AddDebug("ExecuteItemAction AltUseItem " + item.item.GetTechType());
                //AddDebug("ExecuteItemAction action " + action);
                IItemsContainer oppositeContainer = __instance.GetOppositeContainer(item);
                if (Main.advancedInventoryLoaded || action != ItemAction.Switch || oppositeContainer == null || item.container is Equipment || oppositeContainer is Equipment)
                    return true;

                if (transferAllItems || Input.GetKey(Main.config.transferAllItemsKey))
                {
                    //AddDebug("transfer All Items ");
                    return !MoveAllItems(item);
                }
                else if (transferSameItems || Input.GetKey(Main.config.transferSameItemsKey))
                {
                    //AddDebug("transfer same Items ");
                    return !MoveSameItems(item);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GamepadInputModule), "TranslateButtonEvent")]
        class GamepadInputModule_TranslateButtonEvent_Patch
        {
            public static void Postfix(GamepadInputModule __instance, object selectedItem, GameInput.Button button, ref bool __result)
            { // runs when inventory open
                //AddDebug("GamepadInputModule TranslateButtonEvent " + selectedItem + " button " + button);
                if (button == transferAllItemsButton || button == transferSameItemsButton)
                {
                    if (button == transferAllItemsButton)
                    {
                        //AddDebug("ButtonDown transferAllItemsButton");
                        transferAllItems = true;
                    }
                    else if (button == transferSameItemsButton)
                    {
                        //AddDebug("ButtonDown transferSameItemsButton");
                        transferSameItems = true;
                    }
                    PointerEventData evtData;
                    FPSInputModule.current.GetPointerDataFromInputModule(out evtData);
                    evtData.button = PointerEventData.InputButton.Left;
                    ((IPointerClickHandler)selectedItem).OnPointerClick(evtData);
                    __result = true;
                }
            }
        }


    }
}
