using HarmonyLib;
using UnityEngine;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creature_Tweaks
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };


        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                if (Main.config.CreatureFleeChance == 100 && !Main.config.CreatureFleeChanceBasedOnHealth && Main.config.CreatureFleeUseDamageThreshold)
                    return true;
                
                if (!__instance.enabled)
                    return false;

                float damage = damageInfo.damage;
                bool doFlee = false;
                LiveMixin liveMixin = __instance.creature.liveMixin;
                if (Main.config.CreatureFleeChanceBasedOnHealth && liveMixin && liveMixin.IsAlive())
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
                    if (Main.config.CreatureFleeUseDamageThreshold && __instance.accumulatedDamage <= __instance.damageThreshold)
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
        
        [HarmonyPatch(typeof(Stalker), "CheckLoseTooth")]
        public static class Stalker_CheckLoseTooth_Patch
        {
            public static bool Prefix(Stalker __instance, GameObject target)
            { // only scrap metal has HardnessMixin  0.5
                float rndm = UnityEngine.Random.value;
                float stalkerLoseTooth = Main.config.stalkerLoseTooth * .01f;
                if (stalkerLoseTooth >= rndm && HardnessMixin.GetHardness(target) > rndm)
                    __instance.LoseTooth();

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
            }

            //[HarmonyPrefix]
            //[HarmonyPatch("IsInFieldOfView")]
            public static bool IsInFieldOfViewPrefix(Creature __instance, GameObject go, ref bool __result)
            {
                bool canSee = false;
                if (go == null)
                {
                    __result = false;
                    return false;
                }
                if (Mathf.Approximately(__instance.eyeFOV, -1f))
                {
                    //AddDebug(__instance.name + " eyeFOV  = -1");
                    __result = true;
                    return false;
                }
                Vector3 dir = go.transform.position - __instance.transform.position;
                Vector3 rhs = __instance.eyesOnTop ? __instance.transform.up : __instance.transform.forward;
                //if ((Mathf.Approximately(__instance.eyeFOV, -1f) || Vector3.Dot(vector3.normalized, rhs) >= __instance.eyeFOV) && !Physics.Linecast(__instance.transform.position, go.transform.position, Voxeland.GetTerrainLayerMask()))
                //if (go == Player.mainObject)
                //    AddDebug(__instance.name + " eyeFOV " + __instance.eyeFOV + " dir " + dir.normalized + " rhs " + rhs + " Dot " + Vector3.Dot(dir.normalized, rhs));

                if (Vector3.Dot(dir.normalized, rhs) >= __instance.eyeFOV && !Physics.Linecast(__instance.transform.position, go.transform.position))
                    canSee = true;

                //if (go == Player.mainObject && canSee)
                //    AddDebug(__instance.name + " can see player");

                __result = canSee;
                return false;
            }
            [HarmonyPrefix]
            [HarmonyPatch("IsInFieldOfView")]
            public static bool GetCanSeeObjectPrefix(Creature __instance, GameObject go, ref bool __result)
            { // when casting ray from creature to player terrain may not be loaded. Cast from player instead
                __result = false;
                if (go != null)
                {
                    Vector3 vector3 = go.transform.position - __instance.transform.position;
                    Vector3 rhs = __instance.eyesOnTop ? __instance.transform.up : __instance.transform.forward;
                    if ((Mathf.Approximately(__instance.eyeFOV, -1f) || Vector3.Dot(vector3.normalized, rhs) >= __instance.eyeFOV) && !Physics.Linecast(go.transform.position, __instance.transform.position, Voxeland.GetTerrainLayerMask()))
                        __result = true;
                }
                return false;
            }
        }
        
        [HarmonyPatch(typeof(CreatureDeath))]
        class CreatureDeath_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(CreatureDeath __instance)
            {
                if (__instance.GetComponent<Pickupable>()) // fish
                {
                    __instance.respawn = Main.config.fishRespawn;
                    __instance.respawnOnlyIfKilledByCreature = !Main.config.fishRespawnIfKilledByPlayer;
                    if (Main.config.fishRespawnTime > 0)
                        __instance.respawnInterval = Main.config.fishRespawnTime * 1200f;

                    return;
                }
                //AddDebug(__instance.name + " respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
                //AddDebug(__instance.name + " respawn " + __instance.respawn);
                LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                if (!liveMixin)
                    return;

                if (liveMixin.maxHealth >= 3000f) // Leviathan
                {
                    __instance.respawn = Main.config.leviathansRespawn;
                    __instance.respawnOnlyIfKilledByCreature = !Main.config.leviathansRespawnIfKilledByPlayer;
                    if (Main.config.leviathanRespawnTime > 0)
                        __instance.respawnInterval = Main.config.leviathanRespawnTime * 1200f;
                }
                else
                {
                    __instance.respawn = Main.config.creaturesRespawn;
                    __instance.respawnOnlyIfKilledByCreature = !Main.config.creaturesRespawnIfKilledByPlayer;
                    if (Main.config.creatureRespawnTime > 0)
                        __instance.respawnInterval = Main.config.creatureRespawnTime * 1200f;
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnTakeDamage")]
            static void OnTakeDamagePostfix(CreatureDeath __instance, DamageInfo damageInfo)
            {
                //AddDebug(__instance.name + " OnTakeDamage " + damageInfo.dealer.name);
                if (!Main.config.heatBladeCooks && damageInfo.type == DamageType.Heat && damageInfo.dealer == Player.mainObject)
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
            {
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
                if (Main.config.noFishCatching && Util.IsEatableFish(__instance.gameObject) && Util.IsDead(__instance.gameObject))
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

        public static HashSet<GameObject> pickupShinies = new HashSet<GameObject>();

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



    }
}
