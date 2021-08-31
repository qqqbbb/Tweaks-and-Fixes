using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Decoy_Patch
    {
        public static List<GameObject> decoysToDestroy = new List<GameObject>();
        //static Dictionary<CyclopsDecoy, decoyData> decoys = new Dictionary<CyclopsDecoy, decoyData>();

        public class DestroyOnDisable : MonoBehaviour
        {
            private void OnDisable()
            {
                //AddDebug("OnDisable ");
                //if (!gameObject.GetComponentInParent<Player>())
                //{
                    //UniqueIdentifier ui = gameObject.GetComponent<UniqueIdentifier>();
                decoysToDestroy.Remove(gameObject);
                Destroy(gameObject);
                //}
            }
        }

        [HarmonyPatch(typeof(CyclopsDecoy))]
        class CyclopsDecoy_Patch
        {
            [HarmonyPatch(nameof(CyclopsDecoy.Start))]
            [HarmonyPrefix]
            static bool StartPrefix(CyclopsDecoy __instance)
            {
                //AddDebug("CyclopsDecoy launch " + __instance.launch);
                Stabilizer s = __instance.GetComponent<Stabilizer>();
                if (s)
                    UnityEngine.Object.Destroy(s);

                GenericHandTarget ght = __instance.gameObject.GetComponentInChildren<GenericHandTarget>();
                if (ght) // GenericHandTarget blocks Pickupable.OnHandHover 
                    UnityEngine.Object.Destroy(ght);

                WorldForces wf = __instance.GetComponent<WorldForces>();
                wf.underwaterGravity = 0f;
                //UniqueIdentifier ui = __instance.gameObject.GetComponent<UniqueIdentifier>();
                LiveMixin lm = null;

                if (Main.config.decoyHP > 0)
                { // HP not saved 
                    lm = __instance.gameObject.AddComponent<LiveMixin>();
                    lm.data = ScriptableObject.CreateInstance<LiveMixinData>();
                    lm.data.maxHealth = Main.config.decoyHP;
                    lm.data.destroyOnDeath = true;
                    lm.data.explodeOnDestroy = false;
                    lm.data.knifeable = false;
                }
                if (!__instance.launch && Main.config.decoyRequiresSub)
                    __instance.Invoke("Despawn", 0f);
                else
                {
                    __instance.Invoke("Despawn", Main.config.decoyLifeTime);
                    CyclopsDecoyManager.AddDecoyToGlobalHashSet(__instance.gameObject);
                    //p.isPickupable = false;
                }

                //else if (Main.config.decoyRequiresSub)
                //{
                //    p.isPickupable = true;
                //    __instance.Invoke("Despawn", 0);
                //decoys[__instance] = new decoyData(Main.config.subDecoyHP, DayNightCycle.main.timePassedAsFloat);
                //}
                //if (Main.config.expiredDecoys.Contains(ui.id))
                //    __instance.Invoke("Despawn", 0f);
                //__instance.Invoke("Despawn", Main.config.subDecoyLifeTime);
                return false;
            }
            [HarmonyPatch(nameof(CyclopsDecoy.Despawn))]
            [HarmonyPrefix]
            static bool DespawnPrefix(CyclopsDecoy __instance)
            {
                //AddDebug("Despawn Decoy");
                //Pickupable p = __instance.GetComponent<Pickupable>();
                //p.isPickupable = false;
                Pickupable p = __instance.GetComponent<Pickupable>();
                p.isPickupable = false;
                __instance.gameObject.AddComponent<DestroyOnDisable>();
                CyclopsDecoyManager.RemoveDecoyFromGlobalHashSet(__instance.gameObject);
                FMOD_CustomLoopingEmitter cle = __instance.GetComponent<FMOD_CustomLoopingEmitter>();
                if (cle)
                    UnityEngine.Object.Destroy(cle);
                ParticleSystem[] pss = __instance.GetComponentsInChildren<ParticleSystem>();
                //foreach (ParticleSystem ps in pss)
                for (int i = pss.Length - 1; i >= 0; i--)
                {
                    pss[i].Stop();
                    UnityEngine.Object.Destroy(pss[i], 5f);
                }
                WorldForces wf = __instance.GetComponent<WorldForces>();
                wf.underwaterGravity = 1f;
                EcoTarget[] ets = __instance.GetComponents<EcoTarget>();
                for (int i = ets.Length - 1; i >= 0; i--)
                {
                    if (ets[i].type == EcoTargetType.SubDecoy)
                        UnityEngine.Object.Destroy(ets[i]);
                }
                //UniqueIdentifier ui = __instance.gameObject.GetComponent<UniqueIdentifier>();
                //if (ui)
                //    Main.config.expiredDecoys.Add(ui.id);
                decoysToDestroy.Add(__instance.gameObject);
                return false;
            }
        }

        //[HarmonyPatch(typeof(Inventory), "CanDropItemHere")]
        class Inventory_CanDropItemHere_Patch
        {
            static void Postfix(Inventory __instance, Pickupable item, ref bool __result)
            {
                if (Main.config.decoyRequiresSub && item.GetComponent<CyclopsDecoy>())
                   __result = false;
            }
        }

        //[HarmonyPatch(typeof(Pickupable), "OnHandHover")]
        class Pickupable_OnHandHover_Patch
        {
            static void Prefix(Pickupable __instance, GUIHand hand)
            {
                //UnityEngine.Object.Destroy(__instance.gameObject);
                //return false;
                TechType tt = CraftData.GetTechType(__instance.gameObject);
                //AddDebug("OnHandHover " + tt);
                CyclopsDecoy cd = __instance.gameObject.GetComponent<CyclopsDecoy>();
                if (cd)
                {
                    //AddDebug("IsInvoking " + cd.IsInvoking("Despawn"));
                }
                //UniqueIdentifier lm = __instance.gameObject.GetComponent<UniqueIdentifier>();
                //if (lm && Main.config.expiredDecoys.Contains(lm.id))
                //    AddDebug(tt + " expired ");
                //else
                //    AddDebug(tt + " no  UniqueIdentifier");
            }
        }

        //[HarmonyPatch(typeof(AggressiveWhenSeeTarget), "IsTargetValid", new Type[] { typeof(GameObject) })]
        class AggressiveWhenSeeTarget_IsTargetValid_Patch
        {
            static void Postfix(AggressiveWhenSeeTarget __instance, GameObject target, bool __result)
            {
                TechType tt = CraftData.GetTechType(target);
                if (tt == TechType.CyclopsDecoy)
                {
                    TechType mytt = CraftData.GetTechType(__instance.gameObject);
                    //AddDebug(mytt + " IsTargetValid " + __result);
                    //EcoTargetType q = EcoTargetType.SubDecoy
                }
            }
        }


    }
}
