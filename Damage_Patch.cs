using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    class Damage_Patch
    {
        public static float healTempDamageTime = 0;
        static float poisonDamageInterval = 1f;
        static float poisonDamage = .5f;
        public static List<LiveMixin> tempDamageLMs = new List<LiveMixin>();

        static public Dictionary<TechType, float> damageMult = new Dictionary<TechType, float>();
        static void SetBloodColor(GameObject go)
        {   // GenericCreatureHit(Clone)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(0.784, 1.000, 0.157, 0.588)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(1.000, 0.925, 0.333, 1.000)
            // xKnifeHit_Organic(Clone)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(0.784, 1.000, 0.157, 0.392)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            // RGBA(0.784, 1.000, 0.157, 1.000)
            ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
            //AddDebug("SetBloodColor " + go.name + " " + pss.Length);
            //Main.Log("SetBloodColor " + go.name );
            foreach (ParticleSystem ps in pss)
            {
                //ps.startColor = new Color(1f, 0f, 0f);
                ParticleSystem.MainModule psMain = ps.main;
                //Main.Log("startColor " + psMain.startColor.color);
                Color newColor = new Color(Main.config.bloodColor["Red"], Main.config.bloodColor["Green"], Main.config.bloodColor["Blue"], psMain.startColor.color.a);
                psMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
            }
        }

        [HarmonyPatch(typeof(DealDamageOnImpact))]
        class DealDamageOnImpact_patch
        {
            static Rigidbody prevColTarget;

            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(DealDamageOnImpact __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug(" DealDamageOnImpact start " +__instance.gameObject.name);
                //foreach (GameObject go in __instance.exceptions)
                //{
                //AddDebug(tt + " exception " + go.name);
                //}
                if (tt == TechType.Gasopod)
                {
                    UnityEngine.Object.Destroy(__instance);
                }
                else if (tt == TechType.Seamoth || tt == TechType.Exosuit || tt == TechType.Cyclops)
                {
                    __instance.exceptions.Add(Player.main.gameObject);
                    //AddDebug(tt + " exception " );
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnCollisionEnter")]
            public static bool OnCollisionEnterPrefix(DealDamageOnImpact __instance, Collision collision)
            {
                if (!__instance.enabled || collision.contacts.Length == 0 || __instance.exceptions.Contains(collision.gameObject))
                    return false;
                float damageMult = Mathf.Max(0f, Vector3.Dot(-collision.contacts[0].normal, __instance.prevVelocity));
                //AddDebug(" collision " + collision.gameObject.name);
                float colMag = collision.relativeVelocity.magnitude;
                if (colMag <= __instance.speedMinimumForDamage)
                    return false;
                LiveMixin targetLM = __instance.GetLiveMixin(collision.gameObject);
                Vector3 position = collision.contacts.Length == 0 ? collision.transform.position : collision.contacts[0].point;
                Rigidbody rb = Utils.FindAncestorWithComponent<Rigidbody>(collision.gameObject);
                float targetMass = rb != null ? rb.mass : 5000f;
                float myMass = __instance.GetComponent<Rigidbody>().mass;
                float colMult = Mathf.Clamp((1f + (myMass - targetMass) * 0.001f), 0f, damageMult);
                float targetDamage = colMag * colMult;

                if (targetLM && targetLM.IsAlive() && Time.time > __instance.timeLastDamage + __instance.minDamageInterval)
                {
                    bool skip = false;
                    if (prevColTarget == rb && Time.time < __instance.timeLastDamage + 3f)
                        skip = true;
                    if (!skip)
                    {
                        //AddDebug("myMass " + myMass + " targetDamage " + (int)targetDamage);
                        targetLM.TakeDamage(targetDamage, position, DamageType.Collide, __instance.gameObject);
                        __instance.timeLastDamage = Time.time;
                        prevColTarget = rb;
                    }
                }

                if (!__instance.mirroredSelfDamage || colMag < __instance.speedMinimumForSelfDamage)
                    return false;

                LiveMixin myLM = __instance.GetLiveMixin(__instance.gameObject);
                bool tooSmall = rb && rb.mass <= __instance.minimumMassForDamage;

                if (__instance.mirroredSelfDamageFraction == 0f || !myLM || Time.time <= __instance.timeLastDamagedSelf + 1f || tooSmall)
                    return false;
                //AddDebug("minimumMassForDamage " + __instance.minimumMassForDamage + " mass " + rb.mass);
                //float myDamage = targetDamage * __instance.mirroredSelfDamageFraction;

                float myDamage = colMag * Mathf.Clamp((1f + (targetMass - myMass) * 0.001f), 0f, damageMult);
                //AddDebug("mass " + targetMass + " myDamage " + (int)myDamage);
                //AddDebug(" maxHealth " + myLM.maxHealth + " health " + myLM.health);
                if (__instance.capMirrorDamage != -1f) // cyclops is immune to collision damage
                    myDamage = Mathf.Min(__instance.capMirrorDamage, myDamage);
                myLM.TakeDamage(myDamage, position, DamageType.Collide, __instance.gameObject);
                __instance.timeLastDamagedSelf = Time.time;

                return false;
            }
        }

        [HarmonyPatch(typeof(VFXDestroyAfterSeconds), "OnEnable")]
        class VFXDestroyAfterSeconds_OnEnable_Patch
        {
            public static void Prefix(VFXDestroyAfterSeconds __instance)
            {// particles from GenericCreatureHit play on awake
                //AddDebug("GenericCreatureHit OnEnable " + __instance.gameObject.name);
                if (__instance.gameObject.name == "GenericCreatureHit(Clone)")
                {
                    //AddDebug("GenericCreatureHit OnEnable");
                    //setBloodColor = true;
                    SetBloodColor(__instance.gameObject);
                }
            }
        }

        [HarmonyPatch(typeof(VFXSurfaceTypeManager), "Play", new Type[] { typeof(VFXSurfaceTypes), typeof(VFXEventTypes), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
        class VFXSurfaceTypeManager_Play_Patch
        {
            static bool Prefix(VFXSurfaceTypeManager __instance, ref ParticleSystem __result, VFXSurfaceTypes surfaceType, VFXEventTypes eventType, Vector3 position, Quaternion orientation, Transform parent)
            {
                //ProfilingUtils.BeginSample("VFXSurfaceTypeManager.Play");
                ParticleSystem particleSystem = null;
                GameObject fxprefab = __instance.GetFXprefab(surfaceType, eventType);
                if (fxprefab != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(fxprefab, position, orientation);
                    if (eventType == VFXEventTypes.exoDrill)
                    {
                        gameObject.transform.parent = null;
                        gameObject.GetComponent<VFXFakeParent>().Parent(parent, Vector3.zero, Vector3.zero);
                        gameObject.GetComponent<VFXLateTimeParticles>().Play();
                        particleSystem = gameObject.GetComponent<ParticleSystem>();
                    }
                    else
                    {
                        gameObject.transform.parent = parent;
                        if (surfaceType == VFXSurfaceTypes.organic)
                        {
                            SetBloodColor(gameObject);
                        }
                        particleSystem = gameObject.GetComponent<ParticleSystem>();
                        particleSystem.Play();
                    }
                }
                //particleSystem.startColor = new Color(1f, 1f, 1f);
                //ProfilingUtils.EndSample();
                __result = particleSystem;
                return false;
            }
        }

        [HarmonyPatch(typeof(Knife), "OnToolUseAnim")]
        class Knife_OnToolUseAnim_Patch
        {
            public static bool Prefix(Knife __instance, GUIHand hand)
            {
                Vector3 position = new Vector3();
                GameObject closestObj = null;
                UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, __instance.attackDist, ref closestObj, ref position);
                if (closestObj == null)
                {
                    InteractionVolumeUser ivu = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
                    if (ivu != null && ivu.GetMostRecent() != null)
                        closestObj = ivu.GetMostRecent().gameObject;
                }
                if (closestObj)
                {
                    LiveMixin liveMixin = closestObj.FindAncestor<LiveMixin>();
                    if (Knife.IsValidTarget(liveMixin))
                    {
                        if (liveMixin)
                        {
                            bool wasAlive = liveMixin.IsAlive();
                            liveMixin.TakeDamage(__instance.damage, position, __instance.damageType, Player.main.gameObject);
                            __instance.GiveResourceOnDamage(closestObj, liveMixin.IsAlive(), wasAlive);
                        }
                        Utils.PlayFMODAsset(__instance.attackSound, __instance.transform);
                        VFXSurface vfxSurface = closestObj.GetComponent<VFXSurface>();
                        Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                        //setBloodColor = true;
                        ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, __instance.vfxEventType, position, Quaternion.Euler(euler), Player.main.transform);
                        //particleSystem.startColor = new Color(1f, 1f, 1f);
                        //particleSystem.main.
                    }
                    else
                        closestObj = null;
                }
                if (closestObj != null || hand.GetActiveTarget() != null)
                    return false;

                if (Player.main.IsUnderwater())
                    Utils.PlayFMODAsset(__instance.underwaterMissSound, __instance.transform);
                else
                    Utils.PlayFMODAsset(__instance.surfaceMissSound, __instance.transform);

                return false;
            }
        }

        [HarmonyPatch(typeof(LiveMixin))]
        class LiveMixin_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            static void StartPostfix(LiveMixin __instance)
            {
                //__instance.onHealTempDamage = LiveMixin.floatEventPool.Get();
                //if (__instance.data.deathEffect)
                //    Main.Log("deathEffect " + __instance.data.deathEffect);
                if (Main.config.noKillParticles)
                { 
                    //__instance.data.damageEffect = null;
                    __instance.data.deathEffect = null;
                }
                if (!Main.loadingDone)
                { // __instance.tempDamage is -1
                    tempDamageLMs.Add(__instance);
                    //AddDebug("tempDamage " + __instance.tempDamage);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            static bool TakeDamagePrefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position = default(Vector3), DamageType type = DamageType.Normal, GameObject dealer = null)
            {
                //if (dealer)
                //    AddDebug("dealer " + dealer.name);
                bool killed = false;
                bool creativeMode = GameModeUtils.IsInvisible() && __instance.invincibleInCreative;
                if (__instance.health > 0f && !__instance.invincible && !creativeMode)
                {
                    float damage = 0f;
                    if (!__instance.shielded)
                    {
                        if (dealer == Player.mainObject)
                        {
                            //if (type == DamageType.Normal)
                            //{
                            //    Stalker stalker = __instance.GetComponent<Stalker>();
                            //    if (stalker)
                            //    {
                            //        LiveMixin lm = __instance.GetComponent<LiveMixin>();
                            //        if (lm && !lm.IsAlive())
                            //        {
                                        //AddDebug("Stalker TakeDamage ");
                                        //stalker.LoseTooth();
                                //    }
                                //}
                            //}
                            if (type == DamageType.Heat && __instance.GetComponent<LavaLizard>())
                                type = DamageType.Normal;
                        }

                        damage = DamageSystem.CalculateDamage(originalDamage, type, __instance.gameObject, dealer);
                        //if (dealer == Player.mainObject)
                        //    AddDebug("TakeDamage originalDamage " + originalDamage + " damage " + damage);
                    }
                    if (type != DamageType.Poison && type != DamageType.Cold)
                        __instance.health = Mathf.Max(0f, __instance.health - damage);
                    else 
                    {
                        if (Main.config.newPoisonSystem)
                        {
                            //AddDebug(" new Poison System " );
                            if (dealer != __instance.gameObject)
                            {
                                __instance.tempDamage += damage;
                                __instance.SyncUpdatingState();
                                //if (__instance.gameObject == Player.mainObject)
                                //    AddDebug("TakeDamage tempDamage +" + __instance.tempDamage);
                            }
                            else
                                __instance.health = Mathf.Max(0f, __instance.health - damage);
                        }
                        else
                        {
                            //AddDebug("old Poison System ");
                            __instance.health = Mathf.Max(0f, __instance.health - damage);
                            __instance.tempDamage += damage;
                            //AddDebug(__instance.name + " tempDamage " + __instance.tempDamage);
                            __instance.SyncUpdatingState();
                        }
                    }
                    __instance.damageInfo.Clear();
                    __instance.damageInfo.originalDamage = originalDamage;
                    __instance.damageInfo.damage = damage;
                    __instance.damageInfo.position = position == new Vector3() ? __instance.transform.position : position;
                    __instance.damageInfo.type = type;
                    __instance.damageInfo.dealer = dealer;
                    __instance.NotifyAllAttachedDamageReceivers(__instance.damageInfo);
                    if (__instance.shielded)
                    {
                        __result = killed;
                        return false;
                    }
                    if (__instance.damageClip && damage > 0f && (damage >= __instance.minDamageForSound && type != DamageType.Radiation))
                        Utils.PlayEnvSound(__instance.damageClip, __instance.damageInfo.position);

                    if (__instance.loopingDamageEffect && !__instance.loopingDamageEffectObj && __instance.GetHealthFraction() < __instance.loopEffectBelowPercent)
                    {
                        //__instance.loopingDamageEffectObj = UWE.Utils.InstantiateWrap(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj = UnityEngine.Object.Instantiate<GameObject>(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj.transform.parent = __instance.transform;
                    }
                    //GameObject damageEffect = __instance.damageEffect;
                    if (Time.time > __instance.timeLastElecDamageEffect + 2.5f && type == DamageType.Electrical && __instance.electricalDamageEffect != null)
                    {
                        FixedBounds fixedBounds = __instance.gameObject.GetComponent<FixedBounds>();
                        Bounds bounds = fixedBounds == null ? UWE.Utils.GetEncapsulatedAABB(__instance.gameObject) : fixedBounds.bounds;
                        //GameObject electricalDamageEffect = UWE.Utils.InstantiateWrap(__instance.electricalDamageEffect, bounds.center, Quaternion.identity);
                        GameObject electricalDamageEffect = UnityEngine.Object.Instantiate<GameObject>(__instance.electricalDamageEffect, bounds.center, Quaternion.identity);
                        electricalDamageEffect.transform.parent = __instance.transform;
                        electricalDamageEffect.transform.localScale = bounds.size * 0.65f;
                        __instance.timeLastElecDamageEffect = Time.time;
                    }
                    else if (Time.time > __instance.timeLastDamageEffect + 1f && damage > 0f && dealer != Player.main.gameObject && type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter || type == DamageType.Drill)
                    { // dont spawn damage particles if knifed by player
                        VFXSurface vfxSurface = __instance.GetComponentInChildren<VFXSurface>();
                        if (vfxSurface)
                        {
                            //AddDebug("Spawn vfxSurface Prefab ");
                            //Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                            //setBloodColor = true;
                            ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, VFXEventTypes.knife, position, Quaternion.identity, Player.main.transform);
                            __instance.timeLastDamageEffect = Time.time;
                        }
                        else if (__instance.damageEffect)
                        {
                            //AddDebug("Spawn damageEffect Prefab " + __instance.damageEffect.name);
                            GameObject go = Utils.SpawnPrefabAt(__instance.damageEffect, __instance.transform, __instance.damageInfo.position);
                            //setBloodColor = true;
                            if (__instance.GetComponent<Creature>())
                            {
                                SetBloodColor(go);
                            }
                            __instance.timeLastDamageEffect = Time.time;
                        }
                    }
                    if (__instance.health <= 0f || !Main.config.newPoisonSystem && __instance.health - __instance.tempDamage <= 0f)
                    {
                        killed = true;
                        if (!__instance.IsCinematicActive())
                            __instance.Kill(type);
                        else
                        {
                            __instance.cinematicModeActive = true;
                            __instance.SyncUpdatingState();
                        }
                    }
                }
                __result = killed;
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch(nameof(LiveMixin.SyncUpdatingState))]
            static void ManagedUpdatePostfix(LiveMixin __instance)
            {
                if (__instance.gameObject == Player.mainObject)
                {
                    AddDebug("SyncUpdatingState ");
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("TakeDamage")]
            static void TakeDamagePostfix(LiveMixin __instance, bool __result, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (__instance.gameObject.GetComponent<SubControl>())
                {
                    AddDebug(__instance.name + " TakeDamage Postfix " + originalDamage + " " + type);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("HealTempDamage")]
            static bool HealTempDamagePrefix(LiveMixin __instance, float timePassed)
            {
                if (Main.config.newPoisonSystem)
                {
                    if (__instance.tempDamage > 0 && healTempDamageTime < Time.time)
                    {
                        bool damaged = false;
                        if (__instance.gameObject == Player.mainObject)
                        {
                            //AddDebug("HealTempDamage tempDamage " + __instance.tempDamage);
                            int foodMin = Main.config.newHungerSystem ? -99 : 1;
                            float damage = 0f;
                            Survival survival = Main.survival;
                            if (survival.food > foodMin)
                                survival.food -= poisonDamage;
                            else
                                damage += poisonDamage;

                            if (survival.water > foodMin)
                                survival.water -= poisonDamage;
                            else
                                damage += poisonDamage;

                            //if (damage > 0)
                                __instance.TakeDamage(damage + poisonDamage, __instance.transform.position, DamageType.Poison, __instance.gameObject);
                            damaged = true;
                        }
                        //if (__instance.gameObject == Player.mainObject)
                        //    AddDebug("HealTempDamage tempDamage " + __instance.tempDamage);
                        if (!damaged)
                            __instance.TakeDamage(poisonDamage, __instance.transform.position, DamageType.Poison, __instance.gameObject);

                        __instance.tempDamage -= poisonDamage;
                        if (__instance.tempDamage > 0)
                            healTempDamageTime = Time.time + poisonDamageInterval;
                        else if (__instance.tempDamage < 0)
                            __instance.tempDamage = 0;

                        if (__instance.tempDamage == 0)
                            __instance.SyncUpdatingState();
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                if (__result > 0f)
                {
                    Vehicle vehicle = target.GetComponent<Vehicle>();
                    if (target == Player.mainObject)
                    {
                        //AddDebug("Player takes damage");
                        __result *= Main.config.playerDamageMult;
                        if (__result == 0f)
                            return;

                        if (Main.config.dropHeldTool)
                        {
                            if (type != DamageType.Cold && type != DamageType.Poison && type != DamageType.Starve && type != DamageType.Radiation && type != DamageType.Pressure)
                            {
                                int rnd = Main.rndm.Next(1, (int)Player.main.liveMixin.maxHealth);
                                if (rnd < damage)
                                {
                                    //AddDebug("DropHeldItem");
                                    Inventory.main.DropHeldItem(true);
                                }
                            }
                        }
                        //if (Main.config.newPoisonSystem && type == DamageType.Poison)
                        //{
                        //}
                    }
                    else if (vehicle)
                    {
                        //AddDebug("Vehicle takes damage");
                        if (type == DamageType.Normal || type == DamageType.Pressure || type == DamageType.Collide || type ==  DamageType.Explosive || type ==  DamageType.Puncture)
                        {
                            int armorUpgrades = 0;
                            for (int i = 0; i < vehicle.slotIDs.Length; ++i)
                            {
                                TechType tt = vehicle.modules.GetTechTypeInSlot(vehicle.slotIDs[i]);
                                if (tt == TechType.VehicleArmorPlating)
                                    armorUpgrades++;
                            }
                            float armorMult = 1f;
                            if (armorUpgrades == 1)
                                armorMult = .7f;
                            else if (armorUpgrades == 2)
                                armorMult = .5f;
                            else if (armorUpgrades == 3)
                                armorMult = .4f;

                            __result *= armorMult ;
                        }
                        __result *= Main.config.vehicleDamageMult;
                    }
                    else if (target.GetComponent<SubControl>())
                    {
                        //AddDebug("sub takes damage");
                        __result *= Main.config.vehicleDamageMult;
                    }
                    else
                    {
                        //if (damageMult.Count > 0)
                        {
                            TechType tt = CraftData.GetTechType(target);
                            TechTag techTag = target.EnsureComponent<TechTag>();
                            techTag.type = tt;
                            if (damageMult.ContainsKey(tt))
                                __result *= damageMult[tt];

                            if (tt == TechType.AcidMushroom || tt == TechType.WhiteMushroom)
                            {
                                if (type == DamageType.Acid)
                                    __result = 0f;
                            }
                        }
                    }
                }
                //else
                //{
                //    if (target != Player.mainObject && dealer == Player.mainObject)
                //    {

                //    }
                //}
            }
        }

        [HarmonyPatch(typeof(DamageOnPickup))]
        class DamageOnPickup_Patch
        {
            [HarmonyPatch("OnEnable")]
            [HarmonyPostfix]
            static void OnEnablePostfix(DamageOnPickup __instance)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.WhiteMushroom)
                        __instance.damageAmount += __instance.damageAmount * .5f;
                }
            }
            [HarmonyPatch("OnPickedUp")]
            [HarmonyPrefix]
            static bool OnPickedUpPrefix(DamageOnPickup __instance, Pickupable pickupable)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.AcidMushroom || plantable.plantTechType == TechType.WhiteMushroom)
                    {
                        if (Main.config.shroomDamageChance == 0f)
                            return false;
                        int rnd = Main.rndm.Next(0, 100);
                        if (Main.config.shroomDamageChance > rnd)
                        {
                            int damageMin = (int)(__instance.damageAmount * .5f);
                            int damageMax = (int)(__instance.damageAmount * 1.5f);
                            float damageAmount = Main.rndm.Next(damageMin, damageMax + 1);
                            Player.main.gameObject.GetComponent<LiveMixin>().TakeDamage(damageAmount, pickupable.gameObject.transform.position, DamageType.Acid);
                        }
                        //AddDebug("DamageOnPickup OnPickedUp " + __instance.damageChance + " damageAmount " + __instance.damageAmount);
                        return false;
                    }
                }
                return true;
            }
            [HarmonyPatch("OnKill")]
            [HarmonyPrefix]
            static bool OnKillPrefix(DamageOnPickup __instance)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.AcidMushroom || plantable.plantTechType == TechType.WhiteMushroom)
                    {
                        if (Main.config.shroomDamageChance == 0f)
                            return false;
                        //AddDebug("DamageOnPickup OnKill damageAmount " + __instance.damageAmount);
                        int rnd = Main.rndm.Next(0, 100);
                        if (Main.config.shroomDamageChance > rnd)
                        {
                            int damageMin = (int)(__instance.damageAmount * .5f);
                            int damageMax = (int)(__instance.damageAmount * 1.5f);
                            float damageAmount = Main.rndm.Next(damageMin, damageMax + 1);
                            DamageSystem.RadiusDamage(damageAmount, __instance.transform.position, 3f, __instance.damageType);
                        }
                        return false;
                    }
                }
                return true;
            }
        }

    }
}
