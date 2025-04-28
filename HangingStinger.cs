using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UWE;
using static ErrorMessage;

namespace Tweaks_Fixes
{

    //[HarmonyPatch(typeof(HangingStinger), "OnCollisionEnter")]
    static class HangingStinger_OnCollisionEnter_Patch
    {
        static bool Prefix(HangingStinger __instance, Collision other)
        {
            AddDebug(__instance.name + " HangingStinger OnCollisionEnter " + other.gameObject.name);
            return true;
        }
    }

    //[HarmonyPatch(typeof(HangingStinger), "OnCollisionEnter")]
    static class HangingStinger_OnCollisionEnter_Postfix_Patch
    {
        static void Postfix(HangingStinger __instance, Collision other)
        {
            AddDebug(__instance.name + " HangingStinger OnCollisionEnter Postfix " + other.gameObject.name);
        }
    }

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
            }
            HangingStinger_My hangingStinger_My = __instance.gameObject.EnsureComponent<HangingStinger_My>();
            hangingStinger_My.hangingStinger = __instance;
            CapsuleCollider collider = __instance.GetComponentInChildren<CapsuleCollider>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.gameObject.layer = LayerID.Useable;
            }
            foreach (SkinnedMeshRenderer mr in __instance.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (Material m in mr.materials)
                    m.EnableKeyword("FX_KELP"); // mesh will not render inside vehicles
            }
        }
    }

    class HangingStinger_My : MonoBehaviour
    {
        float venomRechargeTime = 5f;
        HashSet<Collider> vehiclColliders = new HashSet<Collider>();
        public HangingStinger hangingStinger;

        public void Poison(GameObject go)
        {
            if (hangingStinger._venomAmount < 1)
                return;

            LiveMixin liveMixin = go.GetComponentInChildren<LiveMixin>();
            if (liveMixin == null)
                return;

            //AddDebug($"Poison {go.name} damage {HangingStinger.damage}");
            if (go == Player.mainObject && (ConfigToEdit.permPoisonDamage.Value > 0 || ConfigToEdit.poisonFoodDamage.Value > 0))
            {
                if (ConfigToEdit.permPoisonDamage.Value > 0)
                    CoroutineHost.StartCoroutine(Poison_Damage.DealPoisonDamage(liveMixin, HangingStinger.damage));
                if (ConfigToEdit.poisonFoodDamage.Value > 0)
                    CoroutineHost.StartCoroutine(Poison_Damage.DealFoodDamage(HangingStinger.damage, Main.survival, liveMixin));
            }
            else
            {
                DamageOverTime damageOverTime = go.AddComponent<DamageOverTime>();
                damageOverTime.doer = this.gameObject;
                damageOverTime.totalDamage = HangingStinger.damage;
                damageOverTime.duration = HangingStinger.damageDuration * (int)hangingStinger.size;
                damageOverTime.damageType = DamageType.Poison;
                damageOverTime.ActivateInterval(0.5f);
            }
            hangingStinger._venomAmount = 0f;
            hangingStinger.venomRechargeTime = Random.value * venomRechargeTime + venomRechargeTime;
        }

        public void DisableShinyShader(GameObject go)
        {
            foreach (SkinnedMeshRenderer mr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (Material m in mr.materials)
                    m.DisableKeyword("MARMO_EMISSION");
            }
        }

        public void EnableShinyShader(GameObject go)
        {
            foreach (SkinnedMeshRenderer mr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (Material m in mr.materials)
                    m.EnableKeyword("MARMO_EMISSION");
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            Poison(other.gameObject);
            TechType tt = CraftData.GetTechType(other.gameObject);
            if (tt == TechType.None)
                return;

            GameObject rootGO = Util.GetEntityRoot(other.gameObject);
            //AddDebug("HangingStinger OnTriggerEnter " + tt);
            if (Vehicle_patch.vehicleTechTypes.Contains(tt))
            {
                vehiclColliders.Add(other);
                DisableShinyShader(this.gameObject);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            //TechType tt = CraftData.GetTechType(other.gameObject);
            //if (tt == TechType.None)
            //    return;

            //AddDebug("HangingStinger OnTriggerExit " + tt);
            vehiclColliders.Remove(other);
            if (vehiclColliders.Count == 0)
            {
                //AddDebug("OnTriggerExit vehiclColliders.Count == 0");
                EnableShinyShader(this.gameObject);
            }
        }


    }

}
