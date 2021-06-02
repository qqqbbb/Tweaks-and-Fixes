
using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.API;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [QModCore]
    public class Main
    {
        public const float version = 1.09f;
        public static GUIHand guiHand;
        public static PDA pda;
        public static Survival survival;
        public static bool crafterOpen = false;
        public static bool canBreathe = false;
        public static bool loadingDone = false;
        public static System.Random rndm = new System.Random();

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        public static float NormalizeTo01range(int value, int min, int max)
        {
            float fl;
            int oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return fl;
        }

        public static float NormalizeTo01range(float value, float min, float max)
        {
            float fl;
            float oldRange = max - min;

            if (oldRange == 0)
                fl = 0f;
            else
                fl = ((float)value - (float)min) / (float)oldRange;

            return fl;
        }

        public static int NormalizeToRange(int value, int oldMin, int oldMax, int newMin, int newMax)
        {
            int oldRange = oldMax - oldMin;
            int newValue;

            if (oldRange == 0)
                newValue = newMin;
            else
            {
                int newRange = newMax - newMin;
                newValue = ((value - oldMin) * newRange) / oldRange + newMin;
            }
            return newValue;
        }

        public static float NormalizeToRange(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newValue;

            if (oldRange == 0)
                newValue = newMin;
            else
            {
                float newRange = newMax - newMin;
                newValue = ((value - oldMin) * newRange) / oldRange + newMin;
            }
            return newValue;
        }

        public static void DisableExosuitClawArmScan()
        {
            if (PDAScanner.mapping.ContainsKey(TechType.ExosuitClawArmFragment))
            {
                //Main.Message("DisableExosuitClawArmScan");
                PDAScanner.mapping.Remove(TechType.ExosuitClawArmFragment);
            }
        }

        public static bool IsEatableFishAlive(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            Eatable eatable = go.GetComponent<Eatable>();
            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            //if (creature && eatable && liveMixin && liveMixin.IsAlive())
            //    return true;

            return creature && eatable && liveMixin && liveMixin.IsAlive();
        }

        public static bool IsEatableFish(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            Eatable eatable = go.GetComponent<Eatable>();
            //if (creature && eatable)
            //    return true;

            return creature && eatable;
        }

        public static void CleanUp()
        {
            loadingDone = false;
            canBreathe = false;
            //AddDebug("CleanUp");
            QuickSlots_Patch.invChanged = true;
            Databox_Light_Patch.databoxLights = new List<GameObject>();
            Base_Light.SubRoot_Awake_Patch.bases = new HashSet<SubRoot>();
            Crush_Damage.extraCrushDepth = 0;
            crafterOpen = false;
            Cyclops_Patch.ceh = null;
            Gravsphere_Patch.gasPods = new HashSet<GasPod>();
            Gravsphere_Patch.gravSphereFish = new HashSet<Pickupable>();
            config.Load();
        }

        public static void Message(string str)
        {
            int count = main.messages.Count;

            if (count == 0)
            {
                AddDebug(str);
            }
            else
            {
                _Message message = main.messages[main.messages.Count - 1];
                message.messageText = str;
                message.entry.text = str;
            }
        }

        public static void Log(string str, QModManager.Utility.Logger.Level lvl = QModManager.Utility.Logger.Level.Info)
        {
            QModManager.Utility.Logger.Log(lvl, str);
        }

        //[HarmonyPatch(typeof(IngameMenu), "QuitGameAsync")]
        internal class IngameMenu_QuitGameAsync_Patch
        {
            public static void Postfix(IngameMenu __instance, bool quitToDesktop)
            {
                //AddDebug("QuitGameAsync " + quitToDesktop);
                if (!quitToDesktop)
                    CleanUp();
            }
        }

        [HarmonyPatch(typeof(Language), "Awake")]
        class Language_Awake_Patch
        {
            static void Postfix(Language __instance)
            {
                if (Language.main.currentLanguage == "English")
                {
                    //AddDebug("English");
                    //LanguageHandler.SetLanguageLine("Tooltip_Bladderfish", "Unique outer membrane has potential as a natural water filter. Can also be used as a source of oxygen.");
                    LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.");
                }
            }
        }

         [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Postfix(Player __instance)
            {
                survival = __instance.GetComponent<Survival>();
                //IngameMenuHandler.RegisterOnSaveEvent(config.Save);
                guiHand = __instance.GetComponent<GUIHand>();
                pda = __instance.GetPDA();
                if (config.cantScanExosuitClawArm)
                    DisableExosuitClawArmScan();

            }
        }

        [HarmonyPatch(typeof(uGUI_SceneLoading), "End")]
        internal class uGUI_SceneLoading_End_Patch
        { // fires after game loads
            public static void Postfix(uGUI_SceneLoading __instance)
            {
                if (!uGUI.main.hud.active)
                {
                    //AddDebug(" is Loading");
                    return;
                }
                //AddDebug(" uGUI_SceneLoading end");
                loadingDone = true;
                if (Cyclops_Patch.cyclopsHelmHUDManager)
                {
                    if (Cyclops_Patch.cyclopsHelmHUDManager.LOD.IsFull() && Player.main.currentSub != Cyclops_Patch.cyclopsHelmHUDManager.subRoot && !Cyclops_Patch.cyclopsHelmHUDManager.subRoot.subDestroyed)
                    {
                        Cyclops_Patch.cyclopsHelmHUDManager.canvasGroup.alpha = 0f;
                    }
                }
                if (EscapePod.main)
                    Escape_Pod_Patch.EscapePod_OnProtoDeserialize_Patch.Postfix(EscapePod.main);
            }
        }

        [HarmonyPatch(typeof(SaveLoadManager), "ClearSlotAsync")]
        internal class SaveLoadManager_ClearSlotAsync_Patch
        {
            public static void Postfix(SaveLoadManager __instance, string slotName)
            {
                //AddDebug("ClearSlotAsync " + slotName);
                config.escapePodSmokeOut.Remove(slotName);
                config.openedWreckDoors.Remove(slotName);
                config.Save();
            }
        }

        static void SaveData()
        {
            //Main.config.activeSlot = Inventory.main.quickSlots.activeSlot;
            if (Player.main.mode == Player.Mode.Normal)
                config.playerCamRot = MainCameraControl.main.viewModel.localRotation.eulerAngles.y;
            else
                config.playerCamRot = -1f;

            config.activeSlot = Inventory.main.quickSlots.activeSlot;
            //config.crushDepth -= Crush_Damage.extraCrushDepth;
            config.Save();
            //config.crushDepth += Crush_Damage.extraCrushDepth;
        }

        [QModPatch]
        public static void Load()
        {

            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            IngameMenuHandler.RegisterOnSaveEvent(SaveData);
            IngameMenuHandler.RegisterOnQuitEvent(CleanUp);


        }


        [QModPostPatch]
        public static void PostPatch()
        {
            //IQMod iqMod = QModServices.Main.FindModById("DayNightSpeed");
            //dayNightSpeedLoaded = iqMod != null;

            foreach (var item in config.crushDepthEquipment)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                //Log("crushDepthEquipment str " + item.Key);
                //Log("crushDepthEquipment TechType " + tt);
                if (tt != TechType.None)
                    Crush_Damage.crushDepthEquipment[tt] = item.Value;
            }
            foreach (var item in config.itemMass)
            {
                TechTypeExtensions.FromString(item.Key, out TechType tt, true);
                //Log("crushDepthEquipment str " + item.Key);
                //Log("crushDepthEquipment TechType " + tt);
                if (tt != TechType.None)
                    Pickupable_Patch.itemMass[tt] = item.Value;
            }
            foreach (string name in config.gravTrappable)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);

                if (tt != TechType.None)
                    Gravsphere_Patch.gravTrappable.Add(tt);
            }
        }
    }
}