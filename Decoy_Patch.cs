using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    public class Decoy_Patch
    {
        public static List<GameObject> decoysToDestroy = new List<GameObject>();
        public static ConditionalWeakTable<GameObject, string> pickupShinies = new ConditionalWeakTable<GameObject, string>();
        //static Dictionary<CyclopsDecoy, decoyData> decoys = new Dictionary<CyclopsDecoy, decoyData>();

        public static void DestroyDecoys()
        {
            for (int i = decoysToDestroy.Count - 1; i >= 0; i--)
                Util.DestroyEntity(decoysToDestroy[i]);
        }

        public class DestroyOnDisable : MonoBehaviour
        {
            private void OnDisable()
            {
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
            [HarmonyPrefix]
            [HarmonyPatch("Start")]
            static bool StartPrefix(CyclopsDecoy __instance)
            {
                //AddDebug("CyclopsDecoy Start launch " + __instance.launch);
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

                if (ConfigMenu.decoyHP.Value > 0)
                { // HP not saved 
                    lm = __instance.gameObject.AddComponent<LiveMixin>();
                    lm.data = ScriptableObject.CreateInstance<LiveMixinData>();
                    lm.data.maxHealth = ConfigMenu.decoyHP.Value;
                    lm.data.destroyOnDeath = true;
                    //lm.data.explodeOnDestroy = false;
                    lm.data.knifeable = false;
                }
                if (!__instance.launch && ConfigToEdit.decoyRequiresSub.Value)
                {
                    //AddDebug("CyclopsDecoy Start decoyRequiresSub");
                    __instance.Invoke("Despawn", 0f);
                }
                else
                {
                    Pickupable p = __instance.GetComponent<Pickupable>();
                    p.isPickupable = false;
                    __instance.Invoke("Despawn", ConfigMenu.decoyLifeTime.Value);
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

            //[HarmonyPrefix]
            //[HarmonyPatch("Update")]
            static bool UpdatePrefix(CyclopsDecoy __instance)
            {
                if (!__instance.launch)
                    return false;

                if (Player.main.currentSub)
                    __instance.transform.Translate(new Vector3(0f, __instance.launchSpeed, 0f), Space.World);
                else
                    __instance.transform.position += __instance.transform.up * __instance.launchSpeed;

                __instance.launchSpeed = Mathf.MoveTowards(__instance.launchSpeed, 0f, Time.deltaTime);
                if (!Mathf.Approximately(__instance.launchSpeed, 0f))
                    return false;

                __instance.launch = false;
                return false;
            }

            //[HarmonyPostfix]
            //[HarmonyPatch("Update")]
            static void UpdatePostfix(CyclopsDecoy __instance)
            {
                //if (__instance.launch)
                //AddDebug("CyclopsDecoy " + __instance.launch);

                //return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("Despawn")]
            static bool DespawnPrefix(CyclopsDecoy __instance)
            {
                //AddDebug("Despawn Decoy");
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

        [HarmonyPatch(typeof(CyclopsDecoyManager), "Start")]
        class CyclopsDecoyManager_Start_Patch
        {
            static void Prefix(CyclopsDecoyManager __instance)
            {
                //AddDebug("CyclopsDecoyManager Start");
                Vehicle_patch.decoyPrefab = __instance.decoyLauncher.decoyPrefab;
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
