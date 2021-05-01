using System;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using HarmonyLib;
using System.Text;

namespace Tweaks_Fixes
{
    internal class Player_Movement
    {
        static float oceanLevel;
        //static int invSize;
        static Equipment equipment;
        static Survival survival;
        static float swimMaxAllowedY = .6f; // .6

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Postfix(Player __instance)
            {
                oceanLevel = Ocean.main.GetOceanLevel();
                //invSize = Inventory.main.container.sizeX * Inventory.main.container.sizeY;
                equipment = Inventory.main.equipment;
                survival = Player.main.GetComponent<Survival>();
            }
        }

        public static float GetWeaknessSpeedMult()
        {
            float foodPen = 1f;
            float waterPen = 1f;
            if (Main.survival.food < 0f)
            {
                foodPen = Mathf.Abs(Main.survival.food / 100f);
                foodPen = 1f - foodPen;
            }
            if (Main.survival.water < 0f)
            {
                waterPen = Mathf.Abs(Main.survival.water / 100f);
                waterPen = 1f - waterPen;
            }
            return (foodPen + waterPen) * .5f;
        }

        public static float GetInvMult()
        {
            float massTotal = 0f;
            foreach (InventoryItem inventoryItem in Inventory.main.container)
            {
                Rigidbody rb = inventoryItem.item.GetComponent<Rigidbody>();
                if (rb)
                    massTotal += rb.mass;
            }

            float mult;
            if (Player.main.IsSwimming())
                mult = 100f - massTotal * Main.config.invMultWater;
            else
                mult = 100f - massTotal * Main.config.invMultLand;      

            //float mult = massTotal * Main.config.InvMult;
            mult = Mathf.Clamp(mult, 0f, 100f);
            return mult * .01f;
        }

        [HarmonyPatch(typeof(Seaglide), "UpdateActiveState")]
        internal class Seaglide_UpdateActiveState_Patch
        { // seaglide works only if moving forward
            public static bool Prefix(Seaglide __instance)
            {
                if (!Main.config.playerMoveSpeedTweaks)
                    return true;

                int num1 = __instance.activeState ? 1 : 0;
                __instance.activeState = false;
                if (__instance.energyMixin.charge > 0f)
                {
                    if (__instance.screenEffectModel != null)
                        __instance.screenEffectModel.SetActive(__instance.usingPlayer != null);
                    if (__instance.usingPlayer != null && __instance.usingPlayer.IsSwimming())
                    {
                        Vector3 moveDirection = GameInput.GetMoveDirection();
                        __instance.activeState = moveDirection.z > 0.0;
                    }
                    if (__instance.powerGlideActive)
                        __instance.activeState = true;
                }
                int num2 = __instance.activeState ? 1 : 0;
                if (num1 == num2)
                    return false;
                __instance.SetVFXActive(__instance.activeState);
                return false;
            }
        }

        [HarmonyPatch(typeof(UnderwaterMotor), "AlterMaxSpeed")]
        internal class AlterMaxSpeedPatch
        {
            public static bool Prefix(UnderwaterMotor __instance, float inMaxSpeed, ref float __result)
            {
                if (!Main.config.playerMoveSpeedTweaks)
                    return true;

                //if (Main.pda == null)
                //    ErrorMessage.AddDebug("pda = null ");
                //if (Inventory.main.container == null)
                //    ErrorMessage.AddDebug("Inventory.main.container = null ");
                //if (equipment == null)
                //    ErrorMessage.AddDebug("equipment = null ");
                //if (survival == null)
                //    ErrorMessage.AddDebug("survival = null ");

                //ErrorMessage.AddDebug("AlterMaxSpeed");
                __result = inMaxSpeed;

                TechType suit = equipment.GetTechTypeInSlot("Body");
                if (suit != TechType.None)  
                    __result *= 0.9f;

                if (Player.main.motorMode != Player.MotorMode.Seaglide)
                    Utils.AdjustSpeedScalarFromWeakness(ref __result);

                TechType fins = equipment.GetTechTypeInSlot("Foots");
                if (fins == TechType.Fins)
                    __result *= 1.2f;
                else if (fins == TechType.UltraGlideFins)
                    __result *= 1.3f;

                TechType tank = equipment.GetTechTypeInSlot("Tank");
                if (tank == TechType.Tank || tank == TechType.DoubleTank || tank == TechType.HighCapacityTank)
                    __result *= 0.9f;

    
                if (Main.pda.isInUse)
                    __result *= 0.5f;
                else 
                {
                    PlayerTool tool = Inventory.main.GetHeldTool();
                    if (tool && !(tool is Seaglide))
                        __result *= 0.7f;                      
                }
                if (Player.main.gameObject.transform.position.y > oceanLevel)
                    __result *= 1.3f;

                //float ms = (float)System.Math.Round(Player.main.movementSpeed * 10f) / 10f;
                //Main.Message("movementSpeed  " + ms);
                return false;
            }


