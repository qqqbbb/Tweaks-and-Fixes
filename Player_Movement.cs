using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    class Player_Movement
    {
        static float invItemsMass;
        public static float timeSprintStart = 0f;
        public static float timeSprinted = 0f;
        public static bool invChanged = true;
        static Dictionary<TechType, float> itemMassDic = new Dictionary<TechType, float>();
        public static Dictionary<TechType, float> waterSpeedEquipment = new Dictionary<TechType, float>();
        public static Dictionary<TechType, float> groundSpeedEquipment = new Dictionary<TechType, float>();
        public static float playerSidewardSpeedMod;
        public static float playerBackwardSpeedMod;
        public static float playerVerticalSpeedMod;
        public static float twoHandToolSwimSpeedMod;
        public static float oneHandToolSwimSpeedMod;
        public static float oneHandToolWalkSpeedMod;
        public static float twoHandToolWalkSpeedMod;
        public static float speedEquipmentMod = float.MaxValue;
        public static float toolMod = float.MaxValue;


        public static void CacheSettings()
        {
            playerSidewardSpeedMod = Mathf.Clamp(ConfigToEdit.playerSidewardSpeedMod.Value, 0, 100) * .01f;
            playerBackwardSpeedMod = Mathf.Clamp(ConfigToEdit.playerBackwardSpeedMod.Value, 0, 100) * .01f;
            playerVerticalSpeedMod = Mathf.Clamp(ConfigToEdit.playerVerticalSpeedMod.Value, 0, 100) * .01f;
            twoHandToolSwimSpeedMod = Mathf.Clamp(ConfigToEdit.twoHandToolSwimSpeedMod.Value, 0, 100) * .01f;
            oneHandToolSwimSpeedMod = Mathf.Clamp(ConfigToEdit.oneHandToolSwimSpeedMod.Value, 0, 100) * .01f;
            oneHandToolWalkSpeedMod = Mathf.Clamp(ConfigToEdit.oneHandToolWalkSpeedMod.Value, 0, 100) * .01f;
            twoHandToolWalkSpeedMod = Mathf.Clamp(ConfigToEdit.twoHandToolWalkSpeedMod.Value, 0, 100) * .01f;
        }

        private static float GetInvItemsMass()
        {
            //AddDebug("Inventory.main.GetTotalItemCount " + Inventory.main.GetTotalItemCount());
            float massTotal = 0;
            foreach (InventoryItem inventoryItem in Inventory.main._container)
            {
                //AddDebug("inventoryItem " + inventoryItem._techType);
                massTotal += GetItemMass(inventoryItem);
            }
            foreach (InventoryItem inventoryItem in (IItemsContainer)Inventory.main._equipment)
            {
                //AddDebug("equipment " + inventoryItem._techType);
                massTotal += GetItemMass(inventoryItem);
            }
            invItemsMass = massTotal;
            return massTotal;
        }

        private static float GetItemMass(InventoryItem inventoryItem)
        {
            if (itemMassDic.ContainsKey(inventoryItem._techType))
                return itemMassDic[inventoryItem._techType];
            else
            {
                Rigidbody rb = inventoryItem.item.GetComponent<Rigidbody>();
                itemMassDic[inventoryItem._techType] = rb.mass;
                return rb.mass;
            }
        }

        public static float GetInvMult()
        {
            float massTotal = 0f;
            if (invChanged)
            {
                massTotal = GetInvItemsMass();
                invChanged = false;
            }
            else
                massTotal = invItemsMass;

            float mult;
            if (Player.main.IsSwimming())
                mult = 100f - massTotal * ConfigMenu.invMultWater.Value;
            else
                mult = 100f - massTotal * ConfigMenu.invMultLand.Value;

            mult = Mathf.Clamp(mult, 0f, 100f);
            //AddDebug("GetInvMult massTotal " + massTotal + " " + (mult * .01f));
            return mult * .01f;
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnAddItem")]
            static void OnAddItemPostfix(MainCameraControl __instance, InventoryItem item)
            {
                //AddDebug("OnAddItem");
                invChanged = true;
            }
            [HarmonyPostfix, HarmonyPatch("OnRemoveItem")]
            static void OnRemoveItemPostfix(MainCameraControl __instance, InventoryItem item)
            {
                //AddDebug("OnRemoveItem");
                invChanged = true;
            }
            [HarmonyPostfix, HarmonyPatch("OnEquip")]
            static void OnEquipPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnEquip");
                speedEquipmentMod = float.MaxValue;
            }
            [HarmonyPostfix, HarmonyPatch("OnUnequip")]
            static void OnUnequipPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnUnequip");
                speedEquipmentMod = float.MaxValue;
            }
        }

        [HarmonyPatch(typeof(MainCameraControl), "GetCameraBob")]
        class MainCameraControl_GetCameraBob_Patch
        {
            static bool Prefix(MainCameraControl __instance, ref bool __result)
            {
                if (!ConfigToEdit.cameraBobbing.Value)
                {
                    __result = false;
                    return false;
                }
                else
                    return true;
            }
        }

        [HarmonyPatch(typeof(Seaglide))]
        class Seaglide_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateActiveState")]
            public static bool UpdateActiveStatePrefix(Seaglide __instance)
            {
                if (!ConfigToEdit.seaglideWorksOnlyForward.Value)
                    return true;
                // seaglide works only if moving forward
                bool activeState = __instance.activeState;
                __instance.activeState = false;
                if (__instance.energyMixin.charge > 0f)
                {
                    if (__instance.screenEffectModel != null)
                        __instance.screenEffectModel.SetActive(__instance.usingPlayer != null);

                    if (__instance.usingPlayer != null && __instance.usingPlayer.IsSwimming())
                    {
                        Vector3 moveDirection = GameInput.GetMoveDirection();
                        __instance.activeState = moveDirection.z > 0f;
                    }
                    if (__instance.powerGlideActive)
                        __instance.activeState = true;
                }
                if (activeState == __instance.activeState)
                    return false;

                __instance.SetVFXActive(__instance.activeState);
                return false;
            }
        }

        [HarmonyPatch(typeof(UnderwaterMotor))]
        class UnderwaterMotor_Patch
        {
            [HarmonyPrefix, HarmonyPatch("AlterMaxSpeed")]
            public static bool AlterMaxSpeedPrefix(UnderwaterMotor __instance, float inMaxSpeed, ref float __result)
            {
                //float ms = (float)System.Math.Round(Player.main.movementSpeed * 10f) / 10f;
                //Util.Message("movementSpeed  " + ms);
                if (waterSpeedEquipment.Count > 0)
                {
                    //AddDebug("waterSpeedEquipment.Count " + waterSpeedEquipment.Count);
                    __result = inMaxSpeed;
                    return false;
                }
                return true;
            }
            [HarmonyPostfix, HarmonyPatch("AlterMaxSpeed")]
            public static void Postfix(UnderwaterMotor __instance, float inMaxSpeed, ref float __result)
            {// van speed forward 5.7 seaglide 11  BZ speed: 4.5, tool 3.75, seaglide 7.12
                float mod = 0;
                if (Player.main.gameObject.transform.position.y > Player.main.GetWaterLevel())
                    mod += 0.3f;

                //AddDebug("AlterMaxSpeed " + __result);
                if (speedEquipmentMod == float.MaxValue)
                {
                    speedEquipmentMod = 0;
                    foreach (var kv in waterSpeedEquipment)
                    {
                        if (Util.IsEquipped(kv.Key))
                        {
                            //AddDebug("Equipped " + kv.Key + " " + kv.Value);
                            speedEquipmentMod += kv.Value;
                        }
                    }
                }
                else
                    mod += speedEquipmentMod;

                if (Player.main.pda.isInUse && oneHandToolSwimSpeedMod > 0)
                    mod -= oneHandToolSwimSpeedMod;
                else if (toolMod == float.MaxValue)
                {
                    //AddDebug("get tool mod");
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    if (tool)
                    {
                        bool oneHanded = Util.IsOneHanded(tool);
                        //AddDebug("AlterMaxSpeed tool oneHanded " + oneHanded);
                        Seaglide seaglide = tool as Seaglide;
                        if (seaglide)
                        {
                            toolMod = 0;
                            HandleSeaglide(seaglide);
                            if (!seaglide.activeState && twoHandToolSwimSpeedMod > 0)
                                toolMod = twoHandToolSwimSpeedMod;
                        }
                        else
                        {
                            if (oneHanded && oneHandToolSwimSpeedMod > 0)
                                toolMod = oneHandToolSwimSpeedMod;
                            else if (!oneHanded && twoHandToolSwimSpeedMod > 0)
                                toolMod = twoHandToolSwimSpeedMod;
                        }
                    }
                    else
                        toolMod = 0;
                }
                else
                {
                    mod -= toolMod;
                    //AddDebug("toolMod " + toolMod);
                }
                Vector3 input = __instance.movementInputDirection;
                if (playerSidewardSpeedMod > 0 && input.x != 0)
                    mod -= playerSidewardSpeedMod * Mathf.Abs(input.normalized.x);
                if (playerBackwardSpeedMod > 0 && input.z < 0)
                    mod -= playerBackwardSpeedMod * Mathf.Abs(input.normalized.z);
                if (playerVerticalSpeedMod > 0 && input.y != 0)
                    mod -= playerVerticalSpeedMod * Mathf.Abs(input.normalized.y);

                //AddDebug("AlterMaxSpeed mod " + mod);
                __result *= ConfigMenu.playerWaterSpeedMult.Value;
                __result += __result * mod;
                bool canMove = __result > 0;
                if (ConfigMenu.invMultWater.Value > 0f)
                    __result *= GetInvMult();

                if (canMove && __result == 0)
                    AddDebug(Language.main.Get("TF_too_much_weight_message"));

                //if (UnityEngine.Input.GetKey(KeyCode.V))
                //{
                //    float ms = Player.main.rigidBody.velocity.magnitude;
                //    AddDebug("movement Speed  " + ms);
                //}
            }
        }

        static void HandleSeaglide(Seaglide seaglide)
        {
            PlayerController pc = Player.main.playerController;
            if (seaglide.activeState)
            {
                pc.underWaterController.forwardMaxSpeed = pc.seaglideForwardMaxSpeed * ConfigMenu.seaglideSpeedMult.Value;
                pc.underWaterController.backwardMaxSpeed = pc.seaglideBackwardMaxSpeed;
                pc.underWaterController.strafeMaxSpeed = pc.seaglideStrafeMaxSpeed;
                pc.underWaterController.verticalMaxSpeed = pc.seaglideVerticalMaxSpeed;
                pc.underWaterController.waterAcceleration = pc.seaglideWaterAcceleration;

                if (ConfigMenu.seaglideSpeedMult.Value > 1)
                    pc.underWaterController.swimDrag = pc.seaglideSwimDrag / ConfigMenu.seaglideSpeedMult.Value;
                else
                    pc.underWaterController.swimDrag = pc.seaglideSwimDrag;
            }
            else
            {
                pc.underWaterController.forwardMaxSpeed = pc.swimForwardMaxSpeed;
                pc.underWaterController.backwardMaxSpeed = pc.swimBackwardMaxSpeed;
                pc.underWaterController.strafeMaxSpeed = pc.swimStrafeMaxSpeed;
                pc.underWaterController.verticalMaxSpeed = pc.swimVerticalMaxSpeed;
                pc.underWaterController.waterAcceleration = pc.swimWaterAcceleration;
                pc.underWaterController.swimDrag = pc.defaultSwimDrag;
            }
        }

        [HarmonyPatch(typeof(PlayerController)), HarmonyPatch("SetMotorMode")]
        class PlayerController_SetMotorMode_Patch
        {
            static void Postfix(PlayerController __instance, Player.MotorMode newMotorMode)
            {
                //AddDebug("SetMotorMode " + newMotorMode);
                speedEquipmentMod = float.MaxValue;
                toolMod = float.MaxValue;
            }
        }


        [HarmonyPatch(typeof(GroundMotor), "ApplyInputVelocityChange")]
        class GroundMotor_Patch
        {
            public static void Prefix(GroundMotor __instance, ref Vector3 __result, ref Vector3 velocity)
            {
                Vector3 input = __instance.movementInputDirection;
                float x = input.normalized.x;
                float z = input.normalized.z;
                float mod = 0;
                //AddDebug("input " + input);
                //AddDebug("velocity " + velocity);
                if (ConfigToEdit.sprintOnlyForward.Value && input.z <= 0)
                    __instance.sprintPressed = false;

                if (playerSidewardSpeedMod > 0 && input.x != 0)
                {
                    float f = 1f - playerSidewardSpeedMod;
                    x = input.normalized.x * f;
                    //AddDebug("Sideward " + f);
                }
                if (playerBackwardSpeedMod > 0 && input.z < 0)
                {
                    float f = 1f - playerBackwardSpeedMod;
                    z = input.normalized.z * f;
                }
                if (speedEquipmentMod == float.MaxValue)
                {
                    speedEquipmentMod = 0;
                    foreach (var kv in groundSpeedEquipment)
                    {
                        if (Util.IsEquipped(kv.Key))
                        {
                            //AddDebug("Equipped " + kv.Key + " " + kv.Value);
                            speedEquipmentMod += kv.Value;
                        }
                    }
                }
                else
                    mod += speedEquipmentMod;

                if (Player.main.pda.isInUse && oneHandToolWalkSpeedMod > 0)
                    mod -= oneHandToolWalkSpeedMod;
                else if (toolMod == float.MaxValue)
                {
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    if (tool)
                    {
                        bool oneHanded = Util.IsOneHanded(tool);
                        //AddDebug("AlterMaxSpeed tool oneHanded " + oneHanded);
                        if (oneHanded && oneHandToolWalkSpeedMod > 0)
                            toolMod = oneHandToolWalkSpeedMod;
                        else if (!oneHanded && twoHandToolWalkSpeedMod > 0)
                            toolMod = twoHandToolWalkSpeedMod;
                    }
                    else
                        toolMod = 0;
                }
                else
                {
                    mod -= toolMod;
                    //AddDebug("toolMod " + toolMod);
                }
                if (x != input.normalized.x || z != input.normalized.z)
                {
                    __instance.movementInputDirection = new Vector3(x, input.y, z);
                    //AddDebug("inputMod " + __instance.movementInputDirection);
                }
                mod += 1;
                bool canMove = mod > 0;
                if (ConfigMenu.invMultWater.Value > 0f)
                    mod *= GetInvMult();

                if (canMove && mod == 0)
                    AddDebug(Language.main.Get("TF_too_much_weight_message"));

                //AddDebug(" mod " + mod);
                __instance.forwardMaxSpeed = __instance.playerController.walkRunForwardMaxSpeed * ConfigMenu.playerGroundSpeedMult.Value * mod;
            }
        }

        [HarmonyPatch(typeof(PlayerTool))]
        class PlayerTool_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnDraw")]
            static void OnDrawPostfix(PlayerTool __instance)
            {
                //AddDebug("OnDraw");
                toolMod = float.MaxValue;
            }
            [HarmonyPostfix, HarmonyPatch("OnHolster")]
            static void OnHolsterPostfix(PlayerTool __instance)
            {
                //AddDebug("OnHolster");
                toolMod = float.MaxValue;
            }
        }

        [HarmonyPatch(typeof(Seaglide), "SetVFXActive")]
        class Seaglide_SetVFXActive_Patch
        {
            public static void Prefix(Seaglide __instance)
            { // seaglide out of power
              //AddDebug("Seaglide SetVFXActive");
                toolMod = float.MaxValue;
            }
        }

    }
}
