using HarmonyLib;
using UnityEngine;
//using static ErrorMessage;

namespace Tweaks_Fixes
{
    
    [HarmonyPatch(typeof(HangingStinger), "Start")]
    class HangingStinger_Start_Patch
    {
        public static void Postfix(HangingStinger __instance)
        {
            LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
            if (liveMixin && liveMixin.data)
            {
                //AddDebug("HangingStinger");
                liveMixin.data.destroyOnDeath = true;
                //liveMixin.data.explodeOnDestroy = false;
                //CapsuleCollider col = __instance.GetComponentInChildren<CapsuleCollider>();
                //col.gameObject.EnsureComponent<HangingStingerCollision>();
                //__instance.gameObject.EnsureComponent<HangingStingerCollision>();
                //collider.isTrigger = true;
                //collider.bounds = col.bounds;
                //AddDebug("invincibleInCreative " + __instance.invincibleInCreative);
            }
            VFXSurface vFXSurface = __instance.gameObject.EnsureComponent<VFXSurface>();
            vFXSurface.surfaceType = VFXSurfaceTypes.vegetation;
        }
    }

    //[HarmonyPatch(typeof(DealDamageOnImpact), "OnCollisionEnter")]
    class DealDamageOnImpact_OnCollisionEnter_Patch
    {
        public static void Postfix(DealDamageOnImpact __instance, Collision collision)
        {
            if (__instance.GetComponent<SeaMoth>())
            {
                TechType tt = CraftData.GetTechType(collision.gameObject);
                if (tt == TechType.HangingStinger)
                {
                    LiveMixin lm = __instance.GetLiveMixin(collision.gameObject);
                    //AddDebug("OnCollisionEnter " + collision.gameObject.name + " " + lm.maxHealth);
                    lm.TakeDamage(111f, collision.transform.position, DamageType.Collide, __instance.gameObject);
                }
            }
        }
    }

    //[HarmonyPatch(typeof(HangingStinger), "OnCollisionEnter")]
    class HangingStinger_OnCollisionEnter_Patch
    {
        public static bool Prefix(HangingStinger __instance, Collision other)
        {
            if (other.gameObject.GetComponent<Vehicle>())
            {
                //AddDebug("OnCollisionEnter " + other.gameObject.name);
                //CapsuleCollider col = __instance.GetComponentInChildren<CapsuleCollider>();
                //col.isTrigger = true;
                LiveMixin lm = __instance.GetComponent<LiveMixin>();
                //AddDebug("OnCollisionEnter " + collision.gameObject.name + " " + lm.maxHealth);
                lm.TakeDamage(1111f, __instance.transform.position, DamageType.Collide, other.gameObject);
            }
            else
            {
                if (__instance._venomAmount < 1f || other.gameObject.GetComponentInChildren<LiveMixin>() == null)
                    return false;

                DamageOverTime damageOverTime = other.gameObject.AddComponent<DamageOverTime>();
                damageOverTime.doer = __instance.gameObject;
                damageOverTime.totalDamage = 30f;
                damageOverTime.duration = 2.5f * (float)__instance.size;
                damageOverTime.damageType = DamageType.Poison;
                damageOverTime.ActivateInterval(0.5f);
                __instance._venomAmount = 0f;
                __instance.venomRechargeTime = Random.value * 5f + 5f;
            }
            return false;
        }
    }

    //[HarmonyPatch(typeof(LiveMixin), "TakeDamage")]
    class LiveMixin_TakeDamage_Patch
    {
        public static void Postfix(LiveMixin __instance, float originalDamage, GameObject dealer)
        {
            if (__instance.GetComponent<HangingStinger>())
            {
                //AddDebug("IsAlive" + __instance.IsAlive());
                //AddDebug("destroyOnDeath" + __instance.destroyOnDeath);
                //AddDebug("explodeOnDestroy " + __instance.explodeOnDestroy);
                //AddDebug("invincible " + __instance.invincible);
                //AddDebug("health " + __instance.health);
            }
        }
    }

    //[HarmonyPatch(typeof(LiveMixin), "Kill")]
    class LiveMixin_Kill_Patch
    {
        public static void Postfix(LiveMixin __instance)
        {
            if (__instance.GetComponent<HangingStinger>())
            {
                //AddDebug("Kill");
                //AddDebug("Damage " + originalDamage);
                //AddDebug("health " + __instance.health);
            }
        }
    }

    //[HarmonyPatch(typeof(LiveMixin), "CleanUp")]
    class LiveMixin_CleanUp_Patch
    {
        public static void Postfix(LiveMixin __instance)
        {
            //if (__instance.GetComponent<HangingStinger>())
            //{
            //AddDebug("CleanUp " );
            //AddDebug("Damage " + originalDamage);
            //AddDebug("health " + __instance.health);
            //}   UnityEngine.Object.Destroy(o, time);
        }
        
}
}
