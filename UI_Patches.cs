using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class UI_Patches
    {

        //static List <TechType> landPlantSeeds = new List<TechType> { TechType.BulboTreePiece, TechType.PurpleVegetable, TechType.FernPalmSeed, TechType.OrangePetalsPlantSeed, TechType.HangingFruit, TechType.MelonSeed, TechType.PurpleVasePlantSeed, TechType.PinkMushroomSpore, TechType.PurpleRattleSpore, TechType.PinkFlowerSeed };
        //static List<TechType> waterPlantSeeds = new List<TechType> { TechType.CreepvineSeedCluster, TechType.AcidMushroomSpore, TechType.BloodOil, TechType.BluePalmSeed, TechType.KooshChunk, TechType.PurpleBranchesSeed, TechType.WhiteMushroomSpore, TechType.EyesPlantSeed, TechType.RedRollPlantSeed, TechType.GabeSFeatherSeed, TechType.JellyPlantSeed, TechType.RedGreenTentacleSeed, TechType.SnakeMushroomSpore, TechType.MembrainTreeSeed, TechType.SmallFanSeed, TechType.RedBushSeed, TechType.RedConePlantSeed, TechType.RedBasketPlantSeed, TechType.SeaCrownSeed, TechType.ShellGrassSeed, TechType.SpottedLeavesPlantSeed, TechType.SpikePlantSeed, TechType.PurpleFanSeed, TechType.PurpleStalkSeed, TechType.PurpleTentacleSeed };
        //static HashSet<TechType> fishTechTypes = new HashSet<TechType> { TechType.Bladderfish, TechType.Boomerang, TechType.Eyeye, TechType.GarryFish, TechType.HoleFish, TechType.Hoopfish, TechType.Hoverfish, TechType.LavaBoomerang, TechType.Oculus, TechType.Peeper, TechType.Reginald, TechType.LavaEyeye, TechType.Spadefish, TechType.Spinefish };
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
        static public string bagString = string.Empty;

        static void GetStrings()
        {
            pickupString = Language.main.Get("PickUp");
            pickupString = pickupString.Substring(0, pickupString.IndexOf('{')).Trim();
            changeTorpedoString = Language.main.Get("TF_change_torpedo");
            rightHandButton = GameInput.FormatButton(GameInput.Button.RightHand);
            leftHandButton = GameInput.FormatButton(GameInput.Button.LeftHand);
            altToolButton = GameInput.FormatButton(GameInput.Button.AltTool);
            cycleNextButton = GameInput.FormatButton(GameInput.Button.CycleNext);
            cyclePrevButton = GameInput.FormatButton(GameInput.Button.CyclePrev);
            deconstructButton = GameInput.FormatButton(GameInput.Button.Deconstruct);
            moveDownButton = GameInput.FormatButton(GameInput.Button.MoveDown);
            moveLeftButton = GameInput.FormatButton(GameInput.Button.MoveLeft);
            moveRightButton = GameInput.FormatButton(GameInput.Button.MoveRight);
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
            bagString = LanguageCache.GetPackUpText(TechType.LuggageBag) + " (" + altToolButton + ")\n";

            constructorString = Language.main.Get("Climb") + "(" + leftHandButton + "), " + LanguageCache.GetPackUpText(TechType.Constructor) + " (" + rightHandButton + ")";
            changeTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + altToolButton + ")";
            exosuitChangeLeftTorpedoButton = changeTorpedoButton;
            exosuitChangeRightTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + deconstructButton + ")";
            exosuitChangeBothTorpedoButton = Language.main.Get("TF_change_torpedo") + "(" + deconstructButton + ") (" + altToolButton + ")";
            //changeTorpedoExosuitButtonGamepad = Language.main.Get("TF_hold_button") + "(" + altToolButton + ")" + Language.main.Get("TF_press_button") + "(" + cycleNextButton + "), " + "(" + cyclePrevButton + ")" + Language.main.Get("TF_change_torpedo_");
            slot1Button = "(" + GameInput.FormatButton(GameInput.Button.Slot1) + ")";
            slot2Button = "(" + GameInput.FormatButton(GameInput.Button.Slot2) + ")";
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

        private static void HandleBaseLightsControl(SubRoot subRoot)
        {
            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, toggleBaseLightsString);
            if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                Base_.ToggleBaseLight(subRoot);
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
                }
            }
        }





        [HarmonyPatch(typeof(GUIHand))]
        class GUIHand_Patch
        {
            //[HarmonyPrefix, HarmonyPatch("Send")]
            public static bool OnUpdatePrefix(GUIHand __instance, GameObject target, HandTargetEventType e, GUIHand hand)
            {
                if (!Main.gameLoaded)
                    return false;

                AddDebug($"GUIHand Send {target.name} {e}");
                if (target == null || !target.activeInHierarchy || e == HandTargetEventType.None)
                {
                    return false;
                }
                IHandTarget handTarget = target.GetComponent<IHandTarget>();
                if (handTarget == null)
                {
                    return false;
                }
                try
                {
                    //switch (e)
                    {
                        //case HandTargetEventType.Hover:
                        //    handTarget.OnHandHover(hand);
                        //    break;
                        //case HandTargetEventType.Click:
                        //    handTarget.OnHandClick(hand);
                        //    break;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
                return false;
            }

            [HarmonyPostfix, HarmonyPatch("OnUpdate")]
            public static void OnUpdatePostfix(GUIHand __instance)
            { // this runs when player in exosuit is looking at floating container
                if (Main.gameLoaded == false || FPSInputModule.current.lockMovement)
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
                    InventoryItem heldItem = Inventory.main.quickSlots.heldItem;
                    if (!Main.flareRepairLoaded && ConfigToEdit.flareTweaks.Value)
                    {
                        Flare flare = tool as Flare;
                        if (flare)
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
                                Flare_.LightFlare(flare);

                            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                        }
                    }
                    if (ConfigToEdit.beaconTweaks.Value)
                    {
                        Beacon beacon = tool as Beacon;
                        if (beacon && beacon.beaconLabel)
                        {
                            HandReticle.main.SetTextRaw(HandReticle.TextType.Use, beaconToolString);
                            if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                            {
                                uGUI.main.userInput.RequestString(beacon.beaconLabel.stringBeaconLabel, beacon.beaconLabel.stringBeaconSubmit, beacon.beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beacon.beaconLabel.SetLabel));
                            }
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
                else if (!IngameMenu.main.isActiveAndEnabled && !Main.baseLightSwitchLoaded && !Player.main.pda.isInUse && !uGUI._main.craftingMenu.selected)
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase && subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline)
                    {
                        HandleBaseLightsControl(subRoot);
                    }
                }
                if (!__instance.activeTarget)
                    return;
                //AddDebug("activeTarget " + __instance.activeTarget.name);
                //AddDebug("activeTarget layer " + __instance.activeTarget.layer);
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
                if (ConfigToEdit.flareTweaks.Value)
                {
                    Flare flareTarget = __instance.activeTarget.GetComponent<Flare>();
                    if (flareTarget && Mathf.Approximately(flareTarget.energyLeft, 0f))
                    {
                        //AddDebug("activeTarget Flare");
                        StringBuilder sb = new StringBuilder(Language.main.Get("TF_burnt_out_flare"));
                        sb.Append(Language.main.Get(targetTT));
                        //HandReticle.main.SetInteractTextRaw(sb.ToString(), string.Empty);
                        HandReticle.main.SetText(HandReticle.TextType.Hand, sb.ToString(), false);
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
                    __instance.rightSide.OpenGroup("SavedGames");
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_PDA), "Update")]
        class uGUI_PDA_Update_Patch
        {
            public static void Postfix(uGUI_PDA __instance)
            {
                if (!Main.gameLoaded || GameInput.lastPrimaryDevice != GameInput.Device.Keyboard || IngameMenu.main.isActiveAndEnabled || !Player.main.pda.isOpen)
                    return;

                if (GameInput.GetButtonDown(OptionsMenu.nextPDAtab))
                {
                    //if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    __instance.OpenTab(__instance.GetNextTab());
                }
                else if (GameInput.GetButtonDown(OptionsMenu.previousPDAtab))
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
                if (!ConfigToEdit.flareTweaks.Value)
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

                if (ConfigMenu.eatRawFish.Value != ConfigMenu.EatingRawFish.Default && Creatures.fishTechTypes.Contains(techType) && GameModeUtils.RequiresSurvival())
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
                    sb_.Append(Crush_Damage.crushDamageEquipment[techType]);
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

        //[HarmonyPatch(typeof(HandReticle), "SetText")]
        class HandReticle_SetText_Patch
        {
            static bool Prefix(HandReticle __instance, HandReticle.TextType type, string text, GameInput.Button button)
            {
                //HandReticle.main.SetText(HandReticle.TextType.Hand)
                //return false;
                AddDebug("SetText " + type + " text " + text + " button " + button);
                //if (ConfigToEdit.disableUseText.Value && (type == HandReticle.TextType.Use || type == HandReticle.TextType.UseSubscript))
                //{
                //    return false;
                //}
                return true;
            }
        }

        [HarmonyPatch(typeof(HandReticle), "SetTextRaw")]
        class HandReticle_SetTextRaw_Patch
        {
            static bool Prefix(HandReticle __instance, HandReticle.TextType type, string text)
            {
                //return false;
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
        public static class uGUI_ExosuitHUD_Update_Patch
        {
            static string tempSuffix;
            static int lastTemperature = int.MinValue;
            public static void Postfix(uGUI_ExosuitHUD __instance)
            { // runs in main menu
                if (Main.gameLoaded == false || ConfigToEdit.showTempFahrenhiet.Value == false || Player.main.currentMountedVehicle is Exosuit == false)
                    return;

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

        [HarmonyPatch(typeof(uGUI_SeamothHUD), "Update")]
        public static class uuGUI_SeamothHUD_Update_Patch
        {
            static string tempSuffix;
            static int lastTemperature = int.MinValue;
            public static void Postfix(uGUI_SeamothHUD __instance)
            {
                if (Main.gameLoaded == false || ConfigToEdit.showTempFahrenhiet.Value == false || Player.main.currentMountedVehicle is SeaMoth == false)
                    return;

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

        [HarmonyPatch(typeof(ThermalPlant))]
        public static class ThermalPlant_Patch
        {
            [HarmonyPostfix, HarmonyPatch("UpdateUI")]
            public static void UpdateUIPostfix(ThermalPlant __instance)
            {
                if (ConfigToEdit.showTempFahrenhiet.Value)
                {
                    __instance.temperatureText.text = (int)Util.CelciusToFahrenhiet(__instance.temperature) + "°F";
                }
            }
            [HarmonyPrefix, HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(ThermalPlant __instance, GUIHand hand)
            {
                if (!__instance.constructable.constructed)
                    return false;

                HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat<int, int>("ThermalPlantStatus", Mathf.RoundToInt(__instance.powerSource.GetPower()), Mathf.RoundToInt(__instance.powerSource.GetMaxPower())), false);
                HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                //HandReticle.main.SetIcon(HandReticle.IconType.Interact);
                return false;
            }
        }


    }
}
