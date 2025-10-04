using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(Drillable))]
    internal class Drillable_
    {
        private static float GetDrillDamage()
        {
            return Drillable.drillDamage * ConfigMenu.drillDamageMult.Value;
        }

        [HarmonyTranspiler, HarmonyPatch("OnDrill")]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var codeMatcher = new CodeMatcher(instructions, ilGen);
            codeMatcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldc_R4, Drillable.drillDamage)
            );
            if (codeMatcher.IsInvalid)
            {
                Debug.LogError("[YourModName] Failed to find Drillable.drillDamage in OnDrill method!");
                return instructions;
            }
            codeMatcher.SetInstructionAndAdvance(
                Transpilers.EmitDelegate<System.Func<float>>(GetDrillDamage)
            );
            return codeMatcher.InstructionEnumeration();
        }

        static List<GameObject> lootPinataList = new List<GameObject>();

        [HarmonyPrefix, HarmonyPatch("ManagedUpdate")]
        public static bool ManagedUpdatePrefix(Drillable __instance)
        {
            if (__instance.timeLastDrilled + 0.5f > Time.time)
            {
                __instance.modelRoot.transform.position = __instance.transform.position + __instance.modelRootOffset + new Vector3(Mathf.Sin(Time.time * 60f), Mathf.Cos(Time.time * 58f + 0.5f), Mathf.Cos(Time.time * 64f + 2f)) * 0.011f;
            }
            if (__instance.lootPinataObjects.Count <= 0 || !__instance.drillingExo)
                return false;

            //AddDebug("lootPinataObjects.Count " + __instance.lootPinataObjects.Count);
            lootPinataList.Clear();
            foreach (GameObject lootPinataObject in __instance.lootPinataObjects)
            {
                if (lootPinataObject == null)
                {
                    lootPinataList.Add(lootPinataObject);
                    continue;
                }
                Vector3 b = __instance.drillingExo.transform.position + new Vector3(0f, 0.8f, 0f);
                lootPinataObject.transform.position = Vector3.Lerp(lootPinataObject.transform.position, b, Time.deltaTime * 5f);
                if (Vector3.Distance(lootPinataObject.transform.position, b) > 3f)
                    continue;

                Pickupable pickupable = lootPinataObject.GetComponentInChildren<Pickupable>();
                if (!pickupable)
                    continue;

                if (ConfigToEdit.spawnResourcesWhenDrilling.Value == false)
                {
                    if (__instance.drillingExo.storageContainer.container.HasRoomFor(pickupable))
                    {
                        string arg = Language.main.Get(pickupable.GetTechName());
                        ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", arg));
                        uGUI_IconNotifier.main.Play(pickupable.GetTechType(), uGUI_IconNotifier.AnimationType.From);
                        pickupable.Initialize();
                        InventoryItem item = new InventoryItem(pickupable);
                        __instance.drillingExo.storageContainer.container.UnsafeAdd(item);
                        pickupable.PlayPickupSound();
                    }
                    else if (Player.main.currentMountedVehicle == __instance.drillingExo)
                        ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
                }
                lootPinataList.Add(lootPinataObject);
            }
            if (lootPinataList.Count <= 0)
                return false;

            foreach (GameObject go in lootPinataList)
                __instance.lootPinataObjects.Remove(go);

            return false;
        }

    }
}
