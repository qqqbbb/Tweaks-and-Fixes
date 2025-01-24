using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    class Player_Movement
    {
        public static float invItemsMod = 1;
        public static float timeSprintStart = 0f;
        public static float timeSprinted = 0f;
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
        public static float equipmentSpeedMod;
        public static float toolMod = 1;
        public static bool swimming;
        static Seaglide seaglide;

        public static void UpdateModifiers()
        {
            GetEquipmentMod();
            GetToolMod();
            GetInvMod();
        }

        public static void CacheSettings()
        {
            playerSidewardSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.playerSidewardSpeedMod.Value, 0, 100) * .01f;
            playerBackwardSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.playerBackwardSpeedMod.Value, 0, 100) * .01f;
            playerVerticalSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.playerVerticalSpeedMod.Value, 0, 100) * .01f;
            twoHandToolSwimSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.twoHandToolSwimSpeedMod.Value, 0, 100) * .01f;
            oneHandToolSwimSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.oneHandToolSwimSpeedMod.Value, 0, 100) * .01f;
            oneHandToolWalkSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.oneHandToolWalkSpeedMod.Value, 0, 100) * .01f;
            twoHandToolWalkSpeedMod = 1 - Mathf.Clamp(ConfigToEdit.twoHandToolWalkSpeedMod.Value, 0, 100) * .01f;
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

        public static void GetInvMod()
        {
            if (ConfigMenu.invMultWater.Value == 0 && ConfigMenu.invMultLand.Value == 0)
            {
                invItemsMod = 1;
                return;
            }
            float massTotal = 0;
            foreach (InventoryItem inventoryItem in Inventory.main._container)
            {
                massTotal += GetItemMass(inventoryItem);
            }
            foreach (InventoryItem inventoryItem in (IItemsContainer)Inventory.main._equipment)
            {
                massTotal += GetItemMass(inventoryItem);
            }
            if (swimming)
                invItemsMod = 100f - massTotal * ConfigMenu.invMultWater.Value;
            else
                invItemsMod = 100f - massTotal * ConfigMenu.invMultLand.Value;

            invItemsMod = Mathf.Clamp(invItemsMod, 0f, 100f) * .01f;
            //AddDebug("GetInvMult massTotal " + massTotal + " Mod " + invItemsMod);
        }

        private static void GetEquipmentMod()
        {
            equipmentSpeedMod = 0;
            if (swimming)
            {
                foreach (var kv in waterSpeedEquipment)
                {
                    if (Util.IsEquipped(kv.Key))
                    {
                        //AddDebug("Equipped " + kv.Key + " " + kv.Value);
                        equipmentSpeedMod += kv.Value;
                    }
                }
            }
            else
            {
                foreach (var kv in groundSpeedEquipment)
                {
                    if (Util.IsEquipped(kv.Key))
                    {
                        //AddDebug("Equipped " + kv.Key + " " + kv.Value);
                        equipmentSpeedMod += kv.Value;
                    }
                }
            }
            if (equipmentSpeedMod < -1)
                equipmentSpeedMod = -1;
            //AddDebug("equipmentSpeedMod " + equipmentSpeedMod);
        }

        private static void GetToolMod(PlayerTool tool = null)
        {
            if (!swimming && oneHandToolWalkSpeedMod == 0 && twoHandToolWalkSpeedMod == 0)
                toolMod = 1;
            else if (swimming && oneHandToolSwimSpeedMod == 0 && twoHandToolSwimSpeedMod == 0)
                toolMod = 1;

            float oneHandToolSpeedMod = swimming ? oneHandToolSwimSpeedMod : oneHandToolWalkSpeedMod;
            float twoHandToolSpeedMod = swimming ? twoHandToolSwimSpeedMod : twoHandToolWalkSpeedMod;

            if (Player.main.pda.isOpen && oneHandToolSpeedMod > 0)
                toolMod = oneHandToolSpeedMod;
            else
            {
                toolMod = 1;
                if (tool == null)
                    tool = Inventory.main.GetHeldTool();

                if (tool)
                {
                    bool oneHanded = Util.IsOneHanded(tool);
                    //AddDebug("AlterMaxSpeed tool oneHanded " + oneHanded);
                    if (oneHanded)
                        toolMod = oneHandToolSpeedMod;
                    else
                        toolMod = twoHandToolSpeedMod;

                    //toolMod = 1 - toolMod;
                    //AddDebug("GetToolMod toolMod " + toolMod);
                }
            }
        }

        static void HandleSeaglide(Seaglide seaglide)
        {
            //AddDebug("HandleSeaglide " + seaglide.activeState);
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

        [HarmonyPatch(typeof(uGUI_PDA))]
        class uGUI_PDA_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnPDAOpened")]
            static void OnPDAOpenedPostfix(uGUI_PDA __instance)
            {
                GetToolMod();
            }
            [HarmonyPostfix, HarmonyPatch("OnPDAClosed")]
            static void OnPDAClosedPostfix(uGUI_PDA __instance)
            {
                GetToolMod();
            }
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnAddItem")]
            static void OnAddItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnAddItem");
                GetInvMod();
            }
            [HarmonyPostfix, HarmonyPatch("OnRemoveItem")]
            static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnRemoveItem");
                GetInvMod();
            }
            [HarmonyPostfix, HarmonyPatch("OnEquip")]
            static void OnEquipPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnEquip");
                GetEquipmentMod();
            }
            [HarmonyPostfix, HarmonyPatch("OnUnequip")]
            static void OnUnequipPostfix(Inventory __instance, InventoryItem item)
            {
                //AddDebug("OnUnequip");
                GetEquipmentMod();
            }
        }

        [HarmonyPatch(typeof(Seaglide))]
        class Seaglide_Patch
        {
            [HarmonyPrefix, HarmonyPatch("UpdateActiveState")]
            public static bool UpdateActiveStatePrefix(Seaglide __instance)
            {
                if (!ConfigToEdit.seaglideWorksOnlyForward.Value)
                    return true;

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

            [HarmonyPrefix, HarmonyPatch("SetVFXActive")]
            public static void SetVFXActivePrefix(Seaglide __instance)
            {// seaglide out of power
                GetToolMod();
            }
        }


        [HarmonyPatch(typeof(UnderwaterMotor))]
        class UnderwaterMotor_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("UpdateMove")]
            public static void UpdateMovePostfix(UnderwaterMotor __instance, ref Vector3 __result)
            {
                if (ConfigMenu.playerWaterSpeedMult.Value > 1f)
                    __instance.rb.drag /= ConfigMenu.playerWaterSpeedMult.Value;
                //AddDebug("UpdateMove rb.drag  " + __instance.rb.drag);
            }
            [HarmonyPrefix, HarmonyPatch("AlterMaxSpeed")]
            public static bool AlterMaxSpeedPrefix(UnderwaterMotor __instance, float inMaxSpeed, ref float __result)
            {
                if (waterSpeedEquipment.Count > 0)
                {
                    //AddDebug("waterSpeedEquipment.Count " + waterSpeedEquipment.Count);
                    //AddDebug("AlterMaxSpeed.inMaxSpeed " + inMaxSpeed);
                    __result = inMaxSpeed;
                    if (Player.main.gameObject.transform.position.y > Player.main.GetWaterLevel())
                        __result *= 1.3f;

                    return false;
                }
                return true;
            }
            [HarmonyPostfix, HarmonyPatch("AlterMaxSpeed")]
            public static void Postfix(UnderwaterMotor __instance, float inMaxSpeed, ref float __result)
            {// 
                float mod = 1;
                mod += equipmentSpeedMod;
                //AddDebug("AlterMaxSpeed Postfix " + __result);
                //AddDebug("equipmentSpeedMod " + equipmentSpeedMod);
                //AddDebug("toolMod " + toolMod);
                if (seaglide)
                {
                    HandleSeaglide(seaglide);
                    if (!seaglide.activeState)
                        mod *= toolMod;
                }
                else
                    mod *= toolMod;

                Vector3 input = __instance.movementInputDirection;
                if (playerSidewardSpeedMod > 0 && input.x != 0)
                    mod *= playerSidewardSpeedMod * Mathf.Abs(input.normalized.x);

                if (playerBackwardSpeedMod > 0 && input.z < 0)
                    mod *= playerBackwardSpeedMod * Mathf.Abs(input.normalized.z);

                if (playerVerticalSpeedMod > 0 && input.y != 0)
                    mod *= playerVerticalSpeedMod * Mathf.Abs(input.normalized.y);

                __result *= ConfigMenu.playerWaterSpeedMult.Value;
                __result *= mod;
                //AddDebug("mod " + mod);
                //AddDebug("__result " + __result);
                bool canMove = __result > 0;
                if (ConfigMenu.invMultWater.Value > 0f)
                    __result *= invItemsMod;

                if (canMove && __result == 0)
                    AddDebug(Language.main.Get("TF_too_much_weight_message"));

                //if (UnityEngine.Input.GetKey(KeyCode.V))
                //{
                //    float ms = Player.main.rigidBody.velocity.magnitude;
                //    AddDebug("movement Speed  " + ms);
                //}
            }
        }

        [HarmonyPatch(typeof(Player), "UpdateMotorMode")]
        class Player_UpdateMotorMode_Patch
        {
            static void Postfix(Player __instance)
            {// SetMotorMode does not fire when player goes from water surface to ground
                if (swimming != __instance.IsUnderwaterForSwimming())
                {
                    //AddDebug("UpdateMotorMode update swimming");
                    swimming = __instance.IsUnderwaterForSwimming();
                    UpdateModifiers();
                }
            }
        }

        [HarmonyPatch(typeof(GroundMotor), "ApplyInputVelocityChange")]
        class GroundMotor_Patch
        {
            public static bool Prefix(GroundMotor __instance, ref Vector3 __result, ref Vector3 velocity)
            {
                if (!Main.gameLoaded)
                    return false;

                Vector3 input = __instance.movementInputDirection;
                float x = input.normalized.x;
                float z = input.normalized.z;
                float mod = 1;
                //AddDebug("ApplyInputVelocityChange input " + input);
                //AddDebug("velocity " + velocity);
                if (playerSidewardSpeedMod > 0 && input.x != 0)
                    x = input.normalized.x * playerSidewardSpeedMod;

                if (playerBackwardSpeedMod > 0 && input.z < 0)
                    z = input.normalized.z * playerBackwardSpeedMod;

                if (x != input.normalized.x || z != input.normalized.z)
                    __instance.movementInputDirection = new Vector3(x, input.y, z);

                mod += equipmentSpeedMod;
                //AddDebug(" mod GetEquipmentMod: " + mod);
                mod *= toolMod;
                bool canMove = mod > 0;
                if (ConfigMenu.invMultLand.Value > 0f)
                    mod *= invItemsMod;

                //AddDebug("ApplyInputVelocityChange mod: " + mod);
                if (canMove && mod == 0)
                    AddDebug(Language.main.Get("TF_too_much_weight_message"));

                __instance.forwardMaxSpeed = __instance.playerController.walkRunForwardMaxSpeed * ConfigMenu.playerGroundSpeedMult.Value * mod;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerTool))]
        class PlayerTool_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnDraw")]
            static void OnDrawPostfix(PlayerTool __instance)
            {
                if (__instance is Seaglide)
                    seaglide = __instance as Seaglide;

                GetToolMod(__instance);
                //AddDebug("OnDraw tool " + __instance.name);
            }
            [HarmonyPostfix, HarmonyPatch("OnHolster")]
            static void OnHolsterPostfix(PlayerTool __instance)
            {
                //AddDebug("OnHolster");
                seaglide = null;
                toolMod = 1;
            }
        }

        //[HarmonyPatch(typeof(Player), "SetMotorMode")]
        class Player_SetMotorMode_Patch
        {
            static void Prefix(Player __instance, Player.MotorMode newMotorMode)
            {// 
                AddDebug("Player SetMotorMode " + newMotorMode);
                AddDebug("Player SetMotorMode " + __instance.IsUnderwaterForSwimming());
                //if (swimming != __instance.IsUnderwaterForSwimming())
                //{
                //    AddDebug("UpdateMotorMode update swimming");
                //    swimming = __instance.IsUnderwaterForSwimming();
                //    GetEquipmentMod();
                //    GetToolMod();
                //    GetInvMod();
                //}
            }
        }

        //[HarmonyPatch(typeof(PlayerController)), HarmonyPatch("SetMotorMode")]
        class PlayerController_SetMotorMode_Patch
        {
            static void Postfix(PlayerController __instance, Player.MotorMode newMotorMode)
            {// newMotorMode is Run when player surfaces. This does not fire when player goes from water surface to ground
                AddDebug("SetMotorMode " + newMotorMode);
                //bool swimming = newMotorMode == Player.MotorMode.Dive || newMotorMode == Player.MotorMode.Seaglide;
                AddDebug("SetMotorMode swimming " + swimming);
                AddDebug("SetMotorMode isUnderwaterForSwimming " + Player.main.IsUnderwaterForSwimming());
                GetEquipmentMod();
                GetToolMod();
                GetInvMod();
            }
        }




    }
}
