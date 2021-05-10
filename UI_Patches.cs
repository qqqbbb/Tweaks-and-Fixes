using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class UI_Patches
    {

        [HarmonyPatch(typeof(StorageContainer), "Open", new Type[] { typeof(Transform) })]
        class StorageContainer_Open_Patch
        {// fixes bug: when opening standing locker from a distance your PDA does not open
            public static bool Prefix(StorageContainer __instance, Transform useTransform)
            {
                //float dist = (useTransform.position - Player.main.transform.position).magnitude;
                //AddDebug("Open dist " + dist);
                PDA pda = Player.main.GetPDA();
                Inventory.main.SetUsedStorage((IItemsContainer)__instance.container);
                Transform target = useTransform;
                PDA.OnClose onCloseCallback = new PDA.OnClose(__instance.OnClosePDA);
                //double num = __instance.modelSizeRadius + 2.0;
                if (!pda.Open(PDATab.Inventory, target, onCloseCallback, 4))
                    return false;

                __instance.open = true;
                return false;
            }
        }

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
                    LiveMixin liveMixin = __instance.activeTarget.GetComponent<LiveMixin>();
                    if (liveMixin && !liveMixin.IsAlive())
                    {
                        //AddDebug("health " + liveMixin.health);
                        Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
                        //CreatureEgg ce = liveMixin.GetComponent<CreatureEgg>();
                        //if (ce)
                        //    name = Language.main.Get(ce.overrideEggType);
                        string name = Language.main.Get(techType);
                        //AddDebug("name " + name);
                        if (pickupable)
                        {
                            if (pickupable.overrideTechType != TechType.None)
                                name = Language.main.Get(pickupable.overrideTechType);

                            name = Language.main.GetFormat<string>("DeadFormat", name);
                            HandReticle.main.SetInteractText(name);
                        }
                        else
                        {
                            name = Language.main.GetFormat<string>("DeadFormat", name);
                            HandReticle.main.SetInteractTextRaw(name, string.Empty);
                        }

                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_MainMenu), "Update")]
        class uGUI_MainMenu_Update_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance)
            {
                //AddDebug("lastGroup " +__instance.lastGroup);
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
                if (Main.config.disableHints)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneIntro), "ControlsHints")]
        class uGUI_SceneIntro_ControlsHints_Patch
        {
            static bool Prefix(uGUI_SceneIntro __instance)
            {
                if (Main.config.disableHints)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerWorldArrows), "CreateWorldArrows")]
        internal class PlayerWorldArrows_CreateWorldArrows_Patch
        {
            internal static bool Prefix(PlayerWorldArrows __instance)
            {
                //AddDebug("CreateWorldArrows");
                return !Main.config.disableHints;
            }
        }

        [HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
        class TooltipFactory_ItemCommons_Prefix_Patch
        {
            static void Prefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                CreatureEgg creatureEgg = obj.GetComponent<CreatureEgg>();
                if (creatureEgg)
                {
                    LiveMixin liveMixin = obj.GetComponent<LiveMixin>();
                    if (!liveMixin.IsAlive())
                    {
                        TooltipFactory.WriteTitle(sb, "Dead ");
                    }
                }
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
                if (techType == TechType.FirstAidKit)
                {
                    TooltipFactory.WriteDescription(sb, "Restores " + Main.config.medKitHP + " health.");
                }
            }
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "OnRightSideOpened")]
        class uGUI_MainMenu_OnRightSideOpened_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance, GameObject root)
            {
                AddDebug("OnRightSideOpened " + __instance.GetCurrentSubMenu());
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
                AddDebug("uGUI_MainMenu OnButtonOptions");
            }
        }

    }
}
