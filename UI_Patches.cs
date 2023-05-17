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
        static bool textInput = false;
        static bool fishTooltip = false;
        static bool chargerOpen = false;
        //static List <TechType> landPlantSeeds = new List<TechType> { TechType.BulboTreePiece, TechType.PurpleVegetable, TechType.FernPalmSeed, TechType.OrangePetalsPlantSeed, TechType.HangingFruit, TechType.MelonSeed, TechType.PurpleVasePlantSeed, TechType.PinkMushroomSpore, TechType.PurpleRattleSpore, TechType.PinkFlowerSeed };
        //static List<TechType> waterPlantSeeds = new List<TechType> { TechType.CreepvineSeedCluster, TechType.AcidMushroomSpore, TechType.BloodOil, TechType.BluePalmSeed, TechType.KooshChunk, TechType.PurpleBranchesSeed, TechType.WhiteMushroomSpore, TechType.EyesPlantSeed, TechType.RedRollPlantSeed, TechType.GabeSFeatherSeed, TechType.JellyPlantSeed, TechType.RedGreenTentacleSeed, TechType.SnakeMushroomSpore, TechType.MembrainTreeSeed, TechType.SmallFanSeed, TechType.RedBushSeed, TechType.RedConePlantSeed, TechType.RedBasketPlantSeed, TechType.SeaCrownSeed, TechType.ShellGrassSeed, TechType.SpottedLeavesPlantSeed, TechType.SpikePlantSeed, TechType.PurpleFanSeed, TechType.PurpleStalkSeed, TechType.PurpleTentacleSeed };
        static Dictionary<ItemsContainer, Planter> planters = new Dictionary<ItemsContainer, Planter>();
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
        public static string seaglideString = string.Empty;
        static public string changeTorpedoButton = string.Empty;
        static public string propCannonString = string.Empty;
        static public string changeTorpedoExosuitButtonGamepad = string.Empty;
        static public string changeTorpedoExosuitButtonKeyboard = string.Empty;
        static public string cycleNextButton = string.Empty;
        static public string cyclePrevButton = string.Empty;
        public static string stasisRifleString = string.Empty;
        public static string scannerString = string.Empty;
        static public string slot1Button = string.Empty;
        static public string slot2Button = string.Empty;
        static public string slot1Plus2Button = string.Empty;
        static public string exosuitExitButton = string.Empty;
        static public string moveRightButton = string.Empty;
        static public string moveLeftButton = string.Empty;
        static public string swivelText = string.Empty;
        
        static void GetStrings()
        {
            //AddDebug("GetStrings translatableStrings.Count " + Main.config.translatableStrings.Count);
            //AddDebug("translatableStrings " + Main.config.translatableStrings[Main.config.translatableStrings.Count - 1]);
            if (Main.config.translatableStrings.Count < 28)
            {
                if (Main.config.translatableStrings.Count == 26)
                {
                    Main.config.translatableStrings.Add("Swivel left");
                    Main.config.translatableStrings.Add("Swivel right");
                    //foreach (var s in oldList)
                    //    Main.logger.LogInfo("translatableStrings " + s);
                }
                else
                    Main.config.translatableStrings = new List<string>{ "Burnt out ", "Lit ", "Toggle lights", "Increases your safe diving depth by ", " meters.", "Restores ", " health.", "mass ", ": min ", ", max ", "Throw", "Light and throw", "Light", "Toggle map", "Push ", "Need a knife to break it", "Need a knife to break it free", "Toggle lights", ". Press and hold ", " to charge the shot", " Change torpedo ", " Hold ", " and press ", " to change torpedo ", "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.", "Generates a localized electric field designed to ward off aggressive fauna. Press and hold the button to charge the shot.", "Swivel left", "Swivel right" };

                Main.config.Save();
            }
            //foreach (string s in Main.config.translatableStrings)
            //    Main.Log("translatableStrings " + s);
            altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);
            cycleNextButton = uGUI.FormatButton(GameInput.Button.CycleNext);
            cyclePrevButton = uGUI.FormatButton(GameInput.Button.CyclePrev);
            deconstructButton = uGUI.FormatButton(GameInput.Button.Deconstruct);
            moveLeftButton = uGUI.FormatButton(GameInput.Button.MoveLeft);
            moveRightButton = uGUI.FormatButton(GameInput.Button.MoveRight);
            Exosuit_Patch.exosuitName = Language.main.Get("Exosuit");
            propCannonString = LanguageCache.GetButtonFormat("PropulsionCannonToRelease", GameInput.Button.AltTool);
            Exosuit_Patch.armNamesChanged = true;
            dropString = TooltipFactory.stringDrop + " (" + TooltipFactory.stringButton1 + ")";
            eatString = TooltipFactory.stringEat + " (" + altToolButton + ")";
            lightFlareString = Main.config.translatableStrings[12] + " (" + altToolButton + ")";
            throwFlareString = Main.config.translatableStrings[10] + " (" + TooltipFactory.stringButton1 + ")";
            lightAndThrowFlareString = Main.config.translatableStrings[11] + " (" + TooltipFactory.stringButton1 + ")";
            swivelText = Main.config.translatableStrings[26] + " (" + moveLeftButton + ")  " + Main.config.translatableStrings[27] + " (" + moveRightButton + ")";
            beaconToolString = TooltipFactory.stringDrop + " (" + TooltipFactory.stringButton1 + ")  " + Language.main.Get("BeaconLabelEdit") + " (" + deconstructButton + ")";
            beaconPickString = "(" + TooltipFactory.stringButton0 + ")\n" + Language.main.Get("BeaconLabelEdit") + " (" + deconstructButton + ")";
            smallStorageString = "\n" + LanguageCache.GetPackUpText(TechType.SmallStorage) + " (" + altToolButton + ")\n";
            //seaglideString = Main.config.translatableStrings[2] + " (" + TooltipFactory.stringRightHand + ")  " + Main.config.translatableStrings[13] + " (" + altToolButton + ")";
            seaglideString = Main.config.translatableStrings[13] + " (" + altToolButton + ")";
            changeTorpedoButton = Main.config.translatableStrings[20] + "(" + altToolButton + ")";
            changeTorpedoExosuitButtonGamepad = Main.config.translatableStrings[21] + "(" + altToolButton + ")" + Main.config.translatableStrings[22] + "(" + cycleNextButton + "), " + "(" + cyclePrevButton + ")" + Main.config.translatableStrings[23];
            slot1Button = "(" + uGUI.FormatButton(GameInput.Button.Slot1) + ")";
            slot2Button = "(" + uGUI.FormatButton(GameInput.Button.Slot2) + ")";
            slot1Plus2Button = slot1Button + slot2Button;
            //Main.Log("changeTorpedoButton GetStrings " + UI_Patches.changeTorpedoButton);
            stasisRifleString = Main.config.translatableStrings[18].Substring(1) + "(" + TooltipFactory.stringButton1 + ")" + Main.config.translatableStrings[19];
            scannerString = LanguageCache.GetButtonFormat("ScannerSelfScanFormat", GameInput.Button.AltTool);
            exosuitExitButton = LanguageCache.GetButtonFormat("PressToExit", GameInput.Button.Exit) + ' ' + Main.config.translatableStrings[17] + " (" + deconstructButton + ")";
            Exosuit exosuit = Player.main.currentMountedVehicle as Exosuit;
            if (exosuit)
                Exosuit_Patch.GetArmNames(exosuit);
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
                    //AddDebug(" equipment ");
                    bool charger = equipment.GetCompatibleSlot( EquipmentType.BatteryCharger, out string s) || equipment.GetCompatibleSlot(EquipmentType.PowerCellCharger, out string ss);
                    //AddDebug("charger " + charger);
                    foreach (var pair in __instance.inventory.items)
                    {
                        EquipmentType itemType = CraftData.GetEquipmentType(pair.Key.item.GetTechType());
                        //AddDebug(pair.Key.item.GetTechType() + " " + itemType);
                        string slot = string.Empty;
                        if (equipment.GetCompatibleSlot(itemType, out slot))
                        {
                            //AddDebug(__instance.name + " slot " + slot);
                            //EquipmentType chargerType = Equipment.GetSlotType(slot);
                            //AddDebug(__instance.name + " " + chargerType);
                            //if (chargerType == EquipmentType.BatteryCharger || chargerType ==  EquipmentType.PowerCellCharger)
                            if (charger)
                            {
                                chargerOpen = true;
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
            //[HarmonyPatch(nameof(GUIHand.OnUpdate))]
            //[HarmonyPrefix]
            public static bool OnUpdatePrefix(GUIHand __instance)
            {
                if (!__instance.activeTarget)
                    return true;

                UniqueIdentifier ui = __instance.activeTarget.GetComponentInParent<UniqueIdentifier>();
                if (ui && Plants_Patch.enteredColliders.ContainsKey(ui.gameObject) && Plants_Patch.enteredColliders[ui.gameObject] > 0)
                {
                    //HandReticle.main.SetInteractTextRaw(string.Empty, string.Empty);
                    return false;
                }
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnUpdate")]
            public static void OnUpdatePostfix(GUIHand __instance)
            {
                if (!Main.config.newUIstrings)
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
                        string text = string.Empty;
                        //string throwFlare = lit ? Main.config.throwFlare : Main.config.lightAndThrowFlare;
                        if (!lit && canThrow)
                        {
                            StringBuilder stringBuilder = new StringBuilder(lightAndThrowFlareString);
                            stringBuilder.Append(",  ");
                            stringBuilder.Append(lightFlareString);
                            text = stringBuilder.ToString();
                        }
                        else if(lit && canThrow)
                            text = throwFlareString;
                        else if (!lit && !canThrow)
                            text = lightFlareString;

                        if (!lit && GameInput.GetButtonDown(GameInput.Button.AltTool))
                            Flare_Patch.LightFlare(flare);

                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
                    }
                    Beacon beacon = tool as Beacon;
                    if (beacon)
                    {
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, beaconToolString);
                        if (beacon.beaconLabel && GameInput.GetButtonDown(GameInput.Button.Deconstruct))
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

                        bool cantEat = Main.config.cantEatUnderwater && Player.main.isUnderwater.value;
                        //Main.Log("GameInput.Button.RightHand) " + uGUI.FormatButton(GameInput.Button.RightHand));
                        if (!cantEat)
                        {
                            if (canDrop)
                                sb.Append(",  ");
                            sb.Append(eatString);
                            if (GameInput.GetButtonDown(GameInput.Button.AltTool))
                            {
                                Inventory playerInv = Inventory.main;
                                //playerInv.UseItem(playerInv.quickSlots.heldItem);
                                Inventory.main.ExecuteItemAction(ItemAction.Eat, heldItem);
                            }
                        }
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, sb.ToString());
                    }
                }
                else if(!Main.baseLightSwitchLoaded && !Player.main.pda.isInUse && !textInput && !uGUI._main.craftingMenu.selected)
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase && subRoot.powerRelay && subRoot.powerRelay.GetPowerStatus() != PowerSystem.Status.Offline)
                    {
                        StringBuilder sb = new StringBuilder(Main.config.translatableStrings[2]);
                        sb.Append(" (");
                        sb.Append(deconstructButton);
                        sb.Append(")");
                        HandReticle.main.SetTextRaw(HandReticle.TextType.Use, sb.ToString());
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                            Base_Patch.ToggleBaseLight(subRoot);
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
                if (flareTarget && Main.languageCheck && flareTarget.energyLeft == 0f)
                {
                    //AddDebug("activeTarget Flare");
                    StringBuilder sb = new StringBuilder(Main.config.translatableStrings[0]);
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
                if (Main.config.pdaTabSwitchHotkey && Input.GetKey(KeyCode.LeftShift))
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
        class PlayerWorldArrows_CreateWorldArrows_Patch
        {
            static bool Prefix(PlayerWorldArrows __instance)
            {
                //AddDebug("CreateWorldArrows");
                return !Main.config.disableHints;
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
                if (!Main.config.newUIstrings)
                    return;

                if (Main.languageCheck)
                {
                    Flare flare = obj.GetComponent<Flare>();
                    if (flare)
                    {
                        //AddDebug("flare.energyLeft " + flare.energyLeft);
                        if (flare.energyLeft <= 0f)
                            TooltipFactory.WriteTitle(sb, Main.config.translatableStrings[0]);
                        else if (flare.flareActivateTime > 0f)
                            TooltipFactory.WriteTitle(sb, Main.config.translatableStrings[1]);
                    }
                }
                fishTooltip = Util.IsEatableFish(obj);
            }
          
            [HarmonyPostfix]
            [HarmonyPatch("ItemCommons")]
            static void ItemCommonsPostfix(StringBuilder sb, TechType techType, GameObject obj)
            {
                if (!Main.config.newUIstrings)
                    return;

                if (Main.languageCheck)
                {
                    if (Crush_Damage.crushDepthEquipment.ContainsKey(techType) && Crush_Damage.crushDepthEquipment[techType] > 0)
                    {
                        StringBuilder sb_ = new StringBuilder(Main.config.translatableStrings[3]);
                        sb_.Append(Crush_Damage.crushDepthEquipment[techType]);
                        sb_.Append(Main.config.translatableStrings[4]);
                        TooltipFactory.WriteDescription(sb, sb_.ToString());
                    }
                    if (techType == TechType.FirstAidKit)
                    {
                        StringBuilder sb_ = new StringBuilder(Main.config.translatableStrings[5]);
                        sb_.Append(Main.config.medKitHP);
                        sb_.Append(Main.config.translatableStrings[6]);
                        TooltipFactory.WriteDescription(sb, sb_.ToString());
                    }
                }
                if (Main.config.invMultLand > 0f || Main.config.invMultWater > 0f)
                {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb)
                    {
                        StringBuilder sb_ = new StringBuilder(Main.config.translatableStrings[7]);
                        sb_.Append(rb.mass);
                        TooltipFactory.WriteDescription(sb, sb_.ToString());
                    }
                }

            }
        }
        
        [HarmonyPatch(typeof(Language), "FormatString", new Type[] { typeof(string), typeof(object[]) })]
        class Language_FormatString_Patch
        {
            static void Postfix(string format, ref string __result, object[] args)
            {
                if (!Main.config.newUIstrings)
                    return;
                //AddDebug("FormatString " + format);
                //AddDebug("!Main.languageCheck " + !Main.languageCheck);
                //AddDebug("FormatString GetType" + args[0].GetType());
                //if (format.Contains("FOOD:") || format.Contains("H₂O:"))
                //    AddDebug("FormatString " + __result);

                if (!Main.languageCheck || !fishTooltip || Main.config.eatRawFish == Config.EatingRawFish.Vanilla || args.Length == 0 || args[0].GetType() != typeof(Int32)  )
                    return;

                int value = (int)args[0];
                if (value > 0f && format.Contains("FOOD:") || format.Contains("H₂O:"))
                {
                    string[] tokens = __result.Split(':');
                    string min = Main.config.translatableStrings[8];
                    string max = Main.config.translatableStrings[9];
                    StringBuilder sb_ = new StringBuilder(tokens[0]);
                    sb_.Append(min);
                    if (Main.config.eatRawFish == Config.EatingRawFish.Harmless)
                    {
                        //__result = tokens[0] + min + "0" + max + value;
                        sb_.Append("0");
                        sb_.Append(max);
                        sb_.Append(value);
                    }
                    else if (Main.config.eatRawFish == Config.EatingRawFish.Risky)
                    {
                        //__result = tokens[0] + min + "-" + value + max + value;
                        sb_.Append("-");
                        sb_.Append(value);
                        sb_.Append(max);
                        sb_.Append(value);
                    }
                    else if(Main.config.eatRawFish == Config.EatingRawFish.Harmful)
                    {
                        //__result = tokens[0] + min + "-" + value + max + "0";
                        sb_.Append("-");
                        sb_.Append(value);
                        sb_.Append(max);
                        sb_.Append("0");
                    }
                    __result = sb_.ToString();
                }
            }
        }
        
        [HarmonyPatch(typeof(Inventory), "ExecuteItemAction", new Type[] {typeof(ItemAction), typeof(InventoryItem)})]
        class Inventory_ExecuteItemAction_Patch
        {
            public static bool Prefix(Inventory __instance, InventoryItem item, ItemAction action)
            {
                //AddDebug("ExecuteItemAction AltUseItem " + item.item.GetTechType());
                //AddDebug("ExecuteItemAction action " + action);
                //ItemAction itemAction = __instance.GetAltUseItemAction(item);
                IItemsContainer oppositeContainer = __instance.GetOppositeContainer(item);
                if (Main.advancedInventoryLoaded || action != ItemAction.Switch || oppositeContainer == null || item.container is Equipment || oppositeContainer is Equipment)
                    return true;

                ItemsContainer container = (ItemsContainer)item.container;
                List<InventoryItem> itemsToTransfer = new List<InventoryItem>();
                if (Input.GetKey(Main.config.transferAllItemsKey))
                {
                    //AddDebug("LeftShift ");
                    foreach (TechType itemType in container.GetItemTypes())
                        container.GetItems(itemType, itemsToTransfer);
                }
                else if (Input.GetKey(Main.config.transferSameItemsKey))
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

        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_HealthBar __instance)
            {
                if (!Main.config.alwaysShowHealthNunbers)
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
                        float has = Main.config.newPoisonSystem ? liveMixin.health : liveMixin.health - liveMixin.tempDamage;

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
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
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
                if (!Main.config.alwaysShowHealthNunbers)
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
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
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
                if (!Main.config.alwaysShowHealthNunbers)
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
                    if (Main.config.alwaysShowHealthNunbers || pda != null && pda.isInUse)
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

        [HarmonyPatch(typeof(PlayerTool), "GetCustomUseText")]
        class PlayerTool_GetCustomUseText_Patch
        {
            static void Postfix(PlayerTool __instance, ref string __result)
            {
                if (!Main.config.newUIstrings)
                    return;

                if (!Main.seaglideMapControlsLoaded && __instance is Seaglide)
                    __result = seaglideString;
            }
        }

        [HarmonyPatch(typeof(HandReticle), "SetTextRaw")]
        class HandReticle_SetTextRaw_Patch
        {
            static bool Prefix(HandReticle __instance, HandReticle.TextType type, string text)
            {
                //AddDebug("SetTextRaw " + type + " " + text);
                if (Main.config.disableUseText && (type == HandReticle.TextType.Use || type == HandReticle.TextType.UseSubscript))
                {
                    return false;
                }
                return true;
            }
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


    }
}
