using FMOD.Studio;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Creatures
    {
        public static HashSet<TechType> creatureTT = new HashSet<TechType> { };
        public static HashSet<GameObject> pickupShinies = new HashSet<GameObject>();
        public static ConditionalWeakTable<GameObject, Rigidbody> objectsRBs = new ConditionalWeakTable<GameObject, Rigidbody>();
        public static ConditionalWeakTable<SwimBehaviour, string> fishSBs = new ConditionalWeakTable<SwimBehaviour, string>();
        public static ConditionalWeakTable<SwimBehaviour, string> reefbackSBs = new ConditionalWeakTable<SwimBehaviour, string>();
        public static ConditionalWeakTable<SwimBehaviour, string> gasopodSBs = new ConditionalWeakTable<SwimBehaviour, string>();
        public static Color bloodColor;

        [HarmonyPatch(typeof(FleeOnDamage), "OnTakeDamage")]
        class FleeOnDamage_OnTakeDamage_Patch
        {
            public static bool Prefix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                if (ConfigMenu.CreatureFleeChance.Value == 100 && !ConfigMenu.creatureFleeChanceBasedOnHealth.Value && ConfigMenu.creatureFleeUseDamageThreshold.Value)
                    return true;

                if (!__instance.enabled)
                    return false;

                float damage = damageInfo.damage;
                bool doFlee = false;
                LiveMixin liveMixin = __instance.creature.liveMixin;
                if (ConfigMenu.creatureFleeChanceBasedOnHealth.Value && liveMixin && liveMixin.IsAlive())
                {
                    //AddDebug(__instance.name + " max Health " + maxHealth + " Health " + health);
                    if (liveMixin.health < UnityEngine.Random.Range(0, liveMixin.maxHealth))
                    {
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
                    if (ConfigMenu.creatureFleeUseDamageThreshold.Value && __instance.accumulatedDamage <= __instance.damageThreshold)
                        return false;

                    int rnd = UnityEngine.Random.Range(1, 101);
                    if (ConfigMenu.CreatureFleeChance.Value >= rnd)
                        doFlee = true;
                }
                if (doFlee)
                {
                    //if (__instance.gameObject == Testing.goToTest)
                    //    AddDebug(__instance.name + " Flee " + __instance.fleeDuration);
                    __instance.timeToFlee = Time.time + __instance.fleeDuration;
                    __instance.creature.Scared.Add(1f);
                    __instance.creature.TryStartAction(__instance);
                }
                return false;
            }

            public static void Postfix(FleeOnDamage __instance, DamageInfo damageInfo)
            {
                CollectShiny collectShiny = __instance.GetComponent<CollectShiny>();
                if (collectShiny)
                    collectShiny.DropShinyTarget();
            }
        }

        [HarmonyPatch(typeof(Stalker))]
        public static class Stalker_Patch
        {
            [HarmonyPrefix, HarmonyPatch("CheckLoseTooth")]
            public static bool CheckLoseToothPrefix(Stalker __instance, GameObject target)
            { // only scrap metal has HardnessMixin  0.5
                float rndm = UnityEngine.Random.value;
                float stalkerLoseTooth = ConfigMenu.stalkerLoseToothChance.Value * .01f;
                if (stalkerLoseTooth >= rndm && HardnessMixin.GetHardness(target) > rndm)
                    __instance.LoseTooth();

                return false;
            }
            [HarmonyPrefix, HarmonyPatch("LoseTooth")]
            public static void LoseToothPrefix(Stalker __instance, ref bool __result)
            {
                //AddDebug("LoseTooth");
                if (ConfigToEdit.stalkerLooseToothSound.Value == false)
                    __instance.loseToothSound = null;
            }
        }

        [HarmonyPatch(typeof(Creature))]
        public static class Creature_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
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
                creatureTT.Add(tt);
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

            [HarmonyPrefix, HarmonyPatch("IsInFieldOfView")]
            public static bool IsInFieldOfViewPrefix(Creature __instance, GameObject go, ref bool __result)
            { // ray does not hit terrain if cast from underneath. Cast from player to avoid it.
                if (Main.aggressiveFaunaLoaded)
                    return true;

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

            [HarmonyPostfix, HarmonyPatch("OnKill")]
            public static void OnKillPostfix(Creature __instance)
            {
                if (__instance is Peeper)
                {
                    //AddDebug("Peeper OnKill ");
                    FixPeeperLOD(__instance, false);
                }
            }

            [HarmonyPostfix, HarmonyPatch("OnDrop")]
            public static void OnDropPostfix(Creature __instance)
            {
                if (__instance is Peeper)
                {
                    //AddDebug("Peeper OnDrop ");
                    FixPeeperLOD(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(ReefbackLife), "OnEnable")]
        class ReefbackLife_OnEnable_patch
        {
            public static void Postfix(ReefbackLife __instance)
            { // make it avoid player life pod
                //AddDebug(" ReefbackLife OnEnable " + (int)__instance.transform.position.y);
                AvoidObstacles ao = __instance.gameObject.GetComponent<AvoidObstacles>();
                if (ao == null)
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
                if (ConfigMenu.noFishCatching.Value == false || Player.main._currentWaterPark || Util.IsEatableFish(__instance.gameObject) == false || Util.IsDead(__instance.gameObject))
                {
                    return;
                }
                __result = false;
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
                if (rigidbody == null)
                    return;

                foreach (Rigidbody rb in Tools_.stasisTargets)
                {
                    if (rigidbody == rb)
                    {
                        __result = true;
                    }
                }

            }
        }

        [HarmonyPatch(typeof(SwimBehaviour), "SwimToInternal")]
        class SwimBehaviour_SwimToInternal_patch
        {
            public static void Prefix(SwimBehaviour __instance, ref float velocity, ref Vector3 targetPosition)
            {
                if (fishSBs.TryGetValue(__instance, out string s) || Util.IsEatableFish(__instance.gameObject))
                {
                    if (ConfigMenu.fishSpeedMult.Value != 1)
                        velocity *= ConfigMenu.fishSpeedMult.Value;

                    if (s == null)
                        fishSBs.Add(__instance, "");
                }
                else
                {
                    velocity *= ConfigMenu.creatureSpeedMult.Value;
                    if (gasopodSBs.TryGetValue(__instance, out string ss) && targetPosition.y > -1f)
                    {
                        targetPosition.y = UnityEngine.Random.Range(-11, -1);
                        return;
                    }
                    else if (reefbackSBs.TryGetValue(__instance, out string sss) && targetPosition.y > -15f)
                    {
                        targetPosition.y = -15f;
                        return;
                    }
                    TechType tt = CraftData.GetTechType(__instance.gameObject);
                    if (tt == TechType.Reefback && targetPosition.y > -15f)
                    { // dont allow them to surface
                        //AddDebug("Fix reefback y pos");
                        targetPosition.y = -15f;
                        reefbackSBs.Add(__instance, "");
                    }
                    else if (tt == TechType.Gasopod && targetPosition.y > -1f)
                    {
                        targetPosition.y = UnityEngine.Random.Range(-11, 0);
                        //AddDebug("Gasopod Swim To " + targetPosition.y);
                        gasopodSBs.Add(__instance, "");
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
                    if (!ConfigToEdit.stalkersGrabShinyTool.Value && gameObject.GetComponentInParent<Player>())
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
                    if (ConfigToEdit.stalkersGrabShinyTool.Value && Player.main.currentSub == null)
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
            { // not fixed
                VFXSurface vFXSurface = __instance.gameObject.EnsureComponent<VFXSurface>();
                vFXSurface.surfaceType = VFXSurfaceTypes.organic;
            }
        }

        [HarmonyPatch(typeof(GasoPod), "Update")]
        class GasoPod_Update_patch
        {
            public static bool Prefix(GasoPod __instance)
            {
                if (ConfigToEdit.stasisRifleTweaks.Value == false)
                    return true;

                Rigidbody rb;
                if (objectsRBs.TryGetValue(__instance.gameObject, out rb))
                {
                    //Rigidbody rb = objectsRBs[__instance.gameObject];
                    if (rb && Tools_.stasisTargets.Contains(rb))
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

                if (ConfigToEdit.stasisRifleTweaks.Value == false)
                    return true;

                Rigidbody rb;
                if (objectsRBs.TryGetValue(__instance.gameObject, out rb))
                {
                    //Rigidbody rb = objectsRBs[__instance.gameObject];
                    if (rb && Tools_.stasisTargets.Contains(rb))
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
                if (!ConfigMenu.waterparkCreaturesBreed.Value)
                    __result = false;
            }
        }

        public static void FixPeeperLOD(Creature peeper, bool alive = true)
        {// fix: they close eyes when near player
            if (alive)
                alive = peeper.GetComponent<LiveMixin>().IsAlive();

            Transform tr = peeper.transform.Find("model/peeper");
            LODGroup lODGroup = tr.GetComponentInChildren<LODGroup>(true);
            lODGroup.enabled = false;
            Transform tr1 = tr.Find("aqua_bird");
            tr1.gameObject.SetActive(!alive);
            tr1 = tr.Find("aqua_bird_LOD1");
            tr1.gameObject.SetActive(alive);
        }

        [HarmonyPatch(typeof(Peeper), "Start")]
        class Peeper_Start_patch
        {
            public static void Postfix(Peeper __instance)
            {
                FixPeeperLOD(__instance);
            }
        }



    }
}
