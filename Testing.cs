
using BepInEx;
using FMOD.Studio;
using HarmonyLib;
using Nautilus.Handlers;
using Nautilus.Options;
using Nautilus.Options.Attributes;
using Nautilus.Utility;
using rail;
using Story;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UWE;
using static ErrorMessage;


namespace Tweaks_Fixes
{
    public class Testing
    {// drillable ion cube 257 -1433 -333
        // geyser -50 -11 -430
        // sealed door 905 -195 613
        // spike plant 360 -106 98
        // databox -489 -500 1328
        // stones in caves 118 -60 127    -6 0 -13
        // repair panel 391 -14 -193
        // seaCrown 245 -217 255
        public static GameObject storedGO;
        public static PrefabIdentifier prefabIdentifier;

        static bool GetScanTarget(float distance, out GameObject result)
        {
            bool flag = false;
            Transform transform = MainCamera.camera.transform;
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            Ray ray = new Ray(position, forward);
            int layerMask = ~(1 << LayerID.OnlyVehicle);
            int numHits = UWE.Utils.RaycastIntoSharedBuffer(ray, distance, layerMask, QueryTriggerInteraction.Collide);
            //DebugTargetConsoleCommand.radius = -1f;
            RaycastHit resultHit = new RaycastHit();
            AddDebug("GetScanTarget numHits1 " + numHits);

            //if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, out resultHit))
            //    flag = true;
            for (int index1 = 0; index1 < numHits; ++index1)
            {
                RaycastHit hit = UWE.Utils.sharedHitBuffer[index1];
                Collider collider = hit.collider;
                if (collider == null)
                    continue;

                GameObject gameObject = collider.gameObject;
                Transform transform1 = collider.transform;
                if (gameObject == null || transform1 == null)
                    continue;

                int layer = gameObject.layer;
                //Transform transform2 = null;
                bool next = false;
                for (int index2 = 0; index2 < Targeting.ignoreList.Count; ++index2)
                {
                    Transform ignore = Targeting.ignoreList[index2];
                    if (transform1.IsAncestorOf(ignore))
                    {
                        //transform2 = ignore;
                        next = true;
                        break;
                    }
                }
                if (next)
                    continue;

                //if (transform2 == null)
                if (resultHit.collider == null || hit.distance < resultHit.distance)
                    resultHit = hit;

            }
            if (resultHit.collider != null)
            {
                GameObject go = Util.GetEntityRoot(resultHit.collider.gameObject);
                if (go)
                    AddDebug("GetScanTarget resultHit " + go.name);
            }


            //if (!flag)
            //{
            //    foreach (float radius in GameInput.IsPrimaryDeviceGamepad() ? Targeting.gamepadRadiuses : Targeting.standardRadiuses)
            //    {
            //        DebugTargetConsoleCommand.radius = radius;
            //        ray.origin = position + forward * radius;
            //        int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radius, distance, layerMask, queryTriggerInteraction);
            //        if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, out resultHit))
            //        {
            //            flag = true;
            //            break;
            //        }
            //    }
            //}
            Targeting.Reset();
            DebugTargetConsoleCommand.Stop();
            result = resultHit.collider != null ? resultHit.collider.gameObject : null;
            distance = resultHit.distance;
            return flag;
        }

