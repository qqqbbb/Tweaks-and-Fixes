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
        static bool chargerOpen = false;
        static List <TechType> landPlantSeeds = new List<TechType> { TechType.BulboTreePiece, TechType.PurpleVegetable, TechType.FernPalmSeed, TechType.OrangePetalsPlantSeed, TechType.HangingFruit, TechType.MelonSeed, TechType.PurpleVasePlantSeed, TechType.PinkMushroomSpore, TechType.PurpleRattleSpore, TechType.PinkFlowerSeed };
        static List<TechType> waterPlantSeeds = new List<TechType> { TechType.CreepvineSeedCluster, TechType.AcidMushroomSpore, TechType.BloodOil, TechType.BluePalmSeed, TechType.KooshChunk, TechType.PurpleBranchesSeed, TechType.WhiteMushroomSpore, TechType.EyesPlantSeed, TechType.RedRollPlantSeed, TechType.GabeSFeatherSeed, TechType.JellyPlantSeed, TechType.RedGreenTentacleSeed, TechType.SnakeMushroomSpore, TechType.MembrainTreeSeed, TechType.SmallFanSeed, TechType.RedBushSeed, TechType.RedConePlantSeed, TechType.RedBasketPlantSeed, TechType.SeaCrownSeed, TechType.ShellGrassSeed, TechType.SpottedLeavesPlantSeed, TechType.SpikePlantSeed, TechType.PurpleFanSeed, TechType.PurpleStalkSeed, TechType.PurpleTentacleSeed };
        static HashSet<ItemsContainer> landPlanters = new HashSet<ItemsContainer>();
        static HashSet<ItemsContainer> waterPlanters = new HashSet<ItemsContainer>();

        [HarmonyPatch(typeof(Aquarium), "Start")]
        class Aquarium_Start_Patch
        {
            static void Postfix(Aquarium __instance)
            {
                //AddDebug("Trashcan " + __instance.biohazard + " " + __instance.storageContainer.hoverText);
                //__instance.storageContainer.hoverText = Language.main.Get("LabTrashcan");
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
                //if (__instance.storageContainer.container.allowedTech == null)
                {
                    //AddDebug("Planter allowedTech == null ");
                    //TechType techType = CraftData.GetTechType(__instance.gameObject);
                    ItemsContainerType type = __instance.GetContainerType();
                    if (type == ItemsContainerType.LandPlants)
                        landPlanters.Add(__instance.storageContainer.container);
                    else if (type == ItemsContainerType.WaterPlants)
                        waterPlanters.Add(__instance.storageContainer.container);
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_InventoryTab))]
        class uGUI_InventoryTab_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnOpenPDA")]
            static void OnOpenPDAPostfix(uGUI_InventoryTab __instance)
            {
                IItemsContainer itemsContainer = Inventory.main.GetUsedStorage(0);
                Equipment equipment = itemsContainer as Equipment;
                ItemsContainer container = itemsContainer as ItemsContainer;
                //    AddDebug("GetUsedStorageCount " + Inventory.main.GetUsedStorageCount());
                if (container != null)
                {
                    //AddDebug(" container ");
                    if (landPlanters.Contains(container))
                    {
                        //AddDebug(" landPlanter ");
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!landPlantSeeds.Contains(pair.Key.item.GetTechType()))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    else if (waterPlanters.Contains(container))
                    {
                        //AddDebug(" waterPlanter ");
                        foreach (var pair in __instance.inventory.items)
                        {
                            if (!waterPlantSeeds.Contains(pair.Key.item.GetTechType()))
                                pair.Value.SetChroma(0f);
                        }
                        return;
                    }
                    foreach (var pair in __instance.inventory.items)
                    {
                        if (!container.IsTechTypeAllowed(pair.Key.item.GetTechType()))
                            pair.Value.SetChroma(0f);
                    }
                }
                if (equipment != null)
                {
                    //AddDebug(" equipment != null ");
                    bool charger = equipment.GetCompatibleSlot( EquipmentType.BatteryCharger, out string s) || equipment.GetCompatibleSlot(EquipmentType.PowerCellCharger, out string ss);
                    //AddDebug("charger " + charger);
                    foreach (var pair in __instance.inventory.items)
                    {
                        EquipmentType itemType = CraftData.GetEquipmentType(pair.Key.item.GetTechType());
                        //AddDebug(pair.Key.item.GetTechType() + " " + equipmentType);
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

        [HarmonyPatch(typeof(StorageContainer), "Open", new Type[] { typeof(Transform) })]
        class StorageContainer_Open_Patch
        {// fix bug: when opening standing locker from a distance your PDA does not open
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
            static string altToolButton = string.Empty;

            //[HarmonyPatch(nameof(GUIHand.OnUpdate))]
            //[HarmonyPrefix]
            public static bool OnUpdatePrefix(GUIHand __instance)
            {
                if (!__instance.activeTarget)
                    return true;

                UniqueIdentifier ui = __instance.activeTarget.GetComponentInParent<UniqueIdentifier>();
                if (ui && Plants_Patch.enteredColliders.ContainsKey(ui.gameObject) && Plants_Patch.enteredColliders[ui.gameObject] > 0)
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
                        if (string.IsNullOrEmpty(altToolButton))
                            altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);

                        if (!lit)
                        {
                            string text1 = Main.config.lightFlare + " (" + altToolButton + ")";
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
                        if (!cantEat)
                        {
                            if (string.IsNullOrEmpty(altToolButton))
                                altToolButton = uGUI.FormatButton(GameInput.Button.AltTool);

                            string text1 = TooltipFactory.stringEat + " (" + altToolButton + ")";
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
                else
                {
                    SubRoot subRoot = Player.main.currentSub;
                    if (subRoot && subRoot.isBase)
                    {
                        string text = "Toggle lights (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
                        HandReticle.main.SetUseTextRaw(null, text);
                        if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                        {
                            Base_Patch.ToggleBaseLight(subRoot);
                        }
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
                if (targetTT == TechType.Flare && Main.english)
                {
                    //AddDebug("activeTarget Flare");
                    string name = Language.main.Get(targetTT);
                    name = "Burnt out " + name;
					HandReticle.main.SetInteractText(name);
                }
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
                        TooltipFactory.WriteTitle(sb, "Burnt out ");
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

        [HarmonyPatch(typeof(uGUI_HealthBar), "LateUpdate")]
        class uGUI_HealthBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_HealthBar __instance)
            {
                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    LiveMixin lm = main.GetComponent<LiveMixin>();
                    if (lm != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            lm.onHealDamage.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnHealDamage));
                        }
                        float health = lm.health - lm.tempDamage;
                        if (Main.config.newPoisonSystem)
                            health = lm.health;
                        float maxHealth = lm.maxHealth;
                        __instance.SetValue(health, maxHealth);
                        float time = 1f - Mathf.Clamp01(health / __instance.pulseReferenceCapacity);
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
                    if (Main.config.alwaysShowHealthNunbers || pda && pda.isInUse)
                        __instance.showNumbers = true;
                }
                if ((TrackedReference)__instance.pulseAnimationState != (TrackedReference)null && __instance.pulseAnimationState.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                double time1 = Time.time;
                float dT = 0.02f;
                double lastFixedUpdateTime = __instance.lastFixedUpdateTime;
                float f = (float)(time1 - lastFixedUpdateTime);
                int num4 = Mathf.FloorToInt(f);
                if (num4 > 20)
                {
                    num4 = 1;
                    dT = f;
                }
                __instance.lastFixedUpdateTime += num4 * dT;
                for (int index = 0; index < num4; ++index)
                {
                    double rotationCurrent1 = __instance.rotationCurrent;
                    float target = __instance.showNumbers ? 180f : 0f;
                    MathExtensions.Spring(ref __instance.rotationVelocity, ref __instance.rotationCurrent, target, __instance.rotationSpringCoef, dT, __instance.rotationVelocityDamp, __instance.rotationVelocityMax);
                    if (Mathf.Abs(target - __instance.rotationCurrent) < 1f && Mathf.Abs(__instance.rotationVelocity) < 1f)
                    {
                        __instance.rotationVelocity = 0f;
                        __instance.rotationCurrent = target;
                    }
                    double rotationCurrent2 = __instance.rotationCurrent;
                    if (rotationCurrent1 != rotationCurrent2)
                        __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_FoodBar), "LateUpdate")]
        class uGUI_FoodBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_FoodBar __instance)
            {
                int showNumbers = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    Survival survival = main.GetComponent<Survival>();
                    if (survival != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            survival.onEat.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnEat));
                        }
                        float food = survival.food;
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
                if ((TrackedReference)__instance.pulseAnimationState != (TrackedReference)null && __instance.pulseAnimation.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (showNumbers != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                double time1 = Time.time;
                float dT = 0.02f;
                double lastFixedUpdateTime = __instance.lastFixedUpdateTime;
                float f = (float)(time1 - lastFixedUpdateTime);
                int num4 = Mathf.FloorToInt(f);
                if (num4 > 20)
                {
                    num4 = 1;
                    dT = f;
                }
                __instance.lastFixedUpdateTime += num4 * dT;
                for (int index = 0; index < num4; ++index)
                {
                    double rotationCurrent1 = __instance.rotationCurrent;
                    float target = __instance.showNumbers ? 180f : 0.0f;
                    MathExtensions.Spring(ref __instance.rotationVelocity, ref __instance.rotationCurrent, target, __instance.rotationSpringCoef, dT, __instance.rotationVelocityDamp, __instance.rotationVelocityMax);
                    if (Mathf.Abs(target - __instance.rotationCurrent) < 1f && Mathf.Abs(__instance.rotationVelocity) < 1f)
                    {
                        __instance.rotationVelocity = 0f;
                        __instance.rotationCurrent = target;
                    }
                    double rotationCurrent2 = __instance.rotationCurrent;
                    if (rotationCurrent1 != rotationCurrent2)
                        __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(uGUI_WaterBar), "LateUpdate")]
        class uGUI_WaterBar_LateUpdate_Patch
        {
            public static bool Prefix(uGUI_WaterBar __instance)
            {
                int num1 = __instance.showNumbers ? 1 : 0;
                __instance.showNumbers = false;
                Player main = Player.main;
                if (main != null)
                {
                    Survival survival = main.GetComponent<Survival>();
                    if (survival != null)
                    {
                        if (!__instance.subscribed)
                        {
                            __instance.subscribed = true;
                            survival.onDrink.AddHandler(__instance.gameObject, new UWE.Event<float>.HandleFunction(__instance.OnDrink));
                        }
                        float water = survival.water;
                        float capacity = 100f;
                        __instance.SetValue(water, capacity);
                        float time = 1f - Mathf.Clamp01(water / __instance.pulseReferenceCapacity);
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
                if ((TrackedReference)__instance.pulseAnimationState != (TrackedReference)null && __instance.pulseAnimationState.enabled)
                {
                    RectTransform icon = __instance.icon;
                    icon.localScale = icon.localScale + __instance.punchScale;
                }
                else
                    __instance.icon.localScale = __instance.punchScale;
                int num3 = __instance.showNumbers ? 1 : 0;
                if (num1 != num3)
                    __instance.rotationVelocity += UnityEngine.Random.Range(-__instance.rotationRandomVelocity, __instance.rotationRandomVelocity);
                double time1 = Time.time;
                float dT = 0.02f;
                double lastFixedUpdateTime = __instance.lastFixedUpdateTime;
                float f = (float)(time1 - lastFixedUpdateTime);
                int num4 = Mathf.FloorToInt(f);
                if (num4 > 20)
                {
                    num4 = 1;
                    dT = f;
                }
                __instance.lastFixedUpdateTime += num4 * dT;
                for (int index = 0; index < num4; ++index)
                {
                    double rotationCurrent1 = __instance.rotationCurrent;
                    float target = __instance.showNumbers ? 180f : 0f;
                    MathExtensions.Spring(ref __instance.rotationVelocity, ref __instance.rotationCurrent, target, __instance.rotationSpringCoef, dT, __instance.rotationVelocityDamp, __instance.rotationVelocityMax);
                    if (Mathf.Abs(target - __instance.rotationCurrent) < 1f && Mathf.Abs(__instance.rotationVelocity) < 1f)
                    {
                        __instance.rotationVelocity = 0f;
                        __instance.rotationCurrent = target;
                    }
                    double rotationCurrent2 = __instance.rotationCurrent;
                    if (rotationCurrent1 != rotationCurrent2)
                        __instance.icon.localRotation = Quaternion.Euler(0f, __instance.rotationCurrent, 0f);
                }
                return false;
            }
        }

    }
}
