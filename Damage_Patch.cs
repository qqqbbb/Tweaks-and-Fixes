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

        //[HarmonyPatch(typeof(LiveMixin), "Kill")]
        class LiveMixin_Kill_Patch
        {
            static void Postfix(LiveMixin __instance, DamageType damageType)
            {
                //ProfilingUtils.BeginSample("LiveMixin.Kill");
                __instance.health = 0.0f;
                __instance.tempDamage = 0.0f;
                __instance.SyncUpdatingState();
                if (__instance.deathClip)
                    Utils.PlayEnvSound(__instance.deathClip, __instance.transform.position, 25f);
                if (__instance.deathEffect != null)
                {
                    GameObject go = UWE.Utils.InstantiateWrap(__instance.deathEffect, __instance.transform.position, Quaternion.identity);


                }

                if (__instance.passDamageDataOnDeath)
                    __instance.gameObject.BroadcastMessage("OnKill", damageType, SendMessageOptions.DontRequireReceiver);
                else if (__instance.broadcastKillOnDeath)
                    __instance.gameObject.BroadcastMessage("OnKill", SendMessageOptions.DontRequireReceiver);
                if (__instance.destroyOnDeath)
                {
                    if (__instance.explodeOnDestroy)
                    {
                        Living component = __instance.gameObject.GetComponent<Living>();
                        if (component)
                            component.enabled = false;
                        ExploderObject.ExplodeGameObject(__instance.gameObject);
                    }
                    else
                    {
                        __instance.CleanUp();
                        UWE.Utils.DestroyWrap(__instance.gameObject);
                    }
                }
                //ProfilingUtils.EndSample();
            }
        }

        [HarmonyPatch(typeof(LiveMixin), "TakeDamage")]
        class LiveMixin_TakeDamage_Patch
        {
            static bool Prefix(LiveMixin __instance, ref bool __result, float originalDamage, Vector3 position = default(Vector3), DamageType type = DamageType.Normal, GameObject dealer = null)
            {
                //if (dealer)
                //    AddDebug("dealer " + dealer.name);
                //ProfilingUtils.BeginSample("LiveMixin.TakeDamage");
                bool killed = false;
                bool creativeMode = GameModeUtils.IsInvisible() && __instance.invincibleInCreative;
                if (__instance.health > 0f && !__instance.invincible && !creativeMode)
                {
                    float damage = 0f;
                    if (!__instance.shielded)
                        damage = DamageSystem.CalculateDamage(originalDamage, type, __instance.gameObject, dealer);

                    __instance.health = Mathf.Max(0f, __instance.health - damage);
                    if (type == DamageType.Cold || type == DamageType.Poison)
                    {
                        __instance.tempDamage += damage;
                        __instance.SyncUpdatingState();
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
                    {
                        //ProfilingUtils.BeginSample("LiveMixin.TakeDamage.PlaySound");
                        Utils.PlayEnvSound(__instance.damageClip, __instance.damageInfo.position);
                        //ProfilingUtils.EndSample();
                    }
                    if (__instance.loopingDamageEffect && !__instance.loopingDamageEffectObj && __instance.GetHealthFraction() < __instance.loopEffectBelowPercent)
                    {
                        //__instance.loopingDamageEffectObj = UWE.Utils.InstantiateWrap(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj = UnityEngine.Object.Instantiate<GameObject>(__instance.loopingDamageEffect, __instance.transform.position, Quaternion.identity);
                        __instance.loopingDamageEffectObj.transform.parent = __instance.transform;
                    }
                    //ProfilingUtils.BeginSample("LiveMixin.TakeDamage.DamageEffect");

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
                    else if (Time.time > __instance.timeLastDamageEffect + 1f && damage > 0f &&  dealer != Player.main.gameObject && type == DamageType.Normal || type == DamageType.Collide || type == DamageType.Explosive || type == DamageType.Puncture || type == DamageType.LaserCutter || type == DamageType.Drill  )
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
                    //ProfilingUtils.EndSample();
                    if (__instance.health <= 0f || __instance.health - __instance.tempDamage <= 0f)
                    {
                        killed = true;
                        if (!__instance.IsCinematicActive())
                        {
                            __instance.Kill(type);
                        }
                        else
                        {
                            __instance.cinematicModeActive = true;
                            __instance.SyncUpdatingState();
                        }
                    }
                }
                //ProfilingUtils.EndSample();
                __result = killed;
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

        [HarmonyPatch(typeof(Creature), "Start")]
        class Creature_Start_Patch
        { 
            public static void Postfix(Creature __instance)
            {
                // reginald and bladderfish dont have VFXSurface
                VFXSurface vFXSurface = __instance.gameObject.EnsureComponent<VFXSurface>();
                vFXSurface.surfaceType = VFXSurfaceTypes.organic;
                //LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                //liveMixin.data.damageEffect = null;
                //liveMixin.data.deathEffect = null;
            }
        }

        [HarmonyPatch(typeof(LiveMixin), "Start")]
        class LiveMixin_Start_Patch
        {
            static void Postfix(LiveMixin __instance)
            {
                //if (__instance.data.deathEffect)
                //{
                //    Main.Log("deathEffect " + __instance.data.deathEffect);
                //}
                if (Main.config.noKillParticles)
                { 
                    //__instance.data.damageEffect = null;
                    __instance.data.deathEffect = null;
                }
            }
        }

        [HarmonyPatch(typeof(DamageSystem), "CalculateDamage")]
        class DamageSystem_CalculateDamage_Patch
        {
            public static void Postfix(DamageSystem __instance, float damage, DamageType type, GameObject target, GameObject dealer, ref float __result)
            {
                if (__result > 0f)
                {
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
                        if (Main.config.replacePoisonDamage && type == DamageType.Poison)
                        {
                            //AddDebug("Player takes Poison damage " + damage);
                            Survival survival = Player.main.GetComponent<Survival>();
                            int foodMin = Main.config.newHungerSystem ? -99 : 1;
                            int damageLeft = 0;
                            for (int i = (int)__result; i > 0; i--)
                            {
                                if (survival.food > foodMin)
                                    survival.food -= 1f;
                                else
                                    damageLeft++;

                                if (survival.water > foodMin)
                                    survival.water -= 1f;
                                else
                                    damageLeft++;
                            }
                            //DamageType.
                            //AddDebug("damageLeft " + damageLeft);
                            Player.main.liveMixin.TakeDamage(damageLeft, target.transform.position, DamageType.Starve, dealer);
                            __result = 0f;
                        }
                    }
                    else if (target.GetComponent<Vehicle>() || target.GetComponent<SubControl>())
                    {
                        //AddDebug("Vehicle takes damage");
                        __result *= Main.config.vehicleDamageMult;
                    }
                    else
                        __result *= Main.config.damageMult;
                }

            }
        }
    }
}
