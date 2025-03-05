using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Coffee_Patch
    {
        public static BoxCollider vendMachCol = null;
        //public static ConditionalWeakTable<GameObject, Rigidbody> objectsRBs = new ConditionalWeakTable<GameObject, Rigidbody>();
        public static ConditionalWeakTable<Pickupable, CoffeeVendingMachine> spawnedCoffeeM = new ConditionalWeakTable<Pickupable, CoffeeVendingMachine>();
        public static ConditionalWeakTable<CoffeeVendingMachine, Pickupable> spawnedCoffee = new ConditionalWeakTable<CoffeeVendingMachine, Pickupable>();
        public static ConditionalWeakTable<CoffeeVendingMachine, Pickupable> spawnedCoffeeRight = new ConditionalWeakTable<CoffeeVendingMachine, Pickupable>();
        //public static Dictionary<CoffeeVendingMachine, BoxCollider> shiftedColliders = new Dictionary<CoffeeVendingMachine, BoxCollider>();
        public static Dictionary<Eatable, float> spawnedCoffeeTime = new Dictionary<Eatable, float>();
        public static float pourCoffeeTime = 10f;


        public static bool HasCoffee(CoffeeVendingMachine cvm)
        {
            Pickupable pickupable = null;
            if (spawnedCoffee.TryGetValue(cvm, out pickupable))
            {
                if (pickupable)
                    return true;
            }
            else if (spawnedCoffeeRight.TryGetValue(cvm, out pickupable))
            {
                if (pickupable)
                    return true;
            }
            return false;
        }

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
            public static bool Prefix(CoffeeVendingMachine __instance)
            {
                __instance.powerRelay = PowerSource.FindRelay(__instance.transform);
                if (Main.gameLoaded)
                    __instance.idleSound.Play();

                return false;
            }
        }

        [HarmonyPatch(typeof(Eatable), "Awake")]
        class Eatable_Awake_Patch
        {
            public static void Postfix(Eatable __instance)
            { // cyclops physics go insane if there is coffee in cvm when game loads 
                if (Main.gameLoaded || __instance.transform.parent == null)
                    return;

                if (__instance.transform.parent.GetComponent<CoffeeVendingMachine>() && CraftData.GetTechType(__instance.gameObject) == TechType.Coffee)
                {
                    //AddDebug("Coffee awake");
                    UnityEngine.Object.Destroy(__instance.gameObject);
                    //Collider myCol = __instance.GetComponent<CapsuleCollider>();
                    //Collider parentCol = __instance.transform.parent.GetComponentInChildren<BoxCollider>();
                    //if (myCol)
                    //    AddDebug("myCol");
                    //if (parentCol)
                    //    AddDebug("parentCol");
                    //Physics.IgnoreCollision(myCol, parentCol);
                }
            }
        }

        [HarmonyPatch(typeof(CoffeeVendingMachine), "OnMachineUse")]
        class CoffeeVendingMachine_OnMachineUse_Patch
        {
            public static bool Prefix(CoffeeVendingMachine __instance)
            {
                if (!__instance.enabled || __instance.powerRelay == null || !__instance.powerRelay.IsPowered())
                    return false;

                //AddDebug(" OnMachineUse spawnedCoffee " + spawnedCoffee.ContainsKey(__instance));
                //AddDebug(" OnMachineUse spawnedCoffeeRight " + spawnedCoffeeRight.ContainsKey(__instance));
                //if (!spawnedCoffee.ContainsKey(__instance) || spawnedCoffee[__instance] == null)

                Pickupable pickupable = null;
                if (!spawnedCoffee.TryGetValue(__instance, out pickupable) || pickupable == null)
                {
                    __instance.vfxController.Play(0);
                    __instance.waterSoundSlot1.Play();
                    //__instance.timeLastUseSlot1 = Time.time;
                    __instance.StartCoroutine(SpawnCoffee(__instance));
                    //SpawnCoffee(__instance);
                    //if (HasCoffee(__instance))
                    {
                        //AddDebug(" ShiftCollider ");
                        //ShiftCollider(__instance);
                    }
                }
                else if (!spawnedCoffeeRight.TryGetValue(__instance, out pickupable) || pickupable == null)
                {
                    __instance.vfxController.Play(1);
                    __instance.waterSoundSlot2.Play();
                    //__instance.timeLastUseSlot2 = Time.time;
                    //SpawnCoffee(__instance, true);
                    __instance.StartCoroutine(SpawnCoffee(__instance, true));
                }
                return false;
            }
        }

        public static IEnumerator SpawnCoffee(CoffeeVendingMachine cvm, bool right = false)
        {
            //pourCoffeeTime = __instance.spawnDelay;
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(TechType.Coffee, result);
            GameObject coffee = result.Get();
            //AddDebug("SpawnCoffee coffee " + coffee.name);
            ShiftCollider(cvm);
            coffee.GetComponent<Rigidbody>().isKinematic = true;
            coffee.transform.localScale = new Vector3(.7f, .7f, .7f);
            //Vector3 pos = __instance.transform.position;
            coffee.transform.rotation = cvm.transform.rotation;
            //int rndFood = Main.rndm.Next(minFood, maxFood);
            //float randomAngle = Main.rndm.Next((int)coffee.transform.rotation.y, (int)coffee.transform.rotation.y + 180);
            //coffee.transform.Rotate(0f, randomAngle, 0f);
            //coffee.transform.position = new Vector3(pos.x, pos.y, pos.z);
            coffee.transform.position = cvm.transform.position;
            coffee.transform.position += coffee.transform.forward * .29f;
            if (right)
                coffee.transform.position -= coffee.transform.right * .08f;
            else
                coffee.transform.position += coffee.transform.right * .08f;

            coffee.transform.position += coffee.transform.up * .06f;
            Pickupable pickupable = coffee.GetComponent<Pickupable>();
            if (right)
                spawnedCoffeeRight.Add(cvm, pickupable);
            else
                spawnedCoffee.Add(cvm, pickupable);

            spawnedCoffeeM.Add(pickupable, cvm);
            coffee.transform.SetParent(cvm.transform, true);
            spawnedCoffeeTime[coffee.GetComponent<Eatable>()] = DayNightCycle.main.timePassedAsFloat;
            //coffee.transform.position += coffee.transform. * .29f;

        }

        [HarmonyPatch(typeof(Pickupable), "Pickup")]
        class Pickupable_Pickup_Patch
        {
            public static void Postfix(Pickupable __instance)
            {
                if (CraftData.GetTechType(__instance.gameObject) == TechType.Coffee)
                {
                    //AddDebug(" Pickup Coffee");
                    Eatable eatable = __instance.GetComponent<Eatable>();
                    if (spawnedCoffeeTime.ContainsKey(eatable))
                    {
                        if (spawnedCoffeeTime[eatable] + pourCoffeeTime > DayNightCycle.main.timePassedAsFloat)
                        {
                            float pouredCoffee = DayNightCycle.main.timePassedAsFloat - spawnedCoffeeTime[eatable];
                            eatable.waterValue *= pouredCoffee * .1f;
                            //AddDebug(" picked up too soon " + pouredCoffee);
                        }
                        spawnedCoffeeTime.Remove(eatable);
                    }
                    //AddDebug("waterValue " + eatable.waterValue);
                    CoffeeVendingMachine cvm = null;
                    if (spawnedCoffeeM.TryGetValue(__instance, out cvm))
                    {
                        if (cvm == null)
                            return;

                        //if (cvm)
                        {
                            Pickupable p;
                            if (spawnedCoffee.TryGetValue(cvm, out p))
                            {
                                cvm.waterSoundSlot1.Stop();
                                spawnedCoffee.Remove(cvm);
                            }
                            else if (spawnedCoffeeRight.TryGetValue(cvm, out p))
                            {
                                cvm.waterSoundSlot2.Stop();
                                spawnedCoffeeRight.Remove(cvm);
                            }
                            if (!HasCoffee(cvm))
                            {
                                //AddDebug(" ShiftCollider down ");
                                ShiftCollider(cvm, true);
                            }
                        }
                    }
                    //foreach (var kv in spawnedCoffee)
                    //{
                    //    if (kv.Value == __instance)
                    //        cvm = kv.Key;
                    //}
                    //if (cvm)
                    //{
                    //    spawnedCoffee[cvm] = null;
                    //    cvm.waterSoundSlot1.Stop();
                    //}
                    //bool found = false;
                    //foreach (var kv in spawnedCoffeeRight)
                    //{
                    //    if (kv.Value == __instance)
                    //    {
                    //        cvm = kv.Key;
                    //        found = true;
                    //    }
                    //}
                    //if (found && cvm)
                    //{
                    //    spawnedCoffeeRight[cvm] = null;
                    //    cvm.waterSoundSlot2.Stop();
                    //AddDebug(" spawnedCoffees_ " + spawnedCoffees_[cvm]);
                    //}

                }

            }
        }

    }
}
