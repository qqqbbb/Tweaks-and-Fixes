
using HarmonyLib;
using QModManager.API.ModLoading;
using QModManager.API;
using System.Reflection;
using System;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
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
        public static GUIHand guiHand;
        public static PDA pda;
        public static Survival survival;
        //public static bool crafterOpen = false;
        public static bool canBreathe = false;
        public static bool loadingDone = false;
        public static bool english = false;
        public static System.Random rndm = new System.Random();
        public static bool advancedInventoryLoaded = false;
        public static bool flareRepairLoaded = false;
        public static bool cyclopsDockingLoaded = false;
        public static bool vehicleLightsImprovedLoaded = false;

        public static Config config { get; } = OptionsPanelHandler.RegisterModOptions<Config>();

        public static Component CopyComponent(Component original, GameObject destination)
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy;
        }

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

        static IEnumerator PlayClip(Animator animator, string name, float delay = 0f)
        {
            AddDebug("PlayClip start " + delay);
            yield return new WaitForSeconds(delay);
            AddDebug("PlayClip " + name);
            animator.Play(name);
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

        public static T[] FindObjectsOfType<T>() where T : Component
        {
            return GameObject.FindObjectOfType(typeof(T)) as T[];
        }

        public static ItemsContainer GetOpenContainer()
        {
            int storageCount = Inventory.main.usedStorage.Count;
            if (storageCount > 0)
            {
                IItemsContainer itemsContainer = Inventory.main.usedStorage[storageCount - 1];
                if (itemsContainer is ItemsContainer)
                    return itemsContainer as ItemsContainer;
            }
            return null;
        }

        public static void CleanUp()
        {
            loadingDone = false;
            canBreathe = false;
            QuickSlots_Patch.invChanged = true;
            Databox_Light_Patch.databoxLights = new List<GameObject>();
            Crush_Damage.extraCrushDepth = 0;
            Cyclops_Patch.ceh = null;
            Cyclops_Patch.collidersInSub = new HashSet<Collider>();
            Gravsphere_Patch.gasPods = new HashSet<GasPod>();
            Gravsphere_Patch.gravSphereFish = new HashSet<Pickupable>();
            Decoy_Patch.decoysToDestroy = new List<GameObject>();
            Vehicle_patch.currentLights = new Light[2];
            Vehicle_patch.dockedVehicles = new Dictionary<Vehicle, Vehicle.DockType>();
            Exosuit_Patch.exosuitStarted = false;
            Damage_Patch.healTempDamageTime = 0;
            Damage_Patch.tempDamageLMs = new List<LiveMixin>();
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
                    LanguageHandler.SetTechTypeTooltip(TechType.SeamothElectricalDefense, "Generates a localized electric field designed to ward off aggressive fauna. Press and hold the button to charge the shot.");
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
                //AddDebug(" uGUI_SceneLoading done");
                loadingDone = true;
                foreach (LiveMixin lm in Damage_Patch.tempDamageLMs)
                {
                    if (lm.tempDamage > 0)
                    {
                        //AddDebug("uGUI_SceneLoading End " + lm.tempDamage);
                        lm.SyncUpdatingState();
                    }
                }
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

        [HarmonyPatch(typeof(SaveLoadManager))]
        internal class SaveLoadManager_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch( "ClearSlotAsync")]
            public static void ClearSlotAsyncPostfix(SaveLoadManager __instance, string slotName)
            {
                //AddDebug("ClearSlotAsync " + slotName);
                config.escapePodSmokeOut.Remove(slotName);
                config.openedWreckDoors.Remove(slotName);
                config.lockerNames.Remove(slotName);
                config.baseLights.Remove(slotName);
                config.Save();
            }

            [HarmonyPostfix]
            [HarmonyPatch("CreateSlotAsync", new Type[0])]
            public static void CreateSlotAsyncPostfix(SaveLoadManager __instance)
            {
                //AddDebug("SaveLoadManager CreateSlotAsync ");
                config.pickedUpFireExt = false;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("LoadSlotsAsync", new Type[0])]
            public static void LoadSlotsAsyncPostfix(SaveLoadManager __instance)
            {

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

        [HarmonyPatch(typeof(WorldForcesManager), "FixedUpdate")]
        class WorldForcesManager_Patch
        {
            static bool Prefix(WorldForcesManager __instance)
            { // without this WorldForcesManager.FixedUpdate gives NRE when game loads
                if (!loadingDone)
                    return false;

                return true;
            }
        }

        [QModPatch]
        public static void Load()
        {
            config.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
            new Harmony($"qqqbbb_{assembly.GetName().Name}").PatchAll(assembly);
            IngameMenuHandler.RegisterOnSaveEvent(SaveData);
            IngameMenuHandler.RegisterOnQuitEvent(CleanUp);
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(18.71f, -26.35f, -155.85f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(348.3f, -25.3f, -205.1f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-637f, -110.5f, -49.2f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-185f, -42f, 138.5f)));
            //CoordinatedSpawnsHandler.Main.RegisterCoordinatedSpawn(new SpawnInfo(TechType.Beacon, new Vector3(-63.85f, -16f, -223f)));
            new Spawnables.Stone().Patch();
        }

        [QModPostPatch]
        public static void PostPatch()
        {
            //IQMod iqMod = QModServices.Main.FindModById("DayNightSpeed");
            //dayNightSpeedLoaded = iqMod != null;
            advancedInventoryLoaded = QModServices.Main.ModPresent("AdvancedInventory");
            flareRepairLoaded = QModServices.Main.ModPresent("Rm_FlareRepair");
            cyclopsDockingLoaded = QModServices.Main.ModPresent("CyclopsDockingMod");
            vehicleLightsImprovedLoaded = QModServices.Main.ModPresent("Rm_VehicleLightsImproved");
            //Main.Log("vehicleLightsImprovedLoaded " + vehicleLightsImprovedLoaded);
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
                    LargeWorldEntity_Patch.removeLight.Add(tt);
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
