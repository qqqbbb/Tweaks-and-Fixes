using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;
using static PlaceTool;
using static Tweaks_Fixes.Damage_Patch;
using static VFXParticlesPool;

namespace Tweaks_Fixes
{
    class Damage_Patch
    {
        public static float healTempDamageTime = 0;
        static float poisonDamageInterval = .8f;
        static float poisonDamage = .5f;
        public static HashSet<LiveMixin> tempDamageLMs = new HashSet<LiveMixin>();
        static public Dictionary<TechType, float> damageMult = new Dictionary<TechType, float>();


        public static void SetBloodColor()
        {
            foreach (GameObject go in Util.FindAllRootGameObjects())
            {
                if (go.name == "xKnifeHit_Organic" || go.name == "GenericCreatureHit" || go.name == "xExoDrill_Organic")
                {
                    ParticleSystem[] pss = go.GetAllComponentsInChildren<ParticleSystem>();
                    //AddDebug("SetBloodColor " + go.name + " " + pss.Length);
                    //Main.Log("SetBloodColor " + go.name );
                    foreach (ParticleSystem ps in pss)
                    {
                        //ps.startColor = new Color(1f, 0f, 0f);
                        ParticleSystem.MainModule psMain = ps.main;
                        //Main.Log("startColor " + psMain.startColor.color);
                        Color newColor = new Color(ConfigToEdit.bloodColor.Value.x, ConfigToEdit.bloodColor.Value.y, ConfigToEdit.bloodColor.Value.z, psMain.startColor.color.a);
                        psMain.startColor = new ParticleSystem.MinMaxGradient(newColor);
                    }
                }
                else if (go.name.Contains("MapRoom"))
                {
                    Main.logger.LogMessage("MapRoom " + go.name);
                }
            }
        }

        [HarmonyPatch(typeof(DealDamageOnImpact))]
        class DealDamageOnImpact_patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("Start")]
            public static void StartPostfix(DealDamageOnImpact __instance)
            {
                TechType tt = CraftData.GetTechType(__instance.gameObject);

                __instance.minDamageInterval = 1f;
                if (tt == TechType.Gasopod)
                {
                    UnityEngine.Object.Destroy(__instance);
                }
                else if (tt == TechType.Seamoth || tt == TechType.Exosuit || tt == TechType.Cyclops)
                {
                    //__instance.allowDamageToPlayer = false;
                    //AddDebug(__instance.name + " tt " + tt + " " + __instance.speedMinimumForDamage);
                }
                //else
                //{
                //    AddDebug(" DealDamageOnImpact start " + __instance.gameObject.name + " allowDamageToPlayer " + __instance.allowDamageToPlayer + " mirroredSelfDamage " + __instance.mirroredSelfDamage + " minimumMassForDamage " + __instance.minimumMassForDamage + " speedMinimumForDamage " + __instance.speedMinimumForDamage);
                //    Main.logger.LogInfo(" DealDamageOnImpact start " + __instance.gameObject.name + " allowDamageToPlayer " + __instance.allowDamageToPlayer + " mirroredSelfDamage " + __instance.mirroredSelfDamage + " minimumMassForDamage " + __instance.minimumMassForDamage + " speedMinimumForDamage " + __instance.speedMinimumForDamage);
                //}
            }

