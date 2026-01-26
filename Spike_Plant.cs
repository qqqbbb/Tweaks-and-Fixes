using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UWE;

namespace Tweaks_Fixes
{
    internal class Spike_Plant
    {
        [HarmonyPatch(typeof(GrownPlant), "Awake")]
        class GrownPlant_Awake_Patch
        { // SpikePlant.OnGrown does not run when loading saved game
            public static void Postfix(GrownPlant __instance)
            {
                //Main.logger.LogMessage("GrownPlant Awake " + __instance.name);
                CoroutineHost.StartCoroutine(MakeSpikePlantDocile(__instance));
            }
        }

        public static IEnumerator MakeSpikePlantDocile(GrownPlant grownPlant)
        {
            if (grownPlant.TryGetComponent(out RangeTargeter rt))
                rt.enabled = false;

            while (grownPlant.transform.parent == null || grownPlant.transform.parent.parent == null || grownPlant.seed == null)
                yield return null;

            if (grownPlant.seed.plantTechType != TechType.SpikePlant)
                yield break;
            else if (rt)
                rt.enabled = true;

            if (grownPlant.transform.parent.parent.TryGetComponent<Planter>(out _))
            {
                //AddDebug("MakeSpikePlantDocile Planter");
                if (grownPlant.TryGetComponent(out RangeAttacker ra))
                    UnityEngine.Object.Destroy(ra);

                if (rt)
                    UnityEngine.Object.Destroy(rt);
            }
            else if (rt)
                rt.enabled = true;
        }

        [HarmonyPatch(typeof(Projectile), "OnCollisionEnter")]
        class Projectile_OnCollisionEnter_Patch
        {
            public static bool Prefix(Projectile __instance, Collision collision)
            {
                if (__instance.name == "SpikePlantProjectile(Clone)")
                {
                    if (collision.gameObject.GetComponentInParent<SubRoot>() || collision.gameObject.GetComponentInParent<Vehicle>())
                    {
                        //AddDebug("OnCollisionEnter SubRoot");
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
