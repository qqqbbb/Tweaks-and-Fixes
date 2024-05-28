using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Predators_Patch
    {
        static HashSet<SubRoot> cyclops = new HashSet<SubRoot>();


        //[HarmonyPatch(typeof(GameModeUtils), "IsInvisible")]
        class GameModeUtils_IsInvisible_Patch
        { 
            public static void Postfix(ref bool __result)
            {
                if (ConfigMenu.aggrMult.Value == 0)
                    __result = true;
            }
        }

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

        private static bool CanVehicleBeAttacked(Vehicle vehicle)
        {
            if (GameModeUtils.IsInvisible() || ConfigMenu.aggrMult.Value == 0)
                return false;

            bool playerInside = Player.main.currentMountedVehicle == vehicle;
            if (playerInside)
            {
                return Player.main.CanBeAttacked();
            }
            else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.No)
            {
                return false;
            }
            else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Vanilla || ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Yes)
            {
                return true;
            }
            else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Only_if_lights_on)
            {
                return IsLightOn(vehicle);
            }
            return true;
        }

        [HarmonyPatch(typeof(SubRoot))]
        class SubRoot_Start_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(SubRoot __instance)
            {
                if (__instance.isCyclops)
                {
                    cyclops.Add(__instance);
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnPlayerExited")]
            public static void OnPlayerExitedPostfix(SubRoot __instance)
            {
                if (__instance.isCyclops)
                { // OnPlayerEntered sets  __instance.live.invincible to false
                    __instance.live.invincible = ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Vanilla || ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.No;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnKill")]
            public static void OnKillPostfix(SubRoot __instance)
            {
                cyclops.Remove(__instance);
            }
        }

        [HarmonyPatch(typeof(CyclopsHelmHUDManager))]
        class CyclopsHelmHUDManager_Patch
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

        [HarmonyPatch(typeof(AggressiveWhenSeeTarget))]
        class AggressiveWhenSeeTarget_Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch("ScanForAggressionTarget")]
            public static bool ScanForAggressionTargetPrefix(AggressiveWhenSeeTarget __instance)
            {
                if (EcoRegionManager.main == null || !__instance.gameObject.activeInHierarchy || !__instance.enabled)
                    return false;

                if ( __instance.targetType != EcoTargetType.Shark || ConfigMenu.aggrMult.Value == 1f)
                    return true;

                if (__instance.creature && __instance.creature.Hunger.Value < __instance.hungerThreshold && ConfigMenu.aggrMult.Value == 1f)
                    return true;

                GameObject aggressionTarget = __instance.GetAggressionTarget();
                if (aggressionTarget == null)
                    return false;

                float dist = Vector3.Distance(aggressionTarget.transform.position, __instance.transform.position);
                float num2 = DayNightUtils.Evaluate(__instance.maxRangeScalar, __instance.maxRangeMultiplier);
                float distMult = __instance.distanceAggressionMultiplier.Evaluate((num2 - dist) / num2);
                //AddDebug(__instance.myTechType + " " + aggressionTarget.name + " aggr dist " + dist  );
                //AddDebug(__instance.myTechType + " " + num2 + " __instance.maxRangeScalar " + __instance.maxRangeScalar) ;
                float infection = 1f;
                if (__instance.targetShouldBeInfected)
                {
                    InfectedMixin im = aggressionTarget.GetComponent<InfectedMixin>();
                    infection = im != null ? im.infectedAmount : 0f;
                }
                //UnityEngine.Debug.DrawLine(aggressionTarget.transform.position, __instance.transform.position, Color.white);
                float playerAggrMult = 1;
                if (aggressionTarget == Player.mainObject)
                {
                    //AddDebug(__instance.myTechType + " target Player " + __instance.creature.Aggression.Value);
                    playerAggrMult = ConfigMenu.aggrMult.Value;
                }
                __instance.creature.Aggression.Add(__instance.aggressionPerSecond * distMult * infection * playerAggrMult);
                __instance.lastTarget.SetTarget(aggressionTarget);
                if (__instance.sightedSound != null && !__instance.sightedSound.GetIsPlaying() && !Creature_Tweaks.silentCreatures.Contains(__instance.myTechType))
                    __instance.sightedSound.StartEvent();
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("GetAggressionTarget")]
            public static bool GetAggressionTargetPrefix(AggressiveWhenSeeTarget __instance, ref GameObject __result)
            {
                if (__instance.targetType == EcoTargetType.Shark && __instance.myTechType != TechType.Crash && __instance.myTechType != TechType.Mesmer && ConfigMenu.aggrMult.Value > 1f && Player.main.CanBeAttacked() && __instance.creature.GetCanSeeObject(Player.mainObject))
                {
                    int rnd = UnityEngine.Random.Range(101, 200);
                    if (ConfigMenu.aggrMult.Value * 100f >= rnd)
                    {
                        //AddDebug(__instance.myTechType + " GetAggressionTarget Player ");
                        __result = Player.mainObject;
                        return false;
                    }
                }
            
                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("IsTargetValid", new Type[] { typeof(GameObject) })]
            public static void IsTargetValidPostfix(AggressiveWhenSeeTarget __instance, GameObject target, ref bool __result)
            {
                //AddDebug(__instance.name + " IsTargetValid " + target.name + " " + __result);
                if (__result)
                {
                    if (ConfigMenu.aggrMult.Value == 0 && target == Player.main.gameObject)
                    {
                        //AddDebug(__instance.name + " IsTargetValid player " + __result);

                        __result = false;
                        return;
                    }
                    if (CreatureData.GetBehaviourType(__instance.myTechType) == BehaviourType.Leviathan)
                    { // prevent leviathan attacking player on land
                      //AddDebug("Player.depthLevel " + Player.main.depthLevel);
                        if (target.transform.position.y > 1f)
                        {
                            //AddDebug(" prevent leviathan attacking on land " + awst.name);
                            __result = false;
                            return;
                        }
                    }
                    Vehicle vehicle = target.GetComponent<Vehicle>();
                    if (vehicle)
                    {
                        __result = CanVehicleBeAttacked(vehicle);
                    }
                }
             }
        }

        public static bool CanAttackSub(AttackCyclops attackCyclops)
        {
            if (Player.main.currentSub && Player.main.currentSub.isCyclops)
                return true;

            CyclopsNoiseManager cyclopsNoiseManager = null;
            if (Player.main.currentSub && Player.main.currentSub.noiseManager)
                cyclopsNoiseManager = Player.main.currentSub.noiseManager;

            if (attackCyclops.forcedNoiseManager)
                cyclopsNoiseManager = attackCyclops.forcedNoiseManager;

            if (cyclopsNoiseManager == null)
                return false;

            if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Yes)
                return true;

            else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Vanilla || ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.No)
                return false;

            bool lightOn = cyclopsNoiseManager.lightingPanel.lightingOn || cyclopsNoiseManager.lightingPanel.floodlightsOn;
            return ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Only_if_lights_on && lightOn;

        }

        public static SubRoot GetClosestSub(Vector3 pos)
        {
            float closestDist = float.MaxValue;
            SubRoot closest = null;
            foreach (SubRoot s in cyclops)
            {
                if (s == null)
                    continue;
                //float dist = Vector3.Distance(s.transform.position, pos);
                Vector3 dir = s.transform.position - pos;
                float distSqr = dir.sqrMagnitude;
                if (distSqr < closestDist)
                {
                    closestDist = distSqr;
                    closest = s;
                }
            }
            return closest;
        }

        [HarmonyPatch(typeof(AttackCyclops))]
        class AttackCyclops_Start_Patch
        {
            static float cyclopsAttackDist = 150f;

            [HarmonyPrefix]
            [HarmonyPatch("UpdateAggression")]
            public static bool UpdateAggressionPrefix(AttackCyclops __instance)
            {
                //AddDebug("AttackCyclops UpdateAggression");
                if (ConfigMenu.aggrMult.Value == 1f)
                    return true;

                if (GameModeUtils.IsInvisible() || ConfigMenu.aggrMult.Value == 0)
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

                if (ConfigMenu.aggrMult.Value == 2f && inSub)
                {
                    //AddDebug("SetCurrentTarget " + Player.main.currentSub.gameObject.name);
                    __instance.aggressiveToNoise.Add(__instance.aggressPerSecond);
                    __instance.SetCurrentTarget(Player.main.currentSub.gameObject, false);
                    return false;
                }
                if (closestDecoy)
                {
                    //target = closestDecoy.gameObject;
                    Vector3 pos = closestDecoy.transform.position;
                    //float aggrMult = Main.config.aggrMult < 1 ? 1 : Main.config.aggrMult;
                    float aggrDist = cyclopsAttackDist * ConfigMenu.aggrMult.Value;
                    if (Vector3.Distance(pos, __instance.transform.position) < aggrDist && Vector3.Distance(pos, __instance.creature.leashPosition) < __instance.maxDistToLeash)
                    {
                        __instance.aggressiveToNoise.Add(__instance.aggressPerSecond * 0.5f);
                        __instance.SetCurrentTarget(closestDecoy.gameObject, true);
                    }
                    return false;
                }
                else if (closestDecoy == null)
                {
                    if (inSub)
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
                    if (__instance.creature.GetCanSeeObject(cyclopsNoiseManager.gameObject))
                    {
                        //AddDebug("can see sub" + __instance.attackAggressionThreshold);
                        if (cyclopsNoiseManager.lightingPanel.lightingOn)
                            noise += .5f;
                        if (cyclopsNoiseManager.lightingPanel.floodlightsOn)
                            noise += .5f;
                        if (noise > 1f)
                            noise = 1f;
                    }
                    float aggrDist = Mathf.Lerp(0f, cyclopsAttackDist * ConfigMenu.aggrMult.Value, noise);

                    if (Vector3.Distance(position, __instance.transform.position) < aggrDist && Vector3.Distance(position, __instance.creature.leashPosition) < __instance.maxDistToLeash)
                    {
                        //target = cyclopsNoiseManager.gameObject;
                        __instance.aggressiveToNoise.Add(__instance.aggressPerSecond * ConfigMenu.aggrMult.Value * .5f);
                        __instance.SetCurrentTarget(cyclopsNoiseManager.gameObject, false);
                    }
                }
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnCollisionEnter")]
            public static bool OnCollisionEnterPrefix(AttackCyclops __instance)
            {
                if (GameModeUtils.IsInvisible() || ConfigMenu.aggrMult.Value == 0)
                {
                    //AddDebug("Main.config.aggrMult == 0f");
                    return false;
                }
                return true;
            }
            [HarmonyPostfix]
            [HarmonyPatch("IsTargetValid")]
            public static void IsTargetValidPrefix(AttackCyclops __instance, IEcoTarget target, ref bool __result)
            {
                //AddDebug("AttackCyclops IsTargetValid " + target.GetGameObject().name);
                __result = Vector3.Distance(target.GetPosition(), __instance.transform.position) < cyclopsAttackDist * ConfigMenu.aggrMult.Value;
            }
            [HarmonyPrefix]
            [HarmonyPatch("Evaluate")]
            public static bool EvaluatePrefix(AttackCyclops __instance, ref float __result)
            {
                if (GameModeUtils.IsInvisible() || ConfigMenu.aggrMult.Value == 0)
                {
                    __result = 0f;
                    return false;
                }
                return true;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("UpdateAttackPoint")]
            public static void UpdateAttackPointPrefix(AttackCyclops __instance)
            {
                //AddDebug("UpdateAttackPoint");
                __instance.targetAttackPoint = Vector3.zero;
                if (__instance.currentTargetIsDecoy || __instance.currentTarget == null)
                    return;
                //__instance.targetAttackPoint.z = Mathf.Clamp(__instance.currentTarget.transform.InverseTransformPoint(__instance.transform.position).z, -26f, 26f);
                __instance.targetAttackPoint.z = Mathf.Clamp(__instance.currentTarget.transform.InverseTransformPoint(__instance.transform.position).z, -26f, 26f);
                //AddDebug("UpdateAttackPoint " + __instance.targetAttackPoint);
                //__instance.targetAttackPoint = Vector3.Lerp(__instance.targetAttackPoint, __instance.transform.position, .01f);
                //AddDebug("UpdateAttackPoint fixed " + __instance.targetAttackPoint);
                //return false;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("OnMeleeAttack")]
            public static void OnMeleeAttackPrefix(AttackCyclops __instance, GameObject target)
            {
                //AddDebug("OnMeleeAttack");
                if (target == __instance.currentTarget)
                    __instance.StopAttack();

                //return false;
            }
            //[HarmonyPostfix]
            //[HarmonyPatch("StopAttack")]
            public static void StopAttackPrefix(AttackCyclops __instance)
            {
                //AddDebug("StopAttack");
            }
        }

        [HarmonyPatch(typeof(AttachToVehicle))]
        class AttachToVehicle_Evaluate_Patch
        {

            //[HarmonyPrefix]
            //[HarmonyPatch("Start")]
            public static void StartPrefix(AttachToVehicle __instance)
            {
                //AddDebug("AttachToVehicle Start " + __instance.name);
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("Evaluate")]
            public static bool EvaluatePrefix(AttachToVehicle __instance, Creature creature, ref float __result, float time)
            {
                if (GameModeUtils.IsInvisible() || ConfigMenu.aggrMult.Value == 0)
                {
                    __result = 0f;
                    return false;
                }
                if (time > __instance.timeNextScan)
                {
                    __instance.UpdateCurrentTarget();
                    __instance.timeNextScan = time + __instance.scanInterval;
                }
                __result = __instance.timeDetached + 4f < time && __instance.currTarget != null && (__instance.currTarget.transform.position - __instance.transform.position).sqrMagnitude <= __instance.currTarget.distanceToStartAction * __instance.currTarget.distanceToStartAction * ConfigMenu.aggrMult.Value ? __instance.GetEvaluatePriority() : 0f;
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("IsValidTarget")]
            public static void IsValidTargetPostfix(AttachToVehicle __instance, IEcoTarget target, ref bool __result)
            {
                if (!__result)
                    return;

                //if (GameModeUtils.IsInvisible() || Main.config.aggrMult == 0f)
                //    __result = false;
                Vehicle vehicle = target.GetGameObject().GetComponent<Vehicle>();
                if (vehicle)
                    __result = CanVehicleBeAttacked(vehicle);
                
            }
        }

        [HarmonyPatch(typeof(MeleeAttack))]
        class MeleeAttack_Patch
        {
            //[HarmonyPostfix]
            //[HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(MeleeAttack __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                AddDebug(tt + " MeleeAttack OnEnable biteAggressionThreshold " + __instance.biteAggressionThreshold);
                AddDebug(tt + " MeleeAttack OnEnable biteInterval " + __instance.biteInterval);
            }

            [HarmonyPrefix]
            [HarmonyPatch("CanDealDamageTo")]
            public static bool CanDealDamageToPrefix(MeleeAttack __instance, GameObject target, ref bool __result)
            { // fix bug: reaper pushes cyclops instead of attacking
                LiveMixin lm = target.GetComponent<LiveMixin>();
                //bool cyclops = target.GetComponent<SubControl>();
                //if (lm && lm.IsAlive())
                {
                    //AddDebug("CanDealDamageTo cyclops !!!");
                    //__result = true;
                    //return false;
                }
                __result = lm && lm.IsAlive();
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("CanBite")]
            public static bool CanBitePrefix(MeleeAttack __instance, GameObject target, ref bool __result)
            {
                //TechType targetTT = CraftData.GetTechType(target);
                //AddDebug(__instance.name + " CanBite Aggression " + (__instance.creature.Aggression.Value < __instance.biteAggressionThreshold));
                //AddDebug(__instance.name + " CanBite timeLastBite " + (Time.time < __instance.timeLastBite));
                //AddDebug(__instance.name + " CanBite IsFriendlyTo " + __instance.creature.IsFriendlyTo(target));
                //AddDebug(__instance.name + " CanBite CanDealDamageTo " + __instance.CanDealDamageTo(target));
                if (__instance.frozen || __instance.creature.IsFriendlyTo(target) || !__instance.CanDealDamageTo(target))
                {
                    __result = false;
                    return false;
                }
                if (__instance.creature.Aggression.Value < __instance.biteAggressionThreshold)
                {
                    __result = false;
                    return false;
                }
                Player player = target.GetComponent<Player>();
                SubRoot subRoot = target.GetComponent<SubRoot>();
                Vehicle vehicle = target.GetComponent<Vehicle>();
                float biteInterval = __instance.biteInterval;
                bool pl = target.GetComponent<Player>();
                if (player || subRoot || vehicle)
                {
                    if (GameModeUtils.IsInvisible() || ConfigMenu.aggrMult.Value == 0)
                    {
                        __result = false;
                        return false;
                    }
                    if (!(__instance is CrabsnakeMeleeAttack))
                    { // crabsnake attack interval is 4
                        //AddDebug("CanBite biteInterval " + biteInterval);
                        float aggrMult = 2f - ConfigMenu.aggrMult.Value;
                        biteInterval *= aggrMult;
                    }
                }
                if (Time.time < __instance.timeLastBite + biteInterval)
                {
                    __result = false;
                    return false;
                }
                if (player)
                {
                    __result = __instance.canBitePlayer && player.CanBeAttacked();
                    return false;
                }
                if (subRoot && subRoot.isCyclops)
                {
                    //AddDebug("MeleeAttack CanBite canBiteCyclops " + __instance.canBiteCyclops);
                    if (ConfigMenu.aggrMult.Value == 0f || !__instance.canBiteCyclops)
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
                    bool inSub = Player.main.currentSub && Player.main.currentSub == subRoot;
                    //AddDebug("inSub " + inSub);
                    if (inSub)
                    {
                        //AddDebug("MeleeAttack CanBite inSub canBiteCyclops " + __instance.canBiteCyclops);
                        __result = __instance.canBiteCyclops;
                        return false;
                    }
                    else
                    {
                        if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Yes)
                        {
                            __result = true;
                            return false;
                        }
                        else if (ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Vanilla || ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.No)
                        {
                            __result = false;
                            return false;
                        }
                        bool lightOn = subRoot.noiseManager.lightingPanel.lightingOn || subRoot.noiseManager.lightingPanel.floodlightsOn;
                        {
                            __result = ConfigMenu.emptyVehiclesCanBeAttacked.Value == ConfigMenu.EmptyVehiclesCanBeAttacked.Only_if_lights_on && lightOn;
                            return false;
                        }
                    }
                }
                if (vehicle)
                {
                    //AddDebug("player.CanBeAttacked() " + Player.main.CanBeAttacked());
                    //bool attackSharks = false;
                    //AggressiveWhenSeeTarget[] awsts = __instance.GetComponents<AggressiveWhenSeeTarget>();
                    //foreach (AggressiveWhenSeeTarget awst in awsts)
                    //{
                    //    if (awst.targetType == EcoTargetType.Shark)
                    //        attackSharks = true;
                    //}
                    if (!__instance.canBiteVehicle)
                    {
                        __result = false;
                        return false;
                    }
                    __result = CanVehicleBeAttacked(vehicle);
                    return false;
                }
                __result = __instance.canBiteCreature && target.GetComponent<Creature>();
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("CanBite")]
            public static void CanBitePostfix(MeleeAttack __instance, GameObject target, bool __result)
            {
                if (target == Player.mainObject)
                {
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    //TechType tt1 = CraftData.GetTechType(target);
                    //if (tt1 == TechType.Cyclops)
                    AddDebug(tt + " MeleeAttack CanBite Player " + __result);
                }
            }

        }

        [HarmonyPatch(typeof(AggressiveToPilotingVehicle), "Start")]
        class AggressiveToPilotingVehicle_Start_Patch
        {
            public static bool Prefix(AggressiveToPilotingVehicle __instance)
            { // only boneshark has this
                //AddDebug("AggressiveToPilotingVehicle Start");
                UnityEngine.Object.Destroy(__instance);
                return false;
            }
        }

        //[HarmonyPatch(typeof(AggressiveWhenSeePlayer))]
        class AggressiveWhenSeePlayer_Patch
        {
            //[HarmonyPrefix]
            //[HarmonyPatch("GetAggressionTarget")]
            public static bool SGetAggressionTargetPrefix(AggressiveWhenSeePlayer __instance)
            {
                return false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnMeleeAttack")]
            public static bool OnMeleeAttackPrefix(AggressiveWhenSeePlayer __instance)
            {
                return false;
            }
        }
        //[HarmonyPatch(typeof(EcoRegionManager), "FindNearestTarget", new Type[] { typeof(EcoTargetType), typeof(Vector3), typeof(float), typeof(EcoRegion.TargetFilter), typeof(int) }, new[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
        class EcoRegionManager_FindNearestTarget_Patch
        {
            static void Prefix(EcoRegionManager __instance, EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float distance, ref int maxRings, ref IEcoTarget __result)
            {
                if (type == EcoTargetType.Shark)
                {
                    if (ConfigMenu.aggrMult.Value == 2f)
                        maxRings = 3;
                    else if (ConfigMenu.aggrMult.Value > 1f)
                        maxRings = 2;
                }
            }
            static void Postfix(EcoRegionManager __instance, EcoTargetType type, Vector3 wsPos, EcoRegion.TargetFilter isTargetValid, ref float distance, int maxRings, ref IEcoTarget __result)
            {
                //if (type == EcoTargetType.Shark && __result != null)
                //    AddDebug("EcoRegionManager FindNearestTarget " + __result.GetName());
            }
        }

    }
}
