using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;

namespace Tweaks_Fixes
{
    class Fish_Patches
    {

        static public HashSet<Pickupable> gravSphereFish = new HashSet<Pickupable>();

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
                        //ErrorMessage.AddDebug("WaterPark ");
                        return;
                    }

                   PropulsionCannonWeapon pc = Inventory.main.GetHeldTool() as PropulsionCannonWeapon;
                    if (pc && pc.propulsionCannon.grabbedObject == __instance.gameObject)
                    {
                        //ErrorMessage.AddDebug("PropulsionCannonWeapon ");
                        __result = true;
                        return;
                    }
                    foreach (Pickupable p in gravSphereFish)
                    {
                        if (p == __instance) 
                        {
                            //ErrorMessage.AddDebug("Gravsphere ");
                            __result = true;
                            return;
                        }
                    }
                }
      
            }
        }

        [HarmonyPatch(typeof(Gravsphere))]
        class Gravsphere_Patch
        {   
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Gravsphere.OnPickedUp))]
            public static void OnPickedUp(Gravsphere __instance)
            {
                //ErrorMessage.AddDebug("OnPickedUp ");
                gravSphereFish = new HashSet<Pickupable>();
            }
            [HarmonyPostfix]
            [HarmonyPatch(nameof(Gravsphere.AddAttractable))]
            public static void AddAttractable(Gravsphere __instance, Rigidbody r)
            {
                if (Main.IsEatableFishAlive(r.gameObject)) 
                { 
                    //ErrorMessage.AddDebug("AddAttractable ");
                    gravSphereFish.Add(r.GetComponent<Pickupable>());
                }
            }
        }

        [HarmonyPatch(typeof(Creature))]
        class Creature_Start_patch
        {
            [HarmonyPatch(nameof(Creature.Start))]
            public static void Postfix(Creature __instance)
            {
                //Main.Log(__instance.gameObject.name + " " + __instance.GetComponent<Rigidbody>().mass);
                if (__instance is Spadefish)
                {
                    //ErrorMessage.AddDebug("Spadefish");
                    __instance.GetComponent<Rigidbody>().mass = 4f;
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
