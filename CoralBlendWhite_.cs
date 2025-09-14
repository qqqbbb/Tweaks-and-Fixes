using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class CoralBlendWhite_
    {
        public static void OnCoralKill(CoralBlendWhite coralBlendWhite)
        {
            coralBlendWhite.killed = true;
            coralBlendWhite.timeOfDeath = Time.time;
            coralBlendWhite.RegisterForDeathUpdate();
            Animator animator = coralBlendWhite.GetComponentInChildren<Animator>();
            if (animator)
                //animator.enabled = false;
                UnityEngine.Object.Destroy(animator);

            AnimatorLink animatorLink = coralBlendWhite.GetComponentInChildren<AnimatorLink>();
            if (animatorLink)
                UnityEngine.Object.Destroy(animatorLink);

            IntermittentInstantiate ii = coralBlendWhite.GetComponent<IntermittentInstantiate>();
            if (ii)
                UnityEngine.Object.Destroy(ii);
        }

        [HarmonyPatch(typeof(CoralBlendWhite))]
        class CoralBlendWhite_OnEnable_Patch
        {
            [HarmonyPostfix, HarmonyPatch("OnEnable")]
            public static void OnEnablePostfix(CoralBlendWhite __instance)
            {
                //AddDebug("CoralBlendWhite OnEnable killed " + __instance.killed);
                //AddDebug("CoralBlendWhite OnEnable done " + __instance.done);
                LiveMixin liveMixin = __instance.GetComponent<LiveMixin>();
                if (liveMixin)
                {
                    if (liveMixin.data)
                        liveMixin.data.destroyOnDeath = false; // dont destroy brain coral
                    //AddDebug("IsAlive " + liveMixin.IsAlive());
                    if (!liveMixin.IsAlive())
                        OnCoralKill(__instance);
                }
                BrainCoral brainCoral = __instance.GetComponent<BrainCoral>();
                if (brainCoral)
                {

                }
            }
            [HarmonyPrefix, HarmonyPatch("OnKill")]
            public static void OnKillPrefix(CoralBlendWhite __instance)
            {
                //AddDebug("CoralBlendWhite OnKill Prefix " + __instance.killed);
                __instance.timesDied = 3;// prevent reviving
            }
            [HarmonyPostfix, HarmonyPatch("OnKill")]
            public static void OnKillPostfix(CoralBlendWhite __instance)
            {
                //AddDebug("CoralBlendWhite OnKill Postfix " + __instance.killed);
                //AddDebug("CoralBlendWhite OnEnable done " + __instance.done);
                BrainCoral brainCoral = __instance.GetComponentInChildren<BrainCoral>();
                if (brainCoral)
                    OnCoralKill(__instance);
            }
        }

    }
}
