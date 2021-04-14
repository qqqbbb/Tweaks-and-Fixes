
using HarmonyLib;
using UnityEngine;

namespace Tweaks_Fixes
{

    [HarmonyPatch(typeof(SeaTreaderSounds), nameof(SeaTreaderSounds.OnStep))]
    class SeaTreaderSounds_OnStep_patch
    { // seatreader spawns chunks only when stomping
        public static bool Prefix(SeaTreaderSounds __instance, Transform legTr, AnimationEvent animationEvent)
        {
            if (!Main.config.seaTreaderChunks)
                return true;

            if (animationEvent.animatorClipInfo.clip == __instance.walkinAnimClip && !__instance.treader.IsWalking())
                return false;
            if (__instance.stepEffect != null)
                Utils.SpawnPrefabAt(__instance.stepEffect, null, legTr.position);
            if (__instance.stepSound != null)
                Utils.PlayEnvSound(__instance.stepSound, legTr.position);
            return false;
        }
    }

}
