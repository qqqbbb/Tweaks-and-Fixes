using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Fish_Patches
    {

        //[HarmonyPatch(typeof(Pickupable), "Drop", new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) })]
        class Pickupable_Drop_Patch
        {
            public static bool Prefix(Pickupable __instance, Vector3 dropPosition)
            {
                if (Main.config.cantEatUnderwater && Player.main.IsUnderwater())
                    return true;

                Inventory playerInv = Inventory.main;
                //if (Main.config.eatFishOnRelease && playerInv.GetHeldTool() != null)
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

        [HarmonyPatch(typeof(Pickupable), "AllowedToPickUp")]
        class Pickupable_AllowedToPickUp_Patch
        {
            public static void Postfix(Pickupable __instance, ref bool __result)
            {
                //__result = __instance.isPickupable && Time.time - __instance.timeDropped > 1.0 && Player.main.HasInventoryRoom(__instance);
                if (Main.config.noFishCatching && Main.IsEatableFishAlive(__instance.gameObject))
                {
                    __result = false;
                    if (Player.main._currentWaterPark)
                    {
                        __result = true;
                        //AddDebug("WaterPark ");
                        return;
                    }

                   PropulsionCannonWeapon pc = Inventory.main.GetHeldTool() as PropulsionCannonWeapon;
                    if (pc && pc.propulsionCannon.grabbedObject == __instance.gameObject)
                    {
                        //AddDebug("PropulsionCannonWeapon ");
                        __result = true;
                        return;
                    }
                    foreach (Pickupable p in Gravsphere_Patch.gravSphereFish)
                    {
                        if (p == __instance) 
                        {
                            //AddDebug("Gravsphere ");
                            __result = true;
                            return;
                        }
                    }
                }
      
            }
        }

        [HarmonyPatch(typeof(SwimBehaviour))]
        class SwimBehaviour_SwimToInternal_patch
        {
            [HarmonyPatch(nameof(SwimBehaviour.SwimToInternal))]
            public static void Prefix(SwimBehaviour __instance, ref float velocity)
            {
                if (Main.IsEatableFish(__instance.gameObject))
                {
                    velocity *= Main.config.fishSpeedMult;
                }
                else
                {
                    velocity *= Main.config.creatureSpeedMult;
                }
            }
        }


    }
}
