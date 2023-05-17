using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_Patch
    {

        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();
        public static HashSet<TechType> shinies = new HashSet<TechType>();
        public static HashSet<TechType> unmovableItems = new HashSet<TechType>();
        
        [HarmonyPatch(typeof(Pickupable))]
        public class Pickupable_Patch_
        {
            [HarmonyPostfix]
            [HarmonyPatch("Awake")]
            static void AwakePostfix(Pickupable __instance)
            {
                TechType tt = __instance.GetTechType();
                if (unmovableItems.Contains(tt))
                { // isKinematic gets saved
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                }
                if (itemMass.ContainsKey(tt))
                {
                    Rigidbody rb = __instance.GetComponent<Rigidbody>();
                    if (rb)
                        rb.mass = itemMass[tt];
                }
                if (shinies.Contains(tt))
                {
                    HardnessMixin hm = __instance.gameObject.EnsureComponent<HardnessMixin>();
                    hm.hardness = 1f;
                    EcoTarget[] ets = __instance.gameObject.GetComponents<EcoTarget>();
                    foreach (EcoTarget et in ets)
                    {
                        if (et && et.type == EcoTargetType.Shiny)
                            return;
                    }
                    EcoTarget ecoTarget = __instance.gameObject.AddComponent<EcoTarget>();
                    ecoTarget.type = EcoTargetType.Shiny;
                }

            }
            
            //[HarmonyPrefix]
            //[HarmonyPatch("OnHandHover")]
            public static bool OnHandHoverPrefix(Pickupable __instance, GUIHand hand)
            {
                if (!hand.IsFreeToInteract())
                    return false;

                TechType techType = __instance.GetTechType();
                HandReticle handReticle = HandReticle.main;
                //AddDebug("Pickupable OnHandHover " + techType);
                if (__instance.AllowedToPickUp())
                {
                    string text1 = string.Empty;
                    string text2 = string.Empty;
                    Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
                    bool canPickUp = exosuit == null || exosuit.HasClaw();
                    if (canPickUp)
                    {
                        //AddDebug("canPickUp");
                        ISecondaryTooltip component = __instance.gameObject.GetComponent<ISecondaryTooltip>();
                        if (component != null)
                            text2 = component.GetSecondaryTooltip();

                        text1 = __instance.usePackUpIcon ? LanguageCache.GetPackUpText(techType) : LanguageCache.GetPickupText(techType);
                        //AddDebug("text2 " + text2);
                        handReticle.SetIcon(__instance.usePackUpIcon ? HandReticle.IconType.PackUp : HandReticle.IconType.Hand);
                    }
                    if (exosuit)
                    {
                        //AddDebug("exosuit");
                        GameInput.Button button = canPickUp ? GameInput.Button.LeftHand : GameInput.Button.None;
                        if (exosuit.leftArmType != TechType.ExosuitClawArmModule)
                            button = GameInput.Button.RightHand;

                        HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, button);
                        HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                    }
                    else
                    {
                        if (techType == TechType.Beacon)
                        {
                            //AddDebug("Beacon ");
                            BeaconLabel beaconLabel = __instance.GetComponentInChildren<BeaconLabel>();
                            if (beaconLabel)
                            {
                                if (GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                                    uGUI.main.userInput.RequestString(beaconLabel.stringBeaconLabel, beaconLabel.stringBeaconSubmit, beaconLabel.labelName, 25, new uGUI_UserInput.UserInputCallback(beaconLabel.SetLabel));
                                text2 = beaconLabel.labelName;
                            }
                            text1 = "(" + TooltipFactory.stringButton0 + ")\n" + Language.main.Get("BeaconLabelEdit") + " (" + uGUI.FormatButton(GameInput.Button.Deconstruct) + ")";
                            StringBuilder stringBuilder = new StringBuilder(text1);
                            stringBuilder.Append(UI_Patches.beaconPickString);

                            HandReticle.main.SetText(HandReticle.TextType.Hand, stringBuilder.ToString(), false, GameInput.Button.LeftHand);
                            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);

                            //handReticle.SetInteractTextRaw(stringBuilder.ToString(), text2);
                        }
                        else
                        {
                            HandReticle.main.SetText(HandReticle.TextType.Hand, text1, false, GameInput.Button.LeftHand);
                            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, text2, false);
                        }
                    }
                }
                else if (__instance.isPickupable && !Player.main.HasInventoryRoom(__instance))
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", true);
                }
                else
                {
                    HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
                    HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false);
                }
                
                return false;
            }
        }
            
        //[HarmonyPatch(typeof(BeaconLabel))]
        class BeaconLabel_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("Start")]
            static void StartPostfix(BeaconLabel __instance)
            {
                Collider collider = __instance.GetComponent<Collider>();
                if (collider)
                    UnityEngine.Object.Destroy(collider);
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnPickedUp")]
            static bool OnPickedUpPrefix(BeaconLabel __instance)
            {
                return false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnDropped")]
            static bool OnDroppedPrefix(BeaconLabel __instance)
            {
                return false;
            }
        }
            
        [HarmonyPatch(typeof(Survival), "Use")]
        class Survival_Awake_Patch
        {
            static bool Prefix(Survival __instance, GameObject useObj, ref bool __result)
            {
                __result = false;
                if (useObj != null)
                {
                    TechType techType = CraftData.GetTechType(useObj);
                    //AddDebug("Use" + techType);
                    if (techType == TechType.None)
                    {
                        Pickupable p = useObj.GetComponent<Pickupable>();
                        if (p)
                            techType = p.GetTechType();
                    }
                    if (techType == TechType.FirstAidKit)
                    {
                        if (Main.config.newPoisonSystem)
                        {
                            LiveMixin lm = Player.main.liveMixin;
                            lm.tempDamage = 0;
                        }
                        __result = true;
                        if (Main.config.medKitHPperSecond >= Main.config.medKitHP)
                        {
                            Player.main.GetComponent<LiveMixin>().AddHealth(Main.config.medKitHP);
                        }
                        else
                        {
                            //AddDebug("Time.timeScale " + Time.timeScale);
                            Main.config.medKitHPtoHeal = Main.config.medKitHP;
                            Player_Patches.healTime = Time.time;
                            //Player_Patches.healTime = DayNightCycle.main.timePassedAsFloat;
                        }
                    }
                    else if (techType == TechType.EnzymeCureBall)
                    {
                        InfectedMixin im = Utils.GetLocalPlayer().gameObject.GetComponent<InfectedMixin>();
                        if (im.IsInfected())
                        {
                            im.RemoveInfection();
                            Utils.PlayFMODAsset(__instance.curedSound, __instance.transform);
                            __result = true;
                        }
                    }
                    if (__result)
                    { 
                        FMODAsset so = ScriptableObject.CreateInstance<FMODAsset>();
                        so.path = CraftData.GetUseEatSound(techType);
                        Utils.PlayFMODAsset(so, __instance.transform);
                        //FMODUWE.PlayOneShot(CraftData.GetUseEatSound(techType), Player.main.transform.position);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetItemAction")]
        class Inventory_GetUseItemAction_Patch
        {
            static void Postfix(Inventory __instance, ref ItemAction __result, InventoryItem item)
            {
                Pickupable pickupable = item.item;
                TechType tt = pickupable.GetTechType();
                if (Main.config.cantEatUnderwater && Player.main.IsUnderwater())
                {
                    if (__result == ItemAction.Eat && pickupable.gameObject.GetComponent<Eatable>())
                    {
                        __result = ItemAction.None;
                        return;
                    }
                }
                if (tt == TechType.FirstAidKit && __result == ItemAction.Use)
                {
                    if (Main.config.cantUseMedkitUnderwater && Player.main.IsUnderwater())
                    {
                        __result = ItemAction.None;
                        return;
                    }
                    LiveMixin liveMixin = Player.main.GetComponent<LiveMixin>();
                    if (liveMixin.maxHealth - liveMixin.health < 0.1f)
                        __result = ItemAction.None;
                }
            }
        }

        
        }
    }
