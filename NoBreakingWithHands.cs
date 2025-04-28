using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{


    [HarmonyPatch(typeof(BreakableResource), "OnHandClick")]
    public static class OnHandClickPatch
    {
        public static bool Prefix()
        {
            if (!ConfigMenu.noBreakingWithHand.Value)
                return true;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
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

    [HarmonyPatch(typeof(BreakableResource), "OnHandHover")]
    public static class OnHandHoverPatch
    {
        public static bool Prefix(BreakableResource __instance)
        {
            Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
            if (!ConfigMenu.noBreakingWithHand.Value && exosuit == null)
                return true;

            if (exosuit)
            {
                if (!exosuit.HasClaw())
                    return false;
                //AddDebug("leftArmType " + exosuit.leftArmType);
                //AddDebug("rightArmType " + exosuit.rightArmType);
                GameInput.Button button;
                if (exosuit.leftArmType == TechType.ExosuitClawArmModule)
                    button = GameInput.Button.LeftHand;
                else
                    button = GameInput.Button.RightHand;

                HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.breakText, true, button);
                //HandReticle.main.SetIcon(HandReticle.IconType.Hand);
                return false;
            }
            if (!ConfigMenu.noBreakingWithHand.Value)
                return true;

            if (!ConfigToEdit.newUIstrings.Value)
                return false;

            Knife knife = Inventory.main.GetHeldTool() as Knife;
            if (knife)
            {
                //HandReticle.main.SetInteractText(__instance.breakText);
                HandReticle.main.SetText(HandReticle.TextType.Hand, __instance.breakText, true, GameInput.Button.LeftHand);
            }
            else
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("TF_need_knife_to_break_outcrop"));
            //HandReticle.main.SetInteractTextRaw(Main.config.translatableStrings[15], null);
            return false;
        }
    }

    [HarmonyPatch(typeof(Pickupable))]
    public static class PickupablePatch
    {
        public static HashSet<TechType> notPickupableResources = new HashSet<TechType>
        {TechType.Salt, TechType.Quartz, TechType.AluminumOxide, TechType.Lithium, TechType.Sulphur, TechType.Diamond, TechType.Kyanite, TechType.Magnetite, TechType.Nickel, TechType.UraniniteCrystal, TechType.JellyPlant  };

        [HarmonyPrefix]
        [HarmonyPatch("AllowedToPickUp")]
        public static bool AllowedToPickUpPrefix(Pickupable __instance, ref bool __result)
        {
            if (!ConfigMenu.noBreakingWithHand.Value)
                return true;

            __result = __instance.isPickupable && Time.time - __instance.timeDropped > 1f && Player.main.HasInventoryRoom(__instance);
            if (__result && !Player.main.inExosuit && notPickupableResources.Contains(__instance.GetTechType()))
            {
                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                if (rb && rb.isKinematic && Inventory.main.GetHeldTool() as Knife == null)
                {
                    //AddDebug("Need knife to break it free");
                    __result = false;
                }
            }
            //AddDebug("Pickupable AllowedToPickUp " + __result);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnHandHover")]
        public static void PickupableOnHandHover(Pickupable __instance)
        {
            if (ConfigMenu.noBreakingWithHand.Value && !__instance.AllowedToPickUp() && notPickupableResources.Contains(__instance.GetTechType()))
            {
                HandReticle.main.SetTextRaw(HandReticle.TextType.Hand, Language.main.Get("TF_need_knife_to_break_free_resource"));
            }
            //HandReticle.main.SetInteractTextRaw(Main.config.translatableStrings[16], null);
            //AddDebug("cantPickUp " + cantPickUp);
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnHandClick")]
        public static bool PickupableOnHandClick(Pickupable __instance)
        {
            //AddDebug("OnHandClick " + __instance.GetTechType());
            if (!ConfigMenu.noBreakingWithHand.Value)
                return true;

            Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
            if (exosuit && exosuit.HasClaw())
                return true;

            if (!notPickupableResources.Contains(__instance.GetTechType()))
                return true;

            Rigidbody rb = __instance.GetComponent<Rigidbody>();
            if (rb == null)
                return true;

            if (rb.isKinematic) // attached to wall
            {
                Knife knife = Inventory.main.GetHeldTool() as Knife;
                if (knife)
                {
                    Player.main.guiHand.usedToolThisFrame = true;
                    knife.OnToolActionStart();
                    if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
                        CoroutineHost.StartCoroutine(MakeKinematic(rb));
                    else
                        rb.isKinematic = false;
                }
                return false;
            }
            return true;
        }

        static IEnumerator MakeKinematic(Rigidbody rb)
        {
            yield return new WaitForSeconds(.25f);
            rb.isKinematic = false;
        }

    }



}
