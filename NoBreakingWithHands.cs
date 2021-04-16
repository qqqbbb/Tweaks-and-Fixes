using HarmonyLib;
using UnityEngine;

namespace Tweaks_Fixes
{
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
            //ErrorMessage.AddDebug("BreakableResource OnHandHover");
            //if (Player.main.inExosuit)
            //{
            //    HandReticle.main.SetInteractText(__instance.breakText);
            //    return false;
            //}
            //if (__instance.GetComponent<LiveMixin>() != null)
            //    ErrorMessage.AddDebug("BreakableResource LiveMixin");
            if (!Main.config.noBreakingWithHand)
                return true;

            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (knife)
            {
                HandReticle.main.SetInteractText(__instance.breakText);
                //if (GameInput.GetButtonDown(GameInput.Button.RightHand))
                //{
                //    __instance.BreakIntoResources();
                //    ErrorMessage.AddDebug("RightHand");
                //}
            }
            else
                HandReticle.main.SetInteractText("Need knife to break it");

            return false;
        }
    }

    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatch
    {
        public static bool CanCollect(Pickupable instance, TechType techType)
        {
            if (!Main.config.noBreakingWithHand || Player.main.inExosuit)
                return true;

            if (Main.config.notPickupableResources.Contains(techType))
            {
                Rigidbody rb = instance.GetComponent<Rigidbody>();
                if (rb == null)
                    return true;

                if (rb.isKinematic)  // attached to terrain
                {
                    Knife knife = Inventory.main.GetHeldTool() as Knife;
                    if (knife)
                    {
                        return true;
                    }
                    HandReticle.main.SetInteractText("Need knife to break it free");
                    return false;
                }
            }
            return true;
        }

        [HarmonyPatch(nameof(Pickupable.OnHandHover))]
        [HarmonyPrefix]
        public static bool PickupableOnHandHover(Pickupable __instance)
        {
            //ErrorMessage.AddDebug("Can Collect " + CanCollect(__instance, __instance.GetTechType()));
            return CanCollect(__instance, __instance.GetTechType());
        }

        [HarmonyPatch(nameof(Pickupable.OnHandClick))]
        [HarmonyPrefix]
        public static bool PickupableOnHandClick(Pickupable __instance)
        {
            if (!Main.config.noBreakingWithHand || Player.main.inExosuit)
                return true;

            if (!Main.config.notPickupableResources.Contains(__instance.GetTechType()))
                return true;

            Rigidbody rb = __instance.GetComponent<Rigidbody>();
            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (rb == null)
                return true;

            if (rb.isKinematic) // attached to wall
            {
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
