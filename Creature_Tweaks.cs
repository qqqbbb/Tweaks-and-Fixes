using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using static ErrorMessage;
using System.Runtime.CompilerServices;

namespace Tweaks_Fixes
{
    class Creature_Tweaks
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };
        public static HashSet<GameObject> pickupShinies = new HashSet<GameObject>();
        public static ConditionalWeakTable<GameObject, Rigidbody> objectsRBs = new ConditionalWeakTable<GameObject, Rigidbody>();
        public static HashSet<TechType> notRespawningCreatures;
        public static HashSet<TechType> notRespawningCreaturesIfKilledByPlayer;
        public static Dictionary<TechType, int> respawnTime = new Dictionary<TechType, int>();

        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                if (Main.config.CreatureFleeChance == 100 && !Main.config.creatureFleeChanceBasedOnHealth && Main.config.creatureFleeUseDamageThreshold)
                    return true;
                
                if (!__instance.enabled)
                    return false;

                float damage = damageInfo.damage;
                bool doFlee = false;
                LiveMixin liveMixin = __instance.creature.liveMixin;
                if (Main.config.creatureFleeChanceBasedOnHealth && liveMixin && liveMixin.IsAlive())
                {
                    int maxHealth = Mathf.RoundToInt(liveMixin.maxHealth);
                    int rnd1 = Main.rndm.Next(0, maxHealth+1);
                    int health = Mathf.RoundToInt(liveMixin.health);
                    //if (__instance.gameObject == Testing.goToTest)
                        //AddDebug(__instance.name + " max Health " + maxHealth + " Health " + health);
                    if (health < rnd1)
                    {
                        //if (__instance.gameObject == Testing.goToTest)
                        //    AddDebug(__instance.name + " health low ");

                        doFlee = true;
                    }
                }
                else
                {
                    if (damageInfo.type == DamageType.Electrical)
                        damage *= 35f;
                    __instance.accumulatedDamage += damage;
                    //if (__instance.gameObject == Testing.goToTest)
                    //    AddDebug(__instance.name + " accumulatedDamage " + __instance.accumulatedDamage + " damageThreshold " + __instance.damageThreshold);

                    __instance.lastDamagePosition = damageInfo.position;
                    if (Main.config.creatureFleeUseDamageThreshold && __instance.accumulatedDamage <= __instance.damageThreshold)
                        return false;

                    int rnd = Main.rndm.Next(1, 101);
                    if (Main.config.CreatureFleeChance >= rnd)
                        doFlee = true;
                }
                if (doFlee)
                {
                    //if (__instance.gameObject == Testing.goToTest)
                    //    AddDebug(__instance.name + " Flee " + __instance.fleeDuration);

                    __instance.timeToFlee = Time.time + __instance.fleeDuration;
                    __instance.creature.Scared.Add(1f);
                    __instance.creature.TryStartAction((CreatureAction)__instance);
                }
                return false;
            }
             
            public static void Postfix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                if (collectShiny)
                {
                    //AddDebug("Stalker DropShinyTarget");
                    collectShiny.DropShinyTarget();
                }
            }
        }
        
        [HarmonyPatch(typeof(Stalker))]
        public static class Stalker_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("CheckLoseTooth")]
            public static bool CheckLoseToothPrefix(Stalker __instance, GameObject target)
            { // only scrap metal has HardnessMixin  0.5
                float rndm = UnityEngine.Random.value;
                float stalkerLoseTooth = Main.config.stalkerLoseToothChance * .01f;
                if (stalkerLoseTooth >= rndm && HardnessMixin.GetHardness(target) > rndm)
                    __instance.LoseTooth();

                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("LoseTooth")]
            public static bool LoseToothPrefix(Stalker __instance, ref bool __result)
            {
                if (ConfigToEdit.stalkerLooseToothSound.Value)
                    return true;

                GameObject go = UnityEngine.Object.Instantiate(__instance.toothPrefab);
                go.transform.position = __instance.loseToothDropLocation.transform.position;
                go.transform.rotation = __instance.loseToothDropLocation.transform.rotation;
                if (go.activeSelf && __instance.isActiveAndEnabled)
                {
                    foreach (Collider componentsInChild in go.GetComponentsInChildren<Collider>())
                        Physics.IgnoreCollision(__instance.stalkerBodyCollider, componentsInChild);
                }
                //Utils.PlayFMODAsset(this.loseToothSound, go.transform);
                LargeWorldEntity.Register(go);
                __result = true;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(Creature))]
        public static class Creature_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(Creature __instance)
            {               
                VFXSurface vFXSurface = __instance.gameObject.EnsureComponent<VFXSurface>();
                vFXSurface.surfaceType = VFXSurfaceTypes.organic;
                if (__instance is Spadefish || __instance is Jumper)
                { // make them not damage seamoth
                    //AddDebug("Spadefish");
                    __instance.GetComponent<Rigidbody>().mass = 4f;
                }
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    //AddDebug("silent " + tt);
                    foreach (FMOD_StudioEventEmitter componentsInChild in __instance.GetComponentsInChildren<FMOD_StudioEventEmitter>())
                        //Object.Destroy(componentsInChild); // crashfish does not attack, NRE for crabsnake
                        componentsInChild.enabled = false; // does not work for crashfish, sandshark
                    foreach (FMOD_CustomEmitter componentsInChild in __instance.GetComponentsInChildren<FMOD_CustomEmitter>())
                        //Object.Destroy(componentsInChild);
                        componentsInChild.enabled = false;
                }
                if (tt == TechType.Gasopod)
                {
                    GasoPod gasoPod = __instance as GasoPod;
                    if (gasoPod)
                    {
                        Rigidbody rb = __instance.GetComponent<Rigidbody>();
                        if (rb)
                        {
                            objectsRBs.Add(__instance.gameObject, rb);
                            //AddDebug("objectsRBs save gasopod ");
                        }
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("IsInFieldOfView")]
            public static bool GetCanSeeObjectPrefix(Creature __instance, GameObject go, ref bool __result)
            { // ray does not hit terrain if cast from underneath. Cast from player to avoid it.
                __result = false;
                if (go == null)
                    return false;

                Vector3 vector3 = go.transform.position - __instance.transform.position;
                Vector3 rhs = __instance.eyesOnTop ? __instance.transform.up : __instance.transform.forward;
                if (Mathf.Approximately(__instance.eyeFOV, -1f) || Vector3.Dot(vector3.normalized, rhs) >= __instance.eyeFOV)
                {
                    bool noLoS = false;
                    if (__instance.techTypeHash == 4791461876223233767) // crashfish
                        noLoS = Physics.Linecast(__instance.transform.position, go.transform.position, Voxeland.GetTerrainLayerMask());
                    else
                        noLoS = Physics.Linecast(go.transform.position, __instance.transform.position, Voxeland.GetTerrainLayerMask());

                    __result = !noLoS;
                }
                return false;
            }
        }
        
        [HarmonyPatch(typeof(CreatureDeath))]
        class CreatureDeath_Patch
        {

            //static HashSet<TechType> creatureDeaths = new HashSet<TechType>();
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CreatureDeath __instance)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                //if (!creatureDeaths.Contains(techType))
                //{
                //    creatureDeaths.Add(techType);
                //    Main.logger.LogMessage("CreatureDeath " + techType + " respawns " + __instance.respawn + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature + " respawnInterval " + __instance.respawnInterval);
                //}
                __instance.respawn = !notRespawningCreatures.Contains(techType);
                __instance.respawnOnlyIfKilledByCreature = notRespawningCreaturesIfKilledByPlayer.Contains(techType);
                //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawn " + __instance.respawn);
                //Main.logger.LogMessage("CreatureDeath Start " + techType + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
                if (respawnTime.ContainsKey(techType))
                    __instance.respawnInterval = respawnTime[techType] * Main.dayLengthSeconds;
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnTakeDamage")]
            static void OnTakeDamagePostfix(CreatureDeath __instance, DamageInfo damageInfo)
            {
                //AddDebug(__instance.name + " OnTakeDamage " + damageInfo.dealer.name);
                if (!ConfigToEdit.heatBladeCooks.Value && damageInfo.type == DamageType.Heat && damageInfo.dealer == Player.mainObject)
                    __instance.lastDamageWasHeat = false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnKill")]
            static void OnKillPrefix(CreatureDeath __instance)
            {
                //AddDebug(__instance.name + " OnKill");
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnKill")]
            static void OnKillPostfix(CreatureDeath __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt != TechType.Peeper)
                    return;
                LODGroup lod = __instance.GetComponentInChildren<LODGroup>(true);
                lod.enabled = false;
                SkinnedMeshRenderer[] renderers = __instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                //AddDebug("Peeper OnKill " + renderers.Length);
                //for (int i = 1; i < renderers.Length; i++)
                renderers[0].enabled = false;
            }
        }
        
        [HarmonyPatch(typeof(SeaTreaderSounds))]
        class SeaTreaderSounds_patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnStep")]
            public static bool OnStepPrefix(SeaTreaderSounds __instance, Transform legTr, AnimationEvent animationEvent)
            {
                if (Main.config.seaTreaderOutcrop == Config.SeaTreaderOutcrop.Vanilla)
                    return true;

                if (animationEvent.animatorClipInfo.clip == __instance.walkinAnimClip && !__instance.treader.IsWalking())
                    return false;

                if (__instance.stepEffect != null)
                    Utils.SpawnPrefabAt(__instance.stepEffect, null, legTr.position);
                if (__instance.stepSound != null)
                    Utils.PlayEnvSound(__instance.stepSound, legTr.position);

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnStomp")]
            public static bool OnStompPrefix(SeaTreaderSounds __instance)
            {
                if (Main.config.seaTreaderOutcrop == Config.SeaTreaderOutcrop.Never)
                {
                    if (Time.time < __instance.lastStompAttackTime + 0.2f)
                        return false;

                    __instance.lastStompAttackTime = Time.time;
                    if (__instance.stompEffect != null)
                        Utils.SpawnPrefabAt(__instance.stompEffect, null, __instance.frontLeg.position);
                    if (__instance.stompSound != null)
                        Utils.PlayEnvSound(__instance.stompSound, __instance.frontLeg.position);

                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(ReefbackLife), "OnEnable")]
        class ReefbackLife_OnEnable_patch
        {
            public static void Postfix(ReefbackLife __instance)
            { // make it avoid player life pod
                //AddDebug(" ReefbackLife OnEnable " + (int)__instance.transform.position.y);
                AvoidObstacles ao = __instance.gameObject.GetComponent<AvoidObstacles>();
                if (!ao)
                    return;
                AvoidEscapePod aep = __instance.gameObject.EnsureComponent<AvoidEscapePod>();
                aep.swimVelocity = ao.swimVelocity;
                aep.swimInterval = ao.swimInterval;
                aep.maxDistanceToPod = 100f;
                //if (__instance.transform.position.y > -15f)
                //    __instance.transform.position = new Vector3(__instance.transform.position.x, -15f, __instance.transform.position.z);
            }
        }

        [HarmonyPatch(typeof(Pickupable), "AllowedToPickUp")]
        class Pickupable_AllowedToPickUp_Patch
        {
            public static void Postfix(Pickupable __instance, ref bool __result)
            {
                //__result = __instance.isPickupable && Time.time - __instance.timeDropped > 1.0 && Player.main.HasInventoryRoom(__instance);
                if (Main.config.noFishCatching && Util.IsEatableFish(__instance.gameObject) && !Util.IsDead(__instance.gameObject))
                {
                    __result = false;
                    if (Player.main._currentWaterPark)
                    {
                        __result = true;
                        //AddDebug("WaterPark ");
                        return;
                    }
                    PropulsionCannonWeapon pc = Inventory.main.GetHeldTool() as PropulsionCannonWeapon;
                    if (pc && pc.propulsionCannon.grabbedObject == __instance.gameObject)
                    {
                        //AddDebug("PropulsionCannonWeapon ");
                        __result = true;
                        return;
                    }
                    foreach (Pickupable p in Gravsphere_Patch.gravSphereFish)
                    {
                        if (p == __instance)
                        {
                            //AddDebug("Gravsphere ");
                            __result = true;
                            return;
                        }
                    }
                    Rigidbody rigidbody = __instance.GetComponent<Rigidbody>();
                    foreach (Rigidbody rb in Tools_Patch.stasisTargets)
                    {
                        if (rigidbody == rb)
                        {
                            __result = true;
                        }
                    }
                }

            }
        }
        
        [HarmonyPatch(typeof(SwimBehaviour), "SwimToInternal")]
        class SwimBehaviour_SwimToInternal_patch
        {
            public static void Prefix(SwimBehaviour __instance, ref float velocity, ref Vector3 targetPosition)
            {
                if (Util.IsEatableFish(__instance.gameObject))
                {
                    velocity *= Main.config.fishSpeedMult;
                }
                else
                {
                    velocity *= Main.config.creatureSpeedMult;
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    if (tt == TechType.Reefback && targetPosition.y > -15f)
                    { // dont allow them to surface
                        //AddDebug("Fix reefback y pos");
                        targetPosition.y = -15f;
                    }
                    else if (tt == TechType.Gasopod && targetPosition.y > -1f)
                    { 
                        targetPosition.y = Main.rndm.Next(-11, -1);
                        //AddDebug("Gasopod Swim To " + targetPosition.y);
                    }
                }
            }
        }



        [HarmonyPatch(typeof(CollectShiny))]
        class CollectShiny_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("UpdateShinyTarget")]
            public static bool UpdateShinyTargetPrefix(CollectShiny __instance)
            {// dont approach player holding shiny if cant grab it
                GameObject gameObject = null;
                if (EcoRegionManager.main != null)
                {
                    IEcoTarget nearestTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Shiny, __instance.transform.position, __instance.isTargetValidFilter);
                    if (nearestTarget != null)
                        gameObject = nearestTarget.GetGameObject();
                    else
                        gameObject = null;
                }
                if (gameObject)
                {
                    if (!Main.config.stalkersGrabShinyTool && gameObject.GetComponentInParent<Player>())
                    {
                        //AddDebug("player holding shiny " + gameObject.name);
                        return false;
                    }
                    Vector3 direction = gameObject.transform.position - __instance.transform.position;
                    float maxDistance = direction.magnitude - 0.5f;
                    Vector3 playerDir = gameObject.transform.position - Player.main.transform.position;
                    //AddDebug(gameObject.name + " maxDistance " + maxDistance);
                    //AddDebug("Raycast Player " + gameObject.name + " " + Physics.Raycast(Player.main.transform.position, playerDir, 111, Voxeland.GetTerrainLayerMask()));
                    if (maxDistance < 0 || Physics.Raycast(__instance.transform.position, direction, maxDistance, Voxeland.GetTerrainLayerMask()))
                        gameObject = null;
                }
                if (__instance.shinyTarget == gameObject || gameObject == null || gameObject.GetComponent<Rigidbody>() == null || gameObject.GetComponent<Pickupable>() == null)
                    return false;

                if (__instance.shinyTarget != null)
                {
                    if ((gameObject.transform.position - __instance.transform.position).magnitude <= (__instance.shinyTarget.transform.position - __instance.transform.position).magnitude)
                        return false;

                    __instance.DropShinyTarget();
                    __instance.shinyTarget = gameObject;
                }
                else
                    __instance.shinyTarget = gameObject;

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("TryPickupShinyTarget")]
            public static bool TryPickupShinyTargetPrefix(CollectShiny __instance)
            {
                if (__instance.shinyTarget == null || !__instance.shinyTarget.activeInHierarchy)
                    return false;

                if (__instance.shinyTarget.GetComponentInParent<Player>() != null)
                {
                    //AddDebug("player holds shiny");
                    if (Main.config.stalkersGrabShinyTool && Player.main.currentSub == null)
                        Inventory.main.DropHeldItem(false);
                    else
                    {
                        __instance.shinyTarget = null;
                        __instance.targetPickedUp = false;
                        __instance.timeNextFindShiny = Time.time + 6f;
                        return false;
                    }
                }
                //AddDebug("save shiny " + __instance.shinyTarget);
                pickupShinies.Add(__instance.shinyTarget);
                __instance.SendMessage("OnShinyPickUp", __instance.shinyTarget, SendMessageOptions.DontRequireReceiver);
                __instance.shinyTarget.gameObject.SendMessage("OnShinyPickUp", __instance.gameObject, SendMessageOptions.DontRequireReceiver);
                UWE.Utils.SetCollidersEnabled(__instance.shinyTarget, false);
                __instance.shinyTarget.transform.parent = __instance.shinyTargetAttach;
                __instance.shinyTarget.transform.localPosition = Vector3.zero;
                __instance.targetPickedUp = true;
                UWE.Utils.SetIsKinematic(__instance.shinyTarget.GetComponent<Rigidbody>(), true);
                UWE.Utils.SetEnabled(__instance.shinyTarget.GetComponent<LargeWorldEntity>(), false);
                __instance.SendMessage("OnShinyPickedUp", __instance.shinyTarget, SendMessageOptions.DontRequireReceiver);
                __instance.swimBehaviour.SwimTo(__instance.transform.position + Vector3.up * 5f + UnityEngine.Random.onUnitSphere, Vector3.up, __instance.swimVelocity);
                __instance.timeNextSwim = Time.time + 1f;
                BehaviourUpdateUtils.Register(__instance);
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("DropShinyTarget", new Type[] { typeof(GameObject) })]
            public static void DropShinyTargetPrefix(CollectShiny __instance, GameObject target)
            {
                if (__instance.shinyTarget && __instance.targetPickedUp)
                {
                    pickupShinies.Remove(target);
                    //AddDebug("DropShinyTarget " + target.name);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("IsTargetValid")]
            public static void IsTargetValidPostfix(CollectShiny __instance, ref bool __result, IEcoTarget target)
            {
                //__result = (target.GetPosition() - __instance.creature.leashPosition).sqrMagnitude > 64.0;
                GameObject targetGO = target.GetGameObject();
                //TechType tt = CraftData.GetTechType(targetGO);
                //if (tt == TechType.ScrapMetal)
                //    __result = false;
                if (pickupShinies.Contains(targetGO))
                {
                    //AddDebug("IsTargetValid pickupShinies " + targetGO.name);
                    __result = false;
                }
                //AddDebug("IsTargetValid " + tt + " " + __result);
            }

        }

        [HarmonyPatch(typeof(CreatureEgg), "Start")]
        class CreatureEgg_Start_patch
        {
            public static void Postfix(CreatureEgg __instance)
            {
                VFXSurface vFXSurface = __instance.gameObject.EnsureComponent<VFXSurface>();
                vFXSurface.surfaceType = VFXSurfaceTypes.organic;
            }
        }

        [HarmonyPatch(typeof(GasoPod), "Update")]
        class GasoPod_Update_patch
        {
            public static bool Prefix(GasoPod __instance)
            {
                Rigidbody rb;
                if (objectsRBs.TryGetValue(__instance.gameObject, out rb))
                {
                    //Rigidbody rb = objectsRBs[__instance.gameObject];
                    if (rb && Tools_Patch.stasisTargets.Contains(rb))
                    {
                        //AddDebug("GasoPod in stasis");
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GasPod))]
        class GasPod_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(GasPod __instance)
            {
                if (__instance.detonated)
                    return;

                Rigidbody rb = __instance.GetComponent<Rigidbody>();
                if (rb)
                {
                    //AddDebug("GasoPod start save");
                    objectsRBs.Add(__instance.gameObject, rb);
                }
            }
            [HarmonyPrefix]
            [HarmonyPatch("Update")]
            public static bool UpdatePrefix(GasPod __instance)
            {
                if (__instance.detonated)
                    return true;

                Rigidbody rb;
                if (objectsRBs.TryGetValue(__instance.gameObject, out rb))
                {
                    //Rigidbody rb = objectsRBs[__instance.gameObject];
                    if (rb && Tools_Patch.stasisTargets.Contains(rb))
                    {
                        //AddDebug("GasPod in stasis");
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(WaterParkCreature), "GetCanBreed")]
        class GWaterParkCreature_GetCanBreed_patch
        {
            public static void Postfix(WaterParkCreature __instance, ref bool __result)
            {
                if (!Main.config.waterparkCreaturesBreed)
                    __result = false;
            }
        }


   
        



    }
}
