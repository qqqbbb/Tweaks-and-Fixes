using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Coffee_Patch
    {
        public static BoxCollider vendMachCol = null;
        public static Dictionary<CoffeeVendingMachine, Pickupable> spawnedCoffees = new Dictionary<CoffeeVendingMachine, Pickupable>();
        public static Dictionary<CoffeeVendingMachine, Pickupable> spawnedCoffeesRight = new Dictionary<CoffeeVendingMachine, Pickupable>();
        //public static Dictionary<CoffeeVendingMachine, BoxCollider> shiftedColliders = new Dictionary<CoffeeVendingMachine, BoxCollider>();
        public static Dictionary<CoffeeVendingMachine, int> spawnedCoffees_ = new Dictionary<CoffeeVendingMachine, int>();

        public static void ShiftCollider(CoffeeVendingMachine __instance, bool down = false)
        {
            BoxCollider collider = __instance.GetComponentInChildren<BoxCollider>();
            Vector3 colPos = collider.transform.position;
            if (down)
            {
                //AddDebug(" ShiftCollider down");
                //shiftedColliders[__instance] = null;
                collider.transform.position = new Vector3(colPos.x, colPos.y - .4f, colPos.z);
            }
            else
            { // allow to pick up coffee
                //AddDebug(" ShiftCollider up");
                //shiftedColliders[__instance] = collider;
                collider.transform.position = new Vector3(colPos.x, colPos.y + .4f, colPos.z);
            }

        }

        [HarmonyPatch(typeof(CoffeeVendingMachine), "Start")]
        class CoffeeVendingMachine_Start_Patch
        {
            public static void Postfix(CoffeeVendingMachine __instance)
            {
                spawnedCoffees_[__instance] = 0;
            }
        }

        [HarmonyPatch(typeof(CoffeeVendingMachine), "OnMachineUse")]
        class CoffeeVendingMachine_OnMachineUse_Patch
        {
            public static bool Prefix(CoffeeVendingMachine __instance)
            {
                if (!__instance.enabled || __instance.powerRelay == null || !__instance.powerRelay.IsPowered())
                    return false;

                if (!spawnedCoffees.ContainsKey(__instance) || spawnedCoffees[__instance] == null)
                {
                    __instance.vfxController.Play(0);
                    __instance.waterSoundSlot1.Play();
                    //__instance.timeLastUseSlot1 = Time.time;
                    if (spawnedCoffees_[__instance] == 0)
                        ShiftCollider(__instance);
                    SpawnCoffee(__instance);

                }
                else if (!spawnedCoffeesRight.ContainsKey(__instance) || spawnedCoffeesRight[__instance] == null)
                {
                    __instance.vfxController.Play(1);
                    __instance.waterSoundSlot2.Play();
                    //__instance.timeLastUseSlot2 = Time.time;
                    SpawnCoffee(__instance, true);
                }

                return false;
            }
        }

        public static void SpawnCoffee(CoffeeVendingMachine __instance, bool right = false)
        {
            spawnedCoffees_[__instance] ++;
            GameObject coffee = CraftData.InstantiateFromPrefab(TechType.Coffee);
            coffee.GetComponent<Rigidbody>().isKinematic = true;
            coffee.transform.localScale = new Vector3(.7f, .7f, .7f);
            Vector3 pos = __instance.transform.position;
            coffee.transform.rotation = __instance.transform.rotation;
            //int rndFood = Main.rndm.Next(minFood, maxFood);
            //float randomAngle = Main.rndm.Next((int)coffee.transform.rotation.y, (int)coffee.transform.rotation.y + 180);
            //coffee.transform.Rotate(0f, randomAngle, 0f);
            //coffee.transform.position = new Vector3(pos.x, pos.y, pos.z);
            coffee.transform.position = __instance.transform.position;
            coffee.transform.position += coffee.transform.forward * .29f;
            if (right)
                coffee.transform.position -= coffee.transform.right * .08f;
            else
                coffee.transform.position += coffee.transform.right * .08f;

            coffee.transform.position += coffee.transform.up * .06f;
            if (right)
                spawnedCoffeesRight[__instance] = coffee.GetComponent<Pickupable>();
            else
                spawnedCoffees[__instance] = coffee.GetComponent<Pickupable>();
            //coffee.transform.position += coffee.transform. * .29f;
            //AddDebug(" spawnedCoffees_ " + spawnedCoffees_[__instance]);
            //AddDebug(" forward " + coffee.transform.right);
            //return false;
        }

        [HarmonyPatch(typeof(Pickupable), "Pickup")]
        class Pickupable_Pickup_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == TechType.Coffee)
                {
                    //AddDebug(" Pickup Coffee");
                    CoffeeVendingMachine cvm = null;
                    foreach (var kvp in spawnedCoffees)
                    {
                        if (kvp.Value == __instance)
                            cvm = kvp.Key;
                    }
                    if (cvm)
                    {
                        spawnedCoffees[cvm] = null;
                        spawnedCoffees_[cvm]--;
                        //AddDebug(" spawnedCoffees_ " + spawnedCoffees_[cvm]);
                    }
                    bool found = false;
                    foreach (var kvp in spawnedCoffeesRight)
                    {
                        if (kvp.Value == __instance)
                        {
                            cvm = kvp.Key;
                            found = true;
                        }
                    }
                    if (found && cvm)
                    {
                        spawnedCoffees_[cvm]--;
                        spawnedCoffeesRight[cvm] = null;
                        //AddDebug(" spawnedCoffees_ " + spawnedCoffees_[cvm]);
                    }
                    if (spawnedCoffees_[cvm] == 0)
                    { 
                        ShiftCollider(cvm, true);
                    }
                }

            }
        }
    }
}
