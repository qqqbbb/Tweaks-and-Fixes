using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Coffee_
    {
        static Vector3 coffeeMugScale = new Vector3(.7f, .7f, .7f);
        enum MugsInCoffeeMaker { None, Left, Right, Both }

        private static bool IsCoffeeMug(GameObject go)
        {
            return CraftData.GetTechType(go) == TechType.Coffee;
        }

        static CoffeeVendingMachine GetCoffeeMaker(GameObject mug)
        {
            if (mug.transform.parent == null)
                return null;

            return mug.transform.parent.GetComponent<CoffeeVendingMachine>();
        }

        static MugsInCoffeeMaker GetMugsInCoffeeMaker(CoffeeVendingMachine cvm)
        {
            MugsInCoffeeMaker mugsInCoffeeMaker = MugsInCoffeeMaker.None;
            bool leftMug = false;
            bool rightMug = false;
            Eatable[] eatables = cvm.GetAllComponentsInChildren<Eatable>();
            foreach (var e in eatables)
            {
                if (IsCoffeeMug(e.gameObject))
                {
                    if (IsLeftMug(e.gameObject))
                        leftMug = true;
                    else
                        rightMug = true;
                }
            }
            if (leftMug && rightMug)
                mugsInCoffeeMaker = MugsInCoffeeMaker.Both;
            else if (leftMug)
                mugsInCoffeeMaker = MugsInCoffeeMaker.Left;
            else if (rightMug)
                mugsInCoffeeMaker = MugsInCoffeeMaker.Right;

            return mugsInCoffeeMaker;
        }

        static bool IsLeftMug(GameObject mug)
        {
            return mug.transform.localPosition.x > 0;
        }

        [HarmonyPatch(typeof(CoffeeVendingMachine))]
        class CoffeeVendingMachine_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            public static void Postfix(CoffeeVendingMachine __instance)
            {
                Transform t = __instance.transform.Find("collisions/Cube");
                BoxCollider bc = t.GetComponent<BoxCollider>();
                bc.size = new Vector3(bc.size.x, bc.size.y, 0.66f);
                bc.center = new Vector3(bc.center.x, bc.center.y, 0.65f);
            }

            [HarmonyPrefix, HarmonyPatch("OnMachineUse")]
            public static bool OnMachineUsePrefix(CoffeeVendingMachine __instance)
            {
                if (!__instance.enabled || __instance.powerRelay == null || !__instance.powerRelay.IsPowered())
                    return false;

                MugsInCoffeeMaker mugsInCoffeeMaker = GetMugsInCoffeeMaker(__instance);
                //AddDebug($"mugsInCoffeeMaker {mugsInCoffeeMaker}");
                if (mugsInCoffeeMaker == MugsInCoffeeMaker.None)
                {
                    if (UnityEngine.Random.value > .5f)
                        FillLeftMug(__instance);
                    else
                        FillRightMug(__instance);
                }
                else if (mugsInCoffeeMaker == MugsInCoffeeMaker.Right)
                    FillLeftMug(__instance);
                else if (mugsInCoffeeMaker == MugsInCoffeeMaker.Left)
                    FillRightMug(__instance);

                return false;
            }

            private static void FillRightMug(CoffeeVendingMachine cvm)
            {
                cvm.vfxController.Play(1);
                cvm.waterSoundSlot2.Play();
                cvm.timeLastUseSlot2 = Time.time;
                cvm.StartCoroutine(SpawnCoffeeMug(cvm, true));
            }

            private static void FillLeftMug(CoffeeVendingMachine cvm)
            {
                cvm.vfxController.Play(0);
                cvm.waterSoundSlot1.Play();
                cvm.timeLastUseSlot1 = Time.time;
                cvm.StartCoroutine(SpawnCoffeeMug(cvm));
            }
        }

        public static IEnumerator SpawnCoffeeMug(CoffeeVendingMachine cvm, bool right = false)
        {
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(TechType.Coffee, result);
            GameObject mug = result.Get();
            mug.GetComponent<Rigidbody>().isKinematic = true;
            Pickupable pickupable = mug.GetComponent<Pickupable>();
            pickupable.isPickupable = false;
            mug.transform.localScale = coffeeMugScale;
            mug.transform.rotation = cvm.transform.rotation;
            mug.transform.position = cvm.transform.position;
            mug.transform.position += mug.transform.forward * .29f;
            mug.transform.position += mug.transform.up * .06f;

            if (right)
                mug.transform.position -= mug.transform.right * .08f;
            else
                mug.transform.position += mug.transform.right * .08f;

            mug.transform.SetParent(cvm.transform, true);
            float y = UnityEngine.Random.Range(45, 136);
            Vector3 rot = new Vector3(0, y, 0);
            mug.transform.Rotate(rot);
            yield return new WaitForSeconds(cvm.spawnDelay);
            pickupable.isPickupable = true;
        }

        [HarmonyPatch(typeof(Pickupable))]
        class Pickupable_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnProtoDeserializeAsync")]
            public static void OnProtoDeserializeAsyncPostfix(Pickupable __instance)
            {
                if (Main.gameLoaded == false && IsCoffeeMug(__instance.gameObject) && GetCoffeeMaker(__instance.gameObject))
                    __instance.isPickupable = true;
            }
        }

    }
}
