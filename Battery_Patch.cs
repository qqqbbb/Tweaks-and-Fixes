﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Battery_Patch
    {
        public static List<PowerRelay> subPowerRelays = new List<PowerRelay>();
     
        [HarmonyPatch(typeof(EnergyMixin), "ConsumeEnergy")]
        class EnergyMixin_OnAfterDeserialize_Patch
        {
            static void Prefix(EnergyMixin __instance, ref float amount)
            {
                //AddDebug("tool Consume Energy");
                amount *= Main.config.toolEnergyConsMult;
            }
        }

        [HarmonyPatch(typeof(Vehicle), "ConsumeEngineEnergy")]
        class Vehicle_ConsumeEngineEnergy_Patch
        {
            static void Prefix(Vehicle __instance, ref float energyCost)
            {
                //AddDebug("Vehicle Consume Energy");
                energyCost *= Main.config.vehicleEnergyConsMult;
            }
        }


        [HarmonyPatch(typeof(PowerRelay), "Start")]
        class PowerRelay_Start_Patch
        {
            static void Postfix(PowerRelay __instance)
            {
                if (__instance.GetComponent<SubControl>())
                {
                    subPowerRelays.Add(__instance);
                }
            }
        }
                
        [HarmonyPatch(typeof(PowerSystem), "ConsumeEnergy")]
        class PowerSystem_ConsumeEnergy_Patch
        {
            static void Prefix(ref float amount, IPowerInterface powerInterface)
            {
                PowerRelay pr = powerInterface as PowerRelay;
                if (pr && subPowerRelays.Contains(pr))
                {
                    //AddDebug("Sub Consume Energy ");
                    amount *= Main.config.vehicleEnergyConsMult;
                }
                else
                { 
                    //AddDebug("base Consume Energy ");
                    amount *= Main.config.baseEnergyConsMult;
                }
            }
        }

        [HarmonyPatch(typeof(Battery))]
        class Battery_Patch_
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnProtoDeserialize")]
            static void OnProtoDeserializePostfix(Battery __instance)
            {
                __instance._capacity *= Main.config.batteryChargeMult;
                if (__instance.charge > __instance._capacity)
                    __instance.charge = __instance._capacity;
                //Main.logger.LogDebug("Battery OnProtoDeserialize " + __instance._capacity);
                //if (Main.gameLoaded)
                //    AddDebug("Battery OnProtoDeserialize " + __instance._capacity);
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
