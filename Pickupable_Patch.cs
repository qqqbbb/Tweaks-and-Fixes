using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Pickupable_Patch
    {

        public static Dictionary<TechType, float> itemMass = new Dictionary<TechType, float>();
        public static HashSet<TechType> shinies = new HashSet<TechType>();


        [HarmonyPatch(typeof(Pickupable), "Awake")]
        class Pickupable_Awake_Patch
        {
            static void Postfix(Pickupable __instance)
            {
                TechType tt = __instance.GetTechType();
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
                    EcoTarget et = __instance.gameObject.GetComponent<EcoTarget>();
                    if (et && et.type == EcoTargetType.Shiny)
                        return;
                    et = __instance.gameObject.AddComponent<EcoTarget>();
                    et.type = EcoTargetType.Shiny;
                }
                //if (tt == TechType.CyclopsDecoy)
                //{
                //    if (__instance.transform.parent.name == "CellRoot(Clone)")
                //    {
                //        PrefabIdentifier pi = __instance.GetComponent<PrefabIdentifier>();
                //}
                //}
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
                    if (techType == TechType.EnzymeCureBall)
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
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetUseItemAction")]
        internal class Inventory_GetUseItemAction_Patch
        {
            internal static void Postfix(Inventory __instance, ref ItemAction __result, InventoryItem item)
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
