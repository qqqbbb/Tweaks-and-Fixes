
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class QuickSlots_Patch
    {
        static HashSet<TechType> eqiupped;
        static Queue<InventoryItem> toEqiup;
        static HashSet<TechType> toEqiupTT;
        public static GameInput.Button quickslotButton;
        public static bool invChanged = true;

        public static void GetTools()
        {
            toEqiup = new Queue<InventoryItem>();
            toEqiupTT = new HashSet<TechType>();
            GetEquippedTools();
            //Main.Log("GetTools " );
            foreach (InventoryItem item in Inventory.main.container)
            {
                if (item.item.GetComponent<PlayerTool>() && !item.item.GetComponent<Eatable>())
                { // eatable fish is PlayerTool
                    TechType techType = item.item.GetTechType();
                    if (!eqiupped.Contains(techType) && !toEqiupTT.Contains(techType))
                    {
                        toEqiup.Enqueue(item);
                        toEqiupTT.Add(techType);
                        //AddDebug("toEqiup " + techType);
                        //Main.Log("toEqiup " + techType);
                    }
                }
            }
        }

        public static void GetEquippedTools()
        {
            eqiupped = new HashSet<TechType>();
            //Main.Log("GetEquippedTools");
            foreach (TechType item in Inventory.main.quickSlots.GetSlotBinding())
            {
                eqiupped.Add(item);
                //Main.Log("eqiupped " + item);
            }
        }

        private static void EquipNextTool()
        {
            if (invChanged)
            {
                GetTools();
                invChanged = false;
            }
            int activeSlot = Inventory.main.quickSlots.activeSlot;
            InventoryItem currentItem = Inventory.main.quickSlots.binding[activeSlot];
            //if (currentItem == null) 
            //    AddDebug("currentItem == null ");
            //AddDebug("currentItem " + currentItem.item.GetTechName());
            //AddDebug("toEqiup Remove " + toEqiup.Peek().item.GetTechName());
            Inventory.main.quickSlots.Bind(activeSlot, toEqiup.Peek());
            toEqiup.Dequeue();
            toEqiup.Enqueue(currentItem);
            Inventory.main.quickSlots.SelectImmediate(activeSlot);
            //GetEquippedTools();
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_OnAddItem_Patch
        { // this called during loading and tools returned are wrong
            [HarmonyPostfix]
            [HarmonyPatch("OnAddItem")]
            public static void OnAddItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnAddItem ");
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            public static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnRemoveItem ");
                }
            }
        }

        //[HarmonyPatch(typeof(QuickSlots))]
        class QuickSlots_Bind_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Bind")]
            public static void BindPostfix(QuickSlots __instance)
            {
                GetEquippedTools();
                //AddDebug(" Bind ");
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("SlotNext")]
            public static bool SlotNextPrefix(QuickSlots __instance)
            {
                //AddDebug("SlotNext");
                if (Input.GetKey(ConfigMenu.quickslotButton.Value) || GameInput.GetButtonHeld(quickslotButton))
                {
                    //AddDebug("quickslotButton");
                    Pickupable pickupable = Inventory.main.GetHeld();
                    if (pickupable != null)
                    {
                        EquipNextTool();
                        return false;
                    }
                }
                return true;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("SlotPrevious")]
            public static bool SlotPreviousPrefix(QuickSlots __instance)
            {
                if (Input.GetKey(ConfigMenu.quickslotButton.Value) || GameInput.GetButtonHeld(quickslotButton))
                //if (Input.GetKey(Main.configOld.quickslotKey) || GameInput.GetButtonHeld(quickslotButton))
                {
                    Pickupable pickupable = Inventory.main.GetHeld();
                    if (pickupable != null)
                    {
                        EquipNextTool();
                        return false;
                    }
                }
                return true;
            }

        }



    }
}