            public static void Postfix(float inMaxSpeed, ref float __result)
            {
                if (Main.config.playerMoveSpeedTweaks && Main.config.invMultWater > 0f)
                    __result *= GetInvMult();
                //__instance.movementSpeed = __instance.playerController.velocity.magnitude / 5f;
                //float ms = (float)System.Math.Round(Player.main.movementSpeed * 10f) / 10f;
                //ms = Player.main.rigidBody.velocity.magnitude;
                //Main.Message("movementSpeed  " + ms);
            }
        }

        //[HarmonyPatch(typeof(PlayerController), "SetMotorMode")]
        class PlayerController_SetMotorMode_Patch
        {
            static void Postfix(PlayerController __instance)
            {
                if (Main.config.playerMoveSpeedTweaks)
                { // underWaterController ignores these
                    //ErrorMessage.AddDebug("SetMotorMode");
                    __instance.underWaterController.backwardMaxSpeed = __instance.underWaterController.forwardMaxSpeed * .5f;
                    __instance.underWaterController.strafeMaxSpeed = __instance.underWaterController.backwardMaxSpeed;
                    __instance.underWaterController.verticalMaxSpeed = __instance.underWaterController.backwardMaxSpeed;

                    __instance.groundController.backwardMaxSpeed = __instance.groundController.forwardMaxSpeed * .5f;
                    __instance.groundController.strafeMaxSpeed = __instance.groundController.backwardMaxSpeed;
                    __instance.groundController.verticalMaxSpeed = __instance.groundController.backwardMaxSpeed;
                    //ErrorMessage.AddDebug("backwardMaxSpeed " + __instance.underWaterController.backwardMaxSpeed);
                    //Main.Log("forwardMaxSpeed " + __instance.forwardMaxSpeed);
                    //Main.Log("backwardMaxSpeed " + __instance.backwardMaxSpeed);
                }
            }
        }

        //[HarmonyPatch(typeof(PlayerController), "UpdateController")]
        class PlayerController_UpdateController_Patch
        {
            static void MovePlayer(PlayerMotor playerMotor)
            {
                if (playerMotor.playerController == null || playerMotor.playerController.forwardReference == null)
                    return;

                UnderwaterMotor underwaterMotor = playerMotor as UnderwaterMotor;
                float forwardForce = 4f;
                float backwardForce = 2f;
                float sidewardForce = 2f;
                float verticalForce = 2f;
                Rigidbody rb = playerMotor.rb;
                Vector3 velocity = rb.velocity;
                Vector3 input = playerMotor.movementInputDirection;

                input.Normalize();
                float z = input.z > 0 ? input.z * forwardForce : input.z * backwardForce;
                //ErrorMessage.AddDebug("z   " + z);
                Vector3 acceleration = new Vector3(input.x * sidewardForce, input.y * verticalForce, z);
                //Vector3 inputRotated = playerMotor.playerController.forwardReference.rotation * input;
                //Main.Message("acceleration magnitude " + acceleration.magnitude);
                acceleration = playerMotor.transform.rotation * acceleration * Time.deltaTime;
                underwaterMotor.desiredVelocity = rb.velocity;
                rb.AddForce(acceleration, ForceMode.VelocityChange);
            }

