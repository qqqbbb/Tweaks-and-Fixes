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
        static bool fishTooltip = false;

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
        { 
            static string AltToolButton = string.Empty;

            //[HarmonyPatch(nameof(GUIHand.OnUpdate))]
            //[HarmonyPrefix]
            public static bool OnUpdatePrefix(GUIHand __instance)
            {
                if (!__instance.activeTarget)
                    return true;

                UniqueIdentifier ui = __instance.activeTarget.GetComponentInParent<UniqueIdentifier>();
                if (ui && Plant_Patch.enteredColliders.ContainsKey(ui.gameObject) && Plant_Patch.enteredColliders[ui.gameObject] > 0)
                {
                    HandReticle.main.SetInteractTextRaw(string.Empty, string.Empty);
                    return false;
                }
                return true;
            }
            [HarmonyPatch(nameof(GUIHand.OnUpdate))]
            [HarmonyPostfix]
            public static void OnUpdatePostfix(GUIHand __instance)
            {
                PlayerTool tool = __instance.GetTool();
                if (tool)
                {
                    Flare flare = tool as Flare;
                    if (flare && !Main.flareRepairLoaded)
                    {
                        bool lit = flare.flareActivateTime > 0;
                        string text = string.Empty;
                        string throwFlare = lit ? Main.config.throwFlare : Main.config.lightAndThrowFlare;
                        if (Inventory.CanDropItemHere(tool.GetComponent<Pickupable>(), false))
                            text = throwFlare + " (" + TooltipFactory.stringRightHand + ")";
                        if (string.IsNullOrEmpty(AltToolButton))
                            AltToolButton = uGUI.FormatButton(GameInput.Button.AltTool);

                        if (!lit)
                        {
                            string text1 = Main.config.lightFlare + " (" + AltToolButton + ")";
                            if (string.IsNullOrEmpty(text))
                                text = text1;
                            else
                                text = text + ",  " + text1;

                            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                                Flare_Patch.LightFlare(flare);
                        }
                        HandReticle.main.SetUseTextRaw(text, null);
                    }
                    if (Main.IsEatableFish(tool.gameObject))
                    {
                        string text = string.Empty;
                        if (Inventory.CanDropItemHere(tool.GetComponent<Pickupable>(), false))
                            text = TooltipFactory.stringDrop + " (" + TooltipFactory.stringRightHand + ")";

                        bool cantEat = Main.config.cantEatUnderwater && Player.main.isUnderwater.value;
                        //Main.Log("GameInput.Button.RightHand) " + uGUI.FormatButton(GameInput.Button.RightHand));
                        if (cantEat)
                        { }
                        else
                        {
                            if (string.IsNullOrEmpty(AltToolButton))
                                AltToolButton = uGUI.FormatButton(GameInput.Button.AltTool);

                            string text1 = TooltipFactory.stringEat + " (" + AltToolButton + ")";
                            if (string.IsNullOrEmpty(text))
                                text = text1;
                            else
                                text = text + ",  " + text1;

                            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                            {
                                Eatable eatable = tool.GetComponent<Eatable>();
                                //if (__instance.GetTechType() == TechType.Bladderfish)
                                if (eatable)
                                {
                                    Inventory playerInv = Inventory.main;
                                    playerInv.UseItem(playerInv.quickSlots.heldItem);
                                }
                            }
                        }
                        HandReticle.main.SetUseTextRaw(text, null);
                    }
                }
                if (!__instance.activeTarget)
                    return;
                //Main.Message("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    Main.Message("activeTarget Not Useable layer ");
                TechType techType = CraftData.GetTechType(__instance.activeTarget);
                if (techType == TechType.None)
                    return;
                // UI tells you if looking at dead fish 
                LiveMixin liveMixin = __instance.activeTarget.GetComponent<LiveMixin>();
                if (liveMixin && !liveMixin.IsAlive())
                {
                    //AddDebug("health " + liveMixin.health);
                    Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
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
                return !Main.config.disableHints;
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneIntro), "ControlsHints")]
        class uGUI_SceneIntro_ControlsHints_Patch
        {
            static bool Prefix(uGUI_SceneIntro __instance)
            {
                return !Main.config.disableHints;
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
        class TooltipFactory_ItemCommons_Patch
        {
            static void Prefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (!Main.english)
                    return;

                Flare flare = obj.GetComponent<Flare>();
                if (flare)
                {
                    //AddDebug("flare.energyLeft " + flare.energyLeft);
                    if (flare.energyLeft <= 0f)
                        TooltipFactory.WriteTitle(sb, "Used ");
                    else if (flare.flareActivateTime > 0f)
                        TooltipFactory.WriteTitle(sb, "Lit ");
                }
                fishTooltip = Main.IsEatableFish(obj);
            }
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

        [HarmonyPatch(typeof(Language), "FormatString")]
        class Language_FormatString_Patch
        {
            static void Postfix(string text, ref string __result, object[] args)
            {
                //AddDebug("FormatString " + text);
                //AddDebug("FormatString " + __result);
                if (!fishTooltip || Main.config.eatRawFish == Config.EatingRawFish.Vanilla || args.Length == 0 || args[0].GetType() != typeof(float)  )
                    return;
                //AddDebug("FormatString GetType" + args[0].GetType());
                float value = (float)args[0];
                if (value > 0f && text.Contains("FOOD:") || text.Contains("H₂O:"))
                {
                    string[] tokens = __result.Split(':');
                    if(Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                        __result = tokens[0] + ": min 0, max " + value;
                    else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                        __result = tokens[0] + ": min -" + value + ", max " + value;
                    else if(Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                        __result = tokens[0] + ": min -" + value + ", max 0";
                }
            }
        }

        [HarmonyPatch(typeof(Inventory), "ExecuteItemAction")]
        class Inventory_ExecuteItemAction_Patch
        {
            public static bool Prefix(Inventory __instance, InventoryItem item, ItemAction action)
            {
                //AddDebug("AltUseItem " + item.item.GetTechType());
                //ItemAction itemAction = __instance.GetAltUseItemAction(item);
                IItemsContainer oppositeContainer = __instance.GetOppositeContainer(item);
                if (Main.advancedInventoryLoaded || action != ItemAction.Switch || oppositeContainer == null || item.container is Equipment || oppositeContainer is Equipment)
                    return true;

                ItemsContainer container = (ItemsContainer)item.container;
                List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    //AddDebug("LeftShift ");
                    foreach (TechType itemType in container.GetItemTypes())
                        container.GetItems(itemType, itemsToTransfer);
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    //AddDebug("LeftControl ");
                    container.GetItems(item.item.GetTechType(), itemsToTransfer);
                }
                foreach (InventoryItem ii in itemsToTransfer)
                {
                    //AddDebug("itemsToTransfer " + ii.item.name);
                    Inventory.AddOrSwap(ii, oppositeContainer);
                }
                if (itemsToTransfer.Count > 0)
                    return false;
                else
                    return true;
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
