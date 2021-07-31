using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Tools_Patch
    {
        [HarmonyPatch(typeof(Knife), nameof(Knife.OnToolUseAnim))]
        class Knife_OnToolUseAnim_Postfix_Patch
        {
            public static void Postfix(Knife __instance)
            {
                if (!Main.guiHand.activeTarget)
                    return;

                BreakableResource breakableResource = Main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //AddDebug("BreakableResource");
                }
                Pickupable pickupable = Main.guiHand.activeTarget.GetComponent<Pickupable>();
                if (pickupable)
                {
                    TechType techType = pickupable.GetTechType();
                    if (Main.config.notPickupableResources.Contains(techType))
                    {
                        Rigidbody rb = pickupable.GetComponent<Rigidbody>();
                        if (rb && rb.isKinematic)  // attached to wall
                            pickupable.OnHandClick(Main.guiHand);
                    }
                }
            }
        } 

        [HarmonyPatch(typeof(PlayerTool), nameof(PlayerTool.OnDraw))]
        class Knife_Awake_Patch
        {
            static float knifeRangeDefault = 0f;
            static float knifeDamageDefault = 0f;

            public static void Postfix(PlayerTool __instance)
            {
                Knife knife = __instance as Knife;
                if (knife)
                {
                    if (knifeRangeDefault == 0f)
                        knifeRangeDefault = knife.attackDist;
                    if (knifeDamageDefault == 0f)
                        knifeDamageDefault = knife.damage;

                    knife.attackDist = knifeRangeDefault * Main.config.knifeRangeMult;
                    knife.damage = knifeDamageDefault * Main.config.knifeDamageMult;
                    //AddDebug(" attackDist  " + knife.attackDist);
                    //AddDebug(" damage  " + knife.damage);
                }
            }
        }


        [HarmonyPatch(typeof(ScannerTool), "Update")]
        class ScannerTool_Update_Patch
        {// SHOW power when equipped
            private static bool Prefix(ScannerTool __instance)
            {
                //PlayerTool playerTool = 
                //bool isDrawn = (bool)PlayerTool_get_isDrawn.Invoke(__instance, new object[] { });
                if (__instance.isDrawn)
                {
                    //float idleTimer = (float)ScannerTool_idleTimer.GetValue(__instance);
                    //AddDebug("useText1 " + HandReticle.main.useText1);
                    //AddDebug("useText2 " + HandReticle.main.useText2);
                    if (__instance.idleTimer > 0f)
                    {
                        __instance.idleTimer = Mathf.Max(0f, __instance.idleTimer - Time.deltaTime);
                        //string buttonFormat = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
                        //               HandReticle.main.SetUseTextRaw(buttonFormat, null);
                    }
                }
                return false;
            }
        }

        static float originalIntensity = -1f;
        [HarmonyPatch(nameof(Flare.OnDraw))]
        [HarmonyPostfix]
        internal static void OnDraw(Flare __instance)
        {
            if (originalIntensity == -1f)
                originalIntensity = __instance.originalIntensity;

            //AddDebug("throwDuration " + __instance.throwDuration);
            __instance.originalIntensity = originalIntensity * Main.config.flareIntensity;
        }

        [HarmonyPatch(typeof(Flare), "OnDrop")]
        class Flare_OnDrop_Patch
        {
            public static bool Prefix(Flare __instance)
            {
                if (__instance.isThrowing)
                {
                    __instance.GetComponent<Rigidbody>().AddForce(MainCamera.camera.transform.forward * __instance.dropForceAmount);
                    __instance.GetComponent<Rigidbody>().AddTorque(__instance.transform.right * __instance.dropTorqueAmount);
                    __instance.isThrowing = false;
                }
                //AddDebug("energyLeft " + __instance.energyLeft);
                if (__instance.energyLeft < 1800f)
                {
                    if (__instance.fxControl && !__instance.fxIsPlaying)
                        __instance.fxControl.Play(1);
                    __instance.fxIsPlaying = true;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Constructor), "OnEnable")]
        class Constructor_OnEnable_Patch
        {
            static void Postfix(Constructor __instance)
            {
                //AddDebug("OnEnable Constructor ");
                ImmuneToPropulsioncannon itpc = __instance.GetComponent<ImmuneToPropulsioncannon>();
                //itpc.enabled = false;
                UnityEngine.Object.Destroy(itpc);
            }
        }

        //[HarmonyPatch(typeof(RepulsionCannon), "ShootObject")]
        class RepulsionCannon_ShootObject_Patch
        {
            static void Prefix(RepulsionCannon __instance, Rigidbody rb, Vector3 velocity)
            {
                rb.constraints = RigidbodyConstraints.None;
                AddDebug("ShootObject " + rb.gameObject.name + " " + velocity);
                //AddDebug("constraints " + rb.constraints);
            }
        }

        //[HarmonyPatch(typeof(RepulsionCannon), "OnToolUseAnim")]
        class RepulsionCannon_OnToolUseAnim_Patch
        {
            static bool Prefix(RepulsionCannon __instance, GUIHand guiHand)
            {
                if (__instance.energyMixin.charge <= 0f)
                    return false;

                float num1 = Mathf.Clamp01(__instance.energyMixin.charge / 4f);
                Vector3 forward = MainCamera.camera.transform.forward;
                Vector3 position = MainCamera.camera.transform.position;
                int num2 = UWE.Utils.SpherecastIntoSharedBuffer(position, 1f, forward, 35f, ~(1 << LayerMask.NameToLayer("Player")));
                float num3 = 0.0f;
                for (int index1 = 0; index1 < num2; ++index1)
                {
                    RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[index1];
                    Vector3 point = raycastHit.point;
                    float num4 = 1f - Mathf.Clamp01(((position - point).magnitude - 1f) / 15f);
                    AddDebug("point " + point);
                    AddDebug("position " + position);
                    AddDebug("magnitude " + (position - point).magnitude);
                    GameObject go = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
                    if (go == null)
                        go = raycastHit.collider.gameObject;
                    Rigidbody component = go.GetComponent<Rigidbody>();
                    if (component != null)
                    {
                        num3 += component.mass;
                        bool flag = true;
                        go.GetComponents<IPropulsionCannonAmmo>(__instance.iammo);
                        for (int index2 = 0; index2 < __instance.iammo.Count; ++index2)
                        {
                            if (!__instance.iammo[index2].GetAllowedToShoot())
                            {
                                flag = false;
                                break;
                            }
                        }
                        __instance.iammo.Clear();
                        if (flag && !(raycastHit.collider is MeshCollider) && (go.GetComponent<Pickupable>() != null || go.GetComponent<Living>() != null || go.GetComponent<EscapePod>() != null || component.mass <= 1300.0 && UWE.Utils.GetAABBVolume(go) <= 400.0))
                        {
                            float num5 = (1.0f + component.mass * 0.00499999988824129f);
                            Vector3 velocity = forward * num4 * num1 * 70f / num5;
                            //AddDebug("num4 " + num4);
                            //AddDebug("num1 " + num1);
                            //AddDebug("num5 " + num5);
                            __instance.ShootObject(component, velocity);
                        }
                    }
                }
                __instance.energyMixin.ConsumeEnergy(4f);
                __instance.fxControl.Play();
                __instance.callBubblesFX = true;
                Utils.PlayFMODAsset(__instance.shootSound, __instance.transform);
                float num6 = Mathf.Clamp(num3 / 100f, 0.0f, 15f);
                Player.main.GetComponent<Rigidbody>().AddForce(-forward * num6, ForceMode.VelocityChange);

                return false;
            }
        }


    }
}
