using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Battery_
    {
        public static HashSet<PowerRelay> subPowerRelays = new HashSet<PowerRelay>();
        static EnergyMixin playerToolEM;
        static EnergyInterface propCannonEI;
        static Dictionary<string, float> defaultBatteryCharge = new Dictionary<string, float>();

        [HarmonyPatch(typeof(EnergyMixin), "ConsumeEnergy")]
        class EnergyMixin_OnAfterDeserialize_Patch
        {
            static void Prefix(EnergyMixin __instance, ref float amount)
            {
                if (playerToolEM != null && playerToolEM == __instance)
                {
                    //AddDebug("tool Consume Energy");
                    amount *= ConfigMenu.toolEnergyConsMult.Value;
                }
            }
        }

        [HarmonyPatch(typeof(PlayerTool), "OnDraw")]
        class PlayerTool_OnDraw_Patch
        {
            static void Postfix(PlayerTool __instance)
            {
                //AddDebug("PlayerTool OnDraw ");
                playerToolEM = __instance.energyMixin;
            }
        }

        [HarmonyPatch(typeof(PropulsionCannonWeapon), "OnDraw")]
        class PropulsionCannonWeapon_OnDraw_Patch
        {
            static void Postfix(PropulsionCannonWeapon __instance)
            {
                propCannonEI = __instance.propulsionCannon.energyInterface;
            }
        }

        [HarmonyPatch(typeof(EnergyInterface), "ConsumeEnergy")]
        class EnergyInterface_ConsumeEnergy_Patch
        {
            static void Prefix(EnergyInterface __instance, ref float amount)
            {
                if (propCannonEI == __instance)
                {
                    //AddDebug(" propCannon ConsumeEnergy");
                    amount *= ConfigMenu.toolEnergyConsMult.Value;
                }
            }
        }

        [HarmonyPatch(typeof(Vehicle), "ConsumeEnergy", new Type[] { typeof(float) })]
        class Vehicle_ConsumeEnergy_Patch
        {
            static void Prefix(Vehicle __instance, ref float amount)
            {
                if (ConfigMenu.vehicleEnergyConsMult.Value != 1)
                    amount *= ConfigMenu.vehicleEnergyConsMult.Value;
                //if (Input.GetKey(KeyCode.LeftShift))
                //    AddDebug("Vehicle Consume Energy " + amount);
            }
        }

        [HarmonyPatch(typeof(SubControl), "Start")]
        class PowerRelay_Start_Patch
        {
            static void Postfix(SubControl __instance)
            {
                if (__instance.powerRelay)
                {
                    subPowerRelays.Add(__instance.powerRelay);
                }
            }
        }

        [HarmonyPatch(typeof(PowerSystem), "ConsumeEnergy")]
        class PowerSystem_ConsumeEnergy_Patch
        {
            static void Prefix(ref float amount, IPowerInterface powerInterface)
            {
                if (ConfigMenu.vehicleEnergyConsMult.Value == 1f)
                    return;

                PowerRelay pr = powerInterface as PowerRelay;
                if (pr && subPowerRelays.Contains(pr))
                {
                    //AddDebug("Sub Consume Energy ");
                    amount *= ConfigMenu.vehicleEnergyConsMult.Value;
                }
                else
                {
                    //AddDebug("base Consume Energy ");
                    amount *= ConfigMenu.baseEnergyConsMult.Value;
                }
            }
        }


        [HarmonyPatch(typeof(Battery))]
        class Battery_Patch_
        {
            [HarmonyPostfix] // runs for new batteries too
            [HarmonyPatch("OnAfterDeserialize")]
            static void OnAfterDeserializePostfix(Battery __instance)
            {
                if (ConfigMenu.batteryChargeMult.Value == 1f || __instance.name.IsNullOrWhiteSpace())
                    return;

                //AddDebug(__instance.name + " Battery OnAfterDeserialize " + __instance._capacity);
                if (!defaultBatteryCharge.ContainsKey(__instance.name))
                {
                    defaultBatteryCharge[__instance.name] = __instance._capacity;
                }
                if (defaultBatteryCharge.ContainsKey(__instance.name))
                {
                    __instance._capacity = defaultBatteryCharge[__instance.name] * ConfigMenu.batteryChargeMult.Value;
                    if (__instance.charge > __instance._capacity)
                        __instance.charge = __instance._capacity;
                }
            }
        }



    }
}
