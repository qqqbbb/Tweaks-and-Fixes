
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

// crash home to fix 272.3 -41.9 -199.8
namespace Tweaks_Fixes
{
    [QModCore]
    public class Main
    {
        public static GUIHand guiHand;
        public static PDA pda;
        public static Survival survival;
        public static bool crafterOpen = false;
        public static bool canBreathe = false;
        public static bool loadingDone = false;
        public static bool english = false;
        public static System.Random rndm = new System.Random();
        public static bool advancedInventoryLoaded = false;
        public static bool flareRepairLoaded = false;
        public static bool cyclopsDockingLoaded = false;

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

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

        public static void DropItems(ItemsContainer container)
        {
            List<Pickupable> pickList = new List<Pickupable>();
            Dictionary<TechType, ItemsContainer.ItemGroup>.Enumerator enumerator = container._items.GetEnumerator();
            while (enumerator.MoveNext())
            {
                List<InventoryItem> items = enumerator.Current.Value.items;
                for (int index = 0; index < items.Count; ++index)
                    pickList.Add(items[index].item);
            }
            foreach (Pickupable p in pickList)
            {
                //AddDebug("Drop  " + p.GetTechName());
                p.Drop();
            }
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
            Cyclops_Patch.collidersInSub = new HashSet<Collider>();
            Gravsphere_Patch.gasPods = new HashSet<GasPod>();
            Gravsphere_Patch.gravSphereFish = new HashSet<Pickupable>();
            //Coffee_Patch.DeleteCoffee();
            Decoy_Patch.decoysToDestroy = new List<GameObject>();
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
                    english = true;
                    //AddDebug("English");
                    //LanguageHandler.SetLanguageLine("Tooltip_Bladderfish", "Unique outer membrane has potential as a natural water filter. Can also be used as a source of oxygen.");
                    LanguageHandler.SetTechTypeTooltip(TechType.Bladderfish, "Unique outer membrane has potential as a natural water filter. Provides some oxygen when consumed raw.");
                }
            }
        }

        [HarmonyPatch(typeof(uGUI_SceneLoading), "End")]
        internal class uGUI_SceneLoading_End_Patch
        { // fires after game loads
            public static void Postfix(uGUI_SceneLoading __instance)
            {
                //if (uGUI.main.loading.isLoading)
                //{
                //    AddDebug(" is Loading");
                //    return;
                //}
                if (!uGUI.main.hud.active)
                {
                    //AddDebug(" hud not active");
                    return;
                }
                //AddDebug(" uGUI_SceneLoading end");
                loadingDone = true;
                //if (Cyclops_Patch.cyclopsHelmHUDManager)
                //{
                //    if (Cyclops_Patch.cyclopsHelmHUDManager.LOD.IsFull() && Player.main.currentSub != Cyclops_Patch.cyclopsHelmHUDManager.subRoot && !Cyclops_Patch.cyclopsHelmHUDManager.subRoot.subDestroyed)
                //    {
                //        Cyclops_Patch.cyclopsHelmHUDManager.canvasGroup.alpha = 0f;
                //    }
                //}
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

        [HarmonyPatch(typeof(IngameMenu), "SaveGame")]
        internal class IngameMenu_SaveGame_Patch
        {
            public static void Prefix(IngameMenu __instance)
            {
                for (int i = Decoy_Patch.decoysToDestroy.Count - 1; i >= 0; i--)
                    UnityEngine.Object.Destroy(Decoy_Patch.decoysToDestroy[i]);
                //AddDebug("decoysToDestroy.Count " + Decoy_Patch.decoysToDestroy.Count);
            }
        }

        static void SaveData()
        {
            //AddDebug("SaveData ");
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
            advancedInventoryLoaded = QModServices.Main.ModPresent("AdvancedInventory");
            flareRepairLoaded = QModServices.Main.ModPresent("Rm_FlareRepair");
            cyclopsDockingLoaded = QModServices.Main.ModPresent("CyclopsDockingMod");

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
            foreach (string name in config.silentCreatures)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                if (tt != TechType.None)
                    Creature_Tweaks.silentCreatures.Add(tt);
            }
            foreach (string name in config.stalkerPlayThings)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                if (tt != TechType.None)
                    Pickupable_Patch.shinies.Add(tt);
            }
            foreach (string name in config.removeLight)
            {
                TechTypeExtensions.FromString(name, out TechType tt, true);
                //Log("config.removeLight " + tt);
                if (tt != TechType.None)
                    Plant_Patch.removeLight.Add(tt);
            }
            foreach (var kv in config.damageMult_)
            {
                TechTypeExtensions.FromString(kv.Key, out TechType tt, true);
                if (tt != TechType.None)
                   Damage_Patch.damageMult.Add(tt, kv.Value);
            }
        }
    }
}