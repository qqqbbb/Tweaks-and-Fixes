using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class PowerConsumption
    {
        public static HashSet<PowerRelay> subPowerRelays = new HashSet<PowerRelay>();
        static EnergyMixin playerToolEM;
        static HashSet<EnergyMixin> mapRoomCameras = new HashSet<EnergyMixin>();
        static EnergyInterface propCannonEI;

        [HarmonyPatch(typeof(MapRoomCamera), "Start")]
        class MapRoomCamera_Start_Patch
        {
            static void Postfix(MapRoomCamera __instance)
            {
                mapRoomCameras.Add(__instance.energyMixin);
            }
        }

        [HarmonyPatch(typeof(EnergyMixin), "ConsumeEnergy")]
        class EnergyMixin_ConsumeEnergy_Patch
        {
            static void Prefix(EnergyMixin __instance, ref float amount)
            {
                if (playerToolEM == __instance || mapRoomCameras.Contains(__instance))
                {
                    //AddDebug(__instance.name + " tool Consume Energy");
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
            static float amount_;
            static void Prefix(ref float amount, IPowerInterface powerInterface, float amountConsumed)
            {
                if (ConfigMenu.vehicleEnergyConsMult.Value == 1 && ConfigMenu.baseEnergyConsMult.Value == 1)
                    return;

                amount_ = float.MinValue;
                PowerRelay pr = powerInterface as PowerRelay;
                //AddDebug("PowerSystem ConsumeEnergy " + pr.name);
                if (pr && subPowerRelays.Contains(pr))
                {
                    //AddDebug("Sub Consume Energy ");
                    if (ConfigMenu.vehicleEnergyConsMult.Value < 1)
                        amount_ = amount;
                    amount *= ConfigMenu.vehicleEnergyConsMult.Value;
                }
                else
                {
                    //AddDebug("base Consume Energy ");
                    if (ConfigMenu.baseEnergyConsMult.Value < 1)
                        amount_ = amount;
                    amount *= ConfigMenu.baseEnergyConsMult.Value;
                }
            }
            static void Postfix(ref float amount, IPowerInterface powerInterface, ref float amountConsumed)
            {// allow docked vehicles to charge if baseEnergyConsMult is 0
                //AddDebug("base Consume Energy Postfix " + amount_);
                if (amount_ > float.MinValue)
                    amountConsumed = amount_;
            }
        }

    }
}
