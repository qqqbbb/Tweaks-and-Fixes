
using System;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class QuickSlots_Patch
    {
        static HashSet<TechType> eqiupped ;
        static Queue<InventoryItem> toEqiup;
        static HashSet<TechType> toEqiupTT;
        public static GameInput.Button quickslotButton;
        public static GameInput.Button lightButton;
        public static bool invChanged = true; 

        public static void GetTools()
        {
            toEqiup = new Queue<InventoryItem>();
            toEqiupTT = new HashSet<TechType>();
            GetEquippedTools();
            //Main.Log("GetTools " );
            foreach (InventoryItem item in Inventory.main.container)
            {
                if (item.item.GetComponent<PlayerTool>() && !item.item.GetComponent<Eatable>())
                { // eatable fish is PlayerTool
                    TechType techType = item.item.GetTechType();
                    if (!eqiupped.Contains(techType) && !toEqiupTT.Contains(techType))
                    {
                        toEqiup.Enqueue(item);
                        toEqiupTT.Add(techType);
                        //AddDebug("toEqiup " + techType);
                        //Main.Log("toEqiup " + techType);
                    }
                }
            }
        }

        public static void GetEquippedTools()
        {
            eqiupped = new HashSet<TechType>();
            //Main.Log("GetEquippedTools");
            foreach (TechType item in Inventory.main.quickSlots.GetSlotBinding())
            {
                eqiupped.Add(item);
                //Main.Log("eqiupped " + item);
            }
        }

        private static void EquipNextTool()
        {
            if (invChanged)
            {
                GetTools();
                invChanged = false;
            }
            int activeSlot = Inventory.main.quickSlots.activeSlot;
            InventoryItem currentItem = Inventory.main.quickSlots.binding[activeSlot];
            //if (currentItem == null) 
            //    AddDebug("currentItem == null ");
            //AddDebug("currentItem " + currentItem.item.GetTechName());
            //AddDebug("toEqiup Remove " + toEqiup.Peek().item.GetTechName());
            Inventory.main.quickSlots.Bind(activeSlot, toEqiup.Peek());
            toEqiup.Dequeue();
            toEqiup.Enqueue(currentItem);
            Inventory.main.quickSlots.SelectImmediate(activeSlot);
            //GetEquippedTools();
        }

        [HarmonyPatch(typeof(Inventory))]
        class Inventory_OnAddItem_Patch
        { // this called during loading and tools returned are wrong
            [HarmonyPostfix]
            [HarmonyPatch( "OnAddItem")]
            public static void OnAddItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnAddItem ");
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnRemoveItem")]
            public static void OnRemoveItemPostfix(Inventory __instance, InventoryItem item)
            {
                if (item != null && item.isBindable)
                {
                    //GetTools();
                    invChanged = true;
                    //AddDebug("Inventory OnRemoveItem ");
                }
            }
        }

        [HarmonyPatch(typeof(QuickSlots))]
        class QuickSlots_Bind_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Bind")]
            public static void BindPostfix(QuickSlots __instance)
            {
                GetEquippedTools();
                //AddDebug(" Bind ");
            }
            [HarmonyPrefix]
            [HarmonyPatch("SlotNext")]
            public static bool SlotNextPrefix(QuickSlots __instance)
            {
                //AddDebug("SlotNext");
                if (Input.GetKey(ConfigMenu.quickslotButton.Value) || GameInput.GetButtonHeld(quickslotButton))
                {
                    //AddDebug("quickslotButton");
                    Pickupable pickupable = Inventory.main.GetHeld();
                    if (pickupable != null)
                    {
                        EquipNextTool();
                        return false;
                    }
                }
                else if (Input.GetKey(ConfigMenu.lightButton.Value) || GameInput.GetButtonHeld(lightButton))
                {
                    //AddDebug("lightButton");
                    Pickupable p = Inventory.main.GetHeld();
                    if (!p)
                    //if (!p && Player.main.currentSub)
                    {
                        //AddDebug("currentSub " + Player.main.currentSub.lightControl.skies.Length);
                        //foreach (LightingController.MultiStatesSky mss in Player.main.currentSub.lightControl.skies)
                        //{
                        //AddDebug("MultiStatesSky " + Main.GetGameObjectPath(mss.sky.gameObject) + " " + mss.masterIntensities[0] + " " + mss.masterIntensities[1] + " " + mss.masterIntensities[2]);
                        //    mss.sky.SpecIntensity -= .1f;
                        //    AddDebug("SpecIntensity " + mss.sky.SpecIntensity);
                        //}
                        return true;
                    }
                    else
                    {
                        Light[] lights = p.GetComponentsInChildren<Light>();
                        //AddDebug("lights.Length  " + lights.Length);
                        if (lights.Length == 0 || !lights[0].gameObject.activeInHierarchy)
                            return true;

                        TechType tt = CraftData.GetTechType(p.gameObject);
                        //AddDebug("lights TechType " + tt);
                        if (tt == TechType.DiveReel || tt == TechType.LaserCutter)
                            return true;

                        if (!Tools_Patch.lightIntensityStep.ContainsKey(tt))
                        {
                            AddDebug("lightIntensityStep missing " + tt);
                            return false;
                        }
                        if (!Tools_Patch.lightOrigIntensity.ContainsKey(tt))
                        {
                            AddDebug("lightOrigIntensity missing " + tt);
                            return false;
                        }
                        float origIntensity = Tools_Patch.lightOrigIntensity[tt];
                        //AddDebug("origIntensity " + origIntensity);
                        //float step = origIntensity / 15f;
                        Flare flare = p.GetComponent<Flare>();
                        if (flare && flare.flareActivateTime == 0)
                            return true;

                        foreach (Light l in lights)
                        {
                            if (l.intensity < origIntensity)
                            {
                                l.intensity += Tools_Patch.lightIntensityStep[tt];
                                //AddDebug("Light Intensity Up " + l.intensity);
                                Main.configMain.lightIntensity[tt] = l.intensity;
                            }
                            if (flare)
                            {
                                Flare_Patch.intensityChanged = true;
                                Flare_Patch.originalIntensity = l.intensity;
                                Flare_Patch.halfOrigIntensity = Flare_Patch.originalIntensity * .5f;
                            }
                        }
                        return false;
                    }
                }
                return true;
            }
            [HarmonyPrefix]
            [HarmonyPatch("SlotPrevious")]
            public static bool SlotPreviousPrefix(QuickSlots __instance)
            {
                if (Input.GetKey(ConfigMenu.quickslotButton.Value) || GameInput.GetButtonHeld(quickslotButton))
                //if (Input.GetKey(Main.configOld.quickslotKey) || GameInput.GetButtonHeld(quickslotButton))
                {
                    Pickupable pickupable = Inventory.main.GetHeld();
                    if (pickupable != null)
                    {
                        EquipNextTool();
                        return false;
                    }
                }
                else if (Input.GetKey(ConfigMenu.lightButton.Value) || GameInput.GetButtonHeld(lightButton))
                {
                    Pickupable p = Inventory.main.GetHeld();
                    if (!p)
                        return true;
                    else
                    {
                        Light[] lights = p.GetComponentsInChildren<Light>();
                        //AddDebug("lights.Length  " + lights.Length);
                        if (lights.Length == 0)
                        {
                            //AddDebug("lights.Length == 0 ");
                            return true;
                        }
                        TechType tt = CraftData.GetTechType(p.gameObject);
                        if (tt == TechType.DiveReel || tt == TechType.LaserCutter)
                            return true;
                        if (!Tools_Patch.lightIntensityStep.ContainsKey(tt))
                        {
                            AddDebug("lightIntensityStep missing " + tt);
                            return false;
                        }
                        if (!Tools_Patch.lightOrigIntensity.ContainsKey(tt))
                        {
                            AddDebug("lightOrigIntensity missing " + tt);
                            return false;
                        }
                        //float origIntensity = Tools_Patch.lightOrigIntensity[CraftData.GetTechType(p.gameObject)];
                        //float step = origIntensity / 15f;
                        Flare flare = p.GetComponent<Flare>();
                        if (flare && flare.flareActivateTime == 0)
                            return true;

                        foreach (Light l in lights)
                        {
                            l.intensity -= Tools_Patch.lightIntensityStep[tt];
                            //AddDebug("Light Intensity Down " + l.intensity);
                            //AddDebug("Light Intensity Step " + Tools_Patch.lightIntensityStep[tt]);
                            Main.configMain.lightIntensity[tt] = l.intensity;
                            if (flare)
                            {
                                Flare_Patch.intensityChanged = true;
                                Flare_Patch.originalIntensity = l.intensity;
                                Flare_Patch.halfOrigIntensity = Flare_Patch.originalIntensity * .5f;
                            }
                        }
                        return false;
                    }
                }
                return true;
            }

        }



    }
}
