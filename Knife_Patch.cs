﻿using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;

namespace Tweaks_Fixes
{
    class Knife_Patch
    {
        [HarmonyPatch(typeof(Knife), nameof(Knife.OnToolUseAnim))]
        class Knife_OnToolUseAnim_Patch
        {
            public static void Postfix(Knife __instance)
            {
                BreakableResource breakableResource = Main.guiHand.activeTarget.GetComponent<BreakableResource>();
                if (breakableResource)
                {
                    breakableResource.BreakIntoResources();
                    //ErrorMessage.AddDebug("BreakableResource");
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
                    //ErrorMessage.AddDebug(" attackDist  " + knife.attackDist);
                    //ErrorMessage.AddDebug(" damage  " + knife.damage);
                }

            }
        }

    }
}