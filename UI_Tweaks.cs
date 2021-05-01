using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Text;

namespace Tweaks_Fixes
{
    class UI_Tweaks
    {
        [HarmonyPatch(typeof(GUIHand), "OnUpdate")]
        class GUIHand_OnUpdate_Patch
        { // UI tells you if looking at dead fish 
            public static void Postfix(GUIHand __instance)
            {
                if (!__instance.activeTarget)
                    return;
                //Main.Message("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    Main.Message("activeTarget Not Useable layer ");
                TechType techType = CraftData.GetTechType(__instance.activeTarget);
                if (techType != TechType.None)
                {
                    //Main.Message("techType " + techType.AsString());
                    LiveMixin liveMixin = __instance.activeTarget.GetComponent<LiveMixin>();
                    if (liveMixin && !liveMixin.IsAlive())
                    {
                        Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
                        string name = Language.main.Get(techType);
                        name = Language.main.GetFormat<string>("DeadFormat", name);
                        if (pickupable)
                            HandReticle.main.SetInteractText(name);
                        else
                            HandReticle.main.SetInteractTextRaw(name, string.Empty);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), "Update")]
        class uGUI_MainMenu_Update_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance)
            {
                //ErrorMessage.AddDebug("lastGroup " +__instance.lastGroup);
                if (__instance.lastGroup == "SavedGames")
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                        __instance.subMenu.SelectItemInDirection(0, -1);
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                        __instance.subMenu.SelectItemInDirection(0, 1);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    __instance.ShowPrimaryOptions(true);
                    //__instance.rightSide.OpenGroup("Home");
                    __instance.rightSide.OpenGroup("SavedGames");
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_PDA), "Update")]
        class uGUI_PDA_Update_Patch
        {
            public static void Postfix(uGUI_PDA __instance)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                        __instance.OpenTab(__instance.GetNextTab());
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                        __instance.OpenTab(__instance.GetPreviousTab());
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_FeedbackCollector), "HintShow")]
        class uGUI_FeedbackCollector_HintShow_Patch
        {
            static bool Prefix(uGUI_FeedbackCollector __instance)
            {
                if (Main.config.disableGoals)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
        class TooltipFactory_ItemCommons_Patch
        {
            static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (Crush_Damage.crushDepthEquipment.ContainsKey(techType) && Crush_Damage.crushDepthEquipment[techType] > 0)
                {
                    TooltipFactory.WriteDescription(sb, "Increases your safe diving depth by " + Crush_Damage.crushDepthEquipment[techType] + " meters.");
                }
                if (Main.config.invMultLand > 0f || Main.config.invMultWater > 0f)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb)
                        TooltipFactory.WriteDescription(sb, "mass " + rb.mass);
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "OnRightSideOpened")]
        class uGUI_MainMenu_OnRightSideOpened_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance, GameObject root)
            {
                ErrorMessage.AddDebug("OnRightSideOpened " + __instance.GetCurrentSubMenu());
                //__instance.subMenu = root.GetComponentInChildren<uGUI_INavigableIconGrid>();
                //__instance.subMenu.
                //if (Input.GetKey(KeyCode.LeftShift))
                //{
                //if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                //    __instance.OpenTab(__instance.GetNextTab());
                //else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                //    __instance.OpenTab(__instance.GetPreviousTab());
                //}
            }
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "OnButtonOptions")]
        class uGUI_MainMenu_OnButtonOptions_Patch
        {
            public static void Postfix(GUIHand __instance)
            {
                ErrorMessage.AddDebug("uGUI_MainMenu OnButtonOptions");
            }
        }

    }
}
