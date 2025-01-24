using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Nautilus.Handlers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public static class Util
    {
        public static bool spawning;

        public static bool GetTarget(Vector3 startPos, Vector3 dir, float distance, out RaycastHit hitInfo)
        {
            //return Physics.Raycast(startPos, dir, out hitInfo, distance, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore);
            return Physics.Raycast(startPos, dir, out hitInfo, distance);
        }


        public static void SetBloodColor(GameObject go)
        {
            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            //Main.logger.LogMessage("SetBloodColor " + go.name + " to " + Creature_Tweaks.bloodColor);
            foreach (ParticleSystem ps in pss)
            {
                ParticleSystem.MainModule psMain = ps.main;
                //Main.logger.LogMessage("startColor " + psMain.startColor.color);
                Color newColor = new Color(Creature_Tweaks.bloodColor.x, Creature_Tweaks.bloodColor.y, Creature_Tweaks.bloodColor.z, psMain.startColor.color.a);
                psMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
            }
        }

        public static IEnumerator SetParent(GameObject go, Transform parent, int framesToWait = 1)
        {
            while (framesToWait >= 0)
            {
                framesToWait--;
                yield return null;
                if (framesToWait == 0)
                {
                    go.transform.SetParent(parent);
                    AddDebug("SetParent ");
                }
            }
        }

        public static IEnumerator Cook(GameObject go)
        {
            TechType cookedData = CraftData.GetCookedData(CraftData.GetTechType(go.gameObject));
            //Main.logger.LogDebug("CookFish " + go.name + " cookedData " + cookedData);
            if (cookedData == TechType.None)
                yield break;

            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.InstantiateFromPrefabAsync(cookedData, (IOut<GameObject>)result);
            GameObject cooked = result.Get();
            cooked.transform.position = go.transform.position;
            cooked.transform.rotation = go.transform.rotation;
            Rigidbody rb = cooked.GetComponent<Rigidbody>();
            rb.mass = go.GetComponent<Rigidbody>().mass;
            rb.velocity = go.GetComponent<Rigidbody>().velocity;
            rb.angularDrag = 1;
            rb.drag = 1; // WorldForces.Start overwrites drag 
            UnityEngine.Object.Destroy(go);
        }

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

        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            Type type = ((object)original).GetType();
            Component component = destination.AddComponent(type);
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
                field.SetValue((object)component, field.GetValue((object)original));
            return component as T;
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

        public static bool CanPlayerEat()
        {
            if (GameModeUtils.IsOptionActive(GameModeOption.NoSurvival))
                return false;

            bool cantEat = ConfigMenu.cantEatUnderwater.Value && Player.main.isUnderwater.value;
            return !cantEat;
        }

        public static bool IsEquipped(TechType tt)
        {
            if (tt == TechType.None)
                return false;

            foreach (var kv in Inventory.main.equipment.equipment)
            {
                if (kv.Value == null)
                    continue;

                if (kv.Value._techType == tt)
                    return true;
            }
            return false;
        }

        public static bool IsOneHanded(PlayerTool playerTool)
        {
            if (playerTool is DiveReel)
                return true;

            if (playerTool is StasisRifle)
                return false;

            if (playerTool is PlaceTool)
                return true;

            if (playerTool is FireExtinguisher)
                return true;

            if (playerTool.GetComponent<Creature>())
                return true;

            return playerTool.hasBashAnimation;
        }

        public static bool IsDecoPlant(GameObject go)
        {
            if (go.GetComponent<Creature>())
                return false;

            if (go.GetComponent<Pickupable>())
                return false;

            if (go.GetComponent<SpawnOnKill>())
                return false;

            if (go.GetComponent<FruitPlant>())
                return false;

            if (!go.GetComponent<Rigidbody>())
                return false;

            if (!go.GetComponent<LiveMixin>())
                return false;

            if (go.GetComponent<Vehicle>())
                return false;

            if (go.GetComponent<EnergyMixin>())
                return false;

            if (go.GetComponent<PowerRelay>())
                return false;

            VFXSurfaceTypes surfaceType = GetObjectSurfaceType(go);
            //Main.logger.LogMessage(go.name + " IsDecoPlant surfaceType " + surfaceType);
            if (surfaceType == VFXSurfaceTypes.none || surfaceType == VFXSurfaceTypes.vegetation || surfaceType == VFXSurfaceTypes.fallback || surfaceType == VFXSurfaceTypes.coral)
                return true;

            return false;
        }

        public static void FreezeObject(GameObject go, bool state)
        {
            WorldForces wf = go.GetComponent<WorldForces>();
            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (wf && rb)
            {
                wf.enabled = !state;
                rb.isKinematic = state;
            }
        }

        public static Bounds GetAABB(GameObject go)
        {
            FixedBounds fb = go.GetComponent<FixedBounds>();
            Bounds bounds = fb == null ? UWE.Utils.GetEncapsulatedAABB(go) : fb.bounds;
            return bounds;
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

        public static bool IsDead(GameObject go)
        {
            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            return liveMixin && !liveMixin.IsAlive();
        }

        public static bool IsDestroyed(GameObject gameObject)
        {
            // UnityEngine overloads the == opeator for the GameObject type
            // and returns null when the object has been destroyed, but 
            // actually the object is still there but has not been cleaned up yet
            // if we test both we can determine if the object has been destroyed.
            return gameObject == null && !ReferenceEquals(gameObject, null);
        }

        public static bool IsDestroyed(Component component)
        {
            return component == null && !ReferenceEquals(component, null);
        }

        public static bool IsEatableFish(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            if (creature == null)
                return false;

            return go.GetComponent<Eatable>();
        }

        public static bool IsCreatureAlive(GameObject go)
        {
            Creature creature = go.GetComponent<Creature>();
            if (creature == null)
                return false;

            LiveMixin liveMixin = go.GetComponent<LiveMixin>();
            if (liveMixin == null)
                return false;

            return liveMixin.IsAlive();
        }

        public static void MakeEatable(GameObject go, float food)
        {
            Eatable eatable = go.EnsureComponent<Eatable>();
            eatable.foodValue = food;
            eatable.despawns = IsEatableFish(go);
        }

        public static void MakeDrinkable(GameObject go, float water)
        {
            Eatable eatable = go.EnsureComponent<Eatable>();
            eatable.waterValue = water;
            eatable.despawns = IsEatableFish(go);
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

        static bool CloseToPosition(Vector3 pos1, Vector3 pos2, float range) => (pos1 - pos2).sqrMagnitude < range * range;

        public static void FindObjectClosestToPlayer(float dist)
        {
            Transform[] ts = UnityEngine.Object.FindObjectsOfType<Transform>();
            AddDebug("Transforms found " + ts.Length);
            List<GameObject> list = new List<GameObject>();
            foreach (Transform t in ts)
            {
                if (t.GetComponentInParent<Player>())
                    continue;

                Vector3 dir = t.transform.position - Player.mainObject.transform.position;
                if (dir.magnitude < dist)
                    list.Add(t.gameObject);
            }
            foreach (GameObject go in list)
            {
                AddDebug(" " + go.name);
                Main.logger.LogInfo("FindObjectClosestToPlayer " + go.name);
            }
        }

        public static void EnsureFruits(GameObject go)
        {
            //AddDebug("EnsureFruits " + go.name);
            PickPrefab[] pickPrefabs = go.GetComponentsInChildren<PickPrefab>(true);
            if (pickPrefabs.Length == 0)
                return;

            FruitPlant fp = go.EnsureComponent<FruitPlant>();
            fp.fruitSpawnEnabled = true;
            //AddDebug(__instance.name + " fruitSpawnInterval orig " + fp.fruitSpawnInterval);
            fp.fruitSpawnInterval = ConfigMenu.fruitGrowTime.Value * DayNightCycle.kDayLengthSeconds;
            if (fp.fruitSpawnInterval == 0f)
                fp.fruitSpawnInterval = 1f;
            //AddDebug(__instance.name + " fruitSpawnInterval after " + fp.fruitSpawnInterval);
            fp.fruits = pickPrefabs;
        }

        public static VFXSurfaceTypes GetObjectSurfaceType(GameObject obj)
        {
            VFXSurfaceTypes result = VFXSurfaceTypes.none;
            if (obj)
            {
                VFXSurface vfxSurface = obj.GetComponent<VFXSurface>();
                if (vfxSurface)
                {
                    result = vfxSurface.surfaceType;
                    //AddDebug(" VFXSurface " + component.name);
                    //AddDebug(" VFXSurface parent " + component.transform.parent.name);
                    //AddDebug(" VFXSurface parent parent " + component.transform.parent.parent.name);
                }
                else
                    vfxSurface = obj.FindAncestor<VFXSurface>();

                if (vfxSurface)
                    result = vfxSurface.surfaceType;
            }
            return result;
        }

        static IEnumerator PrintMass(TechType techType)
        {
            CoroutineTask<GameObject> request = CraftData.GetPrefabForTechTypeAsync(techType, false);
            yield return request;
            GameObject go = request.GetResult();
            if (go)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb)
                {
                    string name = Language.main.Get(techType);
                    string s = techType + ", " + name + ", mass " + rb.mass;
                    //massList.Add(s);
                }
            }
        }

        public static GameObject GetEntityRoot(GameObject go)
        {
            UniqueIdentifier prefabIdentifier = go.GetComponent<UniqueIdentifier>();
            if (prefabIdentifier == null)
                prefabIdentifier = go.GetComponentInParent<UniqueIdentifier>();
            return prefabIdentifier != null ? prefabIdentifier.gameObject : null;
        }

        public static IEnumerable<GameObject> FindAllRootGameObjects()
        {
            return Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t.parent == null)
                .Select(x => x.gameObject);
        }

        public static bool Approximately(float a, float b, float tolerance = 0.00001f)
        { // Mathf.Approximately does not work when compare to 0
            return (Mathf.Abs(a - b) < tolerance);
        }

        public static void AddVFXsurfaceComponent(GameObject go, VFXSurfaceTypes type)
        {
            VFXSurface vFXSurface = go.EnsureComponent<VFXSurface>();
            vFXSurface.surfaceType = type;
        }

        public static IEnumerator Spawn(TechType techType, Vector3 pos = default, bool fadeIn = false)
        {
            //AddDebug("Spawn " + techType);
            GameObject prefab;
            TaskResult<GameObject> result = new TaskResult<GameObject>();
            yield return CraftData.GetPrefabForTechTypeAsync(techType, false, result);
            prefab = result.Get();
            if (!fadeIn)
                spawning = true;

            GameObject go = prefab == null ? Utils.CreateGenericLoot(techType) : Utils.SpawnFromPrefab(prefab, null);
            if (go != null)
            {
                if (pos == default)
                {
                    Transform camTr = MainCamera.camera.transform;
                    go.transform.position = camTr.position + camTr.forward * 3f;
                }
                go.transform.position = pos;
                //AddDebug("Spawn " + techType + " " + pos);
                CrafterLogic.NotifyCraftEnd(go, techType);
            }
            spawning = false;
        }

        public static float CelciusToFahrenhiet(float celcius)
        {
            return celcius * 1.8f + 32f;
        }

        public static bool IsGraphicsPresetHighDetail()
        {
            return GraphicsPreset.GetPresets()[QualitySettings.GetQualityLevel()].detail == 2;
        }

        public static string GetRawBiomeName()
        {
            AtmosphereDirector atmosphereDirector = AtmosphereDirector.main;
            if (atmosphereDirector)
            {
                string biomeOverride = atmosphereDirector.GetBiomeOverride();
                if (!string.IsNullOrEmpty(biomeOverride))
                    return biomeOverride;
            }
            LargeWorld largeWorld = LargeWorld.main;
            return largeWorld && Player.main ? largeWorld.GetBiome(Player.main.transform.position) : "<unknown>";
        }


    }
}