using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creature_Tweaks
    {
        public static HashSet<TechType> silentCreatures = new HashSet<TechType> { };

        [HarmonyPatch(typeof(CreatureEgg), "Awake")]
        class CreatureEgg_Awake_Patch
        {
            public static void Postfix(CreatureEgg __instance)
            {
                __instance.explodeOnHatch = false;
                LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                if (liveMixin && liveMixin.data)
                {
                    liveMixin.data.destroyOnDeath = true;
                    liveMixin.data.explodeOnDestroy = false;
                }
            }
        }

        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Postfix_Patch
        {
            private static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                //int health = (int)__instance.creature.liveMixin?.health; 
                //Main.Message("creaturesDontFlee " + Main.config.creaturesDontFlee);
                //if (Main.config.predatorsDontFlee)
                {
                    LiveMixin liveMixin = __instance.creature.liveMixin;
                    AggressiveWhenSeeTarget awst = __instance.GetComponent<AggressiveWhenSeeTarget>();
                    if (liveMixin && awst && liveMixin.IsAlive())
                    { //  && damageInfo.dealer == Player.main
                        //if (damageInfo.dealer)
                        //  Main.Message("damage dealer " + damageInfo.dealer.name);
                        int maxHealth = Mathf.RoundToInt(liveMixin.maxHealth);
                        //int halfMaxHealth = Mathf.RoundToInt(liveMixin.maxHealth * .5f);
                        int rnd = Main.rndm.Next(1, maxHealth);
                        //float aggrMult = Mathf.Clamp(Main.config.aggrMult, 0f, 2f);
                        int health = Mathf.RoundToInt(liveMixin.health * Main.config.aggrMult);
                        //if (health > halfMaxHealth || rnd < health)
                        if (health > rnd)
                        {
                            damageInfo.damage = 0f;
                            //Main.Message("health " + liveMixin.health + " rnd100 " + rnd100);
                        }
                        if (Main.config.aggrMult == 3f)
                            damageInfo.damage = 0f;

                        return false;
                    }
                }
                return true;
            }
            public static void Postfix(FleeOnDamage __instance, DamageInfo damageInfo)
            { // game code makes them always flee to 0 0 0 
                //AddDebug(__instance.name + " FleeOnDamage OnTakeDamage " + damageInfo.damage);
                __instance.moveTo = __instance.swimBehaviour.originalTargetPosition * damageInfo.damage;
                //__instance.timeToFlee = Time.time;
                //if (damageInfo.type == DamageType.Heat)
                //{
                //TechType techType = CraftData.GetTechType(__instance.gameObject);
                //string name = Language.main.Get(techType);
                //float magnitude = (__instance.transform.position - Player.main.transform.position).magnitude;
                //if (damageInfo.damage == 0 && magnitude < 5)
                //{
                //    LiveMixin liveMixin = __instance.creature.liveMixin;
                //AddDebug(name + " maxHealth " + liveMixin.maxHealth + " Health " + liveMixin.health);
                //}
                //Main.Message(name + " originalTargetPosition " + __instance.swimBehaviour.originalTargetPosition);
                //Main.Message(name + " moveTo " + __instance.moveTo);
                //}
                CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                if (collectShiny)
                {
                    //Main.Message("Stalker DropShinyTarget");
                    collectShiny.DropShinyTarget();
                }
            }
        }

        [HarmonyPatch(typeof(Stalker), "CheckLoseTooth")]
        public static class Stalker_CheckLoseTooth_Patch
        {
            public static bool Prefix(Stalker __instance, GameObject target)
            { // In vanilla only scrap metal has HardnessMixin.  0.5
                float rndm = Random.value;
                if (Main.config.stalkerLoseTooth >= rndm && HardnessMixin.GetHardness(target) > rndm)
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
                {
                    //AddDebug("Spadefish");
                    __instance.GetComponent<Rigidbody>().mass = 4f;
                }
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (silentCreatures.Contains(tt))
                {
                    //AddDebug("silent " + tt);
                    foreach (FMOD_StudioEventEmitter componentsInChild in __instance.GetComponentsInChildren<FMOD_StudioEventEmitter>())
                        //Object.Destroy(componentsInChild); // crashfish does not attack, null raf exception for crabsnake
                        componentsInChild.enabled = false; // does not work for crashfish, sandshark
                    foreach (FMOD_CustomEmitter componentsInChild in __instance.GetComponentsInChildren<FMOD_CustomEmitter>())
                        //Object.Destroy(componentsInChild);
                        componentsInChild.enabled = false;
                }

                //InfectedMixin infectedMixin = __instance.GetComponent<InfectedMixin>();
                //if (infectedMixin && infectedMixin.infectedAmount == 1f)
                //{
                //    AddDebug("Infected " + tt);
                //    Vector3 pos = __instance.transform.position;
                //    GameObject beaconPrefab = CraftData.GetPrefabForTechType(TechType.Beacon);
                //    GameObject prefab = Object.Instantiate(beaconPrefab, pos, Quaternion.identity);
                //    LargeWorldEntity.Register(prefab);
                //    Beacon beacon = prefab.GetComponent<Beacon>();
                //    if (beacon)
                //        beacon.label = tt.AsString();
                //}
            }

            [HarmonyPostfix]
            [HarmonyPatch("OnKill")]
            public static void OnKillPostfix(Creature __instance)
            {
                //AddDebug("respawnOnlyIfKilledByCreature " + __instance.respawnOnlyIfKilledByCreature);
                Animator animator = __instance.GetAnimator();
                if (animator == null)
                    return;

                SafeAnimator.SetBool(animator, "attacking", false);

                if (__instance is Stalker)
                {
                    //AnimateByVelocity animByVelocity = __instance.GetComponentInChildren<AnimateByVelocity>();
                    animator.enabled = false;
                    //animator.SetFloat(AnimateByVelocity.animSpeed, 0.0f);
                    //animator.SetFloat(AnimateByVelocity.animPitch, 0.0f);
                    //animator.SetFloat(AnimateByVelocity.animTilt, 0.0f);
                    //CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                    //collectShiny?.DropShinyTarget();
                }
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
                    if (Main.config.fishRespawnTime > 0)
                        __instance.respawnInterval = Main.config.fishRespawnTime * 1200f;
                }
                else
                {
                    LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                    if (liveMixin)
                    {
                        if (liveMixin.maxHealth >= 3000f) // Leviathan
                        {
                            if (Main.config.leviathanRespawnTime > 0)
                                __instance.respawnInterval = Main.config.leviathanRespawnTime * 1200f;

                            if (Main.config.creatureRespawn == Config.CreatureRespawn.Leviathans_only || Main.config.creatureRespawn == Config.CreatureRespawn.Big_creatures_and_leviathans)
                                __instance.respawnOnlyIfKilledByCreature = false;
                        }
                        else
                        {
                            if (Main.config.creatureRespawnTime > 0)
                                __instance.respawnInterval = Main.config.creatureRespawnTime * 1200f;

                            if (Main.config.creatureRespawn == Config.CreatureRespawn.Big_creatures_and_leviathans || Main.config.creatureRespawn == Config.CreatureRespawn.Big_creatures_only)
                                __instance.respawnOnlyIfKilledByCreature = false;
                        }
                    }
                }
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnTakeDamage")]
            static void OnTakeDamagePostfix(CreatureDeath __instance)
            {
                if (!Main.config.heatBladeCooks)
                    __instance.lastDamageWasHeat = false;
            }
            //[HarmonyPrefix]
            //[HarmonyPatch("OnKill")]
            static void OnKillPrefix(CreatureDeath __instance)
            {
                //AddDebug(__instance.name + " OnKill");
            }
            [HarmonyPostfix]
            [HarmonyPatch("OnKill")]
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

        [HarmonyPatch(typeof(SeaTreaderSounds), nameof(SeaTreaderSounds.OnStep))]
        class SeaTreaderSounds_OnStep_patch
        { 
            public static bool Prefix(SeaTreaderSounds __instance, Transform legTr, AnimationEvent animationEvent)
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
        }

        [HarmonyPatch(typeof(SeaTreaderSounds), nameof(SeaTreaderSounds.OnStomp))]
        class SeaTreaderSounds_OnStomp_patch
        { 
            public static bool Prefix(SeaTreaderSounds __instance)
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
                if (Main.config.noFishCatching && Main.IsEatableFishAlive(__instance.gameObject))
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

        [HarmonyPatch(typeof(SwimBehaviour))]
        class SwimBehaviour_SwimToInternal_patch
        {
            [HarmonyPatch(nameof(SwimBehaviour.SwimToInternal))]
            public static void Prefix(SwimBehaviour __instance, ref float velocity, ref Vector3 targetPosition)
            {
                if (Main.IsEatableFish(__instance.gameObject))
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
                    if (maxDistance > 0f && Physics.Raycast(__instance.transform.position, direction, maxDistance, Voxeland.GetTerrainLayerMask()))
                        gameObject = null;
                }
                if (__instance.shinyTarget == gameObject || gameObject == null ||gameObject.GetComponent<Rigidbody>() == null || gameObject.GetComponent<Pickupable>() == null )
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
                __instance.SendMessage("OnShinyPickUp", __instance.shinyTarget, SendMessageOptions.DontRequireReceiver);
                __instance.shinyTarget.gameObject.SendMessage("OnShinyPickUp", __instance.gameObject, SendMessageOptions.DontRequireReceiver);
                UWE.Utils.SetCollidersEnabled(__instance.shinyTarget, false);
                __instance.shinyTarget.transform.parent = __instance.shinyTargetAttach;
                __instance.shinyTarget.transform.localPosition = Vector3.zero;
                __instance.targetPickedUp = true;
                UWE.Utils.SetIsKinematic(__instance.shinyTarget.GetComponent<Rigidbody>(), true);
                UWE.Utils.SetEnabled(__instance.shinyTarget.GetComponent<LargeWorldEntity>(), false);
                __instance.SendMessage("OnShinyPickedUp", __instance.shinyTarget, SendMessageOptions.DontRequireReceiver);
                __instance.swimBehaviour.SwimTo(__instance.transform.position + Vector3.up * 5f + Random.onUnitSphere, Vector3.up, __instance.swimVelocity);
                __instance.timeNextSwim = Time.time + 1f;
                BehaviourUpdateUtils.Register(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(FPModel), "OnEquip")]
        class FPModel_OnEquip_Patch
        {
            static void Postfix(FPModel __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                if (tt != TechType.Oculus)
                    return;
                LiveMixin lm = __instance.GetComponent<LiveMixin>();
                if (lm && lm.IsAlive())
                {
                    //AddDebug("Oculus FPModel OnEquip");
                    __instance.transform.localPosition = new Vector3(0, -.05f, .04f);
                }
            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "Evaluate")]
        class FleeOnDamage_Evaluate_Patch
        {
            public static void Postfix(FleeOnDamage __instance, Creature creature)
            {
                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string name = Language.main.Get(techType);
                //Creature_Loot_Drop.Creature_Loot Crloot = __instance.GetComponent<Creature_Loot_Drop.Creature_Loot>();
                //if (Crloot)
                //{
                //Main.Message("Stalker DropShinyTarget");
                //if (Time.time < __instance.timeToFlee)
                //{
                //Main.Message(name + " FleeOnDamage GetEvaluatePriority " + __instance.GetEvaluatePriority());
                //}
                //else
                //    Main.Message(name + " FleeOnDamage Evaluate " + 0);
                //}
            }
        }

        //[HarmonyPatch(typeof(FleeOnDamage), "StopPerform")]
        class FleeOnDamage_StopPerform_Patch
        {
            public static void Postfix(FleeOnDamage __instance, Creature creature)
            {

                TechType techType = CraftData.GetTechType(__instance.gameObject);
                string name = Language.main.Get(techType);
                //Main.Message(name + " Stop Perform ");
            }
        }
    }
}
