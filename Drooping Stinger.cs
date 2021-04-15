using HarmonyLib;
using UnityEngine;

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
                //ErrorMessage.AddDebug("HangingStinger");
                liveMixin.data.destroyOnDeath = true;
                liveMixin.data.explodeOnDestroy = false;
                //CapsuleCollider col = __instance.GetComponentInChildren<CapsuleCollider>();
                //col.gameObject.EnsureComponent<HangingStingerCollision>();
                //__instance.gameObject.EnsureComponent<HangingStingerCollision>();
                //collider.isTrigger = true;
                //collider.bounds = col.bounds;
                //ErrorMessage.AddDebug("invincibleInCreative " + __instance.invincibleInCreative);
            }
        }
    }

    //[HarmonyPatch(typeof(HangingStinger), "OnCollisionEnter")]
    class HangingStinger_OnCollisionEnter_Patch
    {
        public static void Postfix(HangingStinger __instance, Collision other)
        {
            //if (__instance.GetComponent<HangingStinger>())
            //{
            ErrorMessage.AddDebug("OnCollisionEnter " + other.gameObject.name);
            CapsuleCollider col = __instance.GetComponentInChildren<CapsuleCollider>();
            col.isTrigger = true;
            //__instance.data.explodeOnDestroy = false;
            //ErrorMessage.AddDebug("invincibleInCreative " + __instance.invincibleInCreative);
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
                //ErrorMessage.AddDebug("IsAlive" + __instance.IsAlive());
                //ErrorMessage.AddDebug("destroyOnDeath" + __instance.destroyOnDeath);
                //ErrorMessage.AddDebug("explodeOnDestroy " + __instance.explodeOnDestroy);
                //ErrorMessage.AddDebug("invincible " + __instance.invincible);
                //ErrorMessage.AddDebug("health " + __instance.health);
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
                ErrorMessage.AddDebug("Kill");
                //ErrorMessage.AddDebug("Damage " + originalDamage);
                //ErrorMessage.AddDebug("health " + __instance.health);
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
                ErrorMessage.AddDebug("ExplodeGameObject " + go.name);
            //ErrorMessage.AddDebug("Damage " + originalDamage);
            //ErrorMessage.AddDebug("health " + __instance.health);
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
            ErrorMessage.AddDebug("CleanUp " );
            //ErrorMessage.AddDebug("Damage " + originalDamage);
            //ErrorMessage.AddDebug("health " + __instance.health);
            //}   UnityEngine.Object.Destroy(o, time);
        }
    }
}