            static bool Prefix(PlayerController __instance)
            {
                if (Main.config.playerMoveSpeedTweaks)
                {
                    __instance.HandleUnderWaterState();
                    float num = UWE.Utils.Slerp(__instance.currentControllerHeight, __instance.desiredControllerHeight, Time.deltaTime * 2f);
                    float maxDistance = num - __instance.currentControllerHeight;
                    bool flag = true;
                    if (maxDistance > 0f)
                    {
                        Vector3 vector3 = __instance.transform.position + new Vector3(0.0f, __instance.currentControllerHeight * 0.5f, 0.0f);
                        flag = !Physics.CapsuleCast(vector3, vector3, __instance.controllerRadius + 0.01f, Vector3.up, out RaycastHit _, maxDistance, -524289);
                    }
                    if (flag)
                        __instance.currentControllerHeight = num;
                    __instance.underWaterController.SetControllerHeight(__instance.currentControllerHeight);
                    __instance.groundController.SetControllerHeight(__instance.currentControllerHeight);
                    __instance.velocity = __instance.activeController.UpdateMove();
                    //__instance.activeController.UpdateMove();
                    //MovePlayer(__instance.activeController);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UnderwaterMotor), "UpdateMove")]
        internal class UnderwaterMotor_UpdateMove_Patch
        { // strafe speed halfed, backward speed halfed, no speed reduction near wrecks 
            public static bool Prefix(UnderwaterMotor __instance, ref Vector3 __result)
            {
                if (!Main.config.playerMoveSpeedTweaks)
                    return true;

                Rigidbody rb = __instance.rb;
                if (__instance.playerController == null || __instance.playerController.forwardReference == null)
                { 
                    __result = rb.velocity;
                    return false;
                }
                __instance.fastSwimMode = Application.isEditor && UnityEngine.Input.GetKey(KeyCode.LeftShift);
                Vector3 velocity = rb.velocity;
                Vector3 input = __instance.movementInputDirection;
                Vector3 inputRaw = input;
                input.Normalize();
                input.y *= .5f;
                input.x *= .5f;
                if (input.z < 0f)
                    input.z *= .5f;
                float y = input.y;
                float num1 = Mathf.Min(1f, input.magnitude);
                input.y = 0.0f;

                float a = 0.0f;
                if (input.z > 0f)
                    a = __instance.forwardMaxSpeed;
                else if (input.z < 0f)
                    a = -__instance.backwardMaxSpeed;
                if (input.x != 0f)
                    a = Mathf.Max(a, __instance.strafeMaxSpeed);
                float num2 = __instance.AlterMaxSpeed(Mathf.Max(a, __instance.verticalMaxSpeed));
                //if (Player.main.GetBiomeString() == "wreck" & Player.main.motorMode == Player.MotorMode.Seaglide)
                    //num2 = __instance.seaglideWreckMaxSpeed;
                float num3 = num2 * Player.main.mesmerizedSpeedMultiplier;
                if (__instance.fastSwimMode)
                    num3 *= 1000f;
                float b = num3 * __instance.debugSpeedMult;
                float num4 = Mathf.Max(velocity.magnitude, b);
                Vector3 vector3_2 = __instance.playerController.forwardReference.rotation * input;
                input = vector3_2;
                input.y += y;
                input.Normalize();
                if (!__instance.canSwim)
                {
                    input.y = 0.0f;
                    input.Normalize();
                }
                float acceleration = __instance.airAcceleration;
                if (__instance.grounded)
                    acceleration = __instance.groundAcceleration;
                else if (__instance.underWater)
                {
                    acceleration = __instance.acceleration;
                    //if (Player.main.GetBiomeString() == "wreck")
                    //    num5 *= 0.5f;
                    if (inputRaw.z > 0 && Player.main.motorMode == Player.MotorMode.Seaglide)
                    {
                        acceleration *= 1.45f;
                    }
                }
                float num7 = (num1 * acceleration) * Time.deltaTime;
                if (num7 > 0f)
                {
                    Vector3 lhs = velocity + input * num7;
                    if (lhs.magnitude > num4)
                    {
                        lhs.Normalize();
                        lhs *= num4;
                    }
                    float num8 = Vector3.Dot(lhs, __instance.surfaceNormal);
                    if (!__instance.canSwim)
                        lhs -= num8 * __instance.surfaceNormal;
                    bool flag1 = vector3_2.y > swimMaxAllowedY;
                    bool flag2 = vector3_2.y < -0.3f;
                    //bool flag2 = vector3_2.y < -0.0f;
                    bool flag3 = y < 0f;
                    //bool flag3 = y < -0.3f;
                    if (__instance.transform.position.y >= swimMaxAllowedY && !flag1 && (!flag2 && !flag3))
                        lhs.y = 0.0f;
                    rb.velocity = lhs;
                    __instance.desiredVelocity = lhs;
                }
                else
                    __instance.desiredVelocity = rb.velocity;
                float gravity = __instance.underWater ? __instance.underWaterGravity : __instance.gravity;
                if (gravity != 0f)
                {
                    rb.AddForce(new Vector3(0.0f, -gravity * Time.deltaTime, 0.0f), ForceMode.VelocityChange);
                    __instance.usingGravity = true;
                }
                else
                    __instance.usingGravity = false;
                float drag = __instance.airDrag;
                if (__instance.underWater)
                    drag = __instance.swimDrag;
                else if (__instance.grounded)
                    drag = __instance.groundDrag;
                rb.drag = drag;
                InertiaGene component = __instance.gameObject.GetComponent<InertiaGene>();
                if (component)
                    rb.drag -= component.Scalar * rb.drag;
                if (__instance.fastSwimMode)
                    rb.drag = 0.0f;
                __instance.grounded = false;
                __instance.vel = rb.velocity;
                __result = __instance.vel;
                return false;
            }
        }

        private static float AdjustGroundSpeed(float maxSpeed)
        {
            Utils.AdjustSpeedScalarFromWeakness(ref maxSpeed);

            TechType suit = equipment.GetTechTypeInSlot("Body");
            if (suit != TechType.None)
                maxSpeed *= 0.9f;
            TechType fins = equipment.GetTechTypeInSlot("Foots");
            if (fins != TechType.None)
                maxSpeed *= 0.9f;

            TechType tank = equipment.GetTechTypeInSlot("Tank");
            if (tank == TechType.Tank || tank == TechType.DoubleTank || tank == TechType.HighCapacityTank)
                maxSpeed *= 0.9f;
            else if (tank == TechType.PlasteelTank)
                maxSpeed *= 0.95f;

            if (Main.config.invMultWater > 0f)
                maxSpeed *= GetInvMult();

            //ErrorMessage.AddDebug("AdjustGroundSpeed " + maxSpeed);
            return maxSpeed;
        }

        [HarmonyPatch(typeof(GroundMotor), "ApplyInputVelocityChange")]
        internal class GroundMotor_ApplyInputVelocityChange_Patch
        {// can sprint only if moving forward 
            public static bool Prefix(GroundMotor __instance, ref Vector3 __result, Vector3 velocity)
            {
                if (!Main.config.playerMoveSpeedTweaks)
                    return true;

                //ErrorMessage.AddDebug("movementInputDirection "+ __instance.movementInputDirection);
                if (__instance.playerController == null || __instance.playerController.forwardReference == null)
                { 
                    __result = Vector3.zero;
                    return false;
                }
                Quaternion quaternion = !__instance.underWater || !__instance.canSwim ? Quaternion.Euler(0.0f, __instance.playerController.forwardReference.rotation.eulerAngles.y, 0.0f) : __instance.playerController.forwardReference.rotation;
                Vector3 input = __instance.movementInputDirection;
                input.Normalize();
                input.x *= .5f;
                if (input.z < 0f)
                    input.z *= .5f;

                float num1 = Mathf.Min(1f, input.magnitude);
                float num2 = !__instance.underWater || !__instance.canSwim ? 0.0f : input.y;
                input.y = 0.0f;
                input = quaternion * input;
                input.y += num2;
                input.Normalize();
                Vector3 hVelocity;
                if (__instance.grounded && !__instance.underWater && (__instance.TooSteep() && __instance.sliding.enabled))
                {
                    Vector3 normalized = new Vector3(__instance.groundNormal.x, 0.0f, __instance.groundNormal.z).normalized;
                    Vector3 vector3_3 = Vector3.Project(__instance.movementInputDirection, normalized);
                    hVelocity = (normalized + vector3_3 * __instance.sliding.speedControl + (__instance.movementInputDirection - vector3_3) * __instance.sliding.sidewaysControl) * __instance.sliding.slidingSpeed;
                }
                else
                {
                    float maxSpeed = 1f;
                    //Utils.AdjustSpeedScalarFromWeakness(ref maxSpeed);
                    //ErrorMessage.AddDebug("maxSpeed " + maxSpeed);
                    if (!__instance.underWater && __instance.sprintPressed && __instance.movementInputDirection.z > 0f)
                    {
                        maxSpeed *= __instance.sprintModifier;
                        __instance.sprinting = true;
                    }
                    //if (__instance.sprinting)
                    //    survival.UpdateStats(survival.kUpdateHungerInterval * .333f);
                    maxSpeed = AdjustGroundSpeed(maxSpeed);
                    hVelocity = input * __instance.forwardMaxSpeed * maxSpeed * num1;
                }
                //if (!__instance.underWater && XRSettings.enabled)
                //    hVelocity *= VROptions.groundMoveScale;
                if (!__instance.underWater && __instance.movingPlatform.enabled && __instance.movingPlatform.movementTransfer == GroundMotor.MovementTransferOnJump.PermaTransfer)
                {
                    hVelocity += __instance.movement.frameVelocity;
                    hVelocity.y = 0.0f;
                }
                if (!__instance.underWater)
                {
                    if (__instance.grounded)
                        hVelocity = __instance.AdjustGroundVelocityToNormal(hVelocity, __instance.groundNormal);
                    else
                        velocity.y = 0.0f;
                }
                float num3 = __instance.GetMaxAcceleration(__instance.grounded) * Time.deltaTime;
                Vector3 vector3_5 = hVelocity - velocity;
                if (vector3_5.sqrMagnitude > num3 * num3)
                    vector3_5 = vector3_5.normalized * num3;
                if (__instance.grounded || __instance.canControl)
                    velocity += vector3_5;
                if (__instance.grounded && !__instance.underWater)
                    velocity.y = Mathf.Min(velocity.y, 0.0f);
                __result = velocity;
                //float ms = (float)System.Math.Round(Player.main.movementSpeed * 10f) / 10f;
                //float  ms = Player.main.rigidBody.velocity.magnitude;
                //Main.Message("movementSpeed  " + ms);
                return false;
            }
        }



    }
}