        public static bool GetTarget(float maxDistance, out GameObject result, out float distance)
        {
            bool flag = false;
            Transform transform = MainCamera.camera.transform;
            Vector3 position = transform.position;
            Vector3 forward = transform.forward;
            Ray ray = new Ray(position, forward);
            int layerMask = ~(1 << LayerID.Trigger | 1 << LayerID.OnlyVehicle);
            QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide;
            int numHits1 = UWE.Utils.RaycastIntoSharedBuffer(ray, maxDistance, layerMask, queryTriggerInteraction);
            DebugTargetConsoleCommand.radius = -1f;
            RaycastHit resultHit;
            if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits1, out resultHit))
                flag = true;
            if (!flag)
            {
                foreach (float radius in GameInput.IsPrimaryDeviceGamepad() ? Targeting.gamepadRadiuses : Targeting.standardRadiuses)
                {
                    DebugTargetConsoleCommand.radius = radius;
                    ray.origin = position + forward * radius;
                    int numHits2 = UWE.Utils.SpherecastIntoSharedBuffer(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
                    if (Targeting.Filter(UWE.Utils.sharedHitBuffer, numHits2, out resultHit))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            Targeting.Reset();
            DebugTargetConsoleCommand.Stop();
            result = resultHit.collider != null ? resultHit.collider.gameObject : null;
            distance = resultHit.distance;
            return flag;
        }

        static void PrintBiomeNames()
        {
            AddDebug("RawBiomeName " + Util.GetRawBiomeName());
            AddDebug("Player biomeString " + Player.main.biomeString);
            //AddDebug("LargeWorld GetBiome " + LargeWorld.main.GetBiome(Player.main.transform.position));
            //AddDebug("GetRichPresence " + PlatformUtils.main.GetServices().GetRichPresence());
            string name = WaterBiomeManager.main.GetBiome(Player.main.transform.position);
            AddDebug($"WaterBiomeManager {name} ");
        }


        //[HarmonyPatch(typeof(DistanceCull), "Start")]
        class DistanceCull_Start_patch
        {
            public static void Postfix(DistanceCull __instance)
            {
                AddDebug($"DistanceCull.Start {__instance.name} parent {__instance.transform.parent.name}");
            }
        }

        //[HarmonyPatch(typeof(StoryGoal), "Trigger")]
        class StoryGoal_Trigger_patch
        {
            public static void Postfix(StoryGoal __instance)
            {
                AddDebug("StoryGoal Trigger " + __instance.key + " delay " + __instance.delay);
            }
        }

        //[HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Prefix_Patch
        {
            public static bool Prefix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                //AddDebug(target.name + " damage Prefix " + damage);
                damage *= DamageSystem.damageMultiplier;
                DamageModifier[] componentsInChildren = target.GetComponentsInChildren<DamageModifier>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    damage = componentsInChildren[i].ModifyDamage(damage, type);
                }
                bool player = target.GetComponent<Player>();
                Sealed @sealed = target.GetComponent<Sealed>();
                bool isSealed = @sealed != null && @sealed.IsSealed();
                switch (type)
                {
                    case DamageType.Heat:
                    case DamageType.Fire:
                        {
                            if ((bool)target.GetComponent<Living>() || player)
                            {
                                damage *= 2f;
                            }
                            HeatResistGene component2 = target.GetComponent<HeatResistGene>();
                            if ((bool)component2)
                            {
                                damage -= 0.75f * component2.Scalar * damage;
                            }
                            break;
                        }
                    case DamageType.Radiation:
                        {
                            if (!player)
                                break;

                            if ((bool)(Player.main.GetVehicle() as Exosuit))
                            {
                                damage = 0f;
                                break;
                            }
                            float num = damage;
                            AddDebug("CalculateDamage rad " + num);
                            if (Inventory.main.equipment.GetCount(TechType.RadiationSuit) > 0)
                            {
                                damage -= num * 0.5f;
                            }
                            if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                            {
                                damage -= num * 0.23f;
                            }
                            if (Inventory.main.equipment.GetCount(TechType.RadiationHelmet) > 0)
                            {
                                damage -= num * 0.23f;
                            }
                            AddDebug("CalculateDamage rad after " + damage);
                            break;
                        }
                    case DamageType.LaserCutter:
                        if (!isSealed)
                        {
                            damage *= 0.5f;
                        }
                        break;
                    case DamageType.Poison:
                        if (CreatureData.GetBehaviourType(target) == BehaviourType.SmallFish || CraftData.GetTechType(target) == TechType.Gasopod || CraftData.GetTechType(target) == TechType.MapRoomCamera || target.GetComponent<Vehicle>() != null)
                        {
                            damage = 0f;
                        }
                        break;
                    case DamageType.Acid:
                        if (DamageSystem.IsAcidImmune(target))
                        {
                            damage = 0f;
                        }
                        else if (target.GetComponent<Vehicle>() != null)
                        {
                            damage *= 0.05f;
                        }
                        break;
                    case DamageType.Collide:
                        if (dealer != null && dealer.GetComponent<Vehicle>() != null && target.GetComponentInParent<Base>() != null)
                        {
                            damage = 0f;
                        }
                        break;
                }
                if (isSealed && type != DamageType.LaserCutter)
                {
                    damage = 0f;
                }
                if (player && type != DamageType.Radiation && type != DamageType.Starve)
                {
                    float num2 = 0f;
                    if (Player.main.HasReinforcedSuit())
                    {
                        num2 += 0.4f;
                    }
                    if (Player.main.HasReinforcedGloves())
                    {
                        num2 += 0.12f;
                    }
                    damage -= damage * num2;
                }
                if ((bool)NoDamageConsoleCommand.main && NoDamageConsoleCommand.main.GetNoDamageCheat())
                {
                    damage = 0f;
                }
                if (DamageSystem.instagib && damage > 0f)
                {
                    LiveMixin component3 = target.GetComponent<LiveMixin>();
                    if ((bool)component3)
                    {
                        damage = component3.maxHealth * 100f;
                    }
                }
                __result = damage;
                return false;
            }
        }

