using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Predators_Patch
    {         //300 -90 60
        static HashSet<SubRoot> cyclops = new HashSet<SubRoot>();
        static Dictionary<AttackCyclops, AggressiveWhenSeeTarget> attackCyclopsAWST = new Dictionary<AttackCyclops, AggressiveWhenSeeTarget>();
        //public static HashSet<string> testMeleeAttack = new HashSet<string>();

        public static bool IsLightOn(Vehicle vehicle)
        { 
            Light[] lights = vehicle.GetComponentsInChildren<Light>();
            foreach (Light l in lights)
            {
			    if (l.enabled && l.gameObject.activeInHierarchy && l.intensity > 0f && l.range > 0f)
                    return true;
            }
            return false;
        }

        [HarmonyPatch(typeof(SubRoot), "Start")]
        internal class SubRoot_Start_Patch
        {
            public static void Postfix(SubRoot __instance)
            {
                if (__instance.isCyclops)
                {
                    cyclops.Add(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(SubRoot), "OnPlayerExited")]
        internal class SubRoot_OnPlayerExited_Patch
        {
            public static void Postfix(SubRoot __instance)
            {
                if (__instance.isCyclops)
                {
                    __instance.live.invincible = Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Vanilla || Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.No;
                }
            }
        }

        [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
        internal class CyclopsHelmHUDManager_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("OnTakeCreatureDamage")]
            public static bool OnTakeCreatureDamagePrefix(CyclopsHelmHUDManager __instance)
            {
                if (__instance.subRoot == Player.main.currentSub)
                {
                    //AddDebug("CyclopsHelmHUDManager OnTakeCreatureDamage");
                    __instance.CancelInvoke("ClearCreatureWarning");
                    __instance.Invoke("ClearCreatureWarning", 10f);
                    __instance.creatureAttackWarning = true;
                    __instance.creatureDamagesSFX.Play();
                    MainCameraControl.main.ShakeCamera(1.5f);
                }
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnTakeCollisionDamage")]
            public static bool OnTakeCollisionDamagePrefix(CyclopsHelmHUDManager __instance, ref float value)
            {
                if (__instance.subRoot == Player.main.currentSub)
                {
                    value *= 1.5f;
                    value = Mathf.Clamp(value / 100f, 0.5f, 1.5f);
                    MainCameraControl.main.ShakeCamera(value);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SubRoot), "OnKill")]
        internal class SubRoot_OnKill_Patch
        {
            public static void Postfix(SubRoot __instance)
            {
                cyclops.Remove(__instance);
            }
        }

        [HarmonyPatch(typeof(Player), "CanBeAttacked")]
        internal class Player_CanBeAttacked_Patch
        {
            public static bool Prefix(Player __instance, ref bool __result)
            {
                //AddDebug("AggressiveWhenSeeTarget start " + __instance.myTechType + " " + __instance.maxSearchRings);
                //__result = !__instance.IsInsideWalkable() && !__instance.justSpawned && !GameModeUtils.IsInvisible() && !Player.main.precursorOutOfWater && !PrecursorMoonPoolTrigger.inMoonpool;
                __result = !__instance.IsInsideWalkable() && !__instance.justSpawned && !GameModeUtils.IsInvisible() && Main.config.aggrMult > 0f;
                return false;
            }
        }

        [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
        internal class AggressiveWhenSeeTarget_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetAggressionTarget")]
            public static bool GetAggressionTargetPrefix(AggressiveWhenSeeTarget __instance, ref GameObject __result)
            {
                if (__instance.targetType != EcoTargetType.Shark || Main.config.aggrMult <= 1 || Main.config.predatorExclusion.Contains(__instance.myTechType))
                    return true;

                int searchRings = Mathf.RoundToInt(__instance.maxSearchRings * Main.config.aggrMult);
                IEcoTarget ecoTarget = null;
                if (__instance.targetType == EcoTargetType.Shark)
                {
                    ecoTarget = EcoRegionManager.main.FindNearestTarget( EcoTargetType.SubDecoy, __instance.transform.position, __instance.isTargetValidFilter, searchRings);
                }
                if (ecoTarget == null)
                {
                    ecoTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, searchRings);
                }
                __result = ecoTarget == null ? null : ecoTarget.GetGameObject();
                //if (__result == Player.main.gameObject)
                //AddDebug(__instance.myTechType + " AggressionTarget PLAYER ");
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("IsTargetValid", new Type[] { typeof(GameObject) })]
            public static bool IsTargetValidPrefix(GameObject target, AggressiveWhenSeeTarget __instance, ref bool __result)
            {
                if (__instance.targetType != EcoTargetType.Shark || Main.config.predatorExclusion.Contains(__instance.myTechType))
                    return true;

                if (target == Player.main.gameObject)
                {
                    //AddDebug(__instance.myTechType + " Player");
                    if (!Player.main.CanBeAttacked() || Player.main.precursorOutOfWater || PrecursorMoonPoolTrigger.inMoonpool)
                    {
                        __result = false;
                        return false;
                    }
                    if (BehaviourData.GetBehaviourType(__instance.myTechType) == BehaviourType.Leviathan)
                    { // prevent leviathan attacking player on land
                        if (Player.main.depthLevel > -5f)
                        {
                            //AddDebug(" prevent leviathan attacking player on land");
                            __result = false;
                            return false;
                        }
                    }
                }
                Vehicle vehicle = target.GetComponent<Vehicle>();
                if (vehicle)
                {
                    if (Main.config.aggrMult == 0 || vehicle.precursorOutOfWater)
                    {
                        __result = false;
                        return false;
                    }
                    if (Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.No && Player.main.currentMountedVehicle == null)
                    {
						__result = false;
						return false;
                    }
                    if (BehaviourData.GetBehaviourType(__instance.myTechType) == BehaviourType.Leviathan)
                    { // prevent leviathan attack on land
                        if (Ocean.main.GetDepthOf(target) < 5f)
                        {
                            __result = false;
                            return false;
                        }
                    }
                    Vector3 vel = vehicle.useRigidbody.velocity;
                    //bool moving = vel.x > 1f || vel.y > 1f || vel.z > 1f;
                    __result = Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Vanilla || Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Yes || Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Only_if_lights_on && IsLightOn(vehicle);
                    //__result = moving || canBeAttacked;
                    return false;
                }
                if (target == null || target == __instance.creature.friend)
                {
                    __result = false;
                    return false;
                }
                TechType targetTT = CraftData.GetTechType(target);
                if (targetTT == TechType.CyclopsDecoy)
                {
                    __result = true;
                    return false;
                }
                if (__instance.ignoreSameKind && targetTT == __instance.myTechType)
                {
                    __result = false;
                    return false;
                }
                if (__instance.targetShouldBeInfected)
                {
                    InfectedMixin im = target.GetComponent<InfectedMixin>();
                    if (im == null || im.GetInfectedAmount() < 0.33f)
                    {
                        __result = false;
                        return false;
                    }
                }
                float dist = Vector3.Distance(target.transform.position, __instance.transform.position);
                //float aggrMult = Main.config.aggrMult > 1 ? Main.config.aggrMult : 1;
                if (dist > __instance.maxRangeScalar * Main.config.aggrMult)
                {
                    __result = false;
                    return false;
                }
                if (!Mathf.Approximately(__instance.minimumVelocity, 0f))
                {
                    Rigidbody rb = target.GetComponentInChildren<Rigidbody>();
                    if (rb && rb.velocity.magnitude <= __instance.minimumVelocity)
                    {
                        __result = false;
                        return false;
                    }
                }
                __result = __instance.creature.GetCanSeeObject(target);
                //__result = !Physics.Linecast(__instance.transform.position, target.transform.position, Voxeland.GetTerrainLayerMask());
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("ScanForAggressionTarget")]
            public static bool ScanForAggressionTargetPrefix(AggressiveWhenSeeTarget __instance)
            {
                if (EcoRegionManager.main == null || !__instance.gameObject.activeInHierarchy)
                    return false;

                if (__instance.targetType != EcoTargetType.Shark || Main.config.predatorExclusion.Contains(__instance.myTechType))
                    return true;

                if (Main.config.aggrMult <= 1 && __instance.creature && __instance.creature.Hunger.Value < __instance.hungerThreshold)
                    return true;

                if (Main.config.aggrMult == 3 && Player.main.CanBeAttacked() && __instance.creature.GetCanSeeObject(Player.main.gameObject))
                {
                    __instance.creature.Aggression.Add(__instance.aggressionPerSecond);
                    __instance.lastScarePosition.lastScarePosition = Player.main.gameObject.transform.position;
                    __instance.lastTarget.target = Player.main.gameObject;
                    if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Tweaks.silentCreatures.Contains(__instance.myTechType))
                        __instance.sightedSound.StartEvent();

                    //AddDebug(__instance.myTechType + " attack player " + );
                    return false;
                }
                GameObject aggressionTarget = __instance.GetAggressionTarget();
                if (aggressionTarget != null)
                {
                    float dist = Vector3.Distance(aggressionTarget.transform.position, __instance.transform.position);
                    float num2 = DayNightUtils.Evaluate(__instance.maxRangeScalar, __instance.maxRangeMultiplier);
                    //distMult < 0 if target very close
                    float distMult = __instance.distanceAggressionMultiplier.Evaluate((num2 - dist) / num2);
                    if (distMult < 1f)
                        distMult = 1f;
                    //Main.Log(__instance.myTechType + " " + aggressionTarget.name + " aggr dist " + dist + " distMult " + distMult + " aggr/second " + __instance.aggressionPerSecond);
                    float infection = 1f;
                    if (__instance.targetShouldBeInfected)
                    {
                        InfectedMixin im = aggressionTarget.GetComponent<InfectedMixin>();
                        infection = im != null ? im.infectedAmount : 0f;
                    }
                    //UnityEngine.Debug.DrawLine(aggressionTarget.transform.position, __instance.transform.position, Color.white);
                    __instance.creature.Aggression.Add((__instance.aggressionPerSecond * distMult * infection));

                    __instance.lastScarePosition.lastScarePosition = aggressionTarget.transform.position;
                    __instance.lastTarget.target = aggressionTarget;
                    if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Tweaks.silentCreatures.Contains(__instance.myTechType))
                        __instance.sightedSound.StartEvent();
                }
                return false;
            }

        }

        [HarmonyPatch(typeof(EcoRegion), "FindNearestTarget")]
        internal class EcoRegion_FindNearestTarget_Patch
        {
            public static bool PreFix(EcoRegion __instance, EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float bestDist, ref IEcoTarget best)
            {
                if (Main.config.aggrMult == 1 || type != EcoTargetType.Shark)
                    return true;
                //ProfilingUtils.BeginSample("EcoRegion.FindNearestTarget");
                __instance.timeStamp = Time.time;
                //float agr = Main.config.aggrMult > 1 ? Main.config.aggrMult : 1;
                HashSet<IEcoTarget> ecoTargetSet;
                float minSqrMagnitude = float.MaxValue;
                if (!__instance.ecoTargets.TryGetValue((int)type, out ecoTargetSet))
                {
                    //ProfilingUtils.EndSample();
                }
                foreach (IEcoTarget ecoTarget in ecoTargetSet)
                {
                    //HashSet<IEcoTarget>.Enumerator enumerator = ecoTargetSet.GetEnumerator();
                    //while (enumerator.MoveNext())
                    //{
                    //IEcoTarget current = enumerator.Current;
                    if (ecoTarget != null && !ecoTarget.Equals(null))
                    {
                        float sqrMagnitude = (wsPos - ecoTarget.GetPosition()).sqrMagnitude;
                        //if (agr > 1f)
                        //{
                            bool player = ecoTarget.GetGameObject() == Player.main.gameObject;
                            //bool vehicle = ecoTarget.GetGameObject().GetComponent<Vehicle>();
                            if (player)
                                sqrMagnitude /= Main.config.aggrMult;
                            //if (agr == 3)
                            //    sqrMagnitude = 0f;
                        //}
                        if (sqrMagnitude < minSqrMagnitude && (isTargetValid == null || isTargetValid(ecoTarget)))
                        {
                            best = ecoTarget;
                            minSqrMagnitude = sqrMagnitude;
                        }
                    }
                }
				if (best != null)
					bestDist = Mathf.Sqrt(minSqrMagnitude);
                return false;
            }
        }

        //[HarmonyPatch(typeof(MoveTowardsTarget), "UpdateCurrentTarget")]
        internal class MoveTowardsTarget_UpdateCurrentTarget_Patch
        { // this does not target player
            public static bool Prefix(MoveTowardsTarget __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (__instance.targetType == EcoTargetType.Shark )
                {
                    //AddDebug(tt + " aggr " + __instance.creature.Aggression.Value + " req aggr " + __instance.requiredAggression);
                    float aggr = Main.config.aggrMult > 1f ? Main.config.aggrMult : 1f;
                    if (EcoRegionManager.main != null && (Mathf.Approximately(__instance.requiredAggression, 0f) || __instance.creature.Aggression.Value * aggr >= __instance.requiredAggression))
                    {
                        AggressiveWhenSeeTarget awst = __instance.GetComponent<AggressiveWhenSeeTarget>();
                        if (awst)
                            aggr *= awst.maxSearchRings;
                        IEcoTarget nearestTarget = EcoRegionManager.main.FindNearestTarget(__instance.targetType, __instance.transform.position, __instance.isTargetValidFilter, (int)aggr);
                        __instance.currentTarget = nearestTarget;
                    }
                    return false;
                }
                else
                    return true;
            }
        }

        public static bool CanAttackSub(AttackCyclops attackCyclops)
        {
            if (Main.config.aggrMult == 0)
                return false;

            if (Player.main.currentSub && Player.main.currentSub.isCyclops)
                return true;
            else
            {
                if (attackCyclopsAWST.ContainsKey(attackCyclops) && attackCyclopsAWST[attackCyclops].lastTarget.target == Player.main.gameObject)
                {
                    //AddDebug("Lev attacking player");
                    return false;
                }
                CyclopsNoiseManager cyclopsNoiseManager = null;
                if (Player.main.currentSub && Player.main.currentSub.noiseManager)
                    cyclopsNoiseManager = Player.main.currentSub.noiseManager;
                if (attackCyclops.forcedNoiseManager)
                    cyclopsNoiseManager = attackCyclops.forcedNoiseManager;

                if ( Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Yes)
                    return true;
                else if (Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Vanilla || Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.No)
                    return false;

                bool lightOn = cyclopsNoiseManager.lightingPanel.lightingOn || cyclopsNoiseManager.lightingPanel.floodlightsOn;
                return Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Only_if_lights_on && lightOn;
            }
        }

        public static SubRoot GetClosestSub(Vector3 pos)
        {
            float closestDist = float.PositiveInfinity;
            SubRoot closest = null;
            foreach (SubRoot s in cyclops)
            {
                float dist = Vector3.Distance(s.transform.position, pos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = s;
                }
            }
            return closest;
        }

        [HarmonyPatch(typeof(AttackCyclops), "Start")]
        internal class AttackCyclops_Start_Patch
        {
            public static void Postfix(AttackCyclops __instance)
            {
                //AddDebug("AttackCyclops " + __instance.name);
                AggressiveWhenSeeTarget[] awst = __instance.GetAllComponentsInChildren<AggressiveWhenSeeTarget>();
                foreach (AggressiveWhenSeeTarget a in awst)
                {
                    if (a.targetType == EcoTargetType.Shark)
                    {
                        attackCyclopsAWST[__instance] = a;
                        break;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(AttackCyclops), "UpdateAggression")]
        internal class AttackCyclops_UpdateAggression_Patch
        {
            public static bool Prefix(AttackCyclops __instance)
            {
                if (Main.config.aggrMult == 1f)
                    return true;
                else if (Main.config.aggrMult == 0f)
                {
                    //AddDebug("Main.config.aggrMult == 0f");
                    __instance.currentTarget = null;
                    __instance.StopAttack();
                    return false;
                }
                __instance.aggressiveToNoise.UpdateTrait(0.5f);
                if (Time.time < __instance.timeLastAttack + __instance.attackPause)
                    return false;

                bool inSub = Player.main.currentSub && Player.main.currentSub.isCyclops;
                //GameObject target = null;
                CyclopsNoiseManager cyclopsNoiseManager = null;
                CyclopsDecoy closestDecoy = __instance.GetClosestDecoy();

                if (Main.config.aggrMult == 3f && inSub)
                {
                    //AddDebug("SetCurrentTarget " + Player.main.currentSub.gameObject.name);
                    __instance.aggressiveToNoise.Add(__instance.aggressPerSecond);
                    __instance.SetCurrentTarget(Player.main.currentSub.gameObject, false);
                    return false;
                }
                if (closestDecoy)
                {
                    //target = closestDecoy.gameObject;
                    Vector3 position = closestDecoy.transform.position;
                    //float aggrMult = Main.config.aggrMult < 1 ? 1 : Main.config.aggrMult;
                    float aggrDist = 150f * Main.config.aggrMult;
                    if (Vector3.Distance(position, __instance.transform.position) < aggrDist && Vector3.Distance(position, __instance.creature.leashPosition) < __instance.maxDistToLeash)
                    {
                        __instance.aggressiveToNoise.Add(__instance.aggressPerSecond * 0.5f);
                        __instance.SetCurrentTarget(closestDecoy.gameObject, true);
                    }
                    return false;
                }
                else if (closestDecoy == null)
                {
                    if(inSub)
                    {
                        cyclopsNoiseManager = Player.main.currentSub.noiseManager;
                        if (__instance.forcedNoiseManager != null)
                            cyclopsNoiseManager = __instance.forcedNoiseManager;
                    }
                }
                if (cyclopsNoiseManager == null && CanAttackSub(__instance))
                {
                    SubRoot subRoot = GetClosestSub(__instance.transform.position);
                    if (subRoot)
                    {
                        //AddDebug("attack sub");
                        //subRoot.live.TakeDamage(400f);
                        cyclopsNoiseManager = subRoot.noiseManager;
                    }
                }
                if (cyclopsNoiseManager != null)
                {
                    Vector3 position = cyclopsNoiseManager.transform.position;
                    //float aggrMult = Main.config.aggrMult < 1 ? 1 : Main.config.aggrMult;
                    float noise = cyclopsNoiseManager.GetNoisePercent();
                    if ( __instance.creature.GetCanSeeObject(cyclopsNoiseManager.gameObject))
                    {
                        //AddDebug("can see sub" + __instance.attackAggressionThreshold);
                        if (cyclopsNoiseManager.lightingPanel.lightingOn)
                            noise += .5f;
                        if (cyclopsNoiseManager.lightingPanel.floodlightsOn)
                            noise += .5f;
                        if (noise > 1f)
                            noise = 1f;
                    }
                    float aggrDist = Mathf.Lerp(0f, 150f * Main.config.aggrMult, noise);

                    if (Vector3.Distance(position, __instance.transform.position) < aggrDist && Vector3.Distance(position, __instance.creature.leashPosition) < __instance.maxDistToLeash)
                    {
                        //target = cyclopsNoiseManager.gameObject;
                        __instance.aggressiveToNoise.Add(__instance.aggressPerSecond * Main.config.aggrMult * .5f);
                        __instance.SetCurrentTarget(cyclopsNoiseManager.gameObject, false);
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(AttackCyclops), "OnCollisionEnter")]
        internal class AttackCyclops_OnCollisionEnter_Patch
        {
            public static bool Prefix(AttackCyclops __instance)
            {
                if (Main.config.aggrMult == 0f)
                {
                    //AddDebug("Main.config.aggrMult == 0f");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(AttackCyclops), "IsTargetValid")]
        internal class AttackCyclops_IsTargetValid_Patch
        {
            public static bool Prefix(AttackCyclops __instance, IEcoTarget target, ref bool __result)
            {
                TechType myTT = CraftData.GetTechType(__instance.gameObject);
                if (Main.config.predatorExclusion.Contains(myTT))
                    __result = false;
                else
                    __result = Vector3.Distance(target.GetPosition(), __instance.transform.position) < 150f * Main.config.aggrMult;
                return false;
            }
        }

        [HarmonyPatch(typeof(AttachToVehicle), "Evaluate")]
        internal class AttachToVehicle_Evaluate_Patch
        {
            public static bool Prefix(AttachToVehicle __instance, Creature creature, ref float __result)
            {
                if (GameModeUtils.IsInvisible())
                {
                    __result = 0f;
                    return false;
                }
                if (Time.time > __instance.timeNextScan)
                {
                    __instance.UpdateCurrentTarget();
                    __instance.timeNextScan = Time.time + __instance.scanInterval;
                }
                __result = __instance.timeDetached + 4f < Time.time && __instance.currTarget != null && (__instance.currTarget.transform.position - __instance.transform.position).sqrMagnitude <= __instance.currTarget.distanceToStartAction * __instance.currTarget.distanceToStartAction * Main.config.aggrMult ? __instance.GetEvaluatePriority() : 0f;
                return false;
            }
        }

        [HarmonyPatch(typeof(AttachToVehicle), "IsValidTarget")]
        internal class AttachToVehicle_IsValidTarget_Patch
        {
            public static void Postfix(AttachToVehicle __instance, IEcoTarget target, ref bool __result)
            {
                if (Main.config.aggrMult == 0f || Main.config.predatorExclusion.Contains(CraftData.GetTechType(__instance.gameObject)))
                    __result = false;
            }
        }

        [HarmonyPatch(typeof(AggressiveToPilotingVehicle), "UpdateAggression")]
        internal class AggressiveToPilotingVehicle_UpdateAggression_Patch
        {
            public static bool Prefix(AggressiveToPilotingVehicle __instance)
            {
                Player main = Player.main;
                if (main == null || main.GetMode() != Player.Mode.LockedPiloting)
                    return false;
                if (Main.config.aggrMult == 0)
                    return false;
                TechType myTT = CraftData.GetTechType(__instance.gameObject);
                if (Main.config.predatorExclusion.Contains(myTT))
                    return false;
                Vehicle vehicle = main.GetVehicle();
                if (vehicle == null || vehicle.prevVelocity == Vector3.zero || Vector3.Distance(vehicle.transform.position, __instance.transform.position) > __instance.range * Main.config.aggrMult)
                    return false;
                //__instance.creature.GetCanSeeObject(Player.main.gameObject))
                __instance.lastTarget.target = vehicle.gameObject;
                __instance.creature.Aggression.Add(__instance.aggressionPerSecond * __instance.updateAggressionInterval * Main.config.aggrMult);
                //AddDebug(" AggressiveToPilotingVehicle range " + __instance.range);
                //AddDebug(" prevVelocity " + vehicle.prevVelocity);
                return false;
            }
        }

        [HarmonyPatch(typeof(MeleeAttack))]
        internal class MeleeAttack_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OnEnable")]
            public static void OnEnablePrefix(MeleeAttack __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug(tt + " canBitePlayer " + __instance.canBitePlayer + " canBiteVehicle " + __instance.canBiteVehicle + " canBiteCyclops " + __instance.canBiteCyclops);
                //testMeleeAttack.Add(tt + " canBitePlayer " + __instance.canBitePlayer + " canBiteVehicle " + __instance.canBiteVehicle + " canBiteCyclops " + __instance.canBiteCyclops);
            }
            [HarmonyPrefix]
            [HarmonyPatch("CanBite")]
            public static bool CanBitePrefix(MeleeAttack __instance, GameObject target, ref bool __result)
            {
                if (__instance.frozen || (__instance.creature.Aggression.Value < __instance.biteAggressionThreshold || Time.time < __instance.timeLastBite + __instance.biteInterval))
                {
                    __result = false;
                    return false;
                }
                Player player = target.GetComponent<Player>();
                if (player != null)
                {
                    //if (!player.CanBeAttacked() || !__instance.canBitePlayer)
                    {
                        __result = Main.config.aggrMult > 0f && player.CanBeAttacked() && __instance.canBitePlayer;
                        return false;
                    }
                }
                SubRoot subRoot = target.GetComponent<SubRoot>();
                if (subRoot && subRoot.isCyclops)
                {
                    if (Main.config.aggrMult == 0f && !__instance.canBiteCyclops)
                    {
                        __result = false;
                        return false;
                    }
                    IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.SubDecoy, __instance.transform.position);
                    if (ecoTarget != null)
                    {
                        __result = false;
                        return false;
                    }
                    bool inSub = player.currentSub && player.currentSub == subRoot;
                    if (inSub)
                    {
                        __result = player.CanBeAttacked();
                        return false;
                    }
                    else
                    {
                        if (Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Yes)
                            return true;
                        else if (Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Vanilla || Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.No)
                            return false;

                        bool lightOn = subRoot.noiseManager.lightingPanel.lightingOn || subRoot.noiseManager.lightingPanel.floodlightsOn;
                        return Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Only_if_lights_on && lightOn;
                    }
                }
                Vehicle vehicle = target.GetComponent<Vehicle>();
                if (vehicle)
                {
                    //bool attackSharks = false;
                    //AggressiveWhenSeeTarget[] awsts = __instance.GetComponents<AggressiveWhenSeeTarget>();
                    //foreach (AggressiveWhenSeeTarget awst in awsts)
                    //{
                    //    if (awst.targetType == EcoTargetType.Shark)
                    //        attackSharks = true;
                    //}
                    if (Main.config.aggrMult == 0f || !__instance.canBiteVehicle)
                    {
                        __result = false;
                        return false;
                    }
                    bool playerInside = player.currentMountedVehicle == vehicle;
                    if (playerInside)
                    {
                        __result = player.CanBeAttacked();
                        return false;
                    }
                    else if (Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.No)
                    {
                        __result = false;
                        return false;
                    }
                    else if (Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Vanilla || Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Yes)
                    {
                        __result = true;
                        return false;
                    }
                    __result = Main.config.emptyVehiclesCanBeAttacked == Config.EmptyVehiclesCanBeAttacked.Only_if_lights_on && IsLightOn(vehicle);
                    return false;
                }
                __result = __instance.canBiteCreature && target.GetComponent<Creature>() != null;
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("CanBite")]
            public static void CanBitePostfix(MeleeAttack __instance, GameObject target, bool __result)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                TechType tt1 = CraftData.GetTechType(target);
                //if (tt1 != TechType.None)
                    //AddDebug(tt + " MeleeAttack CanBite " + tt1 + " " + __result);
            }
        }

        [HarmonyPatch(typeof(AttackLastTarget), "CanAttackTarget")]
        internal class AttackLastTarget_CanAttackTarget_Patch
        { 
            public static bool Prefix(AttackLastTarget __instance, GameObject target, ref bool __result)
            {
                if (target == null)
                {
                    __result = false;
                    return false;
                }
                if (target == Player.main.gameObject)
                {
                    if (Main.config.aggrMult == 0f || !Player.main.CanBeAttacked())
                    {
                        __result = false;
                        return false;
                    }
                }
                LiveMixin lm = target.GetComponent<LiveMixin>();
                __result = lm && lm.IsAlive();
                return false;
            }
        }
      
        //[HarmonyPatch(typeof(AttackCyclops), "SetCurrentTarget")]
        internal class AttackCyclops_SetCurrentTarget_Patch
        {
            public static void Postfix(AttackCyclops __instance, GameObject target, bool isDecoy)
            {
                Main.Message("AttackCyclops SetCurrentTarget " + target + " " + Vector3.Distance(target.transform.position, __instance.transform.position));
            }
        }

        //[HarmonyPatch(typeof(Creature), "Start")]
        internal class Creature_Start_Patch
        {
            public static void Postfix(Creature __instance)
            {
                if (__instance.hasEyes)
                {
                    TechType techType = CraftData.GetTechType(__instance.gameObject);
                    Main.Log(techType + " eyeFOV " + __instance.eyeFOV);
                }

                //AddDebug("Creature Start " + techType);
                //if (Main.config.canAttackSub.Contains(techType))
                {
                    //AttackCyclops attackCyclops = __instance.gameObject.GetComponent<AttackCyclops>();
                    //if (!attackCyclops)
                    //{
                    //    AddDebug("Add  AttackCyclops");
                    //    attackCyclops = __instance.gameObject.AddComponent<AttackCyclops>();
                    //    attackCyclops.aggressiveToNoise = new CreatureTrait(0);
                    //    attackCyclops.maxDistToLeash = 150f;
                    //    attackCyclops.evaluatePriority = .9f;
                    //    attackCyclops.swimVelocity = 25f;
                    //    attackCyclops.attackPause = 3f;
                    //}
                }
            }
        }


    }
}
