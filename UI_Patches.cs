using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static ErrorMessage;
using static HandReticle;

namespace Tweaks_Fixes
{
    class UI_Patches
    {
        static bool textInput = false;
        static bool chargerOpen = false;
        //static List <TechType> landPlantSeeds = new List<TechType> { TechType.BulboTreePiece, TechType.PurpleVegetable, TechType.FernPalmSeed, TechType.OrangePetalsPlantSeed, TechType.HangingFruit, TechType.MelonSeed, TechType.PurpleVasePlantSeed, TechType.PinkMushroomSpore, TechType.PurpleRattleSpore, TechType.PinkFlowerSeed };
        //static List<TechType> waterPlantSeeds = new List<TechType> { TechType.CreepvineSeedCluster, TechType.AcidMushroomSpore, TechType.BloodOil, TechType.BluePalmSeed, TechType.KooshChunk, TechType.PurpleBranchesSeed, TechType.WhiteMushroomSpore, TechType.EyesPlantSeed, TechType.RedRollPlantSeed, TechType.GabeSFeatherSeed, TechType.JellyPlantSeed, TechType.RedGreenTentacleSeed, TechType.SnakeMushroomSpore, TechType.MembrainTreeSeed, TechType.SmallFanSeed, TechType.RedBushSeed, TechType.RedConePlantSeed, TechType.RedBasketPlantSeed, TechType.SeaCrownSeed, TechType.ShellGrassSeed, TechType.SpottedLeavesPlantSeed, TechType.SpikePlantSeed, TechType.PurpleFanSeed, TechType.PurpleStalkSeed, TechType.PurpleTentacleSeed };
        static HashSet<TechType> fishTechTypes = new HashSet<TechType> { TechType.Bladderfish, TechType.Boomerang, TechType.Eyeye, TechType.GarryFish, TechType.HoleFish, TechType.Hoopfish, TechType.Hoverfish, TechType.LavaBoomerang, TechType.Oculus, TechType.Peeper, TechType.Reginald, TechType.LavaEyeye, TechType.Spadefish, TechType.Spinefish };
        public static Dictionary<ItemsContainer, Planter> planters = new Dictionary<ItemsContainer, Planter>();
        static public string beaconToolString = string.Empty;
        static public string beaconPickString = string.Empty;
        static public string dropString = string.Empty;
        static public string eatString = string.Empty;
        static public string altToolButton = string.Empty;
        static public string lightFlareString = string.Empty;
        static public string throwFlareString = string.Empty;
        static public string lightAndThrowFlareString = string.Empty;
        public static string smallStorageString = string.Empty;
        public static string deconstructButton = string.Empty;
        static public string changeTorpedoButton = string.Empty;
        static public string propCannonString = string.Empty;
        static public string cycleNextButton = string.Empty;
        static public string cyclePrevButton = string.Empty;
        public static string stasisRifleString = string.Empty;
        public static string scannerString = string.Empty;
        static public string slot1Button = string.Empty;
        static public string slot2Button = string.Empty;
        static public string slot1Plus2Button = string.Empty;
        static public string vehicleExitButton = string.Empty;
        static public string exosuitExitLightsButton = string.Empty;
        static public string exosuitChangeLeftTorpedoButton = string.Empty;
        static public string exosuitChangeRightTorpedoButton = string.Empty;
        static public string exosuitChangeBothTorpedoButton = string.Empty;
        static public string moveDownButton = string.Empty;
        static public string moveRightButton = string.Empty;
        static public string moveLeftButton = string.Empty;
        static public string swivelText = string.Empty;
        static public string leftHandButton = string.Empty;
        static public string rightHandButton = string.Empty;
        static public string seamothLightsButton = string.Empty;
        static public string seamothName = string.Empty;
        static public string pushSeamothString = string.Empty;
        static public string changeTorpedoString = string.Empty;
        static public string toggleBaseLightsString = string.Empty;
        static public string propCannonEatString = string.Empty;
        static public string pickupString = string.Empty;
        static public string constructorString = string.Empty;