        //[HarmonyPatch(typeof(LiveMixin), "TakeDamage")]   
        public static class LiveMixin_UpdateActiveTarget_Patch
        {
            public static void Prefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position, ref DamageType type, GameObject dealer)
            {
                if (__instance.name == "Cyclops-MainPrefab(Clone)")
                {
                    if (dealer == null)
                        AddDebug("cyclops TakeDamage dealer null");
                    else
                        AddDebug("cyclops TakeDamage " + dealer.name);

                }
            }
        }



        public static void SimulateKeyPress(Key k)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;
            AddDebug("SimulateKeyPress " + k);
            InputSystem.QueueStateEvent(keyboard,
                new KeyboardState(k));
            InputSystem.QueueStateEvent(keyboard,
                new KeyboardState());
        }

        //[HarmonyPatch(typeof(NotificationManager), "Add")]

        class NotificationManager_Add_Patch
        {
            static bool Prefix(NotificationManager __instance, NotificationManager.Group group, string key, float duration, float timeLeft)
            {
                AddDebug("NotificationManager Add " + key);
                //if (Input.GetKey(KeyCode.LeftShift))
                //{
                //    AddDebug("LeftShift");
                //    __instance.Select();
                //    return false;
                //}
                return false;
            }
        }

        //[HarmonyPatch(typeof(PrefabPlaceholder), "Spawn")]
        class PrefabPlaceholder_Spawn_Patch
        {
            static void Prefix(PrefabPlaceholder __instance)
            {
                if (WorldEntityDatabase.TryGetInfo(__instance.prefabClassId, out var info))
                    AddDebug("PrefabPlaceholder Spawn " + info.techType);
                //return true;
            }
        }

        //[HarmonyPatch(typeof(Player), "Update")]
        class Player_Update_Patch
        {
            static float deltaTime = 0.0f;
            static void Postfix(Player __instance)
            {
                if (!Main.gameLoaded)
                    return;

                //deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
                //float fps = 1.0f / deltaTime;
                //Util.Message("FPS " + Mathf.RoundToInt(fps));
                //AddDebug("mode " + __instance.mode);
                if (__instance.currentSub && __instance.currentSub is BaseRoot)
                {
                    BaseRoot baseRoot = __instance.currentSub as BaseRoot;
                    //AddDebug("Leak Amount " + baseRoot.GetLeakAmount());
                    //AddDebug("BaseFloodSim IsLeaking " + baseRoot.flood.tIsLeaking());
                    //AddDebug("currentSub IsLeaking " + __instance.currentSub.IsLeaking());
                    //AddDebug("leakers.Count " + baseRoot.flood.leakers.Count);
                    //AddDebug("IsLeaking " + __instance.currentSub.IsLeaking());
                }
                if (Keyboard.current.bKey.wasPressedThisFrame)
                {
                    //if (Player.main.IsInBase())
                    //    AddDebug("IsInBase");
                    //else if (Player.main.IsInSubmarine())
                    //    AddDebug("IsInSubmarine");
                    //else if (Player.main.inExosuit)
                    //    AddDebug("GetInMechMode");
                    //else if (Player.main.inSeamoth)
                    //    AddDebug("inSeamoth");
                    int x = Mathf.RoundToInt(Player.main.transform.position.x);
                    int y = Mathf.RoundToInt(Player.main.transform.position.y);
                    int z = Mathf.RoundToInt(Player.main.transform.position.z);
                    //AddDebug(x + " " + y + " " + z);

                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    //PrintBiomeNames();
                    ShowColliderName();
                    //if (Input.GetKey(KeyCode.LeftShift))
                    //    Time.timeScale = 0;
                    //else
                    //    Time.timeScale = 1;
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    ShowTargetInfo(true, false, false);
                }
                else if (Input.GetKeyDown(KeyCode.X))
                {
                    PrintClosestObjects(Player.mainObject.transform.position, 2f);
                }
                else if (Input.GetKeyDown(KeyCode.Z))
                {
                    //AddDebug("Light Scalar " + DayNightCycle.main.GetLocalLightScalar());
                    //GameObject goToTest = Player.main.guiHand.activeTarget;
                    //AddDebug("PDAScanner " + PDAScanner.complete.Contains(TechType.SeaglideFragment));
                    //AddDebug("KnownTech " + KnownTech.Contains(TechType.Seaglide));
                    //AddDebug("Exosuit " + BehaviourData.GetEcoTargetType(TechType.Exosuit));
                    //AddDebug("GetDepth " + Player.main.GetDepth());
                    //Vector3 vel = Player.main.currentMountedVehicle.useRigidbody.velocity;
                    //bool moving = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    //AddDebug("moving " + moving);

                    if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                    {
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                    {
                    }
                }
                else if (Input.GetKeyDown(KeyCode.F7))
                {
                    __instance.cinematicModeActive = !__instance.cinematicModeActive;
                }
            }
        }

        private static void PrintClosestObjects(Vector3 pos, float radius)
        {
            AddDebug($" Objects in radius of {radius}");
            Main.logger.LogInfo($"Objects in radius of {radius}");
            foreach (var go in Util.FindObjectsInRadius(pos, radius))
            {
                if (go.GetComponentInParent<Player>())
                    continue;

                AddDebug($"{go.name} ");
                Main.logger.LogInfo($"{go.name} ");
            }
        }

        public static void ShowColliderName(bool ignoreTriggers = true, bool show = true)
        {
            Transform scanTransform = MainCameraControl.main.transform;
            if (Physics.Raycast(scanTransform.position + scanTransform.forward, scanTransform.forward, out RaycastHit hit, float.MaxValue, -1, ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide))
            {
                var hitGameObject = hit.collider.gameObject;
                var parent = hitGameObject.transform.parent;
                var attachedRb = hit.collider.attachedRigidbody;
                var root = UWE.Utils.GetEntityRoot(hitGameObject);
                ErrorMessage.AddMessage($"Raycast hit collider of name '{hitGameObject.name}'");
                if (show && hit.collider.isTrigger == false)
                {
                    bool found = false;
                    foreach (Transform child in hit.collider.transform)
                    {
                        if (child.name == "Debug collider")
                        {
                            found = true;
                            //child.gameObject.SetActive(!child.gameObject.activeSelf);
                            UnityEngine.Object.Destroy(child.gameObject);
                        }
                    }
                    if (found == false)
                    {
                        Collider[] colliders = hit.collider.GetComponents<Collider>();
                        foreach (var collider in colliders)
                            ShowDebugCollider(collider);
                    }
                }
                if (parent != null)
                {
                    ErrorMessage.AddMessage($"Collider is direct child of '{parent.name}'");
                }
                if (attachedRb != null)
                {
                    ErrorMessage.AddMessage($"Collider is attached to the Rigidbody '{attachedRb.gameObject.name} tag {attachedRb.gameObject.tag}'");
                }
                if (root != null)
                {
                    ErrorMessage.AddMessage($"Entity root of this collider is '{root.name}'");
                }
            }
            else
            {
                ErrorMessage.AddMessage("Raycast failed.");
            }
        }

        private static void DumpEncy()
        {
            Main.logger.LogMessage("Dump ency");
            AddDebug("Dump ency");
            foreach (var kv in PDAEncyclopedia.mapping)
            {
                PDAEncyclopedia.EntryData data = kv.Value;
                Main.logger.LogMessage($"{kv.Key}  key: {data.key} path: {data.path} unlocked: {data.unlocked}");
                if (data.nodes != null && data.nodes.Length > 0)
                {
                    for (int i = 0; i < data.nodes.Length; i++)
                        Main.logger.LogMessage($"{kv.Key}   node {i} {data.nodes[i]}");
                }
            }
        }

        private static void DamageTarget(float amount)
        {
            GameObject target = Player.main.guiHand.activeTarget;
            if (!target)
                Targeting.GetTarget(Player.main.gameObject, 111f, out target, out float targetDist);

            if (!target)
                return;

            GameObject root = Util.GetEntityRoot(target);
            if (root)
                target = root;

            LiveMixin lm = target.GetComponent<LiveMixin>();
            if (lm)
            {
                AddDebug($"Damage {target.name} {amount}");
                lm.TakeDamage(amount);
            }
            else
                AddDebug($"{target.name} has no LiveMixin");
        }

        private static void DestroyTarget()
        {
            GameObject target = Player.main.guiHand.activeTarget;
            if (!target)
                Targeting.GetTarget(Player.main.gameObject, 111f, out target, out float targetDist);

            if (!target)
                return;

            GameObject root = Util.GetEntityRoot(target);
            if (root)
                target = root;

            AddDebug($"Destroy {target.name} ");
            UnityEngine.Object.Destroy(target);
        }

        static void ShowTerrainSurfaceType()
        {
            VFXSurfaceTypes vfxSurfaceTypes = VFXSurfaceTypes.none;
            int layerMask = 1 << LayerID.TerrainCollider | 1 << LayerID.Default;
            RaycastHit hitInfo;
            if (Physics.Raycast(MainCamera.camera.transform.position, MainCamera.camera.transform.forward, out hitInfo, 111f, layerMask) && hitInfo.collider.gameObject.layer == LayerID.TerrainCollider)
            {
                vfxSurfaceTypes = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                AddDebug("vfxSurfaceTypes " + vfxSurfaceTypes);
            }
            else
                AddDebug("no terrain ");
        }

        private static void RemoveHotMetalGlow(GameObject gameObject)
        {
            foreach (MeshRenderer mr in gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (Material m in mr.materials)
                {
                    //AddDebug(m.shader.name + " DisableKeyword UWE_WAVING");
                    m.DisableKeyword("MARMO_EMISSION");
                }
            }
        }

        public static void ShowTargetInfo(bool position = false, bool health = false, bool showCollider = false)
        {
            GameObject target = Player.main.guiHand.activeTarget;
            RaycastHit hitInfo = new RaycastHit();
            if (!target)
                //Util.GetTarget(Player.mainObject.transform.position, MainCamera.camera.transform.forward, 11f, out hitInfo);
                Targeting.GetTarget(Player.main.gameObject, 11f, out target, out float targetDist);
            //if (hitInfo.collider)
            //    target = hitInfo.collider.gameObject;

            if (!target)
                return;

            VFXSurfaceTypes vfxSurfaceType = Util.GetObjectSurfaceType(target);
            if (vfxSurfaceType != VFXSurfaceTypes.none)
                AddDebug("vfxSurfaceType  " + vfxSurfaceType);

            //AddDebug("collider  " + target.name);
            PrefabIdentifier pi = target.GetComponentInParent<PrefabIdentifier>();
            TechType techType = TechType.None;

            if (pi == null)
            {
                AddDebug("No PrefabIdentifier");
                Main.logger.LogMessage(target.name);
                target = GetRootGameobjectWithoutIdentifier(target);
            }
            else
            {
                target = pi.gameObject;
                techType = CraftData.GetTechType(target);
                Main.logger.LogDebug($"{pi.name} {techType} {pi.classId}");
            }
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                AddDebug("RemoveHotMetalGlow");
                RemoveHotMetalGlow(target);
            }
            AddDebug(target.name);

            if (position)
            {
                int x = (int)target.transform.position.x;
                int y = (int)target.transform.position.y;
                int z = (int)target.transform.position.z;
                AddDebug($"position {x} {y} {z}");
                Main.logger.LogMessage($"{target.name} position {target.transform.position}");
            }
            LODGroup lODGroup = target.GetComponentInChildren<LODGroup>();
            if (lODGroup != null)
            {
                AddDebug($"LOD count {lODGroup.lodCount} ");
            }
            EcoTarget ecoTarget = target.GetComponent<EcoTarget>();
            if (ecoTarget != null)
            {
                AddDebug("EcoTarget " + ecoTarget.type);
            }
            TerrainChunkPieceCollider tcpc = target.GetComponent<TerrainChunkPieceCollider>();
            if (tcpc)
            {
                vfxSurfaceType = Utils.GetTerrainSurfaceType(hitInfo.point, hitInfo.normal);
                AddDebug("Terrain vfxSurfaceType  " + vfxSurfaceType);
                return;
            }
            if (health)
            {
                LiveMixin lm = target.GetComponent<LiveMixin>();
                if (lm)
                    AddDebug("max HP " + lm.data.maxHealth + " HP " + (int)lm.health);
            }
            if (showCollider)
            {
                var colliders = target.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    if (collider.isTrigger == false)
                        ShowDebugCollider(collider);
                }
                //Debug(target);
            }
            LargeWorldEntity lwe = target.GetComponentInParent<LargeWorldEntity>();
            if (lwe)
                AddDebug(" cellLevel " + lwe.cellLevel);

            //AddDebug(target.name + " IsDecoPlant " + Util.IsDecoPlant(target));
            //if (target.transform.parent)
            //    AddDebug(target.transform.parent.name);
            FruitPlant fruitPlant = target.GetComponent<FruitPlant>();
            if (fruitPlant != null)
            {
                if (!fruitPlant.fruitSpawnEnabled)
                    AddDebug("fruitPlant fruit Spawn disabled ");

                PickPrefab[] pickPrefabs = target.GetComponentsInChildren<PickPrefab>(true);
                AddDebug($"fruitPlant SpawnInterval {fruitPlant.fruitSpawnInterval} pickPrefabs {pickPrefabs.Length}");
            }
            if (techType != TechType.None)
            {
                AddDebug("TechType  " + techType);
                TechType harvestOutput = TechData.GetHarvestOutput(techType);
                //if (harvestOutput != TechType.None)
                //{
                //    AddDebug("harvest_Output " + harvestOutput);
                //    HarvestType harvestType = TechData.GetHarvestType(techType);
                //    if (harvestType != HarvestType.None)
                //        AddDebug("harvest_Type " + harvestType);
                //}
            }
        }

        private static GameObject GetRootGameobjectWithoutIdentifier(GameObject go)
        {
            PrefabSpawn prefabSpawn = go.GetComponentInParent<PrefabSpawn>();
            if (prefabSpawn != null)
                return prefabSpawn.gameObject;

            Transform parent = go.transform.parent;
            if (parent == null)
                return go.gameObject;

            while (parent.parent != null)
                parent = parent.parent;

            return parent.gameObject;
        }

        private Vector3 ClipWithTerrain(GameObject go)
        {
            Vector3 origin = go.transform.position;
            //origin.y = go.transform.position.y + 5f;
            //RaycastHit hitInfo;
            //if (!Physics.Raycast(new Ray(origin, Vector3.down), out hitInfo, 10f, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
            //    return;
            //go.transform.position.y = Mathf.Max(go.transform.position.y, hitInfo.point.y + 0.3f);
            return origin;
        }

        void GetGO()
        {
            int numHits = UWE.Utils.SpherecastIntoSharedBuffer(Player.main.transform.position, 2f, Vector3.forward);
            AddDebug("num Hits " + numHits);
            AddDebug("sharedHitBuffer.Length " + UWE.Utils.sharedHitBuffer.Length);
            for (int index1 = 0; index1 < numHits; ++index1)
            {
                RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[index1];
                Vector3 point = raycastHit.point;
                AddDebug("raycastHit " + raycastHit.collider.gameObject.name);
                GameObject go = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
                if (go == null)
                    AddDebug("go == null " + raycastHit.collider.gameObject.name);
                else
                    AddDebug(go.name);
            }
        }

        //[HarmonyPatch(typeof(VoxelandGrassBuilder), "CreateUnityMeshes")]
        class VoxelandGrassBuilder_CreateUnityMeshes_Patch
        {
            static bool Prefix(VoxelandGrassBuilder __instance, IVoxelandChunk2 chunk, TerrainPoolManager terrainPoolManager)
            {
                for (int index = 0; index < __instance.builtMeshes.Count; ++index)
                {
                    TerrainChunkPiece grassObj = __instance.GetGrassObj(chunk, terrainPoolManager);
                    chunk.grassFilters.Add(grassObj.meshFilter);
                    chunk.grassRenders.Add(grassObj.meshRenderer);
                    chunk.chunkPieces.Add(grassObj);
                    MeshFilter grassFilter = chunk.grassFilters[index];
                    grassFilter.gameObject.SetActive(true);
                    MeshRenderer grassRender = chunk.grassRenders[index];
                    VoxelandBlockType type = __instance.types[index];
                    grassFilter.sharedMesh = terrainPoolManager.GetMeshForPiece(grassObj);
                    Material grassMaterial = type.grassMaterial;
                    grassRender.sharedMaterial = grassMaterial;
                    //Main.logger.LogDebug("material  " + grassRender.material.name + " VoxelandBlockType " + type.name + " grassMeshName " + type.grassMeshName + " layer " + type.layer + " filled " + type.filled);
                    //AddDebug("grassRender.material  " + grassRender.material.name);
                    //Main.logger.LogDebug("grassRender " + grassRender.material.name);
                    //coral_reef_grass_10_gr    coral_reef_grass_11_02_gr   coral_reef_grass_07_gr
                    //if (grassRender.material.name == "Coral_reef_red_seaweed_03 (Instance)" || grassRender.material.name == "Coral_reef_red_seaweed_01 (Instance)")
                    {
                        //AddDebug("!!!");
                        UWE.MeshBuffer builtMesh = __instance.builtMeshes[index];
                        builtMesh.Upload(grassFilter.sharedMesh);
                        builtMesh.Return();
                    }
                }
                __instance.state = VoxelandGrassBuilder.State.Init;
                return false;
            }
        }

        //[HarmonyPatch(typeof(FreezeRigidbodyWhenFar), "FixedUpdate")]
        class SubControl_Update_Patch
        {
            public static void Prefix(FreezeRigidbodyWhenFar __instance)
            {
                if (__instance.transform.position.y > __instance.freezeDist / 2.0)
                    return;
                if ((MainCamera.camera.transform.position - __instance.transform.position).sqrMagnitude > __instance.freezeDist * __instance.freezeDist)
                    AddDebug("FreezeRigidbodyWhenFar FixedUpdate ");
                else
                    AddDebug("FreezeRigidbodyWhenFar FixedUpdate !!!!!!!!!");

            }
        }

        //[HarmonyPatch(typeof(Story.StoryGoal), "Execute")]
        class StoryGoal_Execute_Patch
        {
            public static void Postfix(Story.StoryGoal __instance, string key, Story.GoalType goalType)
            {
                AddDebug("StoryGoal " + key);
                AddDebug("goalType " + goalType);
                //return false;
            }
        }

        //[HarmonyPatch(typeof(PDAEncyclopedia), "AddAndPlaySound")]
        class PDAEncyclopedia_AddAndPlaySound_Patch
        {
            public static void Postfix(string key, PDAEncyclopedia.EntryData __result)
            {
                AddDebug("AddAndPlaySound " + key);
                AddDebug("EntryData " + __result.key);
                //return false;
            }
        }

        [HarmonyPatch(typeof(FreecamController), "Update")]
        class FreecamController_Update_patch
        {
            public static void Prefix(FreecamController __instance)
            {
                if (__instance.GetActive() == false)
                    return;

                Vector2 scrollValue = Mouse.current.scroll.ReadValue();
                if (scrollValue.y != 0)
                {
                    if (scrollValue.y > 0)
                        __instance.speed *= 1.5f;
                    else
                        __instance.speed *= .375f;

                    if (__instance.speed < 1)
                        __instance.speed = 1;
                }
            }
        }

        static void Debug(GameObject go)
        {
            if (!go || go.name == "Debug")
                return;

            if (!go.transform.Find("Debug"))
            {
                GameObject debug = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debug.name = "Debug";
                debug.GetComponent<SphereCollider>().enabled = false;
                debug.transform.SetParent(go.transform, false);
                debug.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
                AddDebug("Creating debug sphere for: " + go.name);
                //debug.transform.localScale = Vector3.one * 0.1f;
            }
            //for (var i = 0; i < go.transform.childCount; ++i)
            //    Debug(go.transform.GetChild(i).gameObject);
        }

        //[HarmonyPatch(typeof(Targeting), "GetTarget", new Type[] { typeof(float), typeof(GameObject), typeof(float), typeof(Targeting.FilterRaycast) }, new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Normal })]
        class Targeting_GetTarget_PostfixPatch
        {
            public static void Postfix(ref GameObject result)
            {
                //AddDebug(" Targeting GetTarget  " + result.name);
            }
        }

        //[HarmonyPatch(typeof(CrafterLogic), "progress", MethodType.Getter)]
        class CrafterLogic_progress_Patch
        { // patch property getter
            public static bool Prefix(CrafterLogic __instance, ref float __result)
            {
                double timePassed = DayNightCycle.main.timePassed;
                double num1 = __instance.timeCraftingEnd - __instance.timeCraftingBegin;
                double timeCraftingBegin = __instance.timeCraftingBegin;
                double num2 = ((timePassed - timeCraftingBegin) / num1);
                __result = __instance.timeCraftingEnd <= __instance.timeCraftingBegin ? -1f : Mathf.Clamp01((float)num2);
                return false;
            }
        }

        //[HarmonyPatch(typeof(ExampleClass), MethodType.Constructor)]
        //[HarmonyPatch(new Type[] { typeof(int) })]
        //class ExampleClassConstructorPatch
        //{
        //    static void Postfix(int value)
        //    {
        //        Console.WriteLine($"Postfix: {value}");
        //    }
        //}

        //[HarmonyPatch(typeof(DamageSystem), nameof(DamageSystem.CalculateDamage))]
        public static class DamageSystem_CalculateDamage_Patch
        {
            //[HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Main.logger.LogInfo("CalculateDamage Instructions dump");
                var instructionList = new List<CodeInstruction>(instructions);
                for (int i = 0; i < instructionList.Count; i++)
                {
                    Main.logger.LogInfo($"IL_{i:D3}: {instructionList[i].opcode} {instructionList[i].operand}");
                }
                return instructionList;
            }
        }

        public static void ShowDebugCollider(Collider collider)
        {
            bool debugCol = false;
            foreach (Transform child in collider.transform)
            {
                if (child.name == "Debug collider")
                {
                    //child.gameObject.SetActive(!child.gameObject.activeSelf);
                    //UnityEngine.Object.Destroy(child.gameObject);
                    //debugCol = true;
                }
            }
            if (debugCol)
                return;

            CreateDebugCollider(collider);
            AddDebug("ShowDebugCollider " + collider.name);
        }

        public static void CreateDebugCollider(Collider collider)
        {
            PrimitiveType pt = PrimitiveType.Cube;
            if (collider is CapsuleCollider)
                pt = PrimitiveType.Capsule;
            else if (collider is SphereCollider)
                pt = PrimitiveType.Sphere;

            GameObject debugCollider = GameObject.CreatePrimitive(pt);
            debugCollider.name = "Debug collider";
            UnityEngine.Object.DestroyImmediate(debugCollider.GetComponent<Collider>());
            //debugCollider.GetComponent<MeshRenderer>().material.color = Color.white;
            //debugCollider.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f);
            Material unlitMaterial = new Material(Shader.Find("Unlit/Color"));
            unlitMaterial.color = new Color(0f, 0f, 1f);
            debugCollider.GetComponent<MeshRenderer>().material = unlitMaterial;
            debugCollider.SetActive(true);
            debugCollider.transform.SetParent(collider.transform, false);
            debugCollider.transform.localEulerAngles = Vector3.zero;
            MatchColliderSize(debugCollider, collider);
            //AddDebug("ShowDebugCollider " + collider.name);
        }

        internal static void MatchColliderSize(GameObject debugObj, Collider collider)
        {
            Transform debugTransform = debugObj.transform;
            debugTransform.localScale = Vector3.one;

            if (collider is BoxCollider box)
            {
                debugTransform.localScale = box.size;
                debugTransform.localPosition = box.center;
            }
            else if (collider is SphereCollider sphere)
            {
                float diameter = sphere.radius * 2;
                debugTransform.localScale = Vector3.one * diameter;
                debugTransform.localPosition = sphere.center;
            }
            else if (collider is CapsuleCollider capsule)
            {
                float height = capsule.height;
                float radius = capsule.radius;
                Vector3 scale = Vector3.one;

                switch (capsule.direction)
                {
                    case 0: // X-axis
                        scale = new Vector3(height, radius * 2, radius * 2);
                        break;
                    case 1: // Y-axis (most common)
                        scale = new Vector3(radius * 2, height, radius * 2);
                        break;
                    case 2: // Z-axis
                        scale = new Vector3(radius * 2, radius * 2, height);
                        break;
                }
                debugTransform.localScale = scale;
            }
            else if (collider is MeshCollider meshCollider)
            {
                if (meshCollider.sharedMesh != null)
                {
                    Bounds bounds = meshCollider.sharedMesh.bounds;
                    debugTransform.localScale = bounds.size;
                }
            }
        }

        //[HarmonyPatch(typeof(GotoConsoleCommand))]
        class GotoConsoleCommand_Patch
        {
            //[HarmonyPostfix, HarmonyPatch("Awake")]
            public static void AwakePostfix(GotoConsoleCommand __instance)
            {
                List<TeleportPosition> tps = new List<TeleportPosition>();
                foreach (TeleportPosition tp in __instance.data.locations)
                {
                    if (tp.name.StartsWith("escapepod"))
                    //if (tp.name.StartsWith("wreck"))
                    {
                        //Main.logger.LogMessage("scatter TeleportPosition: " + tp.name);
                        //AddDebug("GotoConsoleCommand TeleportPosition: " + tp.name);
                        tps.Add(tp);
                    }
                }
                __instance.data.locations = tps.ToArray();
            }
            //[HarmonyPostfix, HarmonyPatch("GotoLocation")]
            public static void GotoLocationPostfix(GotoConsoleCommand __instance)
            {

            }
        }


    }
}
