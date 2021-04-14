using System;
using UnityEngine;
using HarmonyLib;

namespace Tweaks_Fixes
{
    class Eat_fish_on_release
    {
        [HarmonyPatch(typeof(Pickupable), "Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
        class Pickupable_Drop_Patch
        {
            public static bool Prefix(Pickupable __instance, Vector3 dropPosition)
            {
                Inventory playerInv = Inventory.main;
                if (Main.config.eatFishOnRelease && playerInv.GetHeldTool() != null)
                {
                    Eatable eatable = __instance.GetComponent<Eatable>();
                    //if (__instance.GetTechType() == TechType.Bladderfish)
                    if (eatable != null)
                    {
                        playerInv.UseItem(playerInv.quickSlots.heldItem);
                        //Main.Message("tool ");
                        return false;
                    }
                }
                return true;
            }
        }

        //[HarmonyPatch(typeof(Inventory), "UseItem")]
        //class Inventory_UseItem_Patch
        //{
        //    public static void Postfix(Inventory __instance, InventoryItem item)
        //    {
        //        Main.Message("Inventory UseItem " + item.item.gameObject.name);
        //    }
        //}

        //[HarmonyPatch(typeof(PlayerTool), "OnRightHandDown")]
        //class PlayerTool_OnRightHandDown_Patch
        //{
        //    public static void Postfix(PlayerTool __instance)
        //    {
        //        Main.Message("PlayerTool OnRightHandDown ");
        //    }
        //}
    }
}
