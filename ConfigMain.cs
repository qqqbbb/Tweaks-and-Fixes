using BepInEx;
using Nautilus.Commands;
using Nautilus.Handlers;
using Nautilus.Json;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static ErrorMessage;
using static TechStringCache;
using static VehicleUpgradeConsoleInput;


namespace Tweaks_Fixes
{
    public class ConfigMain : JsonFile
    { // dont save enums, nautilus throws exc with certain mods 
        public ConfigMain()
        {
            this.Load();
        }

        public override string JsonFilePath => Paths.ConfigPath + Path.DirectorySeparatorChar + Main.MODNAME + Path.DirectorySeparatorChar + "config.json";
        public Screen_Resolution_Fix.ScreenRes screenRes;
        public Dictionary<string, HashSet<string>> cyclopsFloodLights = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> cyclopsLighting = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> exosuitLights = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> seaglideLights = new Dictionary<string, HashSet<string>>();
        public Dictionary<string, HashSet<string>> seaglideMap = new Dictionary<string, HashSet<string>>();
        public int activeSlot = -1;
        public HashSet<string> escapePodSmokeOut = new HashSet<string>();
        public Dictionary<string, Dictionary<string, int>> subThrottleIndex = new Dictionary<string, Dictionary<string, int>>();
        public Dictionary<string, HashSet<Vector3Int>> openedWreckDoors = new Dictionary<string, HashSet<Vector3Int>>();
        public Dictionary<string, Dictionary<string, HashSet<string>>> cyclopsDoors = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        public Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>> lockerNames = new Dictionary<string, Dictionary<string, Storage_Patch.SavedLabel>>();

        //public Dictionary<string, HashSet<string>> objectsSurvivedDespawn = new Dictionary<string, HashSet<string>> { };
        //public HashSet<string> objectsDespawned = new HashSet<string> { };
        //public List<string> removeLight = new List<string> { };
        //public List<string> biomesRemoveLight = new List<string> { };

        public Dictionary<string, float> lightIntensity = new Dictionary<string, float>();
        public Dictionary<string, float> hpToHeal = new Dictionary<string, float>();
        public HashSet<string> pickedUpFireExt = new HashSet<string>();
        public Dictionary<string, HashSet<string>> baseLights = new Dictionary<string, HashSet<string>>();

        internal int GetSubThrottleIndex(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (subThrottleIndex.ContainsKey(currentSlot) == false)
                return -1;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return -1;

            if (subThrottleIndex[currentSlot].ContainsKey(pi.id))
                return subThrottleIndex[currentSlot][pi.id];

            return -1;
        }

        internal void SaveSubThrottleIndex(GameObject go, int throttle)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (subThrottleIndex.ContainsKey(currentSlot) == false)
                subThrottleIndex[currentSlot] = new Dictionary<string, int>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                subThrottleIndex[currentSlot][pi.id] = throttle;
        }

        internal bool GetExosuitLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (exosuitLights.ContainsKey(currentSlot) == false)
                return false;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return false;

