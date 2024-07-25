using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Battery_Patch
    {
        public static HashSet<PowerRelay> subPowerRelays = new HashSet<PowerRelay>();
        static EnergyMixin PlayerToolEM;
        static EnergyInterface propCannonEI;
        static Dictionary<string, float> defaultBatteryCharge = new Dictionary<string, float>();

        [HarmonyPatch(typeof(EnergyMixin), "ConsumeEnergy")]
        class EnergyMixin_OnAfterDeserialize_Patch
        {
            static void Prefix(EnergyMixin __instance, ref float amount)
            {
                if (PlayerToolEM == __instance)
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
                PlayerToolEM = __instance.energyMixin;
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

        [HarmonyPatch(typeof(Vehicle), "ConsumeEngineEnergy")]
        class Vehicle_ConsumeEngineEnergy_Patch
        {
            static void Prefix(Vehicle __instance, ref float energyCost)
            {
                //AddDebug("Vehicle Consume Energy");
                energyCost *= ConfigMenu.vehicleEnergyConsMult.Value;
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

        //[HarmonyPatch(typeof(Charger), "Start")]
        class Charger_Start_Patch
        {
            static void Postfix(Charger __instance)
            {
                //foreach (string name in Main.config.nonRechargeable)
                //{
                //    TechTypeExtensions.FromString(name, out TechType tt, true);
                //    if (tt != TechType.None && __instance.allowedTech.Contains(tt))
                //    {
                //AddDebug("nonRechargeable " + name);
                //        __instance.allowedTech.Remove(tt);
                //    }
                //}
            }
        }

        //[HarmonyPatch(typeof(Charger), "IsAllowedToAdd")]
        class Charger_IsAllowedToAdd_Patch
        {
            static bool Prefix(Charger __instance, Pickupable pickupable, ref bool __result)
            {
                if (pickupable == null)
                {
                    __result = false;
                    return false;
                }
                TechType t = pickupable.GetTechType();
                string name = t.AsString();
                TechTypeExtensions.FromString(name, out TechType tt, true);
                TechType techType = pickupable.GetTechType();

                //if (tt != TechType.None && Main.config.nonRechargeable.Contains(name) && tt == t)
                //{
                //    AddDebug("nonRechargeable " + name);
                //    __result = false;
                //    return false;
                //}
                if (__instance.allowedTech != null && __instance.allowedTech.Contains(techType))
                    __result = true;

                return false;
            }
        }



    }
}