        static void GetStrings()
        {
            pickupString = Language.main.Get("PickUp");
            pickupString = pickupString.Substring(0, pickupString.IndexOf('{')).Trim();
            changeTorpedoString = Language.main.Get("TF_change_torpedo");
            rightHandButton = uGUI.FormatButton(GameInput.Button.RightHand);
            leftHandButton = uGUI.FormatButton(GameInput.Button.LeftHand);
            altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);
            cycleNextButton = uGUI.FormatButton(GameInput.Button.CycleNext);
            cyclePrevButton = uGUI.FormatButton(GameInput.Button.CyclePrev);
            deconstructButton = uGUI.FormatButton(GameInput.Button.Deconstruct);
            moveDownButton = uGUI.FormatButton(GameInput.Button.MoveDown);
            moveLeftButton = uGUI.FormatButton(GameInput.Button.MoveLeft);
            moveRightButton = uGUI.FormatButton(GameInput.Button.MoveRight);
            Exosuit_Patch.exosuitName = Language.main.Get("Exosuit");
            propCannonString = LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool);
            Exosuit_Patch.armNamesChanged = true;
            dropString = TooltipFactory.stringDrop + " (" + rightHandButton + ")";
            eatString = TooltipFactory.stringEat + " (" + altToolButton + ")";
            propCannonEatString = TooltipFactory.stringEat + " (" + deconstructButton + ")";
            lightFlareString = Language.main.Get("TF_light_flare") + " (" + altToolButton + ")";
            throwFlareString = Language.main.Get("TF_throw_flare") + " (" + rightHandButton + ")";
            lightAndThrowFlareString = Language.main.Get("TF_light_and_throw_flare") + " (" + rightHandButton + ")";
            swivelText = Language.main.Get("TF_swivel_chair_left") + " (" + moveLeftButton + ")  " + Language.main.Get("TF_swivel_chair_right") + " (" + moveRightButton + ")";
            beaconToolString = TooltipFactory.stringDrop + " (" + rightHandButton + ")  " + Language.main.Get("BeaconLabelEdit") + " (" + deconstructButton + ")";
            beaconPickString = LanguageCache.GetPickupText(TechType.Beacon) + "(" + leftHandButton + ")\n" + Language.main.Get("BeaconLabelEdit") + " (" + deconstructButton + ")";
            smallStorageString = "\n" + LanguageCache.GetPackUpText(TechType.SmallStorage) + " (" + altToolButton + ")\n";
            constructorString = Language.main.Get("Climb") + "(" + leftHandButton + "), " + LanguageCache.GetPackUpText(TechType.Constructor) + " (" + rightHandButton + ")";
            changeTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + altToolButton + ")";
            exosuitChangeLeftTorpedoButton = changeTorpedoButton;
            exosuitChangeRightTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + deconstructButton + ")";
            exosuitChangeBothTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + deconstructButton + ") (" + altToolButton + ")";
            //changeTorpedoExosuitButtonGamepad = Language.main.Get("TF_hold_button") + "(" + altToolButton + ")" + Language.main.Get("TF_press_button") + "(" + cycleNextButton + "), " + "(" + cyclePrevButton + ")" + Language.main.Get("TF_change_torpedo_");
            slot1Button = "(" + uGUI.FormatButton(GameInput.Button.Slot1) + ")";
            slot2Button = "(" + uGUI.FormatButton(GameInput.Button.Slot2) + ")";
            slot1Plus2Button = slot1Button + slot2Button;
            stasisRifleString = Language.main.Get("TF_seamoth_defence_press").Substring(1) + "(" + rightHandButton + ")" + Language.main.Get("TF_seamoth_defence_charge");
            scannerString = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
            //exosuitExitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + ' ' + Language.main.Get("TF_toggle_vehicle_lights") + " (" + moveDownButton + ")";
            exosuitExitLightsButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + ' ' + LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.MoveDown);
            vehicleExitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit);
            seamothLightsButton = LanguageCache.GetButtonFormat("SeaglideLightsTooltip", GameInput.Button.RightHand);
            seamothName = Language.main.Get(TechType.Seamoth);
            pushSeamothString = Language.main.Get("TF_push_seamoth") + seamothName;
            toggleBaseLightsString = Language.main.Get("TF_toggle_base_lights") + " (" + deconstructButton + ")";
            Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
            if (exosuit)
                Exosuit_Patch.GetArmNames(exosuit);
        }

        private static void HandleBaseLights(SubRoot subRoot)
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, toggleBaseLightsString);
            if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                Base_Patch.ToggleBaseLight(subRoot);
        }

        [HarmonyPatch(typeof(Aquarium), "Start")]
        class Aquarium_Start_Patch
        {
            static void Postfix(Aquarium __instance)
            {
                if (__instance.storageContainer.container.allowedTech == null)
                {
                    //AddDebug("Aquarium allowedTech == null ");
                    __instance.storageContainer.container.allowedTech = new HashSet<TechType> { TechType.Bladderfish, TechType.Boomerang, TechType.Eyeye, TechType.GarryFish, TechType.HoleFish, TechType.Hoopfish, TechType.Hoverfish, TechType.LavaBoomerang, TechType.Oculus, TechType.Peeper, TechType.LavaEyeye, TechType.Reginald, TechType.Spadefish, TechType.Spinefish };
                }
            }
        }

        [HarmonyPatch(typeof(Trashcan), "OnEnable")]
        class Trashcan_OnEnable_Patch
        {
            static void Postfix(Trashcan __instance)
            {
                //AddDebug("Trashcan " + __instance.biohazard + " " + __instance.storageContainer.hoverText);
                if (__instance.biohazard)
                {
                    __instance.storageContainer.hoverText = Language.main.Get("LabTrashcan");
                    if (__instance.storageContainer.container.allowedTech == null)
                    {
                        //AddDebug("LabTrashcan allowedTech == null ");
                        __instance.storageContainer.container.allowedTech = new HashSet<TechType> { TechType.ReactorRod, TechType.DepletedReactorRod };
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_ItemsContainer), "OnAddItem")]
        class uGUI_ItemsContainer_OnAddItem_Patch
        {
            static void Postfix(uGUI_ItemsContainer __instance, InventoryItem item)
            {
                //AddDebug("uGUI_ItemsContainer OnAddItem " + item.item.GetTechName());
                if (chargerOpen)
                {
                    Battery battery = item.item.GetComponent<Battery>();
                    if (battery && battery.charge == battery.capacity)
                    {
                        //AddDebug(pair.Key.item.GetTechType() + " charge == capacity ");
                        __instance.items[item].SetChroma(0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BaseBioReactor), "Start")]
        class BaseBioReactor_Start_Patch
        {
            static void Postfix(BaseBioReactor __instance)
            {
                if (__instance.container.allowedTech == null)
                {
                    //AddDebug("BaseBioReactor container.allowedTech == null ");
                    __instance.container.allowedTech = new HashSet<TechType>();
                    foreach (var pair in BaseBioReactor.charge)
                        __instance.container.allowedTech.Add(pair.Key);
                }
            }
        }

        [HarmonyPatch(typeof(Planter), "Start")]
        class Planter_Start_Patch
        {
            static void Postfix(Planter __instance)
            {
                planters[__instance.storageContainer.container] = __instance;
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab))]
        class uGUI_InventoryTab_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnOpenPDA")]
            static void OnOpenPDAPostfix(uGUI_InventoryTab __instance)
            {
                IItemsContainer itemsContainer = null;
                //AddDebug("UsedStorageCount " + Inventory.main.usedStorage.Count);
                for (int i = 0; i < Inventory.main.usedStorage.Count; i++)
                {
                    itemsContainer = Inventory.main.GetUsedStorage(i);
                    if (itemsContainer != null)
                    {
                        //AddDebug("UsedStorage " + i);
                        break;
                    }
                }
                Equipment equipment = itemsContainer as Equipment;
                ItemsContainer container = itemsContainer as ItemsContainer;
                if (container != null)
                {
                    //AddDebug(" container ");
                    if (planters.ContainsKey(container))
                    {
                        Planter planter = planters[container];
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!planter.IsAllowedToAdd(pair.Key.item, false))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    foreach (var pair in __instance.inventory.items)
                    {
                        TechType tt = pair.Key.item.GetTechType();
                        //AddDebug(tt + " Allowed " + container.IsTechTypeAllowed(tt));
                        if (!container.IsTechTypeAllowed(tt))
                            pair.Value.SetChroma(0f);
                    }
                }
                if (equipment != null)
                {
                    chargerOpen = equipment.GetCompatibleSlot(EquipmentType.BatteryCharger, out string s) || equipment.GetCompatibleSlot(EquipmentType.PowerCellCharger, out string ss);
                    //AddDebug("charger " + charger);
                    foreach (var pair in __instance.inventory.items)
                    {
                        TechType tt = pair.Key.item.GetTechType();
                        EquipmentType itemType = CraftData.GetEquipmentType(tt);
                        //AddDebug(pair.Key.item.GetTechType() + " " + itemType);
                        string slot = string.Empty;
                        if (equipment.GetCompatibleSlot(itemType, out slot))
                        {
                            //Main.logger.LogMessage(__instance.name + " GetCompatibleSlot " + tt);
                            //EquipmentType chargerType = Equipment.GetSlotType(slot);
                            //AddDebug(__instance.name + " " + chargerType);
                            //if (chargerType == EquipmentType.BatteryCharger || chargerType ==  EquipmentType.PowerCellCharger)
                            if (chargerOpen)
                            {
                                //chargerOpen = true;
                                if (Battery_Patch.notRechargableBatteries.Contains(tt))
                                {
                                    pair.Value.SetChroma(0f);
                                    continue;
                                }
                                Battery battery = pair.Key.item.GetComponent<Battery>();
                                if (battery && battery.charge == battery.capacity)
                                    pair.Value.SetChroma(0f);
                            }
                        }
                        else
                            pair.Value.SetChroma(0f);
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnClosePDA")]
            static void OnClosePDAPostfix(uGUI_InventoryTab __instance)
            {
                chargerOpen = false;
                foreach (var pair in __instance.inventory.items)
                    pair.Value.SetChroma(1f);
            }
        }

        [HarmonyPatch(typeof(GUIHand))]
        class GUIHand_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnUpdate")]
            public static void OnUpdatePostfix(GUIHand __instance)
            {
                if (!ConfigToEdit.newUIstrings.Value || !Main.gameLoaded)
                    return;

                PlayerTool tool = __instance.GetTool();
                //if (uGUI._main.userInput)
                //    AddDebug("userInput " + uGUI._main.userInput.canvasGroup.interactable);
                //if (uGUI._main.itemSelector)
                //    AddDebug("itemSelector " + uGUI._main.itemSelector);
                //if (uGUI._main.craftingMenu)
                //    AddDebug("craftingMenu " + uGUI._main.craftingMenu.selected);
                if (tool)
                {
                    Flare flare = tool as Flare;
                    InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
                    if (flare && !Main.flareRepairLoaded)
                    {
                        bool lit = flare.flareActivateTime > 0;
                        bool canThrow = Inventory.CanDropItemHere(tool.GetComponent<Pickupable>(), false);
                        //AddDebug("CanDropItemHere " + canThrow);
                        string text = string.Empty;
                        //string throwFlare = lit ? Main.config.throwFlare : Main.config.lightAndThrowFlare;
                        if (!lit && canThrow)
                        {
                            StringBuilder stringBuilder = new StringBuilder(lightAndThrowFlareString);
                            stringBuilder.Append(",  ");
                            stringBuilder.Append(lightFlareString);
                            text = stringBuilder.ToString();
                        }
                        else if (lit && canThrow)
                            text = throwFlareString;
                        else if (!lit && !canThrow)
                            text = lightFlareString;

                        if (!lit && GameInput.GetButtonDown(GameInput.Button.AltTool))
                            Flare_Patch.LightFlare(flare);

                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                    }
                    Beacon beacon = tool as Beacon;
                    if (beacon && beacon.beaconLabel)
                    {
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, beaconToolString);
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        {
                            uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));
                        }
                    }
                    DeployableStorage ds = tool as DeployableStorage;
                    if (ds)
                    {
                        //Transform tr = ds.transform.Find("StorageContainer");
                        //StorageContainer sc = tr.GetComponent<StorageContainer>();
                        //StringBuilder sb = new StringBuilder(Language.main.Get(sc.hoverText));
                        //sb.Append(" (");
                        //sb.Append(TooltipFactory.stringLeftHand);
                        //sb.Append(")\n");
                        Transform tr = ds.transform.Find("LidLabel/Label");
                        ColoredLabel cl = tr.GetComponent<ColoredLabel>();
                        StringBuilder sb = new StringBuilder(dropString);
                        //sb.Append(Language.main.Get(cl.stringEditLabel));
                        sb.Append(", ");
                        sb.Append(Language.main.Get(cl.stringEditLabel));
                        sb.Append(" (");
                        sb.Append(deconstructButton);
                        sb.Append(")");
                        //HandReticle.main.SetUseTextRaw(sb.ToString(), null);
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, sb.ToString());
                        //if (GameInput.GetButtonDown(GameInput.Button.LeftHand))
                        //    sc.OnHandClick(__instance);
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                            cl.signInput.Select(true);
                    }

                    if (Util.IsEatableFish(tool.gameObject))
                    {
                        StringBuilder sb = new StringBuilder();
                        bool canDrop = Inventory.CanDropItemHere(tool.GetComponent<Pickupable>(), false);
                        if (canDrop)
                            sb.Append(dropString);

                        //Main.Log("GameInput.Button.RightHand) " + uGUI.FormatButton(GameInput.Button.RightHand));
                        if (Util.CanPlayerEat())
                        {
                            if (canDrop)
                                sb.Append(",  ");

                            sb.Append(eatString);
                            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                            {
                                //Inventory playerInv = Inventory.main;
                                //playerInv.UseItem(playerInv.quickSlots.heldItem);
                                Inventory.main.ExecuteItemAction(ItemAction.Eat, heldItem);
                            }
                        }
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, sb.ToString());
                    }
                }
                else if (!IngameMenu.main.isActiveAndEnabled && !Main.baseLightSwitchLoaded && !Player.main.pda.isInUse && !textInput && !uGUI._main.craftingMenu.selected)
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase && subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline)
                    {
                        HandleBaseLights(subRoot);
                    }
                }
                if (!__instance.activeTarget)
                    return;
                //Main.Message("activeTarget layer " + __instance.activeTarget.layer);
                //if (__instance.activeTarget.layer == LayerID.NotUseable)
                //    Main.Message("activeTarget Not Useable layer ");
                TechType targetTT = CraftData.GetTechType(__instance.activeTarget);
                if (targetTT == TechType.None)
                    return;
                // UI tells you if looking at dead fish 
                LiveMixin liveMixin = __instance.activeTarget.GetComponent<LiveMixin>();
                if (liveMixin && !liveMixin.IsAlive())
                {
                    //AddDebug("health " + liveMixin.health);
                    Pickupable pickupable = liveMixin.GetComponent<Pickupable>();
                    string name = Language.main.Get(targetTT);
                    //AddDebug("name " + name);
                    if (pickupable)
                    {
                        if (pickupable.overrideTechType != TechType.None)
                            name = Language.main.Get(pickupable.overrideTechType);

                        name = Language.main.GetFormat("DeadFormat", name);
                        //HandReticle.main.SetInteractText(name);
                        HandReticle.main.SetText(HandReticle.TextType.Hand, name, false, GameInput.Button.LeftHand);
                    }
                    else
                    {
                        name = Language.main.GetFormat<string>("DeadFormat", name);
                        HandReticle.main.SetText(HandReticle.TextType.Hand, name, true);
                    }
                }
                Flare flareTarget = __instance.activeTarget.GetComponent<Flare>();
                if (flareTarget && Util.Approximately(flareTarget.energyLeft, 0f))
                {
                    //AddDebug("activeTarget Flare");
                    StringBuilder sb = new StringBuilder(Language.main.Get("TF_burnt_out_flare"));
                    sb.Append(Language.main.Get(targetTT));
                    //HandReticle.main.SetInteractTextRaw(sb.ToString(), string.Empty);
                    HandReticle.main.SetText(HandReticle.TextType.Hand, sb.ToString(), false);
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
                    __instance.rightSide.OpenGroup("SavedGames");
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_PDA), "Update")]
        class uGUI_PDA_Update_Patch
        {
            public static void Postfix(uGUI_PDA __instance)
            {
                if (!Main.gameLoaded || GameInput.lastDevice != GameInput.Device.Keyboard || IngameMenu.main.isActiveAndEnabled || !Player.main.pda.isOpen)
                    return;

                if (Input.GetKeyDown(ConfigMenu.nextPDATabKey.Value))
                {
                    //if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    __instance.OpenTab(__instance.GetNextTab());
                }
                else if (Input.GetKeyDown(ConfigMenu.previousPDATabKey.Value))
                {
                    __instance.OpenTab(__instance.GetPreviousTab());
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_FeedbackCollector), "HintShow")]
        class uGUI_FeedbackCollector_HintShow_Patch
        {
            static bool Prefix(uGUI_FeedbackCollector __instance)
            {
                return !ConfigToEdit.disableHints.Value;
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneIntro), "ControlsHints")]
        class uGUI_SceneIntro_ControlsHints_Patch
        {
            static bool Prefix(uGUI_SceneIntro __instance)
            {
                return !ConfigToEdit.disableHints.Value;
            }
        }

        [HarmonyPatch(typeof(PlayerWorldArrows), "CreateWorldArrows")]
        class PlayerWorldArrows_CreateWorldArrows_Patch
        {
            static bool Prefix(PlayerWorldArrows __instance)
            {
                //AddDebug("CreateWorldArrows");
                return !ConfigToEdit.disableHints.Value;
            }
        }

        [HarmonyPatch(typeof(TooltipFactory))]
        class TooltipFactory_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Initialize")]
            static void InitializePostfix()
            {
                if (string.IsNullOrEmpty(altToolButton))
                {
                    //AddDebug("TooltipFactory Initialize ");
                    //Main.logger.LogMessage("TooltipFactory Initialize " );
                    GetStrings();
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnLanguageChanged")]
            static void OnLanguageChangedPostfix()
            {
                //AddDebug("TooltipFactory OnLanguageChanged ");
                GetStrings();
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnBindingsChanged")]
            static void OnBindingsChangedPostfix()
            {
                //AddDebug("TooltipFactory OnBindingsChanged ");
                GetStrings();
            }
            [HarmonyPrefix]
            [HarmonyPatch("ItemCommons")]
            static void ItemCommonsPrefix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (!ConfigToEdit.newUIstrings.Value)
                    return;

                Flare flare = obj.GetComponent<Flare>();
                if (flare)
                {
                    //AddDebug("flare.energyLeft " + flare.energyLeft);
                    if (flare.energyLeft <= 0f)
                        TooltipFactory.WriteTitle(sb, Language.main.Get("TF_burnt_out_flare"));
                    else if (flare.flareActivateTime > 0f)
                        TooltipFactory.WriteTitle(sb, Language.main.Get("TF_lit_flare"));
                }

            }

            [HarmonyPostfix]
            [HarmonyPatch("ItemCommons")]
            static void ItemCommonsPostfix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (techType == TechType.Battery)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_Battery"));
                else if (techType == TechType.PowerCell)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_PowerCell"));
                else if (techType == TechType.PrecursorIonBattery)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_PrecursorIonBattery"));
                else if (techType == TechType.PrecursorIonPowerCell)
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_PrecursorIonPowerCell"));

                //if (!ConfigToEdit.newUIstrings.Value)
                //    return;

                if (ConfigMenu.eatRawFish.Value != ConfigMenu.EatingRawFish.Vanilla && fishTechTypes.Contains(techType) && GameModeUtils.RequiresSurvival())
                {
                    Eatable eatable = obj.GetComponent<Eatable>();
                    if (eatable)
                    {
                        sb.Clear();
                        string name = Language.main.Get(techType);
                        string secondaryTooltip = eatable.GetSecondaryTooltip();
                        if (!string.IsNullOrEmpty(secondaryTooltip))
                            name = Language.main.GetFormat<string, string>("DecomposingFormat", secondaryTooltip, name);
                        TooltipFactory.WriteTitle(sb, name);
                        //TooltipFactory.WriteDebug(sb, techType);
                        int foodValue = Mathf.CeilToInt(eatable.GetFoodValue());
                        if (foodValue != 0)
                        {
                            string food = Language.main.GetFormat<int>("FoodFormat", foodValue);
                            int index = -1;
                            if (foodValue < 0)
                                index = food.LastIndexOf('-');
                            else
                                index = food.LastIndexOf('+');

                            if (index != -1)
                            {
                                if (foodValue > 0)
                                {
                                    if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                                        food = food.Substring(0, index) + "≈ 0";
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                                        food = food.Substring(0, index) + "≈ " + Mathf.CeilToInt(foodValue * .5f);
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                                        food = food.Substring(0, index) + "≈ " + (-Mathf.CeilToInt(foodValue * .5f));
                                }
                                //AddDebug("food  " + food);
                            }
                            TooltipFactory.WriteDescription(sb, food);
                        }
                        int waterValue = Mathf.CeilToInt(eatable.GetWaterValue());
                        if (waterValue != 0)
                        {
                            string water = Language.main.GetFormat<int>("WaterFormat", waterValue);
                            int index = -1;
                            if (waterValue < 0)
                                index = water.LastIndexOf('-');
                            else
                                index = water.LastIndexOf('+');

                            if (index != -1)
                            {
                                if (waterValue > 0)
                                {
                                    if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Risky)
                                        water = water.Substring(0, index) + "≈ 0";
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmless)
                                        water = water.Substring(0, index) + "≈ " + Mathf.CeilToInt(waterValue * .5f);
                                    else if (ConfigMenu.eatRawFish.Value == ConfigMenu.EatingRawFish.Harmful)
                                        water = water.Substring(0, index) + "≈ " + (-Mathf.CeilToInt(waterValue * .5f));
                                }
                                //AddDebug("water  " + water);
                            }
                            TooltipFactory.WriteDescription(sb, water);
                        }
                        TooltipFactory.WriteDescription(sb, Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(techType)));

                    }
                }
                if (Crush_Damage.crushDepthEquipment.ContainsKey(techType) && Crush_Damage.crushDepthEquipment[techType] > 0)
                {
                    StringBuilder sb_ = new StringBuilder(Language.main.Get("TF_crush_depth_equipment"));
                    sb_.Append(Crush_Damage.crushDepthEquipment[techType]);
                    sb_.Append(Language.main.Get("TF_meters"));
                    TooltipFactory.WriteDescription(sb, sb_.ToString());
                }
                if (Crush_Damage.crushDamageEquipment.ContainsKey(techType) && Crush_Damage.crushDamageEquipment[techType] > 0)
                {
                    StringBuilder sb_ = new StringBuilder(Language.main.Get("TF_crush_damage_equipment"));
                    sb_.Append(Crush_Damage.crushDepthEquipment[techType]);
                    sb_.Append("%");
                    TooltipFactory.WriteDescription(sb, sb_.ToString());
                }
                if (techType == TechType.FirstAidKit)
                {
                    sb.Clear();
                    TooltipFactory.WriteTitle(sb, Language.main.Get("FirstAidKit"));
                    TooltipFactory.WriteDescription(sb, Language.main.GetFormat<float>("HealthFormat", ConfigMenu.medKitHP.Value));
                    TooltipFactory.WriteDescription(sb, Language.main.Get("Tooltip_FirstAidKit"));
                }
                if (ConfigMenu.invMultLand.Value > 0f || ConfigMenu.invMultWater.Value > 0f)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        StringBuilder sb_ = new StringBuilder(Language.main.Get("TF_mass"));
                        sb_.Append(rb.mass);
                        sb_.Append(Language.main.Get("TF_kg"));
                        TooltipFactory.WriteDescription(sb, sb_.ToString());
                    }
                }

            }
        }

        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_HealthBar __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                if (!ConfigToEdit.alwaysShowHealthFoodNunbers.Value)
                    return true;

                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player player = Player.main;
                if (player != null)
                {
                    LiveMixin liveMixin = player.GetComponent<LiveMixin>();
                    if (liveMixin != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            liveMixin.onHealDamage.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnHealDamage));
                        }
                        //float has = liveMixin.health - liveMixin.tempDamage;
                        float has = ConfigToEdit.newPoisonSystem.Value ? liveMixin.health : liveMixin.health - liveMixin.tempDamage;

                        float maxHealth = liveMixin.maxHealth;
                        __instance.SetValue(has, maxHealth);
                        float time = 1f - Mathf.Clamp01(has / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0f)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0f)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0f)
                            __instance.statePulse.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = player.GetPDA();
                    if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.statePulse.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;

                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);

                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;

                __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);

                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
        class uGUI_FoodBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_FoodBar __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                if (!ConfigToEdit.alwaysShowHealthFoodNunbers.Value)
                    return true;

                int num1 = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    Survival component = main.GetComponent<Survival>();
                    if (component != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            component.onEat.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnEat));
                        }
                        float food = component.food;
                        float capacity = 100f;
                        __instance.SetValue(food, capacity);
                        float time = 1f - Mathf.Clamp01(food / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0f)
                            __instance.pulseDelay = 0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0f)
                            __instance.pulseTime = 0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0f && num2 <= 0f)
                            __instance.pulseAnimationState.normalizedTime = 0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.pulseAnimationState != null && __instance.pulseAnimation.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (num1 != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;

                __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
        class uGUI_WaterBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_WaterBar __instance)
            {
                if (!Main.gameLoaded)
                    return false;

                if (!ConfigToEdit.alwaysShowHealthFoodNunbers.Value)
                    return true;

                int num1 = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    Survival component = main.GetComponent<Survival>();
                    if (component != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            component.onDrink.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnDrink));
                        }
                        float water = component.water;
                        float capacity = 100f;
                        __instance.SetValue(water, capacity);
                        float time = 1f - Mathf.Clamp01(water / __instance.pulseReferenceCapacity);
                        __instance.pulseDelay = __instance.pulseDelayCurve.Evaluate(time);
                        if (__instance.pulseDelay < 0.0)
                            __instance.pulseDelay = 0.0f;
                        __instance.pulseTime = __instance.pulseTimeCurve.Evaluate(time);
                        if (__instance.pulseTime < 0.0)
                            __instance.pulseTime = 0.0f;
                        float num2 = __instance.pulseDelay + __instance.pulseTime;
                        if (__instance.pulseTween.duration > 0.0 && num2 <= 0.0)
                            __instance.pulseAnimationState.normalizedTime = 0.0f;
                        __instance.pulseTween.duration = num2;
                    }
                    PDA pda = main.GetPDA();
                    if (ConfigToEdit.alwaysShowHealthFoodNunbers.Value || pda != null && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if (__instance.pulseAnimationState != null && __instance.pulseAnimationState.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (num1 != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                if (!MathExtensions.CoinRotation(ref __instance.rotationCurrent, __instance.showNumbers ? 180f : 0.0f, ref __instance.lastFixedUpdateTime, PDA.time, ref __instance.rotationVelocity, __instance.rotationSpringCoef, __instance.rotationVelocityDamp, __instance.rotationVelocityMax))
                    return false;

                __instance.icon.localRotation = Quaternion.Euler(0.0f, __instance.rotationCurrent, 0.0f);

                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_InputGroup))]
        class uGUI_InputGroup_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnSelect")]
            static void OnSelectPostfix(uGUI_InputGroup __instance)
            {
                //AddDebug("uGUI_InputGroup OnSelect");
                textInput = true;
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnDeselect")]
            static void OnDeselectPostfix(uGUI_InputGroup __instance)
            {
                //AddDebug("uGUI_InputGroup OnDeselect");
                textInput = false;
            }
        }

        [HarmonyPatch(typeof(HandReticle), "SetTextRaw")]
        class HandReticle_SetTextRaw_Patch
        {
            static bool Prefix(HandReticle __instance, HandReticle.TextType type, string text)
            {
                //AddDebug("SetTextRaw " + type + " " + text);
                if (ConfigToEdit.disableUseText.Value && (type == HandReticle.TextType.Use || type == HandReticle.TextType.UseSubscript))
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(HintSwimToSurface), "Update")]
        public static class HintSwimToSurface_Update_Patch
        {
            public static bool Prefix(HintSwimToSurface __instance)
            {
                return ConfigToEdit.lowOxygenWarning.Value;
            }
        }

        [HarmonyPatch(typeof(LowOxygenAlert), "Update")]
        public static class LowOxygenAlert_Update_Patch
        {
            public static bool Prefix(LowOxygenAlert __instance)
            {
                return ConfigToEdit.lowOxygenAudioWarning.Value;
            }
        }

        //[HarmonyPatch(typeof(StartScreen), "TryToShowDisclaimer")]
        public static class StartScreenPatch
        {
            public static bool Prefix(StartScreen __instance) => false;
        }

        [HarmonyPatch(typeof(uGUI_EncyclopediaTab), "DisplayEntry")]
        public static class uGUI_EncyclopediaTab_Patch
        {
            public static void Postfix(uGUI_EncyclopediaTab __instance) => __instance.contentScrollRect.verticalNormalizedPosition = 1f;
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "OnRightSideOpened")]
        class uGUI_MainMenu_OnRightSideOpened_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance, GameObject root)
            {
                //AddDebug("OnRightSideOpened " + __instance.GetCurrentSubMenu());
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

        //[HarmonyPatch(typeof(uGUI_BlueprintsTab), "UpdateEntries")]
        class uGUI_BlueprintsTab_UpdateEntries_Patch
        {
            static bool did;
            public static void Postfix(uGUI_BlueprintsTab __instance)
            {
                //AddDebug("uGUI_BlueprintsTab UpdateEntries ");
                if (did)
                    return;

                for (int index1 = 0; index1 < uGUI_BlueprintsTab.groups.Count; ++index1)
                {
                    //did = true;
                    TechGroup group = uGUI_BlueprintsTab.groups[index1];
                    Main.logger.LogMessage("uGUI_BlueprintsTab UpdateEntries group " + group);
                    Main.logger.LogMessage("uGUI_BlueprintsTab UpdateEntries uGUI_BlueprintsTab.sTechCategories.Count " + uGUI_BlueprintsTab.sTechCategories.Count);
                    for (int index2 = 0; index2 < uGUI_BlueprintsTab.sTechCategories.Count; ++index2)
                    {
                        TechCategory sTechCategory = uGUI_BlueprintsTab.sTechCategories[index2];
                        Main.logger.LogMessage("uGUI_BlueprintsTab UpdateEntries sTechCategory " + sTechCategory);
                        for (int index3 = 0; index3 < uGUI_BlueprintsTab.sTechTypes.Count; ++index3)
                        {
                            TechType sTechType = uGUI_BlueprintsTab.sTechTypes[index3];
                            Main.logger.LogMessage("uGUI_BlueprintsTab UpdateEntries sTechType " + sTechType);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_BlueprintsTab), "Open")]
        class uGUI_BlueprintsTab_Open_Patch
        {
            public static void Postfix(uGUI_BlueprintsTab __instance)
            {
                if (!ConfigToEdit.removeCookedFishFromBlueprints.Value && !ConfigToEdit.removeWaterFromBlueprints.Value)
                    return;

                Transform scrollCanvas = __instance.transform.Find("Content/ScrollView/Viewport/ScrollCanvas");
                if (scrollCanvas)
                {
                    for (var i = 0; i < scrollCanvas.transform.childCount; ++i)
                    {
                        Transform child = scrollCanvas.transform.GetChild(i);
                        //Main.logger.LogMessage("uGUI_BlueprintsTab child " + child.name);
                        if (child.name == "BlueprintCategoryTitle(Clone)")
                        {
                            var titleText = child.GetComponentInChildren<TextMeshProUGUI>();
                            if (titleText)
                            {
                                if (ConfigToEdit.removeCookedFishFromBlueprints.Value && titleText.text == "Cooked Food" || titleText.text == "Cured Food")
                                {
                                    child.gameObject.SetActive(false);
                                    scrollCanvas.transform.GetChild(i + 1).gameObject.SetActive(false);
                                }
                                if (ConfigToEdit.removeWaterFromBlueprints.Value && titleText.text == "Water")
                                {
                                    child.gameObject.SetActive(false);
                                    scrollCanvas.transform.GetChild(i + 1).gameObject.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(uGUI_ExosuitHUD), "Update")]
        public static class uGUI_ExosuitHUD_Patch
        {
            static string tempSuffix;
            static int lastTemperature = int.MinValue;
            public static void Postfix(uGUI_ExosuitHUD __instance)
            {
                if (ConfigToEdit.showTempFahrenhiet.Value && Player.main.currentMountedVehicle is Exosuit)
                {
                    if (__instance.lastTemperature == lastTemperature)
                        return;

                    __instance.textTemperature.text = IntStringCache.GetStringForInt((int)Util.CelciusToFahrenhiet(__instance.lastTemperature));
                    if (tempSuffix == null)
                    {
                        __instance.textTemperatureSuffix.text = __instance.textTemperatureSuffix.text.Replace("°C", "°F");
                        tempSuffix = __instance.textTemperatureSuffix.text;
                    }
                    else
                        __instance.textTemperatureSuffix.text = tempSuffix;

                    lastTemperature = __instance.lastTemperature;
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_SeamothHUD), "Update")]
        public static class uuGUI_SeamothHUD_Patch
        {
            static string tempSuffix;
            static int lastTemperature = int.MinValue;
            public static void Postfix(uGUI_SeamothHUD __instance)
            {
                if (ConfigToEdit.showTempFahrenhiet.Value && Player.main.currentMountedVehicle is SeaMoth)
                {
                    if (__instance.lastTemperature == lastTemperature)
                        return;

                    __instance.textTemperature.text = IntStringCache.GetStringForInt((int)Util.CelciusToFahrenhiet(__instance.lastTemperature));
                    if (tempSuffix == null)
                    {
                        __instance.textTemperatureSuffix.text = __instance.textTemperatureSuffix.text.Replace("°C", "°F");
                        tempSuffix = __instance.textTemperatureSuffix.text;
                    }
                    else
                        __instance.textTemperatureSuffix.text = tempSuffix;

                    lastTemperature = __instance.lastTemperature;
                }
            }
        }


    }
}