            [HarmonyPrefix]
            [HarmonyPatch("OnCollisionEnter")]
            public static bool OnCollisionEnterPrefix(DealDamageOnImpact __instance, Collision collision)
            {
                if (!ConfigToEdit.replaceDealDamageOnImpactScript.Value)
                    return true;

                if (!__instance.enabled || collision.contacts.Length == 0 || __instance.exceptions.Contains(collision.gameObject))
                    return false;

                bool terrain = collision.gameObject.GetComponent<TerrainChunkPieceCollider>();
                GameObject colTarget = collision.gameObject;
                if (!terrain)
                    colTarget = Util.GetEntityRoot(collision.gameObject);

                if (!colTarget)
                    colTarget = collision.gameObject;

                if (collision.gameObject.GetComponentInParent<Player>())
                {
                    if (!__instance.allowDamageToPlayer )
                    {
                        //AddDebug(__instance.name + " collided with player");
                        return false;
                    }
                    colTarget = Player.mainObject;
                }
                //if (colTarget)
                //    AddDebug(__instance.name + " OnCollisionEnter " + colTarget.name);
                //else
                //    AddDebug(__instance.name + " OnCollisionEnter colTarget null ");

                // collision.contacts generates garbage
                ContactPoint contactPoint = collision.GetContact(0);
                Vector3 impactPoint = contactPoint.point;
                float damageMult = Mathf.Max(0f, Vector3.Dot(-contactPoint.normal, __instance.prevVelocity));

                damageMult = Mathf.Clamp(damageMult, 0f, 10f);
                Rigidbody otherRB = collision.rigidbody;
                float myMass = __instance.GetComponent<Rigidbody>().mass;
                float massRatioInv;
                float massRatio;
                if (terrain)
                {
                    massRatio = .01f;
                    massRatioInv = 100f;
                }
                else
                {
                    if (otherRB)
                    {
                        massRatio = myMass / otherRB.mass;
                        massRatioInv = otherRB.mass / myMass;
                        //AddDebug("myMass " + myMass + " other mass " + otherRB.mass);
                    }
                    else
                    {
                        Bounds otherBounds = Util.GetAABB(colTarget);
                        Bounds myBounds = Util.GetAABB(__instance.gameObject);
                        massRatioInv = otherBounds.size.magnitude / myBounds.size.magnitude;
                        massRatio = myBounds.size.magnitude / otherBounds.size.magnitude;
                        //AddDebug("myBounds " + myBounds.size.magnitude + " otherBounds " + otherBounds.size.magnitude);
                    }
                }
                TechType myTT = CraftData.GetTechType(__instance.gameObject);
                TechType otherTT = CraftData.GetTechType(colTarget);
                bool vehicle = myTT == TechType.Cyclops || myTT == TechType.Seamoth || myTT == TechType.Exosuit;
                bool otherVehicle = otherTT == TechType.Cyclops || otherTT == TechType.Seamoth || otherTT == TechType.Exosuit;
                //DealDamageOnImpact otherDDOI = colTarget.GetComponent<DealDamageOnImpact>();

                bool canDealDamage = true;
                //if (otherDDOI && damageMult < otherDDOI.speedMinimumForDamage)
                //    canDealDamage = false;
                if (myTT == TechType.Seamoth && damageMult < ConfigToEdit.seamothDealDamageMinSpeed.Value)
                    canDealDamage = false;
                else if (myTT == TechType.Exosuit && damageMult < ConfigToEdit.exosuitDealDamageMinSpeed.Value)
                    canDealDamage = false;
                else if (myTT == TechType.Cyclops && damageMult < ConfigToEdit.cyclopsDealDamageMinSpeed.Value)
                    canDealDamage = false;

                if (damageMult > 0 && canDealDamage && Time.time > __instance.timeLastDamage + 1f)
                {
                    LiveMixin otherLM = __instance.GetLiveMixin(colTarget);
                    if (otherLM && otherLM.health > 0)
                    {
                        //AddDebug(otherLM.name + " max HP " + otherLM.maxHealth + " HP " + (int)otherLM.health);
                        VFXSurfaceTypes mySurfaceType = VFXSurfaceTypes.none;
                        if (vehicle)
                            mySurfaceType = VFXSurfaceTypes.metal;
                        else
                            mySurfaceType = Util.GetObjectSurfaceType(__instance.gameObject);

                        float massRatioClamped = Mathf.Clamp(massRatio, 0, damageMult);
                        massRatioClamped = Util.NormalizeToRange(massRatioClamped, 0f, 10f, 1f, 2f);
                        if (mySurfaceType == VFXSurfaceTypes.metal || mySurfaceType == VFXSurfaceTypes.glass || mySurfaceType == VFXSurfaceTypes.rock)
                            massRatioClamped *= 2f;

                        float damage = damageMult * massRatioClamped;
                        //AddDebug(__instance.name + " deal damage " + (int)damage);
                        //AddDebug(__instance.name + " speedMinimumForDamage " + __instance.speedMinimumForDamage);
                        otherLM.TakeDamage(damage, impactPoint, DamageType.Collide, __instance.gameObject);
                        __instance.timeLastDamage = Time.time;
                    }
                }
                if (terrain && myTT == TechType.Exosuit)
                    return false;

                LiveMixin myLM = __instance.GetLiveMixin(__instance.gameObject);
                if (damageMult <= 0 || __instance.mirroredSelfDamageFraction == 0f || !myLM || Time.time < __instance.timeLastDamagedSelf + 1f)
                    return false;

                //bool canTakeDamage = true;
                if (myTT == TechType.Seamoth && damageMult < ConfigToEdit.seamothTakeDamageMinSpeed.Value)
                    return false;
                else if (myTT == TechType.Exosuit && damageMult < ConfigToEdit.exosuitTakeDamageMinSpeed.Value)
                    return false;
                else if (myTT == TechType.Cyclops && damageMult < ConfigToEdit.cyclopsTakeDamageMinSpeed.Value)
                    return false;

                if (otherRB)
                {
                    if (myTT == TechType.Seamoth && otherRB.mass <= ConfigToEdit.seamothTakeDamageMinMass.Value)
                        return false;
                    else if (myTT == TechType.Exosuit && otherRB.mass <= ConfigToEdit.exosuitTakeDamageMinMass.Value)
                        return false;
                    else if (myTT == TechType.Cyclops && otherRB.mass <= ConfigToEdit.cyclopsTakeDamageMinMass.Value)
                        return false;
                }
                else if (otherRB == null || terrain)
                {
                    if (myTT == TechType.Seamoth && ConfigToEdit.seamothTakeDamageMinMass.Value >= 10000)
                        return false;
                    else if (myTT == TechType.Exosuit && ConfigToEdit.exosuitTakeDamageMinMass.Value >= 10000)
                        return false;
                    else if (myTT == TechType.Cyclops && ConfigToEdit.cyclopsTakeDamageMinMass.Value >= 10000)
                        return false;
                }
                //float myDamage = colMag * Mathf.Clamp((1f + massRatio * 0.001f), 0f, damageMult);
                VFXSurfaceTypes surfaceType = Util.GetObjectSurfaceType(colTarget);
                //AddDebug(colTarget.name + " surface " + surfaceType);
                float massRatioInvClamped = Mathf.Clamp(massRatioInv, 0, damageMult);
                massRatioInvClamped = Util.NormalizeToRange(massRatioInvClamped, 0f, 10f, 1f, 2f);
                if (terrain || surfaceType == VFXSurfaceTypes.glass || surfaceType == VFXSurfaceTypes.metal || surfaceType == VFXSurfaceTypes.rock)
                    massRatioInvClamped *= 2f;

                float myDamage = damageMult * massRatioInvClamped ;
                //AddDebug(__instance.name + " maxHealth " + myLM.maxHealth + " health " + (int)myLM.health);
                if (__instance.capMirrorDamage != -1f)
                    myDamage = Mathf.Min(__instance.capMirrorDamage, myDamage);

                myLM.TakeDamage(myDamage, impactPoint, DamageType.Collide, __instance.gameObject);
                //AddDebug(__instance.name + " take damage " + (int)myDamage);
                //AddDebug(__instance.name + " speedMinimumForSelfDamage " + __instance.speedMinimumForSelfDamage);
                __instance.timeLastDamagedSelf = Time.time;
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("OnCollisionEnter")]
            public static void OnCollisionEnterPostfix(DealDamageOnImpact __instance, Collision collision)
            {
                //if (__instance.GetComponentInChildren<Player>())
                //{
                //    AddDebug("velocity Postfix " + __instance.GetComponent<Rigidbody>().velocity);
                //}
                //else
                    //AddDebug(__instance.name + " OnCollisionEnter" );

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
                if (ConfigToEdit.noKillParticles.Value)
                { 
                    //__instance.data.damageEffect = null;
                    __instance.data.deathEffect = null;
                }
                if (!Main.gameLoaded && __instance.tempDamage > 0)
                { // __instance.tempDamage is -1
                    tempDamageLMs.Add(__instance);
                    //AddDebug("tempDamage " + __instance.tempDamage);
                    //Main.Log("tempDamage " + __instance.tempDamage);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("TakeDamage")]
            static bool TakeDamagePrefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position = default(Vector3), DamageType type = DamageType.Normal, GameObject dealer = null)
            {
                bool killed = false;
                bool isBase = __instance.GetComponent<BaseCell>() != null;
                bool invincible = GameModeUtils.IsInvisible() && __instance.invincibleInCreative | isBase;
                if (__instance.health > 0f && !__instance.invincible && !invincible)
                {
                    float damage = 0f;
                    if (!__instance.shielded)
                    {
                        if (dealer == Player.mainObject)
                        {
                            if (type == DamageType.Heat && __instance.GetComponent<LavaLizard>())
                                type = DamageType.Normal;
                        }
                        damage = DamageSystem.CalculateDamage(originalDamage, type, __instance.gameObject, dealer);
                    }
                    //if (__instance.gameObject == Player.mainObject)
                    //    AddDebug("TakeDamage " + type + " " + damage);

                    if (type != DamageType.Poison && type != DamageType.Cold)
                        __instance.health = Mathf.Max(0f, __instance.health - damage);
                    else 
                    {
                        if (ConfigToEdit.newPoisonSystem.Value && __instance.gameObject == Player.mainObject)
                        { // Survival.onHealTempDamage wont run
                            if (dealer == __instance.gameObject && position == Vector3.one)
                            {// from HealTempDamagePrefix
                                //AddDebug("Take Poison Damage " + damage);
                                __instance.health = Mathf.Max(0f, __instance.health - damage);
                            }
                            else
                            {
                                //AddDebug("TakeDamage tempDamage " + __instance.tempDamage);
                                __instance.tempDamage += damage;
                                __instance.SyncUpdatingState();
                            }
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
                    if (__instance.damageInfo != null)
                    {
                        __instance.damageInfo.Clear();
                        __instance.damageInfo.originalDamage = originalDamage;
                        __instance.damageInfo.damage = damage;
                        __instance.damageInfo.position = position == new Vector3() ? __instance.transform.position : position;
                        __instance.damageInfo.type = type;
                        __instance.damageInfo.dealer = dealer;
                        __instance.NotifyAllAttachedDamageReceivers(__instance.damageInfo);
                    }
                    if (__instance.shielded)
                    {
                        __result = killed;
                        return false;
                    }
                    if (__instance.damageClip && __instance.damageInfo != null && damage > 0f && (damage >= __instance.minDamageForSound && type != DamageType.Radiation))
                        Utils.PlayEnvSound(__instance.damageClip, __instance.damageInfo.position);

                    if (__instance.loopingDamageEffect && !__instance.loopingDamageEffectObj && __instance.GetHealthFraction() < __instance.loopEffectBelowPercent)
                    {
                        __instance.loopingDamageEffectObj = UWE.Utils.InstantiateWrap(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj.transform.parent = __instance.transform;
                    }
                    if (Time.time > __instance.timeLastElecDamageEffect + 2.5f && type == DamageType.Electrical && __instance.electricalDamageEffect != null)
                    {
                        FixedBounds fixedBounds = __instance.gameObject.GetComponent<FixedBounds>();
                        Bounds bounds = fixedBounds == null ? UWE.Utils.GetEncapsulatedAABB(__instance.gameObject) : fixedBounds.bounds;
                        GameObject gameObject = UWE.Utils.InstantiateWrap(__instance.electricalDamageEffect, bounds.center, Quaternion.identity);
                        gameObject.transform.parent = __instance.transform;
                        gameObject.transform.localScale = bounds.size * 0.65f;
                        __instance.timeLastElecDamageEffect = Time.time;
                    }
                    else if (Main.gameLoaded && Time.time > __instance.timeLastDamageEffect + 1f && damage > 0f && dealer != Player.main.gameObject && type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter || type == DamageType.Drill)
                    { // dont spawn damage particles if knifed by player
                        VFXSurface vfxSurface = __instance.GetComponentInChildren<VFXSurface>();
                        if (vfxSurface)
                        {
                            //AddDebug("Spawn vfxSurface Prefab ");
                            //Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
                            ParticleSystem particleSystem = VFXSurfaceTypeManager.main.Play(vfxSurface, VFXEventTypes.knife, position, Quaternion.identity, Player.main.transform);
                            __instance.timeLastDamageEffect = Time.time;
                        }
                        else if (__instance.damageEffect)
                        {
                            //AddDebug("Spawn damageEffect Prefab " + __instance.damageEffect.name);
                            //GameObject go = Utils.SpawnPrefabAt(__instance.damageEffect, __instance.transform, __instance.damageInfo.position);
                            //setBloodColor = true;
                            //if (__instance.GetComponent<Creature>())
                            //    SetBloodColor(go);

                            __instance.timeLastDamageEffect = Time.time;
                        }
                    }
                    if (__instance.health <= 0f || !ConfigToEdit.newPoisonSystem.Value && __instance.health - __instance.tempDamage <= 0f)
                    {
                        killed = true;
                        if (!__instance.IsCinematicActive() || __instance.ShouldKillInCinematic())
                            __instance.Kill(type);
                        else
                        {
                            __instance.cinematicModeActive = true;
                            __instance.SyncUpdatingState();
                        }
                    }
                }
                return killed;
            }

            [HarmonyPrefix]
            [HarmonyPatch("HealTempDamage")]
            static bool HealTempDamagePrefix(LiveMixin __instance, float timePassed)
            {
                if (!ConfigToEdit.newPoisonSystem.Value || __instance.gameObject != Player.mainObject)
                    return true;
         
                if (__instance.tempDamage > 0 && healTempDamageTime < Time.time)
                {
                    int foodMin = ConfigMenu.newHungerSystem.Value ? -99 : 1;
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

                    damage = (damage + poisonDamage) * poisonDamageInterval;
                    __instance.TakeDamage(damage , Vector3.one, DamageType.Poison, __instance.gameObject);
                    //AddDebug("HealTempDamage Player TakeDamage " + damage );
                    __instance.tempDamage -= poisonDamage;
                    //AddDebug("HealTempDamage tempDamage " + __instance.tempDamage);
                    if (__instance.tempDamage > 0)
                        healTempDamageTime = Time.time + poisonDamageInterval;
                    else
                        __instance.tempDamage = 0;

                    if (__instance.tempDamage == 0)
                        __instance.SyncUpdatingState();
                }
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch(nameof(LiveMixin.SyncUpdatingState))]
            static void ManagedUpdatePostfix(LiveMixin __instance)
            {
                if (__instance.gameObject == Player.mainObject)
                {
                    //AddDebug("SyncUpdatingState ");
                }
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("TakeDamage")]
            static void TakeDamagePostfix(LiveMixin __instance, bool __result, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
            {
                if (__instance.gameObject.GetComponent<SubControl>())
                {
                    //AddDebug(__instance.name + " TakeDamage Postfix " + originalDamage + " " + type);
                }
            }
        }

        //[HarmonyPatch(typeof(Survival), "OnHealTempDamage")]
        static void Prefix(Survival __instance, float damage)
        {
            float food = Mathf.Clamp(__instance.food - damage * 0.25f, 0f, 200f);
            //AddDebug("Survival OnHealTempDamage " + food);
        }

        //[HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Prefix_Patch
        {
            public static void Prefix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                //AddDebug(target.name + " damage Prefix " + damage);
            }
        }
           
        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                //AddDebug(target.name + " damage " + damage + ' ' + __result);
                if (__result <= 0f)
                    return;

                Vehicle vehicle = target.GetComponent<Vehicle>();
                if (target == Player.mainObject)
                {
                    __result *= ConfigMenu.playerDamageMult.Value;
                    //AddDebug("Player takes damage " + __result.ToString("0.0"));
                    if (Util.Approximately(__result, 0f))
                        return;

                    if (ConfigToEdit.dropHeldTool.Value)
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
                    __result *= ConfigMenu.vehicleDamageMult.Value;
                }
                else if (target.GetComponent<SubControl>())
                {
                    //AddDebug("sub takes damage");
                    __result *= ConfigMenu.vehicleDamageMult.Value;
                }
                else
                {
                    //if (damageMult.Count > 0)
                    {
                        TechType tt = CraftData.GetTechType(target);
                        //TechTag techTag = target.EnsureComponent<TechTag>();
                        //techTag.type = tt;
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
        }
        
        [HarmonyPatch(typeof(DamageOnPickup))]
        class DamageOnPickup_Patch
        {
            [HarmonyPostfix]
            [HarmonyPatch("OnEnable")]
            static void OnEnablePostfix(DamageOnPickup __instance)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.WhiteMushroom)
                        __instance.damageAmount += __instance.damageAmount * .5f;
                }
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnPickedUp")]
            static bool OnPickedUpPrefix(DamageOnPickup __instance, Pickupable pickupable)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.AcidMushroom || plantable.plantTechType == TechType.WhiteMushroom)
                    {
                        if (ConfigToEdit.shroomDamageChance.Value == 0)
                            return false;

                        int rnd = Main.rndm.Next(0, 100);
                        if (ConfigToEdit.shroomDamageChance.Value > rnd)
                        {
                            int damageMin = (int)(__instance.damageAmount * .5f);
                            int damageMax = (int)(__instance.damageAmount * 1.5f);
                            float damageAmount = Main.rndm.Next(damageMin, damageMax + 1);
                            if (!Player.main.currentMountedVehicle)
                                Player.main.gameObject.GetComponent<LiveMixin>().TakeDamage(damageAmount, pickupable.gameObject.transform.position, DamageType.Acid);
                        }
                        //AddDebug("DamageOnPickup OnPickedUp " + __instance.damageChance + " damageAmount " + __instance.damageAmount);
                        return false;
                    }
                }
                return true;
            }
            [HarmonyPrefix]
            [HarmonyPatch("OnKill")]
            static bool OnKillPrefix(DamageOnPickup __instance)
            {
                Plantable plantable = __instance.GetComponent<Plantable>();
                if (plantable)
                {
                    if (plantable.plantTechType == TechType.AcidMushroom || plantable.plantTechType == TechType.WhiteMushroom)
                    {
                        if (ConfigToEdit.shroomDamageChance.Value == 0)
                            return false;
                        //AddDebug("DamageOnPickup OnKill damageAmount " + __instance.damageAmount);
                        int rnd = Main.rndm.Next(0, 100);
                        if (ConfigToEdit.shroomDamageChance.Value > rnd)
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


        [HarmonyPatch(typeof(DamageFX), "AddHudDamage")]
        class DamageFX_AddHudDamage_Patch
        {
            public static bool Prefix(DamageFX __instance, float damageScalar, Vector3 damageSource, DamageInfo damageInfo)
            {
                //Main.config.crushDamageScreenEffect = false;
                //AddDebug("AddHudDamage " + damageInfo.type);
                if (!ConfigToEdit.crushDamageScreenEffect.Value && damageInfo.type == DamageType.Pressure)
                    return false;

                if (ConfigMenu.damageImpactEffect.Value)
                    __instance.CreateImpactEffect(damageScalar, damageSource, damageInfo.type);

                if (ConfigMenu.damageScreenFX.Value)
                    __instance.PlayScreenFX(damageInfo);

                return false;
            }
        }


    }
}
