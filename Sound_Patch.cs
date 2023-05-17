using HarmonyLib;
using System;
using System.Collections.Generic;
using static ErrorMessage;

namespace Tweaks_Fixes
{
    internal class Sound_Patch
    {

        [HarmonyPatch(typeof(FMOD_CustomEmitter), "Play")]
        class FMOD_CustomEmitter_Play_Patch
        {
            public static bool Prefix(FMOD_CustomEmitter __instance)
            {
                //AddDebug(" FMOD_CustomEmitter Play " + __instance.asset.name);
                if (!Main.loadingDone)
                    return false;

                return true;
            }
        }


        //[HarmonyPatch(typeof(SoundQueue), "Play")]
        class SoundQueue_Play_Patch
        {
            public static void Prefix(SoundQueue __instance, string sound)
            {
                AddDebug(" SoundQueue Play " + sound);
                //if (!Main.loadingDone)
                //    return false;

            }
        }


    }
}
