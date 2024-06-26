﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Cutefish_Patch
    {
        [HarmonyPatch(typeof(CuteFishHandTarget))]
        class CuteFishHandTarget_patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OnHandHover")]
            public static void OnHandHoverPostfix(CuteFishHandTarget __instance)
            {
          
                if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                {
                    AddDebug("Deconstruct Button");
                    Pickupable p = __instance.GetComponent<Pickupable>();
                    if (p)
                    {
                        p.Pickup();
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("AllowedToInteract")]
            public static void AllowedToInteractPostfix(CuteFishHandTarget __instance, ref bool __result)
            {
                if (ConfigToEdit.fixCuteFish.Value && !Player.main.IsSwimming())
                    __result = false;
            }
        }

        //[HarmonyPatch(typeof(WaterParkCreature))]
        class WaterParkCreature_patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void OnHandHoverPostfix(WaterParkCreature __instance)
            {
                if (__instance.isInside)
                {
                    AddDebug("WaterParkCreature Start " + __instance.name);

                }
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("AllowedToInteract")]
            //public static void AllowedToInteractPostfix(CuteFishHandTarget __instance, ref bool __result)
            //{
            //    if (!Player.main.IsSwimming())
            //        __result = false;
            //}
        }

        //[HarmonyPatch(typeof(CuteFish))]
        class CuteFish_patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            public static void Postfix(CuteFish __instance)
            {
                AddDebug("CuteFish Start");
                __instance.gameObject.EnsureComponent<Pickupable>();
            }
        }

    }
}
