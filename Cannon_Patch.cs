using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;

namespace Tweaks_Fixes
{
    class Cannon_Patch
    {
        [HarmonyPatch(typeof(Constructor), "OnEnable")]
        class Constructor_OnEnable_Patch
        {
            static void Postfix(Constructor __instance)
            {
                //ErrorMessage.AddDebug("OnEnable Constructor ");
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
                ErrorMessage.AddDebug("ShootObject " + rb.gameObject.name + " " + velocity);
                //ErrorMessage.AddDebug("constraints " + rb.constraints);
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
                    ErrorMessage.AddDebug("point " + point);
                    ErrorMessage.AddDebug("position " + position);
                    ErrorMessage.AddDebug("magnitude " + (position - point).magnitude);
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
                            //ErrorMessage.AddDebug("num4 " + num4);
                            //ErrorMessage.AddDebug("num1 " + num1);
                            //ErrorMessage.AddDebug("num5 " + num5);
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