            return exosuitLights[currentSlot].Contains(pi.id);
        }

        internal bool GetSeaglideLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideLights.ContainsKey(currentSlot) == false)
                return false;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return false;

            return seaglideLights[currentSlot].Contains(pi.id);
        }

        internal bool GetSeaglideMap(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideMap.ContainsKey(currentSlot) == false)
                return true;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return true;

            return !seaglideMap[currentSlot].Contains(pi.id);
        }

        internal void SaveSeaglideMap(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideMap.ContainsKey(currentSlot) == false)
                seaglideMap[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                seaglideMap[currentSlot].Add(pi.id);
        }

        internal void DeleteSeaglideMap(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideMap.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    seaglideMap[currentSlot].Remove(pi.id);
            }
        }

        internal void SaveExosuitLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (exosuitLights.ContainsKey(currentSlot) == false)
                exosuitLights[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                exosuitLights[currentSlot].Add(pi.id);
        }

        internal void SaveSeaglideLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideLights.ContainsKey(currentSlot) == false)
                seaglideLights[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                seaglideLights[currentSlot].Add(pi.id);
        }

        internal void DeleteSeaglideLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (seaglideLights.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    seaglideLights[currentSlot].Remove(pi.id);
            }
        }

        internal void DeleteExosuitLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (exosuitLights.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    exosuitLights[currentSlot].Remove(pi.id);
            }
        }

        internal bool GetCyclopsFloodLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsFloodLights.ContainsKey(currentSlot) == false)
                return false;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return false;

            return cyclopsFloodLights[currentSlot].Contains(pi.id);
        }

        internal bool GetCyclopsLighting(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsLighting.ContainsKey(currentSlot) == false)
                return false;

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi == null)
                return false;

            return cyclopsLighting[currentSlot].Contains(pi.id);
        }

        internal void SaveCyclopsLighting(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsLighting.ContainsKey(currentSlot) == false)
                cyclopsLighting[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                cyclopsLighting[currentSlot].Add(pi.id);
        }

        internal void SaveCyclopsFloodLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsFloodLights.ContainsKey(currentSlot) == false)
                cyclopsFloodLights[currentSlot] = new HashSet<string>();

            PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
            if (pi)
                cyclopsFloodLights[currentSlot].Add(pi.id);
        }

        internal void DeleteCyclopsLighting(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsLighting.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    cyclopsLighting[currentSlot].Remove(pi.id);
            }
        }

        internal void DeleteCyclopsFloodLights(GameObject go)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsFloodLights.ContainsKey(currentSlot))
            {
                PrefabIdentifier pi = go.GetComponent<PrefabIdentifier>();
                if (pi)
                    cyclopsFloodLights[currentSlot].Remove(pi.id);
            }
        }

        internal bool GetCyclopsDoor(string prefabID, string name)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsDoors.ContainsKey(currentSlot) == false)
                return false;

            var saved = cyclopsDoors[SaveLoadManager.main.currentSlot];
            return saved.ContainsKey(prefabID) && saved[prefabID].Contains(name);
        }

        internal void SaveCyclopsDoor(string prefabID, string name)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsDoors.ContainsKey(currentSlot) == false)
                cyclopsDoors[currentSlot] = new Dictionary<string, HashSet<string>>();

            if (cyclopsDoors[currentSlot].ContainsKey(prefabID))
                cyclopsDoors[currentSlot][prefabID].Add(name);
            else
                cyclopsDoors[currentSlot][prefabID] = new HashSet<string>() { name };
        }

        internal void SaveWreckDoor(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (openedWreckDoors.ContainsKey(currentSlot) == false)
                openedWreckDoors[currentSlot] = new HashSet<Vector3Int>();

            openedWreckDoors[currentSlot].Add(Util.Vecto3ToVecto3int(pos));
        }

        internal bool IsWreckDoorSaved(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (openedWreckDoors.ContainsKey(currentSlot) == false)
                return false;

            return openedWreckDoors[currentSlot].Contains(Util.Vecto3ToVecto3int(pos));
        }

        internal void DeleteWreckDoor(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (openedWreckDoors.ContainsKey(currentSlot) == false)
                return;

            openedWreckDoors[currentSlot].Remove(Util.Vecto3ToVecto3int(pos));
        }

        internal void DeleteCyclopsDoor(string prefabID, string name)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (cyclopsDoors.ContainsKey(currentSlot) == false)
                return;

            if (cyclopsDoors[currentSlot].ContainsKey(prefabID) == false)
                return;

            cyclopsDoors[currentSlot][prefabID].Remove(name);
        }

        internal bool GetBaseLights(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot))
            {
                int x = (int)pos.x;
                int y = (int)pos.y;
                int z = (int)pos.z;
                string key = x + "_" + y + "_" + z;
                if (baseLights[currentSlot].Contains(key))
                    return false;
            }
            return true;
        }

        internal void SaveBaseLights(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot) == false)
                baseLights[currentSlot] = new HashSet<string>();

            int x = (int)pos.x;
            int y = (int)pos.y;
            int z = (int)pos.z;
            string key = x + "_" + y + "_" + z;
            baseLights[currentSlot].Add(key);
        }

        internal void DeleteBaseLights(Vector3 pos)
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (baseLights.ContainsKey(currentSlot) == false)
                return;

            int x = (int)pos.x;
            int y = (int)pos.y;
            int z = (int)pos.z;
            string key = x + "_" + y + "_" + z;
            baseLights[currentSlot].Remove(key);
        }

        internal float GetHPtoHeal()
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            if (hpToHeal.ContainsKey(currentSlot))
                return hpToHeal[currentSlot];

            return 0;
        }

        internal void SetHPtoHeal(float hp)
        {
            if (hp < 0)
                hp = 0;

            hpToHeal[SaveLoadManager.main.currentSlot] = hp;
        }

        internal void DeleteCurrentSaveSlotData()
        {
            string currentSlot = SaveLoadManager.main.currentSlot;
            cyclopsLighting.Remove(currentSlot);
            cyclopsFloodLights.Remove(currentSlot);
            openedWreckDoors.Remove(currentSlot);
            lockerNames.Remove(currentSlot);
            baseLights.Remove(currentSlot);
            cyclopsDoors.Remove(currentSlot);
            escapePodSmokeOut.Remove(currentSlot);
            pickedUpFireExt.Remove(currentSlot);
            hpToHeal.Remove(currentSlot);
            subThrottleIndex.Remove(currentSlot);
            seaglideMap.Remove(currentSlot);
            seaglideLights.Remove(currentSlot);
            exosuitLights.Remove(currentSlot);
            Save();
        }

        internal bool GetEscapePodSmoke()
        {
            return escapePodSmokeOut.Contains(SaveLoadManager.main.currentSlot);
        }

        internal bool SaveEscapePodSmoke()
        {
            return escapePodSmokeOut.Add(SaveLoadManager.main.currentSlot);
        }
        //public Dictionary<string, Dictionary<TechType, int>> deadCreatureLoot = new Dictionary<string, Dictionary<TechType, int>> { { "Stalker", new Dictionary<TechType, int> { { TechType.StalkerTooth, 2 } } }, { "Gasopod", new Dictionary<TechType, int> { { TechType.GasPod, 5 } } } };
        //public bool LEDLightWorksInHand = true;
    }

}