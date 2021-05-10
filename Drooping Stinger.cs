using HarmonyLib;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    [HarmonyPatch(typeof(HangingStinger), "Start")]
    class HangingStinger_Start_Patch
    {
        public static void Postfix(HangingStinger __instance)
        {
            LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
            if (liveMixin)
            {
                //AddDebug("HangingStinger");
                liveMixin.data.destroyOnDeath = true;
                liveMixin.data.explodeOnDestroy = false;
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

    //[HarmonyPatch(typeof(HangingStinger), "OnCollisionEnter")]
    class HangingStinger_OnCollisionEnter_Patch
    {
        public static void Postfix(HangingStinger __instance, Collision other)
        {
            //if (__instance.GetComponent<HangingStinger>())
            //{
            AddDebug("OnCollisionEnter " + other.gameObject.name);
            CapsuleCollider col = __instance.GetComponentInChildren<CapsuleCollider>();
            col.isTrigger = true;
            //__instance.data.explodeOnDestroy = false;
            //AddDebug("invincibleInCreative " + __instance.invincibleInCreative);
            //}
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
                AddDebug("Kill");
                //AddDebug("Damage " + originalDamage);
                //AddDebug("health " + __instance.health);
            }
        }
    }

    //[HarmonyPatch(typeof(ExploderObject), "ExplodeGameObject")]
    class ExploderObject_Kill_Patch
    {
        public static void Postfix(ExploderObject __instance, GameObject go)
        {
            //if (__instance.GetComponent<HangingStinger>())
            //{
                AddDebug("ExplodeGameObject " + go.name);
            //AddDebug("Damage " + originalDamage);
            //AddDebug("health " + __instance.health);
            //}   UnityEngine.Object.Destroy(o, time);
        }
    }


    //[HarmonyPatch(typeof(LiveMixin), "CleanUp")]
    class LiveMixin_CleanUp_Patch
    {
        public static void Postfix(LiveMixin __instance)
        {
            //if (__instance.GetComponent<HangingStinger>())
            //{
            AddDebug("CleanUp " );
            //AddDebug("Damage " + originalDamage);
            //AddDebug("health " + __instance.health);
            //}   UnityEngine.Object.Destroy(o, time);
        }
    }
}
