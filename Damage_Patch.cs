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
        static float poisonDamageInterval = .8f;
        static float poisonDamage = .5f;
        static bool clawArmHit;


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
                    if (!__instance.allowDamageToPlayer)
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
                bool vehicle = Vehicle_patch.vehicleTechTypes.Contains(myTT);
                bool otherVehicle = Vehicle_patch.vehicleTechTypes.Contains(otherTT);
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

                if (terrain && myTT == TechType.Exosuit)
                    return false;

                if (damageMult > 0 && canDealDamage && Time.time > __instance.timeLastDamage + 1f)
                {
                    LiveMixin otherLM = __instance.GetLiveMixin(colTarget);
                    if (otherLM && otherLM.health > 0)
                    {
                        //AddDebug(otherLM.name + " max HP " + otherLM.maxHealth + " HP " + (int)otherLM.health);
                        //if (vehicle && !ConfigToEdit.vehiclesHurtCreatures.Value && Creature_Tweaks.creatureTT.Contains(otherTT))
                        {
                            //AddDebug("vehicle hit creature");
                            //return false;
                        }
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

                float myDamage = damageMult * massRatioInvClamped;
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

        [HarmonyPatch(typeof(ExosuitClawArm))]
        class ExosuitClawArm_Patch
        {
            [HarmonyPrefix, HarmonyPatch("OnHit")]
            static void OnHitPrefix(ExosuitClawArm __instance)
            {
                clawArmHit = true;
            }
            [HarmonyPostfix, HarmonyPatch("OnHit")]
            static void OnHitPostfix(ExosuitClawArm __instance)
            {
                clawArmHit = false;
            }
        }

        [HarmonyPatch(typeof(LiveMixin))]
        class LiveMixin_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
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
            }

            [HarmonyPrefix, HarmonyPatch("TakeDamage")]
            static void TakeDamagePrefix_(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position, ref DamageType type, GameObject dealer)
            {
                bool hitByPlayer = dealer == Player.main.gameObject || clawArmHit || type == DamageType.Drill;
                if (ConfigToEdit.removeBigParticlesWhenKnifing.Value && Main.gameLoaded && hitByPlayer && originalDamage > 0 && type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter)
                { // dont spawn big damage particles if knifed by player
                    if (__instance.damageEffect)
                    {
                        //AddDebug(" damageEffect  " + __instance.damageEffect.name);
                        __instance.timeLastDamageEffect = Time.time;
                    }
                    else if (__instance.GetComponentInChildren<VFXSurface>())
                    {
                        //AddDebug(" vfxSurface  ");
                        __instance.timeLastDamageEffect = Time.time;
                    }
                }
                else if (type == DamageType.Heat && !__instance.shielded && dealer == Player.mainObject)
                {
                    if (__instance.GetComponent<LavaLizard>())
                    { // can damage LavaLizard with heatblade
                        //AddDebug("LavaLizard");
                        type = DamageType.Normal;
                    }
                }
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



        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                //AddDebug(target.name + " damage " + damage + ' ' + __result);
                if (__result <= 0f)
                    return;

                if (type == DamageType.Drill)
                {
                    __result *= ConfigMenu.drillDamageMult.Value;
                    //AddDebug("CalculateDamage Drill");
                }
                if (target == Player.mainObject)
                {
                    __result *= ConfigMenu.playerDamageMult.Value;
                    //AddDebug("Player takes damage " + __result.ToString("0.0"));
                    if (Mathf.Approximately(__result, 0f))
                        return;

                    if (ConfigToEdit.dropHeldTool.Value)
                    {
                        if (type != DamageType.Cold && type != DamageType.Poison && type != DamageType.Starve && type != DamageType.Radiation && type != DamageType.Pressure)
                        {
                            if (UnityEngine.Random.Range(1, 100) < damage)
                            {
                                //AddDebug("DropHeldItem");
                                Inventory.main.DropHeldItem(true);
                            }
                        }
                    }
                    return;
                }
                Vehicle vehicle = target.GetComponent<Vehicle>();
                if (vehicle)
                {
                    //AddDebug("Vehicle takes damage");
                    if (type == DamageType.Normal || type == DamageType.Pressure || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture)
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

                        __result *= armorMult;
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
                    TechType targetTT = CraftData.GetTechType(target);
                    if (targetTT == TechType.AcidMushroom || targetTT == TechType.WhiteMushroom)
                    {
                        if (type == DamageType.Acid)
                        {
                            __result = 0f;
                            return;
                        }
                    }
                    if (dealer && !ConfigToEdit.vehiclesHurtCreatures.Value && Creatures.creatureTT.Contains(targetTT))
                    {
                        TechType dealerTT = CraftData.GetTechType(dealer);
                        if (Vehicle_patch.vehicleTechTypes.Contains(dealerTT))
                        {
                            //AddDebug("CalculateDamage by vehicle " + dealerTT + " to " + targetTT);
                            __result = 0;
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

                        int rnd = UnityEngine.Random.Range(0, 100);
                        if (ConfigToEdit.shroomDamageChance.Value > rnd)
                        {
                            if (!Player.main.currentMountedVehicle)
                                Player.main.gameObject.GetComponent<LiveMixin>().TakeDamage(GetDamageAmount(__instance), pickupable.gameObject.transform.position, DamageType.Acid);
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
                        int rnd = UnityEngine.Random.Range(0, 100);
                        if (ConfigToEdit.shroomDamageChance.Value > rnd)
                        {
                            DamageSystem.RadiusDamage(GetDamageAmount(__instance), __instance.transform.position, 3f, __instance.damageType);
                        }
                        return false;
                    }
                }
                return true;
            }

            private static float GetDamageAmount(DamageOnPickup damageOnPickup)
            {
                float damageMin = damageOnPickup.damageAmount * .5f;
                float damageMax = damageOnPickup.damageAmount * 1.5f;
                return UnityEngine.Random.Range(damageMin, damageMax);
            }
        }

        [HarmonyPatch(typeof(Drillable))]
        class Drillable_Start_Patch
        {
            [HarmonyPostfix, HarmonyPatch("Start")]
            static void StartPostfix(Drillable __instance)
            {
                if (ConfigMenu.drillDamageMult.Value == 1)
                    return;

                for (int index = 0; index < __instance.health.Length; ++index)
                {
                    __instance.health[index] /= ConfigMenu.drillDamageMult.Value;
                    //AddDebug("Drillable Start " + __instance.health[index]);
                }
            }
            [HarmonyPostfix, HarmonyPatch("Restore")]
            static void RestorePostfix(Drillable __instance)
            {
                if (ConfigMenu.drillDamageMult.Value == 1)
                    return;

                for (int index = 0; index < __instance.health.Length; ++index)
                {
                    __instance.health[index] /= ConfigMenu.drillDamageMult.Value;
                    //AddDebug("Drillable Restore " + __instance.health[index]);
                }
            }
        }




    }
}
