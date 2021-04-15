
using HarmonyLib;
using QModManager.API.ModLoading;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Tweaks_Fixes
{
    [QModCore]
    public class Main
    {
        public static GUIHand gUIHand;
        public static PDA pda;


        internal static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

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
            //gameLoaded = false;

            QuickSlots_Patch.invChanged = true;
            Databox_Light_Patch.databoxLights = new List<GameObject>();
        }

        public static void Message(string str)
        {
            int count = ErrorMessage.main.messages.Count;

            if (count == 0)
            {
                ErrorMessage.AddDebug(str);
            }
            else
            {
                ErrorMessage._Message message = ErrorMessage.main.messages[ErrorMessage.main.messages.Count - 1];
                message.messageText = str;
                message.entry.text = str;
            }
        }

        public static void Log(string str, QModManager.Utility.Logger.Level lvl = QModManager.Utility.Logger.Level.Info)
        {
            QModManager.Utility.Logger.Log(lvl, str);
        }

        [HarmonyPatch(typeof(IngameMenu), "QuitGameAsync")]
        internal class IngameMenu_QuitGameAsync_Patch
        {
            public static void Postfix(IngameMenu __instance, bool quitToDesktop)
            {
                //ErrorMessage.AddDebug("QuitGameAsync " + quitToDesktop);
                if (!quitToDesktop)
                    CleanUp();
            }
        }

        [HarmonyPatch(typeof(Player), "Start")]
        class Player_Start_Patch
        {
            static void Postfix(Player __instance)
            {
                //IngameMenuHandler.RegisterOnSaveEvent(config.Save);
                gUIHand = Player.main.GetComponent<GUIHand>();
                pda = Player.main.GetPDA();
                if (config.cantScanExosuitClawArm)
                    DisableExosuitClawArmScan();


            }
        }

        //[HarmonyPatch(typeof(uGUI_MainMenu), "StartNewGame")]
        internal class Initialize_NewGame_Patch
        {
            public static void Postfix(uGUI_MainMenu __instance)
            {
                //ErrorMessage.AddDebug("StartNewGame ");
                //config.escapePodSmokeOut = false;
                //config.openedWreckDoors = new Dictionary<string, Dictionary<int, bool>>();
                //config.Save();
            }
        }

        [HarmonyPatch(typeof(SaveLoadManager), "ClearSlotAsync")]
        internal class SaveLoadManager_ClearSlotAsync_Patch
        {
            public static void Postfix(SaveLoadManager __instance, string slotName)
            {
                //ErrorMessage.AddDebug("ClearSlotAsync " + slotName);
                config.escapePodSmokeOut.Remove(slotName);
                config.openedWreckDoors.Remove(slotName);
                config.Save();
            }
        }

        [QModPatch]
        public static void Load()
        {
            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
        }
    }
}