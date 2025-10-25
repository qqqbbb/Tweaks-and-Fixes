using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Inventory_
    {
        static InventoryItem selectedItem;
        private static readonly Type[] componentsToRemoveFromDeadCreature = new Type[]
        {
            typeof(CollectShiny),
            typeof(CreatureFlinch),
            typeof(CreatureDeath),
            typeof(SwimInSchool),
            typeof(SwimRandom),
            typeof(StayAtLeashPosition),
            typeof(Breach),
            typeof(AvoidObstacles),
            typeof(AvoidEscapePod),
            typeof(AvoidTerrain),
            typeof(FleeOnDamage),
            typeof(FleeWhenScared),
            typeof(MoveTowardsTarget),
            typeof(SwimToVent),
            typeof(SwimToHeroPeeper),
            typeof(SwimToMeat),
            typeof(SwimToTarget),
            typeof(Scareable),
            typeof(CreatureFear),
            typeof(SwimBehaviour),
            typeof(SplineFollowing),
            typeof(Locomotion),
        };


        public static bool MoveAllItems(InventoryItem item)
        {
            if (item == null)
                return false;

            //AddDebug("MoveAllItems ");
            ItemsContainer container = (ItemsContainer)item.container;
            IItemsContainer oppositeContainer = Inventory.main.GetOppositeContainer(item);
            if (container == null || oppositeContainer == null || oppositeContainer is Equipment)
                return false;

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
            selectedItem = null;
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
            selectedItem = null;
            return swapped;
        }

        private static void RemoveComponentsFromDeadCreature(GameObject go)
        {
            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            if (liveMixin == null || liveMixin.IsAlive())
                return;
            // removing CreatureDeath fix bug equipped dead fish moving up in player hand
            foreach (Type componentType in componentsToRemoveFromDeadCreature)
            {
                Component component = go.GetComponent(componentType);
                if (component != null)
                    UnityEngine.Object.Destroy(component);
            }
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch_
        {
            [HarmonyPostfix, HarmonyPatch("OnAddItem")]
            public static void OnAddItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (!Main.gameLoaded && item.item.GetComponent<Creature>())
                    RemoveComponentsFromDeadCreature(item.item.gameObject);
                //Creature creature = item.item.GetComponent<Creature>();
                //FixPeeperLOD(Creature peeper, bool alive = false)
            }
            [HarmonyPrefix]
            [HarmonyPatch("ExecuteItemAction", new Type[] { typeof(ItemAction), typeof(InventoryItem) })]
            public static bool ExecuteItemActionPrefix(Inventory __instance, InventoryItem item, ItemAction action)
            {
                //AddDebug("ExecuteItemAction  " + item.techType);
                //AddDebug("ExecuteItemAction action " + action);
                if (Main.advancedInventoryLoaded || action != ItemAction.Switch || item.container is Equipment)
                    return true;

                if (GameInput.GetButtonHeld(OptionsMenu.moveAllItemsButton))
                    return !MoveAllItems(item);
                else if (GameInput.GetButtonHeld(OptionsMenu.moveSameItemsButton))
                    return !MoveSameItems(item);

                return true;
            }
            [HarmonyPostfix, HarmonyPatch("GetItemAction")]
            static void GetItemActionPostfix(Inventory __instance, ref ItemAction __result, InventoryItem item)
            {
                if (__result == ItemAction.Eat)
                {
                    if (ConfigMenu.cantEatUnderwater.Value && Player.main.IsUnderwater())
                        __result = ItemAction.None;
                }
                else if (__result == ItemAction.Use && ConfigMenu.cantUseMedkitUnderwater.Value && Player.main.IsUnderwater() && item.item.GetTechType() == TechType.FirstAidKit)
                {
                    __result = ItemAction.None;
                    return;
                }
            }
        }

        //[HarmonyPatch(typeof(GamepadInputModule))]
        class GamepadInputModule_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("OnUpdate")]
            public static void OnUpdatePostfix(GamepadInputModule __instance)
            {
                if (selectedItem == null || Inventory.main.GetUsedStorageCount() == 0)
                    return;

                if (GameInput.GetButtonDown(OptionsMenu.moveAllItemsButton))
                    MoveAllItems(selectedItem);
                else if (GameInput.GetButtonDown(OptionsMenu.moveSameItemsButton))
                    MoveSameItems(selectedItem);
            }
        }

        //[HarmonyPatch(typeof(uGUI_ItemsContainer))]
        class uGUI_ItemsContainer_Patch
        {
            //[HarmonyPostfix,HarmonyPatch("SelectItem")]
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
