using HarmonyLib;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{//need check if exosuit has claw
    [HarmonyPatch(typeof(BreakableResource), nameof(BreakableResource.OnHandClick))]
    public static class OnHandClickPatch
    {
        public static bool Prefix()
        {
            if (!Main.config.noBreakingWithHand || Player.main.inExosuit)
                return true;

            PlayerTool tool = Inventory.main.GetHeldTool();
            if (tool && tool.GetComponent<Knife>() != null)
            //if (!tool.te(TechType.Knife))
            {
                return true;
            }
            else
            {
                //Main.Message("no knife !");
                return false;
            }
        }
    }

    [HarmonyPatch(typeof(BreakableResource), nameof(BreakableResource.OnHandHover))]
    public static class OnHandHoverPatch
    {
        public static bool Prefix(BreakableResource __instance)
        {
            //AddDebug("BreakableResource OnHandHover");
            //if (Player.main.inExosuit)
            //{
            //    HandReticle.main.SetInteractText(__instance.breakText);
            //    return false;
            //}
            //if (__instance.GetComponent<LiveMixin>() != null)
            //    AddDebug("BreakableResource LiveMixin");
             if (!Main.config.noBreakingWithHand || Player.main.inExosuit)
                return true;

            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (knife)
            {
                HandReticle.main.SetInteractText(__instance.breakText);
                //if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                //{
                //    __instance.BreakIntoResources();
                //    AddDebug("RightHand");
                //}
            }
            else
                HandReticle.main.SetInteractTextRaw(Main.config.translatableStrings[15], null);

            return false;
        }
    }

    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatch
    {
        static bool cantPickUp = false;

        [HarmonyPrefix]
        [HarmonyPatch("AllowedToPickUp")]
        public static bool AllowedToPickUpPrefix(Pickupable __instance, ref bool __result)
        {
            //AddDebug("Can Collect " + CanCollect(__instance, __instance.GetTechType()));
            if (!Main.config.noBreakingWithHand)
                return true;

            cantPickUp = false;
            __result = __instance.isPickupable && Time.time - __instance.timeDropped > 1f && Player.main.HasInventoryRoom(__instance);
            if (__result && !Player.main.inExosuit && Main.config.notPickupableResources.Contains(__instance.GetTechType()))
            {
                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                if (rb && rb.isKinematic && Inventory.main.GetHeldTool() as Knife == null)
                {
                    //AddDebug("Need knife to break it free");
                    cantPickUp = true;
                    __result = false;
                }
            }
            return false;
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnHandHover")]
        public static void PickupableOnHandHover(Pickupable __instance)
        {
            if (cantPickUp)
                HandReticle.main.SetInteractTextRaw(Main.config.translatableStrings[16], null);
            //AddDebug("Can Collect " + CanCollect(__instance, __instance.GetTechType()));
        }

        [HarmonyPatch("OnHandClick")]
        [HarmonyPrefix]
        public static bool PickupableOnHandClick(Pickupable __instance)
        {
            if (!Main.config.noBreakingWithHand || Player.main.inExosuit)
                return true;

            if (!Main.config.notPickupableResources.Contains(__instance.GetTechType()))
                return true;

            Rigidbody rb = __instance.GetComponent<Rigidbody>();
            if (rb == null)
                return true;

            if (rb.isKinematic) // attached to wall
            {
                Knife knife = Inventory.main.GetHeldTool() as Knife;
                if (knife)
                {
                    Main.guiHand.usedToolThisFrame = true;
                    knife.OnToolActionStart();
                    rb.isKinematic = false;
                    //return false;
                }
                return false;
            }
            return true;
        }
    }



}
